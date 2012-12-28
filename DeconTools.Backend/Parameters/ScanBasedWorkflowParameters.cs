
using System;
using System.Xml.Linq;

namespace DeconTools.Backend.Parameters
{
    public class ScanBasedWorkflowParameters:ParametersBase
    {

        #region Constructors

        public ScanBasedWorkflowParameters()
        {
            ProcessMS1 = true;
            ProcessMS2 = false;

            ExportPeakData = false;
            ExportFileType = Globals.ExporterType.Text;
            ScanBasedWorkflowName = "standard";
            DeconvolutionType = Globals.DeconvolutionType.Thrash;

            IsRefittingPerformed = true;

        }

        #endregion

        #region Properties

        public bool ProcessMS1 { get; set; }
        public bool ProcessMS2 { get; set; }

        public bool ExportPeakData { get; set; }

        /// <summary>
        /// Least-squares fitting is performed on MS feature. This is standard for profiles found Thrash, but not standard for other deconvolution algorithms (e.g. RAPID)
        /// </summary>
        public bool IsRefittingPerformed { get; set; }

        public Globals.ExporterType ExportFileType { get; set; }
        public string ScanBasedWorkflowName { get; set; }

        public Globals.DeconvolutionType DeconvolutionType { get; set; }

        [Obsolete("UseRAPIDDeconvolution is deprecated, use DeconvolutionType instead.")]
        public bool UseRAPIDDeconvolution { get; set; }
      
        #endregion

        #region Public Methods
        public override void LoadParameters(XElement xElement)
        {
            throw new NotImplementedException();
        }


        public override void LoadParametersV2(XElement xElement)
        {
            ProcessMS1 = GetBoolVal(xElement, "Process_MS", ProcessMS1);
            ProcessMS2 = GetBoolVal(xElement, "ProcessMSMS", ProcessMS2);

            ExportFileType = (Globals.ExporterType)GetEnum(xElement, "ExportFileType", ExportFileType.GetType(), ExportFileType);
            ScanBasedWorkflowName = GetStringValue(xElement, "ScanBasedWorkflowType", ScanBasedWorkflowName);
            DeconvolutionType = (Globals.DeconvolutionType)GetEnum(xElement, "DeconvolutionType", DeconvolutionType.GetType(), DeconvolutionType);

            UseRAPIDDeconvolution = GetBoolVal(xElement, "UseRAPIDDeconvolution", UseRAPIDDeconvolution);

            IsRefittingPerformed = GetBoolVal(xElement, "ReplaceRAPIDScoreWithHornFitScore", IsRefittingPerformed);

            ExportPeakData = GetBoolVal(xElement, "WritePeaksToTextFile", ExportPeakData);


        }
        #endregion

 
    }
}
