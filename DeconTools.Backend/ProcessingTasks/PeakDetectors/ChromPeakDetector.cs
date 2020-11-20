using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.PeakDetectors
{
    public class ChromPeakDetector : DeconToolsPeakDetectorV2
    {
        #region Constructors

        public ChromPeakDetector() : base()
        {
            IsDataThresholded = false;
        }

        public ChromPeakDetector(double peakToBackgroundRatio, double signalToNoiseRatio) : this()
        {
            PeakToBackgroundRatio = peakToBackgroundRatio;
            SignalToNoiseThreshold = signalToNoiseRatio;
        }

        #endregion

        #region Properties

        #endregion

        #region Public Methods

        public override Core.Peak CreatePeak(double xValue, float height, float width = 0, float signalToNoise = 0)
        {
            return new ChromPeak(xValue, height, width, signalToNoise);
        }

        public void CalculateElutionTimes(Run run, List<Peak> peakList)
        {
            foreach (var peak in peakList)
            {
                var chromPeak = (ChromPeak)peak;
                chromPeak.NETValue = run.NetAlignmentInfo.GetNETValueForScan((int)Math.Round(chromPeak.XValue));
            }
        }

        public void FilterPeaksOnNET(double chromNetTolerance, double elutionTime, List<Peak> peakList)
        {
            var outOfRange = new List<Peak>();
            foreach (var peak in peakList)
            {
                var chromPeak = (ChromPeak)peak;
                if (Math.Abs(chromPeak.NETValue - elutionTime) >= chromNetTolerance)
                //peak.NETValue was determined by the ChromPeakDetector or a future ChromAligner Task
                {
                    outOfRange.Add(chromPeak);
                }
            }

            foreach (var peak in outOfRange)
            {
                var chromPeak = (ChromPeak)peak;
                peakList.Remove(chromPeak);
            }
        }

        #endregion
        protected override void ExecutePostProcessingHook(Run run)
        {
            CalculateElutionTimes(run, run.PeakList);
        }
    }
}
