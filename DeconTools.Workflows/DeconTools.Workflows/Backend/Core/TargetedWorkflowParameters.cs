using System;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;

namespace DeconTools.Workflows.Backend.Core
{
    public abstract class TargetedWorkflowParameters : WorkflowParameters
    {

        #region Constructors
        protected TargetedWorkflowParameters()
        {
            ChromGeneratorMode = DeconTools.Backend.Globals.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
            ChromNETTolerance = 0.025;
            ChromPeakDetectorPeakBR = 1;
            ChromPeakDetectorSigNoise = 1;
            ChromSmootherNumPointsInSmooth = 9;
            ChromToleranceInPPM = 10;
            MSPeakDetectorPeakBR = 2;
            MSPeakDetectorSigNoise = 2;
            MSToleranceInPPM = 10;
            NumMSScansToSum = 1;
            NumChromPeaksAllowedDuringSelection = 20;

            MultipleHighQualityMatchesAreAllowed = true;
            ChromGenSourceDataPeakBR = 2;
            ChromGenSourceDataSigNoise = 3;

            ChromPeakSelectorMode = DeconTools.Backend.Globals.PeakSelectorMode.Smart;
            SummingMode = SummingModeEnum.SUMMINGMODE_STATIC;
            AreaOfPeakToSumInDynamicSumming = 2;
            MaxScansSummedInDynamicSumming = 100;

            ChromatogramCorrelationIsPerformed = false;


        }

        
        #endregion

        #region Properties
        public int ChromToleranceInPPM { get; set; }
        public double ChromPeakDetectorPeakBR { get; set; }
        public double ChromPeakDetectorSigNoise { get; set; }
        public int ChromSmootherNumPointsInSmooth { get; set; }
        public double ChromNETTolerance { get; set; }
        public int NumMSScansToSum { get; set; }
        public double MSPeakDetectorPeakBR { get; set; }
        public double MSPeakDetectorSigNoise { get; set; }
        public double MSToleranceInPPM { get; set; }
        public DeconTools.Backend.Globals.ChromatogramGeneratorMode ChromGeneratorMode { get; set; }
        public DeconTools.Backend.Globals.ResultType ResultType { get; set; }
        public bool ChromatogramCorrelationIsPerformed { get; set; }
        

        
        /// <summary>
        /// 
        /// </summary>
        public DeconTools.Backend.Globals.PeakSelectorMode ChromPeakSelectorMode { get; set; }




        /// <summary>
        /// The MS peak detection PeakBR that is used in forming the _peaks.txt file that 
        /// contains the peak-level information on which extracted ion chromatograms are based
        /// </summary>
        public double ChromGenSourceDataPeakBR { get; set; }
        
        
        /// <summary>
        /// The MS peak detection SigNoise threshold that is used in forming the _peaks.txt
        /// file that contains the peak-level information on which extracted ion chromatograms
        /// are based
        /// </summary>
        public double ChromGenSourceDataSigNoise { get; set; }


        /// <summary>
        /// Number of chrom peaks allowed. For example if this is set to '5' 
        /// and 6 peaks were found within the tolerance, then the selected best peak is set to
        /// null indicating a failed execution
        /// </summary>
        public int NumChromPeaksAllowedDuringSelection { get; set; }

        /// <summary>
        /// If true, and if there are multiple high quality Features found for a given mass tag, will select the most abundant targeted Feature found within the given tolerances
        /// If false, and if there are multiple high quality Features, will reject them all and report no Feature found. This is a way of being very strict. Good for targeted Alignment
        /// Default = true;
        /// 
        /// </summary>
        public bool MultipleHighQualityMatchesAreAllowed { get; set; }

        /// <summary>
        /// Static or dynamic summing across an elution peak. E.g. in static mode the same number of scans are summed. 
        /// In dynamic, the number of scans summed depends on the width of the peak
        /// </summary>
        public SummingModeEnum SummingMode { get; set; }

        /// <summary>
        /// In dynamic summing we sum across a certain peak width. The setting guides how much of the peak to sum across. The unit is 'variance' or sigma,
        /// such that a value of 2 means 2 sigma, which means the peak is summed across 4 sigma, or ~ 95%. (Following a normal distribution).
        /// </summary>
        public double AreaOfPeakToSumInDynamicSumming { get; set; }

        /// <summary>
        /// In dynamic summing we sum across an elution peak. Some peaks are huge and we might not want to sum that much. So we can use this value to set the max. 
        /// 
        /// </summary>
        public int MaxScansSummedInDynamicSumming { get; set; }

        #endregion


        //public override string WorkflowType
        //{
        //    get { throw new NotImplementedException(); }
        //}




        //public override void LoadParameters(string xmlFilename)
        //{
        //    Check.Require(File.Exists(xmlFilename), "Workflow parameter file could not be loaded. File not found.");

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

        //    this.ChromGeneratorMode = StringToEnum<ChromatogramGeneratorMode>(parameterTableFromXML["ChromGeneratorMode"]);
        //    this.ChromNETTolerance = Convert.ToDouble(parameterTableFromXML["ChromNETTolerance"]);
        //    this.ChromPeakDetectorPeakBR = Convert.ToDouble(parameterTableFromXML["ChromPeakDetectorPeakBR"]);
        //    this.ChromPeakDetectorSigNoise = Convert.ToDouble(parameterTableFromXML["ChromPeakDetectorSigNoise"]);
        //    this.ChromSmootherNumPointsInSmooth = Convert.ToInt32(parameterTableFromXML["ChromSmootherNumPointsInSmooth"]);
        //    this.ChromToleranceInPPM = Convert.ToInt32(parameterTableFromXML["ChromToleranceInPPM"]);
        //    this.MSPeakDetectorPeakBR = Convert.ToDouble(parameterTableFromXML["MSPeakDetectorPeakBR"]);
        //    this.MSPeakDetectorSigNoise = Convert.ToDouble(parameterTableFromXML["MSPeakDetectorSigNoise"]);
        //    this.MSToleranceInPPM = Convert.ToDouble(parameterTableFromXML["MSToleranceInPPM"]);
        //    this.NumMSScansToSum = Convert.ToInt32(parameterTableFromXML["NumMSScansToSum"]);
        //    this.NumChromPeaksAllowedDuringSelection = Convert.ToInt32(parameterTableFromXML["NumChromPeaksAllowedDuringSelection"]);
        //}


        protected T StringToEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }

        
    }
}
