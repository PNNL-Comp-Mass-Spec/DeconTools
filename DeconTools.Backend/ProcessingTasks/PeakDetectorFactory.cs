using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconToolsV2.Peaks;

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

                    //TODO: Use following code when THRASH is refactored
                    //DeconToolsPeakDetectorV2 peakDetector = new DeconToolsPeakDetectorV2();
                    //peakDetector.SignalToNoiseThreshold = parameters.PeakProcessorParameters.SignalToNoiseThreshold;
                    //peakDetector.PeakToBackgroundRatio = parameters.PeakProcessorParameters.PeakBackgroundRatio;
                    //peakDetector.IsDataThresholded = parameters.PeakProcessorParameters.ThresholdedData;

                    //switch (parameters.PeakProcessorParameters.PeakFitType)
                    //{
                    //    case PEAK_FIT_TYPE.LORENTZIAN:
                    //        peakDetector.PeakFitType = Globals.PeakFitType.LORENTZIAN;
                    //        break;
                    //    case PEAK_FIT_TYPE.QUADRATIC:
                    //        peakDetector.PeakFitType= Globals.PeakFitType.QUADRATIC;
                    //        break;
                    //    case PEAK_FIT_TYPE.APEX:
                    //        peakDetector.PeakFitType= Globals.PeakFitType.APEX;
                    //        break;
                    //    default:
                    //        throw new ArgumentOutOfRangeException();
                    //}

                    //peakDetector.PeaksAreStored = parameters.PeakProcessorParameters.WritePeaksToTextFile;


                    //return peakDetector;
                    
                case Globals.PeakDetectorType.DeconToolsChromPeakDetector:
                    return new ChromPeakDetectorOld(parameters.PeakProcessorParameters);
                    
                default:
                    throw new ArgumentOutOfRangeException("peakDetectorType");
            }
        }
    }
}
