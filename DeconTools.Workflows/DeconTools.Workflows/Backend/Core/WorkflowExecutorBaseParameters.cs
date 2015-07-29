
using System;

namespace DeconTools.Workflows.Backend.Core
{
    public abstract class WorkflowExecutorBaseParameters : WorkflowParameters
    {


        #region Constructors
        public WorkflowExecutorBaseParameters()
        {
            CopyRawFileLocal = false;
            DeleteLocalDatasetAfterProcessing = false;
            TargetedAlignmentIsPerformed = false;
            IsMassAlignmentPerformed = false;
            IsNetAlignmentPerformed = false;

            TargetType = Globals.TargetType.DatabaseTarget;

            ChromGenSourceDataPeakBR = 2;
            ChromGenSourceDataSigNoise = 3;
            ChromGenSourceDataProcessMsMs = false;
            ChromGenSourceDataIsThresholded = true;

            MinMzForDefiningChargeStateTargets = 400;
            MaxMzForDefiningChargeStateTargets = 1500;
            MaxNumberOfChargeStateTargetsToCreate = 100;



        }
        #endregion



        #region Properties

        public bool CopyRawFileLocal { get; set; }
        public bool DeleteLocalDatasetAfterProcessing { get; set; }
        public string FolderPathForCopiedRawDataset { get; set; }

        protected string mOutputFolderBase;
        public string OutputFolderBase
        {
            get
            {
                if (string.IsNullOrWhiteSpace(mOutputFolderBase))
                    return string.Empty;

                return mOutputFolderBase;
            }
            set
            {
                mOutputFolderBase = value;
            }
        }
        public string ReferenceTargetsFilePath { get; set; }
        public string TargetsFilePath { get; set; }
        public string TargetsBaseFolder { get; set; }
        public Globals.TargetType TargetType { get; set; }

        [Obsolete("No longer use. Use 'IsMassAlignmentPerformed'")]
        public bool TargetedAlignmentIsPerformed { get; set; }

        public bool IsMassAlignmentPerformed { get; set; }

        public bool IsNetAlignmentPerformed { get; set; }

        public string TargetedAlignmentWorkflowParameterFile { get; set; }
        public string WorkflowParameterFile { get; set; }
        //ChromGen Peak Generator

        //TODO: these chromGen parameters are duplicated in TargetedWorkflowParameters! Need to resolve this
        public double ChromGenSourceDataPeakBR { get; set; }
        public double ChromGenSourceDataSigNoise { get; set; }
        public bool ChromGenSourceDataIsThresholded { get; set; }
        public bool ChromGenSourceDataProcessMsMs { get; set; }


        /// <summary>
        /// Minimum m/z value used for defining the a range of IqChargeState targets
        /// </summary>
        public double MinMzForDefiningChargeStateTargets { get; set; }


        /// <summary>
        /// Maxium m/z value used for defining the a range of IqChargeState targets
        /// </summary>
        public double MaxMzForDefiningChargeStateTargets { get; set; }

        /// <summary>
        /// Maximum number of charge states to create
        /// </summary>
        public int MaxNumberOfChargeStateTargetsToCreate { get; set; }

        //public string ExportAlignmentFolder { get; set; }


        #endregion


    }
}
