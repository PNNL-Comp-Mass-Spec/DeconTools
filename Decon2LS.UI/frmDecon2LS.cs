// Written by Navdeep Jaitly, Anoop Mayampurath and Kyle Littlefield 
// for the Department of Energy (PNNL, Richland, WA)
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
using PNNL.Controls;

namespace Decon2LS
{
	/// <summary>
	/// Summary description for frmDecon2LS.
	/// </summary>
	public class frmDecon2LS : System.Windows.Forms.Form
	{
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem_open;

		/// <summary>
		/// Attached to forms to be notified when the window closed.  
		/// The event is then propogated to the mediator.
		/// </summary>
		private EventHandler mMdiChildCloseHandler;

		/// <summary>
		/// The main mediator for the application.
		/// </summary>
		private Decon2LS.clsMediator mMediator;

		private clsCfgParms mobj_config = new clsCfgParms() ;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.MenuItem menuItem_process;
		private System.Windows.Forms.MenuItem menuItem_ArrangeIcons;
		private System.Windows.Forms.MenuItem menuItem_TileHorizontal;
		private System.Windows.Forms.MenuItem menuItem_TileVertical;
		private System.Windows.Forms.MenuItem menuItem_Windows;
		private System.Windows.Forms.MenuItem menuItem_Cascade;
		private PNNL.Controls.ctlFileView mFileTreeView;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem menuItem_Exit;
		private System.Windows.Forms.MenuItem menuItem_File;
		private System.Windows.Forms.MenuItem menuItem_Tools;
		private System.Windows.Forms.ImageList mimgListMain;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItemAbout;
		private System.Windows.Forms.MenuItem mmnuMercury;
		private System.Windows.Forms.MenuItem mmnuCreateTIC ; 
		private System.Windows.Forms.MenuItem mmnuOpenResults; 

		private DeconToolsV2.HornTransform.clsHornTransformParameters mobjTransformParameters ;
		private DeconToolsV2.DTAGeneration.clsDTAGenerationParameters mobjDTAGenerationParameters ; 
		private DeconToolsV2.Peaks.clsPeakProcessorParameters mobjPeakParameters ; 
		private DeconToolsV2.Readers.clsRawDataPreprocessOptions mobjFTICRRawPreProcessParameters ; 

		public DeconToolsV2.HornTransform.clsHornTransformParameters MassTransformParameters
		{
			get
			{
				return mobjTransformParameters ; 
			}
			set
			{
				mobjTransformParameters = value ; 
			}
		} 

		public DeconToolsV2.Peaks.clsPeakProcessorParameters PeakProcessorParameters
		{
			get
			{
				return mobjPeakParameters ; 
			}
			set
			{
				mobjPeakParameters = value ; 
			}
		}

		public DeconToolsV2.Readers.clsRawDataPreprocessOptions FTICRPreProcessParameters
		{
			get
			{
				return mobjFTICRRawPreProcessParameters ; 
			}
			set
			{
				mobjFTICRRawPreProcessParameters = value ; 
			}
		}

		public DeconToolsV2.DTAGeneration.clsDTAGenerationParameters DTAGenerationParameters
		{
			get
			{
				return mobjDTAGenerationParameters ; 
			}
			set
			{
				mobjDTAGenerationParameters = value ; 
			}
		}




