using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Workflows.Backend.Core.ChromPeakSelection
{
    public abstract class IqChromCorrelatorBase
    {
        protected SavitzkyGolaySmoother Smoother;
        protected PeakChromatogramGenerator PeakChromGen;

        #region Constructors

        protected IqChromCorrelatorBase(int numPointsInSmoother, double minRelativeIntensityForChromCorr,
            double chromTolerance, DeconTools.Backend.Globals.ToleranceUnit toleranceUnit= DeconTools.Backend.Globals.ToleranceUnit.PPM)
        {
            SavitzkyGolaySmoothingOrder = 2;
            NumPointsInSmoother = numPointsInSmoother;

            ChromTolerance = chromTolerance;
            MinimumRelativeIntensityForChromCorr = minRelativeIntensityForChromCorr;

            ChromToleranceUnit = toleranceUnit;

            PeakChromGen = new PeakChromatogramGenerator(ChromTolerance, DeconTools.Backend.Globals.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK,
                                                         DeconTools.Backend.Globals.IsotopicProfileType.UNLABELLED, toleranceUnit);


            Smoother = new SavitzkyGolaySmoother(NumPointsInSmoother, SavitzkyGolaySmoothingOrder, false);
        }

        
        #endregion

        #region Properties

        private double _chromTolerance;
        public double ChromTolerance
        {
            get { return _chromTolerance; }
            set
            {
                _chromTolerance = value;
                
                if (PeakChromGen!=null)
                {
                    PeakChromGen = new PeakChromatogramGenerator(ChromTolerance, DeconTools.Backend.Globals.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK,
                                                                DeconTools.Backend.Globals.IsotopicProfileType.UNLABELLED, ChromToleranceUnit);
                }
                
                
            }
        }

        /// <summary>
        /// Tolerence unit for chromatogram. Either PPM (default) or MZ. Can only be set in the class constructor
        /// </summary>
        public DeconTools.Backend.Globals.ToleranceUnit ChromToleranceUnit { get; private set; }



        public double MinimumRelativeIntensityForChromCorr { get; set; }

        private int _numPointsInSmoother;
        public int NumPointsInSmoother
        {
            get { return _numPointsInSmoother; }
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
            get { return _savitzkyGolaySmoothingOrder; }
            set
            {
                if (_savitzkyGolaySmoothingOrder != value)
                {
                    _savitzkyGolaySmoothingOrder = value;

                    if (Smoother!=null)
                    {
                        Smoother = new SavitzkyGolaySmoother(NumPointsInSmoother, _savitzkyGolaySmoothingOrder);    
                    }
                    
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

        public void Execute(IqResult iqResult)
        {
            int scan = iqResult.LCScanSetSelected.PrimaryScanNumber;

            double chromScanWindowWidth = iqResult.ChromPeakSelected.Width * 2;

            int startScan = scan - (int)Math.Round(chromScanWindowWidth / 2, 0);
            int stopScan = scan + (int)Math.Round(chromScanWindowWidth / 2, 0);


            iqResult.ChromCorrelationData = CorrelateData(iqResult.Target.GetRun(),
                iqResult.ObservedIsotopicProfile, startScan, stopScan);
        }


        public abstract ChromCorrelationData CorrelateData(Run run, IsotopicProfile iso, int startScan, int stopScan);


        public ChromCorrelationData CorrelatePeaksWithinIsotopicProfile(Run run, IsotopicProfile iso, int startScan, int stopScan)
        {

            var correlationData = new ChromCorrelationData();
            int indexMostAbundantPeak = iso.GetIndexOfMostIntensePeak();

            double baseMZValue = iso.Peaklist[indexMostAbundantPeak].XValue;
            bool baseChromDataIsOK;
            var basePeakChromXYData = GetBaseChromXYData(run, startScan, stopScan, baseMZValue);

            baseChromDataIsOK = basePeakChromXYData != null && basePeakChromXYData.Xvalues != null; 
                //&&basePeakChromXYData.Xvalues.Length > 3;


            double minIntensity = iso.Peaklist[indexMostAbundantPeak].Height *
                                 MinimumRelativeIntensityForChromCorr;


            for (int i = 0; i < iso.Peaklist.Count; i++)
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
                    double correlatedMZValue = iso.Peaklist[i].XValue;
                    bool chromDataIsOK;
                    var chromPeakXYData = GetCorrelatedChromPeakXYData(run, startScan, stopScan, basePeakChromXYData, correlatedMZValue);

                    chromDataIsOK = chromPeakXYData != null && chromPeakXYData.Xvalues != null;
                        //&&chromPeakXYData.Xvalues.Length > 3;

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
            var xydata= PeakChromGen.GenerateChromatogram(run, startScan, stopScan, correlatedMZValue, ChromTolerance,ChromToleranceUnit);

            XYData chromPeakXYData;
            if (xydata == null || xydata.Xvalues.Length == 0)
            {
                chromPeakXYData = new XYData();
                chromPeakXYData.Xvalues = basePeakChromXYData.Xvalues;
                chromPeakXYData.Yvalues = new double[basePeakChromXYData.Xvalues.Length];
            }
            else
            {
                chromPeakXYData = Smoother.Smooth(xydata);
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
            var xydata=   PeakChromGen.GenerateChromatogram(run, startScan, stopScan, baseMZValue, ChromTolerance,ChromToleranceUnit);

            if (xydata == null || xydata.Xvalues.Length < 3) return null;

            var basePeakChromXYData = Smoother.Smooth(xydata);

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

            //first fill with zeros
            for (int i = 0; i < basePeakChromXYData.Xvalues.Length; i++)
            {
                filledInData.Add((int)basePeakChromXYData.Xvalues[i], 0);
            }

            //then fill in other values
            for (int i = 0; i < chromPeakXYData.Xvalues.Length; i++)
            {
                int currentScan = (int)chromPeakXYData.Xvalues[i];
                if (filledInData.ContainsKey(currentScan))
                {
                    filledInData[currentScan] = chromPeakXYData.Yvalues[i];
                }
            }

            var xydata = new XYData { Xvalues = basePeakChromXYData.Xvalues, Yvalues = filledInData.Values.ToArray() };

            return xydata;


        }
    }
}
