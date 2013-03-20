using System;
using System.Collections.Generic;
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

        public void CalculateElutionTimes(Run run, List<Peak> peakList)
        {
            foreach (ChromPeak chromPeak in peakList)
            {
                chromPeak.NETValue = run.GetNETValueForScan((int)Math.Round(chromPeak.XValue));
            }
        }

        public void FilterPeaksOnNET(double chromNetTolerance, double elutionTime, List<Peak> peakList)
        {
            List<Peak> outOfRange = new List<Peak>();
            foreach (ChromPeak peak in peakList)
            {
                if (Math.Abs(peak.NETValue - elutionTime) >= chromNetTolerance)
                //peak.NETValue was determined by the ChromPeakDetector or a future ChromAligner Task
                {
                    outOfRange.Add(peak);
                }
            }

            foreach (ChromPeak peak in outOfRange)
            {
                peakList.Remove(peak);
            }
        }


        #endregion
        protected override void ExecutePostProcessingHook(Run run)
        {
            CalculateElutionTimes(run, run.PeakList);
        }

      

    }
}
