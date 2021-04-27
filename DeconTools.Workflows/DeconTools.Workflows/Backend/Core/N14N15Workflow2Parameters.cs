namespace DeconTools.Workflows.Backend.Core
{
    public class N14N15Workflow2Parameters : TargetedWorkflowParameters
    {
        public N14N15Workflow2Parameters()
        {
            ChromGeneratorMode = DeconTools.Backend.Globals.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;

            ChromNETToleranceN15 = 0.001; //very low since it is found in relation to the N14 chrom peak
            TargetedFeatureFinderIsotopicProfileTargetType = DeconTools.Backend.Globals.IsotopicProfileType.LABELED;
            TargetedFeatureFinderToleranceInPPM = MSToleranceInPPM;
            ResultType = DeconTools.Backend.Globals.ResultType.N14N15_TARGETED_RESULT;

            SmartChromSelectorPeakBR = 5;
            SmartChromSelectorPeakSigNoiseRatio = 3;
        }

        #region Properties

        public double ChromNETToleranceN15 { get; set; }
        public int NumPeaksUsedInQuant { get; set; }

        public DeconTools.Backend.Globals.IsotopicProfileType TargetedFeatureFinderIsotopicProfileTargetType { get; set; }
        public double TargetedFeatureFinderToleranceInPPM { get; set; }

        public double SmartChromSelectorPeakBR { get; set; }
        public double SmartChromSelectorPeakSigNoiseRatio { get; set; }

        #endregion

        //public override void LoadParameters(string xmlFilename)
        //{
        //    Check.Require(File.Exists(xmlFilename), "Workflow parameter file could not be loaded. File not found: " + xmlFilename);

        //    XDocument doc = XDocument.Load(xmlFilename);

        //    var query = doc.Element("WorkflowParameters").Elements();

        //    Dictionary<string, string> parameterTableFromXML = new Dictionary<string, string>();

        //    foreach (var item in query)
        //    {
        //        string paramName = item.Name.ToString();
        //        string paramValue = item.Value;

        //        if (!parameterTableFromXML.ContainsKey(paramName))
        //        {
        //            parameterTableFromXML.Add(paramName, paramValue);
        //        }

        //    }

        //    ChromGeneratorMode = StringToEnum<DeconTools.Backend.Globals.ChromatogramGeneratorMode>(parameterTableFromXML["ChromGeneratorMode"]);
        //    ChromNETTolerance = double.Parse(parameterTableFromXML["ChromNETTolerance"], CultureInfo.InvariantCulture);
        //    ChromPeakDetectorPeakBR = double.Parse(parameterTableFromXML["ChromPeakDetectorPeakBR"], CultureInfo.InvariantCulture);
        //    ChromPeakDetectorSigNoise = double.Parse(parameterTableFromXML["ChromPeakDetectorSigNoise"], CultureInfo.InvariantCulture);
        //    ChromSmootherNumPointsInSmooth = int.Parse(parameterTableFromXML["ChromSmootherNumPointsInSmooth"]);
        //    ChromGenTolerance = int.Parse(parameterTableFromXML["ChromGenTolerance"]);
        //    MSPeakDetectorPeakBR = double.Parse(parameterTableFromXML["MSPeakDetectorPeakBR"], CultureInfo.InvariantCulture);
        //    MSPeakDetectorSigNoise = double.Parse(parameterTableFromXML["MSPeakDetectorSigNoise"], CultureInfo.InvariantCulture);
        //    MSToleranceInPPM = double.Parse(parameterTableFromXML["MSToleranceInPPM"], CultureInfo.InvariantCulture);
        //    NumMSScansToSum = int.Parse(parameterTableFromXML["NumMSScansToSum"]);

        //    NumPeaksUsedInQuant = int.Parse(parameterTableFromXML["NumPeaksUsedInQuant"]);
        //    SmartChromSelectorPeakBR = double.Parse(parameterTableFromXML["SmartChromSelectorPeakBR"], CultureInfo.InvariantCulture);
        //    SmartChromSelectorPeakSigNoiseRatio =
        //        double.Parse(parameterTableFromXML["SmartChromSelectorPeakSigNoiseRatio"], CultureInfo.InvariantCulture);

        //    TargetedFeatureFinderIsotopicProfileTargetType =
        //        StringToEnum<DeconTools.Backend.Globals.IsotopicProfileType>(
        //            parameterTableFromXML["TargetedFeatureFinderIsotopicProfileTargetType"]);
        //    TargetedFeatureFinderToleranceInPPM =
        //        double.Parse(parameterTableFromXML["TargetedFeatureFinderToleranceInPPM"], CultureInfo.InvariantCulture);
        //}

        public override Globals.TargetedWorkflowTypes WorkflowType => Globals.TargetedWorkflowTypes.N14N15Targeted1;
    }
}
