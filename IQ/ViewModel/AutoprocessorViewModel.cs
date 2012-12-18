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
                string trimmedPath = "";
                if (!string.IsNullOrEmpty(value))
                {
                    trimmedPath = value.Trim('"');
                }
                _datasetPath = trimmedPath;
                OnPropertyChanged("DatasetPath");
            }
        }

        public string WorkflowParametersFilePath
        {
            get { return ExecutorParameters.WorkflowParameterFile; }
            set
            {
                string trimmedPath = "";
                if (!string.IsNullOrEmpty(value))
                {
                    trimmedPath = value.Trim('"');
                }

                ExecutorParameters.WorkflowParameterFile = trimmedPath;
                OnPropertyChanged("WorkflowParametersFilePath");
            }
        }


        public string TargetsFilePath
        {
            get { return ExecutorParameters.TargetsFilePath; }
            set
            {
                string trimmedPath = "";
                if (!string.IsNullOrEmpty(value))
                {
                    trimmedPath = value.Trim('"');
                }
                ExecutorParameters.TargetsFilePath = trimmedPath;
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
                    else
                    {
                        string currentTargetString = info.Result == null ? "" : info.Result.Target != null ? info.Result.Target.ID.ToString() : "";

                        if (info.Result == null || info.Result.Target == null)
                        {
                            currentTargetString = "";
                        }
                        else
                        {
                            currentTargetString = info.Result.Target.ID.ToString() + "; m/z " +
                                                  info.Result.Target.MZ.ToString("0.0000") + "; z=" +
                                                  info.Result.Target.ChargeState;

                            if (info.Result.IsotopicProfile!=null)
                            {
                                currentTargetString = currentTargetString + "\nTARGET found!. Scan= " +
                                                      info.Result.GetScanNum() + "; Intensity= " +
                                                      info.Result.IntensityAggregate.ToString("0") +
                                                      "; Fit score= " + info.Result.Score.ToString("0.000");
                            }
                            else
                            {
                                currentTargetString = currentTargetString + "\nTarget not found";
                            }

                        }

                        GeneralStatusMessage = "Processed target " + currentTargetString;
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
