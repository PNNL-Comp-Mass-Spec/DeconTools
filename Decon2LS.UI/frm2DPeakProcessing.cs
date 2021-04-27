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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;

namespace Decon2LS
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class frm2DPeakProcessing : PNNL.Controls.CategorizedForm
    {
        public class clsRawDataComparer : IComparer
        {
            int IComparer.Compare(object x_obj, object y_obj)
            {
                var x = (Engine.Results.LcmsPeak)x_obj;
                var y = (Engine.Results.LcmsPeak)y_obj;
                var x_int = x.Intensity;
                var y_int = y.Intensity;
                return x_int.CompareTo(y_int);
            }
        }

        private PNNL.Controls.ExpandPanel mexpandPanelBottom;
        private System.Windows.Forms.Splitter msplitterBottom;
        private System.Windows.Forms.Panel mpanelCenter;

        private DeconToolsV2.Results.clsTransformResults mobj_results;
        private Engine.Results.LcmsPeak[] marr_peaks = null;
        private PNNL.Controls.MS.ctl2DPeaks mctl_rawdata;

        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.ToolBar mtlbMain;
        private System.Windows.Forms.ToolBarButton mtlbButtonSave;
        private System.Windows.Forms.ToolBarButton mtlbButtonLinkRaw;
        private System.Windows.Forms.ToolBarButton mtlnButtonBack;
        private System.Windows.Forms.ToolBarButton mtlnButtonForward;
        private System.Windows.Forms.ImageList mimageListIcons;
        private System.Windows.Forms.ToolBarButton mtlbButtonOpen;
        private PNNL.Controls.MS.ctlSpectrum ctlChartTopPane;
        private System.ComponentModel.IContainer components;

        private PNNL.Controls.clsSeries mobj_top_series;
        private PNNL.Controls.clsPlotParams mobj_elution_profile_plt_params;

        private static readonly Icon TwoDPeakIcon;
        private static readonly PNNL.Controls.CategoryInfo[] TwoDPeakCategory;
        private System.Windows.Forms.ToolBarButton mtlnButtonOverlay;
        private static readonly String CategorizedTextString = "2D Peak View";

        private float mflt_sic_mz_tolerance = 0.1F;
        private float mfltMinIntensity = 0.0F;

        private float[] marr_sic_scans;
        private float[] marr_sic_intensities;

        private float[] marr_spectrum_peak_mzs;
        private float[] marr_spectrum_peak_intensities;

        private float[] marr_scans;
        private float[] marr_intensities;
        private float[] marr_mzs;
        private DeconToolsV2.Readers.clsRawData mobj_raw_data;
        private System.Windows.Forms.Splitter splitter2;

        private ctlMassSpectrum mctl_spectrum;
        private System.Windows.Forms.ToolBarButton mtlnButtonLinkZoom;
        private int mint_num_pixel_screen_tolerance = 6;
        private System.Windows.Forms.ToolBarButton mtlnButtonSelectBackground;

        private clsMediator mMediator;
        static frm2DPeakProcessing()
        {
            // Load the icons used for categorization info from embedded resources.
            TwoDPeakIcon = PNNL.Controls.IconUtils.LoadIconFromAssembly(typeof(frm2DPeakProcessing), "Icons.RawData.ico", 16, 16);
            TwoDPeakCategory = new PNNL.Controls.CategoryInfo[] { new PNNL.Controls.CategoryInfo(CategorizedTextString, TwoDPeakIcon) };
        }

        #region "Constructors"
        public frm2DPeakProcessing()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            mobj_raw_data = null;
            Init();
        }

        public frm2DPeakProcessing(DeconToolsV2.Results.clsTransformResults results)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //

            Init();
            SetData(results);
        }

        private void Init()
        {
            mMediator = new clsMediator(this);

            mctl_spectrum = new ctlMassSpectrum();
            mctl_spectrum.Dock = DockStyle.Fill;
            mexpandPanelBottom.Controls.Add(mctl_spectrum);
            mctl_spectrum.Mediator = mMediator;
            mctl_spectrum.mevntScanChanged += new Decon2LS.ctlMassSpectrum.ScanChangedEventHandler(mctl_spectrum_mevntScanChanged);

            // Set initial categorized info
            this.Category = TwoDPeakCategory;

            mobj_elution_profile_plt_params = new PNNL.Controls.clsPlotParams(new PNNL.Controls.BubbleShape(2, false), Color.Red, false, true, true);

            mctl_rawdata.DefaultZoomHandler.SingleClickNoZoomPerformed += new PNNL.Controls.SingleClickNoZoomHandler(DefaultZoomHandler_SingleClickNoZoomPerformed);
            mctl_rawdata.MZHorizontal = true;
            mctl_rawdata.ViewPortChanged += new PNNL.Controls.ViewPortChangedHandler(mctl_rawdata_ViewPortChanged);
            //SetPlotStructures() ;
        }
        #endregion

        private void SetPlotStructures()
        {
            try
            {
                ctlChartTopPane.SeriesCollection.Clear();
                this.ctlChartTopPane.ViewPortHistory.Clear();

                float minMZ = 0, maxMZ = 0;

                minMZ = mctl_rawdata.ViewPort.Left;
                maxMZ = mctl_rawdata.ViewPort.Right;

                this.mctl_rawdata.XAxisLabel = "m/z";
                this.mctl_rawdata.YAxisLabel = "scan #";

                this.ctlChartTopPane.XAxisLabel = "Intensity";
                this.ctlChartTopPane.YAxisLabel = "scan#";

                if (marr_sic_intensities != null)
                {
                    mobj_top_series = new PNNL.Controls.clsSeries(ref marr_sic_intensities, ref marr_sic_scans, mobj_elution_profile_plt_params);
                    this.ctlChartTopPane.SeriesCollection.Add(mobj_top_series);
                }

                mctl_spectrum.SetPeakSeriesData(mctl_rawdata.FocusScan, marr_spectrum_peak_mzs, marr_spectrum_peak_intensities, minMZ, maxMZ);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }

        #region "Category Info"

        /// <summary>
        /// Adds the Mercury category to the file view.
        /// </summary>
        /// <param name="fileView"></param>
        public static void InitializeCategories(PNNL.Controls.ctlFileView fileView)
        {
            try
            {
                fileView.AddCategory(TwoDPeakCategory);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        #endregion

        public void SetData(DeconToolsV2.Results.clsTransformResults results)
        {
            try
            {
                mobj_results = results;
                marr_peaks = new Engine.Results.LcmsPeak[1];
                var min_intensity = float.MaxValue;
                var max_intensity = float.MinValue;
                mobj_results.GetRawData(out marr_peaks);
                if (marr_peaks.Length == 0)
                    return;

                var myComparer = new clsRawDataComparer();

                Array.Sort(marr_peaks, myComparer);
                min_intensity = (float)marr_peaks[0].Intensity;
                max_intensity = (float)marr_peaks[marr_peaks.Length - 1].Intensity;

                var startIndex = 0;
                if (min_intensity < mfltMinIntensity)
                {
                    while (startIndex < marr_peaks.Length && marr_peaks[startIndex].Intensity < mfltMinIntensity)
                        startIndex++;
                }

                if (startIndex == marr_peaks.Length)
                    return;

                if (max_intensity < 0)
                {
                    max_intensity = 1f;
                }

                marr_scans = new float[marr_peaks.Length - startIndex];
                marr_mzs = new float[marr_peaks.Length - startIndex];
                marr_intensities = new float[marr_peaks.Length - startIndex];

                for (var i = startIndex; i < marr_peaks.Length; i++)
                {
                    marr_scans[i - startIndex] = marr_peaks[i].ScanNum;
                    marr_mzs[i - startIndex] = (float)marr_peaks[i].Mz;
                    marr_intensities[i - startIndex] = (float)marr_peaks[i].Intensity;
                }

                mctl_rawdata.SetData(marr_scans, marr_mzs, marr_intensities, min_intensity, max_intensity);

                var mono_masses = results.MonoMasses;
                var charges = results.Charges;
                var scans = results.Scans;
                mctl_rawdata.SetDeisotopedData(scans, mono_masses, charges);

                this.Text = mobj_results.FileName;
                var slashIndex = mobj_results.FileName.LastIndexOf("\\") + 1;
                this.CategorizedText = mobj_results.FileName.Substring(slashIndex, mobj_results.FileName.Length - slashIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
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
            var resources = new System.Resources.ResourceManager(typeof(frm2DPeakProcessing));
            var penProvider1 = new PNNL.Controls.PenProvider();
            var penProvider2 = new PNNL.Controls.PenProvider();
            var penProvider3 = new PNNL.Controls.PenProvider();
            var penProvider4 = new PNNL.Controls.PenProvider();
            this.mexpandPanelBottom = new PNNL.Controls.ExpandPanel(284);
            this.msplitterBottom = new System.Windows.Forms.Splitter();
            this.ctlChartTopPane = new PNNL.Controls.MS.ctlSpectrum();
            this.mpanelCenter = new System.Windows.Forms.Panel();
            this.mctl_rawdata = new PNNL.Controls.MS.ctl2DPeaks();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.mtlbMain = new System.Windows.Forms.ToolBar();
            this.mtlbButtonOpen = new System.Windows.Forms.ToolBarButton();
            this.mtlbButtonSave = new System.Windows.Forms.ToolBarButton();
            this.mtlbButtonLinkRaw = new System.Windows.Forms.ToolBarButton();
            this.mtlnButtonBack = new System.Windows.Forms.ToolBarButton();
            this.mtlnButtonForward = new System.Windows.Forms.ToolBarButton();
            this.mtlnButtonOverlay = new System.Windows.Forms.ToolBarButton();
            this.mtlnButtonLinkZoom = new System.Windows.Forms.ToolBarButton();
            this.mimageListIcons = new System.Windows.Forms.ImageList(this.components);
            this.mtlnButtonSelectBackground = new System.Windows.Forms.ToolBarButton();
            ((System.ComponentModel.ISupportInitialize)(this.mexpandPanelBottom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ctlChartTopPane)).BeginInit();
            this.mpanelCenter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mctl_rawdata)).BeginInit();
            this.SuspendLayout();
            //
            // mexpandPanelBottom
            //
            this.mexpandPanelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.mexpandPanelBottom.DockPadding.All = 10;
            this.mexpandPanelBottom.ExpandImage = ((System.Drawing.Image)(resources.GetObject("mexpandPanelBottom.ExpandImage")));
            this.mexpandPanelBottom.HeaderRightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mexpandPanelBottom.HeaderTextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.mexpandPanelBottom.Location = new System.Drawing.Point(0, 558);
            this.mexpandPanelBottom.Name = "mexpandPanelBottom";
            this.mexpandPanelBottom.Size = new System.Drawing.Size(1128, 304);
            this.mexpandPanelBottom.TabIndex = 0;
            //
            // msplitterBottom
            //
            this.msplitterBottom.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.msplitterBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.msplitterBottom.Location = new System.Drawing.Point(0, 552);
            this.msplitterBottom.Name = "msplitterBottom";
            this.msplitterBottom.Size = new System.Drawing.Size(1128, 6);
            this.msplitterBottom.TabIndex = 1;
            this.msplitterBottom.TabStop = false;
            //
            // ctlChartTopPane
            //
            this.ctlChartTopPane.AutoViewPortOnAddition = true;
            this.ctlChartTopPane.AutoViewPortOnSeriesChange = true;
            this.ctlChartTopPane.AutoViewPortXBase = 0F;
            this.ctlChartTopPane.AutoViewPortYBase = 0F;
            this.ctlChartTopPane.AxisAndLabelFont = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.ctlChartTopPane.AxisAndLabelMaxFontSize = 16;
            this.ctlChartTopPane.AxisAndLabelMinFontSize = 12;
            this.ctlChartTopPane.ChartBackgroundColor = System.Drawing.Color.White;
            this.ctlChartTopPane.ChartLayout.LegendFraction = 0.2F;
            this.ctlChartTopPane.ChartLayout.LegendLocation = PNNL.Controls.ChartLegendLocation.Right;
            this.ctlChartTopPane.ChartLayout.MaxLegendHeight = 150;
            this.ctlChartTopPane.ChartLayout.MaxLegendWidth = 250;
            this.ctlChartTopPane.ChartLayout.MaxTitleHeight = 50;
            this.ctlChartTopPane.ChartLayout.MinLegendHeight = 50;
            this.ctlChartTopPane.ChartLayout.MinLegendWidth = 75;
            this.ctlChartTopPane.ChartLayout.MinTitleHeight = 15;
            this.ctlChartTopPane.ChartLayout.TitleFraction = 0.1F;
            this.ctlChartTopPane.DefaultZoomHandler.Active = true;
            this.ctlChartTopPane.DefaultZoomHandler.FillColor = System.Drawing.Color.FromArgb(((System.Byte)(60)), ((System.Byte)(119)), ((System.Byte)(136)), ((System.Byte)(153)));
            this.ctlChartTopPane.DefaultZoomHandler.LineColor = System.Drawing.Color.Black;
            this.ctlChartTopPane.Dock = System.Windows.Forms.DockStyle.Right;
            this.ctlChartTopPane.FWHMFont = new System.Drawing.Font("Times New Roman", 8F);
            this.ctlChartTopPane.FWHMLineWidth = 1F;
            this.ctlChartTopPane.FWHMPeakColor = System.Drawing.Color.Purple;
            penProvider1.Color = System.Drawing.Color.FromArgb(((System.Byte)(211)), ((System.Byte)(211)), ((System.Byte)(211)));
            penProvider1.Width = 1F;
            this.ctlChartTopPane.GridLinePen = penProvider1;
            this.ctlChartTopPane.HasLegend = false;
            this.ctlChartTopPane.HilightColor = System.Drawing.Color.Magenta;
            this.ctlChartTopPane.LabelOffset = 0F;
            this.ctlChartTopPane.Legend.BackColor = System.Drawing.Color.Transparent;
            penProvider2.Color = System.Drawing.Color.Black;
            penProvider2.Width = 1F;
            this.ctlChartTopPane.Legend.BorderPen = penProvider2;
            this.ctlChartTopPane.Legend.Bounds = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.ctlChartTopPane.Legend.ColumnWidth = 125;
            this.ctlChartTopPane.Legend.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.ctlChartTopPane.Legend.MaxFontSize = 12F;
            this.ctlChartTopPane.Legend.MinFontSize = 6F;
            this.ctlChartTopPane.LineWidth = 1F;
            this.ctlChartTopPane.Location = new System.Drawing.Point(848, 0);
            this.ctlChartTopPane.Margins.BottomMarginFraction = 0.1F;
            this.ctlChartTopPane.Margins.BottomMarginMax = 72;
            this.ctlChartTopPane.Margins.BottomMarginMin = 30;
            this.ctlChartTopPane.Margins.DefaultMarginFraction = 0.05F;
            this.ctlChartTopPane.Margins.DefaultMarginMax = 15;
            this.ctlChartTopPane.Margins.DefaultMarginMin = 5;
            this.ctlChartTopPane.Margins.LeftMarginFraction = 0.2F;
            this.ctlChartTopPane.Margins.LeftMarginMax = 150;
            this.ctlChartTopPane.Margins.LeftMarginMin = 72;
            this.ctlChartTopPane.MarkerSize = 5;
            this.ctlChartTopPane.MinPixelForFWHM = 5F;
            this.ctlChartTopPane.Name = "ctlChartTopPane";
            this.ctlChartTopPane.NumFWHM = 1F;
            this.ctlChartTopPane.NumXBins = 20;
            this.ctlChartTopPane.PeakColor = System.Drawing.Color.Red;
            this.ctlChartTopPane.PeakLabelRelativeHeightPercent = 5F;
            this.ctlChartTopPane.PeakLineEndCap = System.Drawing.Drawing2D.LineCap.Flat;
            this.ctlChartTopPane.Size = new System.Drawing.Size(280, 521);
            this.ctlChartTopPane.TabIndex = 2;
            this.ctlChartTopPane.Title = "Elution Profile";
            this.ctlChartTopPane.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 18F);
            this.ctlChartTopPane.TitleMaxFontSize = 18F;
            this.ctlChartTopPane.TitleMinFontSize = 16F;
            this.ctlChartTopPane.VerticalExpansion = 1F;
            this.ctlChartTopPane.ViewPort = ((System.Drawing.RectangleF)(resources.GetObject("ctlChartTopPane.ViewPort")));
            this.ctlChartTopPane.XAxisLabel = "Intensity";
            this.ctlChartTopPane.YAxisLabel = "scan#";
            //
            // mpanelCenter
            //
            this.mpanelCenter.Controls.Add(this.mctl_rawdata);
            this.mpanelCenter.Controls.Add(this.splitter2);
            this.mpanelCenter.Controls.Add(this.ctlChartTopPane);
            this.mpanelCenter.Controls.Add(this.splitter1);
            this.mpanelCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mpanelCenter.Location = new System.Drawing.Point(0, 31);
            this.mpanelCenter.Name = "mpanelCenter";
            this.mpanelCenter.Size = new System.Drawing.Size(1128, 521);
            this.mpanelCenter.TabIndex = 4;
            //
            // mctl_rawdata
            //
            this.mctl_rawdata.AutoViewPortOnAddition = true;
            this.mctl_rawdata.AutoViewPortOnSeriesChange = true;
            this.mctl_rawdata.AutoViewPortXBase = 0F;
            this.mctl_rawdata.AutoViewPortYBase = 0F;
            this.mctl_rawdata.AxisAndLabelFont = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.mctl_rawdata.AxisAndLabelMaxFontSize = 18;
            this.mctl_rawdata.AxisAndLabelMinFontSize = 12;
            this.mctl_rawdata.ChartBackgroundColor = System.Drawing.Color.White;
            this.mctl_rawdata.ChartLayout.LegendFraction = 0.2F;
            this.mctl_rawdata.ChartLayout.LegendLocation = PNNL.Controls.ChartLegendLocation.Right;
            this.mctl_rawdata.ChartLayout.MaxLegendHeight = 150;
            this.mctl_rawdata.ChartLayout.MaxLegendWidth = 250;
            this.mctl_rawdata.ChartLayout.MaxTitleHeight = 50;
            this.mctl_rawdata.ChartLayout.MinLegendHeight = 50;
            this.mctl_rawdata.ChartLayout.MinLegendWidth = 75;
            this.mctl_rawdata.ChartLayout.MinTitleHeight = 15;
            this.mctl_rawdata.ChartLayout.TitleFraction = 0.1F;
            this.mctl_rawdata.DefaultZoomHandler.Active = true;
            this.mctl_rawdata.DefaultZoomHandler.FillColor = System.Drawing.Color.FromArgb(((System.Byte)(60)), ((System.Byte)(119)), ((System.Byte)(136)), ((System.Byte)(153)));
            this.mctl_rawdata.DefaultZoomHandler.LineColor = System.Drawing.Color.Black;
            this.mctl_rawdata.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mctl_rawdata.FocusMZ = 0.5F;
            this.mctl_rawdata.FocusScan = 0;
            penProvider3.Color = System.Drawing.Color.FromArgb(((System.Byte)(211)), ((System.Byte)(211)), ((System.Byte)(211)));
            penProvider3.Width = 1F;
            this.mctl_rawdata.GridLinePen = penProvider3;
            this.mctl_rawdata.HasLegend = false;
            this.mctl_rawdata.HilightColor = System.Drawing.Color.Magenta;
            this.mctl_rawdata.Legend.BackColor = System.Drawing.Color.Transparent;
            penProvider4.Color = System.Drawing.Color.Black;
            penProvider4.Width = 1F;
            this.mctl_rawdata.Legend.BorderPen = penProvider4;
            this.mctl_rawdata.Legend.Bounds = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.mctl_rawdata.Legend.ColumnWidth = 125;
            this.mctl_rawdata.Legend.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F);
            this.mctl_rawdata.Legend.MaxFontSize = 12F;
            this.mctl_rawdata.Legend.MinFontSize = 6F;
            this.mctl_rawdata.Location = new System.Drawing.Point(3, 0);
            this.mctl_rawdata.Margins.BottomMarginFraction = 0.1F;
            this.mctl_rawdata.Margins.BottomMarginMax = 72;
            this.mctl_rawdata.Margins.BottomMarginMin = 30;
            this.mctl_rawdata.Margins.DefaultMarginFraction = 0.05F;
            this.mctl_rawdata.Margins.DefaultMarginMax = 15;
            this.mctl_rawdata.Margins.DefaultMarginMin = 5;
            this.mctl_rawdata.Margins.LeftMarginFraction = 0.2F;
            this.mctl_rawdata.Margins.LeftMarginMax = 150;
            this.mctl_rawdata.Margins.LeftMarginMin = 72;
            this.mctl_rawdata.MZHorizontal = false;
            this.mctl_rawdata.Name = "mctl_rawdata";
            this.mctl_rawdata.Size = new System.Drawing.Size(839, 521);
            this.mctl_rawdata.TabIndex = 0;
            this.mctl_rawdata.Title = "Survey Profile";
            this.mctl_rawdata.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 18F);
            this.mctl_rawdata.TitleMaxFontSize = 18F;
            this.mctl_rawdata.TitleMinFontSize = 16F;
            this.mctl_rawdata.VerticalExpansion = 1F;
            this.mctl_rawdata.ViewPort = ((System.Drawing.RectangleF)(resources.GetObject("mctl_rawdata.ViewPort")));
            this.mctl_rawdata.XAxisLabel = "m/z";
            this.mctl_rawdata.YAxisLabel = "scan #";
            //
            // splitter2
            //
            this.splitter2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter2.Location = new System.Drawing.Point(842, 0);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(6, 521);
            this.splitter2.TabIndex = 4;
            this.splitter2.TabStop = false;
            //
            // splitter1
            //
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 521);
            this.splitter1.TabIndex = 3;
            this.splitter1.TabStop = false;
            //
            // mtlbMain
            //
            this.mtlbMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mtlbMain.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
                                                                                        this.mtlbButtonOpen,
                                                                                        this.mtlbButtonSave,
                                                                                        this.mtlbButtonLinkRaw,
                                                                                        this.mtlnButtonBack,
                                                                                        this.mtlnButtonForward,
                                                                                        this.mtlnButtonOverlay,
                                                                                        this.mtlnButtonLinkZoom,
                                                                                        this.mtlnButtonSelectBackground});
            this.mtlbMain.ButtonSize = new System.Drawing.Size(24, 24);
            this.mtlbMain.DropDownArrows = true;
            this.mtlbMain.ImageList = this.mimageListIcons;
            this.mtlbMain.Location = new System.Drawing.Point(0, 0);
            this.mtlbMain.Name = "mtlbMain";
            this.mtlbMain.ShowToolTips = true;
            this.mtlbMain.Size = new System.Drawing.Size(1128, 31);
            this.mtlbMain.TabIndex = 5;
            this.mtlbMain.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.mtlbMain_ButtonClick);
            //
            // mtlbButtonOpen
            //
            this.mtlbButtonOpen.ImageIndex = 1;
            //
            // mtlbButtonSave
            //
            this.mtlbButtonSave.ImageIndex = 2;
            //
            // mtlbButtonLinkRaw
            //
            this.mtlbButtonLinkRaw.ImageIndex = 4;
            //
            // mtlnButtonBack
            //
            this.mtlnButtonBack.ImageIndex = 5;
            //
            // mtlnButtonForward
            //
            this.mtlnButtonForward.ImageIndex = 6;
            //
            // mtlnButtonOverlay
            //
            this.mtlnButtonOverlay.ImageIndex = 7;
            //
            // mtlnButtonLinkZoom
            //
            this.mtlnButtonLinkZoom.ImageIndex = 9;
            this.mtlnButtonLinkZoom.Text = "Linked Zoom";
            this.mtlnButtonLinkZoom.ToolTipText = "Linked Zoom";
            //
            // mimageListIcons
            //
            this.mimageListIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
            this.mimageListIcons.ImageSize = new System.Drawing.Size(16, 16);
            this.mimageListIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("mimageListIcons.ImageStream")));
            this.mimageListIcons.TransparentColor = System.Drawing.Color.Transparent;
            //
            // mtlnButtonSelectBackground
            //
            this.mtlnButtonSelectBackground.ImageIndex = 10;
            //
            // frm2DPeakProcessing
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(1128, 862);
            this.Controls.Add(this.mpanelCenter);
            this.Controls.Add(this.mtlbMain);
            this.Controls.Add(this.msplitterBottom);
            this.Controls.Add(this.mexpandPanelBottom);
            this.Name = "frm2DPeakProcessing";
            this.Text = "frm2DPeakProcessing";
            ((System.ComponentModel.ISupportInitialize)(this.mexpandPanelBottom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ctlChartTopPane)).EndInit();
            this.mpanelCenter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mctl_rawdata)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion

        private void mtlbMain_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
        {
            try
            {
                if (e.Button == mtlnButtonOverlay)
                {
                    var focus_form = new frmFocusParameters();
                    focus_form.FocusMZ = mctl_rawdata.FocusMZ;
                    focus_form.FocusScan = mctl_rawdata.FocusScan;
                    focus_form.FocusMZTolerance = mflt_sic_mz_tolerance;

                    if (focus_form.ShowDialog(this) == DialogResult.OK)
                    {
                        if (focus_form.FocusMZ != mctl_rawdata.FocusMZ || focus_form.FocusScan != mctl_rawdata.FocusScan
                            || focus_form.FocusMZTolerance != mflt_sic_mz_tolerance)
                        {
                            mctl_rawdata.FocusMZ = focus_form.FocusMZ;
                            mctl_rawdata.FocusScan = focus_form.FocusScan;
                            mflt_sic_mz_tolerance = focus_form.FocusMZTolerance;
                            // now its been set. Calculate the new selected ion chromatogram and set that to the
                            SetDataPlots();
                        }
                    }
                }
                else if (e.Button == mtlbButtonLinkRaw)
                {
                    LinkRawFile();
                }
                else if (e.Button == mtlnButtonLinkZoom)
                {
                    mtlnButtonLinkZoom.Pushed = !mtlnButtonLinkZoom.Pushed;
                }
                else if (e.Button == mtlnButtonSelectBackground)
                {
                    var frmThreshold = new frmFloatDialog();
                    frmThreshold.Prompt = "Please enter background intensity";
                    frmThreshold.EditingValue = mfltMinIntensity;
                    if (frmThreshold.ShowDialog() == DialogResult.OK)
                    {
                        mfltMinIntensity = frmThreshold.EditingValue;
                        SetData(mobj_results);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// Function sets the data for the selected ion chromatograms, and if linked also sets the raw data
        /// in the spectrum controls.
        /// </summary>
        private void SetDataPlots()
        {
            try
            {
                if (mctl_rawdata.FocusMZ == 0 || mobj_results == null || marr_peaks == null)
                    return;

                // now get the selected ion chromatogram.
                var currentViewPort = mctl_rawdata.ViewPort;

                if (currentViewPort.Bottom == currentViewPort.Top || currentViewPort.Left == currentViewPort.Right)
                    return;

                int min_scan = 0, max_scan = 0;

                min_scan = (int)currentViewPort.Top;
                max_scan = (int)currentViewPort.Bottom;

                if (marr_sic_intensities == null)
                    marr_sic_intensities = new float[1];

                mobj_results.GetSIC(min_scan, max_scan, mctl_rawdata.FocusMZ, mflt_sic_mz_tolerance, out marr_sic_intensities);
                marr_sic_scans = new float[marr_sic_intensities.Length];
                for (var scan_num = min_scan; scan_num <= max_scan; scan_num++)
                {
                    marr_sic_scans[scan_num - min_scan] = (float)scan_num;
                }

                // now for the peaks in the spectrum.
                mobj_results.GetScanPeaks(mctl_rawdata.FocusScan, out marr_spectrum_peak_mzs, out marr_spectrum_peak_intensities);

                SetPlotStructures();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }
        private void DefaultZoomHandler_SingleClickNoZoomPerformed(object sender, MouseEventArgs e)
        {
            float mz_focus = 0;
            var scan_focus = 0;
            // mz value will be drawn along the x axis.
            mz_focus = mctl_rawdata.GetChartX(mctl_rawdata.GetChartAreaX(e.X));
            scan_focus = (int)mctl_rawdata.GetChartY(mctl_rawdata.GetChartAreaY(e.Y));

            mctl_rawdata.FocusMZ = mz_focus;
            mctl_rawdata.FocusScan = scan_focus;
            //
            var mz_range = mctl_rawdata.ViewPort.Width;
            float ctl_range = mctl_rawdata.ClientRectangle.Width;
            if (mz_range < 10)
                mflt_sic_mz_tolerance = mz_range / ctl_range * mint_num_pixel_screen_tolerance;
            else
                mflt_sic_mz_tolerance = 0.1F;
            // now its been set. Calculate the new selected ion chromatogram and set that to the
            SetDataPlots();
        }

        private void SavePeaks(string fileName)
        {
        }

        private void LinkRawFile()
        {
            var selectRawFile = new frmSelectRaw();
            if (mobj_results.FileType != DeconToolsV2.Readers.FileType.UNDEFINED)
            {
                selectRawFile.FileType = mobj_results.FileType;
                selectRawFile.FileName = mobj_results.FileName;
            }
            selectRawFile.Text = "Please Select a Raw File";
            if (selectRawFile.ShowDialog() == DialogResult.Cancel)
                return;
            mobj_raw_data = new DeconToolsV2.Readers.clsRawData(selectRawFile.FileName, selectRawFile.FileType);
            mctl_spectrum.RawData = mobj_raw_data;
        }

        private void mctl_spectrum_mevntScanChanged(object sender, int new_scan)
        {
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
                mctl_spectrum.HornTransformParameters = value;
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
            }
        }

        private void mctl_rawdata_ViewPortChanged(PNNL.Controls.ctlChartBase chart, PNNL.Controls.ViewPortChangedEventArgs args)
        {
            if (mtlnButtonLinkZoom.Pushed)
            {
                mctl_spectrum.SetMZRange(args.ViewPort.Left, args.ViewPort.Right);
                ctlChartTopPane.ViewPort = new RectangleF(0, args.ViewPort.Y, 1, args.ViewPort.Height);
                ctlChartTopPane.AutoViewPortX();
            }
        }
    }
}
