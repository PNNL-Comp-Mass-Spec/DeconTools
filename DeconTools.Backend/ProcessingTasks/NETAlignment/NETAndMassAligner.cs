using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using MultiAlignEngine.Alignment;
using DeconTools.Backend.Core.Results;
using MultiAlignEngine.MassTags;
using MultiAlignEngine.Features;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.NETAlignment
{
    public class NETAndMassAligner
    {
        private List<MassTag> _massTagList;
        private List<TargetedResult> _featuresToBeAligned;

        #region Constructors

        public NETAndMassAligner()
        {
            _massTagList = new List<MassTag>();
            _featuresToBeAligned = new List<TargetedResult>();

        }

        #endregion

        #region Properties




        #endregion

        #region Public Methods

        public void SetReferenceMassTags(List<MassTag> massTagList)
        {
            _massTagList = massTagList;
        }

        public void SetFeaturesToBeAligned(List<TargetedResult> featuresToAlign)
        {
            _featuresToBeAligned = featuresToAlign;
        }

        public clsAlignmentFunction GetAlignment(List<MassTag> massTagList, List<TargetedResult> featuresToAlign)
        {

            clsMassTagDB multialignMassTagDB = new clsMassTagDB();

            //TODO: figure out which options I need to parameterize in this class. For now, use defaults
            clsAlignmentOptions alignmentOptions = new clsAlignmentOptions();
            clsAlignmentProcessor processor = new clsAlignmentProcessor();


            clsMassTag[] multiAlignMassTags = convertDeconToolsMassTagsToMultialignMassTags(massTagList);

            multialignMassTagDB.AddMassTags(multiAlignMassTags);
            processor.SetReferenceDatasetFeatures(multialignMassTagDB);

            List<clsUMC> multialignUMCs = convertDeconToolsLCMSFeaturesToMultialignFeatures(featuresToAlign);

            processor.SetAligneeDatasetFeatures(multialignUMCs, alignmentOptions.MZBoundaries[0]);
            processor.PerformAlignmentToMSFeatures();

            return processor.GetAlignmentFunction();


        }




        public void Execute(Run run)
        {
            Check.Require(run != null, "Run is not defined.");
            Check.Require(_featuresToBeAligned != null && _featuresToBeAligned.Count > 0, "Features for alignment have not been defined.");
            Check.Require(_massTagList != null && _massTagList.Count > 0, "The reference set of MassTags used in alignment have not been defined.");

            run.AlignmentInfo = GetAlignment(_massTagList, _featuresToBeAligned);



        }

        #endregion

        #region Private Methods
        private List<clsUMC> convertDeconToolsLCMSFeaturesToMultialignFeatures(List<TargetedResult> featuresToAlign)
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

        private clsUMC convertDeconToolsTargetedFeatureToUMC(TargetedResult feature)
        {
            clsUMC umc = new clsUMC();
            umc.AbundanceMax = feature.Intensity;
            umc.AbundanceSum = (long)feature.Intensity;
            umc.AverageDeconFitScore = feature.FitScore;
            umc.AverageInterferenceScore = feature.IScore;
            umc.ChargeMax = feature.ChargeState;
            umc.ChargeRepresentative = (short)feature.ChargeState;
            umc.ClusterId = 0;
            umc.CombinedScore = 0;
            umc.ConformationFitScore = 0;
            umc.ConformationId = 0;
            umc.DatasetId = 0;
            umc.DriftTime = 0;
            umc.DriftTimeUncorrected = 0;
            umc.Id = (int)feature.MassTagID;
            umc.Mass = feature.MonoMass;

            umc.MZForCharge = feature.MonoMZ;
            umc.Net = feature.NET;
            umc.Scan = feature.ScanLC;
            umc.ScanEnd = feature.ScanLCEnd;
            umc.ScanStart = feature.ScanLCStart;
            umc.SpectralCount = 0;

            return umc;
        }

        private clsMassTag[] convertDeconToolsMassTagsToMultialignMassTags(List<MassTag> massTagList)
        {
            List<clsMassTag> massTags = new List<clsMassTag>();

            foreach (var mt in massTagList)
            {
                clsMassTag multialignMassTag = convertDeconToolsMassTagToMultialignMassTag(mt);
                massTags.Add(multialignMassTag);

            }


            return massTags.ToArray();
        }

        private clsMassTag convertDeconToolsMassTagToMultialignMassTag(MassTag mt)
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
            multialignMassTag.NetAverage = mt.NETVal;
            multialignMassTag.NetPredicted = -1;
            multialignMassTag.NetStandardDeviation = 0;
            multialignMassTag.Peptide = mt.PeptideSequence;
            multialignMassTag.PeptideEx = String.Empty;
            multialignMassTag.XCorr = -1;

            return multialignMassTag;
        }


        #endregion

    }
}
