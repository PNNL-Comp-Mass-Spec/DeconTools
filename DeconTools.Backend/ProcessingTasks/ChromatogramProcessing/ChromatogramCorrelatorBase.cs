﻿using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IqLogger;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ChromatogramProcessing
{
    public abstract class ChromatogramCorrelatorBase : Task
    {
        protected SavitzkyGolaySmoother Smoother;
        protected PeakChromatogramGenerator PeakChromGen;

        #region Constructors

        protected ChromatogramCorrelatorBase(int numPointsInSmoother, double minRelativeIntensityForChromCorr,
            double chromTolerance, Globals.ToleranceUnit toleranceUnit = Globals.ToleranceUnit.PPM)
        {
            SavitzkyGolaySmoothingOrder = 2;
            NumPointsInSmoother = numPointsInSmoother;

            ChromTolerance = chromTolerance;
            MinimumRelativeIntensityForChromCorr = minRelativeIntensityForChromCorr;

            ChromToleranceUnit = toleranceUnit;

            PeakChromGen = new PeakChromatogramGenerator(ChromTolerance, Globals.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK,
                                                         Globals.IsotopicProfileType.UNLABELED, toleranceUnit);

            Smoother = new SavitzkyGolaySmoother(NumPointsInSmoother, SavitzkyGolaySmoothingOrder);
        }

        #endregion

        #region Properties

        private double _chromTolerance;
        public double ChromTolerance
        {
            get => _chromTolerance;
            set
            {
                _chromTolerance = value;

                if (PeakChromGen != null)
                {
                    PeakChromGen = new PeakChromatogramGenerator(ChromTolerance, Globals.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK,
                                                                 Globals.IsotopicProfileType.UNLABELED, ChromToleranceUnit);
                }
            }
        }

        /// <summary>
        /// Tolerance unit for chromatogram. Either PPM (default) or MZ. Can only be set in the class constructor
        /// </summary>
        public Globals.ToleranceUnit ChromToleranceUnit { get; }

        public double MinimumRelativeIntensityForChromCorr { get; set; }

        private int _numPointsInSmoother;
        public int NumPointsInSmoother
        {
            get => _numPointsInSmoother;
            set
            {
                if (_numPointsInSmoother != value)
                {
                    _numPointsInSmoother = value;

                    if (Smoother != null)
                    {
                        Smoother = new SavitzkyGolaySmoother(_numPointsInSmoother, SavitzkyGolaySmoothingOrder);
                    }
                }
            }
        }

        private int _savitzkyGolaySmoothingOrder;
        public int SavitzkyGolaySmoothingOrder
        {
            get => _savitzkyGolaySmoothingOrder;
            set
            {
                if (_savitzkyGolaySmoothingOrder != value)
                {
                    _savitzkyGolaySmoothingOrder = value;

                    if (Smoother != null)
                    {
                        Smoother = new SavitzkyGolaySmoother(NumPointsInSmoother, _savitzkyGolaySmoothingOrder);
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        public void GetElutionCorrelationData(XYData chromData1, XYData chromData2, out double slope, out double intercept, out double rSquaredVal)
        {
            slope = -9999;
            intercept = -9999;
            rSquaredVal = -1;

            Check.Require(chromData1?.Xvalues != null, "Chromatogram1 intensities are null");
            if (chromData1?.Xvalues == null)
            {
                return;
            }

            Check.Require(chromData2?.Xvalues != null, "Chromatogram2 intensities are null");
            if (chromData2?.Xvalues == null)
            {
                return;
            }

            Check.Require(Math.Abs(chromData1.Xvalues[0] - chromData2.Xvalues[0]) < float.Epsilon, "Correlation failed. Chromatograms being correlated do not have the same scan values!");

            GetElutionCorrelationData(chromData1.Yvalues, chromData2.Yvalues, out slope, out intercept, out rSquaredVal);
        }

        public void GetElutionCorrelationData(double[] chromIntensities1, double[] chromIntensities2, out double slope, out double intercept, out double rSquaredVal)
        {
            slope = -9999;
            intercept = -9999;
            rSquaredVal = -1;

            Check.Require(chromIntensities1 != null, "Chromatogram1 intensities are null");
            if (chromIntensities1 == null)
            {
                return;
            }

            Check.Require(chromIntensities2 != null, "Chromatogram2 intensities are null");
            if (chromIntensities2 == null)
            {
                return;
            }

            Check.Require(chromIntensities1.Length == chromIntensities2.Length, "Correlation failed. Chromatogram1 and Chromatogram2 must be the same length");

            try
            {
                MathUtils.GetLinearRegression(chromIntensities1, chromIntensities2, out slope, out intercept, out rSquaredVal);
            }
            catch (Exception ex)
            {
                IqLogger.LogError("!! FATAL ERROR in Chrom correlator !! " + ex.Message, ex);
            }
        }

        #endregion

        #region Private Methods

        #endregion

        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList.Run.CurrentMassTag != null, Name + " failed; CurrentMassTag is empty");
            if (resultList.Run.CurrentMassTag == null)
            {
                return;
            }

            Check.Require(resultList.Run.CurrentMassTag.IsotopicProfile != null, Name + " failed; Theor isotopic profile is empty. Run a TheorFeatureGenerator");
            Check.Require(resultList.CurrentTargetedResult != null, Name + " failed; CurrentTargetedResult is empty.");
            if (resultList.CurrentTargetedResult == null)
            {
                return;
            }

            Check.Require(resultList.CurrentTargetedResult.ChromPeakSelected != null, Name + " failed; ChromPeak was never selected.");
            if (resultList.CurrentTargetedResult.ChromPeakSelected == null)
            {
                return;
            }

            Check.Require(resultList.CurrentTargetedResult.IsotopicProfile != null, Name + " failed; Isotopic profile is null.");

            var scan = resultList.CurrentTargetedResult.ScanSet.PrimaryScanNumber;

            var chromScanWindowWidth = resultList.CurrentTargetedResult.ChromPeakSelected.Width * 2;

            var startScan = scan - (int)Math.Round(chromScanWindowWidth / 2, 0);
            var stopScan = scan + (int)Math.Round(chromScanWindowWidth / 2, 0);

            resultList.CurrentTargetedResult.ChromCorrelationData = CorrelateData(resultList.Run,
                resultList.CurrentTargetedResult, startScan, stopScan);
        }

        public abstract ChromCorrelationData CorrelateData(Run run, TargetedResultBase result, int startScan, int stopScan);

        public ChromCorrelationData CorrelatePeaksWithinIsotopicProfile(Run run, IsotopicProfile iso, int startScan, int stopScan)
        {
            var correlationData = new ChromCorrelationData();
            var indexMostAbundantPeak = iso.GetIndexOfMostIntensePeak();

            var baseMZValue = iso.Peaklist[indexMostAbundantPeak].XValue;
            var basePeakChromXYData = GetBaseChromXYData(run, startScan, stopScan, baseMZValue);

            var baseChromDataIsOK = basePeakChromXYData?.Xvalues != null;
            //&&basePeakChromXYData.Xvalues.Length > 3;

            var minIntensity = iso.Peaklist[indexMostAbundantPeak].Height *
                                 MinimumRelativeIntensityForChromCorr;

            for (var i = 0; i < iso.Peaklist.Count; i++)
            {
                if (!baseChromDataIsOK)
                {
                    var defaultChromCorrDataItem = new ChromCorrelationDataItem();
                    correlationData.AddCorrelationData(defaultChromCorrDataItem);
                    break;
                }

                if (i == indexMostAbundantPeak)
                {
                    //peak is being correlated to itself
                    correlationData.AddCorrelationData(1.0, 0, 1);
                }
                else if (iso.Peaklist[i].Height >= minIntensity)
                {
                    var correlatedMZValue = iso.Peaklist[i].XValue;
                    var chromPeakXYData = GetCorrelatedChromPeakXYData(run, startScan, stopScan, basePeakChromXYData, correlatedMZValue);

                    var chromDataIsOK = chromPeakXYData?.Xvalues != null;
                    //&&chromPeakXYData.Xvalues.Length > 3;

                    if (chromDataIsOK)
                    {
                        chromPeakXYData = FillInAnyMissingValuesInChromatogram(basePeakChromXYData, chromPeakXYData);

                        GetElutionCorrelationData(basePeakChromXYData, chromPeakXYData,
                                                                          out var slope, out var intercept, out var rSquaredVal);

                        correlationData.AddCorrelationData(slope, intercept, rSquaredVal);
                    }
                    else
                    {
                        var defaultChromCorrDataItem = new ChromCorrelationDataItem();
                        correlationData.AddCorrelationData(defaultChromCorrDataItem);
                    }
                }
                else
                {
                    var defaultChromCorrDataItem = new ChromCorrelationDataItem();
                    correlationData.AddCorrelationData(defaultChromCorrDataItem);
                }
            }

            return correlationData;
        }

        protected XYData GetCorrelatedChromPeakXYData(Run run, int startScan, int stopScan, XYData basePeakChromXYData, double correlatedMZValue)
        {
            var xyData = PeakChromGen.GenerateChromatogram(run, startScan, stopScan, correlatedMZValue, ChromTolerance, ChromToleranceUnit);

            XYData chromPeakXYData;
            if (xyData == null || xyData.Xvalues.Length == 0)
            {
                chromPeakXYData = new XYData
                {
                    Xvalues = basePeakChromXYData.Xvalues,
                    Yvalues = new double[basePeakChromXYData.Xvalues.Length]
                };
            }
            else
            {
                chromPeakXYData = Smoother.Smooth(xyData);
            }

            var chromDataIsOK = chromPeakXYData?.Xvalues != null && chromPeakXYData.Xvalues.Length > 3;

            if (chromDataIsOK)
            {
                chromPeakXYData = chromPeakXYData.TrimData(startScan, stopScan);
            }
            return chromPeakXYData;
        }

        protected XYData GetBaseChromXYData(Run run, int startScan, int stopScan, double baseMZValue)
        {
            var xyData = PeakChromGen.GenerateChromatogram(run, startScan, stopScan, baseMZValue, ChromTolerance, ChromToleranceUnit);

            if (xyData == null || xyData.Xvalues.Length < 3)
            {
                return null;
            }

            var basePeakChromXYData = Smoother.Smooth(xyData);

            var baseChromDataIsOK = basePeakChromXYData?.Xvalues != null && basePeakChromXYData.Xvalues.Length > 3;

            if (baseChromDataIsOK)
            {
                basePeakChromXYData = basePeakChromXYData.TrimData(startScan, stopScan);
            }
            return basePeakChromXYData;
        }

        /// <summary>
        /// Fills in any missing data in the chrom data being correlated.
        /// This ensures base chrom data and the correlated chrom data are the same length
        /// </summary>
        /// <param name="basePeakChromXYData"></param>
        /// <param name="chromPeakXYData"></param>
        /// <returns></returns>
        protected XYData FillInAnyMissingValuesInChromatogram(XYData basePeakChromXYData, XYData chromPeakXYData)
        {
            if (basePeakChromXYData?.Xvalues == null || basePeakChromXYData.Xvalues.Length == 0)
            {
                return null;
            }

            var filledInData = new SortedDictionary<int, double>();

            //first fill with zeros
            foreach (var dataPoint in basePeakChromXYData.Xvalues)
            {
                filledInData.Add((int)dataPoint, 0);
            }

            //then fill in other values
            for (var i = 0; i < chromPeakXYData.Xvalues.Length; i++)
            {
                var currentScan = (int)chromPeakXYData.Xvalues[i];
                if (filledInData.ContainsKey(currentScan))
                {
                    filledInData[currentScan] = chromPeakXYData.Yvalues[i];
                }
            }

            var xyData = new XYData { Xvalues = basePeakChromXYData.Xvalues, Yvalues = filledInData.Values.ToArray() };

            return xyData;
        }
    }
}
