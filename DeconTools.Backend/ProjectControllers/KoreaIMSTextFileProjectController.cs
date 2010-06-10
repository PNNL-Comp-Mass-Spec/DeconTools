using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using System.ComponentModel;
using DeconTools.Utilities;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.ZeroFillers;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.Data;

namespace DeconTools.Backend.ProjectControllers
{
    public class KoreaIMSTextFileProjectController : ProjectController
    {
        private string m_paramFilename;
        private DeconTools.Backend.Globals.MSFileType m_fileType;
        private BackgroundWorker m_backgroundWorker;

        #region Constructors
        public KoreaIMSTextFileProjectController(string inputFileName, DeconTools.Backend.Globals.MSFileType fileType, string paramFileName, BackgroundWorker bw)
        {
            this.m_fileType = fileType;
            this.m_paramFilename = paramFileName;
            this.m_backgroundWorker = bw;

            Project.Reset();

            Project.getInstance().LoadOldDecon2LSParameters(this.m_paramFilename);

            this.ExporterType = getExporterTypeFromOldParameters(Project.getInstance().Parameters.OldDecon2LSParameters);


            Run run = new MSScanFromTextFileRun(inputFileName,'\t', 1, 2);
            Check.Assert(run != null, "Processing aborted. Could not handle supplied File(s)");
            Project.getInstance().RunCollection.Add(run);

            ScanSetCollectionCreator scanSetCollectionCreator = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan,
                Project.getInstance().Parameters.NumScansSummed,
                Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.NumScansToAdvance, false);
            scanSetCollectionCreator.Create();

            MSGeneratorFactory msGeneratorFactory = new MSGeneratorFactory();
            Task msGen = msGeneratorFactory.CreateMSGenerator(fileType, Project.getInstance().Parameters.OldDecon2LSParameters);
            Project.getInstance().TaskCollection.TaskList.Add(msGen);

            if (Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.ZeroFill)
            {
                Task zeroFiller = new DeconToolsZeroFiller(Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.NumZerosToFill);
                Project.getInstance().TaskCollection.TaskList.Add(zeroFiller);
            }
            if (Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.UseSavitzkyGolaySmooth)
            {
                Task smoother = new DeconToolsSavitzkyGolaySmoother(
                    Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.SGNumLeft,
                    Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.SGNumRight,
                    Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.SGOrder);
                Project.getInstance().TaskCollection.TaskList.Add(smoother);
            }

            Task peakDetector = new DeconToolsPeakDetector(Project.getInstance().Parameters.OldDecon2LSParameters.PeakProcessorParameters);
            Project.getInstance().TaskCollection.TaskList.Add(peakDetector);


            if (Project.getInstance().Parameters.OldDecon2LSParameters.PeakProcessorParameters.WritePeaksToTextFile == true)
            {
                DeconTools.Backend.ProcessingTasks.ResultExporters.PeakListExporters.PeakListExporterFactory peakexporterFactory = new DeconTools.Backend.ProcessingTasks.ResultExporters.PeakListExporters.PeakListExporterFactory();
                Task peakListTextExporter = peakexporterFactory.Create(this.ExporterType, this.m_fileType, 50000, getPeakListFileName(this.ExporterType));
                Project.getInstance().TaskCollection.TaskList.Add(peakListTextExporter);
            }

        }

        private string getPeakListFileName(Globals.ExporterType exporterType)
        {
            var run = Project.getInstance().RunCollection[0];

            string baseFileName = run.DataSetPath + "\\" + run.DatasetName;

            //string baseFileName = Project.getInstance().RunCollection[0].Filename.Substring(0, Project.getInstance().RunCollection[0].Filename.LastIndexOf('.'));
            switch (exporterType)
            {
                case Globals.ExporterType.TEXT:
                    return baseFileName += "_peaks.txt";
                case Globals.ExporterType.SQLite:
                    return baseFileName += "_peaks.sqlite";
                default:
                    return baseFileName += "_peaks.txt";
            }
        }
        #endregion

        #region Properties
        public Globals.ExporterType ExporterType { get; set; }

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        public override void Execute()
        {
            //Logger.Instance.AddEntry("DeconTools.Backend.dll version = " + AssemblyInfoRetriever.GetVersion(typeof(OldSchoolProcRunner)));
            //Logger.Instance.AddEntry("DeconEngine version = " + AssemblyInfoRetriever.GetVersion(typeof(DeconToolsV2.HornTransform.clsHornTransformParameters)));
            //Logger.Instance.AddEntry("Deconvolution_Algorithm = " + Project.getInstance().TaskCollection.GetDeconvolutorType());
            //Logger.Instance.AddEntry("Started file processing");

            TaskControllerFactory taskControllerFactory = new TaskControllerFactory(this.m_backgroundWorker);
            TaskController controller = taskControllerFactory.CreateTaskController(m_fileType, Project.getInstance().TaskCollection);
            controller.IsosResultThresholdNum = 50000;
            controller.Execute(Project.getInstance().RunCollection);

            //Logger.Instance.AddEntry("Finished file processing");


            TaskCleaner cleaner = new TaskCleaner(Project.getInstance().TaskCollection);
            cleaner.CleanTasks();
            //Logger.Instance.AddEntry("Closed output files");
        }
    }
}
