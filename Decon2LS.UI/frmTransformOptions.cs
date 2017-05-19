// Written by Navdeep Jaitly & Anoop Mayampurath for the Department of Energy (PNNL, Richland, WA)
// Copyright 2006, Battelle Memorial Institute
// E-mail: navdeep.jaitly@pnl.gov
// Website: http://omics.pnl.gov/software or http://panomics.pnnl.gov
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
using System.Xml;
namespace Decon2LS
{
    /// <summary>
    /// Summary description for frmTransformOptions.
    /// </summary>
    public class frmTransformOptions : System.Windows.Forms.Form
    {
        private System.Windows.Forms.TreeNode nodePeakPicking;
        private System.Windows.Forms.TreeNode nodePeakPickingGeneral;
        private System.Windows.Forms.TreeNode nodeHornTransform;
        private System.Windows.Forms.TreeNode nodeHornTransformGeneral;
        private System.Windows.Forms.TreeNode nodeThresholdSettings;
        private System.Windows.Forms.TreeNode nodeIsotopeDistribution;			
        private System.Windows.Forms.TreeNode nodeIsotopeDistributionAveragine;
        private System.Windows.Forms.TreeNode nodeIsotopeDistributionIsotopicDistribution;			
        private System.Windows.Forms.TreeNode nodeIsotopeDistributionComposition;
        private System.Windows.Forms.TreeNode nodeMiscellaneousOptions;
        private System.Windows.Forms.TreeNode nodePreprocessingOptions;
        private System.Windows.Forms.TreeNode nodeDTAGenerationOptions ; 
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Panel panel1;
        private Decon2LS.ctlElementalIsotopeDisplay mctlElementalIsotopeDisplay;
        private Decon2LS.ctlAveragineOptions mctlAveragineOptions;
        private Decon2LS.ctlThresholdSettings mctlThresholdSettings;
        private Decon2LS.ctlPeakPickingOptions mctlPeakPickingOptions;
        private System.Windows.Forms.Button mbtnOK;
        private System.Windows.Forms.Button mbtnCancel;
        private System.Windows.Forms.Button mbtnLoadParameterFile;
        private System.Windows.Forms.Button mbtnSaveParameters;
        private bool first_click = true;
        private Decon2LS.ctlMiscellaneousOptions mctlMiscellaneousOptions;
        private Decon2LS.ctlPreprocessOptions mctlPreprocessOptions;
        private Decon2LS.ctlHornMassTransform mctlHornMassTransformOptions;
        private Decon2LS.ctlDTASettings mctlDTASettings;
        
        private System.ComponentModel.IContainer components;

        public frmTransformOptions()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();	
        
            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            Init() ; 
        }

