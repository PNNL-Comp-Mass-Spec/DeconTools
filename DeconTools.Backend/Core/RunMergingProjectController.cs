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

namespace DeconTools.Backend.Core
{
    public class RunMergingProjectController : ProjectController
    {
        private List<string> inputDataFilenames;
        private string paramFilename;
        private string outputFilename;
        private DeconTools.Backend.Globals.MSFileType fileType;
        private BackgroundWorker backgroundWorker;

        #region Properties
        private Project project;

        public Project Project
        {
            get { return project; }
            set { project = value; }
        }


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
            this.project = Project.getInstance();
            this.project.LoadOldDecon2LSParameters(this.paramFilename);
            this.IsosResultThreshold = 100000;       // results will be serialized if count is greater than this number

            RunFactory runfactory = new RunFactory();


            //Create runs and create ScanSets
            foreach (string filename in this.inputDataFilenames)
            {
                Run run;
                run = runfactory.CreateRun(fileType, filename, project.Parameters.OldDecon2LSParameters);
                Check.Assert(run != null, "Processing aborted. Could not handle supplied File(s)");
                project.RunCollection.Add(run);
                
                ScanSetCollectionCreator scanSetCollectionCreator = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan, 
                    Project.getInstance().Parameters.NumScansSummed,
                    Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.NumScansToAdvance,false);
                scanSetCollectionCreator.Create();
            }

            MSGeneratorFactory msGeneratorFactory = new MSGeneratorFactory();
            Task msGen = msGeneratorFactory.CreateMSGenerator(fileType, project.Parameters.OldDecon2LSParameters);
            project.TaskCollection.TaskList.Add(msGen);

            if (project.Parameters.OldDecon2LSParameters.HornTransformParameters.ZeroFill)
            {
                Task zeroFiller = new DeconToolsZeroFiller(project.Parameters.OldDecon2LSParameters.HornTransformParameters.NumZerosToFill);
                project.TaskCollection.TaskList.Add(zeroFiller);
            }
            if (project.Parameters.OldDecon2LSParameters.HornTransformParameters.UseSavitzkyGolaySmooth)
            {
                Task smoother = new DeconToolsSavitzkyGolaySmoother(
                    this.project.Parameters.OldDecon2LSParameters.HornTransformParameters.SGNumLeft,
                    this.project.Parameters.OldDecon2LSParameters.HornTransformParameters.SGNumRight,
                    this.project.Parameters.OldDecon2LSParameters.HornTransformParameters.SGOrder);
                project.TaskCollection.TaskList.Add(smoother);
            }

            Task peakDetector = new DeconToolsPeakDetector(project.Parameters.OldDecon2LSParameters.PeakProcessorParameters);
            project.TaskCollection.TaskList.Add(peakDetector);

            
            //DeconvolutorFactory deconFactory = new DeconvolutorFactory();
            //Task deconvolutor = deconFactory.CreateDeconvolutor(project.Parameters.OldDecon2LSParameters);
            Task deconvolutor = new SimpleDecon(0.0005);
            project.TaskCollection.TaskList.Add(deconvolutor);

            Task isosmergerExporter = new BasicIsosMergerExporter(getIsosOutputFilename());
            project.TaskCollection.TaskList.Add(isosmergerExporter);

            Task scanResultUpdater = new ScanResultUpdater();
            project.TaskCollection.TaskList.Add(scanResultUpdater);

            Task scanmergerExporter = new BasicScansMergerExporter(getScansOutputFilename());
            project.TaskCollection.TaskList.Add(scanmergerExporter);



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
            Logger.Instance.AddEntry("Deconvolution_Algorithm = " + project.TaskCollection.GetDeconvolutorType());
            Logger.Instance.AddEntry("Started file processing");

            TaskControllerFactory taskControllerFactory = new TaskControllerFactory(this.backgroundWorker);
            TaskController controller = taskControllerFactory.CreateTaskController(fileType, project.TaskCollection);
            controller.IsosResultThresholdNum = this.IsosResultThreshold;
            controller.Execute(project.RunCollection);

            Logger.Instance.AddEntry("Finished file processing");


            TaskCleaner cleaner = new TaskCleaner(project.TaskCollection);
            cleaner.CleanTasks();
            Logger.Instance.AddEntry("Closed output files");
        }
    }
}
