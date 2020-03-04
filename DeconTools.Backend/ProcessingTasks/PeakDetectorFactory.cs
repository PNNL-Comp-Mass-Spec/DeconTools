using System;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;

namespace DeconTools.Backend.ProcessingTasks
{
    public class PeakDetectorFactory
    {
        public static PeakDetector CreatePeakDetector(DeconToolsParameters parameters)
        {
            return CreatePeakDetector(Globals.PeakDetectorType.DeconTools, parameters);
        }

        public static PeakDetector CreatePeakDetector(Globals.PeakDetectorType peakDetectorType, DeconToolsParameters parameters)
        {
            switch (peakDetectorType)
            {
                case Globals.PeakDetectorType.DeconTools:

                    var peakDetector = new DeconToolsPeakDetectorV2(
                       parameters.PeakDetectorParameters.PeakToBackgroundRatio,
                       parameters.PeakDetectorParameters.SignalToNoiseThreshold,
                       parameters.PeakDetectorParameters.PeakFitType,
                       parameters.PeakDetectorParameters.IsDataThresholded);

                    peakDetector.PeaksAreStored = parameters.PeakDetectorParameters.PeaksAreStored;

                    return peakDetector;

                case Globals.PeakDetectorType.DeconToolsChromPeakDetector:
                    return new ChromPeakDetector(parameters.PeakDetectorParameters.PeakToBackgroundRatio,
                                                 parameters.PeakDetectorParameters.SignalToNoiseThreshold);

                default:
                    throw new ArgumentOutOfRangeException(nameof(peakDetectorType));
            }
        }
    }
}
