using System;
using System.ComponentModel;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.PeakListExporters;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.Workflows
{
    public class PeakDetectAndExportWorkflow
    {

        DeconToolsPeakDetectorV2 _ms1PeakDetector;
        DeconToolsPeakDetectorV2 _ms2PeakDetectorForCentroidedData;
        DeconToolsPeakDetectorV2 _ms2PeakDetectorForProfileData;

        private readonly BackgroundWorker backgroundWorker;
        private readonly PeakProgressInfo peakProgressInfo;

        PeakListTextExporter peakExporter;

        #region Constructors

        public PeakDetectAndExportWorkflow(Run run)
            : this(run, new PeakDetectAndExportWorkflowParameters())
        {
        }

        public PeakDetectAndExportWorkflow(Run run, PeakDetectAndExportWorkflowParameters parameters)
        {
            WorkflowParameters = parameters;
            Run = run;

            if (Run.MSFileType == Globals.MSFileType.PNNL_UIMF)
            {
                throw new NotSupportedException("PeakDetectAndExportWorkflow does not currently support UIMF files. NOTE to developer: need to add code for defining LC scans and IMS scans to iterate over.");

            }


        }



        public PeakDetectAndExportWorkflow(Run run, PeakDetectAndExportWorkflowParameters parameters, BackgroundWorker bw)
            : this(run, parameters)
        {
            backgroundWorker = bw;
            peakProgressInfo = new PeakProgressInfo();
        }



        #endregion

        protected MSGenerator MSGenerator { get; set; }
        protected Run Run { get; set; }


        public void InitializeWorkflow()
        {
            MSGenerator = MSGeneratorFactory.CreateMSGenerator(Run.MSFileType);

            _ms1PeakDetector = new DeconToolsPeakDetectorV2(WorkflowParameters.PeakBR, WorkflowParameters.SigNoiseThreshold,
                WorkflowParameters.PeakFitType, WorkflowParameters.IsDataThresholded);


            _ms2PeakDetectorForProfileData = new DeconToolsPeakDetectorV2(WorkflowParameters.MS2PeakDetectorPeakBR,
                                                                          WorkflowParameters.MS2PeakDetectorSigNoiseThreshold,
                                                                          WorkflowParameters.PeakFitType,
                                                                          WorkflowParameters.MS2PeakDetectorDataIsThresholded);


            _ms2PeakDetectorForCentroidedData =
                new DeconToolsPeakDetectorV2(0, 0, Globals.PeakFitType.QUADRATIC, true)
                {
                    RawDataType = Globals.RawDataType.Centroided
                };

            _ms2PeakDetectorForProfileData.PeaksAreStored = true;
            _ms2PeakDetectorForCentroidedData.PeaksAreStored = true;
            _ms1PeakDetector.PeaksAreStored = true;


        }

        public void Execute()
        {

            InitializeWorkflow();

            if (Run.MSFileType == Globals.MSFileType.PNNL_UIMF)
            {
                IMSScanSetCollection = CreateIMSScanSetCollection();
            }

            LcScanSetCollection = CreateLcScanSetCollection();


            PrepareOutputDirectory(WorkflowParameters.OutputDirectory);

            var outputPeaksFilePath = getOutputPeaksFilename();

            peakExporter = new PeakListTextExporter(Run.MSFileType, outputPeaksFilePath);

            var numTotalScans = LcScanSetCollection.ScanSetList.Count;
            var lastProgress = DateTime.UtcNow;

            using (var sw = new StreamWriter(new FileStream(outputPeaksFilePath, FileMode.Append, FileAccess.Write, FileShare.Read)))
            {


                if (Run.MSFileType == Globals.MSFileType.PNNL_UIMF && Run is UIMFRun uimfRun)
                {
                    var numTotalFrames = LcScanSetCollection.ScanSetList.Count;
                    var frameCounter = 0;

                    foreach (var frameSet in LcScanSetCollection.ScanSetList)
                    {
                        frameCounter++;
                        uimfRun.CurrentScanSet = frameSet;
                        uimfRun.ResultCollection.MSPeakResultList.Clear();

                        foreach (var scanSet in IMSScanSetCollection.ScanSetList)
                        {
                            uimfRun.CurrentIMSScanSet = (IMSScanSet)scanSet;
                            MSGenerator.Execute(uimfRun.ResultCollection);
                            _ms1PeakDetector.Execute(uimfRun.ResultCollection);

                        }
                        peakExporter.WriteOutPeaks(sw, uimfRun.ResultCollection.MSPeakResultList);

                        if (DateTime.UtcNow.Subtract(lastProgress).TotalSeconds >= 1 || frameCounter == numTotalFrames)
                        {
                            lastProgress = DateTime.UtcNow;
                            var percentProgress = frameCounter * 100 / (double)numTotalFrames;
                            reportProgress(percentProgress);
                        }

                    }

                }
                else
                {
                    var scanCounter = 0;
                    foreach (var scan in LcScanSetCollection.ScanSetList)
                    {
                        scanCounter++;

                        Run.CurrentScanSet = scan;

                        Run.ResultCollection.MSPeakResultList.Clear();

                        MSGenerator.Execute(Run.ResultCollection);
                        if (Run.GetMSLevel(scan.PrimaryScanNumber) == 1)
                        {
                            _ms1PeakDetector.Execute(Run.ResultCollection);
                        }
                        else
                        {
                            var dataIsCentroided = Run.IsDataCentroided(scan.PrimaryScanNumber);
                            if (dataIsCentroided)
                            {
                                _ms2PeakDetectorForCentroidedData.Execute(Run.ResultCollection);
                            }
                            else
                            {
                                _ms2PeakDetectorForProfileData.Execute(Run.ResultCollection);
                            }
                        }

                        peakExporter.WriteOutPeaks(sw, Run.ResultCollection.MSPeakResultList);

                        if (DateTime.UtcNow.Subtract(lastProgress).TotalSeconds >= 1 || scanCounter == numTotalScans)
                        {
                            lastProgress = DateTime.UtcNow;
                            var percentProgress = scanCounter * 100 / (double)numTotalScans;
                            reportProgress(percentProgress);
                        }

                    }
                }

            }

            Run.ResultCollection.MSPeakResultList.Clear();

        }

        protected ScanSetCollection LcScanSetCollection { get; set; }

        private ScanSetCollection CreateLcScanSetCollection()
        {
            var scanSetCollection = new ScanSetCollection();
            scanSetCollection.Create(Run, WorkflowParameters.LCScanMin, WorkflowParameters.LCScanMax,
                                     WorkflowParameters.Num_LC_TimePointsSummed, 1, WorkflowParameters.ProcessMSMS);

            return scanSetCollection;

        }

        private IMSScanSetCollection CreateIMSScanSetCollection()
        {
            throw new NotImplementedException();
        }

        protected IMSScanSetCollection IMSScanSetCollection { get; set; }

        private void PrepareOutputDirectory(string outputDirectoryPath)
        {
            if (string.IsNullOrEmpty(outputDirectoryPath))
            {
                return;
            }

            if (!Directory.Exists(outputDirectoryPath))
            {
                try
                {
                    Directory.CreateDirectory(outputDirectoryPath);
                }
                catch (Exception ex)
                {

                    throw new DirectoryNotFoundException("PeakExporter cannot create output directory.\n\nDetails: " + ex.Message, ex);

                }
            }

        }

        private void reportProgress(double percentProgress)
        {
            if (backgroundWorker != null)
            {
                peakProgressInfo.ProgressInfoString = "Creating Peaks File ";
                backgroundWorker.ReportProgress((int)percentProgress, peakProgressInfo);
            }
            else
            {
                Console.WriteLine("Peak creation progress: " + percentProgress.ToString("0.0") + "%");
            }
        }

        private string getOutputPeaksFilename()
        {
            string expectedPeaksFilename;

            if (string.IsNullOrEmpty(WorkflowParameters.OutputDirectory))
            {
                expectedPeaksFilename = Path.Combine(Run.DatasetDirectoryPath, Run.DatasetName + "_peaks.txt");
            }
            else
            {
                expectedPeaksFilename = Path.Combine(WorkflowParameters.OutputDirectory, Run.DatasetName + "_peaks.txt");
            }

            return expectedPeaksFilename;
        }

        public void InitializeRunRelatedTasks()
        {
            if (Run != null)
            {
                MSGenerator = MSGeneratorFactory.CreateMSGenerator(Run.MSFileType);
                int minLCScan;
                int maxLCScan;

                if (WorkflowParameters.LCScanMax == -1 || WorkflowParameters.LCScanMin == -1)
                {
                    if (Run is UIMFRun thisUimfRun)
                    {
                        minLCScan = thisUimfRun.MinLCScan;
                        maxLCScan = thisUimfRun.MaxLCScan;
                    }
                    else
                    {
                        minLCScan = Run.MinLCScan;
                        maxLCScan = Run.MaxLCScan;
                    }



                }
                else
                {
                    minLCScan = WorkflowParameters.LCScanMin;
                    maxLCScan = WorkflowParameters.LCScanMax;
                }

                if (Run.MSFileType == Globals.MSFileType.PNNL_UIMF && Run is UIMFRun uimfRun)
                {

                    uimfRun.ScanSetCollection.Create(uimfRun, minLCScan, maxLCScan,
                                                                       WorkflowParameters.Num_LC_TimePointsSummed, 1,
                                                                       WorkflowParameters.ProcessMSMS);


                    var sumAllIMSScans = WorkflowParameters.NumIMSScansSummed == -1 ||
                                         WorkflowParameters.NumIMSScansSummed > uimfRun.MaxLCScan;

                    if (sumAllIMSScans)
                    {
                        var primaryIMSScan = Run.MinLCScan;

                        uimfRun.IMSScanSetCollection.ScanSetList.Clear();
                        var imsScanSet = new IMSScanSet(primaryIMSScan, uimfRun.MinIMSScan, uimfRun.MaxIMSScan);
                        uimfRun.IMSScanSetCollection.ScanSetList.Add(imsScanSet);
                    }
                    else
                    {
                        uimfRun.IMSScanSetCollection.Create(Run, uimfRun.MinIMSScan, uimfRun.MaxIMSScan,
                                                                         WorkflowParameters.NumIMSScansSummed, 1);
                    }



                }
                else
                {
                    Run.ScanSetCollection.Create(Run, minLCScan, maxLCScan,
                   WorkflowParameters.Num_LC_TimePointsSummed, 1, WorkflowParameters.ProcessMSMS);

                }


            }
        }


        public PeakDetectAndExportWorkflowParameters WorkflowParameters { get; set; }
    }
}
