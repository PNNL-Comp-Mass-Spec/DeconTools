using System;

namespace DeconTools.Workflows.Backend.Core
{
    public class PeakDetectAndExportWorkflowParameters : WorkflowParameters
    {

        #region Constructors
        public PeakDetectAndExportWorkflowParameters()
        {
            this.PeakBR = 2;
            this.SigNoiseThreshold = 2;
            this.PeakFitType = DeconTools.Backend.Globals.PeakFitType.QUADRATIC;
            this.IsDataThresholded = false;

            //and empty OutputFolder
            this.OutputFolder = String.Empty;

            this.IMSScanMax = -1;
            this.IMSScanMin = -1;
            this.LCScanMin = -1;
            this.LCScanMax = -1;

            this.Num_LC_TimePointsSummed = 1;
            NumIMSScansSummed = -1;

            MS2PeakDetectorPeakBR = PeakBR;
            MS2PeakDetectorSigNoiseThreshold = SigNoiseThreshold;
            MS2PeakDetectorDataIsThresholded = IsDataThresholded;

        }

        #endregion

        #region Properties

        public double PeakBR { get; set; }

        

        public double SigNoiseThreshold { get; set; }

        
        /// <summary>
        /// For MS2 peak detection for data in profile mode. Otherwise, this does not need to be defined.
        /// </summary>
        public double MS2PeakDetectorPeakBR { get; set; }

        /// <summary>
        /// For MS2 peak detection for data in profile mode. Otherwise, this does not need to be defined.
        /// </summary>
        public double MS2PeakDetectorSigNoiseThreshold { get; set; }

        /// <summary>
        /// For MS2 peak detection for data in profile mode. Otherwise, this does not need to be defined.
        /// </summary>
        public bool MS2PeakDetectorDataIsThresholded { get; set; }


        public DeconTools.Backend.Globals.PeakFitType PeakFitType { get; set; }

        public bool IsDataThresholded { get; set; }


        public string OutputFolder { get; set; }

        public int Num_LC_TimePointsSummed { get; set; }

        /// <summary>
        /// Number of IMS Scans summed. -1 means all IMSScans are summed in a single Frame
        /// </summary>
        public int NumIMSScansSummed { get; set; }


        /// <summary>
        /// Minimum LC scan to process. A value of -1 is interpreted as 'all LC scans'
        /// </summary>
        public int LCScanMin { get; set; }

        /// <summary>
        /// Maximum LC scan to process. A value of -1 is interpreted as 'all LC scans'
        /// </summary>
        public int LCScanMax { get; set; }

        /// <summary>
        /// Minimum IMSScan to process. A value of -1 is interpreted as 'all frames'
        /// </summary>
        public int IMSScanMin { get; set; }

        /// <summary>
        /// Maximum IMSScan to process. A value of -1 is interpreted as 'all frames'
        /// </summary>
        public int IMSScanMax { get; set; }

        public bool ProcessMSMS { get; set; }



        #endregion



        public override Globals.TargetedWorkflowTypes WorkflowType
        {
            get { return Globals.TargetedWorkflowTypes.PeakDetectAndExportWorkflow1; }
        }

        public override void LoadParameters(string xmlFilename)
        {
            throw new NotImplementedException();
        }


    }
}
