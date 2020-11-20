
using System;

namespace DeconTools.Workflows.Backend.Core
{
    public abstract class WorkflowExecutorBaseParameters : WorkflowParameters
    {
        #region Constructors

        protected WorkflowExecutorBaseParameters()
        {
            CopyRawFileLocal = false;
            DeleteLocalDatasetAfterProcessing = false;
#pragma warning disable 618
            TargetedAlignmentIsPerformed = false;
#pragma warning restore 618
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
        public string LocalDirectoryPathForCopiedRawDataset { get; set; }

        protected string mOutputDirectoryBase;
        public string OutputDirectoryBase
        {
            get
            {
                if (string.IsNullOrWhiteSpace(mOutputDirectoryBase))
                    return string.Empty;

                return mOutputDirectoryBase;
            }
            set => mOutputDirectoryBase = value;
        }

        /// <summary>
        /// When true, append the targets file name to the output file name
        /// </summary>
        /// <remarks>Useful if searching the same dataset repeatedly with different targets files</remarks>
        public bool AppendTargetsFileNameToResultFile { get; set; }

        public string ReferenceTargetsFilePath { get; set; }

        public string TargetsFilePath { get; set; }

        /// <summary>
        /// Used by TryFindTargetsForCurrentDataset when TargetsFilePath is empty
        /// Will try to auto-find the targets file in this folder based on the input file name
        /// </summary>
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
