using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DeconTools.Backend.Core;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Workflows;
using GWSFileUtilities;

namespace DeconToolsAutoProcessV1
{
    public partial class Form1 : Form
    {
        string[] _inputFileList;
        string _parameterFileName;
        string _startingFolderPath;
        private string _outputPath;
        BackgroundWorker _bw;
        bool _isRunMergingModeUsed;

        DeconToolsParameters _parameters;
        private string _currentFile;

        public Form1()
        {
            InitializeComponent();

            GetSettings();
            progressBar2.Maximum = 10;

            this.Text = "DeconTools AutoProcessor_" + AssemblyInfoRetriever.GetVersion(typeof(Task), false);
        }



        private void GetSettings()
        {
            try
            {
                _startingFolderPath = Properties.Settings.Default.startingFolder;
            }
            catch (Exception)
            {

                _startingFolderPath = "";
            }

            this._isRunMergingModeUsed = Properties.Settings.Default.MergeRuns;


        }

        private void button1_Click(object sender, EventArgs e)
        {
            _inputFileList = getInputFilenames();
            if (_inputFileList == null) return;

            _parameterFileName = getParameterFileName();
            if (_parameterFileName == null) return;
        }

        //private Globals.MSFileType getMSFileType()
        //{
        //    MSFileTypeSelectorForm form;
        //    if (this._inputFileList != null && this._inputFileList.Length > 0)
        //    {
        //        form = new MSFileTypeSelectorForm(this._inputFileList[0]);
        //    }
        //    else
        //    {
        //        form = new MSFileTypeSelectorForm();
        //    }
        //    form.Location = Cursor.Position;


        //    if (form.ShowDialog() == DialogResult.OK)
        //    {
        //        return form.SelectedFiletype;
        //    }
        //    else
        //    {
        //        return Globals.MSFileType.Undefined;
        //    }
        //}

        private string getParameterFileName()
        {
            var ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Title = "Step 2:  Select Parameter File";



            ofd.Filter = "xml files (*.*)|*.xml";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                return ofd.FileName;
            }
            else
            {
                return null;
            }
        }

        private string[] getInputFilenames()
        {
            var frm = new MultiFileSelectionForm(this._startingFolderPath, "*.*");
            frm.Text = "Select files for processing...";
            frm.StartPosition = FormStartPosition.Manual;
            frm.Location = this.Location;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                this._startingFolderPath = frm.StartingFolderPath;
                if (this._startingFolderPath != null) Properties.Settings.Default.startingFolder = this._startingFolderPath;

                return frm.SelectedFileList.ToArray();
            }
            else
            {
                return null;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnAutoProcess_Click(object sender, EventArgs e)
        {

            if (!File.Exists(_parameterFileName))
            {
                MessageBox.Show("File not found error - Parameter file does not exist.");
                return;
            }


            if (_bw != null && _bw.IsBusy)
            {
                MessageBox.Show("Already processing...  please wait or click 'Abort'");
                return;
            }

            if (this._inputFileList == null || this._parameterFileName == null)
            {
                MessageBox.Show("Please run the Setup Wizard first");
                return;
            }

            try
            {
                TrySetOutputFolder();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            try
            {

                _parameters = new DeconToolsParameters();
                _parameters.LoadFromOldDeconToolsParameterFile(_parameterFileName);
            }
            catch (Exception ex)
            {

                MessageBox.Show("Tried to load parameters. Serious error occurred. Error message: " + ex.Message +
                                "\n\n" + ex.StackTrace);
                return;
            }





            this.txtProcessingStatus.Text = "Working...";

            _bw = new BackgroundWorker();

            _bw.WorkerReportsProgress = true;
            _bw.WorkerSupportsCancellation = true;

            _bw.DoWork += bw_DoWork;
            _bw.ProgressChanged += bw_ProgressChanged;
            _bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            _bw.RunWorkerAsync();


            //for (int i = 0; i < inputFileList.Length; i++)
            //{
            //    CALL_DeconConsole(inputFileList[i]);
            //}

            //MessageBox.Show("Processing complete!");






        }



        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                this.txtProcessingStatus.Text = "Cancelled";
            }
            else if (e.Error != null)
            {
                this.txtProcessingStatus.Text = "Error";
            }
            else
            {
                this.txtProcessingStatus.Text = "COMPLETE";
                this.progressBar1.Value = 100;
            }
        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;


            var currentState = (ScanBasedProgressInfo)(e.UserState);

            this.txtFile.Text = Path.GetFileName(currentState.CurrentRun.Filename);

            var numIsotopicProfilesFoundInScan = currentState.CurrentRun.ResultCollection.IsosResultBin.Count;
            this.progressBar2.Value = setProgress2Value(numIsotopicProfilesFoundInScan);


            this.lblNumIsotopicProfiles.Text = this.progressBar2.Maximum.ToString();
            this.txtScanCompleted.Text = currentState.CurrentScanSet.PrimaryScanNumber.ToString();

            if (currentState.CurrentRun.ResultCollection != null && currentState.CurrentRun.ResultCollection.ScanResultList != null)
            {
                try
                {
                    var query = (int?)(from n in currentState.CurrentRun.ResultCollection.ScanResultList
                                       select n.NumIsotopicProfiles).Sum() ?? 0;



                    this.txtTotalFeatures.Text = query.ToString();

                }
                catch (Exception)
                {
                    //throw;
                    //TODO: need to fix this later.  The .Sum is not working correctly on IMF files.
                }
            }
            if (currentState.CurrentRun is UIMFRun)
            {
                this.txtFrame.Text = currentState.CurrentScanSet.PrimaryScanNumber.ToString();

            }
            else
            {
            }
        }

