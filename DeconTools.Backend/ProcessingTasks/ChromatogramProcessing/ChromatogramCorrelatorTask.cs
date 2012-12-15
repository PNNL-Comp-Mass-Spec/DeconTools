using System;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ChromatogramProcessing
{
    public class ChromatogramCorrelatorTask : Task
    {
        private ChromatogramCorrelator _chromatogramCorrelator = new ChromatogramCorrelator();
        private SavitzkyGolaySmoother _smoother;
        private PeakChromatogramGenerator _peakChromGen;
        
        #region Constructors

        public ChromatogramCorrelatorTask()
        {
            _chromatogramCorrelator = new ChromatogramCorrelator();

            _smoother = new SavitzkyGolaySmoother(3, 2);

            ChromToleranceInPPM = 20;

            _peakChromGen = new PeakChromatogramGenerator(ChromToleranceInPPM);

            MinimumRelativeIntensityForChromCorr = 0.01;
        }


        #endregion

        #region Properties

        public double ChromToleranceInPPM { get; set; }

        public double MinimumRelativeIntensityForChromCorr { get; set; }


        #endregion

        #region Public Methods

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


            resultList.CurrentTargetedResult.ChromCorrelationData = CorrelatePeaksWithinIsotopicProfile(resultList.Run, 
                resultList.CurrentTargetedResult.IsotopicProfile, startScan, stopScan);


        }





        public ChromCorrelationData CorrelatePeaksWithinIsotopicProfile(Run run, IsotopicProfile iso1, int startScan, int stopScan)
        {
            ChromCorrelationData correlationData = new ChromCorrelationData();
            int indexMostAbundantPeak = iso1.GetIndexOfMostIntensePeak();

            double baseMZValue = iso1.Peaklist[indexMostAbundantPeak].XValue;

           _peakChromGen.GenerateChromatogram(run, startScan, stopScan, baseMZValue, ChromToleranceInPPM);


            var basePeakChromXYData = _smoother.Smooth(run.XYData);

            bool baseChromDataIsOK = basePeakChromXYData != null && basePeakChromXYData.Xvalues != null &&
                                 basePeakChromXYData.Xvalues.Length > 3;

            if (baseChromDataIsOK)
            {
                basePeakChromXYData = basePeakChromXYData.TrimData(startScan, stopScan);
            }

            double minIntensity = iso1.Peaklist[indexMostAbundantPeak].Height *
                                 MinimumRelativeIntensityForChromCorr;


            for (int i = 0; i < iso1.Peaklist.Count; i++)
            {
                if (!baseChromDataIsOK) break;

                if (i == indexMostAbundantPeak)
                {
                    //peak is being correlated to itself
                    correlationData.AddCorrelationData(1.0, 0, 1);
                    
                    
                }
                else if (iso1.Peaklist[i].Height >= minIntensity)
                {
                    _peakChromGen.GenerateChromatogram(run, startScan, stopScan, iso1.Peaklist[i].XValue, ChromToleranceInPPM);

                    XYData chromPeakXYData;
                    if (run.XYData==null || run.XYData.Xvalues.Length==0)
                    {
                        chromPeakXYData = new XYData();
                        chromPeakXYData.Xvalues = basePeakChromXYData.Xvalues;
                        chromPeakXYData.Yvalues = new double[basePeakChromXYData.Xvalues.Length];
                    }
                    else
                    {
                        chromPeakXYData = _smoother.Smooth(run.XYData);
                    }

                    

                    var chromDataIsOK = chromPeakXYData != null && chromPeakXYData.Xvalues != null &&
                                 chromPeakXYData.Xvalues.Length > 3;

                    if (chromDataIsOK)
                    {
                        chromPeakXYData = chromPeakXYData.TrimData(startScan, stopScan);

                        double slope;
                        double intercept;
                        double rsquaredVal;
                        _chromatogramCorrelator.GetElutionCorrelationData(basePeakChromXYData, chromPeakXYData,
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


    }
}
