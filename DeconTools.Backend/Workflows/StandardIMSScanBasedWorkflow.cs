
using System.ComponentModel;
using DeconTools.Backend.Core;
using DeconTools.Backend.Parameters;

namespace DeconTools.Backend.Workflows
{
    public class StandardIMSScanBasedWorkflow : IMSScanBasedWorkflow
    {
        #region Constructors

        internal StandardIMSScanBasedWorkflow(DeconToolsParameters parameters, Run run, string outputDirectoryPath = null, BackgroundWorker backgroundWorker = null)
            : base(parameters, run, outputDirectoryPath, backgroundWorker)
        {
        }

        #endregion

    }
}
