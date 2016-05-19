using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using DeconTools.Workflows.Backend.Results;
using MultiAlignEngine.Alignment;
using MultiAlignEngine.Features;
using MultiAlignEngine.MassTags;

namespace DeconTools.Workflows.Backend.Core
{
    public class NETAndMassAligner
    {
        private List<TargetBase> _massTagList;
        private TargetedResultRepository _featuresToBeAligned;


        #region Constructors

        public NETAndMassAligner()
        {
            _massTagList = new List<TargetBase>();
            _featuresToBeAligned = new TargetedResultRepository();
            this.Result = new AlignmentResult();
            this.AlignerParameters = new NETAndMassAlignerParameters();
        }

        public NETAndMassAligner(NETAndMassAlignerParameters alignerParameters)
            : this()
        {
            this.AlignerParameters = alignerParameters;

        }

        #endregion


        public AlignmentResult Result { get; set; }


        #region Public Methods

        public void SetReferenceMassTags(List<TargetBase> massTagList)
        {
            _massTagList = massTagList;
        }

        public void SetFeaturesToBeAligned(List<TargetedResultBase> featuresToAlign)
        {
            _featuresToBeAligned.Clear();
            _featuresToBeAligned.AddResults(featuresToAlign);
        }


        public void SetFeaturesToBeAligned(List<TargetedResultDTO> featuresToAlign)
        {
            _featuresToBeAligned.Clear();
            _featuresToBeAligned.AddResults(featuresToAlign);
        }

        public clsAlignmentFunction GetAlignment(List<TargetBase> massTagList, List<TargetedResultDTO> featuresToAlign)
        {

            clsMassTagDB multialignMassTagDB = new clsMassTagDB();

            //TODO: I might be able to dynamically update these values. Take my foundFeatures and calculate their avg PPMDiff. Then use that info here. 
            clsAlignmentOptions alignmentOptions = new clsAlignmentOptions();
             alignmentOptions.MassCalibrationWindow = this.AlignerParameters.MassCalibrationWindow;  //note -  it seems that 50 ppm is used as a default setting in VIPER. 
            alignmentOptions.ContractionFactor = this.AlignerParameters.ContractionFactor;
            alignmentOptions.IsAlignmentBaselineAMasstagDB = this.AlignerParameters.IsAlignmentBaselineAMassTagDB;
            alignmentOptions.MassBinSize = this.AlignerParameters.MassBinSize;
            alignmentOptions.MassCalibrationLSQNumKnots = this.AlignerParameters.MassCalibrationLSQNumKnots;
            alignmentOptions.MassCalibrationLSQZScore = this.AlignerParameters.MassCalibrationLSQZScore;
            alignmentOptions.MassCalibrationMaxJump = this.AlignerParameters.MassCalibrationMaxJump;
            alignmentOptions.MassCalibrationMaxZScore = this.AlignerParameters.MassCalibrationMaxZScore;
            alignmentOptions.MassCalibrationNumMassDeltaBins = this.AlignerParameters.MassCalibrationNumMassDeltaBins;
            alignmentOptions.MassCalibrationNumXSlices = this.AlignerParameters.MassCalibrationNumXSlices;
            alignmentOptions.MassCalibrationUseLSQ = this.AlignerParameters.MassCalibrationUseLSQ;
            alignmentOptions.MassCalibrationWindow = this.AlignerParameters.MassCalibrationWindow;
            alignmentOptions.MassTolerance = this.AlignerParameters.MassToleranceForNETAlignment;
            alignmentOptions.MaxPromiscuity = this.AlignerParameters.MaxPromiscuity;
            alignmentOptions.MaxTimeJump = this.AlignerParameters.MaxTimeJump;
            alignmentOptions.NETBinSize = this.AlignerParameters.NETBinSize;
            alignmentOptions.NETTolerance = this.AlignerParameters.NETTolerance;
            alignmentOptions.NumTimeSections = this.AlignerParameters.NumTimeSections;
            alignmentOptions.UsePromiscuousPoints = this.AlignerParameters.UsePromiscuousPoints;
       
            clsAlignmentProcessor processor = new clsAlignmentProcessor();
            processor.AlignmentOptions = alignmentOptions;


            clsMassTag[] multiAlignMassTags = convertDeconToolsMassTagsToMultialignMassTags(massTagList);

            multialignMassTagDB.AddMassTags(multiAlignMassTags);
            processor.SetReferenceDatasetFeatures(multialignMassTagDB);

            List<clsUMC> multialignUMCs = convertDeconToolsLCMSFeaturesToMultialignFeatures(featuresToAlign);

            processor.SetAligneeDatasetFeatures(multialignUMCs, alignmentOptions.MZBoundaries[0]);
            processor.PerformAlignmentToMSFeatures();

            this.Result = getResultsForAlignment(processor);


            return processor.GetAlignmentFunction();


        }

