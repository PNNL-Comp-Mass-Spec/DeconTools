
using System.ComponentModel;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Workflows
{
    public class StandardIMSScanBasedWorkflow : IMSScanBasedWorkflow
    {

        #region Constructors

        internal StandardIMSScanBasedWorkflow(OldDecon2LSParameters parameters, Run run, string outputFolderPath = null, BackgroundWorker backgroundWorker = null)
            : base(parameters, run, outputFolderPath,backgroundWorker)
        {

        }

       
        #endregion








    }
}
