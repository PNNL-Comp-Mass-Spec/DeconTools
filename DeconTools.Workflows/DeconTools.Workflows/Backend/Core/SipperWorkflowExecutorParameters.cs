namespace DeconTools.Workflows.Backend.Core
{
    public class SipperWorkflowExecutorParameters : WorkflowExecutorBaseParameters
    {

        #region Constructors
        #endregion

        #region Properties

        public string DbServer { get; set; }
        public string DbName { get; set; }
        public string DbTableName { get; set; }

        /// <summary>
        /// File path reference to file containing list of MassTags IDs which are used when filtering. Leave
        /// This empty if you want to analyze all Targets
        /// </summary>
        public string TargetsToFilterOn { get; set; }
        public string ReferenceDataForTargets { get; set; }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        public override Backend.Globals.TargetedWorkflowTypes WorkflowType
        {
            get { return Globals.TargetedWorkflowTypes.SipperWorkflowExecutor1; }
        }
    }
}
