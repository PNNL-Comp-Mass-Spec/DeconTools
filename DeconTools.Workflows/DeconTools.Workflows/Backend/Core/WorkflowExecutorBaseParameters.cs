
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
            TargetType = Globals.TargetType.DatabaseTarget;


            AlignmentInfoIsExported = true;
            AlignmentFeaturesAreSavedToTextFile = true;
        }
        #endregion

      

        #region Properties

        public bool CopyRawFileLocal { get; set; }
        public bool DeleteLocalDatasetAfterProcessing { get; set; }
        public string FolderPathForCopiedRawDataset { get; set; }
        public string LoggingFolder { get; set; }
        public string TargetsUsedForAlignmentFilePath { get; set; }
        public string TargetsFilePath { get; set; }
        public string TargetsBaseFolder { get; set; }
        public Globals.TargetType TargetType { get; set; }
        public string ResultsFolder { get; set; }
        public bool TargetedAlignmentIsPerformed { get; set; }
        public string TargetedAlignmentWorkflowParameterFile { get; set; }
        public string WorkflowParameterFile { get; set; }
        public bool AlignmentInfoIsExported { get; set; }
        public bool AlignmentFeaturesAreSavedToTextFile { get; set; }
        public string AlignmentInfoFolder { get; set; }

        //public string ExportAlignmentFolder { get; set; }

        
        #endregion


    }
}
