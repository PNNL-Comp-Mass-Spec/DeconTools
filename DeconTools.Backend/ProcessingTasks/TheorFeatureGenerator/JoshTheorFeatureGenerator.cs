using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator
{
    public class JoshTheorFeatureGenerator:ITheorFeatureGenerator
    {
        IsotopicDistributionCalculator _isotopicDistCalculator = IsotopicDistributionCalculator.Instance;
        N15IsotopeProfileGenerator _N15IsotopicProfileGenerator = new N15IsotopeProfileGenerator();
        DeuteriumIsotopeProfileGenerator _DeuteriumIsotopicProfileGenerator = new DeuteriumIsotopeProfileGenerator();

        #region Constructors
        public JoshTheorFeatureGenerator()
            : this(Globals.LabellingType.NONE, 0.005)
        {


        }

        public JoshTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType labellingType, double lowPeakCutOff)
        {
            this.LabellingType = labellingType;
            this.LowPeakCutOff = lowPeakCutOff;
        }

        public JoshTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType labellingType, double fractionLabeling, double lowPeakCutOff)
        {
            this.LabellingType = labellingType;
            this.LowPeakCutOff = lowPeakCutOff;
            this.FractionLabeling = fractionLabeling;
        }

        #endregion

        #region Properties
        public Globals.LabellingType LabellingType { get; set; }

        /// <summary>
        /// Degree of labeling. Ranges between 0 and 1.0
        /// </summary>
        public double FractionLabeling { get; set; }

        public double LowPeakCutOff { get; set; }

        #endregion

        #region Public Methods
        public override void GenerateTheorFeature(TargetBase mt)
        {
            Check.Require(mt != null, "FeatureGenerator failed. MassTag not defined.");
            Check.Require(!String.IsNullOrEmpty(mt.EmpiricalFormula), "Theoretical feature generator failed. Can't retrieve empirical formula from Mass Tag");

            switch (LabellingType)
            {
                case Globals.LabellingType.NONE:

                    mt.IsotopicProfile = GetUnlabelledIsotopicProfile(mt);
                    break;
                case Globals.LabellingType.O18:
                    throw new NotImplementedException();
                case Globals.LabellingType.N15:
                    
                    mt.IsotopicProfileLabelled = _N15IsotopicProfileGenerator.GetN15IsotopicProfile2(mt, LowPeakCutOff);

                    break;
                case Globals.LabellingType.Deuterium:
                    
                    mt.IsotopicProfile = GetUnlabelledIsotopicProfile(mt);
                    mt.IsotopicProfileLabelled = _DeuteriumIsotopicProfileGenerator.GetDHIsotopicProfile2(mt, LowPeakCutOff, FractionLabeling);
                    break;
                default:
                    throw new NotImplementedException();
                    break;
            }

        }
        #endregion

        #region Private Methods
        private IsotopicProfile GetUnlabelledIsotopicProfile(TargetBase mt)
        {

            IsotopicProfile iso = new IsotopicProfile();

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

         
            if (iso.ChargeState != 0) calculateMassesForIsotopicProfile(iso, mt);

            return iso;

            
        }

        #endregion


        private void calculateMassesForIsotopicProfile(IsotopicProfile iso, TargetBase mt)
        {
            if (iso == null || iso.Peaklist == null) return;

            



            for (int i = 0; i < iso.Peaklist.Count; i++)
            {
                double calcMZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS + i * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / mt.ChargeState;
                iso.Peaklist[i].XValue = calcMZ;
            }

            iso.MonoPeakMZ = iso.Peaklist[0].XValue;
            iso.MonoIsotopicMass = (iso.MonoPeakMZ - Globals.PROTON_MASS) * mt.ChargeState;

        }

        public override void LoadRunRelatedInfo(Core.ResultCollection results)
        {
            //
        }

      
    }
}
