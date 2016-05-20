using System;
using System.Xml.Linq;

namespace DeconTools.Backend.Parameters
{
	[Serializable]
    public class ThrashParameters : ParametersBase
    {

        #region Constructors

        public ThrashParameters()
        {
            MaxCharge = 10;
            MaxMass = 10000;
            MaxFit = 0.25;
            MinIntensityForDeletion = 10;
            MinIntensityForScore = 10;
            
            
            IsO16O18Data = false;
            NumPeaksForShoulder = 1;
            ChargeCarrierMass = Globals.PROTON_MASS;
            MinMSFeatureToBackgroundRatio = 5;
            AveragineFormula = "C4.9384 H7.7583 N1.3577 O1.4773 S0.0417";
            TagFormula = string.Empty;
            IsThrashUsed = true;
            CompleteFit = false;
            AreAllTheoreticalProfilesCachedBeforeStarting = true;
            NumPeaksUsedInAbundance = 1;
            UseAbsoluteIntensity = false;
            AbsolutePeptideIntensity = 0;
            CheckAllPatternsAgainstChargeState1 = false;
            IsotopicProfileFitType = Globals.IsotopicProfileFitType.AREA;
            LeftFitStringencyFactor = 1;
            RightFitStringencyFactor = 1;
        }



        #endregion

        #region Properties

        [Obsolete("Use the 'DeconvolutionType' parameter in the ScanBasedWorkflowParameters class")]
        public bool UseThrashV1 { get; set; }

        public string TagFormula { get; set; }

        //TODO: adjust IsotopicDistributionCalculator so that averagine isn't hard-coded
        public string AveragineFormula { get; set; }

        /// <summary>
        /// When thrash is processing peaks, it will stop when it hits a peak whose
        /// intensity falls below this value
        /// </summary>
        public double MinMSFeatureToBackgroundRatio { get; set; }


        public double MaxFit { get; set; }

        public double MinIntensityForScore { get; set; }

        public double MinIntensityForDeletion { get; set; }

        public int MaxCharge { get; set; }

        public double MaxMass { get; set; }

        public int NumPeaksForShoulder { get; set; }

        public bool IsO16O18Data { get; set; }

        public bool UseAbsoluteIntensity { get; set; }

        public double AbsolutePeptideIntensity { get; set; }

        public bool IsThrashUsed { get; set; }

        public bool CheckAllPatternsAgainstChargeState1 { get; set; }

        public bool CompleteFit { get; set; }

        public double ChargeCarrierMass { get; set; }

        public Globals.IsotopicProfileFitType IsotopicProfileFitType { get; set; }

        public bool AreAllTheoreticalProfilesCachedBeforeStarting { get; set; }

        public double LeftFitStringencyFactor { get; set; }

        public double RightFitStringencyFactor { get; set; }

        public int NumPeaksUsedInAbundance { get; set; }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        public override void LoadParameters(XElement xElement)
        {
            throw new System.NotImplementedException();
        }

        public override void LoadParametersV2(XElement thrashElement)
        {
            AveragineFormula = GetStringValue(thrashElement, "AveragineFormula", AveragineFormula);

            AbsolutePeptideIntensity = GetDoubleValue(thrashElement, "AbsolutePeptideIntensity");
            ChargeCarrierMass = GetDoubleValue(thrashElement, "CCMass", ChargeCarrierMass);
            CheckAllPatternsAgainstChargeState1 = GetBoolVal(thrashElement, "CheckAllPatternsAgainstChargeState1", CheckAllPatternsAgainstChargeState1);

            CompleteFit = GetBoolVal(thrashElement, "CompleteFit", CompleteFit);
            IsO16O18Data = GetBoolVal(thrashElement, "O16O18Media", IsO16O18Data);
            IsThrashUsed = GetBoolVal(thrashElement, "ThrashOrNot", IsThrashUsed);
            IsotopicProfileFitType = (Globals.IsotopicProfileFitType) GetEnum(thrashElement, "IsotopeFitType", IsotopicProfileFitType.GetType(), IsotopicProfileFitType);

            LeftFitStringencyFactor = GetDoubleValue(thrashElement, "LeftFitStringencyFactor", LeftFitStringencyFactor);

            MaxCharge = GetIntValue(thrashElement, "MaxCharge", MaxCharge);
            MaxFit = GetDoubleValue(thrashElement, "MaxFit", MaxFit);
            MaxMass = GetDoubleValue(thrashElement, "MaxMW", MaxMass);

            MinIntensityForDeletion = GetDoubleValue(thrashElement, "DeleteIntensityThreshold", MinIntensityForDeletion);
            MinIntensityForScore = GetDoubleValue(thrashElement, "MinIntensityForScore", MinIntensityForScore);
            MinMSFeatureToBackgroundRatio = GetDoubleValue(thrashElement, "PeptideMinBackgroundRatio",
                                                           MinMSFeatureToBackgroundRatio);

            NumPeaksForShoulder = GetIntValue(thrashElement, "NumPeaksForShoulder", NumPeaksForShoulder);
            NumPeaksUsedInAbundance = GetIntValue(thrashElement, "NumPeaksUsedInAbundance", NumPeaksUsedInAbundance);

            RightFitStringencyFactor = GetDoubleValue(thrashElement, "RightFitStringencyFactor", RightFitStringencyFactor);
            TagFormula = GetStringValue(thrashElement, "TagFormula", TagFormula);
            UseAbsoluteIntensity = GetBoolVal(thrashElement, "UseAbsolutePeptideIntensity", UseAbsoluteIntensity);
            AreAllTheoreticalProfilesCachedBeforeStarting = GetBoolVal(thrashElement, "UseMercuryCache", AreAllTheoreticalProfilesCachedBeforeStarting);

        }
    }
}
