using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using PNNL.Controls;
using System.Reflection;
using System.IO ; 
using System.Text ; 
using System.Data.Odbc ; 
using System.Data ; 



namespace Decon2LS
{
    /// <summary>
    /// Summary description for frmTICViewer.
    /// </summary>
    public class frmTICViewer : PNNL.Controls.CategorizedForm, IMediatedForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private PNNL.Controls.ctlLineChart mctl_tic; 
        private PNNL.Controls.ctlLineChart mctl_bpi ; 

        const double TIC_HEIGHT_RATIO=0.5;
         

        
        
        private PNNL.Controls.ArrayChartDataProvider m_tic_chart_data_provider;
        private PNNL.Controls.clsSeries mobj_tic_series ; 	
        private PNNL.Controls.clsPlotParams mobj_tic_plt_params ;

        private float [] marr_scans = null ; 
        private float [] marr_tic_values = null ;
        private float [] marr_bpi_values  = null ;
        private System.Data.DataTable mdata_tbl ; 		
        private PNNL.Controls.ArrayChartDataProvider m_bpi_chart_data_provider;
        private PNNL.Controls.clsSeries mobj_bpi_series ; 	
        private PNNL.Controls.clsPlotParams mobj_bpi_plt_params ;
        private DeconToolsV2.Readers.clsRawData mobjRawData ; //For ICR2ls TIC files
        private clsMediator mMediator;		

        public String mFileNameForHeader ; 
        public String mFileName ; 

        public frmTICViewer()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            mMediator = new clsMediator(this) ; 
            Init()  ; 

