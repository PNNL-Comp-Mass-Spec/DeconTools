// Written by Navdeep Jaitly and Kyle Littlefield 
// for the Department of Energy (PNNL, Richland, WA)
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
using System.Reflection;

namespace Decon2LS
{
    /// <summary>
    /// Summary description for frmMercury.
    /// </summary>
    public class frmMercury : PNNL.Controls.CategorizedForm
    {
        enum ResolutionMode  
        {
            Resolution, FWHM
        }

        #region Error Strings
        private static readonly String CHARGE_CARRIER_ERROR_MESSAGE = "Must be a real number";
        private static readonly String CHARGE_STATE_ERROR_MESSAGE = "Must be an integer";
        private static readonly String NO_ERROR_MESSAGE = "";
        private static readonly String PREVIEW_INPUT_ERRORS_MESSAGE = "Input errors exist - preview does not reflect current settings";
        private static readonly String PREVIEW_NOT_SYNCED_MESSAGE = "Preview may not be synced with current settings";
        private static readonly String RESOLUTION_ERROR_MESSAGE = "Must be a real number";
        #endregion
        
        private System.Windows.Forms.Panel mPreviewHoldingPanel;
        private System.Windows.Forms.TextBox mAverageMolecularWeightResultTextBox;
        private System.Windows.Forms.TextBox mMonoMolecularWeightResultTextBox;
        private System.Windows.Forms.TextBox mVarianceResultTextBox;
        private System.Windows.Forms.Label mReferenceLabel;
        private System.Windows.Forms.TextBox mProteinDNAEditorTextBox;
        private System.Windows.Forms.TabControl mProteinOrDNATab;
        private System.Windows.Forms.Panel mProteinButtonsPanel;
        private System.Windows.Forms.Button mProteinButtonAla;
        private System.Windows.Forms.Button mProteinButtonArg;
        private System.Windows.Forms.Button mProteinButtonAsn;
        private System.Windows.Forms.Button mProteinButtonAsp;
        private System.Windows.Forms.Button mProteinButtonCys;
        private System.Windows.Forms.Button mProteinButtonGln;
        private System.Windows.Forms.Button mProteinButtonMet;
        private System.Windows.Forms.Button mProteinButtonLys;
        private System.Windows.Forms.Button mProteinButtonLeu;
        private System.Windows.Forms.Button mProteinButtonIle;
        private System.Windows.Forms.Button mProteinButtonHis;
        private System.Windows.Forms.Button mProteinButtonGly;
        private System.Windows.Forms.Button mProteinButtonVal;
        private System.Windows.Forms.Button mProteinButtonTyr;
        private System.Windows.Forms.Button mProteinButtonTrp;
        private System.Windows.Forms.Button mProteinButtonThr;
        private System.Windows.Forms.Button mProteinButtonSer;
        private System.Windows.Forms.Button mProteinButtonPro;
        private System.Windows.Forms.Button mProteinButtonOrn;
        private System.Windows.Forms.Button mProteinButtonPhe;
        private System.Windows.Forms.Button mProteinButtonGlu;
        private System.Windows.Forms.Button UnknownmProteinButton;
        private System.Windows.Forms.Button mProteinButtonGlnGsp;
        private System.Windows.Forms.Button mProteinButtonAsnAsp;
        private System.Windows.Forms.Button mProteinButtonHse;
        public System.Windows.Forms.CheckBox mComplementCheckBox;
        private System.Windows.Forms.Panel mDNAButtonPanel;
        private System.Windows.Forms.Button mDNAButtonA;
        private System.Windows.Forms.Button mDNAButtonC;
        private System.Windows.Forms.Button mDNAButtonG;
        private System.Windows.Forms.Button mDNAButtonT;
        private System.Windows.Forms.Button mDNAButtonU;
        public System.Windows.Forms.CheckBox mTrisphosCheckBox;
        public System.Windows.Forms.CheckBox mCyclicPhosCheckBox;
        public System.Windows.Forms.ComboBox mDNARNACombo;
        public System.Windows.Forms.CheckBox mTermPhosCheckBox;
        private PNNL.Controls.ExpandPanel mAdvancedSettingsExpandPanel;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.CheckBox mcheckBoxAbsolute;
        public System.Windows.Forms.Label labelMaxIsotopeAbundace;
        private System.Windows.Forms.TextBox mtextBoxMostAbundant;
        private System.Windows.Forms.Button btnCopyToClipboard;
        private static readonly String MOLECULAR_FORMULA_ERROR_MESSAGE = "The given formula can not be parsed.";

        #region Categorization Info

        static frmMercury()
        {
            // Load the icons used for categorization info from embedded resources.
            MercuryIcon = PNNL.Controls.IconUtils.LoadIconFromAssembly(typeof(frmMercury), "Icons.Mercury.ico", 16, 16);
            MercuryCategory = new PNNL.Controls.CategoryInfo[] {new PNNL.Controls.CategoryInfo("Mercury", MercuryIcon)};
        }

        /// <summary>
        /// Adds the Mercury category to the file view.
        /// </summary>
        /// <param name="fileView"></param>
        public static void InitializeCategories(PNNL.Controls.ctlFileView fileView) 
        {
            fileView.AddCategory(MercuryCategory);
        }

        private static readonly Icon MercuryIcon;
        private static readonly PNNL.Controls.CategoryInfo[] MercuryCategory;
        private static readonly String CategorizedTextString = "Mercury";
        private static readonly String DetailFormula = "Formula: ";
        #endregion
    
        private DeconToolsV2.clsMercuryIsotopeDistribution mMercuryIsotopeDistribution;
        private MercuryDataProvider mPreviewSeriesDataProvider;
        private PNNL.Controls.clsSeries mPreviewSeries;
        private Decon2LS.MolecularFormula mMolecularFormula = MolecularFormula.Parse("C10 H22");
        private ResolutionMode mResolutionMode = ResolutionMode.Resolution;
        private double mFWHM = .05;

        private Decon2LS.MolecularFormulaTranslator mProteinTranslator;
        private Decon2LS.MolecularFormulaTranslator mDNATranslator;
        public System.Windows.Forms.GroupBox Frame5;
        public System.Windows.Forms.ComboBox cmbCterm;
        public System.Windows.Forms.ComboBox cmbNterm;
        public System.Windows.Forms.Label Label13;
        public System.Windows.Forms.Label Label12;
        public System.Windows.Forms.Button cmbGenerate;
        public System.Windows.Forms.Label Label8;
        public System.Windows.Forms.Label Label7;
        public System.Windows.Forms.Label Label4;
        public System.Windows.Forms.Label Label1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TabPage mProteinEditorTab;
        private System.Windows.Forms.TabPage mDNAEditorTab;
        private System.Windows.Forms.Panel mOptsPanel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ComboBox mMercurySizeCombo;
        private System.Windows.Forms.ErrorProvider mErrorProvider;
        private System.Windows.Forms.ErrorProvider mWarningProvider;
        public System.Windows.Forms.RadioButton mApodizationTypeOptGaussian;
        public System.Windows.Forms.RadioButton mApodizationTypeOptLorentzian;
        public System.Windows.Forms.TextBox mChargeStateTextBox;
        public System.Windows.Forms.TextBox mChargeCarrierMassTextBox;
        public System.Windows.Forms.TextBox mResolutionTextBox;
        private PNNL.Controls.ctlLineChart mPreviewChart;
        private System.Windows.Forms.Button mPreviewButton;
        private System.Windows.Forms.CheckBox mAutoPreviewCheckBox;
        public System.Windows.Forms.Label mMonoWeightLabel;
        public System.Windows.Forms.Label mVarianceLabel;
        public System.Windows.Forms.Label mAverageMolecularWeightLabel;
        private PNNL.Controls.ExpandPanel mEditorsExpandPanel;
        private PNNL.Controls.ExpandPanel mPreviewExpandPanel;
        private PNNL.Controls.VerticalBubbleUpLayout mVerticalBubbleUpLayout;
        private System.Windows.Forms.Panel panel4;
        private PNNL.Controls.ExpandPanel mSettingsExpandPanel;
        private PNNL.Controls.VerticalBubbleUpLayout mVerticalBubbleUpLayout2;
        public System.Windows.Forms.TextBox mMolecularFormulaTextBox;
        private System.Windows.Forms.Button mSimplifyFormulaButton;
        private System.Windows.Forms.GroupBox mApodizationTypeGroupBox;
        private System.Windows.Forms.TextBox mFWHMTextBox;
        private System.Windows.Forms.Label mFWHMLabel;
        private System.Windows.Forms.Panel mSettingsInternalPanel;
        private System.ComponentModel.IContainer components;

        public frmMercury()
        {
            try
            {
                // Create the preview series and add it to the chart
                this.mPreviewSeriesDataProvider = 
                    new MercuryDataProvider(new float[]{0, 0}, new float[]{1, 0});

                //
                // Required for Windows Form Designer support
                //
                InitializeComponent();

                InitializeMFTranslators();

                this.mMercuryIsotopeDistribution = new DeconToolsV2.clsMercuryIsotopeDistribution();

                // Add the options for Mercury Size - only powers of 2 are allowed
                // otherwise the fourier transform will fail (will cause the program to die
                // without any nice error messages).
                this.mMercurySizeCombo.Items.Add(128);
                this.mMercurySizeCombo.Items.Add(256);
                this.mMercurySizeCombo.Items.Add(512);
                this.mMercurySizeCombo.Items.Add(1024);
                this.mMercurySizeCombo.Items.Add(2048);
                this.mMercurySizeCombo.Items.Add(4096);
                this.mMercurySizeCombo.Items.Add(8192);
                this.mMercurySizeCombo.Items.Add(16384);
                this.mMercurySizeCombo.Items.Add(32768);
                this.mMercurySizeCombo.Items.Add(65536);
                this.mMercurySizeCombo.Items.Add(131072);
                this.mMercurySizeCombo.Items.Add(262144);
        
                mPreviewSeries = new clsMercurySeries(this.mPreviewSeriesDataProvider, 
                    new PNNL.Controls.clsPlotParams(new PNNL.Controls.SquareShape(3, false), Color.DimGray), 
                    this.mPreviewChart);

                // Add series to chart
                this.mPreviewChart.SeriesCollection.Add(mPreviewSeries);

                // Set initial categorized info
                this.Category = frmMercury.MercuryCategory;
                this.CategorizedText = frmMercury.CategorizedTextString;
                this.SyncDetails();
                this.CategorizedIcon = frmMercury.MercuryIcon;

                this.mDNARNACombo.SelectedIndex = 0;

                // Add handler to each of the protein buttons and dna buttons
                foreach (Button button in this.mProteinButtonsPanel.Controls) 
                {
                    button.Click += new EventHandler(this.ProteinOrDNAButtonClickHandler);
                }

                foreach (Button button in this.mDNAButtonPanel.Controls) 
                {
                    button.Click += new EventHandler(this.ProteinOrDNAButtonClickHandler);
                }

                this.Formula = MolecularFormula.Parse("C12 H26");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace) ; 
            }
        }

