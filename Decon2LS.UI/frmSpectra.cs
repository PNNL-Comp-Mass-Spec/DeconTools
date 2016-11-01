// Written by Navdeep Jaitly and Anoop Mayampurath for the Department of Energy (PNNL, Richland, WA)
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
using System.Threading;
using System.Windows.Forms;
using PNNL.Controls;

namespace Decon2LS
{
    /// <summary>
    /// Summary description for frmSpectra.
    /// </summary>
    public class frmSpectra : PNNL.Controls.CategorizedForm, IMediatedForm
    {
        #region CategorizedInfo
        static frmSpectra()
        {
            // Load up category info, attempt to find icons registered with the operating system.
            Icon mainIcon = IconUtils.LoadIconFromAssembly(typeof(frmSpectra), "Icons.Spectra.ico", 16, 16);
            MainCategory = new CategoryInfo("Files", mainIcon);
            XCaliburCategory = CategoryForType("XCalibur", IconUtils.GetIconForFileType("raw", IconSize.Small, false, false, false));
            AgilentCategory = CategoryForType("Agilent", IconUtils.GetIconForFileType("wiff", IconSize.Small, false, false, false));
            MicromassCategory = CategoryForType("Micromass",
                IconUtils.AttemptIconForFile(new String[] {@"C:\MassLynx\mlynx.exe", 
                                                              @"C:\MassLynx\mlynx3.exe",
                                                              @"C:\MassLynx\mlynx4.exe",
                                                              @"C:\MassLynx\mlynx5.exe",
                                                              @"C:\Program Files\MassLynx\mlynx.exe", 
                                                              @"C:\Program Files\MassLynx\mlynx3.exe",
                                                              @"C:\Program Files\MassLynx\mlynx4.exe",
                                                              @"C:\Program Files\MassLynx\mlynx5.exe"},
                IconSize.Small, false, false, false));
            // Who knows what's associated with .s files
            SFileCategory = CategoryForType("S-File", IconUtils.LoadIconFromAssembly(typeof(frmSpectra), "Icons.SFile.ico", 16, 16));
            BrukerCategory = CategoryForType("Bruker", IconUtils.GetIconForFileType("ser", IconSize.Small, false, false, false));
        }


        /// <summary>
        /// Adds the various Files categories to the fileView.
        /// </summary>
        /// <param name="fileView"></param>
        public static void InitializeCategories(ctlFileView fileView)
        {
            fileView.AddCategory(XCaliburCategory);
            fileView.AddCategory(AgilentCategory);
            fileView.AddCategory(MicromassCategory);
            fileView.AddCategory(SFileCategory);
            fileView.AddCategory(BrukerCategory);
        }

        private static CategoryInfo[] CategoryForType(String name, Icon icon)
        {
            CategoryInfo[] info = new CategoryInfo[2];
            info[0] = MainCategory;
            info[1] = new CategoryInfo(name, icon);
            return info;
        }

        private static readonly CategoryInfo MainCategory;
        private static readonly CategoryInfo[] XCaliburCategory;
        private static readonly CategoryInfo[] AgilentCategory;
        private static readonly CategoryInfo[] MicromassCategory;
        private static readonly CategoryInfo[] SFileCategory;
        private static readonly CategoryInfo[] BrukerCategory;

        #endregion

        private String mFileName;
        private DeconToolsV2.Readers.FileType menmFileType;
        private System.ComponentModel.IContainer components;

        private DeconToolsV2.Readers.clsRawData mobjRawData;
        private PNNL.Controls.ctlLineChart mctl_tic;
        private float[] marr_tic_values = null;
        private float[] marr_scan_times = null;



        const double TIC_HEIGHT_RATIO = 0.35;

        private PNNL.Controls.ArrayChartDataProvider m_tic_chart_data_provider;
        private PNNL.Controls.clsSeries mobj_tic_series;
        private PNNL.Controls.clsPlotParams mobj_tic_plt_params;

        private int mint_spectrum_num = 0;
        private double mdbl_spectrum_time = -1;

        private PNNL.Controls.PenProvider m_tic_current_scan_pen_provider;
        private System.Windows.Forms.ImageList mimg_list_toolbar;
        private EventHandler mStatusUpdateDelegate = null;

        // The mediator for this form
        private clsMediator mMediator;

        //NOTE: these don't seemed to be used anywhere.
        //		private bool mbln_overlap_mode = false ; 
        //		private bool mbln_summation_mode = false ; 
        //private bool mbln_tic_view = true ; 

