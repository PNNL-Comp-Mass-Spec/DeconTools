using System.ComponentModel;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.Workflows
{
    public class SaturationIMSScanBasedWorkflow:IMSScanBasedWorkflow
    {

        #region Constructors

        internal SaturationIMSScanBasedWorkflow(OldDecon2LSParameters parameters, Run run, string outputFolderPath = null, BackgroundWorker backgroundWorker = null)
            : base(parameters, run, outputFolderPath, backgroundWorker)
        {
            DeconTools.Utilities.Check.Require(run is UIMFRun, "Cannot create workflow. Run is required to be a UIMFRun for this type of workflow");

        }
        

        #endregion

   

     
    }
}
