
using System;
using System.IO;
using System.Xml.Linq;

namespace DeconTools.Backend.Parameters
{
    public class DeconToolsParameters
    {

        #region Constructors

        public DeconToolsParameters()
        {
            MSGeneratorParameters = new MSGeneratorParameters();
            PeakDetectorParameters = new PeakDetectorParameters();
            ThrashParameters = new ThrashParameters();
            MiscMSProcessingParameters = new MiscMSProcessingParameters();
            ScanBasedWorkflowParameters = new ScanBasedWorkflowParameters();
        }

        #endregion

        #region Properties

        public MSGeneratorParameters MSGeneratorParameters { get; set; }

        public PeakDetectorParameters PeakDetectorParameters { get; set; }

        public ThrashParameters ThrashParameters { get; set; }

        public MiscMSProcessingParameters MiscMSProcessingParameters { get; set; }

        public ScanBasedWorkflowParameters ScanBasedWorkflowParameters { get; set; }

        public string ParameterFilename { get; set; }

        #endregion

        #region Public Methods

        public void LoadFromOldDeconToolsParameterFile(string xmlFilename)
        {
            XDocument xdocument = XDocument.Load(xmlFilename);
            var parameterBaseElement = xdocument.Element("parameters");

            if (parameterBaseElement == null)
            {
                throw new IOException("Problem reading xml file. Expected element 'parameters' but it was not found");
            }

            ParameterFilename = xmlFilename;

            var peakDetectionElement = parameterBaseElement.Element("PeakParameters");
            PeakDetectorParameters.LoadParametersV2(peakDetectionElement);
            
            var thrashElement = parameterBaseElement.Element("HornTransformParameters");
            ThrashParameters.LoadParametersV2(thrashElement);

            var miscElement = parameterBaseElement.Element("Miscellaneous");
            MiscMSProcessingParameters.LoadParametersV2(miscElement);

            //MSGenerator parameters are found in the old DeconTools parameter file under the 'HornTransformParameters' and the 'Miscellaneous' parameters
            MSGeneratorParameters.LoadParametersV2(thrashElement);
            MSGeneratorParameters.LoadParametersV2(miscElement);

            //ScanBasedWorkflowParameters parameters are found in the old DeconTools parameter file under the 'HornTransformParameters' and the 'Miscellaneous' parameters
            ScanBasedWorkflowParameters.LoadParametersV2(thrashElement);
            ScanBasedWorkflowParameters.LoadParametersV2(miscElement);
            ScanBasedWorkflowParameters.LoadParametersV2(peakDetectionElement);
        }

    
        public void Save(string xmlFilename)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
