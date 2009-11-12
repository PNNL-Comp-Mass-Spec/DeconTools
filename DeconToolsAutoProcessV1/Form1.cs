using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using System.IO;
using DeconTools.Backend.Runs;
using GWSFileUtilities;
using DeconTools.Backend.Utilities;

namespace DeconToolsAutoProcessV1
{
    public partial class Form1 : Form
    {
        string[] inputFileList;
        string parameterFileName;
        string startingFolderPath;
        Globals.MSFileType msFileType;
        BackgroundWorker bw;

        bool isRunMergingModeUsed;



        public Form1()
        {
            InitializeComponent();

            getSettings();
            this.progressBar2.Maximum = 10;
            this.Text = "DeconTools AutoProcessor_" + AssemblyInfoRetriever.GetVersion(typeof(Task),false);
        }

        private void getSettings()
        {
            try
            {
                startingFolderPath = Properties.Settings.Default.startingFolder;
            }
            catch (Exception)
            {

                startingFolderPath = "";
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            inputFileList = getInputFilenames();
            if (inputFileList == null) return;

            parameterFileName = getParameterFileName();
            if (parameterFileName == null) return;

            msFileType = getMSFileType();
            if (msFileType == Globals.MSFileType.Undefined) return;
        }

        private Globals.MSFileType getMSFileType()
        {
            MSFileTypeSelectorForm form;
            if (this.inputFileList != null && this.inputFileList.Length > 0)
            {
                form = new MSFileTypeSelectorForm(this.inputFileList[0]);
            }
            else
            {
                form = new MSFileTypeSelectorForm();
            }
            form.Location = Cursor.Position;


            if (form.ShowDialog() == DialogResult.OK)
            {
                return form.SelectedFiletype;
            }
            else
            {
                return Globals.MSFileType.Undefined;
            }
        }

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
            MultiFileSelectionForm frm = new MultiFileSelectionForm(this.startingFolderPath,"*.*");
            frm.Text = "Select files for processing...";
            frm.StartPosition = FormStartPosition.Manual;
            frm.Location = this.Location;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                this.startingFolderPath = frm.StartingFolderPath;
                
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
            if (bw != null && bw.IsBusy)
            {
                MessageBox.Show("Already processing...  please wait or click 'Abort'");
                return;
            }

            if (this.inputFileList == null || this.parameterFileName == null)
            {
                MessageBox.Show("Please run the Setup Wizard first");
                return;
            }

            this.txtProcessingStatus.Text = "Working...";

            bw = new BackgroundWorker();

            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;

            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            bw.RunWorkerAsync();


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
            this.progressBar2.Value = setProgress2Value(currentState.CurrentRun.CurrentScanSet.NumIsotopicProfiles);
            
            
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
                catch (Exception ex)
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
                if (this.isRunMergingModeUsed)
                {
                    ProjectController runner = new RunMergingProjectController(inputFileList.ToList(), this.msFileType, this.parameterFileName,bw);
                    runner.Execute();
                }
                else
                {
                    for (int i = 0; i < inputFileList.Length; i++)
                    {
                        OldSchoolProcRunner runner = new OldSchoolProcRunner(inputFileList[i], this.msFileType, this.parameterFileName, bw);
                        runner.IsosResultThreshold = 10000;
                        runner.Execute();
                    }
                }
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

        private void CALL_DeconConsole(string currentFilename)
        {
            string deconConsoleArgument = buildArgument(currentFilename, msFileType, parameterFileName);




            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.EnableRaisingEvents = true;
            proc.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "\\DeconConsole.exe";
            proc.StartInfo.Arguments = deconConsoleArgument;

            proc.Start();
            proc.WaitForExit();
        }

        private string buildArgument(string currentFilename, Globals.MSFileType msFileType, string parameterFileName)
        {

            StringBuilder sb = new StringBuilder();

            sb.Append(" ");
            sb.Append("\"");
            sb.Append(currentFilename);
            sb.Append("\"");

            sb.Append(" ");
            sb.Append(msFileType.ToString());

            sb.Append(" ");
            sb.Append("\"");
            sb.Append(parameterFileName);
            sb.Append("\"");

            string argument = sb.ToString();
            Console.WriteLine(argument);
            return argument;
        }

        private void CALL_DeconConsole()
        {

        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            if (this.bw != null)
            {
                bw.CancelAsync();
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

            if (this.startingFolderPath != null) Properties.Settings.Default.startingFolder = this.startingFolderPath;
            Properties.Settings.Default.Save();

        }

        private void btnShowOptionsForm_Click(object sender, EventArgs e)
        {
            OptionsForm frm = new OptionsForm(isRunMergingModeUsed);
            frm.Location = new Point(this.Location.X+ this.btnShowOptionsForm.Location.X, this.Location.Y + this.btnShowOptionsForm.Location.Y + this.btnShowOptionsForm.Height);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                this.isRunMergingModeUsed = frm.IsResultMergingModeUsed;
            }
            else
            {

            }


        }
    }
}
