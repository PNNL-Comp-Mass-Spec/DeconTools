using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.PeakProcessing;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks.PeakDetectors
{
    public class ChromPeakDetectorOld:PeakDetector
    {
        PeakProcessor _oldPeakProcessor;

        List<ThrashV1Peak> _oldDeconEnginePeaklist;

        private readonly DeconToolsPeakDetectorV2 _peakDetectorV2;

        #region Constructors

        public ChromPeakDetectorOld()
        { }

        public ChromPeakDetectorOld(double peakBackgroundRatio, double sigNoise)
        {
            PeakBackgroundRatio = peakBackgroundRatio;
            SigNoise = sigNoise;

            _peakDetectorV2 = new DeconToolsPeakDetectorV2
            {
                PeakToBackgroundRatio = peakBackgroundRatio,
                SignalToNoiseThreshold = sigNoise,
                PeakFitType = Globals.PeakFitType.QUADRATIC,
                IsDataThresholded = false
            };
        }

        #endregion

        #region Properties

        public double PeakBackgroundRatio { get; set; }

        public double SigNoise { get; set; }

        #endregion

        #region Public Methods

        //TODO: remove code duplication (see DeconToolsPeakDetector)
        public override List<Peak> FindPeaks(XYData xyData, double xMin = 0, double xMax = 0)
        {
            var peakList = new List<Peak>();

            if (xyData == null)
            {
                return peakList;
            }

            if (UseNewPeakDetector)
            {
                _peakDetectorV2.FindPeaks(xyData, xMin, xMax);

            }
            else
            {
                var xVals = xyData.Xvalues.ToList();
                var yVals = xyData.Yvalues.ToList();

                if (_oldPeakProcessor==null)
                {
                    _oldPeakProcessor = new PeakProcessor();
                }

                BackgroundIntensity = _oldPeakProcessor.GetBackgroundIntensity(yVals);
                _oldPeakProcessor.SetOptions(SigNoise, BackgroundIntensity * PeakBackgroundRatio, false, PeakFitType.Quadratic);

                //Find peaks using DeconEngine
                var largestXValue = xyData.Xvalues[xyData.Xvalues.Length - 1];

                try
                {
                    _oldPeakProcessor.DiscoverPeaks(xVals, yVals, 0, largestXValue);
                }
                catch (Exception ex)
                {
                    Logger.Instance.AddEntry("ChromPeakDetector failed: " + ex.Message, true);
                    throw;
                }

                _oldDeconEnginePeaklist = _oldPeakProcessor.PeakData.PeakTops;
                foreach (var peak in _oldDeconEnginePeaklist)
                {
                    var chromPeak = new ChromPeak
                    {
                        XValue = peak.Mz,      // here,  mz is actually the scan / or NET
                        Height = (float)peak.Intensity,
                        SignalToNoise = peak.SignalToNoise,
                        Width = (float)peak.FWHM
                    };

                    peakList.Add(chromPeak);

                }
            }

            //resultList.Run.PeakList = new List<IPeak>();

            return peakList;
        }

        public override void Execute(ResultCollection resultList)
        {
            base.Execute(resultList);

            foreach (var item in resultList.Run.PeakList)
            {
                var peak = (ChromPeak)item;
                peak.NETValue = resultList.Run.NetAlignmentInfo.GetNETValueForScan((int)peak.XValue);
            }
        }

        protected override double GetBackgroundIntensity(double[] yValues, double[] xValues = null)
        {
            return 0;
        }

        //public override void AddPeakRelatedData(Run run)
        //{
        //    if (run.PeakList == null || run.PeakList.Count == 0)
        //    {
        //        bool workflowIsTargeted = (run.CurrentMassTag != null);
        //        if (workflowIsTargeted)
        //        {
        //            TargetedResultBase result = run.ResultCollection.CurrentTargetedResult;
        //            result.FailedResult = true;
        //            result.FailureType = Globals.TargetedResultFailureType.CHROM_PEAKS_NOT_DETECTED;
        //        }
        //    }

        //    foreach (ChromPeak peak in run.PeakList)
        //    {
        //        peak.NETValue = run.GetNETValueForScan((int)peak.XValue);
        //    }
        //}

        #endregion
    }
}
