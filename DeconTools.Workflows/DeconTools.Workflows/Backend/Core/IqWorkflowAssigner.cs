
using System.Collections.Generic;

namespace DeconTools.Workflows.Backend.Core
{
    

    /// <summary>
    /// Helps developers assign workflows to targets
    /// </summary>
    public class IqWorkflowAssigner
    {
        private IqTargetUtilities _targetUtilities = new IqTargetUtilities();

        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods


        public void AssignWorkflowToParent(IqWorkflow workflow, IqTarget target)
        {
            target.RootTarget.SetWorkflow(workflow);
        }


        public void AssignWorkflowToChildren(IqWorkflow workflow, IqTarget target, int childLevel=1)
        {
            var targetsAtGivenNodeLevel = _targetUtilities.GetTargetsFromNodelLevel(target, childLevel);

            foreach (var childIqTarget in targetsAtGivenNodeLevel)
            {
                childIqTarget.SetWorkflow(workflow);
            }
        }


        public void AssignWorkflowToParent(IqWorkflow workflow, List<IqTarget> targets)
        {
            foreach (var iqTarget in targets)
            {
                iqTarget.RootTarget.SetWorkflow(workflow);
            }
        }


        public void AssignWorkflowToChildren(IqWorkflow workflow, List<IqTarget> targets, int childLevel = 1)
        {
            foreach (var iqTarget in targets)
            {
                var targetsAtGivenNodeLevel = _targetUtilities.GetTargetsFromNodelLevel(iqTarget, childLevel);

                foreach (var childIqTarget in targetsAtGivenNodeLevel)
                {
                    childIqTarget.SetWorkflow(workflow);
                }
            }
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
