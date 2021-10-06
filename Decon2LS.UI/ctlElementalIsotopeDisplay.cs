// Written by Navdeep Jaitly and Anoop Mayampurath for the Department of Energy (PNNL, Richland, WA)
// Copyright 2006, Battelle Memorial Institute
// E-mail: navdeep.jaitly@pnl.gov or proteomics@pnnl.gov
// Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics
// -------------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Data ;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;


namespace Decon2LS
{
    /// <summary>
    /// Summary description for ctlElementalIsotopeDisplay.
    /// </summary>
    public class ctlElementalIsotopeDisplay : System.Windows.Forms.UserControl
    {

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private const string mstrDefaultTableName = "elemental_isotopes" ;
        private const string mstrDefaultFileName = "isotope.xml" ;
        private string mstr_composition_file_name = mstrDefaultFileName;
        private System.Windows.Forms.DataGrid dataGrid1;
        private DataTable table = new DataTable(mstrDefaultTableName) ;
        private DeconToolsV2.clsElementIsotopes mobj_isotope  ;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button_Save;
        private System.Windows.Forms.Button button_SaveAs;
        private System.Windows.Forms.Button button_Load;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;

        public DeconToolsV2.clsElementIsotopes ElementIsotopes
        {
            get
            {
                return mobj_isotope ;
            }
            set
            {
                mobj_isotope = value ;
                UpdateIsotopeTable() ;
            }
        }

        public ctlElementalIsotopeDisplay()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            mobj_isotope = new DeconToolsV2.clsElementIsotopes() ;

            // TODO: Add any initialization after the InitializeComponent call
            Init() ;
        }

        public void Init()
        {
            table.Columns.Add("Atomicity");
            table.Columns.Add("Element Name") ;
            table.Columns.Add("Element Symbol") ;
            table.Columns.Add("Isotope Mass") ;
            table.Columns.Add("Isotope Percentage") ;
            //table.Columns.Add("Mass Average");
            //table.Columns.Add("Mass Variance");
            dataGrid1.DataSource =  table ;

        }

        [Obsolete("DeconToolsV2.clsElementIsotopes no longer supports manually setting isotopes; they must be loaded from a file using LoadV1ElementIsotopes")]
        public void ReadFromDataGrid()
        {
            var element_symbol = "" ;
            var element_name = "";

            var atomicity  = 0 ;
            int next_row_atomicity;

            var isotope_num = 0;
            double isotope_mass ;
            double isotope_probability ;

            var row_index = 0;
            var element_index = 0;

            var colNumAtomicity = 0;
            var colNumName = 1;
            var colNumSymbol = 2;
            var colNumMass = 3;
            var colNumProb = 4;

            var num_elements = mobj_isotope.GetNumberOfElements();

            var num_rows = table.Rows.Count;

            DataRow row, next_row;

            row = table.Rows[row_index];

            while (row_index < num_rows-1)
            {
                atomicity = Convert.ToInt32(row[colNumAtomicity]);
                element_index = atomicity - 1;
                next_row = table.Rows[row_index + 1];
                next_row_atomicity = Convert.ToInt32(next_row[colNumAtomicity]);


                while(next_row_atomicity == atomicity)
                {

                    element_name = row[colNumName].ToString();
                    element_symbol = row[colNumSymbol].ToString();
                    isotope_mass = Convert.ToDouble(row[colNumMass].ToString());
                    isotope_probability = Convert.ToDouble(row[colNumProb].ToString());

                    mobj_isotope.UpdateElementalIsotope(element_index, ref atomicity,
                        ref isotope_num,  ref element_name, ref element_symbol, ref isotope_mass, ref isotope_probability );

                    row_index++;
                    row = next_row;
                    next_row = table.Rows[row_index+1];
                    next_row_atomicity = Convert.ToInt32(next_row[colNumAtomicity]);
                    isotope_num++;
                }


                element_name = row[colNumName].ToString();
                element_symbol = row[colNumSymbol].ToString();
                isotope_mass = Convert.ToDouble(row[colNumMass].ToString());
                isotope_probability = Convert.ToDouble(row[colNumProb].ToString());

                mobj_isotope.UpdateElementalIsotope(element_index, ref atomicity,
                    ref isotope_num,  ref element_name, ref element_symbol, ref isotope_mass, ref isotope_probability );
                row = next_row;
                isotope_num = 0;
                row_index++;


            }

            //last one
            atomicity = element_index;
            element_index++;
            element_name = row[colNumName].ToString();
            element_symbol = row[colNumSymbol].ToString();
            isotope_mass = Convert.ToDouble(row[colNumMass].ToString());
            isotope_probability = Convert.ToDouble(row[colNumProb].ToString());

            mobj_isotope.UpdateElementalIsotope(element_index, ref atomicity,
               ref isotope_num,  ref element_name, ref element_symbol, ref isotope_mass, ref isotope_probability );

        }

