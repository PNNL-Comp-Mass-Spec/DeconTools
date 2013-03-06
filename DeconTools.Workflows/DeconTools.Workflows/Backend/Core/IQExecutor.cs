using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.Runs;
using DeconTools.Workflows.Backend.FileIO;

namespace DeconTools.Workflows.Backend.Core
{
    public class IqExecutor
    {
        private BackgroundWorker _backgroundWorker;

        private readonly IqResultUtilities _iqResultUtilities = new IqResultUtilities();
        private readonly IqTargetUtilities _targetUtilities = new IqTargetUtilities();


        private RunFactory _runFactory = new RunFactory();


        private List<IqWorkflow> _iqWorkflows = new List<IqWorkflow>(); 



        #region Constructors

        public IqExecutor()
        {
            Parameters = new BasicTargetedWorkflowExecutorParameters();
            Results = new List<IqResult>();
            ResultExporter = new IqLabelFreeResultExporter();
            IsDataExported = true;
            DisposeResultDetails = true;


        }

        public IqExecutor(WorkflowExecutorBaseParameters parameters)
            : this()
        {
            Parameters = parameters;
        }

        protected WorkflowExecutorBaseParameters Parameters { get; set; }

        protected bool IsDataExported { get; set; }

        #endregion

        #region Properties

        public bool DisposeResultDetails { get; set; }


        public IqTargetImporter TargetImporter { get; set; }



        #endregion


        #region Public Methods


        public string ChromSourceDataFilePath { get; set; }

        public List<IqResult> Results { get; set; }

        public List<IqTarget> Targets { get; set; } 


        protected ResultExporter ResultExporter { get; set; }

        public void Execute()
        {
            Execute(Targets);
        }



        public void SetRun(string datasetPath)
        {
            Run = _runFactory.CreateRun(datasetPath);
        }


        public void SetRun(Run run)
        {
            Run = run;
        }

        
        public void AddIqWorkflow(IqWorkflow workflow)
        {
            _iqWorkflows.Add(workflow);

        }



        public void Execute(IEnumerable<IqTarget> targets)
        {
            foreach (var target in targets)
            {
                Run = target.GetRun();

                if (!ChromDataIsLoaded)
                {
                    LoadChromData(Run);
                }
               
                target.DoWorkflow();
                var result= target.GetResult();

                if (IsDataExported)
                {
                    ExportResults(result);
                }

                Results.Add(result);

                if (DisposeResultDetails)
                {
                    result.Dispose(); 
                }
            }

        }




        public virtual void InitializeTargets()
        {
            if (TargetImporter ==null)
            {
                TargetImporter = new BasicIqTargetImporter(this.Parameters.TargetsFilePath);
            }

            Targets=  TargetImporter.Import();

            _targetUtilities.CreateChildTargets(Targets);
        }



        public virtual void InitializeWorkflows()
        {
            InitializeWorkflows(_iqWorkflows);


        }

        protected TargetedWorkflowParameters IqWorkflowParameters { get; set; }


        public virtual void InitializeWorkflows(List<IqWorkflow>workflowList)
        {
            if (Targets==null || !Targets.Any())
            {
                throw new InvalidOperationException(
                    "Failed to initialize workflow. Reason: Targets need to be loaded first so that workflows can be associated with targets");
            }

            //int totalNodeLevels = _targetUtilities.GetTotalNodelLevels(Targets.First());

            for (int nodeLevel = 0; nodeLevel < workflowList.Count; nodeLevel++)
            {
                var targetsAtGivenNodeLevel = _targetUtilities.GetTargetsFromNodelLevel(Targets, nodeLevel);


                var workflowAtGivenNode = workflowList[nodeLevel];

                foreach (var iqTarget in targetsAtGivenNodeLevel)
                {
                    workflowAtGivenNode.Run = Run;
                    iqTarget.SetWorkflow(workflowAtGivenNode);
                }


            }


        }



        protected virtual void ExportResults(IqResult iqResult)
        {
            List<IqResult> resultsForExport =_iqResultUtilities.FlattenOutResultTree(iqResult);

            var orderedResults = resultsForExport.OrderBy(p => p.Target.ChargeState).ToList();

            ResultExporter.WriteOutResults(Parameters.ResultsFolder + Path.DirectorySeparatorChar + Run.DatasetName + "_iqResults.txt", orderedResults);
        }

  

        private string CreatePeaksForChromSourceData()
        {
            var parameters = new PeakDetectAndExportWorkflowParameters();

            parameters.PeakBR = Parameters.ChromGenSourceDataPeakBR;
            parameters.PeakFitType = DeconTools.Backend.Globals.PeakFitType.QUADRATIC;
            parameters.SigNoiseThreshold = Parameters.ChromGenSourceDataSigNoise;
            parameters.ProcessMSMS = Parameters.ChromGenSourceDataProcessMsMs;
            parameters.IsDataThresholded = Parameters.ChromGenSourceDataIsThresholded;

            var peakCreator = new PeakDetectAndExportWorkflow(this.Run, parameters, _backgroundWorker);
            peakCreator.Execute();

            var peaksFilename = this.Run.DataSetPath + "\\" + this.Run.DatasetName + "_peaks.txt";
            return peaksFilename;

        }


        private string GetPossiblePeaksFile()
        {
            string baseFileName;
            baseFileName = this.Run.DataSetPath + "\\" + this.Run.DatasetName;

            string possibleFilename1 = baseFileName + "_peaks.txt";

            if (File.Exists(possibleFilename1))
            {
                return possibleFilename1;
            }
            else
            {
                return string.Empty;
            }
        }



        private void LoadChromData(Run run)
        {
            if (string.IsNullOrEmpty(ChromSourceDataFilePath))
            {
                ChromSourceDataFilePath = GetPossiblePeaksFile();
            }

            if (string.IsNullOrEmpty(ChromSourceDataFilePath))
            {
                //ReportGeneralProgress("Creating _Peaks.txt file for extracted ion chromatogram (XIC) source data ... takes 1-5 minutes");
                ChromSourceDataFilePath = CreatePeaksForChromSourceData();

            }
            else
            {
                //ReportGeneralProgress("Using existing _Peaks.txt file");
            }

            //ReportGeneralProgress("Peak loading started...");

            PeakImporterFromText peakImporter = new PeakImporterFromText(ChromSourceDataFilePath, _backgroundWorker);
            peakImporter.ImportPeaks(this.Run.ResultCollection.MSPeakResultList);


        }

        protected bool ChromDataIsLoaded
        {
            get
            {
                if (Run != null)
                {
                    return Run.ResultCollection.MSPeakResultList.Count > 0;
                }

                return false;
            }
        }

        protected bool RunIsInitialized
        {
            get { throw new NotImplementedException(); }
        }

        protected Run Run
        {
            get;
            set;
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
