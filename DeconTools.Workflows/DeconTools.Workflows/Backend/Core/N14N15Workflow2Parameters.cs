using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Utilities;

namespace DeconTools.Workflows.Backend.Core
{
    public class N14N15Workflow2Parameters : TargetedWorkflowParameters
    {


        public N14N15Workflow2Parameters()
        {
            ChromGeneratorMode = ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;

            ChromNETToleranceN15 = 0.001; //very low since it is found in relation to the N14 chrom peak
            TargetedFeatureFinderIsotopicProfileTargetType = IsotopicProfileType.LABELLED;
            TargetedFeatureFinderToleranceInPPM = MSToleranceInPPM;
            ResultType = DeconTools.Backend.Globals.ResultType.N14N15_TARGETED_RESULT;

            
            SmartChromSelectorPeakBR = 5;
            SmartChromSelectorPeakSigNoiseRatio = 3;
 
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

            ChromGeneratorMode = StringToEnum<ChromatogramGeneratorMode>(parameterTableFromXML["ChromGeneratorMode"]);
            ChromNETTolerance = Convert.ToDouble(parameterTableFromXML["ChromNETTolerance"]);
            ChromPeakDetectorPeakBR = Convert.ToDouble(parameterTableFromXML["ChromPeakDetectorPeakBR"]);
            ChromPeakDetectorSigNoise = Convert.ToDouble(parameterTableFromXML["ChromPeakDetectorSigNoise"]);
            ChromSmootherNumPointsInSmooth = Convert.ToInt32(parameterTableFromXML["ChromSmootherNumPointsInSmooth"]);
            ChromToleranceInPPM = Convert.ToInt32(parameterTableFromXML["ChromToleranceInPPM"]);
            MSPeakDetectorPeakBR = Convert.ToDouble(parameterTableFromXML["MSPeakDetectorPeakBR"]);
            MSPeakDetectorSigNoise = Convert.ToDouble(parameterTableFromXML["MSPeakDetectorSigNoise"]);
            MSToleranceInPPM = Convert.ToDouble(parameterTableFromXML["MSToleranceInPPM"]);
            NumMSScansToSum = Convert.ToInt32(parameterTableFromXML["NumMSScansToSum"]);

            NumPeaksUsedInQuant = Convert.ToInt32(parameterTableFromXML["NumPeaksUsedInQuant"]);
            SmartChromSelectorPeakBR = Convert.ToDouble(parameterTableFromXML["SmartChromSelectorPeakBR"]);
            SmartChromSelectorPeakSigNoiseRatio =
                Convert.ToDouble(parameterTableFromXML["SmartChromSelectorPeakSigNoiseRatio"]);

            TargetedFeatureFinderIsotopicProfileTargetType =
                StringToEnum<IsotopicProfileType>(
                    parameterTableFromXML["TargetedFeatureFinderIsotopicProfileTargetType"]);
            TargetedFeatureFinderToleranceInPPM =
                Convert.ToDouble(parameterTableFromXML["TargetedFeatureFinderToleranceInPPM"]);
        }

        public override Globals.TargetedWorkflowTypes WorkflowType
        {
            get { return  Globals.TargetedWorkflowTypes.N14N15Targeted1; }
        }



        
    }
}
