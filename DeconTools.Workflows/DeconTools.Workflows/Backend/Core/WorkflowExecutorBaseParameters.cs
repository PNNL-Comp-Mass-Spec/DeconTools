
namespace DeconTools.Workflows.Backend.Core
{
    public abstract class WorkflowExecutorBaseParameters : WorkflowParameters
    {
        
        #region Constructors
        public WorkflowExecutorBaseParameters()
        {
            this.CopyRawFileLocal = false;
            this.DeleteLocalDatasetAfterProcessing = false;
            this.TargetedAlignmentIsPerformed = false;
        }
        #endregion

        #region Properties

        public bool CopyRawFileLocal { get; set; }
        public bool DeleteLocalDatasetAfterProcessing { get; set; }
        public string FileContainingDatasetPaths { get; set; }
        public string FolderPathForCopiedRawDataset { get; set; }
        public string LoggingFolder { get; set; }
        public string MassTagsForAlignmentFilePath { get; set; }
        public string MassTagsToBeTargetedFilePath { get; set; }
        public string ResultsFolder { get; set; }
        public bool TargetedAlignmentIsPerformed { get; set; }
        public string TargetedAlignmentWorkflowParameterFile { get; set; }
        public string WorkflowParameterFile { get; set; }
        public string AlignmentInfoFolder { get; set; }

        
        #endregion


    }
}
