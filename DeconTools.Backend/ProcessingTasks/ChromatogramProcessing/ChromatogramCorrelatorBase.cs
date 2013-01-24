using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ChromatogramProcessing
{
    public abstract class ChromatogramCorrelatorBase : Task
    {
        
        protected SavitzkyGolaySmoother Smoother;
        protected PeakChromatogramGenerator PeakChromGen;

        #region Constructors

        protected ChromatogramCorrelatorBase(int numPointsInSmoother, int chromToleranceInPPM, double minRelativeIntensityForChromCorr)
        {
            SavitzkyGolaySmoothingOrder = 2;
            NumPointsInSmoother = numPointsInSmoother ;

            ChromToleranceInPPM = chromToleranceInPPM;
            MinimumRelativeIntensityForChromCorr = minRelativeIntensityForChromCorr;
        }


        #endregion

        #region Properties

        private double _chromToleranceInPPM;
        public double ChromToleranceInPPM
        {
            get { return _chromToleranceInPPM; }
            set
            {
                _chromToleranceInPPM = value;
                PeakChromGen = new PeakChromatogramGenerator(ChromToleranceInPPM);
            }
        }

        public double MinimumRelativeIntensityForChromCorr { get; set; }

        private int _numPointsInSmoother;
        public int NumPointsInSmoother
        {
            get { return _numPointsInSmoother; }
            set
            {
                if (_numPointsInSmoother!=value)
                {

                    _numPointsInSmoother = value;
                    Smoother = new SavitzkyGolaySmoother(NumPointsInSmoother, SavitzkyGolaySmoothingOrder);
                }
                
            }
        }

        private int _savitzkyGolaySmoothingOrder;
        public int SavitzkyGolaySmoothingOrder
        {
            get { return _savitzkyGolaySmoothingOrder; }
            set
            {
                if (_savitzkyGolaySmoothingOrder!=value)
                {
                    _savitzkyGolaySmoothingOrder = value;
                    Smoother = new SavitzkyGolaySmoother(NumPointsInSmoother, SavitzkyGolaySmoothingOrder);
                }
                
            }
        }

        #endregion

        #region Public Methods

        public void GetElutionCorrelationData(XYData chromData1, XYData chromData2, out double slope, out double intercept, out double rsquaredVal)
        {
            Check.Require(chromData1 != null && chromData1.Xvalues != null, "Chromatogram1 intensities are null");
            Check.Require(chromData2 != null && chromData2.Xvalues != null, "Chromatogram2 intensities are null");

            Check.Require(chromData1.Xvalues[0] == chromData2.Xvalues[0], "Correlation failed. Chromatograms being correlated do not have the same scan values!");

            GetElutionCorrelationData(chromData1.Yvalues, chromData2.Yvalues, out slope, out intercept, out rsquaredVal);
        }

        public void GetElutionCorrelationData(double[] chromIntensities1, double[] chromIntensities2, out double slope, out double intercept, out double rsquaredVal)
        {
            Check.Require(chromIntensities1 != null, "Chromatogram1 intensities are null");
            Check.Require(chromIntensities2 != null, "Chromatogram2 intensities are null");

            Check.Require(chromIntensities1.Length == chromIntensities2.Length, "Correlation failed. Chromatogram1 and Chromatogram2 must be the same length");

            slope = -9999;
            intercept = -9999;
            rsquaredVal = -1;

            MathUtils.GetLinearRegression(chromIntensities1, chromIntensities2, out slope, out intercept, out rsquaredVal);
        }


        #endregion

        #region Private Methods

        #endregion

        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList.Run.CurrentMassTag != null, this.Name + " failed; CurrentMassTag is empty");
            Check.Require(resultList.Run.CurrentMassTag.IsotopicProfile != null, this.Name + " failed; Theor isotopic profile is empty. Run a TheorFeatureGenerator");
            Check.Require(resultList.CurrentTargetedResult != null, this.Name + " failed; CurrentTargetedResult is empty.");
            Check.Require(resultList.CurrentTargetedResult.ChromPeakSelected != null, this.Name + " failed; ChromPeak was never selected.");
            Check.Require(resultList.CurrentTargetedResult.IsotopicProfile != null, this.Name + " failed; Isotopic profile is null.");


            int scan = resultList.CurrentTargetedResult.ScanSet.PrimaryScanNumber;

            var chromScanWindowWidth = resultList.CurrentTargetedResult.ChromPeakSelected.Width * 2;

            int startScan = scan - (int)Math.Round(chromScanWindowWidth / 2, 0);
            int stopScan = scan + (int)Math.Round(chromScanWindowWidth / 2, 0);


            resultList.CurrentTargetedResult.ChromCorrelationData = CorrelateData(resultList.Run,
                resultList.CurrentTargetedResult, startScan, stopScan);
        }


        public abstract ChromCorrelationData CorrelateData(Run run, TargetedResultBase result, int startScan, int stopScan);
        



        public ChromCorrelationData CorrelatePeaksWithinIsotopicProfile(Run run, IsotopicProfile iso, int startScan, int stopScan)
        {
         
            var correlationData = new ChromCorrelationData();
            int indexMostAbundantPeak = iso.GetIndexOfMostIntensePeak();

            double baseMZValue = iso.Peaklist[indexMostAbundantPeak].XValue;
            bool baseChromDataIsOK;
            var basePeakChromXYData = GetBaseChromXYData(run, startScan, stopScan, baseMZValue);

            baseChromDataIsOK = basePeakChromXYData != null && basePeakChromXYData.Xvalues != null &&
                                 basePeakChromXYData.Xvalues.Length > 3;


            double minIntensity = iso.Peaklist[indexMostAbundantPeak].Height *
                                 MinimumRelativeIntensityForChromCorr;


            for (int i = 0; i < iso.Peaklist.Count; i++)
            {
                if (!baseChromDataIsOK) break;

                if (i == indexMostAbundantPeak)
                {
                    //peak is being correlated to itself
                    correlationData.AddCorrelationData(1.0, 0, 1);


                }
                else if (iso.Peaklist[i].Height >= minIntensity)
                {
                    double correlatedMZValue = iso.Peaklist[i].XValue;
                    bool chromDataIsOK;
                    var chromPeakXYData = GetCorrelatedChromPeakXYData(run, startScan, stopScan, basePeakChromXYData, correlatedMZValue);

                    chromDataIsOK = chromPeakXYData != null && chromPeakXYData.Xvalues != null &&
                                 chromPeakXYData.Xvalues.Length > 3;

                    if (chromDataIsOK)
                    {
                        double slope;
                        double intercept;
                        double rsquaredVal;

                        chromPeakXYData = FillInAnyMissingValuesInChromatogram(basePeakChromXYData, chromPeakXYData);

                        GetElutionCorrelationData(basePeakChromXYData, chromPeakXYData,
                                                                          out slope, out intercept, out rsquaredVal);

                        correlationData.AddCorrelationData(slope, intercept, rsquaredVal);

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
            PeakChromGen.GenerateChromatogram(run, startScan, stopScan, correlatedMZValue, ChromToleranceInPPM);

            XYData chromPeakXYData;
            if (run.XYData == null || run.XYData.Xvalues.Length == 0)
            {
                chromPeakXYData = new XYData();
                chromPeakXYData.Xvalues = basePeakChromXYData.Xvalues;
                chromPeakXYData.Yvalues = new double[basePeakChromXYData.Xvalues.Length];
            }
            else
            {
                chromPeakXYData = Smoother.Smooth(run.XYData);
            }


            var chromDataIsOK = chromPeakXYData != null && chromPeakXYData.Xvalues != null &&
                                chromPeakXYData.Xvalues.Length > 3;

            if (chromDataIsOK)
            {
                chromPeakXYData = chromPeakXYData.TrimData(startScan, stopScan);
            }
            return chromPeakXYData;
        }

        protected XYData GetBaseChromXYData(Run run, int startScan, int stopScan, double baseMZValue)
        {
            PeakChromGen.GenerateChromatogram(run, startScan, stopScan, baseMZValue, ChromToleranceInPPM);

            var basePeakChromXYData = Smoother.Smooth(run.XYData);
            bool baseChromDataIsOK = basePeakChromXYData != null && basePeakChromXYData.Xvalues != null &&
                                     basePeakChromXYData.Xvalues.Length > 3;

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
            if (basePeakChromXYData == null || basePeakChromXYData.Xvalues == null || basePeakChromXYData.Xvalues.Length == 0) return null;

            var filledInData = new SortedDictionary<int, double>();

            for (int i = 0; i < chromPeakXYData.Xvalues.Length; i++)
            {
                filledInData.Add((int)chromPeakXYData.Xvalues[i], chromPeakXYData.Yvalues[i]);
            }

            for (int i = 0; i < basePeakChromXYData.Xvalues.Length; i++)
            {
                int currentScan = (int)basePeakChromXYData.Xvalues[i];
                if (!filledInData.ContainsKey(currentScan))
                {
                    filledInData.Add(currentScan, 0);
                }
            }

            var xydata = new XYData { Xvalues = basePeakChromXYData.Xvalues, Yvalues = filledInData.Values.ToArray() };

            return xydata;


        }
    }
}
