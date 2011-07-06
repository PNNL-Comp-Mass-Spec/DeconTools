using System;

namespace DeconTools.Workflows.Backend.Core
{
    public class PeakDetectAndExportWorkflowParameters:WorkflowParameters
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
            
            this.FrameMax = -1;
            this.FrameMin = -1;
            this.ScanMin = -1;
            this.ScanMax = -1;

            this.Num_LC_TimePointsSummed = 1;

        }

        #endregion

        #region Properties

        public double PeakBR { get; set; }

        public double SigNoiseThreshold { get; set; }

        public DeconTools.Backend.Globals.PeakFitType PeakFitType { get; set; }

        public bool IsDataThresholded { get; set; }


        public string OutputFolder { get; set; }

        public int Num_LC_TimePointsSummed { get; set; }

      

        /// <summary>
        /// Minimum scan to process. A value of -1 is interpreted as 'all scans'
        /// </summary>
        public int ScanMin { get; set; }

        /// <summary>
        /// Maximum scan to process. A value of -1 is interpreted as 'all scans'
        /// </summary>
        public int ScanMax { get; set; }

        /// <summary>
        /// Minimum frame to process. A value of -1 is interpreted as 'all frames'
        /// </summary>
        public int FrameMin { get; set; }

        /// <summary>
        /// Maximum frame to process. A value of -1 is interpreted as 'all frames'
        /// </summary>
        public int FrameMax { get; set; }

        public bool ProcessMSMS { get; set; }



        #endregion

      

        public override string WorkflowType
        {
            get { return "PeakDetectAndExportWorkflow1"; }
        }

        public override void LoadParameters(string xmlFilename)
        {
            throw new NotImplementedException();
        }

       
    }
}
