using System.Reflection;
using PRISM;

namespace IQ.Console
{
    public class IqConsoleOptions
    {
        private const string PROGRAM_DATE = "February 23, 2018";

        public IqConsoleOptions()
        {
            InputFile = string.Empty;
            TargetsFile = string.Empty;
            ReferenceTargetFile = string.Empty;
            TargetFileForAlignment = string.Empty;
            OutputFolder = string.Empty;
            UseInputScan = false;
            WorkflowParameterFile = string.Empty;
            TemporaryWorkingFolder = string.Empty;
            IsAlignmentPerformed = false;
            AlignmentParameterFile = string.Empty;
            IsMassAlignmentPerformed = false;
            IsNetAlignmentPerformed = false;
            ChromGenSourceDataPeakBr = 2;
            ChromGenSourceDataSigNoise = 2;
            DataIsThresholded = true;
            ChromGenSourceDataProcessMsMs = false;
            UseOldIq = false;
            OutputGraphs = false;
        }

        [Option("I", "InputFile", ArgPosition = 1, Required = true, HelpText = "Input dataset or a text file containing a list of datasets. Can be a path to a thermo .Raw file, or a DMS dataset name")]
        public string InputFile { get; set; }

        [Option("T", "TargetsFile", ArgPosition = 3, HelpText = "A text file containing a list of IQ targets (overrides targets file defined in the WorkflowParameterFile .xml file). Supported formats= .txt, .mgf")]
        public string TargetsFile { get; set; }

        [Option("R", "ReferenceFile", HelpText = "A text file containing NET alignment reference data. Supported formats= .txt")]
        public string ReferenceTargetFile { get; set; }

        [Option("A", "AlignmentTargetsFile", HelpText = "A text file containing a list of IQ targets which are used in calibrating mass and elution time. Supported formats= .txt, .mgf")]
        public string TargetFileForAlignment { get; set; }

        [Option("D", "OutputFolder", HelpText = "Output folder ")]
        public string OutputFolder { get; set; }

        [Option("UseInputScan", HelpText = "Determines whether or not the user-supplied LC scan is used during IQ processing.")]
        public bool UseInputScan { get; set; }

        [Option("W", "WorkflowParameterFile", ArgPosition = 2, HelpText = "An .xml file containing a list of IQ parameters")]
        public string WorkflowParameterFile { get; set; }

        [Option("F", "TemporaryWorkingFolder", HelpText = "Temporary working folder where dataset is copied")]
        public string TemporaryWorkingFolder { get; set; }

        [Option("Q", "IsAlignmentPerformed", HelpText = "If true, IQ alignment is performed using AlignmentTargetsFile.  This option is obsolete; use IsMassAlignmentPerformed instead")]
        public bool IsAlignmentPerformed { get; set; }

        [Option("E", "AlignmentParameterFile", HelpText = "Parameters used for IQ mass and NET alignment")]
        public string AlignmentParameterFile { get; set; }

        [Option("DoMassAlignment", HelpText = "If true, mass alignment is performed.")]
        public bool IsMassAlignmentPerformed { get; set; }

        [Option("DoNetAlignment", HelpText = "If true, NET alignment is performed.")]
        public bool IsNetAlignmentPerformed { get; set; }

        [Option("PeakBRForChromGenerator", HelpText = "Peak detector setting. Typical value ranges from 1 - 10, with lower values creating more peaks on which XIC data is based.")]
        public double ChromGenSourceDataPeakBr { get; set; }

        [Option("S", "SignalToNoiseThreshForChromGenerator", HelpText = "Peak detector setting. Typical value ranges from 1 - 10, with lower values creating more peaks on which XIC data is based.")]
        public double ChromGenSourceDataSigNoise { get; set; }

        [Option("H", "Thresholded", HelpText = "Some mass spec data, like Orbitrap data, is thresholded. If so, set this to true.")]
        public bool DataIsThresholded { get; set; }

        [Option("ChromGenSourceDataProcessMsMs", HelpText = "Some mass spec data, like Orbitrap data, is thresholded. If so, set this to true.")]
        public bool ChromGenSourceDataProcessMsMs { get; set; }

        [Option("UseOldIQ", HelpText = "If true, uses the old IQ.")]
        public bool UseOldIq { get; set; }

        [Option("OutputGraphs", HelpText = "Output .png images of mass spectra for all the results. See folder 'OutputGraphs' ")]
        public bool OutputGraphs { get; set; }



        [HelpOption]
        public string GetUsage()
        {
            HelpText help = new HelpText("MyExe");
            help.AddPreOptionsLine("Usage: [add this]");
            help.AdditionalNewLineAfterOption = true;

            try
            {
                help.AddOptions(this);
            }
            catch (TargetInvocationException e)
            {
                System.Console.WriteLine(e.InnerException);
            }
            
            return help;
        }





        private void OnError(HelpText current)
        {
            HelpText.DefaultParsingErrorsHandler(this, current);
        }
    }
}
