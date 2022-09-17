using System.Reflection;
using PRISM;

namespace IQ.Console
{
    public class IqConsoleOptions
    {
        // Ignore Spelling: Orbitrap, workflow

        private const string PROGRAM_DATE = "September 16, 2022";

        public IqConsoleOptions()
        {
            InputFile = string.Empty;
            TargetsFile = string.Empty;
            ReferenceTargetFile = string.Empty;
            TargetFileForAlignment = string.Empty;
            OutputDirectory = string.Empty;
            AppendTargetsFileNameToResultFile = false;
            UseInputScan = false;
            WorkflowParameterFile = string.Empty;
            TemporaryWorkingDirectory = string.Empty;
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

        [Option("I", "InputFile", ArgPosition = 1, Required = true, HelpText = "Input dataset or a text file containing a list of datasets. Can be a path to a Thermo .Raw file, or a DMS dataset name")]
        public string InputFile { get; set; }

        [Option("T", "TargetsFile", ArgPosition = 3, HelpText = "A text file containing a list of IQ targets (overrides targets file defined in the WorkflowParameterFile .xml file). Supported formats= .txt, .mgf")]
        public string TargetsFile { get; set; }

        [Option("R", "ReferenceFile", HelpText = "A text file containing NET alignment reference data. Supported formats= .txt")]
        public string ReferenceTargetFile { get; set; }

        [Option("A", "AlignmentTargetsFile", HelpText = "A text file containing a list of IQ targets which are used in calibrating mass and elution time. Supported formats= .txt, .mgf")]
        public string TargetFileForAlignment { get; set; }

        [Option("D", "OutputDirectory", HelpText = "Output directory")]
        public string OutputDirectory { get; set; }

        [Option("Append", "AppendTargetName", HelpText = "Append the targets file name to the results file")]
        public bool AppendTargetsFileNameToResultFile { get; set; }

        [Option("UseInputScan", HelpText = "Determines whether or not the user-supplied LC scan is used during IQ processing.")]
        public bool UseInputScan { get; set; }

        [Option("W", "WorkflowParameterFile", ArgPosition = 2, HelpText = "An .xml file containing a list of IQ parameters")]
        public string WorkflowParameterFile { get; set; }

        [Option("F", "TemporaryWorkingDirectory", HelpText = "Temporary working directory where dataset is copied")]
        public string TemporaryWorkingDirectory { get; set; }

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

        public static string GetAppVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version + " (" + PROGRAM_DATE + ")";

            return version;
        }

        public void OutputSetOptions()
        {
            System.Console.WriteLine("IqConsole, version " + GetAppVersion());
            System.Console.WriteLine();
            System.Console.WriteLine("Using options:");
            System.Console.WriteLine();
            System.Console.WriteLine("Input file: " + InputFile);
            System.Console.WriteLine("Workflow parameter file: " + WorkflowParameterFile);

            if (!string.IsNullOrEmpty(OutputDirectory))
                System.Console.WriteLine("Output directory: " + OutputDirectory);

            System.Console.WriteLine();
            if (!string.IsNullOrEmpty(TargetsFile))
                System.Console.WriteLine("Targets file: " + TargetsFile);

            if (!string.IsNullOrEmpty(ReferenceTargetFile))
                System.Console.WriteLine("Reference targets file: " + ReferenceTargetFile);

            if (!string.IsNullOrEmpty(TargetFileForAlignment))
                System.Console.WriteLine("Target file for alignment: " + TargetFileForAlignment);

            System.Console.WriteLine();
            System.Console.WriteLine("Use input scan: " + UseInputScan);

            if (!string.IsNullOrEmpty(TemporaryWorkingDirectory))
                System.Console.WriteLine("Temporary working directory: " + TemporaryWorkingDirectory);

            System.Console.WriteLine();
            System.Console.WriteLine("IsAlignmentPerformed: " + IsAlignmentPerformed);

            if (string.IsNullOrEmpty(AlignmentParameterFile))
                System.Console.WriteLine("AlignmentParameterFile: " + AlignmentParameterFile);

            System.Console.WriteLine("IsMassAlignmentPerformed: " + IsMassAlignmentPerformed);
            System.Console.WriteLine("IsNetAlignmentPerformed: " + IsNetAlignmentPerformed);

            System.Console.WriteLine();
            System.Console.WriteLine("ChromGen Source Data PeakBr: {0}", StringUtilities.DblToString(ChromGenSourceDataPeakBr, 2));
            System.Console.WriteLine("ChromGen Source Data Signal/Noise: {0}", StringUtilities.DblToString(ChromGenSourceDataSigNoise, 2));

            if (DataIsThresholded)
            {
                System.Console.WriteLine("Assuming data is thresholded");
            }
            else
            {
                System.Console.WriteLine("Assuming data is not thresholded");
            }

            if (ChromGenSourceDataProcessMsMs)
                System.Console.WriteLine("Processing MS/MS spectra");

            if (UseOldIq)
                System.Console.WriteLine("Using Old IQ algorithm");

            if (OutputGraphs)
                System.Console.WriteLine("Generating .png images of mass spectra for all the results. See folder 'OutputGraphs' ");

        }

        public bool ValidateArgs()
        {
            if (string.IsNullOrWhiteSpace(InputFile))
            {
                ConsoleMsgUtils.ShowWarning("Input dataset not defined (alternatively, provide a text file with a list of datasets");
                return false;
            }

            if (string.IsNullOrWhiteSpace(WorkflowParameterFile) && string.IsNullOrWhiteSpace(TargetsFile))
            {
                ConsoleMsgUtils.ShowWarning("Either define a workflow executor parameter file with -w or define a Targets file with -t");
                return false;
            }


            return true;
        }
    }
}
