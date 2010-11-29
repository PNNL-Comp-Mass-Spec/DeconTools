using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;
using System.IO;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using System.ComponentModel;

namespace DeconTools.Backend.ProjectControllers
{


    public class UIMFMSOnlyProjectController:ProjectController
    {
        private string inputDataFilename;
        private string paramFilename;
        private string outputFilepath;

        private DeconTools.Backend.Globals.MSFileType fileType;
        private BackgroundWorker backgroundWorker;

        #region Properties
        private Project project;

        public Project Project
        {
            get { return project; }

        }


        private int isosResultThreshold;

        public int IsosResultThreshold
        {
            get { return isosResultThreshold; }
            set { isosResultThreshold = value; }
        }

        private Globals.ExporterType exporterType;

        public Globals.ExporterType ExporterType
        {
            get { return exporterType; }
            set { exporterType = value; }
        }


        #endregion


        #region Constructors
 
        public UIMFMSOnlyProjectController(string inputDataFilename, DeconTools.Backend.Globals.MSFileType fileType, string paramFileName, BackgroundWorker backgroundWorker)
        {
            this.backgroundWorker = backgroundWorker;

            Project.Reset();
            project = Project.getInstance();
            this.inputDataFilename = inputDataFilename;
            this.fileType = fileType;
            this.paramFilename = paramFileName;
            this.project = Project.getInstance();
            Project.getInstance().LoadOldDecon2LSParameters(this.paramFilename);
            this.IsosResultThreshold = 25000;       // results will be serialized if count is greater than this number
            this.exporterType = getExporterTypeFromOldParameters(Project.getInstance().Parameters.OldDecon2LSParameters);

            RunFactory runfactory = new RunFactory();

            //Create run
            Run run;
            run = runfactory.CreateRun(fileType, this.inputDataFilename, Project.getInstance().Parameters.OldDecon2LSParameters);
            Project.getInstance().RunCollection.Add(run);

            Check.Assert(run != null, "Processing aborted. Could not handle supplied File(s)");
            //Define ScansetCollection

            Logger.Instance.OutputFilename = run.DataSetPath + "\\" + run.DatasetName + "_log.txt";
            Logger.Instance.AddEntry("DeconTools.Backend.dll version = " + AssemblyInfoRetriever.GetVersion(typeof(OldSchoolProcRunner)));
            Logger.Instance.AddEntry("ParameterFile = " + Path.GetFileName(this.paramFilename));
            Logger.Instance.AddEntry("DeconEngine version = " + AssemblyInfoRetriever.GetVersion(typeof(DeconToolsV2.HornTransform.clsHornTransformParameters)));
            Logger.Instance.AddEntry("RapidEngine version = " + RapidDeconvolutor.getRapidVersion());
            Logger.Instance.AddEntry("UIMFLibrary version = " + DeconTools.Backend.Utilities.AssemblyInfoRetriever.GetVersion(typeof(UIMFLibrary.DataReader)), 
                Logger.Instance.OutputFilename);   //forces it to write out immediately and clear buffer


            if (run is UIMFRun)     // not pretty...  
            {
                UIMFRun uimfRun = (UIMFRun)run;
                FrameSetCollectionCreator frameSetcreator = new FrameSetCollectionCreator(run, uimfRun.MinFrame,
                      uimfRun.MaxFrame, Project.getInstance().Parameters.NumFramesSummed, 1);
                frameSetcreator.Create();

                uimfRun.GetFrameDataAllFrameSets();     //this adds avgTOFlength and framePressureBack to each frame's object data; I do this so it doesn't have to be repeated looked up.
            }

            try
            {
                ScanSetCollectionCreator scanSetCollectionCreator = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan,
                    Project.getInstance().Parameters.NumScansSummed,
                    Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.NumScansToAdvance,
                    Project.getInstance().Parameters.OldDecon2LSParameters.HornTransformParameters.ProcessMSMS);
                scanSetCollectionCreator.Create();
            }
            catch (Exception ex)
            {

                Logger.Instance.AddEntry("ERROR: " + ex.Message, Logger.Instance.OutputFilename);   //forces it to write out immediately and clear buffer
                throw ex;
            }

            //Create Tasks and add to task collection...
            MSGeneratorFactory msGeneratorFactory = new MSGeneratorFactory();
            Task msGen = msGeneratorFactory.CreateMSGenerator(fileType, Project.getInstance().Parameters.OldDecon2LSParameters);
            Project.getInstance().TaskCollection.TaskList.Add(msGen);
        }


        #endregion

        #region Properties
        #endregion

        #region Public Methods



        #endregion

        #region Private Methods
        #endregion

        public override void Execute()
        {
            Logger.Instance.AddEntry("Started file processing", Logger.Instance.OutputFilename);  //this will write out immediately

            TaskControllerFactory taskControllerFactory = new TaskControllerFactory(this.backgroundWorker);
            TaskController controller = taskControllerFactory.CreateTaskController(fileType, Project.getInstance().TaskCollection);
            controller.IsosResultThresholdNum = this.IsosResultThreshold;

            try
            {
                controller.Execute(Project.getInstance().RunCollection);

            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry(" ------------------------------- ERROR: " + ex.Message, Logger.Instance.OutputFilename);
                throw ex;   //throw the error again and let something else catch it. 
            }

            Logger.Instance.AddEntry("Finished file processing", Logger.Instance.OutputFilename);
            Logger.Instance.AddEntry("total processing time = " + Logger.Instance.GetTimeDifference("Started file processing", "Finished file processing"));

            ProjectFacade pf = new ProjectFacade();
            Logger.Instance.AddEntry("total features = " + pf.GetTotalFeaturesFromScanResultCollection());

            Logger.Instance.WriteToFile(Project.getInstance().RunCollection[0].Filename + "_log.txt", true);
            Logger.Instance.Close();

            TaskCleaner taskCleaner = new TaskCleaner(Project.getInstance().TaskCollection);
            taskCleaner.CleanTasks();

            Project.Reset();      //sets Project singleton to null;
        }
    }
}
