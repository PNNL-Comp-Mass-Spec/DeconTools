// Written by Navdeep Jaitly and Anoop Mayampurath for the Department of Energy (PNNL, Richland, WA)
// Copyright 2006, Battelle Memorial Institute
// E-mail: navdeep.jaitly@pnl.gov
// Website: https://omics.pnl.gov/software or https://panomics.pnnl.gov
// -------------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using DeconTools.Backend.Utilities;
using PNNL.Controls;

namespace Decon2LS
{
    /// <summary>
    /// Summary description for ctlMassSpectrum.
    /// </summary>
    public class ctlMassSpectrum : System.Windows.Forms.UserControl
    {
        public delegate void ScanChangedEventHandler(object sender, int new_scan);
        private PNNL.Controls.MS.ctlSpectrum mctl_spectra;
        private System.Windows.Forms.Panel mpanelSpectrum;

        private DeconToolsV2.HornTransform.clsHornTransform mobjTransform;
        private DeconToolsV2.HornTransform.clsHornTransformParameters mobjTransformParameters;
        private DeconToolsV2.Peaks.clsPeakProcessorParameters mobjPeakParameters;
        private DeconToolsV2.Readers.clsRawDataPreprocessOptions mobjFTICRPreProcessOptions;
        private DeconToolsV2.HornTransform.clsAveragine mobjAveragine;
        private DeconToolsV2.DTAGeneration.clsDTAGenerationParameters mobjDTAGenerationParameters;

        private Thread mthrd_decon;

#pragma warning disable 618
        private DeconToolsV2.clsMercuryIsotopeDistribution mMercuryIsotopeDistribution;
        private DeconToolsV2.clsIsotopeFit mobjIsotopeFit;
#pragma warning restore 618

        private double mdbl_current_background_intensity = 0;
        private PNNL.Controls.clsSeries mobj_spectrum_series;
        private PNNL.Controls.clsSeries mobj_spectrum_peak_series;
        private PNNL.Controls.clsPlotParams mobj_spectrum_plt_params;
        private PNNL.Controls.clsPlotParams mobj_spectrum_peak_plt_params;

        private readonly Hashtable mhash_series = new Hashtable();


        private readonly System.Windows.Forms.ContextMenu mcontextMenu_spectrum = new ContextMenu();
        readonly MenuItem menuItem_mass_transform = new MenuItem("&Mass Transform");
        readonly MenuItem menuItem_find_peaks = new MenuItem("&Find Peaks");
        readonly MenuItem menuItem_sg_smooth = new MenuItem("&Apply Savitzky Golay Smoothing");
        readonly MenuItem menuItem_zero_fill_discontinuous = new MenuItem("&Zero Fill Discontinuous Areas");
        readonly MenuItem menuItem_apodize = new MenuItem("&Apodize");
        readonly MenuItem menuItem_zero_fill_time_domain = new MenuItem("&Zero Fill Time Domain Signal");
        readonly MenuItem menuItem_time_domain = new MenuItem("&Convert To Time Domain");
        readonly MenuItem menuItem_export = new MenuItem("&Export");
        readonly MenuItem menuItem_export_spectrum_to_clipboard = new MenuItem("&Copy Data to Clipboard");
        readonly MenuItem menuItem_export_spectrum_to_clipboard_nonzero = new MenuItem("&Copy Nonzero Data to Clipboard");
        readonly MenuItem menuItem_export_spectrum = new MenuItem("&Export Data to file");
        readonly MenuItem menuItem_export_spectrum_nonzero = new MenuItem("&Export Nonzero Data to file");
        readonly MenuItem menuItemSavePeaks = new MenuItem("&Save Peaks");
        readonly MenuItem menuItemCopyPeaks = new MenuItem("&Copy Peaks to Clipboard");
        readonly MenuItem menuItemSaveTransform = new MenuItem("&Save Transform Results");
        readonly MenuItem menuItemCopyTransform = new MenuItem("&Copy Transform Results to Clipboard");

        readonly MenuItem menuItemShowHypertransformSpectrum = new MenuItem("&Show Hypertransform Spectrum");

        private bool mbln_processing = false;
        private DeconToolsV2.HornTransform.clsHornTransformResults[] marr_transformResults;

#pragma warning disable 618
        private DeconToolsV2.Peaks.clsPeakProcessor mobj_PeakProcessor;
        private EventHandler mStatusUpdateDelegate = null;
        private DeconToolsV2.Readers.clsRawData mobjRawData = null;
#pragma warning restore 618

        private readonly int mint_num_zero_fill_discontinuous = 3;
        public event ScanChangedEventHandler mevntScanChanged;

        private DeconToolsV2.Peaks.clsPeak[] mobj_peaks;
        private bool mblnDisplayTimeDomain = false;
        private float[] marr_current_time_domain_values = null;
        private float[] marr_current_mzs = null;
        private float[] marr_current_intensities = null;


        private int mint_spectrum_num = 0;
        private System.Windows.Forms.Timer mStatusTimer;
        private string mstrDescription;

        private clsMediator mMediator;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox mScanTextBox;
        private System.Windows.Forms.TabPage mtabPage_mass_transform;
        private System.Windows.Forms.ListView mlistview_transform;
        private System.Windows.Forms.TabPage mtabPage_peaks;
        private System.Windows.Forms.ListView mlistView_peaks;
        private System.Windows.Forms.ColumnHeader mcolheader_index;
        private System.Windows.Forms.ColumnHeader mcolheader_mass;
        private System.Windows.Forms.ColumnHeader mcolheader_abundance;
        private System.Windows.Forms.ColumnHeader mcolheader_mz;
        private System.Windows.Forms.ColumnHeader mcolheader_charge;
        private System.Windows.Forms.ColumnHeader mcolheader_fit;
        private System.Windows.Forms.ColumnHeader mcolheader_mostmw;
        private System.Windows.Forms.ColumnHeader mcolHeaderIndex;
        private System.Windows.Forms.ColumnHeader mcolHeaderMZ;
        private System.Windows.Forms.ColumnHeader mcolHeaderIntensity;
        private System.Windows.Forms.ColumnHeader mcolHeaderFWHM;
        private System.Windows.Forms.ColumnHeader mcolHeaderSN;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Button btnFindPeaks;
        private System.Windows.Forms.Button btnTransformMS;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnSmooth;
        private System.Windows.Forms.Button btnCopytoClipboard;
        private System.Windows.Forms.Button btnDisplayFit;
        private System.Windows.Forms.TextBox txtLeftFitStringencyFactor;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox txtRightFitStringencyFactor;
        private System.ComponentModel.IContainer components;

        public ctlMassSpectrum()
        {
            try
            {
                // This call is required by the Windows.Forms Form Designer.
                InitializeComponent();

                // TODO: Add any initialization after the InitializeComponent call
                SetupContextMenus();
                Init();
                this.mlistView_peaks.ColumnClick += new ColumnClickEventHandler(mlistView_peaks_ColumnClick);
                this.mlistview_transform.ColumnClick += new ColumnClickEventHandler(mlistView_peaks_ColumnClick);
                this.mScanTextBox.KeyDown += new KeyEventHandler(mScanTextBox_KeyDown);
                this.mlistview_transform.ItemActivate += new EventHandler(mlistview_transform_ItemActivate);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }

        }

        public clsMediator Mediator
        {
            get
            {
                return mMediator;
            }
            set
            {
                mMediator = value;
            }
        }

        public int CurrentSpectrumNum
        {
            get
            {
                return mint_spectrum_num;
            }
        }
        private void Init()
        {
            var shape = new PNNL.Controls.DiamondShape(3, false);
            mobj_spectrum_plt_params = new PNNL.Controls.clsPlotParams(shape, Color.Black, false, true, true);

            this.mobj_spectrum_peak_plt_params = new PNNL.Controls.clsPlotParams(new PNNL.Controls.BubbleShape(2, false),
                Color.Red, true, false);

            this.mStatusTimer = new System.Windows.Forms.Timer();

            mobjTransform = new DeconToolsV2.HornTransform.clsHornTransform();
            mobjTransformParameters = new DeconToolsV2.HornTransform.clsHornTransformParameters();
            mobjPeakParameters = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            mobjFTICRPreProcessOptions = new DeconToolsV2.Readers.clsRawDataPreprocessOptions();
            mobj_PeakProcessor = new DeconToolsV2.Peaks.clsPeakProcessor();
            mMercuryIsotopeDistribution = new DeconToolsV2.clsMercuryIsotopeDistribution();
            mobjDTAGenerationParameters = new DeconToolsV2.DTAGeneration.clsDTAGenerationParameters();
            mobjIsotopeFit = new DeconToolsV2.clsIsotopeFit();
            mobjAveragine = new DeconToolsV2.HornTransform.clsAveragine();


        }

