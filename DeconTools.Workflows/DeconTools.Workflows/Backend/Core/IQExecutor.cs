using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Workflows.Backend.FileIO;

namespace DeconTools.Workflows.Backend.Core
{
    public class IqExecutor
    {
        private BackgroundWorker _backgroundWorker;

        #region Constructors

        public IqExecutor()
        {
            Parameters = new BasicTargetedWorkflowExecutorParameters();
            Results = new List<IqResult>();
            ResultExporter = new IqLabelFreeResultExporter();
            IsDataExported = true;

        }

        public IqExecutor(WorkflowExecutorBaseParameters parameters)
            : this()
        {
            ResultExporter = new IqLabelFreeResultExporter();
            Parameters = parameters;
            IsDataExported = true;
        }

        protected WorkflowExecutorBaseParameters Parameters { get; set; }

        protected bool IsDataExported { get; set; }

        #endregion

        #region Properties




        #endregion


        #region Public Methods


        public string ChromSourceDataFilePath { get; set; }

        public List<IqResult> Results { get; set; }

        protected ResultExporter ResultExporter { get; set; }

        public void Execute(IEnumerable<IqTarget> targets)
        {

            foreach (var target in targets)
            {
                Run = target.GetRun();

                if (!ChromDataIsLoaded)
                {
                    LoadChromData(Run);
                }

                if (target.HasChildren())
                {
                    Execute(target.ChildTargets());
                }

                var result = target.DoWorkflow();
                Results.Add(result);


                if (!target.HasParent)
                {
                    if (IsDataExported)
                    {
                        ExportResults();
                        Results.Clear();
                    }
                    else
                    {

                    }

                    result.IqResultDetail.Dispose();



                }

            }

        }


        protected void ExportResults()
        {
            var orderedResults = Results.OrderBy(p => p.Target.ChargeState).ToList();

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
