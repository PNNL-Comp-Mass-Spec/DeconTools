using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Workflows.Backend.Core
{
    public class N14N15Workflow2Parameters : TargetedWorkflowParameters
    {


        public N14N15Workflow2Parameters()
        {
            this.ChromGeneratorMode = ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
            this.ChromNETTolerance = 0.025;
            this.ChromNETToleranceN15 = 0.001;  //very low since it is found in relation to the N14 chrom peak
            this.ChromPeakDetectorPeakBR = 2;
            this.ChromPeakDetectorSigNoise = 2;
            this.ChromSmootherNumPointsInSmooth = 15;
            this.ChromToleranceInPPM = 10;
            this.MSPeakDetectorPeakBR = 2;
            this.MSPeakDetectorSigNoise = 2;
            this.MSToleranceInPPM = 10;
            this.NumMSScansToSum = 1;
            this.NumPeaksUsedInQuant = 3;
            this.TargetedFeatureFinderIsotopicProfileTargetType = IsotopicProfileType.LABELLED;
            this.TargetedFeatureFinderToleranceInPPM = this.MSToleranceInPPM;
            this.ResultType = DeconTools.Backend.Globals.MassTagResultType.N14N15_MASSTAG_RESULT;

            this.SmartChromSelectorPeakBR = 5;
            this.SmartChromSelectorPeakSigNoiseRatio = 3;
        }

        #region Properties
    
        public double ChromNETToleranceN15 { get; set; }
        public int NumPeaksUsedInQuant { get; set; }
    
        public IsotopicProfileType TargetedFeatureFinderIsotopicProfileTargetType { get; set; }
        public double TargetedFeatureFinderToleranceInPPM { get; set; }

        public double SmartChromSelectorPeakBR { get; set; }
        public double SmartChromSelectorPeakSigNoiseRatio { get; set; }


        #endregion


        public override void LoadParameters(string xmlFilename)
        {
            Check.Require(File.Exists(xmlFilename), "Workflow parameter file could not be loaded. File not found.");

            XDocument doc = XDocument.Load(xmlFilename);

            var query = doc.Element("WorkflowParameters").Elements();


            Dictionary<string, string> parameterTableFromXML = new Dictionary<string, string>();

            foreach (var item in query)
            {
                string paramName = item.Name.ToString();
                string paramValue = item.Value;

                if (!parameterTableFromXML.ContainsKey(paramName))
                {
                    parameterTableFromXML.Add(paramName, paramValue);
                }

            }

            this.ChromGeneratorMode = StringToEnum<ChromatogramGeneratorMode>(parameterTableFromXML["ChromGeneratorMode"]);
            this.ChromNETTolerance = Convert.ToDouble(parameterTableFromXML["ChromNETTolerance"]);
            this.ChromPeakDetectorPeakBR = Convert.ToDouble(parameterTableFromXML["ChromPeakDetectorPeakBR"]);
            this.ChromPeakDetectorSigNoise = Convert.ToDouble(parameterTableFromXML["ChromPeakDetectorSigNoise"]);
            this.ChromSmootherNumPointsInSmooth = Convert.ToInt32(parameterTableFromXML["ChromSmootherNumPointsInSmooth"]);
            this.ChromToleranceInPPM = Convert.ToInt32(parameterTableFromXML["ChromToleranceInPPM"]);
            this.MSPeakDetectorPeakBR = Convert.ToDouble(parameterTableFromXML["MSPeakDetectorPeakBR"]);
            this.MSPeakDetectorSigNoise = Convert.ToDouble(parameterTableFromXML["MSPeakDetectorSigNoise"]);
            this.MSToleranceInPPM = Convert.ToDouble(parameterTableFromXML["MSToleranceInPPM"]);
            this.NumMSScansToSum = Convert.ToInt32(parameterTableFromXML["NumMSScansToSum"]);

            this.NumPeaksUsedInQuant = Convert.ToInt32(parameterTableFromXML["NumPeaksUsedInQuant"]);
            this.SmartChromSelectorPeakBR = Convert.ToDouble(parameterTableFromXML["SmartChromSelectorPeakBR"]);
            this.SmartChromSelectorPeakSigNoiseRatio = Convert.ToDouble(parameterTableFromXML["SmartChromSelectorPeakSigNoiseRatio"]);

            this.TargetedFeatureFinderIsotopicProfileTargetType = StringToEnum<IsotopicProfileType>(parameterTableFromXML["TargetedFeatureFinderIsotopicProfileTargetType"]);
            this.TargetedFeatureFinderToleranceInPPM = Convert.ToDouble(parameterTableFromXML["TargetedFeatureFinderToleranceInPPM"]);
        }

        public override Globals.TargetedWorkflowTypes WorkflowType
        {
            get { return  Globals.TargetedWorkflowTypes.N14N15Targeted1; }
        }



        
    }
}
