using System;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.PeakDetectors
{
    public class ChromPeakDetector:DeconToolsPeakDetectorV2
    {

        #region Constructors

        public ChromPeakDetector():base()
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

        public override Core.Peak CreatePeak(double xvalue, float height, float width = 0, float signalToNoise = 0)
        {
            return new ChromPeak(xvalue, height, width, signalToNoise);
        }


        #endregion
        protected override void ExecutePostProcessingHook(Run run)
        {
            foreach (ChromPeak chromPeak in run.PeakList)
            {
                chromPeak.NETValue = run.GetNETValueForScan((int) Math.Round(chromPeak.XValue));
            }
        }

        #region Private Methods

        #endregion

    }
}
