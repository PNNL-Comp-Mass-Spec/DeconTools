﻿using DeconTools.Backend.ProcessingTasks;

namespace DeconTools.Workflows.Backend.Core
{
    public class BasicTargetedWorkflowParameters:TargetedWorkflowParameters
    {

      
        public BasicTargetedWorkflowParameters()
        {
            this.ChromGeneratorMode = ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
            this.ChromNETTolerance = 0.025;
            this.ChromPeakDetectorPeakBR = 1;
            this.ChromPeakDetectorSigNoise = 1;
            this.ChromSmootherNumPointsInSmooth = 9;
            this.ChromToleranceInPPM = 10;
            this.MSPeakDetectorPeakBR = 2;
            this.MSPeakDetectorSigNoise = 2;
            this.MSToleranceInPPM = 10;
            this.NumMSScansToSum = 1;
            this.NumChromPeaksAllowedDuringSelection = 20;


            this.ChromGenSourceDataPeakBR = 2;
            this.ChromGenSourceDataSigNoise = 3;

            this.ResultType = DeconTools.Backend.Globals.MassTagResultType.BASIC_MASSTAG_RESULT;
        }

        public override Globals.TargetedWorkflowTypes WorkflowType
        {
            get
            {
                return  Globals.TargetedWorkflowTypes.UnlabelledTargeted1;
            }
        }

    }
}