        private AlignmentResult getResultsForAlignment(clsAlignmentProcessor processor)
        {
            float[] scanLCValues = null;
            float[] NETValues = null;

            float[,] scores = null;

            processor.GetAlignmentHeatMap(ref scores, ref scanLCValues, ref NETValues);

            AlignmentResult result = new AlignmentResult();
            result.ScanLCValues = scanLCValues;
            result.NETValues = NETValues;
            result.AlignmentHeatmapScores = scores;


            //get massResiduals_vs_scan and massResiduals_vs_m/z
            classAlignmentResidualData residuals = processor.GetResidualData();
            

            result.Mass_vs_scan_ResidualsBeforeAlignment = residuals.massError;
            result.Mass_vs_scan_ResidualsAfterAlignment = residuals.massErrorCorrected;
            result.Mass_vs_scan_ResidualsScanValues = residuals.scans;


            result.Mass_vs_mz_ResidualsBeforeAlignment = residuals.mzMassError;
            result.Mass_vs_mz_ResidualsAfterAlignment = residuals.mzMassErrorCorrected;
            result.Mass_vs_mz_ResidualsMZValues = residuals.mz;


            //get stats on variability
            result.NETStDev = processor.GetNETStandardDeviation();
            result.NETAverage = processor.GetNETMean();
            result.MassAverage = processor.GetMassMean();
            result.MassStDev = processor.GetMassStandardDeviation();

            double[,] massHistogramData = null;
            double[,] netHistogramData = null;
            double[,] driftHistogramData = null;
            processor.GetErrorHistograms(0.1, 0.002, 0.1, ref massHistogramData, ref netHistogramData, ref driftHistogramData);

            result.massHistogramData = massHistogramData;
            result.NETHistogramData = netHistogramData;


            return result;




        }





        public void Execute(Run run)
        {
            Check.Require(run != null, "Run is not defined.");
            Check.Require(_featuresToBeAligned.HasResults, "Features for alignment have not been defined.");
            Check.Require(_massTagList != null && _massTagList.Count > 0, "The reference set of MassTags used in alignment have not been defined.");

            var lcmswarpAlignmentInfo = GetAlignment(_massTagList, _featuresToBeAligned.Results);

            MassAlignmentInfoLcmsWarp massAlignmentInfo = new MassAlignmentInfoLcmsWarp();
            massAlignmentInfo.AlignmentInfo = lcmswarpAlignmentInfo;

            run.MassAlignmentInfo = massAlignmentInfo;


        }

        #endregion

        #region Private Methods

        private List<clsUMC> convertDeconToolsLCMSFeaturesToMultialignFeatures(List<TargetedResultDTO> featuresToAlign)
        {
            List<clsUMC> umcs = new List<clsUMC>();

            int umcIndexCounter = 0;
            foreach (var feature in featuresToAlign)
            {
                clsUMC umc = convertDeconToolsTargetedFeatureToUMC(feature);


                umc.mint_umc_index = umcIndexCounter;

                umcs.Add(umc);

                umcIndexCounter++;
            }

            return umcs;
        }

        private clsUMC convertDeconToolsTargetedFeatureToUMC(TargetedResultDTO result)
        {
            clsUMC umc = new clsUMC();
            umc.AbundanceMax = result.Intensity;
            umc.AbundanceSum = (long)result.Intensity;
            umc.AverageDeconFitScore = result.FitScore;
            umc.AverageInterferenceScore = result.IScore;
            umc.ChargeMax = result.ChargeState;
            umc.ChargeRepresentative = (short)result.ChargeState;
            umc.ClusterId = 0;
            umc.CombinedScore = 0;
            umc.ConformationFitScore = 0;
            umc.ConformationId = 0;
            umc.DatasetId = 0;
            umc.DriftTime = 0;
            umc.DriftTimeUncorrected = 0;
            umc.Id = (int)result.TargetID;
            umc.Mass = result.MonoMass;
            umc.MZForCharge = result.MonoMZ;
            umc.Net = result.NET;
            umc.Scan = result.ScanLC;
            umc.ScanStart = result.ScanLCStart;
            umc.ScanEnd = result.ScanLCEnd;

            umc.SpectralCount = 0;

            return umc;
        }





        private clsMassTag[] convertDeconToolsMassTagsToMultialignMassTags(List<TargetBase> massTagList)
        {
            List<clsMassTag> massTags = new List<clsMassTag>();

            foreach (var mt in massTagList)
            {
                clsMassTag multialignMassTag = convertDeconToolsMassTagToMultialignMassTag(mt);
                massTags.Add(multialignMassTag);

            }


            return massTags.ToArray();
        }

        private clsMassTag convertDeconToolsMassTagToMultialignMassTag(TargetBase mt)
        {
            clsMassTag multialignMassTag = new clsMassTag();

            multialignMassTag.Id = mt.ID;
            multialignMassTag.Charge1FScore = 0;
            multialignMassTag.Charge2FScore = 0;
            multialignMassTag.Charge3FScore = 0;
            multialignMassTag.ChargeState = mt.ChargeState;
            multialignMassTag.CleavageState = -1;
            multialignMassTag.DiscriminantMax = 0;
            //multialignMassTag.DriftTime = 0;
            multialignMassTag.HighPeptideProphetProbability = 0;
            multialignMassTag.Mass = mt.MonoIsotopicMass;
            multialignMassTag.ModCount = -1;
            multialignMassTag.Modifications = String.Empty;
            multialignMassTag.MSGFSpecProbMax = 0;
            multialignMassTag.MSMSObserved = mt.ObsCount;
            multialignMassTag.NetAverage = mt.NormalizedElutionTime;
            multialignMassTag.NetPredicted = -1;
            multialignMassTag.NetStandardDeviation = 0;
            multialignMassTag.Peptide = mt.Code;
            multialignMassTag.PeptideEx = String.Empty;
            multialignMassTag.XCorr = -1;

            return multialignMassTag;
        }


        #endregion




        public NETAndMassAlignerParameters AlignerParameters { get; set; }
    }
}
