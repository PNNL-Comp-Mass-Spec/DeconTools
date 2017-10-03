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

        private DeconToolsPeakDetectorV2 _peakDetectorV2;

        #region Constructors

        public ChromPeakDetectorOld():base() { }

        public ChromPeakDetectorOld(double peakBackgroundRatio, double sigNoise)
        {
            this.PeakBackgroundRatio = peakBackgroundRatio;
            this.SigNoise = sigNoise;

            _peakDetectorV2 = new DeconToolsPeakDetectorV2();
            _peakDetectorV2.PeakToBackgroundRatio = peakBackgroundRatio;
            _peakDetectorV2.SignalToNoiseThreshold = sigNoise;
            _peakDetectorV2.PeakFitType = Globals.PeakFitType.QUADRATIC;
            _peakDetectorV2.IsDataThresholded = false;
        }

        #endregion

        #region Properties

        public double PeakBackgroundRatio { get; set; }

        public double SigNoise { get; set; }

        #endregion

        #region Public Methods

        //TODO: remove code duplication (see DeconToolsPeakDetector)
        public override List<Peak> FindPeaks(XYData xydata, double xMin = 0, double xMax = 0)
        {
            var peakList = new List<Peak>();

            if (xydata == null)
            {
                return peakList;
            }

            if (UseNewPeakDetector)
            {
                _peakDetectorV2.FindPeaks(xydata, xMin, xMax);

            }
            else
            {
                var xvals = xydata.Xvalues.ToList();
                var yvals = xydata.Yvalues.ToList();

                if (_oldPeakProcessor==null)
                {
                    _oldPeakProcessor = new PeakProcessor();
                }

                BackgroundIntensity = _oldPeakProcessor.GetBackgroundIntensity(yvals);
                _oldPeakProcessor.SetOptions(SigNoise, BackgroundIntensity * PeakBackgroundRatio, false, PeakFitType.Quadratic);

                //Find peaks using DeconEngine
                var largestXValue = xydata.Xvalues[xydata.Xvalues.Length - 1];

                try
                {
                    _oldPeakProcessor.DiscoverPeaks(xvals, yvals, 0, largestXValue);
                }
                catch (Exception ex)
                {
                    Logger.Instance.AddEntry("ChromPeakDetector failed.", Logger.Instance.OutputFilename);
                    throw ex;
                }

                _oldDeconEnginePeaklist = _oldPeakProcessor.PeakData.PeakTops;
                foreach (var peak in _oldDeconEnginePeaklist)
                {
                    var chromPeak = new ChromPeak();
                    chromPeak.XValue = peak.Mz;          // here,  mz is actually the scan / or NET
                    chromPeak.Height = (float)peak.Intensity;
                    chromPeak.SignalToNoise = (float)peak.SignalToNoise;
                    chromPeak.Width = (float)peak.FWHM;

                    peakList.Add(chromPeak);

                }
            }

            //resultList.Run.PeakList = new List<IPeak>();

            return peakList;
        }

        public override void Execute(ResultCollection resultList)
        {
            base.Execute(resultList);

            foreach (ChromPeak peak in resultList.Run.PeakList)
            {
                peak.NETValue = resultList.Run.NetAlignmentInfo.GetNETValueForScan((int)peak.XValue);
            }
        }

        protected override double GetBackgroundIntensity(double[] yvalues, double[] xvalues = null)
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
