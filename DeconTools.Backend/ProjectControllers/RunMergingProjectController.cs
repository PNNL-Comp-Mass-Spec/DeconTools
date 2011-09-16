using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using DeconTools.Backend.Data;
using DeconTools.Utilities;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.ZeroFillers;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.Deconvoluters;
using System.IO;
using DeconTools.Backend.ProcessingTasks.PeakListExporters;
using DeconTools.Backend.ProcessingTasks.IsosMergerExporters;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.Core
{
    public class RunMergingProjectController : ProjectController
    {
        private List<string> inputDataFilenames;
        private string paramFilename;
        private DeconTools.Backend.Globals.MSFileType fileType;
        private BackgroundWorker backgroundWorker;

        #region Properties

        private int isosResultThreshold;

        public int IsosResultThreshold
        {
            get { return isosResultThreshold; }
            set { isosResultThreshold = value; }
        }

        #endregion
        public RunMergingProjectController(List<string> inputDataFilenames, DeconTools.Backend.Globals.MSFileType fileType, string paramFileName)
        {
            this.inputDataFilenames = inputDataFilenames;
            this.fileType = fileType;
            this.paramFilename = paramFileName;

            Project.Reset();
            Project.getInstance().LoadOldDecon2LSParameters(this.paramFilename);
            this.IsosResultThreshold = 100000;       // results will be serialized if count is greater than this number

            RunFactory runfactory = new RunFactory();


            //Create runs and create ScanSets
            foreach (string filename in this.inputDataFilenames)
            {
                Run run;
                run = runfactory.CreateRun(fileType, filename, Project.getInstance().Parameters.OldDecon2LSParameters);
                Check.Assert(run != null, "Processing aborted. Could not handle supplied File(s)");
                Project.getInstance().RunCollection.Add(run);

                ScanSetCollectionCreator scanSetCollectionCreator = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan,
                    Project.getInstance().Parameters.NumScansSummed,
                    Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.NumScansToAdvance, false);
                scanSetCollectionCreator.Create();
            }

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


            //DeconvolutorFactory deconFactory = new DeconvolutorFactory();
            //Task deconvolutor = deconFactory.CreateDeconvolutor(project.Parameters.OldDecon2LSParameters);
            Task deconvolutor = new SimpleDecon(0.0005);
            Project.getInstance().TaskCollection.TaskList.Add(deconvolutor);

            Task isosmergerExporter = new BasicIsosMergerExporter(getIsosOutputFilename());
            Project.getInstance().TaskCollection.TaskList.Add(isosmergerExporter);

            Task scanResultUpdater = new ScanResultUpdater();
            Project.getInstance().TaskCollection.TaskList.Add(scanResultUpdater);

            Task scanmergerExporter = new BasicScansMergerExporter(getScansOutputFilename());
            Project.getInstance().TaskCollection.TaskList.Add(scanmergerExporter);



        }

        private string getScansOutputFilename()
        {
            if (this.inputDataFilenames == null) return "_thereWasAnError_scans.csv";
            string basepath = Path.GetDirectoryName(this.inputDataFilenames[0]);
            string baseFilename = Path.GetFileNameWithoutExtension(this.inputDataFilenames[0]);

            return basepath + Path.DirectorySeparatorChar + baseFilename + "_merged_scans.csv";
        }

        private string getIsosOutputFilename()
        {
            if (this.inputDataFilenames == null) return "_thereWasAnError_isos.csv";
            string basepath = Path.GetDirectoryName(this.inputDataFilenames[0]);
            string baseFilename = Path.GetFileNameWithoutExtension(this.inputDataFilenames[0]);

            return basepath + Path.DirectorySeparatorChar + baseFilename + "_merged_isos.csv";
        }

        public RunMergingProjectController(List<string> inputDataFilenames, DeconTools.Backend.Globals.MSFileType fileType, string paramFileName, BackgroundWorker bw)
            : this(inputDataFilenames, fileType, paramFileName)
        {
            this.backgroundWorker = bw;
        }

        public override void Execute()
        {
            Logger.Instance.AddEntry("DeconTools.Backend.dll version = " + AssemblyInfoRetriever.GetVersion(typeof(OldSchoolProcRunner)));
            Logger.Instance.AddEntry("DeconEngine version = " + AssemblyInfoRetriever.GetVersion(typeof(DeconToolsV2.HornTransform.clsHornTransformParameters)));
            Logger.Instance.AddEntry("Deconvolution_Algorithm = " + Project.getInstance().TaskCollection.GetDeconvolutorType());
            Logger.Instance.AddEntry("Started file processing");

            TaskControllerFactory taskControllerFactory = new TaskControllerFactory(this.backgroundWorker);
            TaskController controller = taskControllerFactory.CreateTaskController(fileType, Project.getInstance().TaskCollection);
            controller.IsosResultThresholdNum = this.IsosResultThreshold;
            controller.Execute(Project.getInstance().RunCollection);

            Logger.Instance.AddEntry("Finished file processing");


            TaskCleaner cleaner = new TaskCleaner(Project.getInstance().TaskCollection);
            cleaner.CleanTasks();
            Logger.Instance.AddEntry("Closed output files");
        }
    }
}