        public bool CheckData(ref string element_name)
        {
            var num_elements = mobj_isotope.GetNumberOfElements();
            var row_index = 0;
            var colNumAtomcity = 0;
            var colNumProb = 4;
            var colName = 1;

            var atomicity = 0;
            var next_row_atomicity = 0;

            var set_check = true;

            double sum_prob = 0;

            double isotope_prob = 0;

            DataRow row, next_row;

            row = table.Rows[row_index];

            var num_rows = table.Rows.Count;

            while (row_index < num_rows-1)
            {
                atomicity = Convert.ToInt32(row[colNumAtomcity]);
                sum_prob = Convert.ToDouble(row[colNumProb]);
                element_name = row[colName].ToString();
                next_row = table.Rows[row_index + 1];
                next_row_atomicity = Convert.ToInt32(next_row[colNumAtomcity]);

                while (next_row_atomicity == atomicity)
                {
                    isotope_prob = Convert.ToDouble(next_row[colNumProb].ToString());
                    sum_prob = sum_prob + isotope_prob;

                    row_index++;
                    row = next_row;
                    next_row = table.Rows[row_index+1];
                    next_row_atomicity = Convert.ToInt32(next_row[colNumAtomcity]);
                }

                if (sum_prob < 0.99995 || sum_prob > 1.00005)
                {
                    set_check = false;
                    break;
                }
                row_index++;
                row = next_row;
            }

            return set_check;

        }

        public void SetElementIsotopes(DeconToolsV2.clsElementIsotopes elementIsotopes)
        {
            mobj_isotope = elementIsotopes ;
            UpdateIsotopeTable() ;
        }
        private void UpdateIsotopeTable()
        {
            var elementSymbol = "" ;
            var elementName = "";
            var atomicity = 0 ;
            var numIsotopes = 0 ;
            var isotope_mass = new float[1];
            var isotope_probability = new float[1];
            float average_mass = 0 ;
            float mass_variance = 0 ;

            table.Rows.Clear() ;
            var num_elements = mobj_isotope.GetNumberOfElements();
            for (var element_num = 0; element_num<num_elements; element_num++)
            {
                mobj_isotope.GetElementalIsotope(element_num, ref atomicity,
                    ref numIsotopes, ref elementName, ref elementSymbol,
                    ref average_mass, ref mass_variance, ref isotope_mass,
                    ref isotope_probability );

                for (var isotope_num=0; isotope_num<numIsotopes; isotope_num++)
                {
                    var row = table.NewRow() ;
                    row[0] = atomicity.ToString();
                    row[1] =  elementName;
                    row[2] = elementSymbol;
                    row[3] = isotope_mass[isotope_num].ToString();
                    row[4] = isotope_probability[isotope_num].ToString();
                    table.Rows.Add(row) ;
                }
            }
        }

        public void LoadXML(string fileName)
        {
            dataGrid1.BackColor = System.Drawing.Color.Silver;
            dataGrid1.AlternatingBackColor = System.Drawing.Color.Silver;

            mobj_isotope.Load(fileName);
            UpdateIsotopeTable() ;
        }

