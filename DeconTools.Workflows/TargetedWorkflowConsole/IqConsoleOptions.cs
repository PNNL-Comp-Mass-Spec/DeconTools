using CommandLine;
using CommandLine.Text;

namespace IQ.Console
{
    public class IqConsoleOptions
    {
        [Option('i', "InputFile", Required = true, HelpText = "Input dataset or a text file containing a list of datasets. Supported file formats = Thermo .raw; ")]
        public string InputFile { get; set; }

        [Option('t', "TargetsFile", Required = true, HelpText = "A text file containing a list of IQ targets. Supported formats= .txt, .mgf")]
        public string TargetFile { get; set; }

        [Option('r', "ReferenceFile", Required = false, HelpText = "A text file containing NET alignment reference data. Supported formats= .txt")]
        public string ReferenceTargetFile { get; set; }

        [Option('a', "AlignmentTargetsFile", HelpText = "A text file containing a list of IQ targets which are used in calibrating mass and elution time. Supported formats= .txt, .mgf")]
        public string TargetFileForAlignment { get; set; }

        [Option('d', "OutputFolder", HelpText = "Output folder ")]
        public string OutputFolder { get; set; }
        
        [Option('s', "UseInputScan", DefaultValue = false, HelpText = "Determines whether or not the user-supplied LC scan is used during IQ processing.")]
        public bool UseInputScan { get; set; }

        [Option('w', "WorkflowParameterFile", Required = true, HelpText = "An .xml file containing a list of IQ parameters")]
        public string WorkflowParameterFile { get; set; }

        [Option('f', "TemporaryWorkingFolder", HelpText = "Temporary working folder where dataset is copied")]
        public string TemporaryWorkingFolder { get; set; } 

        [ParserState]
        public IParserState LastParserState { get; set; }

        [Option('q', "IsAlignmentPerformed", DefaultValue = false, HelpText = "If true, IQ alignment is performed using AlignmentTargetsFile.")]
        public bool IsAlignmentPerformed { get; set; }

        [Option('e', "AlignmentParameterFile", HelpText = "Parameters used for IQ mass and NET alignment")]
        public string AlignmentParameterFile { get; set; }

        [Option("DoMassAlignment", DefaultValue = false, HelpText = "If true, mass alignment is performed.")]
        public bool IsMassAlignmentPerformed { get; set; }

        [Option("DoNetAlignment", DefaultValue = false, HelpText = "If true, NET alignment is performed.")]
        public bool IsNetAlignmentPerformed { get; set; }

        [Option("PeakBRForChromGenerator", DefaultValue = 2, HelpText = "Peak detector setting. Typical value ranges from 1 - 10, with lower values creating more peaks on which XIC data is based.")]
        public double ChromGenSourceDataPeakBr { get; set; }

        [Option('s', "SignalToNoiseThreshForChromGenerator", DefaultValue = 2, HelpText = "Peak detector setting. Typical value ranges from 1 - 10, with lower values creating more peaks on which XIC data is based.")]
        public double ChromGenSourceDataSigNoise { get; set; }

        [Option('h', "Thresholded", DefaultValue = true, HelpText = "Some mass spec data, like Orbitrap data, is thresholded. If so, set this to true.")]
        public bool DataIsThresholded { get; set; }

        [Option("ChromGenSourceDataProcessMsMs", DefaultValue = false, HelpText = "Some mass spec data, like Orbitrap data, is thresholded. If so, set this to true.")]
        public bool ChromGenSourceDataProcessMsMs { get; set; }

         [Option("UseNewIQ", DefaultValue = true, HelpText = "Use the latest version of IQ or not.")]
        public bool UseNewIQ { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }

    }
}
