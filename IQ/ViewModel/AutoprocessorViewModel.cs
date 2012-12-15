using System;
using System.ComponentModel;
using DeconTools.Backend.Core;
using DeconTools.Workflows.Backend.Core;

namespace IQ.ViewModel
{
    public class AutoprocessorViewModel : ViewModelBase
    {

        private BackgroundWorker _backgroundWorker;

        #region Constructors

        public AutoprocessorViewModel()
        {
            ExecutorParameters = new BasicTargetedWorkflowExecutorParameters();
            TargetedWorkflowParameters = new BasicTargetedWorkflowParameters();


          

        }

        #endregion

        #region Properties

        public Run Run { get; set; }

        public WorkflowExecutorBaseParameters ExecutorParameters { get; set; }

        public TargetedWorkflowParameters TargetedWorkflowParameters { get; set; }

        private string _datasetPath;
        public string DatasetPath
        {
            get { return _datasetPath; }
            set
            {
                _datasetPath = value;
                OnPropertyChanged("DatasetPath");
            }
        }

        public string WorkflowParametersFilePath
        {
            get { return ExecutorParameters.WorkflowParameterFile; }
            set
            {
                ExecutorParameters.WorkflowParameterFile = value;
                OnPropertyChanged("WorkflowParametersFilePath");
            }
        }


        public string TargetsFilePath
        {
            get { return ExecutorParameters.TargetsFilePath; }
            set
            {
                ExecutorParameters.TargetsFilePath = value;
                OnPropertyChanged("TargetsFilePath");
            }
        }

        private string _generalStatusMessage;
        public string GeneralStatusMessage
        {
            get { return _generalStatusMessage; }
            set
            {
                _generalStatusMessage = value;
                OnPropertyChanged("GeneralStatusMessage");
            }
        }

        private int _percentProgress;
        public int PercentProgress
        {
            get { return _percentProgress; }
            set
            {
                _percentProgress = value;
                OnPropertyChanged("PercentProgress");
            }
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        public void Execute()
        {
            

            if (_backgroundWorker != null && _backgroundWorker.IsBusy)
            {
                GeneralStatusMessage = "Already processing.... please wait or click 'Cancel'";
                return;
            }
            
             GeneralStatusMessage = "IQ working....";


            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.WorkerReportsProgress = true;

            _backgroundWorker.DoWork += _backgroundWorker_DoWork;
            _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;
            _backgroundWorker.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;
            _backgroundWorker.RunWorkerAsync();



            

        }

        public void CancelProcessing()
        {
            if (_backgroundWorker != null)
            {
                _backgroundWorker.CancelAsync();
                GeneralStatusMessage = "Cancelled processing.";
            }
        }

        void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            var executor = new BasicTargetedWorkflowExecutor(ExecutorParameters, DatasetPath, worker);
            executor.Execute();
            
            if (worker.CancellationPending)
            {
                e.Cancel = true;
            }

        }

        void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                GeneralStatusMessage = "Cancelled";
            }
            else if (e.Error != null)
            {
                GeneralStatusMessage = "Error - check log file or results output";
            }
            else
            {
                GeneralStatusMessage = "Processing COMPLETE.";
                //PercentProgress = 100;
            }
        }

     
        void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PercentProgress = e.ProgressPercentage;

            if (e.UserState != null)
            {
                if (e.UserState is TargetedWorkflowExecutorProgressInfo)
                {
                    var info = (TargetedWorkflowExecutorProgressInfo)e.UserState;
                    if (info.IsGeneralProgress)
                    {

                        var infostrings = info.ProgressInfoString.Split(new string[] { Environment.NewLine },
                                                                        StringSplitOptions.RemoveEmptyEntries);

                        foreach (var infostring in infostrings)
                        {
                            if (!String.IsNullOrEmpty(infostring))
                            {
                                //StatusCollection.Add(infostring);
                                GeneralStatusMessage = infostring;
                            }
                        }



                    }
                }
                else
                {
                    Console.WriteLine(e.UserState);
                }
            }





        }

      
    }
}
