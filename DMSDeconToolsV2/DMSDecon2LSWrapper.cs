using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend;
using System.Diagnostics;
using DeconTools.Backend.Utilities.Converters;
using System.ComponentModel;
using DeconTools.Backend.Core;

namespace DMSDeconToolsV2
{
    public enum DeconState
    {
        IDLE = 0,
        RUNNING_DECON,
        RUNNING_TIC,
        DONE,
        ERROR
    }


    public class DMSDecon2LSWrapper
    {
        private OldSchoolProcRunner oldschoolprocRunner;
        private BackgroundWorker bw;
        private UserState detailedState;

        #region Constructors
        public DMSDecon2LSWrapper()
        {

        }
        #endregion

        #region Properties
        private string paramFile;

        public string ParameterFile
        {
            get { return paramFile; }
            set { paramFile = value; }
        }
        private string dataFile;

        public string DataFile
        {
            get { return dataFile; }
            set { dataFile = value; }
        }
        private string outFile;

        public string OutFile
        {
            get { return outFile; }
            set { outFile = value; }
        }
        private DeconToolsV2.Readers.FileType fileType;

        public DeconToolsV2.Readers.FileType FileType
        {
            get { return fileType; }
            set { fileType = value; }
        }
        private DeconState state;

        public DeconState State
        {
            get { return state; }
            set { state = value; }
        }

        private float percentDone;

        public float PercentDone
        {
            get { return percentDone; }
            set { percentDone = value; }
        }
        private int currentScan;

        public int CurrentScan
        {
            get { return currentScan; }
            set { currentScan = value; }
        }
        #endregion

        #region Public Methods
        public void Deconvolute()
        {
            try
            {
                bool areRequirementsMet = validateDeconRequirements();
                if (areRequirementsMet)
                {
                    state = DeconState.RUNNING_DECON;
                

                    bw = new BackgroundWorker();

                    bw.WorkerReportsProgress = true;
                    bw.WorkerSupportsCancellation = true;

                    bw.DoWork += new DoWorkEventHandler(bw_DoWork);
                    bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
                    bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                    bw.RunWorkerAsync();


                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                EventLog elog = new EventLog();
                elog.Log = "Application";
                elog.Source = "Decon2LS";
                elog.WriteEntry(ex.Message + ex.StackTrace);
                elog.Close();
                state = DeconState.ERROR;
                throw ex;
            }
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                state = DeconState.ERROR;
            }
            else if (e.Error != null)
            {
                state = DeconState.ERROR;
            }
            else
            {
                state = DeconState.DONE;
            }
        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            detailedState = (UserState)(e.UserState);
            this.currentScan = detailedState.getScanOrFrameNum();
            this.percentDone = detailedState.PercentDone;

            //the following is for testing...
            Console.WriteLine("CurrentScan = " + this.currentScan + "; PercentDone = " + this.percentDone);


        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = (BackgroundWorker)sender;

            try
            {
                Console.WriteLine("test");
                Console.WriteLine(FileTypeConverter.ConvertDeconEngineFileType(fileType));
                
                this.oldschoolprocRunner = new OldSchoolProcRunner(this.dataFile,
                    FileTypeConverter.ConvertDeconEngineFileType(fileType), this.paramFile, backgroundWorker);
                this.oldschoolprocRunner.IsosResultThreshold = 10000;
                this.oldschoolprocRunner.Execute();
            }
            catch (Exception ex)
            {
                //Also write details on UserState 

                EventLog elog = new EventLog();
                elog.Log = "Application";
                elog.Source = "Decon2LS";
                elog.WriteEntry(ex.Message + ex.StackTrace);
                elog.Close();
                state = DeconState.ERROR;
                throw ex;

            }




            if (bw.CancellationPending)
            {
                e.Cancel = true;
            }
        }


        public void CreateTIC()
        {
            // in the old version of Decon2LS,  the clsProcRunner was re-run solely to extract the _scans data. 
            // here is the line:  				mobj_proc_runner.CreateTransformResultWithPeaksOnly() ; 
            // In this version, the _scans results are exported as part of the Deconvolute() method
            // and there is no need to re-execute the same methods. 
        }

        public void ResetState()
        {
            state = DeconState.IDLE;
            dataFile = null;
            outFile = null;
            paramFile = null;
        }
        #endregion

        #region Private Methods
        private bool validateDeconRequirements()
        {
            if (dataFile == null || dataFile == "")
            {
                state = DeconState.ERROR;
                throw new Exception("Please specify a data file name");
            }
            if (outFile == null || outFile == "")
            {
                state = DeconState.ERROR;
                throw new Exception("Please specify an output file name");
            }
            if (paramFile == null || paramFile == "")
            {
                state = DeconState.ERROR;
                throw new Exception("Please specify a parameter file name");
            }
            return true;
        }

        //private void loadParameters(string parameterFile)
        //{
        //    ParameterLoader loader = new ParameterLoader();
        //    loader.LoadParametersFromFile(parameterFile);

        //}
        #endregion

    }
}
