using System;
using System.ComponentModel;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.PeakListExporters;
using DeconTools.Backend.Utilities;

namespace DeconTools.Workflows.Backend.Core
{
    public class PeakDetectAndExportWorkflow : WorkflowBase
    {

        DeconToolsPeakDetector _peakDetector;
        PeakDetectAndExportWorkflowParameters _workflowParameters;
        private BackgroundWorker backgroundWorker;

        PeakListTextExporter peakExporter;

        #region Constructors

        public PeakDetectAndExportWorkflow(Run run)
            : this(run, new PeakDetectAndExportWorkflowParameters())
        {

        }

        public PeakDetectAndExportWorkflow(Run run, PeakDetectAndExportWorkflowParameters parameters)
        {
            this.WorkflowParameters = parameters;
            this.Run = run;

            InitializeWorkflow();

        }

        public PeakDetectAndExportWorkflow(Run run, PeakDetectAndExportWorkflowParameters parameters, BackgroundWorker bw)
            : this(run, parameters)
        {
            this.backgroundWorker = bw;
        }



        #endregion

        #region Properties


        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        public override void InitializeWorkflow()
        {
            _peakDetector = new DeconToolsPeakDetector(this._workflowParameters.PeakBR, this._workflowParameters.SigNoiseThreshold,
                this._workflowParameters.PeakFitType, this._workflowParameters.IsDataThresholded);

            _peakDetector.StorePeakData = true;


        }

        public override void Execute()
        {

            string outputPeaksFileName = getOutputPeaksFilename();

            peakExporter = new PeakListTextExporter(Run.MSFileType, outputPeaksFileName);

            int numTotalScans = Run.ScanSetCollection.ScanSetList.Count;

            int scanCounter = 0;
            foreach (var scan in Run.ScanSetCollection.ScanSetList)
            {
                scanCounter++;

                Run.CurrentScanSet = scan;

                Run.ResultCollection.MSPeakResultList.Clear();

                MSGenerator.Execute(Run.ResultCollection);
                this._peakDetector.Execute(Run.ResultCollection);

                peakExporter.WriteOutPeaks(Run.ResultCollection.MSPeakResultList);

                if (scanCounter % 50 == 0 || scanCounter == numTotalScans)
                {
                    double percentProgress = scanCounter * 100 / numTotalScans;
                    reportProgress(percentProgress);
                }

            }


        }

        private void reportProgress(double percentProgress)
        {
            if (backgroundWorker != null)
            {
                backgroundWorker.ReportProgress((int)percentProgress);
            }
        }

        private string getOutputPeaksFilename()
        {
            string expectedPeaksFilename;

            if (this._workflowParameters.OutputFolder == String.Empty)
            {
                expectedPeaksFilename = Run.DataSetPath + Path.DirectorySeparatorChar + Run.DatasetName + "_peaks.txt";
            }
            else
            {
                expectedPeaksFilename = _workflowParameters.OutputFolder + Path.DirectorySeparatorChar + Run.DatasetName + "_peaks.txt";
            }

            return expectedPeaksFilename;
        }

        public override void InitializeRunRelatedTasks()
        {
            if (Run != null)
            {
                MSGenerator = MSGeneratorFactory.CreateMSGenerator(Run.MSFileType);


                int minScan;
                int maxScan;

                if (this._workflowParameters.ScanMax == -1 || this._workflowParameters.ScanMin == -1)
                {
                    minScan = Run.MinScan;
                    maxScan = Run.MaxScan;

                }
                else
                {
                    minScan = this._workflowParameters.ScanMin;
                    maxScan = this._workflowParameters.ScanMax;
                }

                Run.ScanSetCollection = ScanSetCollection.Create(Run, minScan, maxScan,
                    this._workflowParameters.Num_LC_TimePointsSummed, 1, this._workflowParameters.ProcessMSMS);
                
            }
        }


        public override WorkflowParameters WorkflowParameters
        {
            get
            {
                return _workflowParameters;
            }
            set
            {
                _workflowParameters = value as PeakDetectAndExportWorkflowParameters;
            }
        }
    }
}