		private const string mstrDEFAULTPARAMFILE = "Decon2LS.xml" ;
		private System.Windows.Forms.MenuItem menuItem_Options; 
		private string mstrParamFile ; 
		public frmDecon2LS()
		{
			try
			{
				this.mMediator = new Decon2LSMediator(this);
				//
				// Required for Windows Form Designer support
				//
				InitializeComponent();

				this.mMdiChildCloseHandler = new EventHandler(this.MdiChildClosed);

				// attach file viewer to mediator
				this.mFileTreeView.AttachTo(mMediator);

				// Initialize the categories that we always want to appear in the side view
				frmSpectra.InitializeCategories(mFileTreeView);
				frmMercury.InitializeCategories(mFileTreeView);
				frm2DPeakProcessing.InitializeCategories(mFileTreeView);

			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message) ; 
			}
			try
			{
				clsParameterLoader paramLoader = new clsParameterLoader() ; 
				if (System.IO.File.Exists(Application.ExecutablePath + mstrDEFAULTPARAMFILE))
				{
					mstrParamFile = Application.ExecutablePath + mstrDEFAULTPARAMFILE ; 
					paramLoader.LoadParametersFromFile(mstrParamFile) ; 
					mobjPeakParameters = paramLoader.PeakParameters ; 
					mobjTransformParameters = paramLoader.TransformParameters ; 
					mobjFTICRRawPreProcessParameters = paramLoader.FTICRPreprocessOptions ; 
					mobjDTAGenerationParameters = paramLoader.DTAParameters ; 
				}
				else if (System.IO.File.Exists(Application.UserAppDataPath + mstrDEFAULTPARAMFILE))
				{
					mstrParamFile = Application.UserAppDataPath + mstrDEFAULTPARAMFILE ; 
					paramLoader.LoadParametersFromFile(mstrParamFile) ; 
					mobjPeakParameters = paramLoader.PeakParameters ; 
					mobjTransformParameters = paramLoader.TransformParameters ; 
					mobjFTICRRawPreProcessParameters = paramLoader.FTICRPreprocessOptions ; 
					mobjDTAGenerationParameters = paramLoader.DTAParameters ; 
				}
				else
				{
					// no default parameters found, loading defaults. 
					MessageBox.Show(this, "No parameter file found. Loading defaults.") ; 
					mobjPeakParameters = new DeconToolsV2.Peaks.clsPeakProcessorParameters() ; 
					mobjTransformParameters = new DeconToolsV2.HornTransform.clsHornTransformParameters() ; 
					mobjFTICRRawPreProcessParameters = new DeconToolsV2.Readers.clsRawDataPreprocessOptions() ; 
					mobjDTAGenerationParameters  = new DeconToolsV2.DTAGeneration.clsDTAGenerationParameters() ; 
					mstrParamFile = Application.ExecutablePath + mstrDEFAULTPARAMFILE ; 
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message + ex.StackTrace) ; 
				mobjPeakParameters = new DeconToolsV2.Peaks.clsPeakProcessorParameters() ; 
				mobjTransformParameters = new DeconToolsV2.HornTransform.clsHornTransformParameters() ; 
				mobjFTICRRawPreProcessParameters = new DeconToolsV2.Readers.clsRawDataPreprocessOptions() ; 
				mobjDTAGenerationParameters  = new DeconToolsV2.DTAGeneration.clsDTAGenerationParameters() ; 
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{

			
			if( disposing )
			{
				//write out parameters to file. 
				clsParameterLoader paramLoader = new clsParameterLoader() ; 
				paramLoader.PeakParameters = mobjPeakParameters ; 
				paramLoader.TransformParameters = mobjTransformParameters ; 
				paramLoader.FTICRPreprocessOptions = mobjFTICRRawPreProcessParameters ; 
				paramLoader.DTAParameters = mobjDTAGenerationParameters ; 
				paramLoader.SaveParametersToFile(mstrParamFile) ; 

				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		[STAThread()]
		public static void Main(string [] aa)
		{
			Application.Run(new frmDecon2LS()) ;
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDecon2LS));
            System.Configuration.AppSettingsReader configurationAppSettings = new System.Configuration.AppSettingsReader();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem_File = new System.Windows.Forms.MenuItem();
            this.menuItem_open = new System.Windows.Forms.MenuItem();
            this.mmnuOpenResults = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.mmnuCreateTIC = new System.Windows.Forms.MenuItem();
            this.mmnuMercury = new System.Windows.Forms.MenuItem();
            this.menuItem_Exit = new System.Windows.Forms.MenuItem();
            this.menuItem_Tools = new System.Windows.Forms.MenuItem();
            this.menuItem_process = new System.Windows.Forms.MenuItem();
            this.menuItem_Windows = new System.Windows.Forms.MenuItem();
            this.menuItem_Cascade = new System.Windows.Forms.MenuItem();
            this.menuItem_ArrangeIcons = new System.Windows.Forms.MenuItem();
            this.menuItem_TileHorizontal = new System.Windows.Forms.MenuItem();
            this.menuItem_TileVertical = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItemAbout = new System.Windows.Forms.MenuItem();
            this.menuItem_Options = new System.Windows.Forms.MenuItem();
            this.mimgListMain = new System.Windows.Forms.ImageList(this.components);
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.mFileTreeView = new PNNL.Controls.ctlFileView();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem_File,
            this.menuItem_Tools,
            this.menuItem_Windows,
            this.menuItem1,
            this.menuItem_Options});
            // 
            // menuItem_File
            // 
            this.menuItem_File.Index = 0;
            this.menuItem_File.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem_open,
            this.mmnuOpenResults,
            this.menuItem3,
            this.mmnuCreateTIC,
            this.mmnuMercury,
            this.menuItem_Exit});
            this.menuItem_File.Text = "File";
            // 
            // menuItem_open
            // 
            this.menuItem_open.Index = 0;
            this.menuItem_open.Text = "Open";
            this.menuItem_open.Click += new System.EventHandler(this.menuItem_open_Click);
            // 
            // mmnuOpenResults
            // 
            this.mmnuOpenResults.Index = 1;
            this.mmnuOpenResults.Text = "Open Results";
            this.mmnuOpenResults.Click += new System.EventHandler(this.mmnuOpenResults_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 2;
            this.menuItem3.Text = "-";
            // 
            // mmnuCreateTIC
            // 
            this.mmnuCreateTIC.Index = 3;
            this.mmnuCreateTIC.Text = "TIC";
            this.mmnuCreateTIC.Click += new System.EventHandler(this.mmnuCreateTIC_Click);
            // 
            // mmnuMercury
            // 
            this.mmnuMercury.Index = 4;
            this.mmnuMercury.Text = "Mercury";
            this.mmnuMercury.Click += new System.EventHandler(this.mmnuMercury_Click);
            // 
            // menuItem_Exit
            // 
            this.menuItem_Exit.Index = 5;
            this.menuItem_Exit.Text = "Exit";
            this.menuItem_Exit.Click += new System.EventHandler(this.menuItem_Exit_Click);
            // 
            // menuItem_Tools
            // 
            this.menuItem_Tools.Index = 1;
            this.menuItem_Tools.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem_process});
            this.menuItem_Tools.Text = "Tools";
            // 
            // menuItem_process
            // 
            this.menuItem_process.Index = 0;
            this.menuItem_process.Text = "Process";
            this.menuItem_process.Click += new System.EventHandler(this.menuItem_process_Click);
            // 
            // menuItem_Windows
            // 
            this.menuItem_Windows.Index = 2;
            this.menuItem_Windows.MdiList = true;
            this.menuItem_Windows.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem_Cascade,
            this.menuItem_ArrangeIcons,
            this.menuItem_TileHorizontal,
            this.menuItem_TileVertical});
            this.menuItem_Windows.Text = "Windows";
            // 
            // menuItem_Cascade
            // 
            this.menuItem_Cascade.Index = 0;
            this.menuItem_Cascade.Text = "Cascade";
            this.menuItem_Cascade.Click += new System.EventHandler(this.menuItem_Cascade_Click);
            // 
            // menuItem_ArrangeIcons
            // 
            this.menuItem_ArrangeIcons.Index = 1;
            this.menuItem_ArrangeIcons.Text = "Arrange Icons";
            this.menuItem_ArrangeIcons.Click += new System.EventHandler(this.menuItem_ArrangeIcons_Click);
            // 
            // menuItem_TileHorizontal
            // 
            this.menuItem_TileHorizontal.Index = 2;
            this.menuItem_TileHorizontal.Text = "Tile Horizontal";
            this.menuItem_TileHorizontal.Click += new System.EventHandler(this.menuItem_TileHorizontal_Click);
            // 
            // menuItem_TileVertical
            // 
            this.menuItem_TileVertical.Index = 3;
            this.menuItem_TileVertical.Text = "Tile Vertical";
            this.menuItem_TileVertical.Click += new System.EventHandler(this.menuItem_TileVertical_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 3;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemAbout});
            this.menuItem1.Text = "Help";
            // 
            // menuItemAbout
            // 
            this.menuItemAbout.Index = 0;
            this.menuItemAbout.Text = "About";
            this.menuItemAbout.Click += new System.EventHandler(this.menuItemAbout_Click);
            // 
            // menuItem_Options
            // 
            this.menuItem_Options.Index = 4;
            this.menuItem_Options.Text = "Options";
            this.menuItem_Options.Click += new System.EventHandler(this.menuItem_Options_Click);
            // 
            // mimgListMain
            // 
            this.mimgListMain.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("mimgListMain.ImageStream")));
            this.mimgListMain.TransparentColor = System.Drawing.Color.Transparent;
            this.mimgListMain.Images.SetKeyName(0, "");
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.SystemColors.Control;
            this.splitter1.Location = new System.Drawing.Point(230, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(8, 502);
            this.splitter1.TabIndex = 5;
            this.splitter1.TabStop = false;
            // 
            // mFileTreeView
            // 
            this.mFileTreeView.Dock = System.Windows.Forms.DockStyle.Left;
            this.mFileTreeView.ImageIndex = 0;
            this.mFileTreeView.Location = new System.Drawing.Point(0, 0);
            this.mFileTreeView.Name = "mFileTreeView";
            this.mFileTreeView.SelectedImageIndex = 0;
            this.mFileTreeView.Size = new System.Drawing.Size(230, 502);
            this.mFileTreeView.TabIndex = 7;
            this.mFileTreeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.mFileTreeView_BeforeExpand);
            // 
            // frmDecon2LS
            // 
            this.AllowDrop = true;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(744, 502);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.mFileTreeView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = ((bool)(configurationAppSettings.GetValue("frmDecon2LS.IsMdiContainer", typeof(bool))));
            this.Menu = this.mainMenu1;
            this.Name = "frmDecon2LS";
            this.Text = "DeconTools_BetaV1.3.3";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.MdiChildActivate += new System.EventHandler(this.frmDecon2LS_MdiChildActivate);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.frmDecon2LS_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.frmDecon2LS_DragEnter);
            this.ResumeLayout(false);

		}
		#endregion
 
		#region Menu response





		
		
		private void menuItem_open_Click(object sender, System.EventArgs e)
		{
			try
			{
				OpenFileDialog openFileDialog1 = new OpenFileDialog();
				openFileDialog1.Filter = "Xcalibur files (*.RAW)|*.RAW|Agilent files (*.wiff)|*.wiff|Micromass files (_FUNC*.DAT)|_FUNC*.DAT|Bruker files(acqu)|acqu|S files ICR2LS Format(*.*)|*.*|S files SUN Extrel Format(*.*)|*.*|MZ Xml File(*.mzXML)|*.mzXML|PNNL IMF File(*.IMF)|*.IMF|PNNL UIMF File(*.UIMF)|*.UIMF|Bruker Ascii peak File(*.ascii)|*.ascii|Raw Ascii File(*.txt)|*.txt|All files(*.*)|*.*" ;
				openFileDialog1.FilterIndex = 1 ;
				openFileDialog1.RestoreDirectory = true ;
				openFileDialog1.InitialDirectory = mobj_config.OpenDir ; 

				if(openFileDialog1.ShowDialog() == DialogResult.OK)
				{
					string file_name = openFileDialog1.FileName ; 
					int index = file_name.LastIndexOf("\\") ;
					string path_dir = "" ; 

					if (index > 0)
					{
						path_dir = file_name.Substring(0, index) ; 
						mobj_config.OpenDir = path_dir ; 
					}

					frmSpectra spectra_form ; 
					DialogResult ticComputeResult ; 
					switch (openFileDialog1.FilterIndex)
					{
						case 1:
							// Open Xcalibur File.
							spectra_form = new frmSpectra() ; 
							spectra_form.PeakProcessorParameters = mobjPeakParameters ; 
							spectra_form.HornTransformParameters = mobjTransformParameters ; 
							spectra_form.FTICRPreProcessOptions = mobjFTICRRawPreProcessParameters ; 
							spectra_form.LoadXCaliburFile(file_name) ;
							mMediator.RequestFormOpen(spectra_form);
							break ; 
						case 2:
							// Open Agilent File
							spectra_form = new frmSpectra() ; 
							spectra_form.PeakProcessorParameters = mobjPeakParameters ; 
							spectra_form.HornTransformParameters = mobjTransformParameters ; 
							spectra_form.FTICRPreProcessOptions = mobjFTICRRawPreProcessParameters ; 
							spectra_form.LoadAgilentTOFFile(file_name) ;
							mMediator.RequestFormOpen(spectra_form); 
							break ; 
						case 3:
							// Open Micromass File
							spectra_form = new frmSpectra() ; 
							spectra_form.PeakProcessorParameters = mobjPeakParameters ; 
							spectra_form.HornTransformParameters = mobjTransformParameters ; 
							spectra_form.FTICRPreProcessOptions = mobjFTICRRawPreProcessParameters ; 
							spectra_form.LoadMicromassTOFFile(path_dir) ;
							mMediator.RequestFormOpen(spectra_form);
							break ; 
						case 4:
							// Open Bruker File
							ticComputeResult = MessageBox.Show(this, 
								"Opening Bruker ser File. The .ser file does not have precomputed TIC stored. Would you like to load tic (could take a couple of minutes)?", 
								"Compute Tic for .ser file?", MessageBoxButtons.YesNoCancel) ; 
							if (ticComputeResult == DialogResult.Cancel)
								return ; 
							spectra_form = new frmSpectra() ; 
							spectra_form.PeakProcessorParameters = mobjPeakParameters ; 
							spectra_form.HornTransformParameters = mobjTransformParameters ; 
							spectra_form.FTICRPreProcessOptions = mobjFTICRRawPreProcessParameters ; 
							if (ticComputeResult == DialogResult.Yes)
							{
								spectra_form.LoadFileWithTicThreaded(path_dir, DeconToolsV2.Readers.FileType.BRUKER) ; 
								mMediator.RequestFormOpen(spectra_form);
							}
							else
							{
								spectra_form.LoadBrukerFile(path_dir) ; 
								mMediator.RequestFormOpen(spectra_form);
							}
							mMediator.RequestFormOpen(spectra_form);
							break ; 
						case 5:
							// Open S file (ICR2LS format) file
							spectra_form = new frmSpectra() ; 
							spectra_form.PeakProcessorParameters = mobjPeakParameters ; 
							spectra_form.HornTransformParameters = mobjTransformParameters ; 
							spectra_form.FTICRPreProcessOptions = mobjFTICRRawPreProcessParameters ; 
							spectra_form.LoadSFileICR2LSFormat(file_name) ; 
							mMediator.RequestFormOpen(spectra_form);
							break ; 
						case 6:
							// Open S file (SUNExtrel format) file
							spectra_form = new frmSpectra() ; 
							spectra_form.PeakProcessorParameters = mobjPeakParameters ; 
							spectra_form.HornTransformParameters = mobjTransformParameters ; 
							spectra_form.FTICRPreProcessOptions = mobjFTICRRawPreProcessParameters ; 
							spectra_form.LoadSFileSUNExtrelFormat(file_name) ; 
							mMediator.RequestFormOpen(spectra_form);
							break ; 
						case 7:
							ticComputeResult = MessageBox.Show(this, 
								"Opening MZXml File. MZXml does not have precomputed TIC stored. Would you like to compute tic (could take a minute or two)?", 
								"Compute Tic for MZXml file?", MessageBoxButtons.YesNoCancel) ; 
							if (ticComputeResult == DialogResult.Cancel)
								return ; 
							// Open MZXML file (MZXML format) file
							spectra_form = new frmSpectra() ; 
							spectra_form.PeakProcessorParameters = mobjPeakParameters ; 
							spectra_form.HornTransformParameters = mobjTransformParameters ; 
							spectra_form.FTICRPreProcessOptions = mobjFTICRRawPreProcessParameters ; 
							if (ticComputeResult == DialogResult.Yes)
							{
								spectra_form.LoadFileWithTicThreaded(file_name, DeconToolsV2.Readers.FileType.MZXMLRAWDATA) ; 
								mMediator.RequestFormOpen(spectra_form);
							}
							else
							{
								spectra_form.LoadMZXMLFile(file_name) ; 
								mMediator.RequestFormOpen(spectra_form);
							}
							break ; 
						case 8:
							// Open IMF File
							spectra_form = new frmSpectra() ; 
							spectra_form.PeakProcessorParameters = mobjPeakParameters ; 
							spectra_form.HornTransformParameters = mobjTransformParameters ; 
							spectra_form.FTICRPreProcessOptions = mobjFTICRRawPreProcessParameters ; 
							spectra_form.LoadIMFTOFFile(file_name) ;
							mMediator.RequestFormOpen(spectra_form);
							break ; 
						case 9:
							// Open UIMF File
							spectra_form = new frmSpectra() ; 
							spectra_form.PeakProcessorParameters = mobjPeakParameters ; 
							spectra_form.HornTransformParameters = mobjTransformParameters ; 
							spectra_form.FTICRPreProcessOptions = mobjFTICRRawPreProcessParameters ; 
							spectra_form.LoadUIMFFile(file_name) ;
							mMediator.RequestFormOpen(spectra_form);
							break ; 
						case 10:
							ticComputeResult = MessageBox.Show(this, 
								"Opening Bruker ascii File. The ascii file does not have precomputed TIC stored. Would you like to compute tic (could take a couple of minutes)?", 
								"Compute Tic for ascii file?", MessageBoxButtons.YesNoCancel) ; 
							if (ticComputeResult == DialogResult.Cancel)
								return ; 
							spectra_form = new frmSpectra() ; 
							spectra_form.PeakProcessorParameters = mobjPeakParameters ; 
							spectra_form.HornTransformParameters = mobjTransformParameters ; 
							spectra_form.FTICRPreProcessOptions = mobjFTICRRawPreProcessParameters ; 
							if (ticComputeResult == DialogResult.Yes)
							{
								spectra_form.LoadFileWithTicThreaded(file_name, DeconToolsV2.Readers.FileType.BRUKER_ASCII) ; 
								mMediator.RequestFormOpen(spectra_form);
							}
							else
							{
								spectra_form.LoadBrukerAsciiFile(path_dir) ; 
								mMediator.RequestFormOpen(spectra_form);
							}
							mMediator.RequestFormOpen(spectra_form);
							break ; 
						case 11:
							ticComputeResult = MessageBox.Show(this, 
								"Opening ascii file. Since the file is unindexed we will have to scan the file first. This could take a few minutes for large files. Continue ?", 
								"Warning", MessageBoxButtons.YesNo) ; 
							if (ticComputeResult == DialogResult.No)
								return ; 

							spectra_form = new frmSpectra() ; 
							spectra_form.PeakProcessorParameters = mobjPeakParameters ; 
							spectra_form.HornTransformParameters = mobjTransformParameters ; 
							spectra_form.FTICRPreProcessOptions = mobjFTICRRawPreProcessParameters ; 
							if (ticComputeResult == DialogResult.Yes)
							{
								spectra_form.LoadFileWithTicThreaded(file_name, DeconToolsV2.Readers.FileType.ASCII) ; 
								mMediator.RequestFormOpen(spectra_form);
							}

							mMediator.RequestFormOpen(spectra_form);
							break ; 
						default:
							// Open any kind of file. First get file type here.
							break ; 
					}
				}
				else
				{
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString()) ; 
			}

		}

		private void menuItem_process_Click(object sender, System.EventArgs e)
		{
			try
			{
				frmProcess process_frm = new frmProcess(ref mobj_config) ; 
				process_frm.PeakProcessorParameters = mobjPeakParameters ; 
				process_frm.MassTransformParameters = mobjTransformParameters ; 
					process_frm.FTICRPreProcessParameters = mobjFTICRRawPreProcessParameters ; 
				process_frm.DTAGenerationParameters = mobjDTAGenerationParameters ; 
				process_frm.Show() ; 
				process_frm.MdiParent = this ; 
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message + ex.StackTrace) ; 
			}

		}
		#endregion

		#region "Mdi Children layout"
		private void menuItem_Cascade_Click(object sender, System.EventArgs e)
		{
			this.LayoutMdi(MdiLayout.Cascade);
		}

		private void menuItem_ArrangeIcons_Click(object sender, System.EventArgs e)
		{
			this.LayoutMdi(MdiLayout.ArrangeIcons);
		}

		private void menuItem_TileHorizontal_Click(object sender, System.EventArgs e)
		{
			this.LayoutMdi(MdiLayout.TileHorizontal);
		}

		private void menuItem_TileVertical_Click(object sender, System.EventArgs e)
		{
			this.LayoutMdi(MdiLayout.TileVertical);
		}
		#endregion

		private void frmDecon2LS_MdiChildActivate(object sender, System.EventArgs e)
		{
		}

		/// <summary>
		/// Notified when a form opened via the OpenMdiChild method is closed.  
		/// Propagates event to the mediator.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MdiChildClosed(object sender, EventArgs e)
		{
			try 
			{
				Form form = (Form) sender;
				// Propogate event to mediator
				if (form is ICategorizedItem) 
				{
					mMediator.RaiseItemClose(this, (ICategorizedItem) form);
				}
				// Remove closed event handler from form
				form.Closed -= this.mMdiChildCloseHandler;
			} 
			catch (Exception ex) 
			{
				MessageBox.Show(this, ex.Message + ex.StackTrace) ; 
			}
		}

		private void menuItem_Exit_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void mtlbMain_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			
		}

		private void menuItemAbout_Click(object sender, System.EventArgs e)
		{
			try
			{
				frmAbout aboutForm = new frmAbout() ; 
				aboutForm.ShowDialog(this) ; 
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message + ex.StackTrace) ; 
			}
		}

		private void mmnuOpenResults_Click(object sender, System.EventArgs e)
		{
			try
			{
				OpenFileDialog openFileDialog1 = new OpenFileDialog();
				openFileDialog1.Filter = "Raw Peak Files (*.dat)|*.dat" ;
				openFileDialog1.FilterIndex = 1 ;
				openFileDialog1.RestoreDirectory = true ;
				openFileDialog1.InitialDirectory = mobj_config.OpenDir ; 
				
				

				if(openFileDialog1.ShowDialog() == DialogResult.OK)
				{
					string file_name = openFileDialog1.FileName ; 
					int index = file_name.LastIndexOf("\\") ;
					string path_dir = "" ; 

					if (index > 0)
					{
						path_dir = file_name.Substring(0, index) ; 
						mobj_config.OpenDir = path_dir ; 
					}
					DeconToolsV2.Results.clsTransformResults transformResults = new DeconToolsV2.Results.clsTransformResults() ; 
					transformResults.ReadResults(file_name) ; 
					frm2DPeakProcessing frmTwoD = new frm2DPeakProcessing(transformResults) ;
					frmTwoD.HornTransformParameters = mobjTransformParameters ; 
					frmTwoD.PeakProcessorParameters = mobjPeakParameters ;
					frmTwoD.FTICRPreProcessOptions = mobjFTICRRawPreProcessParameters ;
					mMediator.RequestFormOpen(frmTwoD) ; 
				}
			}			
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString()) ; 
			}
		}

		private void mmnuMercury_Click(object sender, System.EventArgs e)
		{
			try
			{
				frmMercury mercury = new frmMercury();
				mercury.ElementIsotopes = mobjTransformParameters.ElementIsotopeComposition ; 
				mMediator.RequestFormOpen(mercury);		
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message + ex.StackTrace) ; 
			}
		}

		private void mmnuCreateTIC_Click(object sender, System.EventArgs e)
		{
			try
			{

				OpenFileDialog openFileDialog1 = new OpenFileDialog();
				openFileDialog1.Filter = "Scans CSV File (*_scans.csv)|*.csv|ICR2LS TIC File (*.*)|*.tic" ;
				openFileDialog1.FilterIndex = 1 ;
				openFileDialog1.RestoreDirectory = true ;
				openFileDialog1.InitialDirectory = mobj_config.OpenDir ; 

				if(openFileDialog1.ShowDialog() == DialogResult.OK)
				{
					string file_name = openFileDialog1.FileName ; 
					int index = file_name.LastIndexOf("\\") ;
					index++ ; 
					string file_name_without_path = "" ; 

					if (index > 0)
					{
						file_name_without_path = file_name.Substring(index, file_name.Length-index) ; 							
					}
					
					frmTICViewer frmTIC = new frmTICViewer() ; 
					frmTIC.mFileName = file_name  ; 
					frmTIC.mFileNameForHeader = file_name_without_path ; 
					switch(openFileDialog1.FilterIndex)
					{
						case 1 :frmTIC.LoadScansTICFile() ; 					
							break  ; 
						case 2: frmTIC.LoadIcr2lsTICFile()  ; 
							break ;
						default: break ; 
					}
			
					mMediator.RequestFormOpen(frmTIC) ; 
				}
				
				
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message + ex.StackTrace) ; 
			}
		}

		private void frmDecon2LS_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			foreach (string fileName in (string[])e.Data.GetData(DataFormats.FileDrop) )
			{

				int index = fileName.LastIndexOf("\\") ;
				string path_dir = "" ; 			

				if (index > 0)
				{
					path_dir = fileName.Substring(0, index) ; 
					mobj_config.OpenDir = path_dir ; 
				}		

				if (fileName.EndsWith("_scans.csv"))
				{					
					index++ ; 
					string file_name_without_path = "" ; 

					if (index > 0)
					{
						file_name_without_path = fileName.Substring(index, fileName.Length-index) ; 							
					}
					
					frmTICViewer frmTIC = new frmTICViewer() ; 
					frmTIC.mFileName = fileName  ; 
					frmTIC.mFileNameForHeader = file_name_without_path ; 
					frmTIC.LoadScansTICFile() ; 					
					mMediator.RequestFormOpen(frmTIC) ;
				}
				else if (fileName.EndsWith(".dat"))
				{	
					DeconToolsV2.Results.clsTransformResults transformResults = new DeconToolsV2.Results.clsTransformResults() ; 
					transformResults.ReadResults(fileName) ; 
					frm2DPeakProcessing frmTwoD = new frm2DPeakProcessing(transformResults) ;
					frmTwoD.HornTransformParameters = mobjTransformParameters ; 
					frmTwoD.PeakProcessorParameters = mobjPeakParameters ;
					frmTwoD.FTICRPreProcessOptions = mobjFTICRRawPreProcessParameters ;
					mMediator.RequestFormOpen(frmTwoD) ; 
				}	
				else if(fileName.EndsWith(".tic"))
				{
					index++ ; 
					string file_name_without_path = "" ; 

					if (index > 0)
					{
						file_name_without_path = fileName.Substring(index, fileName.Length-index) ; 							
					}
					
					frmTICViewer frmTIC = new frmTICViewer() ; 
					frmTIC.mFileName = fileName  ; 
					frmTIC.mFileNameForHeader = file_name_without_path ; 
					frmTIC.LoadIcr2lsTICFile() ; 				
					mMediator.RequestFormOpen(frmTIC) ;
				}
				else
				{
					
					Console.WriteLine("File cannot be dragged and dropped") ; 
				}
			}
		}

		private void frmDecon2LS_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
			{
				e.Effect = DragDropEffects.All;
			}
		}

		private void menuItem_Options_Click(object sender, System.EventArgs e)
		{
			try
			{
					// time to expose the options. 
					frmTransformOptions frmOptions = new frmTransformOptions(mobjPeakParameters, mobjTransformParameters, 
						mobjFTICRRawPreProcessParameters, mobjDTAGenerationParameters) ; 
					DialogResult result = frmOptions.ShowDialog(this) ; 
					if (result == DialogResult.Cancel)
						return ; 
				
					//this fills the local parameters with those from frmOptions
					frmOptions.GetTransformOptions(mobjTransformParameters) ; 
					frmOptions.GetMiscellaneousOptions(mobjTransformParameters) ; 
					frmOptions.GetDTASettings(mobjDTAGenerationParameters) ; 
					frmOptions.GetPeakPickingOptions(mobjPeakParameters) ; 
					frmOptions.GetFTICRRawPreProcessing(mobjFTICRRawPreProcessParameters) ; 
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message + ex.StackTrace) ; 
			}
		
		}

		private void mFileTreeView_BeforeExpand(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
		{

//			if (e.Node.Tag is CategorizedInfo)
//			{
//				MessageBox.Show(((CategorizedInfo)(e.Node.Tag)).Text);
//			}

		}



	}
}
