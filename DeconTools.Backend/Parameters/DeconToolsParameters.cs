
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

        #endregion

        #region Public Methods

        public void Load(string xmlFilename)
        {
            XDocument xdocument = XDocument.Load(xmlFilename);

            var parameterBaseElement = xdocument.Element("parameters");


            if (parameterBaseElement == null)
            {
                throw new IOException("Problem reading xml file. Expected element 'parameters' but it was not found");
            }


            var peakDetectionElement = parameterBaseElement.Element("PeakParameters");
            PeakDetectorParameters.LoadParameters(peakDetectionElement);
            
            var thrashElement = parameterBaseElement.Element("HornTransformParameters");
            ThrashParameters.LoadParameters(thrashElement);

            var miscElement = parameterBaseElement.Element("Miscellaneous");
            MiscMSProcessingParameters.LoadParameters(miscElement);

            //MSGenerator parameters are found in the old DeconTools parameter file under the 'HornTransformParameters' and the 'Miscellaneous' parameters
            MSGeneratorParameters.LoadParameters(thrashElement);
            MSGeneratorParameters.LoadParameters(miscElement);

            //ScanBasedWorkflowParameters parameters are found in the old DeconTools parameter file under the 'HornTransformParameters' and the 'Miscellaneous' parameters
            ScanBasedWorkflowParameters.LoadParameters(thrashElement);
            ScanBasedWorkflowParameters.LoadParameters(miscElement);
            ScanBasedWorkflowParameters.LoadParameters(peakDetectionElement);





        }

    

        #endregion
    }
}
