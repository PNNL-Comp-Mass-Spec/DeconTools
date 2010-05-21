using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DeconTools.Backend;
using System.Diagnostics;
using System.IO;

namespace DeconTools.QCTesting
{
    public partial class Form1 : Form
    {


        IList<TimeResult> timingResults = new BindingList<TimeResult>();


        string m_DatasetSelected;
        string m_ParameterSelected;




        public Form1()
        {
            InitializeComponent();

            this.listBox1.DataSource = timingResults;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;

            string testFile = this.textBox1.Text;
            string testParamFile = this.textBox2.Text;

            
            sw.Start();
            OldSchoolProcRunner runner = new OldSchoolProcRunner(testFile, Globals.MSFileType.PNNL_UIMF, testParamFile,bw);
            runner.Execute();


            sw.Stop();


            double timeOfProcessing = (double)sw.ElapsedMilliseconds / 1000;
            TimeResult t = new TimeResult(DateTime.Now, timeOfProcessing, Path.GetFileName(testFile));

            this.timingResults.Add(t);


        }

        private void txtBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

            string firstFile = droppedFiles.First();

            if (firstFile != null)
            {
                this.textBox1.Text = firstFile;
            }

        }

        private void textBox2_DragDrop(object sender, DragEventArgs e)
        {
            string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

            string firstFile = droppedFiles.First();

            if (firstFile != null)
            {
                this.textBox2.Text = firstFile;
            }

        }


    }
}
