using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DeconTools.Backend;
using DeconTools.Backend.Core;
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
        bool _createMSFeatureForEachPeak;

        Globals.ProjectControllerType _projectControllerType;



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

            this._createMSFeatureForEachPeak = Properties.Settings.Default.MSFeatureForEachPeak;
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
            OpenFileDialog ofd = new OpenFileDialog();
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
            MultiFileSelectionForm frm = new MultiFileSelectionForm(this._startingFolderPath,"*.*");
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


            UserState currentState = (UserState)(e.UserState);

            this.txtFile.Text = Path.GetFileName(currentState.CurrentRun.Filename);

            int numIsotopicProfilesFoundInScan =  currentState.CurrentRun.ResultCollection.IsosResultBin.Count;
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
                this.txtFrame.Text = currentState.CurrentFrameSet.PrimaryFrame.ToString();

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
            BackgroundWorker bw = (BackgroundWorker)sender;




            try
            {

                for (int i = 0; i < _inputFileList.Length; i++)
                {
                    var workflow = ScanBasedWorkflow.CreateWorkflow(_inputFileList[i], _parameterFileName, _outputPath, bw);
                    workflow.Execute();
                }

                //No longer supported. If collaborators still use, will need to create a new workflow and use that. 
                //if (this._isRunMergingModeUsed)
                //{
                //    ProjectController runner = new RunMergingProjectController(_inputFileList.ToList(), this.msFileType, this._parameterFileName,bw);
                //    runner.Execute();
                //}
                //else if (this._CreateMSFeatureForEachPeak)
                //{
                //    ProjectController runner = new BonesProjectController(_inputFileList.ToList(), this.msFileType, this._parameterFileName,3, bw);
                //    runner.Execute();
                //}
                //else
                //{
                    
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                
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
            Properties.Settings.Default.MSFeatureForEachPeak = _createMSFeatureForEachPeak;
            Properties.Settings.Default.MergeRuns = _isRunMergingModeUsed;
            
            Properties.Settings.Default.Save();

        }

        private void btnShowOptionsForm_Click(object sender, EventArgs e)
        {
            //OptionsForm frm = new OptionsForm(this.m_projectControllerType);
            OptionsForm frm = new OptionsForm(this._isRunMergingModeUsed, this._createMSFeatureForEachPeak);
            frm.Location = new Point(this.Location.X+ this.btnShowOptionsForm.Location.X, this.Location.Y + this.btnShowOptionsForm.Location.Y + this.btnShowOptionsForm.Height);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                this._projectControllerType = frm.ProjectControllerType;
                this._isRunMergingModeUsed = frm.IsResultMergingModeUsed;
                this._createMSFeatureForEachPeak = frm.CreateMSFeatureForEachPeakMode;
            }
            else
            {

            }


        }

        private void btnSetOutputPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
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
            string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

            string firstFile = droppedFiles.First();


            bool isFile = !Directory.Exists(firstFile) &&
                         File.Exists(firstFile);




            bool isDir = (File.GetAttributes(firstFile) & FileAttributes.Directory)
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


    }
}
