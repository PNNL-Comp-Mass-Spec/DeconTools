
namespace DeconTools.Workflows.Backend.Core
{
    public class IqTargetBasic : IqTarget
    {
        public IqTargetBasic()
        {
        }

        public IqTargetBasic(IqWorkflow workflow)
            : base(workflow)
        {
        }

        /// <summary>
        /// Copies the target parameter to a new IqTarget.
        /// </summary>
        /// <param name="target">target parameter</param>
        /// <param name="includeRecursion">whether or not to include the root, parent, and children in the clone</param>
        public IqTargetBasic(IqTarget target)
            : base(target)
        {
        }
    }
}
