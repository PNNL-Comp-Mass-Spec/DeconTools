using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator
{
    /// <summary>
    /// Theoretical isotopic pattern generator
    /// </summary>
    /// <remarks>Use IsotopicDistributionCalculator, developed by Josh Aldrich, Tom Taverner, and Gordon Slysz</remarks>
    public class JoshTheorFeatureGenerator : ITheorFeatureGenerator
    {
        readonly IsotopicDistributionCalculator _isotopicDistCalculator = IsotopicDistributionCalculator.Instance;
        readonly N15IsotopeProfileGenerator _N15IsotopicProfileGenerator = new N15IsotopeProfileGenerator();
        readonly DeuteriumIsotopeProfileGenerator _DeuteriumIsotopicProfileGenerator = new DeuteriumIsotopeProfileGenerator();

        #region Constructors
        public JoshTheorFeatureGenerator()
            : this(Globals.LabelingType.NONE, 0.005)
        {
        }

        public JoshTheorFeatureGenerator(Globals.LabelingType labelingType, double lowPeakCutOff)
        {
            LabelingType = labelingType;
            LowPeakCutOff = lowPeakCutOff;
        }

        public JoshTheorFeatureGenerator(Globals.LabelingType labelingType, double fractionLabeling, double lowPeakCutOff)
        {
            LabelingType = labelingType;
            LowPeakCutOff = lowPeakCutOff;
            FractionLabeling = fractionLabeling;
        }

        public JoshTheorFeatureGenerator(Globals.LabelingType labelingType, double fractionLabeling, double lowPeakCutOff, double molarMixingFraction)
        {
            LabelingType = labelingType;
            LowPeakCutOff = lowPeakCutOff;
            FractionLabeling = fractionLabeling;
            MolarMixingFraction = molarMixingFraction;
            // Unused:
            // var _DeuteriumIsotopicProfileGenerator = new DeuteriumIsotopeProfileGenerator(molarMixingFraction, 1 - molarMixingFraction);
        }

        #endregion

        #region Properties
        public Globals.LabelingType LabelingType { get; set; }

        /// <summary>
        /// Degree of labeling. Ranges between 0 and 1.0.  1 = 100% label incorporation
        /// </summary>
        public double FractionLabeling { get; set; }

        public double LowPeakCutOff { get; set; }

        /// <summary>
        /// how the two samples were mixed together.  0.5 means 1:1 ratio.  Range is 0 to 1.  .25 ia
        /// </summary>
        public double MolarMixingFraction { get; set; }

        #endregion

        #region Public Methods


        public override IsotopicProfile GenerateTheorProfile(string empiricalFormula, int chargeState)
        {
            var iso = _isotopicDistCalculator.GetIsotopePattern(empiricalFormula);
            iso.ChargeState = chargeState;

            var monoMass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(empiricalFormula);

            if (chargeState != 0)
            {
                CalculateMassesForIsotopicProfile(iso, monoMass, chargeState);
            }

            return iso;
        }

        public override void GenerateTheorFeature(TargetBase mt)
        {
            Check.Require(mt != null, "FeatureGenerator failed. MassTag not defined.");
            Check.Require(!string.IsNullOrEmpty(mt.EmpiricalFormula), "Theoretical feature generator failed. Can't retrieve empirical formula from Mass Tag");

            switch (LabelingType)
            {
                case Globals.LabelingType.NONE:
                    mt.IsotopicProfile = GetUnlabeledIsotopicProfile(mt);
                    break;
                case Globals.LabelingType.O18:
                    throw new NotImplementedException();
                case Globals.LabelingType.N15:
                    mt.IsotopicProfileLabeled = _N15IsotopicProfileGenerator.GetN15IsotopicProfile2(mt, LowPeakCutOff);
                    break;
                case Globals.LabelingType.Deuterium:
                    //mt.IsotopicProfile = GetUnlabeledIsotopicProfile(mt);
                    //mt.IsotopicProfileLabeled = _DeuteriumIsotopicProfileGenerator.GetDHIsotopicProfile2(mt, LowPeakCutOff, FractionLabeling, MolarMixingFraction);

                    //swap so we can keep the normal in labeled box and use the D/H for general processing
                    mt.IsotopicProfile = GetUnlabeledIsotopicProfile(mt);//needed for _DeuteriumIsotopicProfileGenerator
                    mt.IsotopicProfile = _DeuteriumIsotopicProfileGenerator.GetDHIsotopicProfile2(mt, LowPeakCutOff, FractionLabeling, MolarMixingFraction);
                    mt.IsotopicProfileLabeled = GetUnlabeledIsotopicProfile(mt);

                    break;
                default:
                    throw new NotImplementedException();
            }

        }
        #endregion

        #region Private Methods

        private IsotopicProfile GetUnlabeledIsotopicProfile(TargetBase mt)
        {
            IsotopicProfile iso;

            try
            {
                //empirical formula may contain non-integer values for
                iso = _isotopicDistCalculator.GetIsotopePattern(mt.EmpiricalFormula);
            }
            catch (Exception ex)
            {
                throw new Exception("Theoretical feature generator failed. Details: " + ex.Message);
            }

            PeakUtilities.TrimIsotopicProfile(iso, LowPeakCutOff);
            iso.ChargeState = mt.ChargeState;


            if (iso.ChargeState != 0)
                CalculateMassesForIsotopicProfile(iso, mt.MonoIsotopicMass, mt.ChargeState);

            return iso;
        }

        #endregion


        private void CalculateMassesForIsotopicProfile(IsotopicProfile iso, double targetMonoMass, double targetChargeState)
        {
            if (iso?.Peaklist == null) return;

            for (var i = 0; i < iso.Peaklist.Count; i++)
            {
                var calcMZ = targetMonoMass / targetChargeState + Globals.PROTON_MASS + i * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / targetChargeState;
                iso.Peaklist[i].XValue = calcMZ;
            }

            iso.MonoPeakMZ = iso.Peaklist[0].XValue;
            iso.MonoIsotopicMass = (iso.MonoPeakMZ - Globals.PROTON_MASS) * targetChargeState;
        }

        public override void LoadRunRelatedInfo(Core.ResultCollection results)
        {
            //
        }


    }
}
