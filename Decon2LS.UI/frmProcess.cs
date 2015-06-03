// Written by Navdeep Jaitly for the Department of Energy (PNNL, Richland, WA)
// Copyright 2006, Battelle Memorial Institute
// E-mail: navdeep.jaitly@pnl.gov
// Website: http://ncrr.pnl.gov/software
// -------------------------------------------------------------------------------
// 
// Licensed under the Apache License, Version 2.0; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data ; 
using System.Threading ; 
using PRISM;

namespace Decon2LS
{
	/// <summary>
	/// Summary description for frmProcess.
	/// </summary>
	public class frmProcess : System.Windows.Forms.Form
	{
		enum enmProcessType { DECON, TIC, RAW2D, DTA } ;
		private enmProcessType menm_process_type ;
		private System.Windows.Forms.Label mlabel_process_type;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button mbtn_file_open;
		private System.Windows.Forms.Button mbtn_add_files;
		private System.Windows.Forms.Button mbtn_process;
		private System.Windows.Forms.Button mbtn_cancel;
		private clsCfgParms mobj_config ; 

		public string mstr_param_file_name ;
		private System.Windows.Forms.ComboBox mcmb_process_type;
		private System.Windows.Forms.TextBox mtxt_param_file; 
		private bool mbln_processing = false ; 
		private Thread mthrd_process = null ; 
		private PNNL.Controls.ArrayChartDataProvider data_provider ;

		private string mstr_current_file_name ; 
		private DeconToolsV2.Readers.FileType menm_current_file_type ; 
		private string mstr_current_out_file_name ; 
		private System.Windows.Forms.Button mbtn_add_sfolder;
		private System.Windows.Forms.DataGrid mdatagrid_files;
		private System.Windows.Forms.Button mbtn_display; 
		private EventHandler mStatusUpdateDelegate = null;

		private DeconToolsV2.clsProcRunner mobj_proc_runner ;
		private DeconToolsV2.HornTransform.clsHornTransformParameters mobj_transform_parameters ; 
		private DeconToolsV2.Readers.clsRawDataPreprocessOptions mobj_fticr_preprocess_parameters ; 
		private DeconToolsV2.DTAGeneration.clsDTAGenerationParameters mobj_dta_parameters ; 

		private DeconToolsV2.Peaks.clsPeakProcessorParameters mobj_peak_parameters ; 
		private int mint_current_scan_in_preview = -1 ; 

		public DeconToolsV2.HornTransform.clsHornTransformParameters MassTransformParameters
		{
			get
			{
				return mobj_transform_parameters ; 
			}
			set
			{
				mobj_transform_parameters = value ; 
			}
		} 

		public DeconToolsV2.Peaks.clsPeakProcessorParameters PeakProcessorParameters
		{
			get
			{
				return mobj_peak_parameters ; 
			}
			set
			{
				mobj_peak_parameters = value ; 
			}
		}

		public DeconToolsV2.Readers.clsRawDataPreprocessOptions FTICRPreProcessParameters
		{
			get
			{
				return mobj_fticr_preprocess_parameters ; 
			}
			set
			{
				mobj_fticr_preprocess_parameters = value ; 
			}
		}

		public DeconToolsV2.DTAGeneration.clsDTAGenerationParameters DTAGenerationParameters
		{
			get
			{
				return mobj_dta_parameters ; 
			}
			set
			{
				mobj_dta_parameters = value ; 
			}
		}






		/// <summary>
		/// Object for reading a raw data for showing status update figures 
		/// when performing a deisotoping.
		/// </summary>
		private DeconToolsV2.Readers.clsRawData mobj_raw_data ; 

		private bool mbln_display_on = false ;
		private int mint_display_panel_default_height = 250 ; 
		private System.Windows.Forms.Panel mpanel_display;
		private System.Windows.Forms.Timer mStatusTimer;
		private System.Windows.Forms.Panel panelProcType;
		private System.Windows.Forms.Panel panelParamFile;
		private System.Windows.Forms.Panel panelButtons;
		private System.Windows.Forms.Panel panelMain;
		private System.Windows.Forms.Splitter splitter1;
//		private PNNL.Controls.ctlLineChart mctl_spectra;
		private System.Windows.Forms.Panel panelPBar;
		private System.Windows.Forms.ProgressBar mpbar_process;
		private System.Windows.Forms.Button mbtn_add_imsfolder;
		private PNNL.Controls.MS.ctlSpectrum mctl_spectra;
		private System.Windows.Forms.CheckBox chkUseParameterFile;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.ComponentModel.IContainer components;

		public frmProcess(ref clsCfgParms config)
		{
			try
			{
				InitializeComponent();
				mobj_config = config ; 
				SetDataTable() ; 
				mobj_proc_runner = new DeconToolsV2.clsProcRunner() ; 
				mbln_processing = false ; 
				mbln_display_on = false ; 
				if (this.Height > mpanel_display.Height)
					this.Height = this.Height - mpanel_display.Height ; 
				mpanel_display.Height = 0 ; 
				mobj_raw_data = new DeconToolsV2.Readers.clsRawData() ; 
				mobj_transform_parameters = new DeconToolsV2.HornTransform.clsHornTransformParameters() ; 
				mobj_peak_parameters = new DeconToolsV2.Peaks.clsPeakProcessorParameters() ; 
				mobj_dta_parameters = new DeconToolsV2.DTAGeneration.clsDTAGenerationParameters() ; 

				mcmb_process_type.SelectedIndex=0;


				loadSettings();

			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message + ex.StackTrace) ;
			}
		}

		private void loadSettings()
		{
//			this.mtxt_param_file.Text=System.Configuration.ConfigurationSettings.AppSettings.Get("frmProcessUserParamFile");


		}
		private void saveSettings()
		{
			//this doesn't work in .net 1.1;  have to build own saving class;
//		if (this.mtxt_param_file.Text!=null && this.mtxt_param_file.Text.Length>0)
//		{
//			System.Configuration.ConfigurationSettings.AppSettings.Set("frmProcessUserParamFile",this.mtxt_param_file.Text);
//
//		}

		}