        private int setProgress2Value(int p)
        {
            if (p > progressBar2.Maximum) progressBar2.Maximum = p;
            return p;
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var bw = (BackgroundWorker)sender;


            try
            {


                //This mode was requested by Julia Laskin. 
                //This mode detects peaks in each dataset and merges the output
                if (_parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName.ToLower() == "run_merging_with_peak_export")
                {
                    var workflow = new RunMergingPeakExportingWorkflow(_parameters, _inputFileList, _outputPath, bw);
                    workflow.Execute();
                }
                else
                {
                    foreach (var file in _inputFileList)
                    {
                        _currentFile = file;
                        var workflow = ScanBasedWorkflow.CreateWorkflow(file, _parameterFileName, _outputPath, bw);
                        
                        workflow.Execute();
                    }
                }


            }
            catch (COMException ex)
            {
               // bool isFile =   RunUtilities.RunIsFileOrFolder(_currentFile);

                var errorMessage =
                    "A 'COMException' has occurred. This can happen when the vendor library has not been installed.\n\n";

                errorMessage +=
                    "If you are trying to read Thermo .raw files, please install Thermo's MSFileReader library. ";

                errorMessage += "To do so, Google 'thermo msfilereader' and you should find it. If not, contact us.\n\n";

                errorMessage += "Full error details below:\n";

                errorMessage += ex.Message;

                MessageBox.Show(errorMessage, "COMError occurred");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine+  Environment.NewLine+ "**NOTE: see log file for additional details.");
            }


            if (bw.CancellationPending)
            {
                e.Cancel = true;
            }
        }


        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            if (this._bw != null)
            {
                _bw.CancelAsync();
            }

            resetStatus();

        }

        private void resetStatus()
        {
            this.progressBar1.Value = 0;

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            saveSettings();
        }

        private void saveSettings()
        {

            if (this._startingFolderPath != null) Properties.Settings.Default.startingFolder = this._startingFolderPath;
            Properties.Settings.Default.MergeRuns = _isRunMergingModeUsed;

            Properties.Settings.Default.Save();

        }

        private void btnShowOptionsForm_Click(object sender, EventArgs e)
        {
            //OptionsForm frm = new OptionsForm(this.m_projectControllerType);
            //var frm = new OptionsForm(_isRunMergingModeUsed);
            //frm.Location = new Point(this.Location.X+ this.btnShowOptionsForm.Location.X, this.Location.Y + this.btnShowOptionsForm.Location.Y + this.btnShowOptionsForm.Height);
            //if (frm.ShowDialog() == DialogResult.OK)
            //{
            //    _isRunMergingModeUsed = frm.IsResultMergingModeUsed;

            //}



        }

        private void btnSetOutputPath_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                this.txtOutputPath.Text = fbd.SelectedPath;
            }

        }

        private void basic_dragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void txtOutputPath_DragDrop(object sender, DragEventArgs e)
        {
            var droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

            var firstFile = droppedFiles.First();


            var isFile = !Directory.Exists(firstFile) &&
                         File.Exists(firstFile);




            var isDir = (File.GetAttributes(firstFile) & FileAttributes.Directory)
                 == FileAttributes.Directory;


            if (isDir)
            {
                txtOutputPath.Text = firstFile;
                return;
            }

            if (isFile)
            {
                txtOutputPath.Text = Path.GetDirectoryName(firstFile);
            }



        }

        private void txtOutputPath_TextChanged(object sender, EventArgs e)
        {
            if (Directory.Exists(txtOutputPath.Text))
            {
                _outputPath = txtOutputPath.Text;

            }
            else
            {
                _outputPath = null;
            }
        }


        private void TrySetOutputFolder()
        {
            if (String.IsNullOrEmpty(txtOutputPath.Text))
            {
                _outputPath = null;
                return;
            }

            if (!Directory.Exists(txtOutputPath.Text))
            {

                try
                {
                    Directory.CreateDirectory(txtOutputPath.Text);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Check your output directory. Something wrong there.", ex);

                }
            }

            _outputPath = txtOutputPath.Text;

            return;
        }


    }
}
