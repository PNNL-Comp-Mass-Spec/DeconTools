// Written by Navdeep Jaitly for the Department of Energy (PNNL, Richland, WA)
// Copyright 2006, Battelle Memorial Institute
// E-mail: navdeep.jaitly@pnl.gov
// Website: https://omics.pnl.gov/software or http://panomics.pnnl.gov
// -------------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Windows.Forms;

namespace Decon2LS
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class frmSelectRaw : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox mtxtFile;
        private System.Windows.Forms.Button mbtnFileSelect;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        DeconToolsV2.Readers.FileType menmFileType;

        private string mstrInFile;
        private System.Windows.Forms.Button mbtnAddSFolder;
        private System.Windows.Forms.Button mbtnOK;
        private System.Windows.Forms.Button mbtnAddMicromass;
        private System.Windows.Forms.Button mbtnCancel;

        public frmSelectRaw()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            try
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.mtxtFile = new System.Windows.Forms.TextBox();
            this.mbtnFileSelect = new System.Windows.Forms.Button();
            this.mbtnOK = new System.Windows.Forms.Button();
            this.mbtnAddSFolder = new System.Windows.Forms.Button();
            this.mbtnCancel = new System.Windows.Forms.Button();
            this.mbtnAddMicromass = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // label1
            //
            this.label1.Location = new System.Drawing.Point(8, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "File Name:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // mtxtFile
            //
            this.mtxtFile.Location = new System.Drawing.Point(88, 16);
            this.mtxtFile.Name = "mtxtFile";
            this.mtxtFile.Size = new System.Drawing.Size(344, 20);
            this.mtxtFile.TabIndex = 1;
            this.mtxtFile.Text = "";
            //
            // mbtnFileSelect
            //
            this.mbtnFileSelect.Location = new System.Drawing.Point(456, 16);
            this.mbtnFileSelect.Name = "mbtnFileSelect";
            this.mbtnFileSelect.Size = new System.Drawing.Size(24, 24);
            this.mbtnFileSelect.TabIndex = 2;
            this.mbtnFileSelect.Text = "..";
            this.mbtnFileSelect.Click += new System.EventHandler(this.mbtnFileSelect_Click);
            //
            // mbtnOK
            //
            this.mbtnOK.Location = new System.Drawing.Point(248, 56);
            this.mbtnOK.Name = "mbtnOK";
            this.mbtnOK.Size = new System.Drawing.Size(88, 24);
            this.mbtnOK.TabIndex = 6;
            this.mbtnOK.Text = "OK";
            this.mbtnOK.Click += new System.EventHandler(this.mbtnOK_Click);
            //
            // mbtnAddSFolder
            //
            this.mbtnAddSFolder.Location = new System.Drawing.Point(16, 56);
            this.mbtnAddSFolder.Name = "mbtnAddSFolder";
            this.mbtnAddSFolder.Size = new System.Drawing.Size(88, 24);
            this.mbtnAddSFolder.TabIndex = 10;
            this.mbtnAddSFolder.Text = "Add S Folder";
            this.mbtnAddSFolder.Click += new System.EventHandler(this.mbtnAddSFolder_Click);
            //
            // mbtnCancel
            //
            this.mbtnCancel.Location = new System.Drawing.Point(368, 56);
            this.mbtnCancel.Name = "mbtnCancel";
            this.mbtnCancel.Size = new System.Drawing.Size(88, 24);
            this.mbtnCancel.TabIndex = 11;
            this.mbtnCancel.Text = "Cancel";
            this.mbtnCancel.Click += new System.EventHandler(this.mbtnCancel_Click);
            //
            // mbtnAddMicromass
            //
            this.mbtnAddMicromass.Location = new System.Drawing.Point(120, 56);
            this.mbtnAddMicromass.Name = "mbtnAddMicromass";
            this.mbtnAddMicromass.Size = new System.Drawing.Size(96, 24);
            this.mbtnAddMicromass.TabIndex = 12;
            this.mbtnAddMicromass.Text = "Add Micromass";
            this.mbtnAddMicromass.Click += new System.EventHandler(this.mbtnAddMicromass_Click);
            //
            // frmSelectRaw
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(496, 94);
            this.Controls.Add(this.mbtnAddMicromass);
            this.Controls.Add(this.mbtnCancel);
            this.Controls.Add(this.mbtnAddSFolder);
            this.Controls.Add(this.mtxtFile);
            this.Controls.Add(this.mbtnOK);
            this.Controls.Add(this.mbtnFileSelect);
            this.Controls.Add(this.label1);
            this.Name = "frmSelectRaw";
            this.Text = "Select Raw File";
            this.ResumeLayout(false);
        }
        #endregion

