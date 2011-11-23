using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;

namespace DeconTools.Backend.ProcessingTasks
{
    public class PeakDetectorFactory
    {


        public static PeakDetector CreatePeakDetector(OldDecon2LSParameters parameters)
         {
             return CreatePeakDetector(Globals.PeakDetectorType.DeconTools, parameters);
         }


        public static PeakDetector CreatePeakDetector(Globals.PeakDetectorType peakDetectorType, OldDecon2LSParameters parameters)
        {
            switch (peakDetectorType)
            {
                case Globals.PeakDetectorType.DeconTools:
                    return new DeconToolsPeakDetector(parameters.PeakProcessorParameters);
                    
                case Globals.PeakDetectorType.DeconToolsChromPeakDetector:
                    return new ChromPeakDetector(parameters.PeakProcessorParameters);
                    
                default:
                    throw new ArgumentOutOfRangeException("peakDetectorType");
            }
        }
    }
}
