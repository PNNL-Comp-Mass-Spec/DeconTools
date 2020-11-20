using System;

namespace DeconTools.Workflows.Backend.Core
{
    public class IqWorkflowFactory
    {
        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        public IqWorkflow  CreateWorkflow(string workflowType, TargetedWorkflowParameters parameters)
        {
            workflowType = workflowType.ToLower();

            switch (workflowType)
            {
                case "unlabeled":
                    return new BasicIqWorkflow(parameters);
                case "o16o18":
                    break;
                case "n14n15":
                    break;
                default:
                    throw new NotImplementedException("Cannot create workflow. Workflow type is not known. Input workflowType= " +
                                                      workflowType);
            }

            return null;
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