        private void SetupContextMenus()
        {
            try
            {
                // Set up Context Menu on Peaks List

                menuItemSavePeaks.Click += new EventHandler(menuItemPeaksSave_Click);
                menuItemCopyPeaks.Click += new EventHandler(menuItemPeaksCopy_Click);

                var contextMenuPeaksList = new ContextMenu();
                contextMenuPeaksList.MenuItems.Add(menuItemSavePeaks);
                contextMenuPeaksList.MenuItems.Add(menuItemCopyPeaks);
                contextMenuPeaksList.MenuItems.Add(menuItemShowHypertransformSpectrum);
                mlistView_peaks.ContextMenu = contextMenuPeaksList;

                // Set up Context Menu on Transform Results list.
                var contextMenuTransformList = new ContextMenu();
                menuItemSaveTransform.Click += new EventHandler(menuItemTransformSave_Click);
                menuItemCopyTransform.Click += new EventHandler(menuItemTransformCopy_Click);
                menuItemShowHypertransformSpectrum.Click += new EventHandler(menuItemShowHypertransformSpectrum_Click);

                contextMenuTransformList.MenuItems.Add(menuItemShowHypertransformSpectrum);
                contextMenuTransformList.MenuItems.Add(new MenuItem("-"));
                contextMenuTransformList.MenuItems.Add(menuItemSaveTransform);
                contextMenuTransformList.MenuItems.Add(menuItemCopyTransform);
                mlistview_transform.ContextMenu = contextMenuTransformList;

                // Context Menu on spectrum
                mcontextMenu_spectrum.MenuItems.Add(menuItem_sg_smooth);
                mcontextMenu_spectrum.MenuItems.Add(menuItem_zero_fill_discontinuous);

                mcontextMenu_spectrum.MenuItems.Add(menuItem_find_peaks);
                mcontextMenu_spectrum.MenuItems.Add(menuItem_mass_transform);
                mcontextMenu_spectrum.MenuItems.Add(new MenuItem("-"));

                mcontextMenu_spectrum.MenuItems.Add(menuItem_time_domain);
                mcontextMenu_spectrum.MenuItems.Add(menuItem_zero_fill_time_domain);
                mcontextMenu_spectrum.MenuItems.Add(menuItem_apodize);
                mcontextMenu_spectrum.MenuItems.Add(new MenuItem("-"));

                menuItem_export.MenuItems.Add(menuItem_export_spectrum_to_clipboard);
                menuItem_export.MenuItems.Add(menuItem_export_spectrum_to_clipboard_nonzero);
                menuItem_export.MenuItems.Add(menuItem_export_spectrum);
                menuItem_export.MenuItems.Add(menuItem_export_spectrum_nonzero);
                mcontextMenu_spectrum.MenuItems.Add(menuItem_export);

                this.mctl_spectra.ContextMenu = this.mcontextMenu_spectrum;

                menuItem_find_peaks.Click += new EventHandler(menuItem_find_peaks_Click);
                menuItem_mass_transform.Click += new EventHandler(menuItem_mass_transform_Click);
                menuItem_sg_smooth.Click += new EventHandler(menuItem_sg_smooth_Click);

                menuItem_zero_fill_discontinuous.Click += new EventHandler(menuItem_zero_fill_discontinuous_Click);

                menuItem_time_domain.Click += new EventHandler(menuItem_time_domain_Click);
                menuItem_apodize.Click += new EventHandler(menuItem_apodize_Click);
                menuItem_zero_fill_time_domain.Click += new EventHandler(menuItem_zero_fill_time_domain_Click);

                menuItem_export_spectrum_to_clipboard.Click += new EventHandler(menuItem_export_spectrum_to_clipboard_Click);
                menuItem_export_spectrum_to_clipboard_nonzero.Click += new EventHandler(menuItem_export_spectrum_to_clipboard_nonzero_Click);
                menuItem_export_spectrum.Click += new EventHandler(menuItem_export_spectrum_Click);
                menuItem_export_spectrum_nonzero.Click += new EventHandler(menuItem_export_spectrum_nonzero_Click);

            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }


        #region "Properties"
        public DeconToolsV2.Peaks.clsPeakProcessor PeakProcessor
        {
            get
            {
                return mobj_PeakProcessor;
            }
        }
        public DeconToolsV2.Peaks.clsPeakProcessorParameters PeakProcessorParameters
        {
            get
            {
                return mobjPeakParameters;
            }
            set
            {
                mobjPeakParameters = value;

                //				mobjPeakParameters = (DeconToolsV2.Peaks.clsPeakProcessorParameters) value.Clone() ;
            }
        }

        public DeconToolsV2.HornTransform.clsHornTransformParameters HornTransformParameters
        {
            get
            {
                return mobjTransformParameters;
            }
            set
            {
                //				mobjTransformParameters =
                //					(DeconToolsV2.HornTransform.clsHornTransformParameters) value.Clone() ;
                mobjTransformParameters = value;
                mMercuryIsotopeDistribution.ElementIsotopes = mobjTransformParameters.ElementIsotopeComposition;
                mobjAveragine.SetElementalIsotopeComposition(mobjTransformParameters.ElementIsotopeComposition);
                updateFormTransformParameters();
            }
        }

        public DeconToolsV2.Readers.clsRawDataPreprocessOptions FTICRPreProcessOptions
        {
            get
            {
                return mobjFTICRPreProcessOptions;
            }
            set
            {
                mobjFTICRPreProcessOptions = value;
                if (mobjRawData != null)
                    mobjRawData.FTICRRawPreprocessOptions = mobjFTICRPreProcessOptions;
            }
        }

        public DeconToolsV2.DTAGeneration.clsDTAGenerationParameters DTAGenerationParameters
        {
            get
            {
                return mobjDTAGenerationParameters;
            }
            set
            {
                mobjDTAGenerationParameters = value;
                //				mobjDTAGenerationParameters = (DeconToolsV2.DTAGeneration.clsDTAGenerationParameters) value.Clone()  ;
            }
        }

        public DeconToolsV2.Readers.clsRawData RawData
        {
            get
            {
                return mobjRawData;
            }
            set
            {
                mobjRawData = value;
                var fileType = mobjRawData.FileType;
                if (fileType == DeconToolsV2.Readers.FileType.MICROMASSRAWDATA
                    || fileType == DeconToolsV2.Readers.FileType.PNNL_IMS
                    || fileType == DeconToolsV2.Readers.FileType.PNNL_UIMF
                    || fileType == DeconToolsV2.Readers.FileType.MZXMLRAWDATA
                    || fileType == DeconToolsV2.Readers.FileType.ASCII
                    || fileType == DeconToolsV2.Readers.FileType.FINNIGAN)
                {
                    menuItem_zero_fill_discontinuous.Enabled = true;
                }
                else
                {
                    menuItem_zero_fill_discontinuous.Enabled = false;
                }

                if (fileType == DeconToolsV2.Readers.FileType.ICR2LSRAWDATA)
                {
                    menuItem_time_domain.Enabled = true;
                    SetTimeDomainMenuItemsEnabled(mblnDisplayTimeDomain);
                }
                else
                {
                    SetTimeDomainMenuItemsEnabled(false);
                    menuItem_time_domain.Enabled = false;
                }
            }
        }
        public void SetViewProperties(DeconToolsV2.Readers.FileType fileType)
        {
            switch (fileType)
            {
                case DeconToolsV2.Readers.FileType.PNNL_IMS:
                    mctl_spectra.UseAutoViewPortYBase = true;
                    mctl_spectra.AutoViewPortYAxis = true;
                    mctl_spectra.AutoViewPortYBase = 0;
                    break;
                case DeconToolsV2.Readers.FileType.PNNL_UIMF:
                    mctl_spectra.UseAutoViewPortYBase = true;
                    mctl_spectra.AutoViewPortYAxis = true;
                    mctl_spectra.AutoViewPortYBase = 0;
                    break;
                default:
                    break;
            }
        }
        #endregion

        public void Clear()
        {
            mctl_spectra.Clear();
            mlistView_peaks.Items.Clear();
            mlistview_transform.Items.Clear();
            ClearViewPort();
        }

        public void ClearViewPort()
        {
            this.mctl_spectra.ViewPortHistory.Clear();
        }

        public void SetMZRange(float left, float right)
        {
            mctl_spectra.ViewPort = new RectangleF(left, 0, right - left, 1);
            mctl_spectra.AutoViewPortY();
        }

        public void GetSummedSpectra(int scan_num, int scan_range, ref float[] mz_values, ref float[] intensity_values)
        {
            if (mobjRawData == null)
                return;
            var xvals = XYValueConverter.ConvertFloatsToDoubles(mz_values);
            var yvals = XYValueConverter.ConvertFloatsToDoubles(intensity_values);
            mobjRawData.GetSummedSpectra(scan_num, scan_range, ref xvals, ref yvals);


            XYValueConverter.ConvertDoublesToFloats(xvals, ref mz_values);
            XYValueConverter.ConvertDoublesToFloats(yvals, ref intensity_values);



            mstrDescription = "Summed Spectra Across Range";
        }

        public void GetSummedSpectra(int start_scan, int stop_scan, double min_mz, double max_mz, ref float[] mz_values, ref float[] intensity_values)
        {
            if (mobjRawData == null)
                return;
            var xvals = XYValueConverter.ConvertFloatsToDoubles(mz_values);
            var yvals = XYValueConverter.ConvertFloatsToDoubles(intensity_values);
            mobjRawData.GetSummedSpectra(start_scan, stop_scan, min_mz, max_mz, ref xvals, ref yvals);

            XYValueConverter.ConvertDoublesToFloats(xvals, ref mz_values);
            XYValueConverter.ConvertDoublesToFloats(yvals, ref intensity_values);


            mstrDescription = "Summed Spectra Across All Scans";
        }

        public void GetSpectrum(int scan_num, ref float[] mz_values, ref float[] intensity_values)
        {
            if (mobjRawData == null)
                return;

            var xvals = XYValueConverter.ConvertFloatsToDoubles(mz_values);
            var yvals = XYValueConverter.ConvertFloatsToDoubles(intensity_values);

            mobjRawData.GetSpectrum(scan_num, ref xvals, ref yvals, false);

            XYValueConverter.ConvertDoublesToFloats(xvals, ref mz_values);
            XYValueConverter.ConvertDoublesToFloats(yvals, ref intensity_values);
            mstrDescription = mobjRawData.GetScanDescription(scan_num);
        }
        private void AddDataToSpectralChart()
        {
            try
            {
                if (mhash_series.ContainsKey(mint_spectrum_num))
                    return;

                var plotParams = (clsPlotParams)mobj_spectrum_plt_params.Clone();
                var isFTScan = mobjRawData.IsFTScan(mint_spectrum_num);
                var msLevel = mobjRawData.GetMSLevel(mint_spectrum_num);

                if (msLevel > 1 && !isFTScan)
                    plotParams.DrawSticks = true;
                if (mobjRawData.FileType == DeconToolsV2.Readers.FileType.PNNL_IMS)
                    plotParams.DrawSticks = false;
                if (mobjRawData.FileType == DeconToolsV2.Readers.FileType.PNNL_UIMF)
                    plotParams.DrawSticks = false;
                plotParams.Name = "Scan # " + Convert.ToString(mint_spectrum_num);

                if (!mblnDisplayTimeDomain)
                {
                    mobj_spectrum_series = new clsSpectraSeries(
                        new PNNL.Controls.ArrayChartDataProvider(marr_current_mzs, marr_current_intensities),
                        plotParams, mobjRawData.FileName);
                }
                else
                {
                    mobj_spectrum_series = new clsSpectraSeries(
                        new PNNL.Controls.ArrayChartDataProvider(marr_current_time_domain_values, marr_current_intensities),
                        plotParams, mobjRawData.FileName);
                }
                mhash_series[mint_spectrum_num] = mobj_spectrum_series;
                mctl_spectra.SeriesCollection.Add(mobj_spectrum_series);
                mctl_spectra.Title = mstrDescription;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }

        }

        public void SetPeakSeriesData(int scan, float[] mzs, float[] intensities, float minMZ, float maxMZ)
        {
            mctl_spectra.ViewPortHistory.Clear();
            mctl_spectra.SeriesCollection.Clear();

            if (mobjRawData != null)
            {
                ShowSpectrumInSpectralChart(scan, true, false);
            }


            var max_intensity = float.MinValue;
            if (mzs != null)
            {
                mobj_spectrum_peak_series = new clsSeries(ref mzs, ref intensities, mobj_spectrum_peak_plt_params);
                mctl_spectra.SeriesCollection.Add(mobj_spectrum_peak_series);

                var num_peaks = mzs.Length;
                for (var pk_num = 0; pk_num < num_peaks; pk_num++)
                {
                    if (intensities[pk_num] > max_intensity && mzs[pk_num] >= minMZ
                        && mzs[pk_num] <= maxMZ)
                    {
                        max_intensity = intensities[pk_num];
                    }
                }
            }
            if (max_intensity == float.MinValue)
                max_intensity = 1;

            mctl_spectra.ViewPortHistory.Clear();
            mctl_spectra.AutoViewPort();

            this.mctl_spectra.ViewPort = new RectangleF(minMZ, 0, maxMZ - minMZ, max_intensity);
        }

        private void ClearSpectralChart()
        {
            try
            {
                mctl_spectra.Clear();
                mhash_series.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }

        public void ShowSpectrumInSpectralChart(int spectrum_num, bool get_data, bool reset_viewport)
        {
            mobj_peaks = null;
            marr_transformResults = null;
            mlistView_peaks.Items.Clear();
            mlistview_transform.Items.Clear();

            if (spectrum_num < 1)
            {
                spectrum_num = 1;
            }
            if (spectrum_num > mobjRawData.GetNumScans())
            {
                spectrum_num = mobjRawData.GetNumScans();
            }
            this.mint_spectrum_num = spectrum_num;
            var ms_level = mobjRawData.GetMSLevel(mint_spectrum_num);

            if (get_data)
            {
                if (mobjTransformParameters.SumSpectraAcrossScanRange && ms_level == 1)
                {
                    var scan_range = mobjTransformParameters.NumScansToSumOver;
                    marr_current_mzs = null;
                    marr_current_intensities = null;
                    GetSummedSpectra(mint_spectrum_num, scan_range, ref marr_current_mzs, ref marr_current_intensities);
                }
                else if (mobjTransformParameters.SumSpectra)
                {
                    var min_mz = mobjTransformParameters.MinMZ;
                    var max_mz = mobjTransformParameters.MaxMZ;

                    var start_scan = mobjRawData.GetFirstScanNum();
                    var stop_scan = mobjRawData.GetNumScans();
                    if (mobjTransformParameters.UseScanRange)
                    {
                        start_scan = mobjTransformParameters.MinScan;
                        stop_scan = mobjTransformParameters.MaxScan;
                    }
                    GetSummedSpectra(start_scan, stop_scan, min_mz, max_mz, ref marr_current_mzs, ref marr_current_intensities);
                }
                else
                    GetSpectrum(spectrum_num, ref marr_current_mzs, ref marr_current_intensities);

            }
            var currentVP = this.mctl_spectra.ViewPort;
            ClearSpectralChart();
            AddDataToSpectralChart();
            mctl_spectra.ViewPortHistory.Clear();
            mctl_spectra.AutoViewPort();
            if (mobjRawData.FileType == DeconToolsV2.Readers.FileType.BRUKER || mobjRawData.FileType == DeconToolsV2.Readers.FileType.ICR2LSRAWDATA)
            {
                // set the mz range.
                mctl_spectra.ViewPort = new RectangleF(400, mctl_spectra.ViewPort.Y, 1600, mctl_spectra.ViewPort.Height);
            }
            if (!reset_viewport)
            {
                mctl_spectra.ViewPort = currentVP;
            }

            // Update scan text box
            this.mScanTextBox.Text = spectrum_num.ToString();


        }
        public void DisplayFitResultForCurrentScan(string formula, short charge)
        {
            try
            {
                if (mobj_peaks == null || mobj_peaks.Length == 0)
                {
                    MessageBox.Show(this, "Please find peaks first");
                    return;
                }
                if (charge < 1)
                {
                    MessageBox.Show(this, "Please enter a positive charge.");
                    return;
                }

                this.mMercuryIsotopeDistribution.ChargeState = charge;
                var molecularFormula = MolecularFormula.Parse(formula);
                this.mMercuryIsotopeDistribution.ElementIsotopes = mobjTransformParameters.ElementIsotopeComposition;

                var points =
                    this.mMercuryIsotopeDistribution.CalculateDistribution(
                    molecularFormula.ToElementTable());


                // go through the list of peaks for the possible peaks.
                var mostAbundantMZ = mMercuryIsotopeDistribution.MostAbundantMZ;
                var minDistance = double.MaxValue;
                var minDistancePeakIndex = -1;
                for (var pkNum = 0; pkNum < mobj_peaks.Length; pkNum++)
                {
                    var diff = mobj_peaks[pkNum].mdbl_mz - mostAbundantMZ;
                    if (Math.Abs(diff) < Math.Abs(minDistance))
                    {
                        minDistance = diff;
                        minDistancePeakIndex = pkNum;
                    }
                }

                mMercuryIsotopeDistribution.Resolution = mobj_peaks[minDistancePeakIndex].mdbl_mz / mobj_peaks[minDistancePeakIndex].mdbl_FWHM;
                points = this.mMercuryIsotopeDistribution.CalculateDistribution(molecularFormula.ToElementTable());

                // go through the list of peaks for the possible peaks.
                mostAbundantMZ = mMercuryIsotopeDistribution.MostAbundantMZ;
                minDistance = double.MaxValue;
                minDistancePeakIndex = -1;
                for (var pkNum = 0; pkNum < mobj_peaks.Length; pkNum++)
                {
                    var diff = mobj_peaks[pkNum].mdbl_mz - mostAbundantMZ;
                    if (Math.Abs(diff) < Math.Abs(minDistance))
                    {
                        minDistance = diff;
                        minDistancePeakIndex = pkNum;
                    }
                }

                if (Math.Abs(minDistance) > 1)
                {
                    MessageBox.Show(this, "There is no peak within 1 Da of supplied formula.");
                    return;
                }

                var peakHeight = mobj_peaks[minDistancePeakIndex].mdbl_intensity;
                // now we have found the peak we want to scale relative to.
                var theoreticalMZs = new float[points.Length];
                var theoreticalIntensities = new float[points.Length];

                for (var pointNum = 0; pointNum < points.Length; pointNum++)
                {
                    theoreticalMZs[pointNum] = points[pointNum].X + (float)minDistance;
                    theoreticalIntensities[pointNum] = points[pointNum].Y * (float)peakHeight / 100;
                }

                var minMZ = theoreticalMZs[0];
                var maxMZ = theoreticalMZs[points.Length - 1];


                var formulaParams = (clsPlotParams)this.mobj_spectrum_plt_params.Clone();
                formulaParams.LinePen.Color = Color.Black;
                formulaParams.LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                var fitScore = mobjIsotopeFit.GetFitScore(ref marr_current_mzs, ref marr_current_intensities, ref mobj_peaks, charge,
                    minDistancePeakIndex, mobjTransformParameters.DeleteIntensityThreshold, mobjTransformParameters.MinIntensityForScore, molecularFormula.ToElementTable());

                formulaParams.Name = molecularFormula.ToSimpleOrganicElementalString() + " - " + fitScore.ToString("f3");


                var formulaSeries = new clsSeries(ref theoreticalMZs, ref theoreticalIntensities, formulaParams);
                mctl_spectra.SeriesCollection.Add(formulaSeries);


                mctl_spectra.ViewPort = new RectangleF(minMZ, 0, maxMZ - minMZ, (float)peakHeight);
                mctl_spectra.HasLegend = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }

        }

        private void DisplayFitResultForCurrentScan(string formula, short charge, double mzForResolution,
            double mostAbundantMZ)
        {
            try
            {
                if (mobj_peaks == null || mobj_peaks.Length == 0)
                {
                    MessageBox.Show(this, "Please find peaks first");
                    return;
                }
                if (charge < 1)
                {
                    MessageBox.Show(this, "Please enter a positive charge.");
                    return;
                }

                this.mMercuryIsotopeDistribution.ChargeState = charge;
                var molecularFormula = MolecularFormula.Parse(formula);
                this.mMercuryIsotopeDistribution.ElementIsotopes = mobjTransformParameters.ElementIsotopeComposition;


                var minDistance = double.MaxValue;
                var minDistancePeakIndex = -1;
                for (var pkNum = 0; pkNum < mobj_peaks.Length; pkNum++)
                {
                    var diff = mobj_peaks[pkNum].mdbl_mz - mzForResolution;
                    if (Math.Abs(diff) < Math.Abs(minDistance))
                    {
                        minDistance = diff;
                        minDistancePeakIndex = pkNum;
                    }
                }
                if (Math.Abs(minDistance) > 1)
                {
                    MessageBox.Show(this, "There is no peak within 1 Da of supplied formula.");
                    return;
                }

                mMercuryIsotopeDistribution.Resolution = mzForResolution / mobj_peaks[minDistancePeakIndex].mdbl_FWHM;
                var points =
                    this.mMercuryIsotopeDistribution.CalculateDistribution(
                    molecularFormula.ToElementTable());

                var diffMZ = mostAbundantMZ - mMercuryIsotopeDistribution.MostAbundantMZ;

                var peakHeight = mobj_peaks[minDistancePeakIndex].mdbl_intensity;
                points = this.mMercuryIsotopeDistribution.CalculateDistribution(molecularFormula.ToElementTable());


                // now we have found the peak we want to scale relative to.
                var theoreticalMZs = new float[points.Length];
                var theoreticalIntensities = new float[points.Length];

                for (var pointNum = 0; pointNum < points.Length; pointNum++)
                {
                    theoreticalMZs[pointNum] = points[pointNum].X + (float)diffMZ;
                    theoreticalIntensities[pointNum] = points[pointNum].Y * (float)peakHeight / 100;
                }

                var minMZ = theoreticalMZs[0];
                var maxMZ = theoreticalMZs[points.Length - 1];


                var formulaParams = (clsPlotParams)this.mobj_spectrum_plt_params.Clone();
                formulaParams.LinePen.Color = Color.Black;
                formulaParams.LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                var fitScore = mobjIsotopeFit.GetFitScore(ref marr_current_mzs, ref marr_current_intensities, ref mobj_peaks, charge,
                    minDistancePeakIndex, mobjTransformParameters.DeleteIntensityThreshold, mobjTransformParameters.MinIntensityForScore, molecularFormula.ToElementTable());

                formulaParams.Name = molecularFormula.ToSimpleOrganicElementalString() + " - " + fitScore.ToString("f3");


                var formulaSeries = new clsSeries(ref theoreticalMZs, ref theoreticalIntensities, formulaParams);
                mctl_spectra.SeriesCollection.Add(formulaSeries);


                mctl_spectra.ViewPort = new RectangleF(minMZ, 0, maxMZ - minMZ, (float)peakHeight);
                mctl_spectra.HasLegend = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }

        }



        #region "Export Routines"
        private void ExportSpectrumToFile(double threshold)
        {
            try
            {
                var fileDialog = new System.Windows.Forms.SaveFileDialog();
                fileDialog.AddExtension = true;
                fileDialog.CheckPathExists = true;
                fileDialog.DefaultExt = "*.csv";
                fileDialog.DereferenceLinks = true;
                fileDialog.ValidateNames = true;
                fileDialog.Filter = "Comma Separated Files (*.csv)|*.csv";
                fileDialog.OverwritePrompt = true;
                fileDialog.FilterIndex = 1;
                if (fileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                System.IO.TextWriter writer = new System.IO.StreamWriter(fileDialog.FileName, false);
                var num_pts = marr_current_mzs.Length;
                for (var pt_num = 0; pt_num < num_pts; pt_num++)
                {
                    if (marr_current_intensities[pt_num] < threshold)
                        continue;
                    writer.Write(marr_current_mzs[pt_num]);
                    writer.Write(",");
                    writer.WriteLine(marr_current_intensities[pt_num]);
                }
                writer.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }

        private void ExportSpectrumToClipboard(double threshold)
        {


            try
            {
                var num_pts = marr_current_mzs.Length;
                var strBuilder = new System.Text.StringBuilder(2 * num_pts * (2 * 12 + 3));
                for (var pt_num = 0; pt_num < num_pts; pt_num++)
                {
                    if (marr_current_intensities[pt_num] < threshold)
                        continue;
                    strBuilder.AppendFormat("{0:#.####}", marr_current_mzs[pt_num]);
                    strBuilder.Append(",");
                    strBuilder.AppendFormat("{0:f}", marr_current_intensities[pt_num]);
                    strBuilder.Append("\r\n");
                }
                Clipboard.SetDataObject(strBuilder.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }
        #endregion

        #region "Menu Event Handlers"
        private void menuItem_sg_smooth_Click(object sender, EventArgs e)
        {
            SmoothWithSavitzkyGolay();

        }
        private void menuItem_zero_fill_discontinuous_Click(object sender, EventArgs e)
        {
            try
            {
                var inputFrm = new frmFloatDialog();
                inputFrm.Prompt = "Please enter # of points to fill";
                inputFrm.EditingValue = Convert.ToSingle(mint_num_zero_fill_discontinuous);
                if (inputFrm.ShowDialog() == DialogResult.OK)
                {
                    var num_zero_fill = Convert.ToInt32(inputFrm.EditingValue);
                    if (num_zero_fill < 1)
                    {
                        MessageBox.Show(this, "Please enter a positive value for number of points");
                        return;
                    }
                    var num_pts_returned = DeconEngine.Utils.ZeroFillUnevenData(ref marr_current_mzs, ref marr_current_intensities, num_zero_fill);
                    ShowSpectrumInSpectralChart(mint_spectrum_num, false, false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }

        private void menuItem_export_spectrum_to_clipboard_Click(object sender, EventArgs e)
        {
            try
            {
                clsClipboardUtility.CopyXYValuesToClipboard(this.marr_current_mzs, this.marr_current_intensities);

                //ExportSpectrumToClipboard(Double.NegativeInfinity) ;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }
        private void menuItem_export_spectrum_to_clipboard_nonzero_Click(object sender, EventArgs e)
        {
            try
            {
                ExportSpectrumToClipboard(Double.Epsilon);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }

        private void menuItem_export_spectrum_Click(object sender, EventArgs e)
        {
            try
            {
                ExportSpectrumToFile(Double.NegativeInfinity);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }

        private void menuItem_export_spectrum_nonzero_Click(object sender, EventArgs e)
        {
            try
            {
                ExportSpectrumToFile(Double.Epsilon);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }
        #endregion

        # region "Event Handlers"

        //Added by Gord..  note see frmSpectra for other key press events. It seems to me
        //that this is the best place to put them
        private void mctl_spectra_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.T && !e.Handled && e.Modifiers == Keys.Shift)
            {
                InitiateAndExecuteTransform();

            }
            if (e.KeyCode == Keys.F && !e.Handled && e.Modifiers == Keys.Shift)
            {
                findPeaks();
            }

            if (e.KeyCode == Keys.S && !e.Handled && e.Modifiers == Keys.Shift)
            {
                SmoothWithSavitzkyGolay();
            }

            if (e.KeyCode == Keys.C && !e.Handled && e.Modifiers == Keys.Shift)
            {
                clsClipboardUtility.CopyXYValuesToClipboard(this.marr_current_mzs, this.marr_current_intensities);
            }




        }


        private void mScanTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled == false && e.KeyCode == Keys.Enter)
            {
                // update to the scan given in the box
                try
                {
                    var scan = int.Parse(this.mScanTextBox.Text);
                    var max_scan = mobjRawData.GetNumScans() + mobjRawData.GetFirstScanNum() - 1;
                    if (mobjRawData != null && scan > max_scan)
                    {
                        scan = max_scan;
                    }
                    if (mevntScanChanged != null)
                    {
                        mevntScanChanged(sender, scan);
                    }
                }
                catch (FormatException ex)
                {
                    MessageBox.Show(this, ex.Message);
                }
            }
        }
        private void menuItem_find_peaks_Click(object sender, EventArgs e)
        {
            try
            {
                findPeaks();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
            }
        }

        public void findPeaks()
        {
            if (mobjRawData == null) return;

            mlistView_peaks.Items.Clear();
            this.mlistView_peaks.ListViewItemSorter = null;

            if (mobjRawData != null &&
                (mobjRawData.FileType == DeconToolsV2.Readers.FileType.FINNIGAN ||
                mobjRawData.FileType == DeconToolsV2.Readers.FileType.MZXMLRAWDATA))
            {
                mobjPeakParameters.ThresholdedData = true;
            }





            mobj_PeakProcessor.SetOptions(mobjPeakParameters);
            mobj_peaks = new DeconToolsV2.Peaks.clsPeak[1];        //[gord] what's this doing?
            mobj_PeakProcessor.DiscoverPeaks(ref marr_current_mzs, ref marr_current_intensities, ref mobj_peaks,
                Convert.ToSingle(mobjTransformParameters.MinMZ), Convert.ToSingle(mobjTransformParameters.MaxMZ));
            mdbl_current_background_intensity = mobj_PeakProcessor.GetBackgroundIntensity(ref marr_current_intensities);

            var peakMzs = new float[mobj_peaks.Length];
            var peakIntensities = new float[mobj_peaks.Length];
            var peakFWHMs = new float[mobj_peaks.Length];

            for (var pkNum = 0; pkNum < mobj_peaks.Length; pkNum++)
            {
                peakMzs[pkNum] = Convert.ToSingle(mobj_peaks[pkNum].mdbl_mz);
                peakIntensities[pkNum] = Convert.ToSingle(mobj_peaks[pkNum].mdbl_intensity);
                peakFWHMs[pkNum] = Convert.ToSingle(mobj_peaks[pkNum].mdbl_FWHM);
            }

            mctl_spectra.AddPeaks(peakMzs, peakIntensities, peakFWHMs);

            mlistView_peaks.SuspendLayout();
            mlistView_peaks.Items.Clear();
            // now add the peaks to the listview as well.
            for (var pkNum = 0; pkNum < mobj_peaks.Length; pkNum++)
            {
                var item = mlistView_peaks.Items.Add(Convert.ToString(pkNum + 1));
                item.Tag = pkNum;
                item.SubItems.Add(Convert.ToString(mobj_peaks[pkNum].mdbl_mz));
                item.SubItems.Add(Convert.ToString(mobj_peaks[pkNum].mdbl_intensity));
                item.SubItems.Add(Convert.ToString(mobj_peaks[pkNum].mdbl_FWHM));
                item.SubItems.Add(Convert.ToString(mobj_peaks[pkNum].mdbl_SN));
            }
            mlistView_peaks.ResumeLayout(true);
            tabControl1.SelectedIndex = 2;
            mctl_spectra.Invalidate();
        }


        private void CheckDeconStatusHandler(object sender, EventArgs args)
        {
            try
            {
                if (!mMediator.StatusForm.IsHandleCreated)
                    return;
                var cancel = false;
                mMediator.RaiseProgressMessage(sender, mobjTransform.PercentDone, ref cancel);
                mMediator.RaiseStatusMessage(sender, mobjTransform.StatusMessage, ref cancel);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void PerformTransform()
        {
            try
            {
                mlistview_transform.SuspendLayout();
                mlistview_transform.Items.Clear();

                mbln_processing = true;
                marr_transformResults = new DeconToolsV2.HornTransform.clsHornTransformResults[1];
                mobjTransform.TransformParameters = mobjTransformParameters;

                var min_peptide_intensity = mdbl_current_background_intensity * mobjTransformParameters.PeptideMinBackgroundRatio;
                if (mobjTransformParameters.UseAbsolutePeptideIntensity)
                {
                    if (min_peptide_intensity < mobjTransformParameters.AbsolutePeptideIntensity)
                        min_peptide_intensity = mobjTransformParameters.AbsolutePeptideIntensity;
                }

                mobjTransform.PerformTransform(Convert.ToSingle(mdbl_current_background_intensity), Convert.ToSingle(min_peptide_intensity), ref marr_current_mzs, ref marr_current_intensities, ref mobj_peaks, ref marr_transformResults);

                //the following should not be part of 'PerformTransform'
                var chargePeaks = new PNNL.Controls.MS.clsChargePeak[marr_transformResults.Length];

                for (var chNum = 0; chNum < marr_transformResults.Length; chNum++)
                {
                    chargePeaks[chNum] = new PNNL.Controls.MS.clsChargePeak(marr_transformResults[chNum].mint_peak_index,
                        marr_transformResults[chNum].mshort_cs, Convert.ToInt16(marr_transformResults[chNum].mint_num_isotopes_observed),
                        marr_transformResults[chNum].marr_isotope_peak_indices);

                    var item = mlistview_transform.Items.Add(Convert.ToString(chNum + 1));
                    item.Tag = chNum;
                    item.SubItems.Add(Convert.ToString(marr_transformResults[chNum].mdbl_mono_mw));
                    item.SubItems.Add(Convert.ToString(marr_transformResults[chNum].mint_abundance));
                    item.SubItems.Add(Convert.ToString(marr_transformResults[chNum].mdbl_mz));
                    item.SubItems.Add(Convert.ToString(marr_transformResults[chNum].mshort_cs));
                    item.SubItems.Add(Convert.ToString(marr_transformResults[chNum].mdbl_fit));
                    item.SubItems.Add(Convert.ToString(marr_transformResults[chNum].mdbl_most_intense_mw));
                }
                mlistview_transform.ResumeLayout(true);
                mctl_spectra.AddCharges(chargePeaks);
                mbln_processing = false;
                mMediator.StatusForm.DialogResult = DialogResult.OK;
                this.BeginInvoke(new ThreadStart(InvokeMassTransformFinished));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
            }
        }

        private void InvokeMassTransformFinished()
        {
            mMediator.StatusForm.DialogResult = DialogResult.OK;
            mMediator.StatusForm.Hide();
        }



        private void displayFitResult()
        {
            var fitForm = new frmFitFormulaInput();
            if (fitForm.ShowDialog(this) != DialogResult.OK)
                return;
            var formula = fitForm.Formula;
            var charge = fitForm.Charge;
            DisplayFitResultForCurrentScan(formula, charge);
        }


        public void SmoothWithSavitzkyGolay()
        {
            try
            {
                DeconEngine.Utils.SavitzkyGolaySmooth(mobjTransformParameters.SGNumLeft, mobjTransformParameters.SGNumRight, mobjTransformParameters.SGOrder, ref marr_current_mzs, ref marr_current_intensities);
                ShowSpectrumInSpectralChart(mint_spectrum_num, false, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }

        }


        public void InitiateAndExecuteTransform()
        {
            this.mlistview_transform.ListViewItemSorter = null;
            if (mobj_peaks == null || mobj_peaks.Length == 0)
            {
                findPeaks();
            }
            mMediator.StatusForm.Reset();
            mMediator.StatusForm.Text = "Performing Mass Transform";
            mMediator.StatusForm.DialogResult = DialogResult.None;

            if (this.mStatusUpdateDelegate != null)
            {
                this.mStatusTimer.Tick -= this.mStatusUpdateDelegate;
            }

            //this.mthrd_status.Start();
            var processThreadStart = new ThreadStart(PerformTransform);
            mthrd_decon = new Thread(processThreadStart);
            mthrd_decon.Name = "Process Files";
            mthrd_decon.IsBackground = true;
            mthrd_decon.Start();

            this.mStatusUpdateDelegate = new EventHandler(this.CheckDeconStatusHandler);
            this.mStatusTimer.Tick += this.mStatusUpdateDelegate;
            this.mStatusTimer.Start();
            var result = mMediator.StatusForm.ShowDialog(this);

            mStatusTimer.Tick -= new EventHandler(this.mStatusUpdateDelegate);
            mStatusTimer.Stop();
            //Console.WriteLine("Dialog Result {0}", result);
            if (result == DialogResult.Cancel)
            {
                this.FinishOrAbortProcessing(true);
            }

            FinishOrAbortProcessing(false);
            tabControl1.SelectedIndex = 1;
            mctl_spectra.Invalidate();

        }


        private void menuItem_mass_transform_Click(object sender, EventArgs e)
        {
            try
            {
                InitiateAndExecuteTransform();

            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }

        private void FinishOrAbortProcessing(bool abort)
        {
            try
            {
                if (mbln_processing)
                {
                    this.mStatusTimer.Stop();
                    // doing its business.. someone wants to abort.
                    if (mthrd_decon != null && mthrd_decon.IsAlive)
                    {
                        if (abort)
                        {
                            mthrd_decon.Abort();
                        }
                        // wait until it to fully terminate or abort
                        mthrd_decon.Join();
                    }
                    mbln_processing = false;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }

        private void btnFindPeaks_Click(object sender, System.EventArgs e)
        {
            findPeaks();
        }
        private void btnTransformMS_Click(object sender, System.EventArgs e)
        {
            InitiateAndExecuteTransform();

        }
        private void btnSmooth_Click(object sender, System.EventArgs e)
        {
            SmoothWithSavitzkyGolay();

        }

        private void btnCopytoClipboard_Click(object sender, System.EventArgs e)
        {
            clsClipboardUtility.CopyXYValuesToClipboard(this.marr_current_mzs, this.marr_current_intensities);

        }

        private void btnDisplayFit_Click(object sender, System.EventArgs e)
        {
            this.displayFitResult();

        }


        public void CopyValuesToClipboard()
        {
            clsClipboardUtility.CopyXYValuesToClipboard(this.marr_current_mzs, this.marr_current_intensities);

        }




        #endregion

        #region "ListView related copying and saving."
        private string GetListViewDataAsString(ListView listView, string delimiter)
        {
            try
            {
                var num_peaks = listView.Items.Count;
                var num_columns = listView.Columns.Count;

                var strBuilder = new System.Text.StringBuilder(num_peaks * num_columns * 12);

                for (var col_num = 0; col_num < num_columns; col_num++)
                {
                    strBuilder.Append(listView.Columns[col_num].Text);
                    if (col_num != num_columns - 1)
                        strBuilder.Append(delimiter);
                }
                strBuilder.Append("\r\n");

                for (var pk_num = 0; pk_num < num_peaks; pk_num++)
                {
                    for (var col_num = 0; col_num < num_columns; col_num++)
                    {
                        strBuilder.AppendFormat("{0:#.####}", listView.Items[pk_num].SubItems[col_num].Text);
                        if (col_num != num_columns - 1)
                            strBuilder.Append(delimiter);
                    }
                    strBuilder.Append("\r\n");
                }
                return strBuilder.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
            return null;
        }

        private void menuItemPeaksSave_Click(object sender, EventArgs e)
        {
            try
            {
                var fileDialog = new System.Windows.Forms.SaveFileDialog();
                fileDialog.AddExtension = true;
                fileDialog.CheckPathExists = true;
                fileDialog.DefaultExt = "*.csv";
                fileDialog.DereferenceLinks = true;
                fileDialog.ValidateNames = true;
                fileDialog.Filter = "Comma Separated Files (*.csv)|*.csv|Tab delimited File (*.txt)|*.txt";
                fileDialog.OverwritePrompt = true;
                fileDialog.FilterIndex = 1;

                if (fileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var delimiter = "";
                if (fileDialog.FilterIndex == 1)
                    delimiter = ",";
                else
                    delimiter = "\t";

                System.IO.TextWriter writer = new System.IO.StreamWriter(fileDialog.FileName, false);
                writer.Write(GetListViewDataAsString(mlistView_peaks, delimiter));
                writer.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }

        private void menuItemPeaksCopy_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetDataObject(GetListViewDataAsString(mlistView_peaks, "\t"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }

        private void menuItemTransformSave_Click(object sender, EventArgs e)
        {
            try
            {
                var fileDialog = new System.Windows.Forms.SaveFileDialog();
                fileDialog.AddExtension = true;
                fileDialog.CheckPathExists = true;
                fileDialog.DefaultExt = "*.csv";
                fileDialog.DereferenceLinks = true;
                fileDialog.ValidateNames = true;
                fileDialog.Filter = "Comma Separated Files (*.csv)|*.csv|Tab delimited File (*.txt)|*.txt";
                fileDialog.OverwritePrompt = true;
                fileDialog.FilterIndex = 1;

                if (fileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var delimiter = "";
                if (fileDialog.FilterIndex == 1)
                    delimiter = ",";
                else
                    delimiter = "\t";

                System.IO.TextWriter writer = new System.IO.StreamWriter(fileDialog.FileName, false);
                writer.Write(GetListViewDataAsString(mlistview_transform, delimiter));
                writer.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }

        private void menuItemTransformCopy_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetDataObject(GetListViewDataAsString(mlistview_transform, "\t"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }
        private void mlistView_peaks_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            try
            {
                if (sender == mlistView_peaks)
                    this.mlistView_peaks.ListViewItemSorter = new ListViewItemComparer(e.Column);
                else if (sender == mlistview_transform)
                    this.mlistview_transform.ListViewItemSorter = new ListViewItemComparer(e.Column);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace);
            }
        }

        #endregion
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

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            var penProvider1 = new PNNL.Controls.PenProvider();
            var penProvider2 = new PNNL.Controls.PenProvider();
            var resources = new System.Resources.ResourceManager(typeof(ctlMassSpectrum));
            this.mctl_spectra = new PNNL.Controls.MS.ctlSpectrum();
            this.mpanelSpectrum = new System.Windows.Forms.Panel();
            this.txtRightFitStringencyFactor = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtLeftFitStringencyFactor = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnDisplayFit = new System.Windows.Forms.Button();
            this.btnCopytoClipboard = new System.Windows.Forms.Button();
            this.btnSmooth = new System.Windows.Forms.Button();
            this.btnTransformMS = new System.Windows.Forms.Button();
            this.btnFindPeaks = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.mScanTextBox = new System.Windows.Forms.TextBox();
            this.mtabPage_mass_transform = new System.Windows.Forms.TabPage();
            this.mlistview_transform = new System.Windows.Forms.ListView();
            this.mcolheader_index = new System.Windows.Forms.ColumnHeader();
            this.mcolheader_mass = new System.Windows.Forms.ColumnHeader();
            this.mcolheader_abundance = new System.Windows.Forms.ColumnHeader();
            this.mcolheader_mz = new System.Windows.Forms.ColumnHeader();
            this.mcolheader_charge = new System.Windows.Forms.ColumnHeader();
            this.mcolheader_fit = new System.Windows.Forms.ColumnHeader();
            this.mcolheader_mostmw = new System.Windows.Forms.ColumnHeader();
            this.mtabPage_peaks = new System.Windows.Forms.TabPage();
            this.mlistView_peaks = new System.Windows.Forms.ListView();
            this.mcolHeaderIndex = new System.Windows.Forms.ColumnHeader();
            this.mcolHeaderMZ = new System.Windows.Forms.ColumnHeader();
            this.mcolHeaderIntensity = new System.Windows.Forms.ColumnHeader();
            this.mcolHeaderFWHM = new System.Windows.Forms.ColumnHeader();
            this.mcolHeaderSN = new System.Windows.Forms.ColumnHeader();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.mctl_spectra)).BeginInit();
            this.mpanelSpectrum.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.mtabPage_mass_transform.SuspendLayout();
            this.mtabPage_peaks.SuspendLayout();
            this.SuspendLayout();
            //
            // mctl_spectra
            //
            this.mctl_spectra.AutoViewPortOnAddition = true;
            this.mctl_spectra.AutoViewPortOnSeriesChange = true;
            this.mctl_spectra.AutoViewPortXBase = 0F;
            this.mctl_spectra.AutoViewPortYAxis = true;
            this.mctl_spectra.AutoViewPortYBase = 0F;
            this.mctl_spectra.AxisAndLabelFont = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.mctl_spectra.AxisAndLabelMaxFontSize = 24;
            this.mctl_spectra.AxisAndLabelMinFontSize = 10;
            this.mctl_spectra.BackColor = System.Drawing.Color.WhiteSmoke;
            this.mctl_spectra.ChartBackgroundColor = System.Drawing.Color.White;
            this.mctl_spectra.ChartLayout.LegendFraction = 0.2F;
            this.mctl_spectra.ChartLayout.LegendLocation = PNNL.Controls.ChartLegendLocation.Right;
            this.mctl_spectra.ChartLayout.MaxLegendHeight = 150;
            this.mctl_spectra.ChartLayout.MaxLegendWidth = 250;
            this.mctl_spectra.ChartLayout.MaxTitleHeight = 50;
            this.mctl_spectra.ChartLayout.MinLegendHeight = 50;
            this.mctl_spectra.ChartLayout.MinLegendWidth = 75;
            this.mctl_spectra.ChartLayout.MinTitleHeight = 15;
            this.mctl_spectra.ChartLayout.TitleFraction = 0.1F;
            this.mctl_spectra.DefaultZoomHandler.Active = true;
            this.mctl_spectra.DefaultZoomHandler.FillColor = System.Drawing.Color.FromArgb(((System.Byte)(60)), ((System.Byte)(119)), ((System.Byte)(136)), ((System.Byte)(153)));
            this.mctl_spectra.DefaultZoomHandler.LineColor = System.Drawing.Color.Black;
            this.mctl_spectra.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mctl_spectra.FWHMFont = new System.Drawing.Font("Times New Roman", 8F);
            this.mctl_spectra.FWHMLineWidth = 1F;
            this.mctl_spectra.FWHMPeakColor = System.Drawing.Color.Purple;
            penProvider1.Color = System.Drawing.Color.FromArgb(((System.Byte)(211)), ((System.Byte)(211)), ((System.Byte)(211)));
            penProvider1.Width = 1F;
            this.mctl_spectra.GridLinePen = penProvider1;
            this.mctl_spectra.HasLegend = false;
            this.mctl_spectra.HilightColor = System.Drawing.Color.Magenta;
            this.mctl_spectra.LabelOffset = 4F;
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
            this.mctl_spectra.PeakColor = System.Drawing.Color.Black;
            this.mctl_spectra.PeakLabelRelativeHeightPercent = 5F;
            this.mctl_spectra.PeakLineEndCap = System.Drawing.Drawing2D.LineCap.Flat;
            this.mctl_spectra.Size = new System.Drawing.Size(768, 552);
            this.mctl_spectra.TabIndex = 1;
            this.mctl_spectra.Title = "Mass Spectrum";
            this.mctl_spectra.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 14F);
            this.mctl_spectra.TitleMaxFontSize = 14F;
            this.mctl_spectra.TitleMinFontSize = 6F;
            this.mctl_spectra.VerticalExpansion = 1.1F;
            this.mctl_spectra.ViewPort = ((System.Drawing.RectangleF)(resources.GetObject("mctl_spectra.ViewPort")));
            this.mctl_spectra.XAxisLabel = "m/z";
            this.mctl_spectra.YAxisLabel = "intensity";
            this.mctl_spectra.KeyDown += new System.Windows.Forms.KeyEventHandler(this.mctl_spectra_KeyDown);
            //
            // mpanelSpectrum
            //
            this.mpanelSpectrum.Controls.Add(this.txtRightFitStringencyFactor);
            this.mpanelSpectrum.Controls.Add(this.label2);
            this.mpanelSpectrum.Controls.Add(this.txtLeftFitStringencyFactor);
            this.mpanelSpectrum.Controls.Add(this.label1);
            this.mpanelSpectrum.Controls.Add(this.btnDisplayFit);
            this.mpanelSpectrum.Controls.Add(this.btnCopytoClipboard);
            this.mpanelSpectrum.Controls.Add(this.btnSmooth);
            this.mpanelSpectrum.Controls.Add(this.btnTransformMS);
            this.mpanelSpectrum.Controls.Add(this.btnFindPeaks);
            this.mpanelSpectrum.Controls.Add(this.mctl_spectra);
            this.mpanelSpectrum.Controls.Add(this.textBox2);
            this.mpanelSpectrum.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mpanelSpectrum.Location = new System.Drawing.Point(0, 0);
            this.mpanelSpectrum.Name = "mpanelSpectrum";
            this.mpanelSpectrum.Size = new System.Drawing.Size(768, 552);
            this.mpanelSpectrum.TabIndex = 2;
            //
            // txtRightFitStringencyFactor
            //
            this.txtRightFitStringencyFactor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtRightFitStringencyFactor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRightFitStringencyFactor.Location = new System.Drawing.Point(32, 529);
            this.txtRightFitStringencyFactor.Name = "txtRightFitStringencyFactor";
            this.txtRightFitStringencyFactor.Size = new System.Drawing.Size(40, 20);
            this.txtRightFitStringencyFactor.TabIndex = 9;
            this.txtRightFitStringencyFactor.Text = "";
            this.toolTip1.SetToolTip(this.txtRightFitStringencyFactor, "\'Right Fit Stringency Factor\'");
            this.txtRightFitStringencyFactor.TextChanged += new System.EventHandler(this.txtRightFitStringencyFactor_TextChanged);
            //
            // label2
            //
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label2.Location = new System.Drawing.Point(3, 529);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 24);
            this.label2.TabIndex = 10;
            this.label2.Text = "RFF";
            this.toolTip1.SetToolTip(this.label2, "\'Right Fit Stringency Factor\'");
            //
            // txtLeftFitStringencyFactor
            //
            this.txtLeftFitStringencyFactor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtLeftFitStringencyFactor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLeftFitStringencyFactor.Location = new System.Drawing.Point(32, 507);
            this.txtLeftFitStringencyFactor.Name = "txtLeftFitStringencyFactor";
            this.txtLeftFitStringencyFactor.Size = new System.Drawing.Size(40, 20);
            this.txtLeftFitStringencyFactor.TabIndex = 9;
            this.txtLeftFitStringencyFactor.Text = "";
            this.toolTip1.SetToolTip(this.txtLeftFitStringencyFactor, "\'Left Fit Stringency Factor\'");
            this.txtLeftFitStringencyFactor.TextChanged += new System.EventHandler(this.txtLeftFitStringencyFactor_TextChanged);
            //
            // label1
            //
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.label1.Location = new System.Drawing.Point(3, 507);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 24);
            this.label1.TabIndex = 10;
            this.label1.Text = "LFF";
            //
            // btnDisplayFit
            //
            this.btnDisplayFit.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnDisplayFit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDisplayFit.Location = new System.Drawing.Point(2, 72);
            this.btnDisplayFit.Name = "btnDisplayFit";
            this.btnDisplayFit.Size = new System.Drawing.Size(75, 24);
            this.btnDisplayFit.TabIndex = 8;
            this.btnDisplayFit.Text = "Fit";
            this.toolTip1.SetToolTip(this.btnDisplayFit, "Overlay MS with theoretical");
            this.btnDisplayFit.Click += new System.EventHandler(this.btnDisplayFit_Click);
            //
            // btnCopytoClipboard
            //
            this.btnCopytoClipboard.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnCopytoClipboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCopytoClipboard.Location = new System.Drawing.Point(2, 97);
            this.btnCopytoClipboard.Name = "btnCopytoClipboard";
            this.btnCopytoClipboard.Size = new System.Drawing.Size(75, 32);
            this.btnCopytoClipboard.TabIndex = 5;
            this.btnCopytoClipboard.Text = "Copy data to clipboard";
            this.toolTip1.SetToolTip(this.btnCopytoClipboard, "Copy XY values to clipboard (Shift + \'C\')");
            this.btnCopytoClipboard.Click += new System.EventHandler(this.btnCopytoClipboard_Click);
            //
            // btnSmooth
            //
            this.btnSmooth.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnSmooth.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSmooth.Location = new System.Drawing.Point(2, 0);
            this.btnSmooth.Name = "btnSmooth";
            this.btnSmooth.TabIndex = 4;
            this.btnSmooth.Text = "Smooth";
            this.toolTip1.SetToolTip(this.btnSmooth, "Apply Savitzky-Golay smoothing (Shift + \'S\')");
            this.btnSmooth.Click += new System.EventHandler(this.btnSmooth_Click);
            //
            // btnTransformMS
            //
            this.btnTransformMS.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnTransformMS.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTransformMS.Location = new System.Drawing.Point(2, 48);
            this.btnTransformMS.Name = "btnTransformMS";
            this.btnTransformMS.TabIndex = 3;
            this.btnTransformMS.Text = "Transform";
            this.toolTip1.SetToolTip(this.btnTransformMS, "Apply Horn Transform to MS (shift + \'T\')");
            this.btnTransformMS.Click += new System.EventHandler(this.btnTransformMS_Click);
            //
            // btnFindPeaks
            //
            this.btnFindPeaks.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnFindPeaks.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFindPeaks.Location = new System.Drawing.Point(2, 24);
            this.btnFindPeaks.Name = "btnFindPeaks";
            this.btnFindPeaks.TabIndex = 2;
            this.btnFindPeaks.Text = "Find Peaks";
            this.toolTip1.SetToolTip(this.btnFindPeaks, "Find MS Peaks (shift+\'F\')");
            this.btnFindPeaks.Click += new System.EventHandler(this.btnFindPeaks_Click);
            //
            // textBox2
            //
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox2.Location = new System.Drawing.Point(40, 160);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(40, 20);
            this.textBox2.TabIndex = 9;
            this.textBox2.Text = "";
            this.toolTip1.SetToolTip(this.textBox2, "\'Left Fit Stringency Factor\'");
            //
            // panel1
            //
            this.panel1.Controls.Add(this.tabControl1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(776, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(248, 552);
            this.panel1.TabIndex = 3;
            //
            // tabControl1
            //
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.mtabPage_mass_transform);
            this.tabControl1.Controls.Add(this.mtabPage_peaks);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(248, 552);
            this.tabControl1.TabIndex = 3;
            //
            // tabPage1
            //
            this.tabPage1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.tabPage1.Controls.Add(this.mScanTextBox);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(240, 526);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Scan #";
            //
            // mScanTextBox
            //
            this.mScanTextBox.Location = new System.Drawing.Point(8, 8);
            this.mScanTextBox.Name = "mScanTextBox";
            this.mScanTextBox.Size = new System.Drawing.Size(136, 20);
            this.mScanTextBox.TabIndex = 0;
            this.mScanTextBox.Text = "";
            //
            // mtabPage_mass_transform
            //
            this.mtabPage_mass_transform.BackColor = System.Drawing.Color.WhiteSmoke;
            this.mtabPage_mass_transform.Controls.Add(this.mlistview_transform);
            this.mtabPage_mass_transform.Location = new System.Drawing.Point(4, 22);
            this.mtabPage_mass_transform.Name = "mtabPage_mass_transform";
            this.mtabPage_mass_transform.Size = new System.Drawing.Size(240, 526);
            this.mtabPage_mass_transform.TabIndex = 1;
            this.mtabPage_mass_transform.Text = "Mass Transform Results";
            this.mtabPage_mass_transform.Visible = false;
            //
            // mlistview_transform
            //
            this.mlistview_transform.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                                  this.mcolheader_index,
                                                                                                  this.mcolheader_mass,
                                                                                                  this.mcolheader_abundance,
                                                                                                  this.mcolheader_mz,
                                                                                                  this.mcolheader_charge,
                                                                                                  this.mcolheader_fit,
                                                                                                  this.mcolheader_mostmw});
            this.mlistview_transform.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mlistview_transform.FullRowSelect = true;
            this.mlistview_transform.GridLines = true;
            this.mlistview_transform.Location = new System.Drawing.Point(0, 0);
            this.mlistview_transform.Name = "mlistview_transform";
            this.mlistview_transform.Size = new System.Drawing.Size(240, 526);
            this.mlistview_transform.TabIndex = 0;
            this.mlistview_transform.View = System.Windows.Forms.View.Details;
            //
            // mcolheader_index
            //
            this.mcolheader_index.Text = "#";
            this.mcolheader_index.Width = 30;
            //
            // mcolheader_mass
            //
            this.mcolheader_mass.Text = "MonoMW";
            //
            // mcolheader_abundance
            //
            this.mcolheader_abundance.Text = "Intensity";
            //
            // mcolheader_mz
            //
            this.mcolheader_mz.Text = "m/z";
            this.mcolheader_mz.Width = 50;
            //
            // mcolheader_charge
            //
            this.mcolheader_charge.Text = "Z";
            this.mcolheader_charge.Width = 30;
            //
            // mcolheader_fit
            //
            this.mcolheader_fit.Text = "fit";
            this.mcolheader_fit.Width = 20;
            //
            // mcolheader_mostmw
            //
            this.mcolheader_mostmw.Text = "MostMW";
            //
            // mtabPage_peaks
            //
            this.mtabPage_peaks.BackColor = System.Drawing.Color.WhiteSmoke;
            this.mtabPage_peaks.Controls.Add(this.mlistView_peaks);
            this.mtabPage_peaks.Location = new System.Drawing.Point(4, 22);
            this.mtabPage_peaks.Name = "mtabPage_peaks";
            this.mtabPage_peaks.Size = new System.Drawing.Size(240, 526);
            this.mtabPage_peaks.TabIndex = 2;
            this.mtabPage_peaks.Text = "Peaks";
            this.mtabPage_peaks.Visible = false;
            //
            // mlistView_peaks
            //
            this.mlistView_peaks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                              this.mcolHeaderIndex,
                                                                                              this.mcolHeaderMZ,
                                                                                              this.mcolHeaderIntensity,
                                                                                              this.mcolHeaderFWHM,
                                                                                              this.mcolHeaderSN});
            this.mlistView_peaks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mlistView_peaks.FullRowSelect = true;
            this.mlistView_peaks.GridLines = true;
            this.mlistView_peaks.Location = new System.Drawing.Point(0, 0);
            this.mlistView_peaks.Name = "mlistView_peaks";
            this.mlistView_peaks.Size = new System.Drawing.Size(240, 526);
            this.mlistView_peaks.TabIndex = 0;
            this.mlistView_peaks.View = System.Windows.Forms.View.Details;
            //
            // mcolHeaderIndex
            //
            this.mcolHeaderIndex.Text = "Index";
            //
            // mcolHeaderMZ
            //
            this.mcolHeaderMZ.Text = "m/z";
            //
            // mcolHeaderIntensity
            //
            this.mcolHeaderIntensity.Text = "Intensity";
            //
            // mcolHeaderFWHM
            //
            this.mcolHeaderFWHM.Text = "FWHM";
            //
            // mcolHeaderSN
            //
            this.mcolHeaderSN.Text = "S/N";
            //
            // splitter1
            //
            this.splitter1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.splitter1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter1.Location = new System.Drawing.Point(768, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(8, 552);
            this.splitter1.TabIndex = 4;
            this.splitter1.TabStop = false;
            //
            // ctlMassSpectrum
            //
            this.Controls.Add(this.mpanelSpectrum);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel1);
            this.Name = "ctlMassSpectrum";
            this.Size = new System.Drawing.Size(1024, 552);
            ((System.ComponentModel.ISupportInitialize)(this.mctl_spectra)).EndInit();
            this.mpanelSpectrum.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.mtabPage_mass_transform.ResumeLayout(false);
            this.mtabPage_peaks.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void menuItem_apodize_Click(object sender, EventArgs e)
        {
            mobjRawData.FTICRRawPreprocessOptions = mobjFTICRPreProcessOptions;
            var sampleRate = mobjRawData.GetFTICRSamplingRate();
            mobjRawData.GetFTICRTransient(ref marr_current_intensities);
            marr_current_time_domain_values = new float[marr_current_intensities.Length];
            for (var i = 0; i < marr_current_intensities.Length; i++)
                marr_current_time_domain_values[i] = Convert.ToSingle((i * 1.0) / sampleRate);

            ClearSpectralChart();
            AddDataToSpectralChart();
            mctl_spectra.ViewPortHistory.Clear();
            mctl_spectra.AutoViewPort();
        }

        private void SetTimeDomainMenuItemsEnabled(bool enabled)
        {
            menuItem_apodize.Enabled = enabled;
            menuItem_zero_fill_time_domain.Enabled = enabled;
        }

        private void menuItem_time_domain_Click(object sender, EventArgs e)
        {

            if (mblnDisplayTimeDomain)
            {
                mblnDisplayTimeDomain = false;
                menuItem_time_domain.Text = "Convert to Time Domain";

                var xvals = XYValueConverter.ConvertFloatsToDoubles(marr_current_mzs);
                var yvals = XYValueConverter.ConvertFloatsToDoubles(marr_current_intensities);

                mobjRawData.GetSpectrum(mint_spectrum_num, ref xvals, ref yvals, false);

                XYValueConverter.ConvertDoublesToFloats(xvals, ref marr_current_mzs);
                XYValueConverter.ConvertDoublesToFloats(yvals, ref marr_current_intensities);
            }
            else
            {
                mblnDisplayTimeDomain = true;
                menuItem_time_domain.Text = "Convert to Mass";
                var sampleRate = mobjRawData.GetFTICRSamplingRate();
                mobjRawData.GetFTICRTransient(ref marr_current_intensities);
                marr_current_time_domain_values = new float[marr_current_intensities.Length];
                for (var i = 0; i < marr_current_intensities.Length; i++)
                    marr_current_time_domain_values[i] = Convert.ToSingle((i * 1.0) / sampleRate);
            }

            SetTimeDomainMenuItemsEnabled(mblnDisplayTimeDomain);

            ClearSpectralChart();
            AddDataToSpectralChart();

            if (mblnDisplayTimeDomain)
            {
                mctl_spectra.ViewPortHistory.Clear();
                mctl_spectra.AutoViewPort();
            }
            else
            {
                var minY = mctl_spectra.ViewPort.Y;
                var height = mctl_spectra.ViewPort.Height;
                mctl_spectra.ViewPortHistory.Clear();
                mctl_spectra.ViewPortHistory.SetCurrentEntry(new RectangleF(400, minY, 1600, height), true, false);
            }
        }

        private void ZeroFillTimeDomain()
        {
            mobjRawData.FTICRRawPreprocessOptions = mobjFTICRPreProcessOptions;

            var sampleRate = mobjRawData.GetFTICRSamplingRate();
            mobjRawData.GetFTICRTransient(ref marr_current_intensities);
            marr_current_time_domain_values = new float[marr_current_intensities.Length];
            for (var i = 0; i < marr_current_intensities.Length; i++)
                marr_current_time_domain_values[i] = Convert.ToSingle((i * 1.0) / sampleRate);

            ClearSpectralChart();
            AddDataToSpectralChart();
            mctl_spectra.ViewPortHistory.Clear();
            mctl_spectra.AutoViewPort();
        }

        private void menuItem_zero_fill_time_domain_Click(object sender, EventArgs e)
        {
            ZeroFillTimeDomain();
        }

        private void mlistview_transform_ItemActivate(object sender, EventArgs e)
        {
            if (mlistview_transform.SelectedItems.Count == 0)
                return;
            var selectedIndex = mlistview_transform.SelectedIndices[0];
            var selectedItem = mlistview_transform.Items[selectedIndex];
            var mz = Convert.ToDouble(selectedItem.SubItems[3].Text);
            var mostAbundantMW = Convert.ToDouble(selectedItem.SubItems[6].Text);
            var charge = Convert.ToInt16(selectedItem.SubItems[4].Text);
            var averageMass = (mz - 1.00727638) * charge;
            var empiricalFormula = mobjAveragine.GenerateAveragineFormula(averageMass, mobjTransformParameters.AveragineFormula,
                mobjTransformParameters.TagFormula);
            DisplayFitResultForCurrentScan(empiricalFormula, charge, mz, mostAbundantMW / charge + 1.00727638);
        }

        private void menuItemShowHypertransformSpectrum_Click(object sender, EventArgs e)
        {
            if (mlistview_transform.SelectedItems.Count == 0)
                return;
            var selectedIndex = mlistview_transform.SelectedIndices[0];
            var selectedItem = mlistview_transform.Items[selectedIndex];
            var mostAbundantMW = Convert.ToDouble(selectedItem.SubItems[6].Text);
            var charge = Convert.ToInt16(selectedItem.SubItems[4].Text);
            // now find the alternative charge state pieces.
            var sumMZs = new float[1];
            var sumIntensities = new float[1];
            var hyperTransform = new DeconToolsV2.clsHyperTransform();
            hyperTransform.GetHyperTransformSpectrum(ref marr_transformResults, mostAbundantMW, charge, ref sumMZs, ref sumIntensities, ref marr_current_mzs, ref marr_current_intensities);
            var newForm = new Form();
            var massSpec = new ctlMassSpectrum();
            massSpec.HornTransformParameters = this.HornTransformParameters;
            massSpec.PeakProcessorParameters = this.PeakProcessorParameters;
            massSpec.Mediator = Mediator;
            massSpec.PeakProcessorParameters.PeakBackgroundRatio = 0.6;
            massSpec.ShowSpectrum("Summed Spectrum", Convert.ToString(mostAbundantMW), sumMZs, sumIntensities);
            massSpec.Dock = DockStyle.Fill;
            newForm.Controls.Add(massSpec);
            newForm.ShowDialog(this.Parent);
        }

        public void ShowSpectrum(string name, string title, float[] mzs, float[] intensities)
        {
            mstrDescription = title;

            marr_current_mzs = mzs;
            marr_current_intensities = intensities;
            mobj_peaks = null;
            marr_transformResults = null;
            mlistView_peaks.Items.Clear();
            mlistview_transform.Items.Clear();

            var currentVP = this.mctl_spectra.ViewPort;
            ClearSpectralChart();

            var plotParams = (clsPlotParams)mobj_spectrum_plt_params.Clone();
            plotParams.Name = name;

            mobj_spectrum_series = new clsSpectraSeries(
                new PNNL.Controls.ArrayChartDataProvider(marr_current_mzs, marr_current_intensities),
                plotParams, name);

            mhash_series[mint_spectrum_num] = mobj_spectrum_series;
            mctl_spectra.SeriesCollection.Add(mobj_spectrum_series);
            mctl_spectra.Title = title;


            mctl_spectra.ViewPortHistory.Clear();
            mctl_spectra.AutoViewPort();

            // Update scan text box
            this.mScanTextBox.Text = mint_spectrum_num.ToString();
            this.mctl_spectra.Title = title;
        }



        private void copyXYDataToClipBoard()
        {

        }

        private void txtLeftFitStringencyFactor_TextChanged(object sender, System.EventArgs e)
        {
            try
            {
                mobjTransformParameters.LeftFitStringencyFactor = Convert.ToDouble(this.txtLeftFitStringencyFactor.Text);
            }
            catch
            {
                updateFormTransformParameters();
                this.txtLeftFitStringencyFactor.SelectAll();
            }

        }

        private void txtRightFitStringencyFactor_TextChanged(object sender, System.EventArgs e)
        {
            try
            {
                mobjTransformParameters.RightFitStringencyFactor = Convert.ToDouble(this.txtRightFitStringencyFactor.Text);
            }
            catch
            {
                updateFormTransformParameters();
                this.txtRightFitStringencyFactor.SelectAll();
            }


        }


        private void updateFormTransformParameters()
        {
            this.txtRightFitStringencyFactor.Text = Convert.ToString(mobjTransformParameters.RightFitStringencyFactor);
            this.txtLeftFitStringencyFactor.Text = Convert.ToString(mobjTransformParameters.LeftFitStringencyFactor);
        }











    }
    // Implements the manual sorting of items by columns.
    class ListViewItemComparer : IComparer
    {
        private readonly int col;
        public ListViewItemComparer()
        {
            col = 0;
        }
        public ListViewItemComparer(int column)
        {
            col = column;
        }
        public int Compare(object x, object y)
        {
            var val1 = Convert.ToDouble(((System.Windows.Forms.ListViewItem)(x)).SubItems[col].Text);
            var val2 = Convert.ToDouble(((System.Windows.Forms.ListViewItem)(y)).SubItems[col].Text);
            return val1.CompareTo(val2);
        }
    }

}