        public void SaveXML(string fileName)
        {
            mobj_isotope.Write(fileName);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGrid1 = new System.Windows.Forms.DataGrid();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_SaveAs = new System.Windows.Forms.Button();
            this.button_Save = new System.Windows.Forms.Button();
            this.button_Load = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            //
            // dataGrid1
            //
            this.dataGrid1.AlternatingBackColor = System.Drawing.Color.White;
            this.dataGrid1.BackColor = System.Drawing.Color.White;
            this.dataGrid1.BackgroundColor = System.Drawing.SystemColors.GrayText;
            this.dataGrid1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dataGrid1.CaptionBackColor = System.Drawing.SystemColors.GrayText;
            this.dataGrid1.DataMember = "";
            this.dataGrid1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dataGrid1.Location = new System.Drawing.Point(3, 21);
            this.dataGrid1.Name = "dataGrid1";
            this.dataGrid1.Size = new System.Drawing.Size(498, 280);
            this.dataGrid1.TabIndex = 0;
            this.dataGrid1.Navigate += new System.Windows.Forms.NavigateEventHandler(this.dataGrid1_Navigate);
            this.dataGrid1.Leave += new System.EventHandler(this.dataGrid1_Leave);
            //
            // groupBox1
            //
            this.groupBox1.Controls.Add(this.button_SaveAs);
            this.groupBox1.Controls.Add(this.button_Save);
            this.groupBox1.Controls.Add(this.button_Load);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox1.Location = new System.Drawing.Point(3, 301);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(498, 64);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            //
            // button_SaveAs
            //
            this.button_SaveAs.Location = new System.Drawing.Point(280, 16);
            this.button_SaveAs.Name = "button_SaveAs";
            this.button_SaveAs.Size = new System.Drawing.Size(80, 32);
            this.button_SaveAs.TabIndex = 2;
            this.button_SaveAs.Text = "Save As";
            this.button_SaveAs.Click += new System.EventHandler(this.button_SaveAs_Click);
            //
            // button_Save
            //
            this.button_Save.Location = new System.Drawing.Point(152, 16);
            this.button_Save.Name = "button_Save";
            this.button_Save.Size = new System.Drawing.Size(88, 32);
            this.button_Save.TabIndex = 1;
            this.button_Save.Text = "Save";
            this.button_Save.Click += new System.EventHandler(this.button_Save_Click);
            //
            // button_Load
            //
            this.button_Load.Location = new System.Drawing.Point(16, 16);
            this.button_Load.Name = "button_Load";
            this.button_Load.Size = new System.Drawing.Size(96, 32);
            this.button_Load.TabIndex = 0;
            this.button_Load.Text = "Load";
            this.button_Load.Click += new System.EventHandler(this.button_Load_Click);
            //
            // groupBox2
            //
            this.groupBox2.Controls.Add(this.dataGrid1);
            this.groupBox2.Controls.Add(this.groupBox1);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(504, 368);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Compostion";
            //
            // groupBox3
            //
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox3.Location = new System.Drawing.Point(0, 368);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(504, 96);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Helpful Tips";
            //
            // label3
            //
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label3.Location = new System.Drawing.Point(80, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(408, 24);
            this.label3.TabIndex = 2;
            this.label3.Text = "- Values can be edited in the grid and saved in original format or as new .xml";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label2
            //
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label2.Location = new System.Drawing.Point(80, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(400, 24);
            this.label2.TabIndex = 1;
            this.label2.Text = " - Loads in the isotopic composition in the form of a .xml document";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label1
            //
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label1.Location = new System.Drawing.Point(32, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(160, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Controls Isotopic Composition";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // ctlElementalIsotopeDisplay
            //
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Name = "ctlElementalIsotopeDisplay";
            this.Size = new System.Drawing.Size(504, 464);
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void dataGrid1_Navigate(object sender, System.Windows.Forms.NavigateEventArgs ne)
        {

        }


        private void button_Save_Click(object sender, System.EventArgs e)
        {
            var fileName1 = mstr_composition_file_name;
            SaveXML(fileName1);
        }

        private void button_SaveAs_Click(object sender, System.EventArgs e)
        {
            try
            {
                var savefileDialog1 = new SaveFileDialog();
                savefileDialog1.Title = "Specify Destination Filename";
                savefileDialog1.Filter = "Composition files (*.xml)|*.xml";
                savefileDialog1.FilterIndex = 1;
                savefileDialog1.OverwritePrompt = true;
                savefileDialog1.RestoreDirectory = true;
                savefileDialog1.InitialDirectory = "";

                String fileName = null;
                if(savefileDialog1.ShowDialog() == DialogResult.OK)
                {
                    fileName = savefileDialog1.FileName ;
                    SaveXML(fileName);
                }
                else
                {
                    return ;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString()) ;
            }
        }

        private void button_Load_Click(object sender, System.EventArgs e)
        {

            try
            {
                var openfileDialog1 = new OpenFileDialog();
                openfileDialog1.Filter = "Composition files (*.xml)|*.xml" ;
                openfileDialog1.FilterIndex = 1;
                openfileDialog1.RestoreDirectory = true;
                openfileDialog1.InitialDirectory = "";

                string fileName = null ;
                if(openfileDialog1.ShowDialog() == DialogResult.OK)
                {
                    fileName = openfileDialog1.FileName ;
                    mstr_composition_file_name = fileName;
                    LoadXML(fileName) ;
                }
                else
                {
                    return ;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString()) ;

            }

        }

        private void dataGrid1_Leave(object sender, System.EventArgs e)
        {
            var elementName = "";
            var check = CheckData(ref elementName);
            if (check)
            {
                ReadFromDataGrid();
                UpdateIsotopeTable();
            }
            else
            {
                var msg = String.Format("The Probabilities of isotopes of {0} don't add up to one", elementName);
                //throw new Exception(msg);
                MessageBox.Show(msg);
                UpdateIsotopeTable();
            }
        }

    }
}
