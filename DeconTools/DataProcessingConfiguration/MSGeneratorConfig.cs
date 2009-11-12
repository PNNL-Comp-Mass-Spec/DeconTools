using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.Core;

namespace DeconTools.DataProcessingConfiguration
{
    public partial class MSGeneratorConfig : Form
    {

        private GenericMSGenerator data;

        public GenericMSGenerator Data
        {
            get { return data; }
            set { data = value; }
        }


        public MSGeneratorConfig(Task task)
        {
            if (task is DeconTools.Backend.ProcessingTasks.MSGenerators.GenericMSGenerator)
            {

            }
            else
            {
                throw new Exception("The task type is not correct.");
            }

            InitializeComponent();
            this.data = (GenericMSGenerator)task;
            updateFormData();
        }

        private void updateFormData()
        {
            this.minMZUpDown.Value = (decimal)(this.data.MinMZ);
            this.maxMZUpDown.Value = (decimal)this.data.MaxMZ;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            updateTaskData();
            this.Close();
        }

        private void updateTaskData()
        {
            this.data.MinMZ = (double)this.minMZUpDown.Value;
            this.data.MaxMZ = (double)this.maxMZUpDown.Value;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