        private void InitializeMFTranslators() 
        {
            try
            {
                mProteinTranslator = new MolecularFormulaTranslator();
                mProteinTranslator.Add("U", MolecularFormula.Parse("C4 H7 N1 O2", "Hse"));
                mProteinTranslator.Add("A", MolecularFormula.Parse("C3 H5 N1 O1", "Ala"));
                mProteinTranslator.Add("R", MolecularFormula.Parse("C6 H12 N4 O1", "Arg"));
                mProteinTranslator.Add("N", MolecularFormula.Parse("C4 H6 N2 O2", "Asn"));
                mProteinTranslator.Add("D", MolecularFormula.Parse("C4 H5 N1 O3", "Asp"));
                mProteinTranslator.Add("CC", MolecularFormula.Parse("C4 H4 N2 O1 S1", "Cys"));
                mProteinTranslator.Add("C", MolecularFormula.Parse("C3 H5 N1 O1 S1", "Cys"));
                mProteinTranslator.Add("E", MolecularFormula.Parse("C5 H7 N1 O3", "Glu"));
                mProteinTranslator.Add("G", MolecularFormula.Parse("C2 H3 N1 O1", "Gly"));
                mProteinTranslator.Add("H", MolecularFormula.Parse("C6 H7 N3 O1", "His"));
                mProteinTranslator.Add("I", MolecularFormula.Parse("C6 H11 N1 O1", "Ile"));
                mProteinTranslator.Add("L", MolecularFormula.Parse("C6 H11 N1 O1", "Leu"));
                mProteinTranslator.Add("K", MolecularFormula.Parse("C6 H12 N2 O1", "Lys"));
                mProteinTranslator.Add("M", MolecularFormula.Parse("C5 H9 N1 O1 S1", "Met"));
                mProteinTranslator.Add("F", MolecularFormula.Parse("C9 H9 N1 O1", "Phe"));
                mProteinTranslator.Add("P", MolecularFormula.Parse("C5 H7 N1 O1", "Pro"));
                mProteinTranslator.Add("S", MolecularFormula.Parse("C3 H5 N1 O2", "Ser"));
                mProteinTranslator.Add("T", MolecularFormula.Parse("C4 H7 N1 O2", "Thr"));
                mProteinTranslator.Add("W", MolecularFormula.Parse("C11 H10 N2 O1", "Trp"));
                mProteinTranslator.Add("Y", MolecularFormula.Parse("C9 H9 N1 O2", "Tyr"));
                mProteinTranslator.Add("V", MolecularFormula.Parse("C5 H9 N1 O1", "Val"));
                mProteinTranslator.Add("Q", MolecularFormula.Parse("C5 H8 N2 O2", "Gln"));
                mProteinTranslator.Add("O", MolecularFormula.Parse("C5 H10 N2 O1", "Orn"));
                mProteinTranslator.Add("Z", MolecularFormula.Parse("C5 H8 N2 O2", "Gln/Glu"));
                mProteinTranslator.Add("B", MolecularFormula.Parse("C4 H6 N2 O2", "Asn/Asp"));
                mProteinTranslator.Add("X", MolecularFormula.Parse("C6 H8 N2 O2", "unknown"));

                this.cmbNterm.Items.Clear();
                this.cmbNterm.Items.Add(MolecularFormula.Parse("", "(none)"));
                this.cmbNterm.Items.Add(MolecularFormula.Parse("H1", "Hydrogen"));
                this.cmbNterm.Items.Add(MolecularFormula.Parse("C2 H3 O1", "N-Acetyl"));
                this.cmbNterm.Items.Add(MolecularFormula.Parse("C1 H1 O1", "N-Formyl"));

                this.cmbCterm.Items.Clear();
                this.cmbCterm.Items.Add(MolecularFormula.Parse("", "(none)"));
                this.cmbCterm.Items.Add(MolecularFormula.Parse("H1 O1", "Free acid"));
                this.cmbCterm.Items.Add(MolecularFormula.Parse("N1 H2", "Amide"));
                
                mDNATranslator =  new MolecularFormulaTranslator();
                mDNATranslator.Add("A", MolecularFormula.Parse("C10 H13 N5 O3", "Aacid"));
                mDNATranslator.Add("G", MolecularFormula.Parse("C10 H13 N5 O4", "Gacid"));
                mDNATranslator.Add("C", MolecularFormula.Parse("C9 H13 N3 O4", "Cytosine"));
                mDNATranslator.Add("T", MolecularFormula.Parse("C10 H14 N2 O5", "Tacid"));
                mDNATranslator.Add("U", MolecularFormula.Parse("C9 H12 N2 O5", "Urasil"));

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
            this.components = new System.ComponentModel.Container();
            var penProvider1 = new PNNL.Controls.PenProvider();
            var penProvider2 = new PNNL.Controls.PenProvider();
            var resources = new System.Resources.ResourceManager(typeof(frmMercury));
            this.mApodizationTypeOptGaussian = new System.Windows.Forms.RadioButton();
            this.mApodizationTypeOptLorentzian = new System.Windows.Forms.RadioButton();
            this.mChargeStateTextBox = new System.Windows.Forms.TextBox();
            this.mTrisphosCheckBox = new System.Windows.Forms.CheckBox();
            this.mCyclicPhosCheckBox = new System.Windows.Forms.CheckBox();
            this.mChargeCarrierMassTextBox = new System.Windows.Forms.TextBox();
            this.Frame5 = new System.Windows.Forms.GroupBox();
            this.cmbCterm = new System.Windows.Forms.ComboBox();
            this.cmbNterm = new System.Windows.Forms.ComboBox();
            this.Label13 = new System.Windows.Forms.Label();
            this.Label12 = new System.Windows.Forms.Label();
            this.mDNARNACombo = new System.Windows.Forms.ComboBox();
            this.mComplementCheckBox = new System.Windows.Forms.CheckBox();
            this.mTermPhosCheckBox = new System.Windows.Forms.CheckBox();
            this.mResolutionTextBox = new System.Windows.Forms.TextBox();
            this.cmbGenerate = new System.Windows.Forms.Button();
            this.mMolecularFormulaTextBox = new System.Windows.Forms.TextBox();
            this.Label8 = new System.Windows.Forms.Label();
            this.mMonoWeightLabel = new System.Windows.Forms.Label();
            this.Label7 = new System.Windows.Forms.Label();
            this.Label4 = new System.Windows.Forms.Label();
            this.mVarianceLabel = new System.Windows.Forms.Label();
            this.mAverageMolecularWeightLabel = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.mPreviewChart = new PNNL.Controls.ctlLineChart();
            this.mMercurySizeCombo = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.mProteinOrDNATab = new System.Windows.Forms.TabControl();
            this.mProteinEditorTab = new System.Windows.Forms.TabPage();
            this.mProteinButtonsPanel = new System.Windows.Forms.Panel();
            this.mProteinButtonLeu = new System.Windows.Forms.Button();
            this.mProteinButtonSer = new System.Windows.Forms.Button();
            this.mProteinButtonMet = new System.Windows.Forms.Button();
            this.mProteinButtonThr = new System.Windows.Forms.Button();
            this.mProteinButtonAla = new System.Windows.Forms.Button();
            this.mProteinButtonLys = new System.Windows.Forms.Button();
            this.mProteinButtonAsnAsp = new System.Windows.Forms.Button();
            this.mProteinButtonPro = new System.Windows.Forms.Button();
            this.mProteinButtonVal = new System.Windows.Forms.Button();
            this.mProteinButtonHis = new System.Windows.Forms.Button();
            this.mProteinButtonAsn = new System.Windows.Forms.Button();
            this.mProteinButtonArg = new System.Windows.Forms.Button();
            this.mProteinButtonCys = new System.Windows.Forms.Button();
            this.mProteinButtonHse = new System.Windows.Forms.Button();
            this.mProteinButtonGlnGsp = new System.Windows.Forms.Button();
            this.mProteinButtonOrn = new System.Windows.Forms.Button();
            this.mProteinButtonAsp = new System.Windows.Forms.Button();
            this.mProteinButtonGlu = new System.Windows.Forms.Button();
            this.mProteinButtonPhe = new System.Windows.Forms.Button();
            this.mProteinButtonGln = new System.Windows.Forms.Button();
            this.UnknownmProteinButton = new System.Windows.Forms.Button();
            this.mProteinButtonTyr = new System.Windows.Forms.Button();
            this.mProteinButtonGly = new System.Windows.Forms.Button();
            this.mProteinButtonTrp = new System.Windows.Forms.Button();
            this.mProteinButtonIle = new System.Windows.Forms.Button();
            this.mDNAEditorTab = new System.Windows.Forms.TabPage();
            this.mDNAButtonPanel = new System.Windows.Forms.Panel();
            this.mDNAButtonU = new System.Windows.Forms.Button();
            this.mDNAButtonT = new System.Windows.Forms.Button();
            this.mDNAButtonG = new System.Windows.Forms.Button();
            this.mDNAButtonC = new System.Windows.Forms.Button();
            this.mDNAButtonA = new System.Windows.Forms.Button();
            this.mProteinDNAEditorTextBox = new System.Windows.Forms.TextBox();
            this.mSettingsExpandPanel = new PNNL.Controls.ExpandPanel(284);
            this.mSettingsInternalPanel = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.mAdvancedSettingsExpandPanel = new PNNL.Controls.ExpandPanel(150);
            this.panel4 = new System.Windows.Forms.Panel();
            this.mcheckBoxAbsolute = new System.Windows.Forms.CheckBox();
            this.mFWHMLabel = new System.Windows.Forms.Label();
            this.mFWHMTextBox = new System.Windows.Forms.TextBox();
            this.mApodizationTypeGroupBox = new System.Windows.Forms.GroupBox();
            this.mSimplifyFormulaButton = new System.Windows.Forms.Button();
            this.mEditorsExpandPanel = new PNNL.Controls.ExpandPanel(164);
            this.panel1 = new System.Windows.Forms.Panel();
            this.mReferenceLabel = new System.Windows.Forms.Label();
            this.mPreviewButton = new System.Windows.Forms.Button();
            this.mAutoPreviewCheckBox = new System.Windows.Forms.CheckBox();
            this.mPreviewExpandPanel = new PNNL.Controls.ExpandPanel(511);
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnCopyToClipboard = new System.Windows.Forms.Button();
            this.mtextBoxMostAbundant = new System.Windows.Forms.TextBox();
            this.labelMaxIsotopeAbundace = new System.Windows.Forms.Label();
            this.mVarianceResultTextBox = new System.Windows.Forms.TextBox();
            this.mMonoMolecularWeightResultTextBox = new System.Windows.Forms.TextBox();
            this.mAverageMolecularWeightResultTextBox = new System.Windows.Forms.TextBox();
            this.mOptsPanel = new System.Windows.Forms.Panel();
            this.mPreviewHoldingPanel = new System.Windows.Forms.Panel();
            this.mVerticalBubbleUpLayout = new PNNL.Controls.VerticalBubbleUpLayout(this.components);
            this.mErrorProvider = new System.Windows.Forms.ErrorProvider();
            this.mWarningProvider = new System.Windows.Forms.ErrorProvider();
            this.mVerticalBubbleUpLayout2 = new PNNL.Controls.VerticalBubbleUpLayout(this.components);
            this.Frame5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mPreviewChart)).BeginInit();
            this.mProteinOrDNATab.SuspendLayout();
            this.mProteinEditorTab.SuspendLayout();
            this.mProteinButtonsPanel.SuspendLayout();
            this.mDNAEditorTab.SuspendLayout();
            this.mDNAButtonPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mSettingsExpandPanel)).BeginInit();
            this.mSettingsExpandPanel.SuspendLayout();
            this.mSettingsInternalPanel.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mAdvancedSettingsExpandPanel)).BeginInit();
            this.mAdvancedSettingsExpandPanel.SuspendLayout();
            this.panel4.SuspendLayout();
            this.mApodizationTypeGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mEditorsExpandPanel)).BeginInit();
            this.mEditorsExpandPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mPreviewExpandPanel)).BeginInit();
            this.mPreviewExpandPanel.SuspendLayout();
            this.panel2.SuspendLayout();
            this.mOptsPanel.SuspendLayout();
            this.mPreviewHoldingPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mApodizationTypeOptGaussian
            // 
            this.mApodizationTypeOptGaussian.Checked = true;
            this.mApodizationTypeOptGaussian.Cursor = System.Windows.Forms.Cursors.Default;
            this.mApodizationTypeOptGaussian.ForeColor = System.Drawing.SystemColors.ControlText;
            this.mApodizationTypeOptGaussian.Location = new System.Drawing.Point(8, 16);
            this.mApodizationTypeOptGaussian.Name = "mApodizationTypeOptGaussian";
            this.mApodizationTypeOptGaussian.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mApodizationTypeOptGaussian.Size = new System.Drawing.Size(72, 18);
            this.mApodizationTypeOptGaussian.TabIndex = 71;
            this.mApodizationTypeOptGaussian.TabStop = true;
            this.mApodizationTypeOptGaussian.Text = "Gaussian";
            this.mApodizationTypeOptGaussian.CheckedChanged += new System.EventHandler(this.mApodizationTypeOptGaussian_CheckedChanged);
            // 
            // mApodizationTypeOptLorentzian
            // 
            this.mApodizationTypeOptLorentzian.Cursor = System.Windows.Forms.Cursors.Default;
            this.mApodizationTypeOptLorentzian.ForeColor = System.Drawing.SystemColors.ControlText;
            this.mApodizationTypeOptLorentzian.Location = new System.Drawing.Point(8, 40);
            this.mApodizationTypeOptLorentzian.Name = "mApodizationTypeOptLorentzian";
            this.mApodizationTypeOptLorentzian.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mApodizationTypeOptLorentzian.Size = new System.Drawing.Size(76, 18);
            this.mApodizationTypeOptLorentzian.TabIndex = 70;
            this.mApodizationTypeOptLorentzian.TabStop = true;
            this.mApodizationTypeOptLorentzian.Text = "Lorentzian";
            this.mApodizationTypeOptLorentzian.CheckedChanged += new System.EventHandler(this.mApodizationTypeOptLorentzian_CheckedChanged);
            // 
            // mChargeStateTextBox
            // 
            this.mChargeStateTextBox.AutoSize = false;
            this.mChargeStateTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.mChargeStateTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.mChargeStateTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.mChargeStateTextBox.Location = new System.Drawing.Point(128, 64);
            this.mChargeStateTextBox.MaxLength = 0;
            this.mChargeStateTextBox.Name = "mChargeStateTextBox";
            this.mChargeStateTextBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mChargeStateTextBox.Size = new System.Drawing.Size(104, 20);
            this.mChargeStateTextBox.TabIndex = 56;
            this.mChargeStateTextBox.Text = "";
            this.mChargeStateTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.mChargeStateTextBox_Validating);
            // 
            // mTrisphosCheckBox
            // 
            this.mTrisphosCheckBox.BackColor = System.Drawing.SystemColors.Control;
            this.mTrisphosCheckBox.Cursor = System.Windows.Forms.Cursors.Default;
            this.mTrisphosCheckBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.mTrisphosCheckBox.Location = new System.Drawing.Point(112, 64);
            this.mTrisphosCheckBox.Name = "mTrisphosCheckBox";
            this.mTrisphosCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mTrisphosCheckBox.Size = new System.Drawing.Size(232, 20);
            this.mTrisphosCheckBox.TabIndex = 68;
            this.mTrisphosCheckBox.Text = "Triphosphate";
            this.mTrisphosCheckBox.CheckedChanged += new System.EventHandler(this.UpdateFormulaFromProteinOrDNAEventHandler2);
            // 
            // mCyclicPhosCheckBox
            // 
            this.mCyclicPhosCheckBox.BackColor = System.Drawing.SystemColors.Control;
            this.mCyclicPhosCheckBox.Cursor = System.Windows.Forms.Cursors.Default;
            this.mCyclicPhosCheckBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.mCyclicPhosCheckBox.Location = new System.Drawing.Point(112, 48);
            this.mCyclicPhosCheckBox.Name = "mCyclicPhosCheckBox";
            this.mCyclicPhosCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mCyclicPhosCheckBox.Size = new System.Drawing.Size(232, 20);
            this.mCyclicPhosCheckBox.TabIndex = 67;
            this.mCyclicPhosCheckBox.Text = "Cyclic Phosphate";
            this.mCyclicPhosCheckBox.CheckedChanged += new System.EventHandler(this.UpdateFormulaFromProteinOrDNAEventHandler2);
            // 
            // mChargeCarrierMassTextBox
            // 
            this.mChargeCarrierMassTextBox.AutoSize = false;
            this.mChargeCarrierMassTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.mChargeCarrierMassTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.mChargeCarrierMassTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.mChargeCarrierMassTextBox.Location = new System.Drawing.Point(128, 92);
            this.mChargeCarrierMassTextBox.MaxLength = 0;
            this.mChargeCarrierMassTextBox.Name = "mChargeCarrierMassTextBox";
            this.mChargeCarrierMassTextBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mChargeCarrierMassTextBox.Size = new System.Drawing.Size(104, 20);
            this.mChargeCarrierMassTextBox.TabIndex = 63;
            this.mChargeCarrierMassTextBox.Text = "";
            this.mChargeCarrierMassTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.mChargeCarrierMassTextBox_Validating);
            // 
            // Frame5
            // 
            this.Frame5.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Frame5.Controls.Add(this.cmbCterm);
            this.Frame5.Controls.Add(this.cmbNterm);
            this.Frame5.Controls.Add(this.Label13);
            this.Frame5.Controls.Add(this.Label12);
            this.Frame5.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Frame5.Location = new System.Drawing.Point(232, 16);
            this.Frame5.Name = "Frame5";
            this.Frame5.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Frame5.Size = new System.Drawing.Size(104, 72);
            this.Frame5.TabIndex = 31;
            this.Frame5.TabStop = false;
            this.Frame5.Text = "Terminal groups";
            // 
            // cmbCterm
            // 
            this.cmbCterm.BackColor = System.Drawing.SystemColors.Window;
            this.cmbCterm.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmbCterm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCterm.ForeColor = System.Drawing.SystemColors.WindowText;
            this.cmbCterm.Location = new System.Drawing.Point(24, 44);
            this.cmbCterm.Name = "cmbCterm";
            this.cmbCterm.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmbCterm.Size = new System.Drawing.Size(76, 21);
            this.cmbCterm.TabIndex = 35;
            this.cmbCterm.SelectedIndexChanged += new System.EventHandler(this.UpdateFormulaFromProteinOrDNAEventHandler2);
            // 
            // cmbNterm
            // 
            this.cmbNterm.BackColor = System.Drawing.SystemColors.Window;
            this.cmbNterm.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmbNterm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNterm.ForeColor = System.Drawing.SystemColors.WindowText;
            this.cmbNterm.Location = new System.Drawing.Point(24, 16);
            this.cmbNterm.Name = "cmbNterm";
            this.cmbNterm.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmbNterm.Size = new System.Drawing.Size(76, 21);
            this.cmbNterm.TabIndex = 32;
            this.cmbNterm.SelectedIndexChanged += new System.EventHandler(this.UpdateFormulaFromProteinOrDNAEventHandler2);
            // 
            // Label13
            // 
            this.Label13.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Label13.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label13.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label13.Location = new System.Drawing.Point(8, 44);
            this.Label13.Name = "Label13";
            this.Label13.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label13.Size = new System.Drawing.Size(9, 17);
            this.Label13.TabIndex = 36;
            this.Label13.Text = "C";
            // 
            // Label12
            // 
            this.Label12.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Label12.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label12.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label12.Location = new System.Drawing.Point(8, 20);
            this.Label12.Name = "Label12";
            this.Label12.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label12.Size = new System.Drawing.Size(9, 17);
            this.Label12.TabIndex = 34;
            this.Label12.Text = "N";
            // 
            // mDNARNACombo
            // 
            this.mDNARNACombo.BackColor = System.Drawing.SystemColors.Window;
            this.mDNARNACombo.Cursor = System.Windows.Forms.Cursors.Default;
            this.mDNARNACombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mDNARNACombo.ForeColor = System.Drawing.SystemColors.WindowText;
            this.mDNARNACombo.Items.AddRange(new object[] {
                                                              "DNA",
                                                              "RNA",
                                                              "Phosphorothioate"});
            this.mDNARNACombo.Location = new System.Drawing.Point(112, 8);
            this.mDNARNACombo.Name = "mDNARNACombo";
            this.mDNARNACombo.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mDNARNACombo.Size = new System.Drawing.Size(109, 21);
            this.mDNARNACombo.TabIndex = 39;
            this.mDNARNACombo.SelectedIndexChanged += new System.EventHandler(this.UpdateFormulaFromProteinOrDNAEventHandler2);
            // 
            // mComplementCheckBox
            // 
            this.mComplementCheckBox.BackColor = System.Drawing.SystemColors.Control;
            this.mComplementCheckBox.Cursor = System.Windows.Forms.Cursors.Default;
            this.mComplementCheckBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.mComplementCheckBox.Location = new System.Drawing.Point(240, 8);
            this.mComplementCheckBox.Name = "mComplementCheckBox";
            this.mComplementCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mComplementCheckBox.TabIndex = 38;
            this.mComplementCheckBox.Text = "Complement";
            this.mComplementCheckBox.CheckedChanged += new System.EventHandler(this.UpdateFormulaFromProteinOrDNAEventHandler2);
            // 
            // mTermPhosCheckBox
            // 
            this.mTermPhosCheckBox.BackColor = System.Drawing.SystemColors.Control;
            this.mTermPhosCheckBox.Cursor = System.Windows.Forms.Cursors.Default;
            this.mTermPhosCheckBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.mTermPhosCheckBox.Location = new System.Drawing.Point(112, 32);
            this.mTermPhosCheckBox.Name = "mTermPhosCheckBox";
            this.mTermPhosCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mTermPhosCheckBox.Size = new System.Drawing.Size(232, 20);
            this.mTermPhosCheckBox.TabIndex = 23;
            this.mTermPhosCheckBox.Text = "Terminal Phosphate";
            this.mTermPhosCheckBox.CheckedChanged += new System.EventHandler(this.UpdateFormulaFromProteinOrDNAEventHandler2);
            // 
            // mResolutionTextBox
            // 
            this.mResolutionTextBox.AutoSize = false;
            this.mResolutionTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.mResolutionTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.mResolutionTextBox.Location = new System.Drawing.Point(128, 8);
            this.mResolutionTextBox.MaxLength = 0;
            this.mResolutionTextBox.Name = "mResolutionTextBox";
            this.mResolutionTextBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mResolutionTextBox.Size = new System.Drawing.Size(104, 20);
            this.mResolutionTextBox.TabIndex = 52;
            this.mResolutionTextBox.Text = "";
            this.mResolutionTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.mResolutionTextBox_Validating);
            this.mResolutionTextBox.Enter += new System.EventHandler(this.mResolutionTextBox_Enter);
            // 
            // cmbGenerate
            // 
            this.cmbGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbGenerate.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmbGenerate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbGenerate.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmbGenerate.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.cmbGenerate.Location = new System.Drawing.Point(376, 214);
            this.cmbGenerate.Name = "cmbGenerate";
            this.cmbGenerate.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmbGenerate.Size = new System.Drawing.Size(336, 24);
            this.cmbGenerate.TabIndex = 48;
            this.cmbGenerate.Text = "Generate to External Scope";
            // 
            // mMolecularFormulaTextBox
            // 
            this.mMolecularFormulaTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.mMolecularFormulaTextBox.AutoSize = false;
            this.mMolecularFormulaTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.mMolecularFormulaTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.mMolecularFormulaTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.mMolecularFormulaTextBox.Location = new System.Drawing.Point(122, 8);
            this.mMolecularFormulaTextBox.MaxLength = 0;
            this.mMolecularFormulaTextBox.Name = "mMolecularFormulaTextBox";
            this.mMolecularFormulaTextBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mMolecularFormulaTextBox.Size = new System.Drawing.Size(182, 20);
            this.mMolecularFormulaTextBox.TabIndex = 46;
            this.mMolecularFormulaTextBox.Text = "C10 H22";
            this.mMolecularFormulaTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.mMolecularFormulaTextBox_Validating);
            // 
            // Label8
            // 
            this.Label8.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label8.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label8.Location = new System.Drawing.Point(8, 92);
            this.Label8.Name = "Label8";
            this.Label8.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label8.Size = new System.Drawing.Size(112, 20);
            this.Label8.TabIndex = 64;
            this.Label8.Text = "Charge carrier mass";
            this.Label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // mMonoWeightLabel
            // 
            this.mMonoWeightLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.mMonoWeightLabel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.mMonoWeightLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.mMonoWeightLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.mMonoWeightLabel.Location = new System.Drawing.Point(8, 462);
            this.mMonoWeightLabel.Name = "mMonoWeightLabel";
            this.mMonoWeightLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mMonoWeightLabel.Size = new System.Drawing.Size(160, 18);
            this.mMonoWeightLabel.TabIndex = 62;
            this.mMonoWeightLabel.Text = "Mono-Isotopic Weight";
            this.mMonoWeightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Label7
            // 
            this.Label7.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label7.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label7.Location = new System.Drawing.Point(8, 64);
            this.Label7.Name = "Label7";
            this.Label7.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label7.Size = new System.Drawing.Size(112, 20);
            this.Label7.TabIndex = 57;
            this.Label7.Text = "Charge state";
            this.Label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Label4
            // 
            this.Label4.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label4.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label4.Location = new System.Drawing.Point(16, 8);
            this.Label4.Name = "Label4";
            this.Label4.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label4.Size = new System.Drawing.Size(104, 20);
            this.Label4.TabIndex = 53;
            this.Label4.Text = "Resolution";
            this.Label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // mVarianceLabel
            // 
            this.mVarianceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.mVarianceLabel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.mVarianceLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.mVarianceLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.mVarianceLabel.Location = new System.Drawing.Point(8, 486);
            this.mVarianceLabel.Name = "mVarianceLabel";
            this.mVarianceLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mVarianceLabel.Size = new System.Drawing.Size(160, 17);
            this.mVarianceLabel.TabIndex = 51;
            this.mVarianceLabel.Text = "Variance";
            this.mVarianceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // mAverageMolecularWeightLabel
            // 
            this.mAverageMolecularWeightLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.mAverageMolecularWeightLabel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.mAverageMolecularWeightLabel.Cursor = System.Windows.Forms.Cursors.Default;
            this.mAverageMolecularWeightLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.mAverageMolecularWeightLabel.Location = new System.Drawing.Point(8, 437);
            this.mAverageMolecularWeightLabel.Name = "mAverageMolecularWeightLabel";
            this.mAverageMolecularWeightLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mAverageMolecularWeightLabel.Size = new System.Drawing.Size(160, 18);
            this.mAverageMolecularWeightLabel.TabIndex = 50;
            this.mAverageMolecularWeightLabel.Text = "Average Molecular Weight";
            this.mAverageMolecularWeightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Label1
            // 
            this.Label1.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label1.Location = new System.Drawing.Point(8, 8);
            this.Label1.Name = "Label1";
            this.Label1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label1.Size = new System.Drawing.Size(104, 24);
            this.Label1.TabIndex = 47;
            this.Label1.Text = "Molecular Formula";
            this.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mPreviewChart
            // 
            this.mPreviewChart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.mPreviewChart.AutoSizeFonts = false;
            this.mPreviewChart.AutoViewPortOnSeriesChange = true;
            this.mPreviewChart.AutoViewPortXBase = 0F;
            this.mPreviewChart.AutoViewPortYAxis = true;
            this.mPreviewChart.AutoViewPortYBase = 0F;
            this.mPreviewChart.AxisAndLabelFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.mPreviewChart.AxisAndLabelMaxFontSize = 15;
            this.mPreviewChart.AxisAndLabelMinFontSize = 8;
            this.mPreviewChart.BackColor = System.Drawing.Color.WhiteSmoke;
            this.mPreviewChart.ChartBackgroundColor = System.Drawing.Color.White;
            this.mPreviewChart.ChartLayout.LegendFraction = 0.2F;
            this.mPreviewChart.ChartLayout.LegendLocation = PNNL.Controls.ChartLegendLocation.Right;
            this.mPreviewChart.ChartLayout.MaxLegendHeight = 150;
            this.mPreviewChart.ChartLayout.MaxLegendWidth = 250;
            this.mPreviewChart.ChartLayout.MaxTitleHeight = 50;
            this.mPreviewChart.ChartLayout.MinLegendHeight = 50;
            this.mPreviewChart.ChartLayout.MinLegendWidth = 75;
            this.mPreviewChart.ChartLayout.MinTitleHeight = 10;
            this.mPreviewChart.ChartLayout.TitleFraction = 0.07F;
            this.mPreviewChart.DefaultZoomHandler.Active = true;
            this.mPreviewChart.DefaultZoomHandler.FillColor = System.Drawing.Color.FromArgb(((System.Byte)(60)), ((System.Byte)(119)), ((System.Byte)(136)), ((System.Byte)(153)));
            this.mPreviewChart.DefaultZoomHandler.LineColor = System.Drawing.Color.Black;
            penProvider1.Color = System.Drawing.Color.FromArgb(((System.Byte)(211)), ((System.Byte)(211)), ((System.Byte)(211)));
            penProvider1.Width = 1F;
            this.mPreviewChart.GridLinePen = penProvider1;
            this.mPreviewChart.HasLegend = false;
            this.mPreviewChart.HilightColor = System.Drawing.Color.Magenta;
            this.mWarningProvider.SetIconAlignment(this.mPreviewChart, System.Windows.Forms.ErrorIconAlignment.TopRight);
            this.mErrorProvider.SetIconAlignment(this.mPreviewChart, System.Windows.Forms.ErrorIconAlignment.TopRight);
            this.mErrorProvider.SetIconPadding(this.mPreviewChart, -16);
            this.mWarningProvider.SetIconPadding(this.mPreviewChart, -16);
            this.mPreviewChart.LabelOffset = 8F;
            this.mPreviewChart.Legend.BackColor = System.Drawing.Color.Transparent;
            penProvider2.Color = System.Drawing.Color.Black;
            penProvider2.Width = 1F;
            this.mPreviewChart.Legend.BorderPen = penProvider2;
            this.mPreviewChart.Legend.Bounds = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.mPreviewChart.Legend.ColumnWidth = 125;
            this.mPreviewChart.Legend.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.mPreviewChart.Legend.MaxFontSize = 12F;
            this.mPreviewChart.Legend.MinFontSize = 6F;
            this.mPreviewChart.Location = new System.Drawing.Point(8, 8);
            this.mPreviewChart.Margins.BottomMarginFraction = 0.25F;
            this.mPreviewChart.Margins.BottomMarginMax = 40;
            this.mPreviewChart.Margins.BottomMarginMin = 10;
            this.mPreviewChart.Margins.DefaultMarginFraction = 0.01F;
            this.mPreviewChart.Margins.DefaultMarginMax = 10;
            this.mPreviewChart.Margins.DefaultMarginMin = 3;
            this.mPreviewChart.Margins.LeftMarginFraction = 0.15F;
            this.mPreviewChart.Margins.LeftMarginMax = 150;
            this.mPreviewChart.Margins.LeftMarginMin = 25;
            this.mPreviewChart.Name = "mPreviewChart";
            this.mPreviewChart.NumXBins = 20;
            this.mPreviewChart.PanWithArrowKeys = false;
            this.mPreviewChart.SeriesPasteEnabled = false;
            this.mPreviewChart.Size = new System.Drawing.Size(704, 411);
            this.mPreviewChart.TabIndex = 72;
            this.mPreviewChart.Title = "Theoretical Profile";
            this.mPreviewChart.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.mPreviewChart.TitleMaxFontSize = 50F;
            this.mPreviewChart.TitleMinFontSize = 6F;
            this.mPreviewChart.UseAutoViewPortYBase = true;
            this.mPreviewChart.VerticalExpansion = 1.15F;
            this.mPreviewChart.ViewPort = ((System.Drawing.RectangleF)(resources.GetObject("mPreviewChart.ViewPort")));
            this.mPreviewChart.XAxisLabel = "m/z";
            this.mPreviewChart.YAxisLabel = "% of Highest Peak";
            // 
            // mMercurySizeCombo
            // 
            this.mMercurySizeCombo.Location = new System.Drawing.Point(128, 120);
            this.mMercurySizeCombo.Name = "mMercurySizeCombo";
            this.mMercurySizeCombo.Size = new System.Drawing.Size(104, 21);
            this.mMercurySizeCombo.TabIndex = 76;
            this.mMercurySizeCombo.SelectedIndexChanged += new System.EventHandler(this.mMercurySizeCombo_SelectedIndexChanged);
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(8, 120);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(104, 20);
            this.label9.TabIndex = 77;
            this.label9.Text = "Mercury Size";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // mProteinOrDNATab
            // 
            this.mProteinOrDNATab.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.mProteinOrDNATab.Controls.Add(this.mProteinEditorTab);
            this.mProteinOrDNATab.Controls.Add(this.mDNAEditorTab);
            this.mProteinOrDNATab.Location = new System.Drawing.Point(8, 36);
            this.mProteinOrDNATab.Name = "mProteinOrDNATab";
            this.mProteinOrDNATab.SelectedIndex = 0;
            this.mProteinOrDNATab.Size = new System.Drawing.Size(344, 122);
            this.mProteinOrDNATab.TabIndex = 75;
            this.mProteinOrDNATab.SelectedIndexChanged += new System.EventHandler(this.UpdateFormulaFromProteinOrDNAEventHandler);
            // 
            // mProteinEditorTab
            // 
            this.mProteinEditorTab.BackColor = System.Drawing.Color.WhiteSmoke;
            this.mProteinEditorTab.Controls.Add(this.mProteinButtonsPanel);
            this.mProteinEditorTab.Controls.Add(this.Frame5);
            this.mProteinEditorTab.Location = new System.Drawing.Point(4, 22);
            this.mProteinEditorTab.Name = "mProteinEditorTab";
            this.mProteinEditorTab.Size = new System.Drawing.Size(336, 96);
            this.mProteinEditorTab.TabIndex = 0;
            this.mProteinEditorTab.Text = "Protein";
            // 
            // mProteinButtonsPanel
            // 
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonLeu);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonSer);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonMet);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonThr);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonAla);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonLys);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonAsnAsp);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonPro);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonVal);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonHis);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonAsn);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonArg);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonCys);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonHse);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonGlnGsp);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonOrn);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonAsp);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonGlu);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonPhe);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonGln);
            this.mProteinButtonsPanel.Controls.Add(this.UnknownmProteinButton);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonTyr);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonGly);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonTrp);
            this.mProteinButtonsPanel.Controls.Add(this.mProteinButtonIle);
            this.mProteinButtonsPanel.Location = new System.Drawing.Point(0, 0);
            this.mProteinButtonsPanel.Name = "mProteinButtonsPanel";
            this.mProteinButtonsPanel.Size = new System.Drawing.Size(224, 96);
            this.mProteinButtonsPanel.TabIndex = 61;
            // 
            // mProteinButtonLeu
            // 
            this.mProteinButtonLeu.Location = new System.Drawing.Point(96, 24);
            this.mProteinButtonLeu.Name = "mProteinButtonLeu";
            this.mProteinButtonLeu.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonLeu.TabIndex = 45;
            this.mProteinButtonLeu.Tag = "L";
            this.mProteinButtonLeu.Text = "Leu";
            // 
            // mProteinButtonSer
            // 
            this.mProteinButtonSer.Location = new System.Drawing.Point(32, 48);
            this.mProteinButtonSer.Name = "mProteinButtonSer";
            this.mProteinButtonSer.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonSer.TabIndex = 49;
            this.mProteinButtonSer.Tag = "S";
            this.mProteinButtonSer.Text = "Ser";
            // 
            // mProteinButtonMet
            // 
            this.mProteinButtonMet.Location = new System.Drawing.Point(160, 24);
            this.mProteinButtonMet.Name = "mProteinButtonMet";
            this.mProteinButtonMet.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonMet.TabIndex = 47;
            this.mProteinButtonMet.Tag = "M";
            this.mProteinButtonMet.Text = "Met";
            // 
            // mProteinButtonThr
            // 
            this.mProteinButtonThr.Location = new System.Drawing.Point(64, 48);
            this.mProteinButtonThr.Name = "mProteinButtonThr";
            this.mProteinButtonThr.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonThr.TabIndex = 50;
            this.mProteinButtonThr.Tag = "T";
            this.mProteinButtonThr.Text = "Thr";
            // 
            // mProteinButtonAla
            // 
            this.mProteinButtonAla.Location = new System.Drawing.Point(0, 0);
            this.mProteinButtonAla.Name = "mProteinButtonAla";
            this.mProteinButtonAla.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonAla.TabIndex = 32;
            this.mProteinButtonAla.Tag = "A";
            this.mProteinButtonAla.Text = "Ala";
            // 
            // mProteinButtonLys
            // 
            this.mProteinButtonLys.Location = new System.Drawing.Point(128, 24);
            this.mProteinButtonLys.Name = "mProteinButtonLys";
            this.mProteinButtonLys.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonLys.TabIndex = 46;
            this.mProteinButtonLys.Tag = "K";
            this.mProteinButtonLys.Text = "Lys";
            // 
            // mProteinButtonAsnAsp
            // 
            this.mProteinButtonAsnAsp.Location = new System.Drawing.Point(32, 72);
            this.mProteinButtonAsnAsp.Name = "mProteinButtonAsnAsp";
            this.mProteinButtonAsnAsp.Size = new System.Drawing.Size(64, 24);
            this.mProteinButtonAsnAsp.TabIndex = 58;
            this.mProteinButtonAsnAsp.Tag = "Z";
            this.mProteinButtonAsnAsp.Text = "Asn/Asp";
            // 
            // mProteinButtonPro
            // 
            this.mProteinButtonPro.Location = new System.Drawing.Point(0, 48);
            this.mProteinButtonPro.Name = "mProteinButtonPro";
            this.mProteinButtonPro.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonPro.TabIndex = 48;
            this.mProteinButtonPro.Tag = "P";
            this.mProteinButtonPro.Text = "Pro";
            // 
            // mProteinButtonVal
            // 
            this.mProteinButtonVal.Location = new System.Drawing.Point(160, 48);
            this.mProteinButtonVal.Name = "mProteinButtonVal";
            this.mProteinButtonVal.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonVal.TabIndex = 53;
            this.mProteinButtonVal.Tag = "V";
            this.mProteinButtonVal.Text = "Val";
            // 
            // mProteinButtonHis
            // 
            this.mProteinButtonHis.Location = new System.Drawing.Point(32, 24);
            this.mProteinButtonHis.Name = "mProteinButtonHis";
            this.mProteinButtonHis.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonHis.TabIndex = 43;
            this.mProteinButtonHis.Tag = "H";
            this.mProteinButtonHis.Text = "His";
            // 
            // mProteinButtonAsn
            // 
            this.mProteinButtonAsn.Location = new System.Drawing.Point(64, 0);
            this.mProteinButtonAsn.Name = "mProteinButtonAsn";
            this.mProteinButtonAsn.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonAsn.TabIndex = 35;
            this.mProteinButtonAsn.Tag = "N";
            this.mProteinButtonAsn.Text = "Asn";
            // 
            // mProteinButtonArg
            // 
            this.mProteinButtonArg.Location = new System.Drawing.Point(32, 0);
            this.mProteinButtonArg.Name = "mProteinButtonArg";
            this.mProteinButtonArg.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonArg.TabIndex = 34;
            this.mProteinButtonArg.Tag = "R";
            this.mProteinButtonArg.Text = "Arg";
            // 
            // mProteinButtonCys
            // 
            this.mProteinButtonCys.Location = new System.Drawing.Point(128, 0);
            this.mProteinButtonCys.Name = "mProteinButtonCys";
            this.mProteinButtonCys.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonCys.TabIndex = 37;
            this.mProteinButtonCys.Tag = "C";
            this.mProteinButtonCys.Text = "Cys";
            // 
            // mProteinButtonHse
            // 
            this.mProteinButtonHse.Location = new System.Drawing.Point(0, 72);
            this.mProteinButtonHse.Name = "mProteinButtonHse";
            this.mProteinButtonHse.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonHse.TabIndex = 57;
            this.mProteinButtonHse.Tag = "U";
            this.mProteinButtonHse.Text = "Hse";
            // 
            // mProteinButtonGlnGsp
            // 
            this.mProteinButtonGlnGsp.Location = new System.Drawing.Point(96, 72);
            this.mProteinButtonGlnGsp.Name = "mProteinButtonGlnGsp";
            this.mProteinButtonGlnGsp.Size = new System.Drawing.Size(64, 24);
            this.mProteinButtonGlnGsp.TabIndex = 59;
            this.mProteinButtonGlnGsp.Tag = "B";
            this.mProteinButtonGlnGsp.Text = "Gln/Glu";
            // 
            // mProteinButtonOrn
            // 
            this.mProteinButtonOrn.Location = new System.Drawing.Point(192, 48);
            this.mProteinButtonOrn.Name = "mProteinButtonOrn";
            this.mProteinButtonOrn.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonOrn.TabIndex = 56;
            this.mProteinButtonOrn.Tag = "O";
            this.mProteinButtonOrn.Text = "Orn";
            // 
            // mProteinButtonAsp
            // 
            this.mProteinButtonAsp.Location = new System.Drawing.Point(96, 0);
            this.mProteinButtonAsp.Name = "mProteinButtonAsp";
            this.mProteinButtonAsp.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonAsp.TabIndex = 36;
            this.mProteinButtonAsp.Tag = "D";
            this.mProteinButtonAsp.Text = "Asp";
            // 
            // mProteinButtonGlu
            // 
            this.mProteinButtonGlu.Location = new System.Drawing.Point(192, 0);
            this.mProteinButtonGlu.Name = "mProteinButtonGlu";
            this.mProteinButtonGlu.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonGlu.TabIndex = 54;
            this.mProteinButtonGlu.Tag = "E";
            this.mProteinButtonGlu.Text = "Glu";
            // 
            // mProteinButtonPhe
            // 
            this.mProteinButtonPhe.Location = new System.Drawing.Point(192, 24);
            this.mProteinButtonPhe.Name = "mProteinButtonPhe";
            this.mProteinButtonPhe.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonPhe.TabIndex = 55;
            this.mProteinButtonPhe.Tag = "F";
            this.mProteinButtonPhe.Text = "Phe";
            // 
            // mProteinButtonGln
            // 
            this.mProteinButtonGln.Location = new System.Drawing.Point(160, 0);
            this.mProteinButtonGln.Name = "mProteinButtonGln";
            this.mProteinButtonGln.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonGln.TabIndex = 41;
            this.mProteinButtonGln.Tag = "Q";
            this.mProteinButtonGln.Text = "Gln";
            // 
            // UnknownmProteinButton
            // 
            this.UnknownmProteinButton.Location = new System.Drawing.Point(160, 72);
            this.UnknownmProteinButton.Name = "UnknownmProteinButton";
            this.UnknownmProteinButton.Size = new System.Drawing.Size(64, 24);
            this.UnknownmProteinButton.TabIndex = 60;
            this.UnknownmProteinButton.Tag = "X";
            this.UnknownmProteinButton.Text = "Unknown";
            // 
            // mProteinButtonTyr
            // 
            this.mProteinButtonTyr.Location = new System.Drawing.Point(128, 48);
            this.mProteinButtonTyr.Name = "mProteinButtonTyr";
            this.mProteinButtonTyr.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonTyr.TabIndex = 52;
            this.mProteinButtonTyr.Tag = "Y";
            this.mProteinButtonTyr.Text = "Tyr";
            // 
            // mProteinButtonGly
            // 
            this.mProteinButtonGly.Location = new System.Drawing.Point(0, 24);
            this.mProteinButtonGly.Name = "mProteinButtonGly";
            this.mProteinButtonGly.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonGly.TabIndex = 42;
            this.mProteinButtonGly.Tag = "G";
            this.mProteinButtonGly.Text = "Gly";
            // 
            // mProteinButtonTrp
            // 
            this.mProteinButtonTrp.Location = new System.Drawing.Point(96, 48);
            this.mProteinButtonTrp.Name = "mProteinButtonTrp";
            this.mProteinButtonTrp.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonTrp.TabIndex = 51;
            this.mProteinButtonTrp.Tag = "W";
            this.mProteinButtonTrp.Text = "Trp";
            // 
            // mProteinButtonIle
            // 
            this.mProteinButtonIle.Location = new System.Drawing.Point(64, 24);
            this.mProteinButtonIle.Name = "mProteinButtonIle";
            this.mProteinButtonIle.Size = new System.Drawing.Size(32, 24);
            this.mProteinButtonIle.TabIndex = 44;
            this.mProteinButtonIle.Tag = "I";
            this.mProteinButtonIle.Text = "Ile";
            // 
            // mDNAEditorTab
            // 
            this.mDNAEditorTab.BackColor = System.Drawing.SystemColors.Control;
            this.mDNAEditorTab.Controls.Add(this.mDNAButtonPanel);
            this.mDNAEditorTab.Controls.Add(this.mCyclicPhosCheckBox);
            this.mDNAEditorTab.Controls.Add(this.mTrisphosCheckBox);
            this.mDNAEditorTab.Controls.Add(this.mDNARNACombo);
            this.mDNAEditorTab.Controls.Add(this.mComplementCheckBox);
            this.mDNAEditorTab.Controls.Add(this.mTermPhosCheckBox);
            this.mDNAEditorTab.Location = new System.Drawing.Point(4, 22);
            this.mDNAEditorTab.Name = "mDNAEditorTab";
            this.mDNAEditorTab.Size = new System.Drawing.Size(336, 96);
            this.mDNAEditorTab.TabIndex = 1;
            this.mDNAEditorTab.Text = "DNA";
            // 
            // mDNAButtonPanel
            // 
            this.mDNAButtonPanel.Controls.Add(this.mDNAButtonU);
            this.mDNAButtonPanel.Controls.Add(this.mDNAButtonT);
            this.mDNAButtonPanel.Controls.Add(this.mDNAButtonG);
            this.mDNAButtonPanel.Controls.Add(this.mDNAButtonC);
            this.mDNAButtonPanel.Controls.Add(this.mDNAButtonA);
            this.mDNAButtonPanel.Location = new System.Drawing.Point(12, 12);
            this.mDNAButtonPanel.Name = "mDNAButtonPanel";
            this.mDNAButtonPanel.Size = new System.Drawing.Size(80, 72);
            this.mDNAButtonPanel.TabIndex = 69;
            // 
            // mDNAButtonU
            // 
            this.mDNAButtonU.Location = new System.Drawing.Point(0, 48);
            this.mDNAButtonU.Name = "mDNAButtonU";
            this.mDNAButtonU.Size = new System.Drawing.Size(80, 23);
            this.mDNAButtonU.TabIndex = 4;
            this.mDNAButtonU.Tag = "U";
            this.mDNAButtonU.Text = "U";
            // 
            // mDNAButtonT
            // 
            this.mDNAButtonT.Location = new System.Drawing.Point(40, 24);
            this.mDNAButtonT.Name = "mDNAButtonT";
            this.mDNAButtonT.Size = new System.Drawing.Size(40, 23);
            this.mDNAButtonT.TabIndex = 3;
            this.mDNAButtonT.Tag = "T";
            this.mDNAButtonT.Text = "T";
            // 
            // mDNAButtonG
            // 
            this.mDNAButtonG.Location = new System.Drawing.Point(0, 24);
            this.mDNAButtonG.Name = "mDNAButtonG";
            this.mDNAButtonG.Size = new System.Drawing.Size(40, 23);
            this.mDNAButtonG.TabIndex = 2;
            this.mDNAButtonG.Tag = "G";
            this.mDNAButtonG.Text = "G";
            // 
            // mDNAButtonC
            // 
            this.mDNAButtonC.Location = new System.Drawing.Point(40, 0);
            this.mDNAButtonC.Name = "mDNAButtonC";
            this.mDNAButtonC.Size = new System.Drawing.Size(40, 23);
            this.mDNAButtonC.TabIndex = 1;
            this.mDNAButtonC.Tag = "C";
            this.mDNAButtonC.Text = "C";
            // 
            // mDNAButtonA
            // 
            this.mDNAButtonA.Location = new System.Drawing.Point(0, 0);
            this.mDNAButtonA.Name = "mDNAButtonA";
            this.mDNAButtonA.Size = new System.Drawing.Size(40, 23);
            this.mDNAButtonA.TabIndex = 0;
            this.mDNAButtonA.Tag = "A";
            this.mDNAButtonA.Text = "A";
            // 
            // mProteinDNAEditorTextBox
            // 
            this.mProteinDNAEditorTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.mProteinDNAEditorTextBox.Location = new System.Drawing.Point(8, 8);
            this.mProteinDNAEditorTextBox.Name = "mProteinDNAEditorTextBox";
            this.mProteinDNAEditorTextBox.Size = new System.Drawing.Size(344, 20);
            this.mProteinDNAEditorTextBox.TabIndex = 86;
            this.mProteinDNAEditorTextBox.Text = "";
            this.mProteinDNAEditorTextBox.TextChanged += new System.EventHandler(this.UpdateFormulaFromProteinOrDNAEventHandler);
            // 
            // mSettingsExpandPanel
            // 
            this.mSettingsExpandPanel.Controls.Add(this.mSettingsInternalPanel);
            this.mSettingsExpandPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.mSettingsExpandPanel.ExpandImage = ((System.Drawing.Image)(resources.GetObject("mSettingsExpandPanel.ExpandImage")));
            this.mSettingsExpandPanel.ExpandTime = 200;
            this.mSettingsExpandPanel.HeaderForeColor = System.Drawing.Color.Navy;
            this.mSettingsExpandPanel.HeaderRightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mSettingsExpandPanel.HeaderText = "Settings";
            this.mSettingsExpandPanel.HeaderTextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.mWarningProvider.SetIconAlignment(this.mSettingsExpandPanel, System.Windows.Forms.ErrorIconAlignment.TopRight);
            this.mErrorProvider.SetIconAlignment(this.mSettingsExpandPanel, System.Windows.Forms.ErrorIconAlignment.TopRight);
            this.mWarningProvider.SetIconPadding(this.mSettingsExpandPanel, -16);
            this.mErrorProvider.SetIconPadding(this.mSettingsExpandPanel, -16);
            this.mSettingsExpandPanel.Location = new System.Drawing.Point(8, 8);
            this.mSettingsExpandPanel.Name = "mSettingsExpandPanel";
            this.mSettingsExpandPanel.Size = new System.Drawing.Size(720, 304);
            this.mSettingsExpandPanel.TabIndex = 73;
            // 
            // mSettingsInternalPanel
            // 
            this.mSettingsInternalPanel.Controls.Add(this.panel3);
            this.mSettingsInternalPanel.Controls.Add(this.mMolecularFormulaTextBox);
            this.mSettingsInternalPanel.Controls.Add(this.mSimplifyFormulaButton);
            this.mSettingsInternalPanel.Controls.Add(this.Label1);
            this.mSettingsInternalPanel.Controls.Add(this.mEditorsExpandPanel);
            this.mSettingsInternalPanel.Controls.Add(this.mReferenceLabel);
            this.mSettingsInternalPanel.Controls.Add(this.cmbGenerate);
            this.mSettingsInternalPanel.Controls.Add(this.mPreviewButton);
            this.mSettingsInternalPanel.Controls.Add(this.mAutoPreviewCheckBox);
            this.mSettingsInternalPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mSettingsInternalPanel.Location = new System.Drawing.Point(1, 20);
            this.mSettingsInternalPanel.Name = "mSettingsInternalPanel";
            this.mSettingsInternalPanel.Size = new System.Drawing.Size(718, 283);
            this.mSettingsInternalPanel.TabIndex = 92;
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.Controls.Add(this.mAdvancedSettingsExpandPanel);
            this.panel3.Location = new System.Drawing.Point(376, 8);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(336, 200);
            this.panel3.TabIndex = 98;
            // 
            // mAdvancedSettingsExpandPanel
            // 
            this.mAdvancedSettingsExpandPanel.Controls.Add(this.panel4);
            this.mAdvancedSettingsExpandPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.mAdvancedSettingsExpandPanel.ExpandImage = ((System.Drawing.Image)(resources.GetObject("mAdvancedSettingsExpandPanel.ExpandImage")));
            this.mAdvancedSettingsExpandPanel.ExpandTime = 200;
            this.mAdvancedSettingsExpandPanel.HeaderForeColor = System.Drawing.Color.Navy;
            this.mAdvancedSettingsExpandPanel.HeaderRightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mAdvancedSettingsExpandPanel.HeaderText = "Advanced Settings";
            this.mAdvancedSettingsExpandPanel.HeaderTextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.mAdvancedSettingsExpandPanel.Location = new System.Drawing.Point(0, 0);
            this.mAdvancedSettingsExpandPanel.Name = "mAdvancedSettingsExpandPanel";
            this.mAdvancedSettingsExpandPanel.Size = new System.Drawing.Size(336, 170);
            this.mAdvancedSettingsExpandPanel.TabIndex = 93;
            // 
            // panel4
            // 
            this.mVerticalBubbleUpLayout.SetBubbleUp(this.panel4, true);
            this.panel4.Controls.Add(this.mcheckBoxAbsolute);
            this.panel4.Controls.Add(this.mResolutionTextBox);
            this.panel4.Controls.Add(this.Label4);
            this.panel4.Controls.Add(this.mFWHMLabel);
            this.panel4.Controls.Add(this.mFWHMTextBox);
            this.panel4.Controls.Add(this.Label7);
            this.panel4.Controls.Add(this.mChargeStateTextBox);
            this.panel4.Controls.Add(this.mChargeCarrierMassTextBox);
            this.panel4.Controls.Add(this.mMercurySizeCombo);
            this.panel4.Controls.Add(this.Label8);
            this.panel4.Controls.Add(this.label9);
            this.panel4.Controls.Add(this.mApodizationTypeGroupBox);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(1, 20);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(334, 149);
            this.panel4.TabIndex = 94;
            // 
            // mcheckBoxAbsolute
            // 
            this.mcheckBoxAbsolute.Location = new System.Drawing.Point(240, 8);
            this.mcheckBoxAbsolute.Name = "mcheckBoxAbsolute";
            this.mcheckBoxAbsolute.Size = new System.Drawing.Size(88, 24);
            this.mcheckBoxAbsolute.TabIndex = 81;
            this.mcheckBoxAbsolute.Text = "Absolute";
            // 
            // mFWHMLabel
            // 
            this.mFWHMLabel.Location = new System.Drawing.Point(8, 36);
            this.mFWHMLabel.Name = "mFWHMLabel";
            this.mFWHMLabel.Size = new System.Drawing.Size(104, 20);
            this.mFWHMLabel.TabIndex = 80;
            this.mFWHMLabel.Text = "FWHM";
            this.mFWHMLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // mFWHMTextBox
            // 
            this.mFWHMTextBox.BackColor = System.Drawing.Color.White;
            this.mFWHMTextBox.Location = new System.Drawing.Point(128, 36);
            this.mFWHMTextBox.Name = "mFWHMTextBox";
            this.mFWHMTextBox.ReadOnly = true;
            this.mFWHMTextBox.Size = new System.Drawing.Size(104, 20);
            this.mFWHMTextBox.TabIndex = 79;
            this.mFWHMTextBox.Text = "";
            this.mFWHMTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.mFWHMTextBox_Validating);
            this.mFWHMTextBox.Enter += new System.EventHandler(this.mFWHMTextBox_Enter);
            // 
            // mApodizationTypeGroupBox
            // 
            this.mApodizationTypeGroupBox.Controls.Add(this.mApodizationTypeOptGaussian);
            this.mApodizationTypeGroupBox.Controls.Add(this.mApodizationTypeOptLorentzian);
            this.mApodizationTypeGroupBox.Location = new System.Drawing.Point(240, 40);
            this.mApodizationTypeGroupBox.Name = "mApodizationTypeGroupBox";
            this.mApodizationTypeGroupBox.Size = new System.Drawing.Size(88, 64);
            this.mApodizationTypeGroupBox.TabIndex = 78;
            this.mApodizationTypeGroupBox.TabStop = false;
            this.mApodizationTypeGroupBox.Text = "Apodization";
            // 
            // mSimplifyFormulaButton
            // 
            this.mSimplifyFormulaButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mSimplifyFormulaButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.mSimplifyFormulaButton.Location = new System.Drawing.Point(312, 8);
            this.mSimplifyFormulaButton.Name = "mSimplifyFormulaButton";
            this.mSimplifyFormulaButton.Size = new System.Drawing.Size(58, 20);
            this.mSimplifyFormulaButton.TabIndex = 95;
            this.mSimplifyFormulaButton.Text = "Simplify";
            this.mSimplifyFormulaButton.Click += new System.EventHandler(this.mSimplifyFormulaButton_Click);
            // 
            // mEditorsExpandPanel
            // 
            this.mEditorsExpandPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.mEditorsExpandPanel.Controls.Add(this.panel1);
            this.mEditorsExpandPanel.ExpandImage = ((System.Drawing.Image)(resources.GetObject("mEditorsExpandPanel.ExpandImage")));
            this.mEditorsExpandPanel.ExpandImageWidth = 18;
            this.mEditorsExpandPanel.ExpandTime = 200;
            this.mEditorsExpandPanel.HeaderForeColor = System.Drawing.Color.Navy;
            this.mEditorsExpandPanel.HeaderRightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mEditorsExpandPanel.HeaderText = "Protein/DNA Editor";
            this.mEditorsExpandPanel.HeaderTextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.mWarningProvider.SetIconAlignment(this.mEditorsExpandPanel, System.Windows.Forms.ErrorIconAlignment.TopRight);
            this.mErrorProvider.SetIconAlignment(this.mEditorsExpandPanel, System.Windows.Forms.ErrorIconAlignment.TopRight);
            this.mWarningProvider.SetIconPadding(this.mEditorsExpandPanel, -16);
            this.mErrorProvider.SetIconPadding(this.mEditorsExpandPanel, -16);
            this.mEditorsExpandPanel.Location = new System.Drawing.Point(8, 40);
            this.mEditorsExpandPanel.Name = "mEditorsExpandPanel";
            this.mEditorsExpandPanel.Size = new System.Drawing.Size(360, 184);
            this.mEditorsExpandPanel.TabIndex = 89;
            // 
            // panel1
            // 
            this.mVerticalBubbleUpLayout.SetBubbleUp(this.panel1, true);
            this.panel1.Controls.Add(this.mProteinDNAEditorTextBox);
            this.panel1.Controls.Add(this.mProteinOrDNATab);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(1, 20);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(358, 163);
            this.panel1.TabIndex = 87;
            // 
            // mReferenceLabel
            // 
            this.mReferenceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.mReferenceLabel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.mReferenceLabel.Location = new System.Drawing.Point(8, 232);
            this.mReferenceLabel.Name = "mReferenceLabel";
            this.mReferenceLabel.Size = new System.Drawing.Size(360, 52);
            this.mReferenceLabel.TabIndex = 96;
            this.mReferenceLabel.Text = "Reference:\nRockwood, A.L., Van Orden, S.L. Smith, R.D.\n\"Rapid Calculation of Isot" +
                "ope Distributions\". Analytical Chemistry. Vol. 67, No. 15. August 1, 1995";
            // 
            // mPreviewButton
            // 
            this.mPreviewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mPreviewButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.mPreviewButton.Location = new System.Drawing.Point(488, 246);
            this.mPreviewButton.Name = "mPreviewButton";
            this.mPreviewButton.Size = new System.Drawing.Size(120, 24);
            this.mPreviewButton.TabIndex = 92;
            this.mPreviewButton.Text = "Update Preview";
            this.mPreviewButton.Click += new System.EventHandler(this.mPreviewButton_Click);
            // 
            // mAutoPreviewCheckBox
            // 
            this.mAutoPreviewCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mAutoPreviewCheckBox.Checked = true;
            this.mAutoPreviewCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mAutoPreviewCheckBox.Location = new System.Drawing.Point(384, 246);
            this.mAutoPreviewCheckBox.Name = "mAutoPreviewCheckBox";
            this.mAutoPreviewCheckBox.TabIndex = 78;
            this.mAutoPreviewCheckBox.Text = "Auto Preview";
            this.mAutoPreviewCheckBox.CheckedChanged += new System.EventHandler(this.mAutoPreviewCheckBox_CheckedChanged);
            // 
            // mPreviewExpandPanel
            // 
            this.mPreviewExpandPanel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.mPreviewExpandPanel.Controls.Add(this.panel2);
            this.mPreviewExpandPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mPreviewExpandPanel.ExpandImage = ((System.Drawing.Image)(resources.GetObject("mPreviewExpandPanel.ExpandImage")));
            this.mPreviewExpandPanel.ExpandTime = 200;
            this.mPreviewExpandPanel.HeaderForeColor = System.Drawing.Color.Navy;
            this.mPreviewExpandPanel.HeaderRightToLeft = System.Windows.Forms.RightToLeft.No;
            this.mPreviewExpandPanel.HeaderText = "Preview ";
            this.mPreviewExpandPanel.HeaderTextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.mErrorProvider.SetIconAlignment(this.mPreviewExpandPanel, System.Windows.Forms.ErrorIconAlignment.TopRight);
            this.mWarningProvider.SetIconAlignment(this.mPreviewExpandPanel, System.Windows.Forms.ErrorIconAlignment.TopRight);
            this.mWarningProvider.SetIconPadding(this.mPreviewExpandPanel, -16);
            this.mErrorProvider.SetIconPadding(this.mPreviewExpandPanel, -16);
            this.mPreviewExpandPanel.Location = new System.Drawing.Point(0, 8);
            this.mPreviewExpandPanel.Name = "mPreviewExpandPanel";
            this.mPreviewExpandPanel.Size = new System.Drawing.Size(720, 531);
            this.mPreviewExpandPanel.TabIndex = 90;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnCopyToClipboard);
            this.panel2.Controls.Add(this.mtextBoxMostAbundant);
            this.panel2.Controls.Add(this.labelMaxIsotopeAbundace);
            this.panel2.Controls.Add(this.mVarianceResultTextBox);
            this.panel2.Controls.Add(this.mMonoMolecularWeightResultTextBox);
            this.panel2.Controls.Add(this.mAverageMolecularWeightResultTextBox);
            this.panel2.Controls.Add(this.mPreviewChart);
            this.panel2.Controls.Add(this.mMonoWeightLabel);
            this.panel2.Controls.Add(this.mVarianceLabel);
            this.panel2.Controls.Add(this.mAverageMolecularWeightLabel);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(1, 20);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(718, 510);
            this.panel2.TabIndex = 92;
            // 
            // btnCopyToClipboard
            // 
            this.btnCopyToClipboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCopyToClipboard.Location = new System.Drawing.Point(3, 5);
            this.btnCopyToClipboard.Name = "btnCopyToClipboard";
            this.btnCopyToClipboard.Size = new System.Drawing.Size(75, 32);
            this.btnCopyToClipboard.TabIndex = 80;
            this.btnCopyToClipboard.Text = "Copy XY values";
            this.btnCopyToClipboard.Click += new System.EventHandler(this.btnCopyToClipboard_Click);
            // 
            // mtextBoxMostAbundant
            // 
            this.mtextBoxMostAbundant.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.mtextBoxMostAbundant.BackColor = System.Drawing.Color.Gainsboro;
            this.mtextBoxMostAbundant.Location = new System.Drawing.Point(552, 486);
            this.mtextBoxMostAbundant.Name = "mtextBoxMostAbundant";
            this.mtextBoxMostAbundant.ReadOnly = true;
            this.mtextBoxMostAbundant.Size = new System.Drawing.Size(104, 20);
            this.mtextBoxMostAbundant.TabIndex = 79;
            this.mtextBoxMostAbundant.Text = "";
            // 
            // labelMaxIsotopeAbundace
            // 
            this.labelMaxIsotopeAbundace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelMaxIsotopeAbundace.BackColor = System.Drawing.Color.WhiteSmoke;
            this.labelMaxIsotopeAbundace.Cursor = System.Windows.Forms.Cursors.Default;
            this.labelMaxIsotopeAbundace.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labelMaxIsotopeAbundace.Location = new System.Drawing.Point(296, 486);
            this.labelMaxIsotopeAbundace.Name = "labelMaxIsotopeAbundace";
            this.labelMaxIsotopeAbundace.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.labelMaxIsotopeAbundace.Size = new System.Drawing.Size(240, 17);
            this.labelMaxIsotopeAbundace.TabIndex = 78;
            this.labelMaxIsotopeAbundace.Text = "Relative Height of Most Abundant Isotope";
            this.labelMaxIsotopeAbundace.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // mVarianceResultTextBox
            // 
            this.mVarianceResultTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.mVarianceResultTextBox.BackColor = System.Drawing.Color.Gainsboro;
            this.mVarianceResultTextBox.Location = new System.Drawing.Point(176, 485);
            this.mVarianceResultTextBox.Name = "mVarianceResultTextBox";
            this.mVarianceResultTextBox.ReadOnly = true;
            this.mVarianceResultTextBox.Size = new System.Drawing.Size(104, 20);
            this.mVarianceResultTextBox.TabIndex = 77;
            this.mVarianceResultTextBox.Text = "";
            // 
            // mMonoMolecularWeightResultTextBox
            // 
            this.mMonoMolecularWeightResultTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.mMonoMolecularWeightResultTextBox.BackColor = System.Drawing.Color.Gainsboro;
            this.mMonoMolecularWeightResultTextBox.Location = new System.Drawing.Point(176, 461);
            this.mMonoMolecularWeightResultTextBox.Name = "mMonoMolecularWeightResultTextBox";
            this.mMonoMolecularWeightResultTextBox.ReadOnly = true;
            this.mMonoMolecularWeightResultTextBox.Size = new System.Drawing.Size(536, 20);
            this.mMonoMolecularWeightResultTextBox.TabIndex = 76;
            this.mMonoMolecularWeightResultTextBox.Text = "";
            // 
            // mAverageMolecularWeightResultTextBox
            // 
            this.mAverageMolecularWeightResultTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.mAverageMolecularWeightResultTextBox.BackColor = System.Drawing.Color.Gainsboro;
            this.mAverageMolecularWeightResultTextBox.Location = new System.Drawing.Point(176, 437);
            this.mAverageMolecularWeightResultTextBox.Name = "mAverageMolecularWeightResultTextBox";
            this.mAverageMolecularWeightResultTextBox.ReadOnly = true;
            this.mAverageMolecularWeightResultTextBox.Size = new System.Drawing.Size(536, 20);
            this.mAverageMolecularWeightResultTextBox.TabIndex = 75;
            this.mAverageMolecularWeightResultTextBox.Text = "";
            // 
            // mOptsPanel
            // 
            this.mOptsPanel.Controls.Add(this.mPreviewHoldingPanel);
            this.mOptsPanel.Controls.Add(this.mSettingsExpandPanel);
            this.mOptsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mOptsPanel.DockPadding.All = 8;
            this.mOptsPanel.Location = new System.Drawing.Point(0, 0);
            this.mOptsPanel.Name = "mOptsPanel";
            this.mOptsPanel.Size = new System.Drawing.Size(736, 859);
            this.mOptsPanel.TabIndex = 91;
            // 
            // mPreviewHoldingPanel
            // 
            this.mPreviewHoldingPanel.Controls.Add(this.mPreviewExpandPanel);
            this.mPreviewHoldingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mPreviewHoldingPanel.DockPadding.Top = 8;
            this.mPreviewHoldingPanel.Location = new System.Drawing.Point(8, 312);
            this.mPreviewHoldingPanel.Name = "mPreviewHoldingPanel";
            this.mPreviewHoldingPanel.Size = new System.Drawing.Size(720, 539);
            this.mPreviewHoldingPanel.TabIndex = 91;
            // 
            // mErrorProvider
            // 
            this.mErrorProvider.ContainerControl = this;
            // 
            // mWarningProvider
            // 
            this.mWarningProvider.ContainerControl = this;
            this.mWarningProvider.Icon = ((System.Drawing.Icon)(resources.GetObject("mWarningProvider.Icon")));
            // 
            // mVerticalBubbleUpLayout2
            // 
            this.mVerticalBubbleUpLayout2.InitialSpacing = 0;
            // 
            // frmMercury
            // 
            this.AutoScale = false;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(736, 859);
            this.Controls.Add(this.mOptsPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(568, 0);
            this.Name = "frmMercury";
            this.Text = "Mercury";
            this.VisibleChanged += new System.EventHandler(this.frmMercury_VisibleChanged);
            this.Frame5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mPreviewChart)).EndInit();
            this.mProteinOrDNATab.ResumeLayout(false);
            this.mProteinEditorTab.ResumeLayout(false);
            this.mProteinButtonsPanel.ResumeLayout(false);
            this.mDNAEditorTab.ResumeLayout(false);
            this.mDNAButtonPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mSettingsExpandPanel)).EndInit();
            this.mSettingsExpandPanel.ResumeLayout(false);
            this.mSettingsInternalPanel.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mAdvancedSettingsExpandPanel)).EndInit();
            this.mAdvancedSettingsExpandPanel.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.mApodizationTypeGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mEditorsExpandPanel)).EndInit();
            this.mEditorsExpandPanel.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mPreviewExpandPanel)).EndInit();
            this.mPreviewExpandPanel.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.mOptsPanel.ResumeLayout(false);
            this.mPreviewHoldingPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        #region "Data loading from MercuryIsotopeDistribution"

        /// <summary>
        /// These function load the settings in the MercuryIsotopeDistribution and 
        /// private class variables into the GUI elements.
        /// </summary>
        private void LoadControlDataFromMercuryIsotopeDistribution() 
        {
            try
            {
                this.LoadResolutionFromMercuryIsotopeDistribution();
                this.LoadChargeStateFromMercuryIsotopeDistribution();
                this.LoadChargeCarrierMassFromMercuryIsotopeDistribution();
                this.LoadApodizationTypeFromMercuryIsotopeDistribution();
                this.LoadMercurySizeFromMercuryIsotopeDistribution();
                this.LoadFormula();
                this.LoadFWHM();
                this.UpdateResolutionAndFWHM();
                this.UpdatePreviewDistribution();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace) ;
            }
        }

        private void LoadFormula() 
        {
            try
            {
                this.mMolecularFormulaTextBox.Text = this.mMolecularFormula.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace) ;
            }
        }
        
        private void LoadResolutionFromMercuryIsotopeDistribution() 
        {
            try
            {
                this.mResolutionTextBox.Text = this.mMercuryIsotopeDistribution.Resolution.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace) ;
            }
        }

        private void LoadChargeStateFromMercuryIsotopeDistribution() 
        {
            try
            {
                this.mChargeStateTextBox.Text = this.mMercuryIsotopeDistribution.ChargeState.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace) ;
            }
        }

        private void LoadChargeCarrierMassFromMercuryIsotopeDistribution() 
        {
            try
            {
                this.mChargeCarrierMassTextBox.Text = this.mMercuryIsotopeDistribution.ChargeCarrierMass.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace) ;
            }
        }

        private void LoadApodizationTypeFromMercuryIsotopeDistribution() 
        {
            try
            {
                var apType = this.mMercuryIsotopeDistribution.ApodizationType;
                if (apType == DeconToolsV2.ApodizationType.Gaussian) 
                {
                    this.mApodizationTypeOptGaussian.Checked = true;
                } 
                else if (apType == DeconToolsV2.ApodizationType.Lorentzian) 
                {
                    this.mApodizationTypeOptLorentzian.Checked = true;
                }
                else 
                {
                    this.mApodizationTypeOptGaussian.Checked = false;
                    this.mApodizationTypeOptLorentzian.Checked = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace) ;
            }
        }

        private void LoadMercurySizeFromMercuryIsotopeDistribution() 
        {	
            try
            {
                this.mMercurySizeCombo.SelectedItem = this.mMercuryIsotopeDistribution.MercurySize;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace) ;
            }
        }

        private void LoadFWHM() 
        {
            try
            {
                this.mFWHMTextBox.Text = this.mFWHM.ToString("G5");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace) ;
            }
        }

        #endregion

        #region "GUI Updating"
        private void UpdatePreviewDistribution() 
        {
            try
            {
                this.UpdatePreviewDistribution(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace) ;
            }
        }

        /// <summary>
        /// Updates the preview chart.  If force is false, the preview is updated only if the 
        /// Auto-Preview mode check box is checked.  If force is true, the preview is always updated.  
        /// If there are input errors on screen, the last successful inputs are used.
        /// </summary>
        /// <param name="force"></param>
        private void UpdatePreviewDistribution(bool forceUpdate) 
        {
            try
            {
                if (this.HasInputErrors()) 
                {
                    SetError(this.mPreviewChart, PREVIEW_INPUT_ERRORS_MESSAGE);	
                } 
                else 
                {
                    SetError(this.mPreviewChart, NO_ERROR_MESSAGE);
                }

                this.SyncDetails();

                if (!forceUpdate && !this.mAutoPreviewCheckBox.Checked) 
                {
                    if (this.HasInputErrors()) 
                    {
                        SetWarning(this.mPreviewChart, NO_ERROR_MESSAGE);
                    } 
                    else 
                    {
                        SetWarning(this.mPreviewChart, PREVIEW_NOT_SYNCED_MESSAGE);
                    }
                    return;
                }
                var points = 
                    this.mMercuryIsotopeDistribution.CalculateDistribution(
                    this.mMolecularFormula.ToElementTable());

                var maxAbundance = float.NegativeInfinity ; 
                float sum = 0 ; 
                for (var ptNum = 0 ; ptNum < points.Length ; ptNum++)
                {
                    sum += points[ptNum].Y ; 
                    if (maxAbundance < points[ptNum].Y)
                    {
                        maxAbundance = points[ptNum].Y ; 
                    }
                }
                var ratio = (100*maxAbundance)/sum ; 
                mtextBoxMostAbundant.Text = ratio.ToString("F2") ; 

                if (!mcheckBoxAbsolute.Checked)
                {
                    for (var ptNum = 0 ; ptNum < points.Length ; ptNum++)
                    {
                        points[ptNum].Y = (points[ptNum].Y*100)/sum ; 
                    }
                    maxAbundance /= sum ; 
                }
                this.mPreviewSeriesDataProvider.SetData(points);
                
                this.mPreviewSeries.PlotParams.Name = this.mMolecularFormula.ToSimpleOrganicElementalString();
                this.mPreviewChart.ViewPortHistory.Clear() ; 
                this.mPreviewChart.AutoViewPort() ; 

                // copy molecular weight results from the 
                this.mMonoMolecularWeightResultTextBox.Text = 
                    this.mMercuryIsotopeDistribution.MonoMolecularMass.ToString();
                this.mAverageMolecularWeightResultTextBox.Text = 
                    this.mMercuryIsotopeDistribution.AverageMolecularMass.ToString();
                this.mVarianceResultTextBox.Text = 
                    this.mMercuryIsotopeDistribution.MassVariance.ToString();

                SetWarning(this.mPreviewChart, NO_ERROR_MESSAGE);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace) ;
            }
        }

        private void UpdateResolutionAndFWHM() 
        {
            try
            {
                this.mMercuryIsotopeDistribution.CalculateMasses(
                    this.mMolecularFormula.ToElementTable());
                if (this.mResolutionMode == ResolutionMode.FWHM) 
                {
                    //take FWHM
                    var mass = this.mMercuryIsotopeDistribution.AverageMolecularMass;
                    this.mMercuryIsotopeDistribution.Resolution = mass / this.mFWHM;
                    this.LoadResolutionFromMercuryIsotopeDistribution();
                }
                else 
                {
                    //Using Resolution, so update FWHM text box based on mass of current formula
                    var mass = this.mMercuryIsotopeDistribution.AverageMolecularMass;
                    var resolution = this.mMercuryIsotopeDistribution.Resolution;
                    this.mFWHM  = mass/resolution;
                    this.LoadFWHM();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace) ;
            }
        }
        #endregion

        #region "Error Utilities"
        private void SetWarning(Control control, String message) 
        {
            try
            {
                this.mWarningProvider.SetError(control, message);
                //SetParentWarning(control);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace) ;
            }
        }

        private void SetError(Control control, String message) 
        {
            try
            {
                this.mErrorProvider.SetError(control, message);
                //SetParentError(control);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + ex.StackTrace) ;
            }
        }

        //		private void SetParentError(Control control) 
        //		{
        //			Control parent = control.Parent;
        //			if (parent != this) 
        //			{
        //				bool hasErrors = false;
        //				foreach (Control child in parent.Controls) 
        //				{
        //					if (this.mErrorProvider.GetError(child) != "") 
        //					{
        //						hasErrors = true;
        //					}
        //				}
        //				if (hasErrors) 
        //				{
        //					this.SetError(parent, frmMercury.CHILD_CONTROLS_HAVE_ERRORS);
        //				} 
        //				else 
        //				{
        //					this.SetError(parent, frmMercury.NO_ERROR_MESSAGE);
        //				}
        //			}
        //		}
        //
        //		private void SetParentWarning(Control control) 
        //		{
        //			Control parent = control.Parent;
        //			if (parent != this) 
        //			{
        //				bool hasErrors = false;
        //				foreach (Control child in parent.Controls) 
        //				{
        //					if (this.mWarningProvider.GetError(child) != "") 
        //					{
        //						hasErrors = true;
        //					}
        //				}
        //				if (hasErrors) 
        //				{
        //					this.SetWarning(parent, frmMercury.CHILD_CONTROLS_HAVE_ERRORS);
        //				} 
        //				else 
        //				{
        //					this.SetWarning(parent, frmMercury.NO_ERROR_MESSAGE);
        //				}
        //			}
        //		}

        private bool HasInputErrors() 
        {
            return ControlHasError(this.mChargeCarrierMassTextBox) 
                || ControlHasError(this.mChargeStateTextBox)
                || ControlHasError(this.mMolecularFormulaTextBox) 
                || ControlHasError(this.mResolutionTextBox) 
                || ControlHasError(this.mFWHMTextBox);
        }

        private bool ControlHasError(Control control) 
        {
            return this.mErrorProvider.GetError(control) != "";
        }
        #endregion

        #region "GUI Input Validations" 
        /// <summary>
        /// These functions parse GUI input from controls that can have errors.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mChargeCarrierMassTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try 
            {
                var cc_mass = double.Parse(this.mChargeCarrierMassTextBox.Text);
                this.mMercuryIsotopeDistribution.ChargeCarrierMass = cc_mass;
                this.LoadChargeCarrierMassFromMercuryIsotopeDistribution();
                SetError(this.mChargeCarrierMassTextBox, NO_ERROR_MESSAGE);
            } 
            catch (Exception ex) 
            {
                SetError(this.mChargeCarrierMassTextBox, frmMercury.CHARGE_CARRIER_ERROR_MESSAGE);
                Console.WriteLine(ex.Message + ex.StackTrace) ; 
            } 
            finally 
            {
                this.UpdatePreviewDistribution();
            }
        }

        private void mChargeStateTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try 
            {
                var charge = short.Parse(this.mChargeStateTextBox.Text);
                this.mMercuryIsotopeDistribution.ChargeState = charge;
                this.LoadChargeStateFromMercuryIsotopeDistribution();
                SetError(this.mChargeStateTextBox, NO_ERROR_MESSAGE);
            } 
            catch (Exception ex) 
            {
                SetError(this.mChargeStateTextBox, 
                    frmMercury.CHARGE_STATE_ERROR_MESSAGE + "\n(" + ex.Message + ")");
            }
            finally 
            {
                this.UpdatePreviewDistribution();
            }
        }

        private void mMolecularFormulaTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UpdateFormulaAndPreview(false);
        }

        private void UpdateFormulaAndPreview(bool force) 
        {
            try 
            {
                mErrorProvider.Dispose() ; 
                this.mMolecularFormula = MolecularFormula.Parse(this.mMolecularFormulaTextBox.Text);
                this.UpdateResolutionAndFWHM();
            }
            catch (Exception ex) 
            {
                SetError(this.mMolecularFormulaTextBox, 
                    frmMercury.MOLECULAR_FORMULA_ERROR_MESSAGE + "\n" + ex.Message + ")");
            }
            finally  
            {
                this.UpdatePreviewDistribution(force);
            }
        }

        private void mFWHMTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try 
            {
                mFWHM = double.Parse(this.mFWHMTextBox.Text);
                SetError(this.mFWHMTextBox, NO_ERROR_MESSAGE);
            } 
            catch (Exception ex) 
            {
                SetError(this.mFWHMTextBox, 
                    frmMercury.RESOLUTION_ERROR_MESSAGE + "\n(" + ex.Message + ")");
            }
            finally 
            {
                this.UpdateResolutionAndFWHM();
                this.UpdatePreviewDistribution();
            }
        }

        private void mResolutionTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try 
            {
                var resolution = double.Parse(this.mResolutionTextBox.Text);
                this.mMercuryIsotopeDistribution.Resolution = resolution;
                this.LoadResolutionFromMercuryIsotopeDistribution();
                SetError(this.mResolutionTextBox, NO_ERROR_MESSAGE);
            } 
            catch (Exception ex) 
            {
                SetError(this.mResolutionTextBox, 
                    frmMercury.RESOLUTION_ERROR_MESSAGE + "\n(" + ex.Message + ")");
            }
            finally 
            {
                this.UpdateResolutionAndFWHM();
                this.UpdatePreviewDistribution();
            }
        }
        #endregion

        // These functions respond to GUI operations that do not require any 
        // validation of the input data, such as check boxes changing, buttons pressed, etc.
        #region "Event Responders"
        private void btnCopyToClipboard_Click(object sender, System.EventArgs e)
        {
            this.CopyDataFromPreviewToClipboard(); 
        
        }
        
        private void frmMercury_VisibleChanged(object sender, System.EventArgs e)
        {
            if (Visible) 
            {
                this.LoadControlDataFromMercuryIsotopeDistribution();
                this.SyncDetails();
            }
        }

        private void mMercurySizeCombo_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            this.mMercuryIsotopeDistribution.MercurySize = (int) this.mMercurySizeCombo.SelectedItem;
            this.LoadMercurySizeFromMercuryIsotopeDistribution();
            this.UpdatePreviewDistribution();
        }

        private void mSimplifyFormulaButton_Click(object sender, System.EventArgs e)
        {
            try 
            {
                mMolecularFormula = MolecularFormula.Parse(
                    MolecularFormula.Parse(mMolecularFormulaTextBox.Text).ToSimpleOrganicElementalString());
                mMolecularFormulaTextBox.Text = mMolecularFormula.ToString();
                SyncDetails();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message + ex.StackTrace) ; 
            }
        }

        /// <summary>
        /// Update the resolution mode to FWHM and change the color of the resolution 
        /// text box to make it look disabled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mFWHMTextBox_Enter(object sender, System.EventArgs e)
        {
            //this.mFWHMTextBox.BackColor = Color.Empty;
            //this.mResolutionTextBox.BackColor = SystemColors.Control;
            this.mFWHMTextBox.ReadOnly = false;
            this.mResolutionTextBox.ReadOnly = true;
            this.mResolutionMode = ResolutionMode.FWHM;
            this.UpdateResolutionAndFWHM();
        }


        /// <summary>
        /// Update the resolution mode to Resolution and change the color of the 
        /// FWHM text box to make it look disabled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mResolutionTextBox_Enter(object sender, System.EventArgs e)
        {
            //this.mResolutionTextBox.BackColor = Color.Empty;
            //this.mFWHMTextBox.BackColor = SystemColors.Control;
            this.mFWHMTextBox.ReadOnly = true;
            this.mResolutionTextBox.ReadOnly = false;
            this.mResolutionMode = ResolutionMode.Resolution;
            this.UpdateResolutionAndFWHM();
        }

        private void mApodizationTypeOptLorentzian_CheckedChanged(object sender, System.EventArgs e)
        {
            if (this.mApodizationTypeOptLorentzian.Checked) 
            {
                this.mMercuryIsotopeDistribution.ApodizationType = DeconToolsV2.ApodizationType.Lorentzian;
            }
            this.UpdatePreviewDistribution();
        }

        private void mApodizationTypeOptGaussian_CheckedChanged(object sender, System.EventArgs e)
        {
            if (this.mApodizationTypeOptGaussian.Checked) 
            {
                this.mMercuryIsotopeDistribution.ApodizationType = DeconToolsV2.ApodizationType.Gaussian;
            }
            this.UpdatePreviewDistribution();
        }

        private void mAutoPreviewCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            this.UpdatePreviewDistribution();
        }

        private void mPreviewButton_Click(object sender, System.EventArgs e)
        {
            this.UpdateFormulaAndPreview(true);
        }
        #endregion

        /// <summary>
        /// Sets the categorized form details to reflect the current form state.
        /// </summary>
        private void SyncDetails() 
        {
            var details = new PNNL.Controls.DetailInfo[1];
            details[0] = new PNNL.Controls.DetailInfo(frmMercury.DetailFormula + this.mMolecularFormula.ToString());
            this.Details = details;
        }

        private void UpdateFormulaFromProteinOrDNAEventHandler(object sender, System.EventArgs e)
        {
            this.UpdateFormulaFromProteinOrDNA(false);
        }

        private void UpdateFormulaFromProteinOrDNA(bool updatePreview) 
        {
            try 
            {
                if (this.mProteinOrDNATab.SelectedTab == this.mProteinEditorTab) 
                {
                    // update using protein translation
                    var translator = mProteinTranslator;
                    var mf = translator.Translate(this.mProteinDNAEditorTextBox.Text, ".");
                    var nEnd = (MolecularFormula) cmbNterm.SelectedItem;
                    var cEnd = (MolecularFormula) cmbCterm.SelectedItem;
                    mf = mf.Add(nEnd, 1).Add(cEnd, 1);

                    SetFormula(mf, updatePreview);
                } 
                else 
                {
                    // update using DNA translation
                    var translator = mDNATranslator;
                    var input = this.mProteinDNAEditorTextBox.Text;
                    if (this.mComplementCheckBox.Checked) 
                    {
                        var original = input;
                        input = "";
                        foreach (var ch in original.ToCharArray()) 
                        {
                            var c = ch;
                            if (c == 'A') 
                            {
                                c = 'T';
                            } 
                            else if (c == 'T') 
                            {
                                c = 'A';
                            }
                            else if (c == 'G') 
                            {
                                c = 'C';
                            } 
                            else if (c == 'C') 
                            {
                                c = 'G';
                            }
                            input += c;
                        }
                    }
                    var mf = translator.Translate(input, ".");
                    var numBases = input.Length;
                    mf = mf.Add(MolecularFormula.Hydrogen, -1 * (numBases - 1), true);
                    mf = mf.Add(MolecularFormula.Phosphorus, (numBases - 1));
                    mf = mf.Add(MolecularFormula.Oxygen, 2 * (numBases - 1));
                    if (this.mDNARNACombo.SelectedItem.Equals("RNA")) 
                    {
                        mf = mf.Add(MolecularFormula.Oxygen, numBases);
                    }
                    if (this.mDNARNACombo.SelectedItem.Equals("Phosphorothioate")) 
                    {
                        mf = mf.Add(MolecularFormula.Oxygen, -1 * (numBases - 1), true);
                        mf = mf.Add(MolecularFormula.Sulphur, (numBases - 1));
                    }
                    if (this.mTermPhosCheckBox.Checked) 
                    {
                        mf = mf.Add(MolecularFormula.Parse("H1 P1 O3"), 1);
                    }
                    if (this.mCyclicPhosCheckBox.Checked) 
                    {
                        mf = mf.Add(MolecularFormula.Parse("H-1 P1 O2", null, true), 1);
                    }
                    if (this.mTrisphosCheckBox.Checked) 
                    {
                        mf = mf.Add(MolecularFormula.Parse("H3 P3 O9"), 1);
                    }

                    SetFormula(mf, updatePreview);
                }
                this.mErrorProvider.SetError(this.mProteinDNAEditorTextBox, "");
            } 
            catch (Exception e) 
            {
                this.mErrorProvider.SetError(this.mProteinDNAEditorTextBox, "Input error");
                Console.WriteLine(e.Message + e.StackTrace) ; 
            }
        }

        public MolecularFormula Formula 
        {
            get 
            {
                return mMolecularFormula;
            }
            set 
            {
                if (value == null) 
                {
                    throw new ArgumentNullException("formula");
                }
                this.SetFormula(value, false);
                this.UpdatePreviewDistribution(true);
            }
        }
        public DeconToolsV2.clsElementIsotopes ElementIsotopes
        {
            set
            {
                mMercuryIsotopeDistribution.ElementIsotopes = value ; 
            }
        }

        private void SetFormula(MolecularFormula formula, bool updatePreview) 
        {
            if (formula == null) 
            {
                throw new ArgumentNullException("formula");
            }
            this.mMolecularFormula = formula;
            this.mMolecularFormulaTextBox.Text = mMolecularFormula.ToSimpleOrganicElementalString();
            this.UpdateResolutionAndFWHM();
            if (updatePreview) 
            {
                this.UpdatePreviewDistribution();
            }
        }

        private void ProteinOrDNAButtonClickHandler(object sender, EventArgs args) 
        {
            var source = (Button) sender;
            // Get the amino acid abbreviation from the tag of the button
            var aminoAcid = (String) source.Tag;
            this.mProteinDNAEditorTextBox.Text += aminoAcid;
            this.UpdateFormulaFromProteinOrDNA(true);
        }

        private void UpdateFormulaFromProteinOrDNAEventHandler2(object sender, System.EventArgs e)
        {
            UpdateFormulaFromProteinOrDNA(true);
        }

        
        private void CopyDataFromPreviewToClipboard()
        {
            clsClipboardUtility.CopyXYValuesToClipboard(this.mPreviewSeriesDataProvider.xs,this.mPreviewSeriesDataProvider.ys);
            
        }


    }

    /// <summary>
    /// Custom clsSeries for mercury.  When copied to another chart enables the height 
    /// multiplier and mz shift menu items.  Copying also enables its being deleted.
    /// </summary>
    internal class clsMercurySeries : PNNL.Controls.clsSeries
    {
        private PNNL.Controls.ctlChartBase mChartInitiallyIn;
        private bool copyByRefAllowed = false;
        private MenuItem menuItemScaleHeight;
        private MenuItem menuItemMzOffset;

        internal clsMercurySeries(MercuryDataProvider dataProvider, PNNL.Controls.clsPlotParams plotParams, 
            PNNL.Controls.ctlChartBase initiallyIn) 
            : base(dataProvider, plotParams)
        {
            this.mChartInitiallyIn = initiallyIn;
            CreateMenuItems();
        }

        private void CreateMenuItems() 
        {
            menuItemScaleHeight = new MenuItem("Adjust Scale Height");
            menuItemMzOffset = new MenuItem("Adjust m/z Offset");
            menuItemScaleHeight.Click += new EventHandler(menuItemScaleHeight_Click);
            menuItemMzOffset.Click += new EventHandler(menuItemMzOffset_Click);
        }

        /// <summary>
        /// Don't allow delete from the chart in the mercury form.
        /// </summary>
        /// <param name="chartIn"></param>
        /// <returns></returns>
        public override bool Deletable(PNNL.Controls.ctlChartBase chartIn)
        {
            return (chartIn != mChartInitiallyIn);
        }

        public override bool CopyByReferenceAllowed
        {
            get
            {
                return copyByRefAllowed;
            }
        }

        protected override PNNL.Controls.IChartDataProvider CloneChartDataProvider()
        {
            return new MercuryDataProvider((float[]) ((MercuryDataProvider) this.DataProvider).xs.Clone(), 
                (float[]) ((MercuryDataProvider) this.DataProvider).ys.Clone());
        }


        public override MenuItem[] GetCustomMenuItems(PNNL.Controls.ctlChartBase chartFor)
        {
            if (chartFor != this.mChartInitiallyIn) 
            {
                return new MenuItem[] {menuItemScaleHeight, menuItemMzOffset};
            }
            return base.GetCustomMenuItems(chartFor);
        }

        /// <summary>
        /// Once it's copied, enable deletion and copying by ref of the copied series.
        /// </summary>
        /// <returns></returns>
        public override PNNL.Controls.clsSeries CopySeries()
        {
            var newSeries = new clsMercurySeries(
                (MercuryDataProvider) this.CloneChartDataProvider(), 
                (PNNL.Controls.clsPlotParams) this.PlotParams.Clone(), null);
            
            return newSeries;
        }

        private void menuItemScaleHeight_Click(object sender, EventArgs e)
        {
            try 
            {
                var form = new frmFloatDialog();
                form.Text = "Adjust Height Multiplier";
                form.Prompt = "Multiplier";
                form.EditingValue = ((MercuryDataProvider) this.DataProvider).HeightMultiplier;
                if (form.ShowDialog() == DialogResult.OK) 
                {
                    ((MercuryDataProvider) this.DataProvider).HeightMultiplier = form.EditingValue;
                }
            } 
            catch (Exception ex) 
            {
                MessageBox.Show("Setting of scale height failed: " + ex.Message);
            }
        }

        private void menuItemMzOffset_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Changing Mercury Series {0} {1}", this, this.GetHashCode());
            try 
            {
                var form = new frmFloatDialog();
                form.Text = "Adjust m/z Offset";
                form.Prompt = "m/z Offset";
                form.EditingValue = ((MercuryDataProvider) this.DataProvider).MZOffset;
                if (form.ShowDialog() == DialogResult.OK) 
                {
                    ((MercuryDataProvider) this.DataProvider).MZOffset = form.EditingValue;
                }
            } 
            catch (Exception ex) 
            {
                MessageBox.Show("Setting of scale height failed: " + ex.Message);
            }
        }
    }

    internal class MercuryDataProvider : PNNL.Controls.ArrayChartDataProvider 
    {
        internal float heightMult = 1;
        internal float mzOffset = 0;
        internal float[] xs;
        internal float[] ys;

        internal MercuryDataProvider(float[] x, float[] y) : base(x, y)
        {
        }

        internal float HeightMultiplier 
        {
            get 
            {
                return this.heightMult;
            }
            set 
            {
                if (value <= 0) 
                {
                    throw new ArgumentOutOfRangeException("HeightMultiplier", value, "Must be > 0");
                }
                this.heightMult = value;
                this.SetData(xs, ys);
            }
        }

        internal float MZOffset 
        {
            get 
            {
                return this.mzOffset;
            }
            set 
            {
                this.mzOffset = value;
                this.SetData(xs, ys);
            }
        }

        /// <summary>
        /// Applies MZOffset and Height Multiplier.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        protected override void PreProcess(ref float[] x, ref float[] y, ref float[] z)
        {
            xs = (float[]) x.Clone();
            ys = (float[]) y.Clone();
            Console.WriteLine("New mercury provider");
//			for (int i = 0; i < x.Length; i++) 
//			{
//			}
            for (var i = 0; i < x.Length; i++) 
            {
                x[i] = x[i] + this.MZOffset;
                y[i] = y[i] * this.HeightMultiplier;
                if (i % 1000 == 0) 
                {
                    Console.WriteLine("Point {0} {1}", x[i], y[i]);
                }
            }
        }
    }
}
