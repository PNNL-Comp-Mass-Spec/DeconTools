using System;
using System.ComponentModel;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.PeakListExporters;
using DeconTools.Backend.Runs;

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

            _peakDetector.PeaksAreStored = true;


        }

        public override void Execute()
        {

            string outputPeaksFileName = getOutputPeaksFilename();

            peakExporter = new PeakListTextExporter(Run.MSFileType, outputPeaksFileName);

            int numTotalScans = Run.ScanSetCollection.ScanSetList.Count;
            int scanCounter = 0;

            if (Run.MSFileType==DeconTools.Backend.Globals.MSFileType.PNNL_UIMF)
            {
                var uimfrun = Run as UIMFRun;

                int numTotalFrames = uimfrun.FrameSetCollection.FrameSetList.Count;
                int frameCounter = 0;

                foreach (var frameSet in uimfrun.FrameSetCollection.FrameSetList)
                {
                    frameCounter++;
                    uimfrun.CurrentFrameSet = frameSet;
                    uimfrun.ResultCollection.MSPeakResultList.Clear();

                    foreach (var scanSet in Run.ScanSetCollection.ScanSetList)
                    {
                        Run.CurrentScanSet = scanSet;
                        MSGenerator.Execute(Run.ResultCollection);
                        this._peakDetector.Execute(Run.ResultCollection);
                        
                    }
                    peakExporter.WriteOutPeaks(Run.ResultCollection.MSPeakResultList);

                    if (frameCounter % 5 == 0 || scanCounter == numTotalFrames)
                    {
                        double percentProgress = frameCounter * 100 / numTotalFrames;
                        reportProgress(percentProgress);
                    }

                }

            }
            else
            {
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


            
           

            Run.ResultCollection.MSPeakResultList.Clear();

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
                int minLCScan;
                int maxLCScan;

                if (this._workflowParameters.LCScanMax == -1 || this._workflowParameters.LCScanMin == -1)
                {
                    if (Run is UIMFRun)
                    {
                        minLCScan = ((UIMFRun)Run).MinFrame;
                        maxLCScan = ((UIMFRun)Run).MaxFrame;
                    }
                    else
                    {
                        minLCScan = Run.MinScan;
                        maxLCScan = Run.MaxScan;
                    }



                }
                else
                {
                    minLCScan = this._workflowParameters.LCScanMin;
                    maxLCScan = this._workflowParameters.LCScanMax;
                }

                if (Run.MSFileType == DeconTools.Backend.Globals.MSFileType.PNNL_UIMF)
                {
                    var uimfRun = Run as UIMFRun;

                    uimfRun.FrameSetCollection = FrameSetCollection.Create(uimfRun, minLCScan, maxLCScan,
                                                                       _workflowParameters.Num_LC_TimePointsSummed, 1,
                                                                       _workflowParameters.ProcessMSMS);


                    bool sumAllScans = (_workflowParameters.NumIMSScansSummed == -1 ||
                                        _workflowParameters.NumIMSScansSummed > uimfRun.MaxScan);

                    if (sumAllScans)
                    {
                        int primaryIMSScan = Run.MinScan;

                        uimfRun.ScanSetCollection.ScanSetList.Clear();
                        var scanset = new ScanSet(primaryIMSScan, Run.MinScan, Run.MaxScan);
                        uimfRun.ScanSetCollection.ScanSetList.Add(scanset);
                    }
                    else
                    {
                        Run.ScanSetCollection = ScanSetCollection.Create(Run, Run.MinScan, Run.MaxScan,
                                                                         _workflowParameters.NumIMSScansSummed, 1, false);
                    }



                }
                else
                {
                    Run.ScanSetCollection = ScanSetCollection.Create(Run, minLCScan, maxLCScan,
                   this._workflowParameters.Num_LC_TimePointsSummed, 1, this._workflowParameters.ProcessMSMS);

                }


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
