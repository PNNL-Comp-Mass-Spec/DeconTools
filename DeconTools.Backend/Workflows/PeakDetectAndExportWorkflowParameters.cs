using System;

namespace DeconTools.Backend.Workflows
{
    public class PeakDetectAndExportWorkflowParameters
    {
        #region Constructors
        public PeakDetectAndExportWorkflowParameters()
        {
            PeakBR = 2;
            SigNoiseThreshold = 2;
            PeakFitType = Globals.PeakFitType.QUADRATIC;
            IsDataThresholded = false;

            OutputDirectory = string.Empty;

            IMSScanMax = -1;
            IMSScanMin = -1;
            LCScanMin = -1;
            LCScanMax = -1;

            Num_LC_TimePointsSummed = 1;
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

        public Globals.PeakFitType PeakFitType { get; set; }

        /// <summary>
        /// Should be set to True if the instrument thresholds the data when saving to disk
        /// </summary>
        public bool IsDataThresholded { get; set; }

        /// <summary>
        /// Output directory
        /// </summary>
        public string OutputDirectory { get; set; }

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

        /// <summary>
        /// If true, process MS/MS spectra
        /// </summary>
        public bool ProcessMSMS { get; set; }

        #endregion

    }
}