            this.frmTICViewer_Resize(this,null);

        }

        private void Init()
        {
            try
            {
                
                PNNL.Controls.DiamondShape shape = new PNNL.Controls.DiamondShape(3, false) ; 
                mobj_tic_plt_params = new PNNL.Controls.clsPlotParams(shape, Color.Red, false, true, true) ; 
                mobj_bpi_plt_params = new PNNL.Controls.clsPlotParams(shape, Color.Blue, false, true, true) ; 
                m_tic_chart_data_provider = new PNNL.Controls.ArrayChartDataProvider() ; 
                m_bpi_chart_data_provider = new PNNL.Controls.ArrayChartDataProvider() ; 
                mobjRawData = new DeconToolsV2.Readers.clsRawData() ;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message) ; 
            }
        }

        public void LoadIcr2lsTICFile() 
        {
            try
            {				
                mobjRawData.LoadFile(mFileName, DeconToolsV2.Readers.FileType.ICR2LSRAWDATA)  ; 
                mobjRawData.GetTicFromFile(ref marr_tic_values, ref marr_scans, true) ; 
                clsPlotParams plotParams = (clsPlotParams) mobj_tic_plt_params.Clone();
                plotParams.Name = "TIC" ;				

                mobj_tic_series = 
                    new clsSpectraSeries(new PNNL.Controls.ArrayChartDataProvider(marr_scans, marr_tic_values), 
                    plotParams, mFileNameForHeader) ; 
                mctl_tic.SeriesCollection.Add(mobj_tic_series) ; 					
                m_tic_chart_data_provider.SetData(marr_scans, marr_tic_values);
                mctl_tic.Title = "TIC" ; 
                this.mctl_tic.ViewPortHistory.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace) ; 
            }
        }


        public void LoadScansTICFile() 
        {
            
            try
            {
                mdata_tbl = new System.Data.DataTable() ; 
                using (clsGenericParserAdapter parser = new clsGenericParserAdapter())
                {
                    parser.SetDataSource(mFileName) ; 
                    parser.ColumnDelimiter = ",".ToCharArray() ; 
                    parser.FirstRowHasHeader = true ; 
                    parser.MaxBufferSize = 4096 ; 
                    parser.TextQualifier  = '\'';
                    mdata_tbl = parser.GetDataTable() ; 
                    int scan_num_col = parser.GetColumnIndex("scan_num") ; 
                    int tic_col = parser.GetColumnIndex("tic") ; 
                    int bpi_col = parser.GetColumnIndex("bpi") ; 
                    int num_entries  = mdata_tbl.Rows.Count ; 
                    double scan ; 
                    double tic ; 
                    double bpi ; 

                    marr_tic_values = new float[num_entries] ; 
                    marr_scans = new float[num_entries] ; 
                    marr_bpi_values = new float[num_entries] ; 
                    
                    for (int entry_num = 0 ; entry_num < num_entries ; entry_num++)
                    {
                        DataRow row = mdata_tbl.Rows[entry_num] ; 
                        scan =  Convert.ToDouble(row[scan_num_col].ToString())  ; 
                        tic = Convert.ToDouble(row[tic_col].ToString())  ; 
                        bpi = Convert.ToDouble(row[bpi_col].ToString()) ; 

                        marr_scans.SetValue((float)scan , entry_num) ; 
                        marr_tic_values.SetValue((float)tic , entry_num) ; 
                        marr_bpi_values.SetValue((float)bpi, entry_num) ; 
                    }

                    parser.Close() ; 

                    clsPlotParams plotParams = (clsPlotParams) mobj_tic_plt_params.Clone();
                    plotParams.Name = "TIC" ;

                    mobj_tic_series = 
                        new clsSpectraSeries(new PNNL.Controls.ArrayChartDataProvider(marr_scans, marr_tic_values), 
                        plotParams, mFileNameForHeader) ; 

                    clsPlotParams plotParams1 = (clsPlotParams) mobj_bpi_plt_params.Clone() ;
                    plotParams1.Name = "BPI"  ; 

                    mobj_bpi_series = 
                        new clsSpectraSeries(new PNNL.Controls.ArrayChartDataProvider(marr_scans, marr_bpi_values), 
                        plotParams1, mFileNameForHeader) ; 

                        
                    mctl_tic.SeriesCollection.Add(mobj_tic_series) ; 					
                    mctl_bpi.SeriesCollection.Add(mobj_bpi_series) ; 
                    m_tic_chart_data_provider.SetData(marr_scans, marr_tic_values);
                    m_bpi_chart_data_provider.SetData(marr_scans, marr_bpi_values) ; 
                    mctl_bpi.Title = "BPI"; 
                    mctl_tic.Title = "TIC" ; 
                    this.Text = mFileNameForHeader ; 


                    this.mctl_tic.ViewPortHistory.Clear();
                    this.mctl_bpi.ViewPortHistory.Clear() ; 
                }
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
            PNNL.Controls.PenProvider penProvider1 = new PNNL.Controls.PenProvider();
            PNNL.Controls.PenProvider penProvider2 = new PNNL.Controls.PenProvider();
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmTICViewer));
            PNNL.Controls.PenProvider penProvider3 = new PNNL.Controls.PenProvider();
            PNNL.Controls.PenProvider penProvider4 = new PNNL.Controls.PenProvider();
            System.Configuration.AppSettingsReader configurationAppSettings = new System.Configuration.AppSettingsReader();
            this.mctl_bpi = new PNNL.Controls.ctlLineChart();
            this.mctl_tic = new PNNL.Controls.ctlLineChart();
            ((System.ComponentModel.ISupportInitialize)(this.mctl_bpi)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mctl_tic)).BeginInit();
            this.SuspendLayout();
            // 
            // mctl_bpi
            // 
            this.mctl_bpi.AutoViewPortOnAddition = true;
            this.mctl_bpi.AutoViewPortOnSeriesChange = true;
            this.mctl_bpi.AutoViewPortXBase = 0F;
            this.mctl_bpi.AutoViewPortYAxis = true;
            this.mctl_bpi.AutoViewPortYBase = 0F;
            this.mctl_bpi.AxisAndLabelFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.mctl_bpi.AxisAndLabelMaxFontSize = 12;
            this.mctl_bpi.AxisAndLabelMinFontSize = 8;
            this.mctl_bpi.ChartBackgroundColor = System.Drawing.Color.White;
            this.mctl_bpi.ChartLayout.LegendFraction = 0.2F;
            this.mctl_bpi.ChartLayout.LegendLocation = PNNL.Controls.ChartLegendLocation.Right;
            this.mctl_bpi.ChartLayout.MaxLegendHeight = 150;
            this.mctl_bpi.ChartLayout.MaxLegendWidth = 250;
            this.mctl_bpi.ChartLayout.MaxTitleHeight = 50;
            this.mctl_bpi.ChartLayout.MinLegendHeight = 50;
            this.mctl_bpi.ChartLayout.MinLegendWidth = 75;
            this.mctl_bpi.ChartLayout.MinTitleHeight = 15;
            this.mctl_bpi.ChartLayout.TitleFraction = 0.1F;
            this.mctl_bpi.DefaultZoomHandler.Active = true;
            this.mctl_bpi.DefaultZoomHandler.FillColor = System.Drawing.Color.FromArgb(((System.Byte)(60)), ((System.Byte)(119)), ((System.Byte)(136)), ((System.Byte)(153)));
            this.mctl_bpi.DefaultZoomHandler.LineColor = System.Drawing.Color.Black;
            penProvider1.Color = System.Drawing.Color.FromArgb(((System.Byte)(211)), ((System.Byte)(211)), ((System.Byte)(211)));
            penProvider1.Width = 1F;
            this.mctl_bpi.GridLinePen = penProvider1;
            this.mctl_bpi.HasLegend = false;
            this.mctl_bpi.HilightColor = System.Drawing.Color.Magenta;
            this.mctl_bpi.LabelOffset = 5F;
            this.mctl_bpi.Legend.BackColor = System.Drawing.Color.Transparent;
            penProvider2.Color = System.Drawing.Color.Black;
            penProvider2.Width = 1F;
            this.mctl_bpi.Legend.BorderPen = penProvider2;
            this.mctl_bpi.Legend.Bounds = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.mctl_bpi.Legend.ColumnWidth = 125;
            this.mctl_bpi.Legend.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.mctl_bpi.Legend.MaxFontSize = 12F;
            this.mctl_bpi.Legend.MinFontSize = 6F;
            this.mctl_bpi.Location = new System.Drawing.Point(24, 296);
            this.mctl_bpi.Margins.BottomMarginFraction = 0.1F;
            this.mctl_bpi.Margins.BottomMarginMax = 72;
            this.mctl_bpi.Margins.BottomMarginMin = 30;
            this.mctl_bpi.Margins.DefaultMarginFraction = 0.05F;
            this.mctl_bpi.Margins.DefaultMarginMax = 15;
            this.mctl_bpi.Margins.DefaultMarginMin = 5;
            this.mctl_bpi.Margins.LeftMarginFraction = 0.2F;
            this.mctl_bpi.Margins.LeftMarginMax = 150;
            this.mctl_bpi.Margins.LeftMarginMin = 72;
            this.mctl_bpi.Name = "mctl_bpi";
            this.mctl_bpi.NumXBins = 20;
            this.mctl_bpi.PanWithArrowKeys = false;
            this.mctl_bpi.Size = new System.Drawing.Size(600, 272);
            this.mctl_bpi.TabIndex = 0;
            this.mctl_bpi.Title = "";
            this.mctl_bpi.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 22F);
            this.mctl_bpi.TitleMaxFontSize = 50F;
            this.mctl_bpi.TitleMinFontSize = 6F;
            this.mctl_bpi.UseAutoViewPortYBase = true;
            this.mctl_bpi.VerticalExpansion = 1.15F;
            this.mctl_bpi.ViewPort = ((System.Drawing.RectangleF)(resources.GetObject("mctl_bpi.ViewPort")));
            this.mctl_bpi.XAxisLabel = "Scan Number";
            this.mctl_bpi.YAxisLabel = "Base Peak Intensity";
            // 
            // mctl_tic
            // 
            this.mctl_tic.AutoViewPortOnAddition = true;
            this.mctl_tic.AutoViewPortOnSeriesChange = true;
            this.mctl_tic.AutoViewPortXBase = 0F;
            this.mctl_tic.AutoViewPortYAxis = true;
            this.mctl_tic.AutoViewPortYBase = 0F;
            this.mctl_tic.AxisAndLabelFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.mctl_tic.AxisAndLabelMaxFontSize = 12;
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
            penProvider3.Color = System.Drawing.Color.FromArgb(((System.Byte)(211)), ((System.Byte)(211)), ((System.Byte)(211)));
            penProvider3.Width = 1F;
            this.mctl_tic.GridLinePen = penProvider3;
            this.mctl_tic.HasLegend = false;
            this.mctl_tic.HilightColor = System.Drawing.Color.Magenta;
            this.mctl_tic.LabelOffset = 5F;
            this.mctl_tic.Legend.BackColor = System.Drawing.Color.Transparent;
            penProvider4.Color = System.Drawing.Color.Black;
            penProvider4.Width = 1F;
            this.mctl_tic.Legend.BorderPen = penProvider4;
            this.mctl_tic.Legend.Bounds = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.mctl_tic.Legend.ColumnWidth = 125;
            this.mctl_tic.Legend.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.mctl_tic.Legend.MaxFontSize = 12F;
            this.mctl_tic.Legend.MinFontSize = 6F;
            this.mctl_tic.Location = new System.Drawing.Point(32, 16);
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
            this.mctl_tic.Size = new System.Drawing.Size(592, 248);
            this.mctl_tic.TabIndex = 0;
            this.mctl_tic.Title = "";
            this.mctl_tic.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 22F);
            this.mctl_tic.TitleMaxFontSize = 50F;
            this.mctl_tic.TitleMinFontSize = 6F;
            this.mctl_tic.UseAutoViewPortYBase = true;
            this.mctl_tic.VerticalExpansion = 1.15F;
            this.mctl_tic.ViewPort = ((System.Drawing.RectangleF)(resources.GetObject("mctl_tic.ViewPort")));
            this.mctl_tic.XAxisLabel = "Scan Number";
            this.mctl_tic.YAxisLabel = "Total Intensity";
            // 
            // frmTICViewer
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(792, 566);
            this.Controls.Add(this.mctl_tic);
            this.Controls.Add(this.mctl_bpi);
            this.IsMdiContainer = ((bool)(configurationAppSettings.GetValue("frmSpectra.IsMdiContainer", typeof(bool))));
            this.Name = "frmTICViewer";
            this.Text = "TICViewer";
            this.Resize += new System.EventHandler(this.frmTICViewer_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.mctl_bpi)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mctl_tic)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private void mctl_bpi_Load(object sender, System.EventArgs e)
        {
        
        }

        private void frmTICViewer_Resize(object sender, System.EventArgs e)
        {
            this.mctl_tic.Left=1;
            this.mctl_tic.Top=1;
            this.mctl_tic.Width=this.Width;
            this.mctl_tic.Height=(int)(this.Height*TIC_HEIGHT_RATIO);

            this.mctl_bpi.Left=1;
            this.mctl_bpi.Top=this.mctl_tic.Top+this.mctl_tic.Height;
            this.mctl_bpi.Width=this.Width;
            this.mctl_bpi.Height=this.Height-this.mctl_tic.Height-40;

            
        }


    /*	/// <summary>
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
                        return ; 
                    // Get the time that the spectrum occurs in the overall run
                    double spectrumTime = mdbl_spectrum_time ;
                    // Convert into a pixel offset relative to the left of the charting area
                    float xChartValue = this.mctl_tic.GetScreenPixelX((float) spectrumTime);
                    // Draw the line onto the chart					
                } 
                catch (Exception e) 
                {
                    Console.WriteLine(e);
                }
            }
        }*/

        #region IMediatedForm Members

        public clsMediator Mediator
        {
            get
            {
                return mMediator;
            }
        }

        #endregion

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
}
