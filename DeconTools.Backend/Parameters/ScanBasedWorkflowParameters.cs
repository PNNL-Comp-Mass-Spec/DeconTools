
namespace DeconTools.Backend.Parameters
{
    public class ScanBasedWorkflowParameters
    {

        #region Constructors

        public ScanBasedWorkflowParameters()
        {
            ProcessMS1 = true;
            ProcessMS2 = false;

            ExportFileType = Globals.ExporterType.Text;
            ScanBasedWorkflowType = Globals.ScanBasedWorkflowType.Standard;
            DeconvolutionType = Globals.DeconvolutionType.Thrash;
        }

        #endregion

        #region Properties

        public bool ProcessMS1 { get; set; }
        public bool ProcessMS2 { get; set; }

        public Globals.ExporterType ExportFileType { get; set; }
        public Globals.ScanBasedWorkflowType ScanBasedWorkflowType { get; set; }

        public Globals.DeconvolutionType DeconvolutionType { get; set; }

      
        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

    }
}