        public frmTransformOptions(DeconToolsV2.Peaks.clsPeakProcessorParameters peakParameters, 
            DeconToolsV2.HornTransform.clsHornTransformParameters hornParameters, 
            DeconToolsV2.Readers.clsRawDataPreprocessOptions fticrPreprocessParameters, DeconToolsV2.DTAGeneration.clsDTAGenerationParameters dtaParameters)
        {
            try
            {
                //
                // Required for Windows Form Designer support
                //
                InitializeComponent();	
        
                //
                // TODO: Add any constructor code after InitializeComponent call
                //
                Init() ; 
                SetPeakPickingOptions(peakParameters) ; 
                SetTransformOptions(hornParameters) ; 
                SetMiscellaneousOptions(hornParameters) ; 
                SetFTICRRawPreProcessing(fticrPreprocessParameters) ; 
                SetDTAGeneration(dtaParameters) ; 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace) ;
            }
        }

        public void SetDTAGeneration(DeconToolsV2.DTAGeneration.clsDTAGenerationParameters dtaParameters)
        {
            try
            {
                mctlDTASettings.MinimumMassToConsider = dtaParameters.MinMass ; 
                mctlDTASettings.MaximumMassToConsider = dtaParameters.MaxMass ;
                mctlDTASettings.MinimumScanToConsider = dtaParameters.MinScan ; 
                mctlDTASettings.MaximumScanToConsider = dtaParameters.MaxScan ;
                mctlDTASettings.MinIonCount = dtaParameters.MinIonCount ; 
                mctlDTASettings.OutputType = dtaParameters.OutputType ; 
                mctlDTASettings.ChargeToConsider = dtaParameters.ConsiderChargeValue ; 
                if (dtaParameters.ConsiderChargeValue > 0)
                    mctlDTASettings.ConsiderCharge = true ;
                else
                    mctlDTASettings.ConsiderCharge = false ; 
                mctlDTASettings.CCMass = dtaParameters.CCMass ; 				
                mctlDTASettings.WindowSize = dtaParameters.WindowSizetoCheck ; 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace) ;
            }
        }

        public void SetPeakPickingOptions(DeconToolsV2.Peaks.clsPeakProcessorParameters peakParameters)
        {
            try
            {
                mctlPeakPickingOptions.PeakBackgroundRatio = peakParameters.PeakBackgroundRatio ; 
                mctlPeakPickingOptions.SignalToNoiseThreshold = peakParameters.SignalToNoiseThreshold ; 
                mctlPeakPickingOptions.PeakFitType = peakParameters.PeakFitType ; 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace) ;
            }
        }

        public void SetTransformOptions(DeconToolsV2.HornTransform.clsHornTransformParameters hornParameters)
        {
            try
            {
                mctlAveragineOptions.TagFormula = hornParameters.TagFormula ; 
                mctlAveragineOptions.AveragineFormula = hornParameters.AveragineFormula ; 

                mctlThresholdSettings.DeleteIntensityThreshold = hornParameters.DeleteIntensityThreshold ; 
                mctlThresholdSettings.MaxFit = hornParameters.MaxFit ; 
                mctlThresholdSettings.MinIntensityForScore = hornParameters.MinIntensityForScore ; 

                mctlHornMassTransformOptions.MaxCharge = hornParameters.MaxCharge ; 
                mctlHornMassTransformOptions.MaxMW = hornParameters.MaxMW ; 

                mctlHornMassTransformOptions.SumSpectra = hornParameters.SumSpectra ; 
                mctlHornMassTransformOptions.SumAcrossScanRange = hornParameters.SumSpectraAcrossScanRange  ; 
                mctlHornMassTransformOptions.NumScansToSumOver = hornParameters.NumScansToSumOver ; 

                mctlHornMassTransformOptions.NumPeaksForShoulder = hornParameters.NumPeaksForShoulder ; 
                mctlHornMassTransformOptions.O16O18Media = hornParameters.O16O18Media ; 
                mctlHornMassTransformOptions.PeptideMinBackgroundRatio = hornParameters.PeptideMinBackgroundRatio ; 
                mctlHornMassTransformOptions.ThrashOrNot = hornParameters.ThrashOrNot ; 
                mctlHornMassTransformOptions.CompleteFit = hornParameters.CompleteFit ; 
                mctlHornMassTransformOptions.CCMass = hornParameters.CCMass ; 
                mctlHornMassTransformOptions.CompleteFit = hornParameters.CompleteFit ; 
                mctlHornMassTransformOptions.UseMercuryCaching = hornParameters.UseMercuryCaching ; 
                mctlHornMassTransformOptions.CheckAllPatternsAgainstCharge1 = hornParameters.CheckAllPatternsAgainstCharge1 ;
                mctlHornMassTransformOptions.IsotopeFitType = hornParameters.IsotopeFitType ;
                mctlHornMassTransformOptions.UseAbsolutePeptideIntensity = hornParameters.UseAbsolutePeptideIntensity ; 
                mctlHornMassTransformOptions.AbsolutePeptideIntensity = hornParameters.AbsolutePeptideIntensity ; 
                mctlElementalIsotopeDisplay.ElementIsotopes = hornParameters.ElementIsotopeComposition ; 
                mctlHornMassTransformOptions.IsActualMonoMZUsed = hornParameters.IsActualMonoMZUsed;
                mctlHornMassTransformOptions.LeftFitStringencyFactor = hornParameters.LeftFitStringencyFactor;
                mctlHornMassTransformOptions.RightFitStringencyFactor = hornParameters.RightFitStringencyFactor;


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace) ;
            }
        }

        public void SetMiscellaneousOptions(DeconToolsV2.HornTransform.clsHornTransformParameters hornParameters)
        {
            try
            {
                mctlMiscellaneousOptions.UseScanRange = hornParameters.UseScanRange ; 
                if (hornParameters.UseScanRange)
                {
                    mctlMiscellaneousOptions.MinScan = hornParameters.MinScan ; 
                    mctlMiscellaneousOptions.MaxScan = hornParameters.MaxScan ; 
                }
                mctlMiscellaneousOptions.UseMZRange = hornParameters.UseMZRange ; 
                if (hornParameters.UseMZRange)
                {
                    mctlMiscellaneousOptions.MinMZ = hornParameters.MinMZ ; 
                    mctlMiscellaneousOptions.MaxMZ = hornParameters.MaxMZ ; 
                }

                mctlMiscellaneousOptions.UseSavitzkyGolaySmooth = hornParameters.UseSavitzkyGolaySmooth ; 
                if (hornParameters.UseSavitzkyGolaySmooth)
                {
                    mctlMiscellaneousOptions.SGNumLeft = hornParameters.SGNumLeft ; 
                    mctlMiscellaneousOptions.SGNumRight = hornParameters.SGNumRight ; 
                    mctlMiscellaneousOptions.SGOrder = hornParameters.SGOrder ; 
                }
                mctlMiscellaneousOptions.ZeroFill = hornParameters.ZeroFill ; 
                if (hornParameters.ZeroFill)
                {
                    mctlMiscellaneousOptions.NumZerosToFill  = hornParameters.NumZerosToFill ; 
                }
                mctlMiscellaneousOptions.ProcessMSMS = hornParameters.ProcessMSMS ; 

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace) ;
            }
        }

        public void SetFTICRRawPreProcessing(DeconToolsV2.Readers.clsRawDataPreprocessOptions fticrPreprocessingOptions)
        {
            try
            {
                mctlPreprocessOptions.ApodizationMinX = fticrPreprocessingOptions.ApodizationMinX ; 
                mctlPreprocessOptions.ApodizationMaxX = fticrPreprocessingOptions.ApodizationMaxX ; 
                mctlPreprocessOptions.ApodizationType = fticrPreprocessingOptions.ApodizationType ; 
                mctlPreprocessOptions.ApodizationPercent = fticrPreprocessingOptions.ApodizationPercent ; 
                mctlPreprocessOptions.NumZeroFills = fticrPreprocessingOptions.NumZeroFills ; 
                mctlPreprocessOptions.CalibrationType = fticrPreprocessingOptions.CalibrationType ; 
                mctlPreprocessOptions.CalibrationConstA = fticrPreprocessingOptions.A ; 
                mctlPreprocessOptions.CalibrationConstB = fticrPreprocessingOptions.B ; 
                mctlPreprocessOptions.CalibrationConstC = fticrPreprocessingOptions.C ; 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace) ;
            }

        }

        public void GetPeakPickingOptions(DeconToolsV2.Peaks.clsPeakProcessorParameters peakParameters)
        {
            try
            {
                peakParameters.PeakBackgroundRatio = mctlPeakPickingOptions.PeakBackgroundRatio; 
                peakParameters.SignalToNoiseThreshold = mctlPeakPickingOptions.SignalToNoiseThreshold ; 
                peakParameters.PeakFitType = mctlPeakPickingOptions.PeakFitType ; 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace) ;
            }
        }

        public void GetTransformOptions(DeconToolsV2.HornTransform.clsHornTransformParameters hornParameters)
        {
            try
            {
                hornParameters.TagFormula = mctlAveragineOptions.TagFormula ; 
                hornParameters.AveragineFormula = mctlAveragineOptions.AveragineFormula ; 

                hornParameters.DeleteIntensityThreshold = mctlThresholdSettings.DeleteIntensityThreshold ; 
                hornParameters.MaxFit = mctlThresholdSettings.MaxFit ; 
                hornParameters.MinIntensityForScore = mctlThresholdSettings.MinIntensityForScore ; 

                hornParameters.MaxCharge = mctlHornMassTransformOptions.MaxCharge ; 
                hornParameters.MaxMW = mctlHornMassTransformOptions.MaxMW ; 

                hornParameters.NumPeaksForShoulder = mctlHornMassTransformOptions.NumPeaksForShoulder ; 
                hornParameters.O16O18Media = mctlHornMassTransformOptions.O16O18Media ; 
                hornParameters.PeptideMinBackgroundRatio = mctlHornMassTransformOptions.PeptideMinBackgroundRatio ; 
                hornParameters.ThrashOrNot = mctlHornMassTransformOptions.ThrashOrNot ; 
                hornParameters.CompleteFit = mctlHornMassTransformOptions.CompleteFit ; 
                hornParameters.CCMass = mctlHornMassTransformOptions.CCMass ; 
                hornParameters.CompleteFit = mctlHornMassTransformOptions.CompleteFit ; 
                hornParameters.UseMercuryCaching = mctlHornMassTransformOptions.UseMercuryCaching ; 
                hornParameters.IsotopeFitType = mctlHornMassTransformOptions.IsotopeFitType ;
                hornParameters.CheckAllPatternsAgainstCharge1 = mctlHornMassTransformOptions.CheckAllPatternsAgainstCharge1 ; 

                hornParameters.UseAbsolutePeptideIntensity = mctlHornMassTransformOptions.UseAbsolutePeptideIntensity ; 
                hornParameters.AbsolutePeptideIntensity = mctlHornMassTransformOptions.AbsolutePeptideIntensity ; 
                hornParameters.ElementIsotopeComposition = mctlElementalIsotopeDisplay.ElementIsotopes ; 
                hornParameters.SumSpectra = mctlHornMassTransformOptions.SumSpectra ; 
                hornParameters.SumSpectraAcrossScanRange = mctlHornMassTransformOptions.SumAcrossScanRange ; 
                hornParameters.NumScansToSumOver = mctlHornMassTransformOptions.NumScansToSumOver ; 

                hornParameters.MinS2N =  mctlPeakPickingOptions.SignalToNoiseThreshold ; 
                hornParameters.IsActualMonoMZUsed = mctlHornMassTransformOptions.IsActualMonoMZUsed;
                hornParameters.LeftFitStringencyFactor = mctlHornMassTransformOptions.LeftFitStringencyFactor;
                hornParameters.RightFitStringencyFactor = mctlHornMassTransformOptions.RightFitStringencyFactor;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace) ;
            }
        }

        public void GetMiscellaneousOptions(DeconToolsV2.HornTransform.clsHornTransformParameters hornParameters)
        {
            try
            {
                hornParameters.UseScanRange = mctlMiscellaneousOptions.UseScanRange ; 
                if (hornParameters.UseScanRange)
                {
                    hornParameters.MinScan = mctlMiscellaneousOptions.MinScan ; 
                    hornParameters.MaxScan = mctlMiscellaneousOptions.MaxScan ;
                }
                hornParameters.UseMZRange = mctlMiscellaneousOptions.UseMZRange ; 
                if (hornParameters.UseMZRange)
                {
                    hornParameters.MinMZ = mctlMiscellaneousOptions.MinMZ ; 
                    hornParameters.MaxMZ = mctlMiscellaneousOptions.MaxMZ ; 
                }

                hornParameters.UseSavitzkyGolaySmooth = mctlMiscellaneousOptions.UseSavitzkyGolaySmooth ; 
                if (hornParameters.UseSavitzkyGolaySmooth)
                {
                    hornParameters.SGNumLeft = mctlMiscellaneousOptions.SGNumLeft ; 
                    hornParameters.SGNumRight = mctlMiscellaneousOptions.SGNumRight ; 
                    hornParameters.SGOrder = mctlMiscellaneousOptions.SGOrder ; 
                }
                hornParameters.ZeroFill = mctlMiscellaneousOptions.ZeroFill ; 
                if (hornParameters.ZeroFill)
                {
                    hornParameters.NumZerosToFill = mctlMiscellaneousOptions.NumZerosToFill ; 
                }
                hornParameters.ProcessMSMS = mctlMiscellaneousOptions.ProcessMSMS ; 

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace) ;
            }
        }
        public void GetDTASettings (DeconToolsV2.DTAGeneration.clsDTAGenerationParameters dtaParameters) 
        {
            try
            {
                dtaParameters.ConsiderChargeValue = mctlDTASettings.ChargeToConsider ; 
                dtaParameters.MinIonCount = mctlDTASettings.MinIonCount ; 
                dtaParameters.MaxMass = mctlDTASettings.MaximumMassToConsider ; 
                dtaParameters.MinMass = mctlDTASettings.MinimumMassToConsider ; 
                dtaParameters.MaxScan = mctlDTASettings.MaximumScanToConsider ;
                dtaParameters.MinScan = mctlDTASettings.MinimumScanToConsider ;
                dtaParameters.CCMass = mctlDTASettings.CCMass ; 				
                dtaParameters.OutputType = mctlDTASettings.OutputType ; 
                dtaParameters.WindowSizetoCheck = mctlDTASettings.WindowSize ; 					
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace) ; 
            }
        
        }


        public void GetFTICRRawPreProcessing(DeconToolsV2.Readers.clsRawDataPreprocessOptions fticrPreprocessingOptions)
        {
            try
            {
                fticrPreprocessingOptions.ApodizationMinX=	mctlPreprocessOptions.ApodizationMinX ; 
                fticrPreprocessingOptions.ApodizationMaxX=	mctlPreprocessOptions.ApodizationMaxX ; 
                fticrPreprocessingOptions.ApodizationType=	mctlPreprocessOptions.ApodizationType ; 
                fticrPreprocessingOptions.ApodizationPercent= mctlPreprocessOptions.ApodizationPercent ; 
                fticrPreprocessingOptions.NumZeroFills = mctlPreprocessOptions.NumZeroFills ; 
                fticrPreprocessingOptions.CalibrationType =	mctlPreprocessOptions.CalibrationType;
                fticrPreprocessingOptions.A = mctlPreprocessOptions.CalibrationConstA ;
                fticrPreprocessingOptions.B = mctlPreprocessOptions.CalibrationConstB ;
                fticrPreprocessingOptions.C = mctlPreprocessOptions.CalibrationConstC ;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace) ;
            }

        }

        private void Init()
        {
            try
            {
                nodePeakPicking = new TreeNode();
                nodePeakPickingGeneral = new TreeNode();
                nodeHornTransform = new TreeNode();
                nodeHornTransformGeneral = new TreeNode();
                nodeIsotopeDistribution = new TreeNode();				
                nodeIsotopeDistributionAveragine = new TreeNode();
                nodeIsotopeDistributionComposition = new TreeNode();
                nodeIsotopeDistributionIsotopicDistribution = new TreeNode();				
                nodeThresholdSettings = new TreeNode();
                nodeMiscellaneousOptions = new TreeNode();
                nodeDTAGenerationOptions = new TreeNode() ; 
                nodePreprocessingOptions = new TreeNode();
        
        
                nodePeakPicking.Text = "Peak Picking";
                nodeHornTransform.Text = "Horn Transform";
                nodeIsotopeDistribution.Text = "Isotope Distribution";
                nodePeakPickingGeneral.Text = "General";
                nodeHornTransformGeneral.Text = "General";				
                nodeIsotopeDistributionAveragine.Text = "Averagine";
                nodeIsotopeDistributionComposition.Text = "Isotopic Composition";				
                nodeThresholdSettings.Text = "Threshold Settings";				
                nodeMiscellaneousOptions.Text = "Miscellaneous Options" ; 
                nodePreprocessingOptions.Text = "FTICR Preprocessing Options" ; 
                nodeDTAGenerationOptions.Text = "DTA Generation Options" ; 

        

                treeView1.Nodes.Add(nodePeakPicking);				
                nodePeakPicking.Nodes.Add(nodePeakPickingGeneral);
            
                treeView1.Nodes.Add(nodeHornTransform);				
                nodeHornTransform.Nodes.Add(nodeHornTransformGeneral);
                nodeHornTransform.Nodes.Add(nodeThresholdSettings);

                treeView1.Nodes.Add(nodeIsotopeDistribution);								
                nodeIsotopeDistribution.Nodes.Add(nodeIsotopeDistributionAveragine);				
                nodeIsotopeDistribution.Nodes.Add(nodeIsotopeDistributionComposition);			

                treeView1.Nodes.Add(nodeMiscellaneousOptions) ; 

                treeView1.Nodes.Add(nodePreprocessingOptions) ; 

                treeView1.Nodes.Add(nodeDTAGenerationOptions) ; 
            
                nodePeakPicking.Expand();
                nodePeakPicking.ImageIndex = 1;
                nodePeakPickingGeneral.ImageIndex = 2;		
        
                mctlElementalIsotopeDisplay = new ctlElementalIsotopeDisplay() ;
                mctlPeakPickingOptions = new ctlPeakPickingOptions() ; 
                mctlPreprocessOptions = new ctlPreprocessOptions() ; 
                mctlHornMassTransformOptions = new ctlHornMassTransform() ; 
                mctlDTASettings = new ctlDTASettings() ; 

                this.mctlHornMassTransformOptions.Location = new System.Drawing.Point(232, 8);
                this.mctlHornMassTransformOptions.Name = "mctlHornMassTransformOptions";
                this.mctlHornMassTransformOptions.Size = new System.Drawing.Size(520, 472);
                this.mctlHornMassTransformOptions.TabIndex = 9;
                this.mctlHornMassTransformOptions.Visible = false;
                this.mctlHornMassTransformOptions.Dock = DockStyle.Fill ; 
                this.mctlHornMassTransformOptions.BringToFront() ; 

                this.mctlElementalIsotopeDisplay.Location = new System.Drawing.Point(232, 8);
                this.mctlElementalIsotopeDisplay.Name = "mctlElementalIsotopeDisplay";
                this.mctlElementalIsotopeDisplay.Size = new System.Drawing.Size(520, 472);
                this.mctlElementalIsotopeDisplay.TabIndex = 10 ;
                this.mctlElementalIsotopeDisplay.Visible = false;

                this.mctlPeakPickingOptions.Location = new System.Drawing.Point(232, 8);
                this.mctlPeakPickingOptions.Name = "mctlPeakPickingOptions";
                this.mctlPeakPickingOptions.Size = new System.Drawing.Size(520, 472);
                this.mctlPeakPickingOptions.TabIndex = 11;
                this.mctlPeakPickingOptions.Visible = true;			

                this.mctlPreprocessOptions.Location = new System.Drawing.Point(232, 8);
                this.mctlPreprocessOptions.Name = "mctlPreprocessOptions";
                this.mctlPreprocessOptions.Size = new System.Drawing.Size(520, 472);
                this.mctlPreprocessOptions.TabIndex = 12;
                this.mctlPreprocessOptions.Visible = false;		
                this.mctlPreprocessOptions.Dock = DockStyle.Fill ; 
                this.mctlPreprocessOptions.BringToFront() ; 

                this.mctlDTASettings.Location = new System.Drawing.Point(232, 8);
                this.mctlDTASettings.Name = "mctlDTASettings";
                this.mctlDTASettings.Size = new System.Drawing.Size(520, 472);
                this.mctlDTASettings.TabIndex = 13 ;
                this.mctlDTASettings.Visible = false;			
                this.mctlDTASettings.Dock = DockStyle.Fill ; 
        
                this.panel1.Controls.Add(this.mctlPeakPickingOptions);
                this.panel1.Controls.Add(this.mctlHornMassTransformOptions);
                this.panel1.Controls.Add(this.mctlElementalIsotopeDisplay);
                this.panel1.Controls.Add(this.mctlMiscellaneousOptions);
                this.panel1.Controls.Add(this.mctlPreprocessOptions);
                this.panel1.Controls.Add(this.mctlDTASettings) ; 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace) ;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if (components != null) 
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
            var resources = new System.Resources.ResourceManager(typeof(frmTransformOptions));
            this.mbtnOK = new System.Windows.Forms.Button();
            this.mbtnCancel = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.mbtnSaveParameters = new System.Windows.Forms.Button();
            this.mbtnLoadParameterFile = new System.Windows.Forms.Button();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.mctlThresholdSettings = new Decon2LS.ctlThresholdSettings();
            this.mctlMiscellaneousOptions = new Decon2LS.ctlMiscellaneousOptions();
            this.mctlAveragineOptions = new Decon2LS.ctlAveragineOptions();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mbtnOK
            // 
            this.mbtnOK.Location = new System.Drawing.Point(408, 19);
            this.mbtnOK.Name = "mbtnOK";
            this.mbtnOK.Size = new System.Drawing.Size(120, 30);
            this.mbtnOK.TabIndex = 2;
            this.mbtnOK.Text = "OK";
            this.mbtnOK.Click += new System.EventHandler(this.mbtnOK_Click);
            // 
            // mbtnCancel
            // 
            this.mbtnCancel.Location = new System.Drawing.Point(576, 18);
            this.mbtnCancel.Name = "mbtnCancel";
            this.mbtnCancel.Size = new System.Drawing.Size(120, 30);
            this.mbtnCancel.TabIndex = 3;
            this.mbtnCancel.Text = "Cancel";
            this.mbtnCancel.Click += new System.EventHandler(this.mbtnCancel_Click);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.mbtnSaveParameters);
            this.groupBox1.Controls.Add(this.mbtnLoadParameterFile);
            this.groupBox1.Controls.Add(this.mbtnOK);
            this.groupBox1.Controls.Add(this.mbtnCancel);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox1.Location = new System.Drawing.Point(0, 632);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(722, 56);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            // 
            // mbtnSaveParameters
            // 
            this.mbtnSaveParameters.Location = new System.Drawing.Point(167, 18);
            this.mbtnSaveParameters.Name = "mbtnSaveParameters";
            this.mbtnSaveParameters.Size = new System.Drawing.Size(120, 30);
            this.mbtnSaveParameters.TabIndex = 5;
            this.mbtnSaveParameters.Text = "Save Parameters";
            this.mbtnSaveParameters.Click += new System.EventHandler(this.mbtnSaveParameters_Click);
            // 
            // mbtnLoadParameterFile
            // 
            this.mbtnLoadParameterFile.Location = new System.Drawing.Point(16, 18);
            this.mbtnLoadParameterFile.Name = "mbtnLoadParameterFile";
            this.mbtnLoadParameterFile.Size = new System.Drawing.Size(120, 30);
            this.mbtnLoadParameterFile.TabIndex = 4;
            this.mbtnLoadParameterFile.Text = "Load Parameters";
            this.mbtnLoadParameterFile.Click += new System.EventHandler(this.mbtnLoadParameterFile_Click);
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Left;
            this.treeView1.ImageList = this.imageList1;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.SelectedImageIndex = 1;
            this.treeView1.ShowLines = false;
            this.treeView1.ShowPlusMinus = false;
            this.treeView1.ShowRootLines = false;
            this.treeView1.Size = new System.Drawing.Size(232, 632);
            this.treeView1.TabIndex = 5;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.mctlThresholdSettings);
            this.panel1.Controls.Add(this.mctlMiscellaneousOptions);
            this.panel1.Controls.Add(this.mctlAveragineOptions);
            this.panel1.Controls.Add(this.treeView1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(722, 632);
            this.panel1.TabIndex = 11;
            // 
            // mctlThresholdSettings
            // 
            this.mctlThresholdSettings.DeleteIntensityThreshold = 1;
            this.mctlThresholdSettings.Location = new System.Drawing.Point(432, 8);
            this.mctlThresholdSettings.MaxFit = 0.1;
            this.mctlThresholdSettings.MinIntensityForScore = 5;
            this.mctlThresholdSettings.Name = "mctlThresholdSettings";
            this.mctlThresholdSettings.Size = new System.Drawing.Size(528, 502);
            this.mctlThresholdSettings.TabIndex = 8;
            this.mctlThresholdSettings.Visible = false;
            // 
            // mctlMiscellaneousOptions
            // 
            this.mctlMiscellaneousOptions.Location = new System.Drawing.Point(256, 8);
            this.mctlMiscellaneousOptions.MaxMZ = 2000;
            this.mctlMiscellaneousOptions.MaxScan = 1000;
            this.mctlMiscellaneousOptions.MinMZ = 400;
            this.mctlMiscellaneousOptions.MinScan = 1;
            this.mctlMiscellaneousOptions.Name = "mctlMiscellaneousOptions";
            this.mctlMiscellaneousOptions.NumZerosToFill = ((short)(3));
            this.mctlMiscellaneousOptions.ProcessMSMS = false;
            this.mctlMiscellaneousOptions.SGNumLeft = ((short)(2));
            this.mctlMiscellaneousOptions.SGNumRight = ((short)(2));
            this.mctlMiscellaneousOptions.SGOrder = ((short)(2));
            this.mctlMiscellaneousOptions.Size = new System.Drawing.Size(528, 502);
            this.mctlMiscellaneousOptions.TabIndex = 10;
            this.mctlMiscellaneousOptions.UseMZRange = false;
            this.mctlMiscellaneousOptions.UseSavitzkyGolaySmooth = false;
            this.mctlMiscellaneousOptions.UseScanRange = false;
            this.mctlMiscellaneousOptions.Visible = false;
            this.mctlMiscellaneousOptions.ZeroFill = false;
            // 
            // mctlAveragineOptions
            // 
            this.mctlAveragineOptions.AveragineFormula = "C4.9384 H7.7583 N1.3577 O1.4773 S0.0417";
            this.mctlAveragineOptions.Location = new System.Drawing.Point(560, 112);
            this.mctlAveragineOptions.Name = "mctlAveragineOptions";
            this.mctlAveragineOptions.Size = new System.Drawing.Size(528, 502);
            this.mctlAveragineOptions.TabIndex = 9;
            this.mctlAveragineOptions.TagFormula = "";
            this.mctlAveragineOptions.Visible = false;
            // 
            // frmTransformOptions
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(722, 688);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmTransformOptions";
            this.Text = "Options";
            this.groupBox1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion		

        private void treeView1_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            try
            {
                var selectedNode = e.Node ; 
                if (nodePeakPicking.IsSelected)
                {
                    if(!first_click)					
                        nodePeakPicking.Toggle();					
                    else
                        first_click = false;

                    if (nodePeakPicking.IsExpanded)
                    {
                        nodeHornTransform.Collapse();
                        nodeHornTransform.ImageIndex = 0;
                        nodeIsotopeDistribution.Collapse();
                        nodeIsotopeDistribution.ImageIndex = 0;	
                        nodeMiscellaneousOptions.ImageIndex = 0 ; 
                        nodePreprocessingOptions.ImageIndex = 0 ; 
                        nodeDTAGenerationOptions.ImageIndex  = 0 ; 					
                        nodePeakPickingGeneral.ImageIndex = 2;
                        mctlPeakPickingOptions.BringToFront() ; 
                        mctlPeakPickingOptions.Dock = DockStyle.Fill ; 
                        mctlPeakPickingOptions.Visible = true;
                        mctlHornMassTransformOptions.Visible = false;
                        mctlAveragineOptions.Visible = false;
                        mctlElementalIsotopeDisplay.Visible = false;
                        mctlThresholdSettings.Visible = false;
                        mctlPreprocessOptions.Visible = false ; 
                        mctlMiscellaneousOptions.Visible = false ; 
                        mctlDTASettings.Visible = false ; 

                    }
                }				
                else if (nodeHornTransform.IsSelected)
                {					
                    nodeHornTransform.Toggle();
                    if (nodeHornTransform.IsExpanded)
                    {	
                        nodePeakPicking.Collapse();
                        nodePeakPicking.ImageIndex = 0;						
                        nodeIsotopeDistribution.Collapse();
                        nodeIsotopeDistribution.ImageIndex = 0;
                        nodeMiscellaneousOptions.ImageIndex = 0 ; 
                        nodePreprocessingOptions.ImageIndex = 0 ; 
                        nodeDTAGenerationOptions.ImageIndex  = 0 ; 					
                        nodeHornTransformGeneral.ImageIndex = 2;
                        nodeThresholdSettings.ImageIndex = 3;
                        mctlHornMassTransformOptions.Dock = DockStyle.Fill ; 
                        mctlHornMassTransformOptions.Visible = true;
                        mctlPeakPickingOptions.Visible = false;
                        mctlAveragineOptions.Visible = false;	
                        mctlThresholdSettings.Visible = false;
                        mctlElementalIsotopeDisplay.Visible = false;
                        mctlPreprocessOptions.Visible = false ; 
                        mctlMiscellaneousOptions.Visible = false ; 
                        mctlDTASettings.Visible = false ; 
                        mctlHornMassTransformOptions.BringToFront() ; 

                    }
                }			
                else if (nodeIsotopeDistribution.IsSelected)
                {
                    nodeIsotopeDistribution.Toggle();
                    if (nodeIsotopeDistribution.IsExpanded)
                    {
                        nodePeakPicking.Collapse();
                        nodePeakPicking.ImageIndex = 0;
                        nodeHornTransform.Collapse();	
                        nodeHornTransform.ImageIndex = 0;
                        nodeMiscellaneousOptions.ImageIndex = 0 ; 
                        nodePreprocessingOptions.ImageIndex = 0 ; 
                        nodeDTAGenerationOptions.ImageIndex  = 0 ; 					
                        nodeIsotopeDistributionComposition.ImageIndex = 3;
                        nodeIsotopeDistributionAveragine.ImageIndex = 2;
                        mctlPeakPickingOptions.Visible = false;
                        mctlHornMassTransformOptions.Visible = false;
                        mctlAveragineOptions.BringToFront() ; 
                        mctlAveragineOptions.Dock = DockStyle.Fill ; 
                        mctlAveragineOptions.Visible = true;
                        mctlElementalIsotopeDisplay.Visible = false;
                        mctlThresholdSettings.Visible = false;
                        mctlPreprocessOptions.Visible = false ; 
                        mctlMiscellaneousOptions.Visible = false ; 
                        mctlDTASettings.Visible = false ; 

                    }
                }				

                else if (nodePeakPickingGeneral.IsSelected)
                {
                    nodeHornTransform.Collapse();
                    nodeHornTransform.ImageIndex = 0;
                    nodeIsotopeDistribution.Collapse();
                    nodeIsotopeDistribution.ImageIndex = 0;
                    nodeMiscellaneousOptions.ImageIndex = 0 ; 
                    nodePreprocessingOptions.ImageIndex = 0 ; 
                    nodeDTAGenerationOptions.ImageIndex  = 0 ; 					
                    nodePeakPickingGeneral.SelectedImageIndex = 2;
                    nodePeakPicking.ImageIndex = 1;
                    mctlPeakPickingOptions.Dock = DockStyle.Fill ; 
                    mctlPeakPickingOptions.Visible = true;
                    mctlHornMassTransformOptions.Visible = false;
                    mctlAveragineOptions.Visible = false;
                    mctlElementalIsotopeDisplay.Visible = false;
                    mctlThresholdSettings.Visible = false;
                    mctlPreprocessOptions.Visible = false ; 
                    mctlMiscellaneousOptions.Visible = false ; 
                    mctlDTASettings.Visible = false ; 
                    mctlPeakPickingOptions.BringToFront() ; 
                }
                else if (nodeHornTransformGeneral.IsSelected)
                {
                    nodePeakPicking.Collapse();
                    nodePeakPicking.ImageIndex = 0;			
                    nodeMiscellaneousOptions.ImageIndex = 0 ; 
                    nodePreprocessingOptions.ImageIndex = 0 ; 
                    nodeDTAGenerationOptions.ImageIndex  = 0 ; 					
                    nodeIsotopeDistribution.Collapse();
                    nodeIsotopeDistribution.ImageIndex = 0;
                    nodeHornTransformGeneral.SelectedImageIndex = 2;
                    nodeThresholdSettings.ImageIndex = 3;			
                    nodeHornTransform.ImageIndex = 1;
                    mctlHornMassTransformOptions.Dock = DockStyle.Fill ; 
                    mctlHornMassTransformOptions.Visible = true;
                    mctlPeakPickingOptions.Visible = false;
                    mctlAveragineOptions.Visible = false;
                    mctlElementalIsotopeDisplay.Visible = false;
                    mctlThresholdSettings.Visible = false;
                    mctlPreprocessOptions.Visible = false ; 
                    mctlDTASettings.Visible = false ; 
                    mctlMiscellaneousOptions.Visible = false ; 
                    mctlHornMassTransformOptions.BringToFront() ;
                }

                else if (nodeThresholdSettings.IsSelected)
                {
                    nodePeakPicking.Collapse();
                    nodePeakPicking.ImageIndex = 0;						
                    nodeIsotopeDistribution.Collapse();
                    nodeIsotopeDistribution.ImageIndex = 0;
                    nodeThresholdSettings.SelectedImageIndex = 2;
                    nodeHornTransformGeneral.ImageIndex = 3;
                    nodeHornTransform.ImageIndex = 1;
                    nodeMiscellaneousOptions.ImageIndex = 0 ; 
                    nodePreprocessingOptions.ImageIndex = 0 ; 
                    nodeDTAGenerationOptions.ImageIndex  = 0 ; 
                    mctlThresholdSettings.Dock = DockStyle.Fill ; 
                    mctlThresholdSettings.Visible = true;
                    mctlPeakPickingOptions.Visible = false;
                    mctlAveragineOptions.Visible = false;
                    mctlElementalIsotopeDisplay.Visible = false;
                    mctlHornMassTransformOptions.Visible = false;
                    mctlPreprocessOptions.Visible = false ; 
                    mctlMiscellaneousOptions.Visible = false ; 
                    mctlThresholdSettings.BringToFront() ;
                }
                else if (nodeMiscellaneousOptions.IsSelected)
                {
                    nodePeakPicking.Collapse();
                    nodePeakPicking.ImageIndex = 0;						
                    nodeIsotopeDistribution.Collapse();
                    nodeIsotopeDistribution.ImageIndex = 0;
                    nodeMiscellaneousOptions.SelectedImageIndex = 2;
                    nodePreprocessingOptions.ImageIndex = 0 ;
                    nodeDTAGenerationOptions.ImageIndex = 0 ; 
                    nodeHornTransformGeneral.ImageIndex = 0 ;
                    nodeHornTransform.ImageIndex = 0 ;
                    mctlMiscellaneousOptions.BringToFront() ; 
                    mctlMiscellaneousOptions.Dock = DockStyle.Fill ; 
                    mctlMiscellaneousOptions.Visible = true;
                    mctlThresholdSettings.Visible = false;
                    mctlPeakPickingOptions.Visible = false;
                    mctlAveragineOptions.Visible = false;
                    mctlElementalIsotopeDisplay.Visible = false;
                    mctlHornMassTransformOptions.Visible = false;
                    mctlPreprocessOptions.Visible = false ; 
                    mctlDTASettings.Visible = false ; 

                }
                else if (nodeDTAGenerationOptions.IsSelected)
                {
                    nodePeakPicking.Collapse();
                    nodePeakPicking.ImageIndex = 0;						
                    nodeIsotopeDistribution.Collapse();
                    nodeIsotopeDistribution.ImageIndex = 0;
                    nodeHornTransformGeneral.ImageIndex = 0 ;
                    nodeHornTransform.ImageIndex = 0 ;
                    nodeMiscellaneousOptions.ImageIndex = 0;
                    nodePreprocessingOptions.ImageIndex = 0;
                    nodeDTAGenerationOptions.SelectedImageIndex = 2 ; 
                    mctlMiscellaneousOptions.Visible = false ;
                    mctlThresholdSettings.Visible = false;
                    mctlPeakPickingOptions.Visible = false;
                    mctlAveragineOptions.Visible = false;
                    mctlElementalIsotopeDisplay.Visible = false;
                    mctlHornMassTransformOptions.Visible = false;					
                    mctlPreprocessOptions.Visible = false ;
                    mctlDTASettings.Dock = DockStyle.Fill ; 
                    mctlDTASettings.BringToFront() ; 
                    mctlDTASettings.Visible = true ; 
                }
                else if (nodePreprocessingOptions.IsSelected)
                {
                    nodePeakPicking.Collapse();
                    nodePeakPicking.ImageIndex = 0;						
                    nodeIsotopeDistribution.Collapse();
                    nodeIsotopeDistribution.ImageIndex = 0;
                    nodeMiscellaneousOptions.ImageIndex = 0 ; 
                    nodePreprocessingOptions.SelectedImageIndex = 2 ; 
                    nodeDTAGenerationOptions.ImageIndex = 0 ; 
                    nodeHornTransformGeneral.ImageIndex = 0 ;
                    nodeHornTransform.ImageIndex = 0 ;
                    mctlMiscellaneousOptions.Visible = false ;
                    mctlThresholdSettings.Visible = false;
                    mctlPeakPickingOptions.Visible = false;
                    mctlAveragineOptions.Visible = false;
                    mctlElementalIsotopeDisplay.Visible = false;
                    mctlHornMassTransformOptions.Visible = false;
                    mctlPreprocessOptions.Dock = DockStyle.Fill ; 
                    mctlPreprocessOptions.Visible = true ; 
                    mctlDTASettings.Visible = false ; 
                    mctlPreprocessOptions.BringToFront() ;
                }
                else if (nodeIsotopeDistributionAveragine.IsSelected)
                {
                    nodePeakPicking.Collapse();
                    nodePeakPicking.ImageIndex = 0;
                    nodeHornTransform.Collapse();	
                    nodeHornTransform.ImageIndex = 0;
                    nodeIsotopeDistributionAveragine.SelectedImageIndex = 2;
                    nodeIsotopeDistributionComposition.ImageIndex = 3;
                    nodeIsotopeDistribution.ImageIndex = 1;
                    mctlPeakPickingOptions.Visible = false;
                    mctlHornMassTransformOptions.Visible = false;
                    mctlAveragineOptions.Dock = DockStyle.Fill ;
                    mctlAveragineOptions.Visible = true;
                    mctlElementalIsotopeDisplay.Visible = false;
                    mctlThresholdSettings.Visible = false;;
                    mctlPreprocessOptions.Visible = false ; 
                    mctlMiscellaneousOptions.Visible = false ; 
                }
                else if (nodeIsotopeDistributionComposition.IsSelected)
                {
                    nodePeakPicking.Collapse();
                    nodePeakPicking.ImageIndex = 0;
                    nodeHornTransform.Collapse();	
                    nodeHornTransform.ImageIndex = 0;
                    nodeIsotopeDistributionComposition.SelectedImageIndex = 2;
                    nodeIsotopeDistributionAveragine.ImageIndex = 3;
                    nodeIsotopeDistribution.ImageIndex = 1;
                    mctlElementalIsotopeDisplay.BringToFront() ; 
                    mctlElementalIsotopeDisplay.Dock = DockStyle.Fill ;
                    mctlElementalIsotopeDisplay.Visible = true;
                    mctlHornMassTransformOptions.Visible = false;
                    mctlPeakPickingOptions.Visible = false;
                    mctlAveragineOptions.Visible = false;
                    mctlThresholdSettings.Visible = false;
                    mctlPreprocessOptions.Visible = false ; 
                    mctlDTASettings.Visible = false ; 
                    mctlMiscellaneousOptions.Visible = false ; 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace) ;
            }			
        }			

        private void mctlThresholdSettings_Load(object sender, System.EventArgs e)
        {
        
        }

        private void mbtnOK_Click(object sender, System.EventArgs e)
        {
            try
            {
                mctlMiscellaneousOptions.SanityCheck() ; 
                DialogResult = DialogResult.OK ; 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace) ;
            }
        }

        private void mbtnCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel ; 
            this.Hide() ; 
        }

        private void SaveV1ParametersToFile(string file_name)
        {
            try
            {
                var paramLoader = new clsParameterLoader() ; 

                var peakOptions = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
                GetPeakPickingOptions(peakOptions) ; 
                paramLoader.PeakParameters = peakOptions ; 

                var dtaOptions = new DeconToolsV2.DTAGeneration.clsDTAGenerationParameters() ;
                GetDTASettings(dtaOptions) ; 
                paramLoader.DTAParameters = dtaOptions ; 

                var transformOptions = new DeconToolsV2.HornTransform.clsHornTransformParameters() ; 
                GetTransformOptions(transformOptions) ; 
                GetMiscellaneousOptions(transformOptions) ; 
                // And the isotope abundances.
                transformOptions.ElementIsotopeComposition = mctlElementalIsotopeDisplay.ElementIsotopes ; 
                paramLoader.TransformParameters = transformOptions ; 

                var fticrPreProcessOptions = new DeconToolsV2.Readers.clsRawDataPreprocessOptions() ; 
                GetFTICRRawPreProcessing(fticrPreProcessOptions) ; 
                paramLoader.FTICRPreprocessOptions = fticrPreProcessOptions ; 

                paramLoader.SaveParametersToFile(file_name) ; 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace) ;
            }
        }

        private void LoadV1ParametersFromFile(string file_name)
        {
            try
            {
                var paramLoader = new clsParameterLoader() ; 
                paramLoader.LoadParametersFromFile(file_name) ; 
                SetPeakPickingOptions(paramLoader.PeakParameters) ; 
                SetTransformOptions(paramLoader.TransformParameters) ; 
                SetMiscellaneousOptions(paramLoader.TransformParameters) ; 
                SetFTICRRawPreProcessing(paramLoader.FTICRPreprocessOptions) ; 
                SetDTAGeneration(paramLoader.DTAParameters) ; 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace) ;
            }
        }


        private void mbtnLoadParameterFile_Click(object sender, System.EventArgs e)
        {
            try
            {
                var openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "Parameter files (*.xml)|*.xml" ;
                openFileDialog1.FilterIndex = 1 ;
                openFileDialog1.RestoreDirectory = true ;

                if(openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    LoadV1ParametersFromFile(openFileDialog1.FileName) ;
                }
                else
                {
                    return ; 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace) ;
            }
        }

        private void mbtnSaveParameters_Click(object sender, System.EventArgs e)
        {
            try
            {
                mctlMiscellaneousOptions.SanityCheck() ; 

                var saveDialog = new SaveFileDialog(); 
                saveDialog.Title = "Specify Parameter File to Save to";
                saveDialog.Filter = "Parameter File (*.xml)|*.xml";
                saveDialog.FilterIndex = 1;
                saveDialog.OverwritePrompt = true;
                saveDialog.RestoreDirectory = true;
                saveDialog.InitialDirectory = "";				
                
                String fileName = null;
                if (saveDialog.ShowDialog() == DialogResult.Cancel)
                    return ; 

                fileName = saveDialog.FileName ; 
                SaveV1ParametersToFile(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace) ;
            }
        }

        
                
    }
}