        private bool mbln_processing = false;

        private System.Windows.Forms.Timer mStatusTimer;
        private ctlMassSpectrum mctl_spectrum;
        private System.Windows.Forms.Button btnCopytoClipboard;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel1;

        private Thread mthrd_tic;
        public frmSpectra()
        {

            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            mMediator = new clsMediator(this);

            this.mctl_tic.AddPostProcessor(new ChartPostRenderingProcessor(mctl_tic_PostRender), PostProcessPriority.Highest);
            Init();
        }


        private void Init()
        {
            try
            {


                mctl_spectrum = new ctlMassSpectrum();
                mctl_spectrum.Dock = DockStyle.Fill;
                panel1.Controls.Add(mctl_spectrum);
                mctl_spectrum.Mediator = mMediator;
                mctl_spectrum.mevntScanChanged += new Decon2LS.ctlMassSpectrum.ScanChangedEventHandler(mctl_spectrum_mevntScanChanged);

                marr_tic_values = new float[1];
                marr_scan_times = new float[1];

                this.m_tic_current_scan_pen_provider = new PenProvider();
                m_tic_current_scan_pen_provider.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                PNNL.Controls.DiamondShape shape = new PNNL.Controls.DiamondShape(3, false);
                mobj_tic_plt_params = new PNNL.Controls.clsPlotParams(shape, Color.DarkSlateGray, false, true, true);

                this.KeyDown += new KeyEventHandler(frmSpectra_KeyDown);
                this.mctl_tic.KeyDown += new KeyEventHandler(frmSpectra_KeyDown);
                this.mctl_spectrum.KeyDown += new KeyEventHandler(frmSpectra_KeyDown);

                mint_spectrum_num = -1;

                this.m_tic_chart_data_provider = new ArrayChartDataProvider();
                this.mobj_tic_series = new clsSeries(this.m_tic_chart_data_provider, this.mobj_tic_plt_params);
                this.mctl_tic.SeriesCollection.Add(mobj_tic_series);

                // Set category info
                this.Category = new CategoryInfo[] { frmSpectra.MainCategory, new CategoryInfo("Unopened") };
                this.CategorizedText = "Unopened";

                mobjRawData = new DeconToolsV2.Readers.clsRawData();

                mctl_tic.DefaultZoomHandler.SingleClickNoZoomPerformed += new SingleClickNoZoomHandler(TicDefaultZoomHandler_SingleClickNoZoomPerformed);

                this.frmSpectra_Resize(this, null);


            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }



        public DeconToolsV2.Peaks.clsPeakProcessorParameters PeakProcessorParameters
        {
            get
            {
                return mctl_spectrum.PeakProcessorParameters;
            }
            set
            {
                mctl_spectrum.PeakProcessorParameters = value;
                //				mctl_spectrum.PeakProcessorParameters = (DeconToolsV2.Peaks.clsPeakProcessorParameters) value.Clone() ; 
            }
        }

        public DeconToolsV2.DTAGeneration.clsDTAGenerationParameters DTAGenerationParameters
        {
            get
            {
                return mctl_spectrum.DTAGenerationParameters;
            }
            set
            {
                mctl_spectrum.DTAGenerationParameters = value;

                //				mctl_spectrum.DTAGenerationParameters = (DeconToolsV2.DTAGeneration.clsDTAGenerationParameters) value.Clone() ; 
            }
        }


        public DeconToolsV2.HornTransform.clsHornTransformParameters HornTransformParameters
        {
            get
            {
                return mctl_spectrum.HornTransformParameters;
            }
            set
            {
                //				mctl_spectrum.HornTransformParameters = 
                //					(DeconToolsV2.HornTransform.clsHornTransformParameters) value.Clone() ; 

                mctl_spectrum.HornTransformParameters = value;
            }
        }

        public DeconToolsV2.Peaks.clsPeakProcessor PeakProcessor
        {
            get
            {
                return mctl_spectrum.PeakProcessor;
            }
        }

        public DeconToolsV2.Readers.clsRawDataPreprocessOptions FTICRPreProcessOptions
        {
            get
            {
                return mctl_spectrum.FTICRPreProcessOptions;
            }
            set
            {
                mctl_spectrum.FTICRPreProcessOptions = value;
                if (mobjRawData != null)
                    mobjRawData.FTICRRawPreprocessOptions = value;
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
            this.components = new System.ComponentModel.Container();
            PNNL.Controls.PenProvider penProvider1 = new PNNL.Controls.PenProvider();
            PNNL.Controls.PenProvider penProvider2 = new PNNL.Controls.PenProvider();
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmSpectra));
            System.Configuration.AppSettingsReader configurationAppSettings = new System.Configuration.AppSettingsReader();
            this.mctl_tic = new PNNL.Controls.ctlLineChart();
            this.mimg_list_toolbar = new System.Windows.Forms.ImageList(this.components);
            this.mStatusTimer = new System.Windows.Forms.Timer(this.components);
            this.btnCopytoClipboard = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.mctl_tic)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mctl_tic
            // 
            this.mctl_tic.AutoViewPortOnAddition = true;
            this.mctl_tic.AutoViewPortOnSeriesChange = true;
            this.mctl_tic.AutoViewPortXBase = 0F;
            this.mctl_tic.AutoViewPortYAxis = true;
            this.mctl_tic.AutoViewPortYBase = 0F;
            this.mctl_tic.AxisAndLabelFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.mctl_tic.AxisAndLabelMaxFontSize = 15;
            this.mctl_tic.AxisAndLabelMinFontSize = 8;
            this.mctl_tic.BackColor = System.Drawing.Color.WhiteSmoke;
            this.mctl_tic.ChartBackgroundColor = System.Drawing.Color.White;
            this.mctl_tic.ChartLayout.LegendFraction = 0.2F;
            this.mctl_tic.ChartLayout.LegendLocation = PNNL.Controls.ChartLegendLocation.Right;
            this.mctl_tic.ChartLayout.MaxLegendHeight = 150;
            this.mctl_tic.ChartLayout.MaxLegendWidth = 250;
            this.mctl_tic.ChartLayout.MaxTitleHeight = 50;
            this.mctl_tic.ChartLayout.MinLegendHeight = 50;
            this.mctl_tic.ChartLayout.MinLegendWidth = 75;
            this.mctl_tic.ChartLayout.MinTitleHeight = 15;
            this.mctl_tic.ChartLayout.TitleFraction = 0.1F;
            this.mctl_tic.DefaultZoomHandler.Active = true;
            this.mctl_tic.DefaultZoomHandler.FillColor = System.Drawing.Color.FromArgb(((System.Byte)(60)), ((System.Byte)(119)), ((System.Byte)(136)), ((System.Byte)(153)));
            this.mctl_tic.DefaultZoomHandler.LineColor = System.Drawing.Color.Black;
            this.mctl_tic.Dock = System.Windows.Forms.DockStyle.Fill;
            penProvider1.Color = System.Drawing.Color.FromArgb(((System.Byte)(211)), ((System.Byte)(211)), ((System.Byte)(211)));
            penProvider1.Width = 1F;
            this.mctl_tic.GridLinePen = penProvider1;
            this.mctl_tic.HasLegend = false;
            this.mctl_tic.HilightColor = System.Drawing.Color.Magenta;
            this.mctl_tic.LabelOffset = 5F;
            this.mctl_tic.Legend.BackColor = System.Drawing.Color.Transparent;
            penProvider2.Color = System.Drawing.Color.Black;
            penProvider2.Width = 1F;
            this.mctl_tic.Legend.BorderPen = penProvider2;
            this.mctl_tic.Legend.Bounds = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.mctl_tic.Legend.ColumnWidth = 125;
            this.mctl_tic.Legend.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.mctl_tic.Legend.MaxFontSize = 12F;
            this.mctl_tic.Legend.MinFontSize = 6F;
            this.mctl_tic.Location = new System.Drawing.Point(3, 16);
            this.mctl_tic.Margins.BottomMarginFraction = 0.1F;
            this.mctl_tic.Margins.BottomMarginMax = 72;
            this.mctl_tic.Margins.BottomMarginMin = 30;
            this.mctl_tic.Margins.DefaultMarginFraction = 0.05F;
            this.mctl_tic.Margins.DefaultMarginMax = 15;
            this.mctl_tic.Margins.DefaultMarginMin = 5;
            this.mctl_tic.Margins.LeftMarginFraction = 0.2F;
            this.mctl_tic.Margins.LeftMarginMax = 150;
            this.mctl_tic.Margins.LeftMarginMin = 72;
            this.mctl_tic.Name = "mctl_tic";
            this.mctl_tic.NumXBins = 20;
            this.mctl_tic.PanWithArrowKeys = false;
            this.mctl_tic.Size = new System.Drawing.Size(690, 277);
            this.mctl_tic.TabIndex = 0;
            this.mctl_tic.Title = "Tic";
            this.mctl_tic.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.mctl_tic.TitleMaxFontSize = 50F;
            this.mctl_tic.TitleMinFontSize = 6F;
            this.mctl_tic.UseAutoViewPortYBase = true;
            this.mctl_tic.VerticalExpansion = 1.15F;
            this.mctl_tic.ViewPort = ((System.Drawing.RectangleF)(resources.GetObject("mctl_tic.ViewPort")));
            this.mctl_tic.XAxisLabel = "Scan Time";
            this.mctl_tic.YAxisLabel = "intensity";
            // 
            // mimg_list_toolbar
            // 
            this.mimg_list_toolbar.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            this.mimg_list_toolbar.ImageSize = new System.Drawing.Size(16, 16);
            this.mimg_list_toolbar.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("mimg_list_toolbar.ImageStream")));
            this.mimg_list_toolbar.TransparentColor = System.Drawing.Color.White;
            // 
            // btnCopytoClipboard
            // 
            this.btnCopytoClipboard.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnCopytoClipboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCopytoClipboard.Location = new System.Drawing.Point(3, 7);
            this.btnCopytoClipboard.Name = "btnCopytoClipboard";
            this.btnCopytoClipboard.Size = new System.Drawing.Size(79, 37);
            this.btnCopytoClipboard.TabIndex = 6;
            this.btnCopytoClipboard.Text = "Copy data to clipboard";
            this.toolTip1.SetToolTip(this.btnCopytoClipboard, "Copy data to clipboard (Shift + \'C\')");
            this.btnCopytoClipboard.Click += new System.EventHandler(this.btnCopytoClipboard_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.groupBox1.Controls.Add(this.btnCopytoClipboard);
            this.groupBox1.Controls.Add(this.mctl_tic);
            this.groupBox1.Location = new System.Drawing.Point(32, 16);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(696, 296);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(32, 328);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(696, 248);
            this.panel1.TabIndex = 8;
            // 
            // frmSpectra
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(1024, 654);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = ((bool)(configurationAppSettings.GetValue("frmSpectra.IsMdiContainer", typeof(bool))));
            this.Name = "frmSpectra";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Spectra";
            this.Resize += new System.EventHandler(this.frmSpectra_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.mctl_tic)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        # region "Loading of files and getting data"

        public void LoadRawFileWithTic(string file_name, DeconToolsV2.Readers.FileType fileType, bool show_tic)
        {
            if (show_tic)
            {
                LoadRawFileWithTic(file_name, fileType);
            }
            else
            {
                LoadRawFileWithoutTic(file_name, fileType);
            }
        }

        private void LoadRawFileWithTic(string file_name, DeconToolsV2.Readers.FileType fileType)
        {
            try
            {
                menmFileType = fileType;

                this.mFileName = file_name;
                this.Text = file_name;
                mobjRawData.LoadFile(file_name, fileType);
                mctl_spectrum.RawData = mobjRawData;

                // Add the ability to read in the tic over here.
                float[] ticIntensities = new float[1];
                float[] ticTimes = new float[1];
                if (menmFileType != DeconToolsV2.Readers.FileType.BRUKER)
                    mobjRawData.GetTicFromFile(ref marr_tic_values, ref marr_scan_times, false);
                else
                    mobjRawData.GetTicFromFile(ref marr_tic_values, ref marr_scan_times, true);

                mint_spectrum_num = mobjRawData.GetFirstScanNum();
                ShowSpectrumInSpectralChart(mint_spectrum_num, true, true);
                this.CategorizedIcon = IconUtils.GetIconForFile(file_name, IconSize.Small, false, false, false);
                this.CategorizedText = file_name;

                clsPlotParams plotParams = (clsPlotParams)mobj_tic_plt_params.Clone();
                plotParams.Name = "TIC";

                mobj_tic_series =
                    new clsSpectraSeries(new PNNL.Controls.ArrayChartDataProvider(marr_scan_times, marr_tic_values),
                    plotParams, file_name);

                mctl_tic.SeriesCollection.Add(mobj_tic_series);
                m_tic_chart_data_provider.SetData(marr_scan_times, marr_tic_values);
                mctl_tic.Title = "TIC";

                this.mctl_tic.ViewPortHistory.Clear();
                this.UpdateDetails();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void LoadRawFileWithoutTic(string file_name, DeconToolsV2.Readers.FileType fileType)
        {
            try
            {

                this.mFileName = file_name;
                this.Text = file_name;
                mobjRawData.LoadFile(file_name, fileType);
                mctl_spectrum.RawData = mobjRawData;
                mint_spectrum_num = mobjRawData.GetFirstScanNum();
                mdbl_spectrum_time = mobjRawData.GetScanTime(mint_spectrum_num);
                ShowSpectrumInSpectralChart(mint_spectrum_num, true, true);
                this.CategorizedIcon = IconUtils.GetIconForFile(file_name, IconSize.Small, false, false, false);
                this.CategorizedText = file_name;
                marr_tic_values = new float[0];
                marr_scan_times = new float[0];
                mctl_tic.Title = "Tic not computed yet";
                this.mctl_tic.ViewPortHistory.Clear();
                mctl_spectrum.ClearViewPort();
                this.UpdateDetails();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }


        /// <summary>
        /// Updates the details associated with the categorized view of this form.
        /// </summary>
        private void UpdateDetails()
        {
        }

        public void ShowSpectrumInSpectralChart(int spectrum_num, bool get_data, bool reset_viewport)
        {
            if (spectrum_num != mint_spectrum_num)
                mdbl_spectrum_time = mobjRawData.GetScanTime(spectrum_num);
            mctl_spectrum.ShowSpectrumInSpectralChart(spectrum_num, get_data, reset_viewport);
            mint_spectrum_num = mctl_spectrum.CurrentSpectrumNum;
            mctl_tic.Invalidate();
        }

        /// <summary>
        /// Loads an XCalibur file and associated tic.
        /// </summary>
        /// <param name="file_name">Path of Xcalibur file</param>
        public void LoadXCaliburFile(string file_name)
        {
            PeakProcessorParameters.ThresholdedData = true;
            PeakProcessor.ProfileType = DeconToolsV2.enmProfileType.PROFILE;
            mctl_spectrum.SetViewProperties(DeconToolsV2.Readers.FileType.FINNIGAN);
            LoadRawFileWithTic(file_name, DeconToolsV2.Readers.FileType.FINNIGAN);
            this.Category = XCaliburCategory;
        }

        public void LoadIMFTOFFile(string file_name)
        {
            PeakProcessor.ProfileType = DeconToolsV2.enmProfileType.PROFILE;
            mctl_spectrum.SetViewProperties(DeconToolsV2.Readers.FileType.PNNL_IMS);
            LoadRawFileWithTic(file_name, DeconToolsV2.Readers.FileType.PNNL_IMS);
        }
        public void LoadUIMFFile(string file_name)
        {
            PeakProcessor.ProfileType = DeconToolsV2.enmProfileType.PROFILE;
            mctl_spectrum.SetViewProperties(DeconToolsV2.Readers.FileType.PNNL_UIMF);
            LoadRawFileWithTic(file_name, DeconToolsV2.Readers.FileType.PNNL_UIMF);
        }

        /// <summary>
        /// Loads an Agilent TOF file and associated tic.
        /// </summary>
        /// <param name="file_name">Path of Agilent file</param>
        public void LoadAgilentTOFFile(string file_name)
        {
            PeakProcessor.ProfileType = DeconToolsV2.enmProfileType.PROFILE;
            mctl_spectrum.SetViewProperties(DeconToolsV2.Readers.FileType.AGILENT_TOF);
            LoadRawFileWithTic(file_name, DeconToolsV2.Readers.FileType.AGILENT_TOF);
            this.Category = AgilentCategory;
        }

        /// <summary>
        /// Loads a Micromass TOF file and associated tic.
        /// </summary>
        /// <param name="file_name">Path of Agilent file</param>
        public void LoadMicromassTOFFile(string file_name)
        {
            mctl_spectrum.SetViewProperties(DeconToolsV2.Readers.FileType.MICROMASSRAWDATA);
            LoadRawFileWithTic(file_name, DeconToolsV2.Readers.FileType.MICROMASSRAWDATA);
            PeakProcessor.ProfileType = DeconToolsV2.enmProfileType.PROFILE;
            this.Category = MicromassCategory;
        }

        public void LoadSFileICR2LSFormat(string file_name)
        {
            mctl_spectrum.SetViewProperties(DeconToolsV2.Readers.FileType.ICR2LSRAWDATA);
            LoadRawFileWithoutTic(file_name, DeconToolsV2.Readers.FileType.ICR2LSRAWDATA);
            this.Category = SFileCategory;
        }

        public void LoadSFileSUNExtrelFormat(string file_name)
        {
            mctl_spectrum.SetViewProperties(DeconToolsV2.Readers.FileType.SUNEXTREL);
            LoadRawFileWithoutTic(file_name, DeconToolsV2.Readers.FileType.SUNEXTREL);
            this.Category = SFileCategory;
        }

        public void LoadFileWithTicThreaded(string file_name, DeconToolsV2.Readers.FileType file_type)
        {
            // start loading in a different thread. 
            this.mFileName = file_name;
            this.menmFileType = file_type;

            ThreadStart processThreadStart = new ThreadStart(GetTic);
            mthrd_tic = new Thread(processThreadStart);
            mthrd_tic.Name = "Create Tic";
            mthrd_tic.IsBackground = true;
            mthrd_tic.Start();

            this.mStatusUpdateDelegate = new EventHandler(this.CheckTicLoadingStatusHandler);
            this.mStatusTimer.Tick += this.mStatusUpdateDelegate;
            this.mStatusTimer.Start();
            DialogResult result = mMediator.StatusForm.ShowDialog(this);

            mStatusTimer.Tick -= new EventHandler(this.mStatusUpdateDelegate);
            mStatusTimer.Stop();
            if (result == DialogResult.Cancel)
            {
                this.FinishOrAbortProcessing(true);
            }

            FinishOrAbortProcessing(false);
            this.Invalidate();
        }

        public void LoadMZXMLFile(string file_name)
        {
            mctl_spectrum.SetViewProperties(DeconToolsV2.Readers.FileType.MZXMLRAWDATA);
            LoadRawFileWithoutTic(file_name, DeconToolsV2.Readers.FileType.MZXMLRAWDATA);
            this.Category = SFileCategory;
        }


        private void GetTic()
        {
            mbln_processing = true;
            LoadRawFileWithTic(mFileName, menmFileType);
            mMediator.StatusForm.DialogResult = DialogResult.OK;
            this.BeginInvoke(new ThreadStart(InvokeTicLoadingFinished));
        }

        private void CheckTicLoadingStatusHandler(object sender, EventArgs args)
        {
            try
            {
                if (!mMediator.StatusForm.IsHandleCreated || mobjRawData == null)
                    return;
                bool cancel = false;
                mMediator.RaiseProgressMessage(sender, mobjRawData.PercentDone, ref cancel);
                mMediator.RaiseStatusMessage(sender, mobjRawData.StatusMessage, ref cancel);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void InvokeTicLoadingFinished()
        {
            mMediator.StatusForm.DialogResult = DialogResult.OK;
            mMediator.StatusForm.Hide();
        }

        private void FinishOrAbortProcessing(bool abort)
        {
            try
            {
                if (mbln_processing)
                {
                    this.mStatusTimer.Stop();
                    // doing its business.. someone wants to abort. 
                    if (mthrd_tic != null && mthrd_tic.IsAlive)
                    {
                        if (abort)
                        {
                            mthrd_tic.Abort();
                        }
                        // wait until it to fully terminate or abort
                        mthrd_tic.Join();
                    }
                    mbln_processing = false;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// Loads a Bruker File at scan number 1. The tic is not available as such and needs to be computed.
        /// </summary>
        /// <param name="folder_name">Passes in the name of the folder containing the data file and the header file(acqu)</param>
        public void LoadBrukerFile(string folder_name)
        {
            mctl_spectrum.SetViewProperties(DeconToolsV2.Readers.FileType.BRUKER);
            LoadRawFileWithoutTic(folder_name, DeconToolsV2.Readers.FileType.BRUKER);
            this.Category = BrukerCategory;
        }

        /// <summary>
        /// Loads a Bruker File at scan number 1. The tic is not available as such and needs to be computed.
        /// </summary>
        /// <param name="folder_name">Passes in the name of the folder containing the data file and the header file(acqu)</param>
        public void LoadBrukerAsciiFile(string folder_name)
        {
            mctl_spectrum.SetViewProperties(DeconToolsV2.Readers.FileType.BRUKER_ASCII);
            LoadRawFileWithoutTic(folder_name, DeconToolsV2.Readers.FileType.BRUKER_ASCII);
            this.Category = BrukerCategory;
        }


        #endregion


        private void frmSpectra_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                bool reset_viewport = true;
                Console.WriteLine("Key Press {0} {1}", e.KeyCode, e.Modifiers);
                if (e.KeyCode == Keys.Right && !e.Handled && e.Modifiers == Keys.None)
                {
                    ShowSpectrumInSpectralChart(mint_spectrum_num + 1, true, reset_viewport);
                    e.Handled = true;
                }
                if (e.KeyCode == Keys.Left && !e.Handled && e.Modifiers == Keys.None)
                {
                    ShowSpectrumInSpectralChart(mint_spectrum_num - 1, true, reset_viewport);
                    e.Handled = true;
                }
                if (e.KeyCode == Keys.Right && !e.Handled && e.Modifiers == Keys.Control)
                {
                    ShowSpectrumInSpectralChart(mint_spectrum_num + mobjRawData.GetNumScans() / 20, true, reset_viewport);
                    e.Handled = true;
                }
                if (e.KeyCode == Keys.Left && !e.Handled && e.Modifiers == Keys.Control)
                {
                    ShowSpectrumInSpectralChart(mint_spectrum_num - mobjRawData.GetNumScans() / 20, true, reset_viewport);
                    e.Handled = true;
                }
                if (e.KeyCode == Keys.Right && !e.Handled && e.Modifiers == Keys.Shift)
                {
                    ShowSpectrumInSpectralChart(mint_spectrum_num + 20, true, reset_viewport);
                    e.Handled = true;
                }
                if (e.KeyCode == Keys.Left && !e.Handled && e.Modifiers == Keys.Shift)
                {
                    ShowSpectrumInSpectralChart(mint_spectrum_num - 20, true, reset_viewport);
                    e.Handled = true;
                }
                if (e.KeyCode == Keys.T && !e.Handled && e.Modifiers == Keys.Shift)
                {
                    this.mctl_spectrum.InitiateAndExecuteTransform();

                }
                if (e.KeyCode == Keys.F && !e.Handled && e.Modifiers == Keys.Shift)
                {
                    this.mctl_spectrum.findPeaks();
                }

                if (e.KeyCode == Keys.S && !e.Handled && e.Modifiers == Keys.Shift)
                {
                    this.mctl_spectrum.SmoothWithSavitzkyGolay();
                }

                if (e.KeyCode == Keys.C && !e.Handled && e.Modifiers == Keys.Shift)
                {
                    clsClipboardUtility.CopyXYValuesToClipboard(this.marr_scan_times, this.marr_tic_values);
                }




            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// Draws the line indicating the currently active spectrum scan in the 
        /// tic chart.  Draws after the chart is drawn.
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="args"></param>
        private void mctl_tic_PostRender(PNNL.Controls.ctlChartBase chart, PNNL.Controls.PostRenderEventArgs args)
        {
            // If we have a tic, attempt to draw the spectrum line on it
            if (this.marr_tic_values != null)
            {
                try
                {
                    if (this.mint_spectrum_num <= 0)
                        return;
                    // Get the time that the spectrum occurs in the overall run
                    double spectrumTime = mdbl_spectrum_time;
                    // Convert into a pixel offset relative to the left of the charting area
                    float xChartValue = this.mctl_tic.GetScreenPixelX((float)spectrumTime);
                    // Draw the line onto the chart
                    using (Pen p = this.m_tic_current_scan_pen_provider.Pen)
                    {
                        args.Graphics.DrawLine(p, xChartValue, 0, xChartValue,
                            this.mctl_tic.MaxChartAreaYPixel);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }


        private void mMediator_mevnt_Zoom(object sender, clsZoomEventArgs event_args)
        {
        }


        // deleted the Toolbar containing four buttons; two of which didn't do anything see below for more notes
        private void mtlb_spectra_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
        {


            // 2009_02_19: Gord decommissioned these buttons. 'mbln_overlap_mode'+ 'mbln_summation_mode' not ever used
            // Also, people wanted to apply the global options, instead of creating a new instance of options
            // Moved the theoretical fit to ctlMassSpectrum

            //				if (e.Button == mtlb_button_overlap_mode)
            //				{
            //					mbln_overlap_mode = mtlb_button_overlap_mode.Pushed;
            //				}
            //				else if(e.Button == mtlb_button_summation_mode)
            //				{
            //					mbln_summation_mode = mtlb_button_summation_mode.Pushed;
            //				}

            //				if (e.Button == mtlb_button_options)
            //				{
            //					// time to expose the options. 
            //					frmTransformOptions frmOptions = new frmTransformOptions(this.PeakProcessorParameters, this.HornTransformParameters, 
            //						this.FTICRPreProcessOptions, this.DTAGenerationParameters) ; 
            //					DialogResult result = frmOptions.ShowDialog(this) ; 
            //					if (result == DialogResult.Cancel)
            //						return ; 
            //					frmOptions.GetPeakPickingOptions(this.PeakProcessorParameters) ; 
            //					frmOptions.GetTransformOptions(this.HornTransformParameters) ; 
            //					frmOptions.GetMiscellaneousOptions(this.HornTransformParameters) ; 
            //					frmOptions.GetFTICRRawPreProcessing(this.FTICRPreProcessOptions) ;
            //				}
            //				else if (e.Button == mtlb_button_fit_formula)
            //				{
            //					 
            //				}
            //			}
            //			catch (Exception ex)
            //			{
            //				MessageBox.Show(this, ex.Message + ex.StackTrace) ; 
            //			}
        }


        #region IMediatedForm Members

        public clsMediator Mediator
        {
            get
            {
                return mMediator;
            }
        }

        #endregion

        private void mctl_spectrum_mevntScanChanged(object sender, int new_scan)
        {
            bool reset_viewport = true;
            mint_spectrum_num = new_scan;
            ShowSpectrumInSpectralChart(mint_spectrum_num, true, reset_viewport);
        }

        private int GetScanIndex(float scan_time)
        {
            int startIndex = 0;
            int stopIndex = marr_scan_times.Length - 1;
            int midIndex;

            if (scan_time <= marr_scan_times[0])
                return 1;
            if (scan_time > marr_scan_times[stopIndex])
                return stopIndex;

            while (stopIndex - startIndex > 1)
            {
                midIndex = (startIndex + stopIndex) / 2;
                if (marr_scan_times[midIndex] > scan_time)
                    stopIndex = midIndex;
                else
                    startIndex = midIndex;
            }
            return startIndex;
        }



        private void TicDefaultZoomHandler_SingleClickNoZoomPerformed(object sender, MouseEventArgs e)
        {
            // mz value will be drawn along the x axis. 
            float time_focus = mctl_tic.GetChartX(mctl_tic.GetChartAreaX(e.X));
            int scanIndex = GetScanIndex(time_focus) + 1;
            if (scanIndex >= mobjRawData.GetFirstScanNum() && scanIndex < mobjRawData.GetNumScans())
                ShowSpectrumInSpectralChart(scanIndex, true, true);
            // Anoop: changed back to true, happened with .imf data
            // SHOULD WE CHANGE BEHAVIOUR OF ZOOMING. PERHAPS ON ARROW BUTTONS 
            // WE SHOULD STAY IN THE VIEWPORT BUT ON CLICKING ZOOM OUT. 
            // Anoop: changed reset_viewport from false to true
            // Everytime you loaded in a .raw data set, on clicking a scan on the tic the spectrum was not
            // being shown. Only after hitting the right and left arrows was it made clear. thus, emulating
            //what is done there.

        }

        private void btnCopytoClipboard_Click(object sender, System.EventArgs e)
        {
            clsClipboardUtility.CopyXYValuesToClipboard(this.marr_scan_times, this.marr_tic_values);

        }

        private void frmSpectra_Resize(object sender, System.EventArgs e)
        {
            this.panel1.Left = 1;
            this.groupBox1.Left = 1;
            this.groupBox1.Width = this.Width;
            this.panel1.Width = this.Width;

            this.groupBox1.Top = 1;
            this.groupBox1.Height = (int)(this.Height * TIC_HEIGHT_RATIO);
            this.panel1.Top = this.groupBox1.Top + this.groupBox1.Height;
            this.panel1.Height = this.Height - this.groupBox1.Height - 40;

        }


    }

    /// <summary>
    /// Spectra customized clsSeries with special copy support for including file name
    /// </summary>
    internal class clsSpectraSeries : clsSeries
    {
        private String mFilename;
        internal clsSpectraSeries(IChartDataProvider provider, clsPlotParams plotParams, String filename) :
            base(provider, plotParams)
        {
            this.mFilename = filename;
        }

        public override clsSeries CopySeries()
        {
            clsSeries series = base.CopySeries();
            series.PlotParams.Name += " (" + new System.IO.FileInfo(mFilename).Name + ")";
            return series;
        }


    }
}