		private void SetDataTable()
		{
			try
			{
				DataGridTableStyle tb_style = new DataGridTableStyle() ; 
				tb_style.AlternatingBackColor = Color.LightGray ; 

				DataGridTextBoxColumn filename_column = new DataGridTextBoxColumn() ; 
				filename_column.Width = mdatagrid_files.Width/3 ;
				filename_column.HeaderText = "Filename" ;

				DataGridTextBoxColumn outfilename_column = new DataGridTextBoxColumn() ; 
				outfilename_column.Width = mdatagrid_files.Width/3 ;
				outfilename_column.HeaderText = "Output File" ;

				DataGridTextBoxColumn filetype_column = new DataGridTextBoxColumn() ; 
				filetype_column.Width = mdatagrid_files.Width - 10 - filename_column.Width - outfilename_column.Width ; 
				filetype_column.HeaderText = "File type" ;


				DataTable table = new DataTable() ; 
				table.Columns.Add("Filename"); 
				table.Columns.Add("Output Filename") ; 
				table.Columns.Add("Type") ; 
				table.TableName = "Files" ; 

				mdatagrid_files.DataSource = table ; 
				tb_style.MappingName = table.TableName ; 

				filename_column.MappingName = table.Columns[0].ColumnName ; 
				outfilename_column.MappingName = table.Columns[1].ColumnName ; 
				filetype_column.MappingName = table.Columns[2].ColumnName ; 

				tb_style.GridColumnStyles.Add(filename_column) ; 
				tb_style.GridColumnStyles.Add(outfilename_column) ; 
				tb_style.GridColumnStyles.Add(filetype_column) ; 
				mdatagrid_files.TableStyles.Add(tb_style) ; 
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message + ex.StackTrace) ;
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			saveSettings();
			
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            PNNL.Controls.PenProvider penProvider1 = new PNNL.Controls.PenProvider();
            PNNL.Controls.PenProvider penProvider2 = new PNNL.Controls.PenProvider();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmProcess));
            this.panelProcType = new System.Windows.Forms.Panel();
            this.mcmb_process_type = new System.Windows.Forms.ComboBox();
            this.mlabel_process_type = new System.Windows.Forms.Label();
            this.panelParamFile = new System.Windows.Forms.Panel();
            this.chkUseParameterFile = new System.Windows.Forms.CheckBox();
            this.mbtn_file_open = new System.Windows.Forms.Button();
            this.mtxt_param_file = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.mbtn_add_imsfolder = new System.Windows.Forms.Button();
            this.mbtn_display = new System.Windows.Forms.Button();
            this.mbtn_add_sfolder = new System.Windows.Forms.Button();
            this.mbtn_cancel = new System.Windows.Forms.Button();
            this.mbtn_process = new System.Windows.Forms.Button();
            this.mbtn_add_files = new System.Windows.Forms.Button();
            this.panelMain = new System.Windows.Forms.Panel();
            this.mdatagrid_files = new System.Windows.Forms.DataGrid();
            this.panelPBar = new System.Windows.Forms.Panel();
            this.mpbar_process = new System.Windows.Forms.ProgressBar();
            this.mpanel_display = new System.Windows.Forms.Panel();
            this.mctl_spectra = new PNNL.Controls.MS.ctlSpectrum();
            this.mStatusTimer = new System.Windows.Forms.Timer(this.components);
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panelProcType.SuspendLayout();
            this.panelParamFile.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mdatagrid_files)).BeginInit();
            this.panelPBar.SuspendLayout();
            this.mpanel_display.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mctl_spectra)).BeginInit();
            this.SuspendLayout();
            // 
            // panelProcType
            // 
            this.panelProcType.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panelProcType.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelProcType.Controls.Add(this.mcmb_process_type);
            this.panelProcType.Controls.Add(this.mlabel_process_type);
            this.panelProcType.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelProcType.Location = new System.Drawing.Point(0, 0);
            this.panelProcType.Name = "panelProcType";
            this.panelProcType.Padding = new System.Windows.Forms.Padding(15, 45, 15, 30);
            this.panelProcType.Size = new System.Drawing.Size(624, 55);
            this.panelProcType.TabIndex = 0;
            // 
            // mcmb_process_type
            // 
            this.mcmb_process_type.Cursor = System.Windows.Forms.Cursors.Default;
            this.mcmb_process_type.Items.AddRange(new object[] {
            "Horn Mass Transform",
            "Tic Generation",
            "2D Rawdata Plot",
            "DTA Generation"});
            this.mcmb_process_type.Location = new System.Drawing.Point(144, 6);
            this.mcmb_process_type.Name = "mcmb_process_type";
            this.mcmb_process_type.Size = new System.Drawing.Size(230, 24);
            this.mcmb_process_type.TabIndex = 1;
            this.mcmb_process_type.SelectedIndexChanged += new System.EventHandler(this.mcmb_process_type_SelectedIndexChanged);
            // 
            // mlabel_process_type
            // 
            this.mlabel_process_type.Location = new System.Drawing.Point(10, 9);
            this.mlabel_process_type.Name = "mlabel_process_type";
            this.mlabel_process_type.Size = new System.Drawing.Size(201, 28);
            this.mlabel_process_type.TabIndex = 0;
            this.mlabel_process_type.Text = "Process Type:";
            // 
            // panelParamFile
            // 
            this.panelParamFile.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panelParamFile.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelParamFile.Controls.Add(this.chkUseParameterFile);
            this.panelParamFile.Controls.Add(this.mbtn_file_open);
            this.panelParamFile.Controls.Add(this.mtxt_param_file);
            this.panelParamFile.Controls.Add(this.label1);
            this.panelParamFile.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelParamFile.Location = new System.Drawing.Point(0, 0);
            this.panelParamFile.Name = "panelParamFile";
            this.panelParamFile.Padding = new System.Windows.Forms.Padding(15, 100, 15, 0);
            this.panelParamFile.Size = new System.Drawing.Size(624, 55);
            this.panelParamFile.TabIndex = 1;
            // 
            // chkUseParameterFile
            // 
            this.chkUseParameterFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkUseParameterFile.Location = new System.Drawing.Point(19, 10);
            this.chkUseParameterFile.Name = "chkUseParameterFile";
            this.chkUseParameterFile.Size = new System.Drawing.Size(163, 28);
            this.chkUseParameterFile.TabIndex = 3;
            this.chkUseParameterFile.Text = "Use Parameter File";
            this.chkUseParameterFile.CheckedChanged += new System.EventHandler(this.chkUseParameterFile_CheckedChanged);
            // 
            // mbtn_file_open
            // 
            this.mbtn_file_open.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.mbtn_file_open.Location = new System.Drawing.Point(706, 13);
            this.mbtn_file_open.Name = "mbtn_file_open";
            this.mbtn_file_open.Size = new System.Drawing.Size(28, 23);
            this.mbtn_file_open.TabIndex = 2;
            this.mbtn_file_open.Text = "..";
            this.mbtn_file_open.Click += new System.EventHandler(this.mbtn_file_open_Click);
            // 
            // mtxt_param_file
            // 
            this.mtxt_param_file.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mtxt_param_file.Location = new System.Drawing.Point(298, 13);
            this.mtxt_param_file.Name = "mtxt_param_file";
            this.mtxt_param_file.Size = new System.Drawing.Size(394, 22);
            this.mtxt_param_file.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(192, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 28);
            this.label1.TabIndex = 0;
            this.label1.Text = "Parameter File:";
            // 
            // panelButtons
            // 
            this.panelButtons.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panelButtons.Controls.Add(this.mbtn_add_imsfolder);
            this.panelButtons.Controls.Add(this.mbtn_display);
            this.panelButtons.Controls.Add(this.mbtn_add_sfolder);
            this.panelButtons.Controls.Add(this.mbtn_cancel);
            this.panelButtons.Controls.Add(this.mbtn_process);
            this.panelButtons.Controls.Add(this.mbtn_add_files);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 294);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(624, 46);
            this.panelButtons.TabIndex = 2;
            // 
            // mbtn_add_imsfolder
            // 
            this.mbtn_add_imsfolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.mbtn_add_imsfolder.Location = new System.Drawing.Point(230, 9);
            this.mbtn_add_imsfolder.Name = "mbtn_add_imsfolder";
            this.mbtn_add_imsfolder.Size = new System.Drawing.Size(116, 28);
            this.mbtn_add_imsfolder.TabIndex = 5;
            this.mbtn_add_imsfolder.Text = "Add IMS folder";
            this.mbtn_add_imsfolder.Click += new System.EventHandler(this.mbtn_add_imsfolder_Click);
            // 
            // mbtn_display
            // 
            this.mbtn_display.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.mbtn_display.Location = new System.Drawing.Point(576, 9);
            this.mbtn_display.Name = "mbtn_display";
            this.mbtn_display.Size = new System.Drawing.Size(86, 28);
            this.mbtn_display.TabIndex = 4;
            this.mbtn_display.Text = "Display On";
            this.mbtn_display.Click += new System.EventHandler(this.mbtn_display_Click);
            // 
            // mbtn_add_sfolder
            // 
            this.mbtn_add_sfolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.mbtn_add_sfolder.Location = new System.Drawing.Point(125, 9);
            this.mbtn_add_sfolder.Name = "mbtn_add_sfolder";
            this.mbtn_add_sfolder.Size = new System.Drawing.Size(96, 28);
            this.mbtn_add_sfolder.TabIndex = 3;
            this.mbtn_add_sfolder.Text = "Add Sfolder";
            this.mbtn_add_sfolder.Click += new System.EventHandler(this.mbtn_add_sfolder_Click);
            // 
            // mbtn_cancel
            // 
            this.mbtn_cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.mbtn_cancel.Location = new System.Drawing.Point(470, 9);
            this.mbtn_cancel.Name = "mbtn_cancel";
            this.mbtn_cancel.Size = new System.Drawing.Size(87, 28);
            this.mbtn_cancel.TabIndex = 2;
            this.mbtn_cancel.Text = "Cancel";
            this.mbtn_cancel.Click += new System.EventHandler(this.mbtn_cancel_Click);
            // 
            // mbtn_process
            // 
            this.mbtn_process.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.mbtn_process.Location = new System.Drawing.Point(365, 9);
            this.mbtn_process.Name = "mbtn_process";
            this.mbtn_process.Size = new System.Drawing.Size(86, 28);
            this.mbtn_process.TabIndex = 1;
            this.mbtn_process.Text = "Process";
            this.mbtn_process.Click += new System.EventHandler(this.mbtn_process_Click);
            // 
            // mbtn_add_files
            // 
            this.mbtn_add_files.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.mbtn_add_files.Location = new System.Drawing.Point(19, 9);
            this.mbtn_add_files.Name = "mbtn_add_files";
            this.mbtn_add_files.Size = new System.Drawing.Size(87, 28);
            this.mbtn_add_files.TabIndex = 0;
            this.mbtn_add_files.Text = "Add Files";
            this.mbtn_add_files.Click += new System.EventHandler(this.mbtn_add_files_Click);
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.mdatagrid_files);
            this.panelMain.Controls.Add(this.panelPBar);
            this.panelMain.Controls.Add(this.panelParamFile);
            this.panelMain.Controls.Add(this.panelButtons);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 55);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(624, 340);
            this.panelMain.TabIndex = 3;
            // 
            // mdatagrid_files
            // 
            this.mdatagrid_files.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.mdatagrid_files.DataMember = "";
            this.mdatagrid_files.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mdatagrid_files.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.mdatagrid_files.Location = new System.Drawing.Point(0, 55);
            this.mdatagrid_files.Name = "mdatagrid_files";
            this.mdatagrid_files.Size = new System.Drawing.Size(624, 211);
            this.mdatagrid_files.TabIndex = 1;
            // 
            // panelPBar
            // 
            this.panelPBar.Controls.Add(this.mpbar_process);
            this.panelPBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelPBar.Location = new System.Drawing.Point(0, 266);
            this.panelPBar.Name = "panelPBar";
            this.panelPBar.Size = new System.Drawing.Size(624, 28);
            this.panelPBar.TabIndex = 3;
            // 
            // mpbar_process
            // 
            this.mpbar_process.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mpbar_process.Location = new System.Drawing.Point(0, 0);
            this.mpbar_process.Name = "mpbar_process";
            this.mpbar_process.Size = new System.Drawing.Size(624, 28);
            this.mpbar_process.TabIndex = 0;
            this.mpbar_process.Click += new System.EventHandler(this.mpbar_process_Click);
            // 
            // mpanel_display
            // 
            this.mpanel_display.Controls.Add(this.mctl_spectra);
            this.mpanel_display.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.mpanel_display.Location = new System.Drawing.Point(0, 395);
            this.mpanel_display.Name = "mpanel_display";
            this.mpanel_display.Size = new System.Drawing.Size(624, 259);
            this.mpanel_display.TabIndex = 4;
            // 
            // mctl_spectra
            // 
            this.mctl_spectra.AutoViewPortXAxis = true;
            this.mctl_spectra.AutoViewPortXBase = 0F;
            this.mctl_spectra.AutoViewPortYAxis = true;
            this.mctl_spectra.AutoViewPortYBase = 0F;
            this.mctl_spectra.AxisAndLabelFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.mctl_spectra.AxisAndLabelMaxFontSize = 15;
            this.mctl_spectra.AxisAndLabelMinFontSize = 8;
            this.mctl_spectra.BackColor = System.Drawing.Color.White;
            this.mctl_spectra.ChartBackgroundColor = System.Drawing.Color.White;
            this.mctl_spectra.ChartLayout.LegendFraction = 0.2F;
            this.mctl_spectra.ChartLayout.LegendLocation = PNNL.Controls.ChartLegendLocation.Right;
            this.mctl_spectra.ChartLayout.MaxLegendHeight = 150;
            this.mctl_spectra.ChartLayout.MaxLegendWidth = 250;
            this.mctl_spectra.ChartLayout.MaxTitleHeight = 50;
            this.mctl_spectra.ChartLayout.MinLegendHeight = 50;
            this.mctl_spectra.ChartLayout.MinLegendWidth = 75;
            this.mctl_spectra.ChartLayout.MinTitleHeight = 5;
            this.mctl_spectra.ChartLayout.TitleFraction = 0.1F;
            this.mctl_spectra.DefaultZoomHandler.Active = true;
            this.mctl_spectra.DefaultZoomHandler.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(119)))), ((int)(((byte)(136)))), ((int)(((byte)(153)))));
            this.mctl_spectra.DefaultZoomHandler.LineColor = System.Drawing.Color.Black;
            this.mctl_spectra.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mctl_spectra.FWHMFont = new System.Drawing.Font("Times New Roman", 8F);
            this.mctl_spectra.FWHMLineWidth = 1F;
            this.mctl_spectra.FWHMPeakColor = System.Drawing.Color.Purple;
            penProvider1.Color = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(211)))), ((int)(((byte)(211)))));
            penProvider1.Width = 1F;
            this.mctl_spectra.GridLinePen = penProvider1;
            this.mctl_spectra.HasLegend = false;
            this.mctl_spectra.HilightColor = System.Drawing.Color.Magenta;
            this.mctl_spectra.LabelOffset = 8F;
            this.mctl_spectra.Legend.BackColor = System.Drawing.Color.Transparent;
            penProvider2.Color = System.Drawing.Color.Black;
            penProvider2.Width = 1F;
            this.mctl_spectra.Legend.BorderPen = penProvider2;
            this.mctl_spectra.Legend.Bounds = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.mctl_spectra.Legend.ColumnWidth = 125;
            this.mctl_spectra.Legend.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.mctl_spectra.Legend.MaxFontSize = 12F;
            this.mctl_spectra.Legend.MinFontSize = 6F;
            this.mctl_spectra.LineWidth = 1F;
            this.mctl_spectra.Location = new System.Drawing.Point(0, 0);
            this.mctl_spectra.Margins.BottomMarginFraction = 0.1F;
            this.mctl_spectra.Margins.BottomMarginMax = 72;
            this.mctl_spectra.Margins.BottomMarginMin = 30;
            this.mctl_spectra.Margins.DefaultMarginFraction = 0.05F;
            this.mctl_spectra.Margins.DefaultMarginMax = 15;
            this.mctl_spectra.Margins.DefaultMarginMin = 5;
            this.mctl_spectra.Margins.LeftMarginFraction = 0.2F;
            this.mctl_spectra.Margins.LeftMarginMax = 150;
            this.mctl_spectra.Margins.LeftMarginMin = 72;
            this.mctl_spectra.MarkerSize = 5;
            this.mctl_spectra.MinPixelForFWHM = 5F;
            this.mctl_spectra.Name = "mctl_spectra";
            this.mctl_spectra.NumFWHM = 1F;
            this.mctl_spectra.NumXBins = 20;
            this.mctl_spectra.PeakColor = System.Drawing.Color.DarkGray;
            this.mctl_spectra.PeakLabelRelativeHeightPercent = 5F;
            this.mctl_spectra.PeakLineEndCap = System.Drawing.Drawing2D.LineCap.Flat;
            this.mctl_spectra.Size = new System.Drawing.Size(624, 259);
            this.mctl_spectra.TabIndex = 0;
            this.mctl_spectra.Title = "Mass Spectrum";
            this.mctl_spectra.TitleFont = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
            this.mctl_spectra.TitleMaxFontSize = 50F;
            this.mctl_spectra.TitleMinFontSize = 6F;
            this.mctl_spectra.VerticalExpansion = 1F;
            this.mctl_spectra.ViewPort = ((System.Drawing.RectangleF)(resources.GetObject("mctl_spectra.ViewPort")));
            this.mctl_spectra.XAxisGridLines = false;
            this.mctl_spectra.XAxisLabel = "m/z";
            this.mctl_spectra.YAxisGridLines = false;
            this.mctl_spectra.YAxisLabel = "intensity";
            // 
            // mStatusTimer
            // 
            this.mStatusTimer.Interval = 1000;
            // 
            // splitter1
            // 
            this.splitter1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter1.Location = new System.Drawing.Point(0, 388);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(624, 7);
            this.splitter1.TabIndex = 5;
            this.splitter1.TabStop = false;
            // 
            // frmProcess
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(624, 654);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.panelProcType);
            this.Controls.Add(this.mpanel_display);
            this.Name = "frmProcess";
            this.Text = "Process";
            this.Load += new System.EventHandler(this.frmProcess_Load);
            this.panelProcType.ResumeLayout(false);
            this.panelParamFile.ResumeLayout(false);
            this.panelParamFile.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            this.panelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mdatagrid_files)).EndInit();
            this.panelPBar.ResumeLayout(false);
            this.mpanel_display.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mctl_spectra)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		private void mbtn_file_open_Click(object sender, System.EventArgs e)
		{
			try
			{
				OpenFileDialog openFileDialog1 = new OpenFileDialog();
				openFileDialog1.Filter = "Parameter files (*.xml)|*.xml" ;
				openFileDialog1.FilterIndex = 1 ;
				openFileDialog1.RestoreDirectory = true ;
				openFileDialog1.InitialDirectory = mobj_config.OpenDir ; 

				if(openFileDialog1.ShowDialog() == DialogResult.OK)
				{
					mstr_param_file_name = openFileDialog1.FileName ; 
					this.mtxt_param_file.Text = mstr_param_file_name ; 
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

		private void mbtn_add_files_Click(object sender, System.EventArgs e)
		{
			try
			{
				OpenFileDialog openFileDialog1 = new OpenFileDialog();
				openFileDialog1.Filter = "Xcalibur files (*.RAW)|*.RAW|Agilent files (*.wiff)|*.wiff|Micromass files (_FUNC*.DAT)|_FUNC*.DAT|PNNL IMF files (*.IMF)|*.IMF|PNNL UIMF files (*.UIMF)|*.UIMF|Bruker files(acqu)|acqu|MZ XML files (*.mzxml)|*.mzXML|S files(*.*)|*.*" ;
				openFileDialog1.FilterIndex = 1 ;
				openFileDialog1.Multiselect = true ; 
				openFileDialog1.RestoreDirectory = true ;
				openFileDialog1.InitialDirectory = mobj_config.OpenDir ; 

				if(openFileDialog1.ShowDialog() == DialogResult.OK)
				{
					DataTable table = (DataTable) mdatagrid_files.DataSource ; 
					int num_files = openFileDialog1.FileNames.Length ; 
					for (int i = 0 ; i < num_files ; i++)
					{
						string file_name = openFileDialog1.FileNames[i] ; 
						int index = file_name.LastIndexOf("\\") ;
						string path_dir = "" ; 

						if (index > 0)
						{
							path_dir = file_name.Substring(0, index) ; 
							mobj_config.OpenDir = path_dir ; 
						}

						DataRow row = table.NewRow() ; 
						string outfile_name ; 
						string file_dir = System.IO.Path.GetDirectoryName(file_name) ; 

						switch (openFileDialog1.FilterIndex)
						{
							case 1:
								// Open Xcalibur File.
								outfile_name = file_name.Substring(0, file_name.Length - 4) ;
								row[0] = file_name ; 
								row[1] = outfile_name ; 
								row[2] = DeconToolsV2.Readers.FileType.FINNIGAN.ToString() ; 
								break ; 
							case 2:
								// Open Agilent File
								outfile_name = file_name.Substring(0, file_name.Length - 4) ;
								row[0] = file_name ; 
								row[1] = outfile_name ; 
								row[2] = DeconToolsV2.Readers.FileType.AGILENT_TOF.ToString() ; 
								break ; 
							case 3:
								// Open Micromass File
								outfile_name = path_dir.Substring(0, path_dir.Length - 4) ;
								row[0] = path_dir ; 
								row[1] = path_dir ; 
								row[2] = DeconToolsV2.Readers.FileType.MICROMASSRAWDATA.ToString() ; 
								break ; 
							case 4:
								// Open PNNL IMF File
								outfile_name = file_name.Substring(0, file_name.Length - 4) ;
								row[0] = file_name ; 
								row[1] = outfile_name ; 
								row[2] = DeconToolsV2.Readers.FileType.PNNL_IMS.ToString() ; 
								break ; 
							case 5:
								// Open PNNL UIMF File
								outfile_name = file_name.Substring(0, file_name.Length - 4) ;
								row[0] = file_name ; 
								row[1] = outfile_name ; 
								row[2] = DeconToolsV2.Readers.FileType.PNNL_UIMF.ToString() ; 
								break ; 
							case 6:
								// Open Bruker File
								outfile_name = file_dir ;
								row[0] = file_dir ; 
								row[1] = outfile_name ; 
								row[2] = DeconToolsV2.Readers.FileType.BRUKER.ToString() ; 
								break ; 
							case 7:
								// Open MZXML file
								outfile_name = file_name ;
								row[0] = file_name ; 
								row[1] = file_name ; 
								row[2] = DeconToolsV2.Readers.FileType.MZXMLRAWDATA.ToString() ; 
								break ; 
							case 8:
								// Open S file (ICR2LS format) file
								outfile_name = file_name ;
								row[0] = file_name ; 
								row[1] = file_name ; 
								row[2] = DeconToolsV2.Readers.FileType.ICR2LSRAWDATA.ToString() ; 
								break ; 
							default:
								break ; 
						}
						table.Rows.Add(row) ; 
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString()) ; 
			}
		}

		
	

		private void mbtn_add_imsfolder_Click(object sender, System.EventArgs e)
		{
			
			int num_files_loaded ; 
			PRISM.Files.FolderBrowser fb=new PRISM.Files.FolderBrowser();
			
			if (!fb.BrowseForFolder(""))
			{
				return;
			}
			else
			{
				System.IO.DirectoryInfo ims_dir = new System.IO.DirectoryInfo(fb.FolderPath) ;
				num_files_loaded = LoadIMFFilesIntoTable(ims_dir) ; 				
				if (num_files_loaded == 0 )
				{
					System.IO.DirectoryInfo[] ims_all_dirs  = ims_dir.GetDirectories() ; 
					for (int dir_num = 0 ; dir_num < ims_all_dirs.Length ; dir_num++)
					{
						ims_dir  = ims_all_dirs[dir_num] ; 
						num_files_loaded = LoadIMFFilesIntoTable(ims_dir) ; 
					}
				}


			}
//			
//			try
//			{
//				FolderBrowserDialog fld_browser = new FolderBrowserDialog()  ; 
//				fld_browser.ShowNewFolderButton = false ; 
//				DialogResult res = fld_browser.ShowDialog(this) ; 
//				int num_files_loaded ; 
//				if (res != DialogResult.OK)
//					return ; 
//
//				
//				System.IO.DirectoryInfo ims_dir = new System.IO.DirectoryInfo(fld_browser.SelectedPath) ;
//				num_files_loaded = LoadIMFFilesIntoTable(ims_dir) ; 				
//				if (num_files_loaded == 0 )
//				{
//					System.IO.DirectoryInfo[] ims_all_dirs  = ims_dir.GetDirectories() ; 
//					for (int dir_num = 0 ; dir_num < ims_all_dirs.Length ; dir_num++)
//					{
//						ims_dir  = ims_all_dirs[dir_num] ; 
//						num_files_loaded = LoadIMFFilesIntoTable(ims_dir) ; 
//					}
//				}
//			}
//			
//			catch (Exception ex)
//			{
//				MessageBox.Show(this, ex.Message + ex.StackTrace) ;
//			}			
		}

		private int LoadIMFFilesIntoTable(System.IO.DirectoryInfo path_ims_dir)
		{
			//loads all imf files into datagrid, return num of imf_files loaded
			try
			{
				System.IO.FileInfo[] ims_files;		
				System.IO.FileSystemInfo ims_single_file ; 
				ims_files = path_ims_dir.GetFiles() ; 

				DataTable table = (DataTable) mdatagrid_files.DataSource ; 
				int num_files = ims_files.Length ; 
				int num_imf_files =  0 ; 
					
				for (int i = 0 ; i < num_files ; i++)
				{						
					ims_single_file = ims_files[i] ; 
					if (ims_single_file.Extension.ToUpper() == ".IMF")
					{
						string file_name = ims_single_file.FullName;
						num_imf_files++ ; 
						DataRow row = table.NewRow() ; 
						string outfile_name ; 
						// Open PNNL IMF File
						outfile_name = file_name.Substring(0, file_name.Length - 4) ;
						row[0] = file_name ; 
						row[1] = outfile_name ; 
						row[2] = DeconToolsV2.Readers.FileType.PNNL_IMS.ToString() ; 
						table.Rows.Add(row) ; 
					}
				}

				return num_imf_files ; 
			}
			catch(Exception ex)
			{
				MessageBox.Show(this, ex.Message + ex.StackTrace) ;
				return 0 ; 
			}	
		}


		
		private void mbtn_add_sfolder_Click(object sender, System.EventArgs e)
		{
			try
			{
				add_S_Folder();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error selecting folder\n\nDetails:\n"+ex.Message);

			}

//			try
//			{
//				
//				
//				FolderBrowserDialog fld_browser = new FolderBrowserDialog()  ; 
//				fld_browser.SelectedPath = "c:\\projects\\decon2ls\\data\\" ; 
//				fld_browser.ShowNewFolderButton = false ; 
//				DialogResult res = fld_browser.ShowDialog(this) ; 
//				if (res != DialogResult.OK)
//					return ; 
//
//				DataTable table = (DataTable) mdatagrid_files.DataSource ; 
//				DataRow row = table.NewRow() ; 
//				row[0] = fld_browser.SelectedPath ; 
//				row[1] = fld_browser.SelectedPath ; 
//				row[2] = DeconToolsV2.Readers.FileType.ICR2LSRAWDATA.ToString() ; 
//				table.Rows.Add(row) ; 
//			}
//			catch (Exception ex)
//			{
//				MessageBox.Show(this, ex.Message + ex.StackTrace) ;
//			}
		}

		private void FinishOrAbortProcessing(bool abort) 
		{
			try
			{
				if (mbln_processing)
				{
					this.mStatusTimer.Stop();
					// doing its business.. someone wants to abort. 
					if (mthrd_process != null && mthrd_process.IsAlive)
					{
						if (abort) 
						{
							mthrd_process.Abort();
						}
						// wait until it to fully terminate or abort
						mthrd_process.Join();
					}
					if (mobj_proc_runner != null)
					{
						mobj_proc_runner.Reset() ; 
					}
					mbln_processing = false ;
					mbtn_process.Text = "Process" ;
					mpbar_process.Value = 0;
					this.mdatagrid_files.ReadOnly = false;
					this.mtxt_param_file.ReadOnly = false;					
					return ;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message + ex.StackTrace) ;
			}
		}

		private void mbtn_process_Click(object sender, System.EventArgs e)
		{
			
			try
			{
				if (this.chkUseParameterFile.Checked)
				{
					if(validateParameterFile(this.mtxt_param_file.Text))
					{
						updateCurrentParameters(this.mtxt_param_file.Text);
					}
					else
					{
						MessageBox.Show("Parameter file doesn't exist - check the file path\n\n" + this.mtxt_param_file.Text);
						return;

					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				return;

			}


		

			
			
			try
			{
				if (this.mbln_processing) 
				{
					this.FinishOrAbortProcessing(true);
					return;
				}

				if (mcmb_process_type.SelectedIndex < 0)
				{
					MessageBox.Show("Please select the type of processing you would like to perform") ; 
					return ; 
				}
				switch (mcmb_process_type.SelectedIndex)
				{
					case 0:
						menm_process_type = enmProcessType.DECON ; 
						break ; 
					case 1:
						menm_process_type = enmProcessType.TIC ; 
						break ; 
					case 2:
						menm_process_type = enmProcessType.RAW2D ; 
						break ; 
					case 3: 
						menm_process_type = enmProcessType.DTA;
						break;
					default:
						break ; 
				}

				mbln_processing = true ; 
				mbtn_process.Text = "Abort" ; 
				ProcessFiles();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message + ex.StackTrace) ;
			}
		}

		private void ProcessFiles()
		{
			try
			{
				
			
				
				this.mctl_spectra.SeriesCollection.Clear();

				data_provider = new PNNL.Controls.ArrayChartDataProvider();
				PNNL.Controls.DiamondShape shape = new PNNL.Controls.DiamondShape(3, false) ; 
				PNNL.Controls.clsPlotParams plt_params = new PNNL.Controls.clsPlotParams(shape, Color.Red) ; 
				PNNL.Controls.clsSeries series = new PNNL.Controls.clsSeries(data_provider, plt_params);
				this.mctl_spectra.SeriesCollection.Add(series);

				DataTable table = (DataTable) this.mdatagrid_files.DataSource ; 
				this.mdatagrid_files.ReadOnly = true;
				this.mtxt_param_file.ReadOnly = true;

				if (this.mStatusUpdateDelegate != null) 
				{
					this.mStatusTimer.Tick -= this.mStatusUpdateDelegate;
				}

				switch (menm_process_type)
				{
					case enmProcessType.DECON:
						this.mStatusUpdateDelegate = new EventHandler(this.CheckDeconStatusHandler);
						break;
					case enmProcessType.TIC:
						this.mStatusUpdateDelegate = new EventHandler(this.CheckTicStatusHandler);
						break ; 
					case enmProcessType.RAW2D:
						this.mStatusUpdateDelegate = new EventHandler(this.CheckDeconStatusHandler);
						break ; 
					case enmProcessType.DTA:
						this.mStatusUpdateDelegate = new EventHandler(this.CheckMassRefineStatusHandler);
						break;
					default:
						return ;  
				}
				this.mStatusTimer.Tick += this.mStatusUpdateDelegate;
				this.mStatusTimer.Start();

				//this.mthrd_status.Start();
				ThreadStart processThreadStart = new ThreadStart(ProcessFilesThreaded);
				mthrd_process = new Thread(processThreadStart);
				mthrd_process.Name = "Process Files";
				mthrd_process.IsBackground = true;
				mthrd_process.Start();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message + ex.StackTrace) ;
			}
		}

		
		private bool validateParameterFile(string filename)
		{
			if(System.IO.File.Exists(filename)) return true;
			return false;
			

		}

		private void updateCurrentParameters(string filename)
		{

			try
			{
				clsParameterLoader paramLoader = new clsParameterLoader() ; 
				paramLoader.LoadParametersFromFile(filename);
				this.mobj_dta_parameters=paramLoader.DTAParameters;
				this.mobj_fticr_preprocess_parameters=paramLoader.FTICRPreprocessOptions;
				this.mobj_peak_parameters=paramLoader.PeakParameters;
				this.mobj_transform_parameters=paramLoader.TransformParameters;


				
			}
			catch (Exception ex)
			{
				throw new System.IO.IOException("Could not load the parameters.\nDetails:"+ex.Message);
			}


		}

		
		private void ProcessFilesThreaded() 
		{
			try
			{
				DataTable table = (DataTable) this.mdatagrid_files.DataSource ; 
				String paramFile = this.mtxt_param_file.Text;
				for (int i = 0 ; i < table.Rows.Count ; i++)
				{
					DataRow row = table.Rows[i] ; 
					String fileName = (string) row[0] ;
					String outFileName = (string) row[1] ; 
					mstr_current_file_name = fileName ; 
					mstr_current_out_file_name = outFileName ; 
					mstr_param_file_name = paramFile ; 
					menm_current_file_type = GetFileType((string)row[2]) ;
					this.Text = "Processing " + mstr_current_file_name ; 
					this.mdatagrid_files.Select(i) ;
					ProcessCurrentFile(fileName, paramFile, outFileName, menm_current_file_type);
				}
				this.BeginInvoke(new FinishedDelegate(this.FinishOrAbortProcessing), new Object[] {false});
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message + ex.StackTrace) ;
			}
		}

		private delegate void FinishedDelegate(bool abort);

		private void add_S_Folder()
		{
			PRISM.Files.FolderBrowser fb=new PRISM.Files.FolderBrowser();
			
			if (!fb.BrowseForFolder(""))
			{
				return;
			}
			else
			{
				DataTable table = (DataTable) mdatagrid_files.DataSource ; 
				DataRow row = table.NewRow() ; 
				row[0] = fb.FolderPath; 
				row[1] = fb.FolderPath; 
				row[2] = DeconToolsV2.Readers.FileType.ICR2LSRAWDATA.ToString() ; 
				table.Rows.Add(row) ; 

			}
		}



		private void CheckTicStatusHandler(object sender, EventArgs args) 
		{
			try 
			{
//				float []scan_values = new float[1] ; 
//				float []intensity_values = new float[1] ; 
//			
//				mpbar_process.Value = Convert.ToInt32(mobj_decon.MassTransformProgress) ;
//				if (mbln_display_on)
//				{
//					int current_scan = mobj_decon.CurrentScanNum ; 
//					mobj_decon.GetCurrentTic(ref intensity_values,ref scan_values) ; 
//					data_provider.SetData(scan_values, intensity_values) ; 
//				}
//				Console.WriteLine("status = " + Convert.ToString(mobj_decon.MassTransformProgress)); 
//				//Thread.Sleep(300) ;
			
			} 
			catch (Exception e) 
			{
				Console.WriteLine(e);
			}
		}

		private void CheckMassRefineStatusHandler(object sender, EventArgs args)
		{
			try
			{			

				mpbar_process.Value = mobj_proc_runner.PercentDone ;
				
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private void CheckDeconStatusHandler(object sender, EventArgs args) 
		{
			try 
			{
				float []mz_values = new float[1] ; 
				float []intensity_values = new float[1] ; 

				mpbar_process.Value = mobj_proc_runner.PercentDone ;
				if (mbln_display_on && menm_current_file_type != DeconToolsV2.Readers.FileType.AGILENT_TOF)
				{
					string current_file = mobj_raw_data.FileName ; 
					bool just_loaded = false ; 
					if (current_file == null || current_file.CompareTo(mstr_current_file_name) != 0)
					{
						mobj_raw_data = new DeconToolsV2.Readers.clsRawData(mstr_current_file_name, menm_current_file_type) ; 
						just_loaded = true ; 
					}
					int current_scan = mobj_proc_runner.CurrentScanNum ; 
					if (current_scan == mint_current_scan_in_preview)
						return ; 
					mint_current_scan_in_preview = current_scan ; 
					lock (this) 
					{

						double[] xvals = XYValueConverter.ConvertFloatsToDoubles(mz_values);
						double[] yvals = XYValueConverter.ConvertFloatsToDoubles(intensity_values);

						mobj_raw_data.GetSpectrum(current_scan, ref xvals, ref yvals) ; 

						XYValueConverter.ConvertDoublesToFloats(xvals,ref mz_values);
						XYValueConverter.ConvertDoublesToFloats(yvals,ref intensity_values);
					}

					data_provider.SetData(mz_values, intensity_values) ; 
					mctl_spectra.Title = "Scan# " + current_scan.ToString() ; 
					mctl_spectra.AutoViewPortY() ; 
//					mctl_spectra.Invalidate() ; 

					if (just_loaded)
					{
						mctl_spectra.ViewPortHistory.Clear() ; 
						mctl_spectra.AutoViewPort() ; 
						if (menm_current_file_type == DeconToolsV2.Readers.FileType.BRUKER || menm_current_file_type == DeconToolsV2.Readers.FileType.ICR2LSRAWDATA)
						{
							// set the mz range. 
							mctl_spectra.ViewPort = new RectangleF(400, mctl_spectra.ViewPort.Y, 1600, mctl_spectra.ViewPort.Height) ; 
						}
					}
				}
			}
			catch (Exception e) 
			{
				Console.WriteLine(e);
			}
		}


		private DeconToolsV2.Readers.FileType GetFileType(string file_type)
		{
			try
			{
				if(file_type == DeconToolsV2.Readers.FileType.FINNIGAN.ToString())
					return DeconToolsV2.Readers.FileType.FINNIGAN ; 
				else if (file_type == DeconToolsV2.Readers.FileType.AGILENT_TOF.ToString())
					return DeconToolsV2.Readers.FileType.AGILENT_TOF ; 
				else if (file_type == DeconToolsV2.Readers.FileType.MICROMASSRAWDATA.ToString())
					return DeconToolsV2.Readers.FileType.MICROMASSRAWDATA ; 
				else if (file_type == DeconToolsV2.Readers.FileType.PNNL_IMS.ToString())
					return DeconToolsV2.Readers.FileType.PNNL_IMS ; 
				else if (file_type == DeconToolsV2.Readers.FileType.BRUKER.ToString())
					return DeconToolsV2.Readers.FileType.BRUKER ; 
				else if (file_type == DeconToolsV2.Readers.FileType.ICR2LSRAWDATA.ToString())
					return DeconToolsV2.Readers.FileType.ICR2LSRAWDATA ; 
				else if (file_type == DeconToolsV2.Readers.FileType.MZXMLRAWDATA.ToString())
					return DeconToolsV2.Readers.FileType.MZXMLRAWDATA ; 
				else if (file_type == DeconToolsV2.Readers.FileType.PNNL_UIMF.ToString())
					return DeconToolsV2.Readers.FileType.PNNL_UIMF ; 
				return DeconToolsV2.Readers.FileType.UNDEFINED ; 
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message + ex.StackTrace) ;
			}
			return DeconToolsV2.Readers.FileType.UNDEFINED ; 
		}

		private string RemoveExtension(string fileName, DeconToolsV2.Readers.FileType fileType)
		{
			try
			{
				int last_pos = fileName.LastIndexOf(".") ;
				if (last_pos == -1)
					return fileName ; 
				return fileName.Substring(0, last_pos) ; 
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message + ex.StackTrace) ;
			}
			return null ; 
		}

		private void ProcessCurrentFile(string fileName, string paramFile, string outFileName, DeconToolsV2.Readers.FileType fileType)
		{

			try
			{
				System.DateTime start_time = DateTime.Now ; 
				if (mobj_proc_runner == null)
					mobj_proc_runner = new DeconToolsV2.clsProcRunner() ; 
				switch (menm_process_type)
				{
					case enmProcessType.DECON:
						mobj_proc_runner.HornTransformParameters = mobj_transform_parameters ; 
						mobj_proc_runner.PeakProcessorParameters = mobj_peak_parameters ;
						mobj_proc_runner.FTICRPreprocessOptions = mobj_fticr_preprocess_parameters ; 
						mobj_proc_runner.DTAGenerationParameters = mobj_dta_parameters ; 
						mobj_proc_runner.FileName = fileName ; 
						mobj_proc_runner.FileType = menm_current_file_type ; 
						mobj_proc_runner.CreateTransformResults() ; 
						if (fileType != DeconToolsV2.Readers.FileType.ICR2LSRAWDATA)
							mobj_proc_runner.HornTransformResults.WriteResults(RemoveExtension(fileName, fileType), false) ; 
						else
							mobj_proc_runner.HornTransformResults.WriteResults(RemoveExtension(fileName, fileType), true) ; 						
						break;
					case enmProcessType.TIC:
						break ; 
					case enmProcessType.RAW2D:
						mobj_proc_runner.HornTransformParameters = mobj_transform_parameters ; 
						mobj_proc_runner.PeakProcessorParameters = mobj_peak_parameters ;
						mobj_proc_runner.FTICRPreprocessOptions = mobj_fticr_preprocess_parameters ; 
						mobj_proc_runner.DTAGenerationParameters = mobj_dta_parameters ; 
						mobj_proc_runner.FileName = fileName ; 
						mobj_proc_runner.FileType = menm_current_file_type ; 
						mobj_proc_runner.CreateTransformResultWithPeaksOnly() ; 
						if (fileType != DeconToolsV2.Readers.FileType.ICR2LSRAWDATA)
							mobj_proc_runner.HornTransformResults.WriteResults(RemoveExtension(fileName, fileType), false) ; 
						else
							mobj_proc_runner.HornTransformResults.WriteResults(RemoveExtension(fileName, fileType), true) ; 
						break;
					case enmProcessType.DTA:
						mobj_proc_runner.HornTransformParameters = mobj_transform_parameters;
						mobj_proc_runner.PeakProcessorParameters = mobj_peak_parameters;
						mobj_proc_runner.FTICRPreprocessOptions = mobj_fticr_preprocess_parameters ; 
						mobj_proc_runner.DTAGenerationParameters = mobj_dta_parameters ; 
						mobj_proc_runner.FileName = fileName ; 
						mobj_proc_runner.FileType = menm_current_file_type ;					
						mobj_proc_runner.CreateDTAFile() ; 					
						break;				
					default:
						break ; 
				}
				System.DateTime stop_time = DateTime.Now ; 
				System.TimeSpan span = new System.TimeSpan(stop_time.Ticks - start_time.Ticks) ; 
				Console.WriteLine("Processed file: " + fileName + " in " + Convert.ToString(span.TotalMilliseconds) + " ms") ; 
				GC.Collect() ; 
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message + ex.StackTrace) ;
				mobj_proc_runner = null ; 
			}
		}

		protected override void OnResize(EventArgs e)
		{
			try
			{
				if (this.mdatagrid_files != null && this.mdatagrid_files.TableStyles.Count > 0)
				{

					if (this.mdatagrid_files.TableStyles[0].GridColumnStyles.Count > 0)
					{
						this.mdatagrid_files.TableStyles[0].GridColumnStyles[0].Width = (this.Width*3)/ 8 ;
						this.mdatagrid_files.TableStyles[0].GridColumnStyles[1].Width = (this.Width*3)/ 8 ;
						this.mdatagrid_files.TableStyles[0].GridColumnStyles[2].Width = this.Width - (this.Width*3)/ 4 - 5 ;
					}
				}
				this.mbtn_file_open.Left = this.Width - 45 ; 
				this.mtxt_param_file.Width = this.mbtn_file_open.Left - 10 - this.mtxt_param_file.Left ; 
				base.OnResize (e);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message + ex.StackTrace) ;
			}
		}

		private void mbtn_cancel_Click(object sender, System.EventArgs e)
		{
			try
			{
				this.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message + ex.StackTrace) ;
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			try
			{
				this.FinishOrAbortProcessing(true);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message + ex.StackTrace) ;
			}
		}

		private void mbtn_display_Click(object sender, System.EventArgs e)
		{
			try
			{
				if (mbln_display_on == false)
				{
					mpanel_display.Height = mint_display_panel_default_height ; 
					this.Height = this.Height + mint_display_panel_default_height ; 
					mbtn_display.Text = "Display Off" ; 
				}
				else
				{
					mpanel_display.Height = 0 ; 
					this.Height = this.Height - mint_display_panel_default_height ; 
					mbtn_display.Text = "Display On" ; 
				}
				mbln_display_on = !mbln_display_on ; 
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message + ex.StackTrace) ;
			}
		}

		private void frmProcess_Load(object sender, System.EventArgs e)
		{
		
		}

		private void mcmb_process_type_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}

		private void mpbar_process_Click(object sender, System.EventArgs e)
		{
		}

		private void chkUseParameterFile_CheckedChanged(object sender, System.EventArgs e)
		{
			if (!this.chkUseParameterFile.Checked)   // in this case, the user unchecks the box
			{
				if (this.Parent==null || this.Parent.Parent==null) return;
				
				//Apply frmDecon2LS's settings back to this form
				if (this.Parent.Parent is frmDecon2LS)
				{
					this.mobj_peak_parameters=((frmDecon2LS)(this.Parent.Parent)).PeakProcessorParameters;
					this.mobj_dta_parameters=((frmDecon2LS)(this.Parent.Parent)).DTAGenerationParameters;
					this.mobj_fticr_preprocess_parameters=((frmDecon2LS)(this.Parent.Parent)).FTICRPreProcessParameters;
					this.mobj_transform_parameters=((frmDecon2LS)(this.Parent.Parent)).MassTransformParameters;

					MessageBox.Show("FYI - Parameters from 'Options' on the DeconTools main form have been re-applied here.");
				}
				else
				{
					MessageBox.Show("Original parameters were not applied. You should re-open this form to apply original parameters.");
				}
			}

//			MessageBox.Show("Smoothing = " + this.MassTransformParameters.UseSavitzkyGolaySmooth.ToString());

			


		
		}
	}
}