#pragma warning disable 618

        private void mbtnFileSelect_Click(object sender, System.EventArgs e)
        {
            try
            {
                var openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "Xcalibur files (*.RAW)|*.RAW|Agilent files (*.wiff)|*.wiff|Micromass files (_FUNC*.DAT)|_FUNC*.DAT|PNNL IMF files (*.IMF)|*.IMF|Bruker files(acqu)|acqu|MZ XML files (*.mzxml)|*.mzXML|S files(*.*)|*.*|PNNL UIMF files (*.UIMF)|*.UIMF";
                openFileDialog1.FilterIndex = 1;
                openFileDialog1.Multiselect = true;
                openFileDialog1.RestoreDirectory = true;

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    var num_files = openFileDialog1.FileNames.Length;
                    for (var i = 0; i < num_files; i++)
                    {
                        var file_name = openFileDialog1.FileNames[i];
                        var index = file_name.LastIndexOf("\\");
                        var path_dir = "";

                        if (index > 0)
                        {
                            path_dir = file_name.Substring(0, index);
                        }

                        string outfile_name;
                        switch (openFileDialog1.FilterIndex)
                        {
                            case 1:
                                // Open Xcalibur File.
                                outfile_name = file_name.Substring(0, file_name.Length - 4);
                                mstrInFile = file_name;
                                menmFileType = DeconToolsV2.Readers.FileType.FINNIGAN;
                                break;
                            case 2:
                                // Open Agilent File
                                outfile_name = file_name.Substring(0, file_name.Length - 4);
                                mstrInFile = file_name;
                                menmFileType = DeconToolsV2.Readers.FileType.AGILENT_TOF;
                                break;
                            case 3:
                                // Open Micromass File
                                outfile_name = path_dir.Substring(0, path_dir.Length - 4);
                                mstrInFile = path_dir;
                                menmFileType = DeconToolsV2.Readers.FileType.MICROMASSRAWDATA;
                                break;
                            case 4:
                                // Open PNNL IMF File
                                outfile_name = file_name.Substring(0, file_name.Length - 4);
                                mstrInFile = file_name;
                                menmFileType = DeconToolsV2.Readers.FileType.PNNL_IMS;
                                break;
                            case 5:
                                // Open Bruker File
                                outfile_name = file_name.Substring(0, file_name.Length - 4);
                                mstrInFile = file_name;
                                menmFileType = DeconToolsV2.Readers.FileType.BRUKER;
                                break;
                            case 6:
                                // Open MZXML file
                                outfile_name = file_name;
                                mstrInFile = file_name;
                                menmFileType = DeconToolsV2.Readers.FileType.MZXMLRAWDATA;
                                break;
                            case 7:
                                // Open S file (ICR2LS format) file
                                outfile_name = file_name;
                                mstrInFile = file_name;
                                menmFileType = DeconToolsV2.Readers.FileType.ICR2LSRAWDATA;
                                break;
                            default:
                                break;
                        }
                    }
                    mtxtFile.Text = mstrInFile;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void mbtnAddSFolder_Click(object sender, System.EventArgs e)
        {
            var fld_browser = new FolderBrowserDialog();
            fld_browser.Description = "Please select S file folder";
            fld_browser.ShowNewFolderButton = false;
            var res = fld_browser.ShowDialog(this);
            if (res != DialogResult.OK)
                return;

            mstrInFile = fld_browser.SelectedPath;
            mtxtFile.Text = fld_browser.SelectedPath;
            menmFileType = DeconToolsV2.Readers.FileType.ICR2LSRAWDATA;
        }

        private void mbtnOK_Click(object sender, System.EventArgs e)
        {
            if (mstrInFile == null || mstrInFile == "")
            {
                if (mtxtFile.Text == "")
                {
                    MessageBox.Show("Please Select a File or press Cancel");
                    return;
                }
                else
                {
                    MessageBox.Show("Please Use buttons to select file");
                    return;
                }
            }
            if ((menmFileType != DeconToolsV2.Readers.FileType.ICR2LSRAWDATA
                    && menmFileType != DeconToolsV2.Readers.FileType.MICROMASSRAWDATA
                    && !System.IO.File.Exists(mstrInFile))
                || (menmFileType == DeconToolsV2.Readers.FileType.ICR2LSRAWDATA
                    && !System.IO.Directory.Exists(mstrInFile))
                || (menmFileType == DeconToolsV2.Readers.FileType.MICROMASSRAWDATA
                    && !System.IO.Directory.Exists(mstrInFile)))
            {
                MessageBox.Show("File specified does not exist. Please select a File or press Cancel");
                return;
            }

            DialogResult = DialogResult.OK;
            this.Hide();
        }

        private void mbtnCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Hide();
        }

        private void mbtnAddMicromass_Click(object sender, System.EventArgs e)
        {
            var fld_browser = new FolderBrowserDialog();
            fld_browser.Description = "Please select Micromass .raw folder";
            fld_browser.ShowNewFolderButton = false;
            var res = fld_browser.ShowDialog(this);
            if (res != DialogResult.OK)
                return;

            mstrInFile = fld_browser.SelectedPath;
            mtxtFile.Text = fld_browser.SelectedPath;
            menmFileType = DeconToolsV2.Readers.FileType.MICROMASSRAWDATA;
        }
#pragma warning restore 618

        public DeconToolsV2.Readers.FileType FileType
        {
            get
            {
                return menmFileType;
            }
            set
            {
                menmFileType = value;
            }
        }

        public string FileName
        {
            get
            {
                return mstrInFile;
            }
            set
            {
                mstrInFile = value;
            }
        }
    }
}
