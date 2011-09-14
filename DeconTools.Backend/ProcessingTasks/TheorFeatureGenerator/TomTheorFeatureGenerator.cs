using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator
{
    public class TomTheorFeatureGenerator : ITheorFeatureGenerator
    {
        #region Constructors
        public TomTheorFeatureGenerator()
            : this(Globals.LabellingType.NONE,0.005)
        {
            
        }

        public TomTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType labellingType, double lowPeakCutOff)
        {
            this.LabellingType = labellingType;
            this.LowPeakCutOff = lowPeakCutOff;
        }
        #endregion

        #region Properties

        public Globals.LabellingType LabellingType { get; set; }
        
        /// <summary>
        /// Peaks below the cutoff will be trimmed out from the theoretical profile
        /// </summary>
        public double LowPeakCutOff { get; set; }



        #endregion

        #region Public Methods
        public override void GenerateTheorFeature(MassTag mt)
        {
            Check.Require(mt != null, "FeatureGenerator failed. MassTag not defined.");
            Check.Require(mt.EmpiricalFormula != null, "Theoretical feature generator failed. Can't retrieve empirical formula from Mass Tag");

            switch (LabellingType)
            {
                case Globals.LabellingType.NONE:

                    mt.IsotopicProfile = GetUnlabelledIsotopicProfile(mt);
                    break;
                case Globals.LabellingType.O18:
                    throw new NotImplementedException();
                case Globals.LabellingType.N15:
                    mt.IsotopicProfileLabelled = N15IsotopeProfileGenerator.GetN15IsotopicProfile(mt, LowPeakCutOff);
                    
                    break;
                default:
                    break;
            }

        }

        private IsotopicProfile GetUnlabelledIsotopicProfile(MassTag mt)
        {

            IsotopicProfile iso = new IsotopicProfile();

            try
            {
                iso = TomIsotopicPattern.GetIsotopePattern(mt.GetEmpiricalFormulaAsIntArray(), TomIsotopicPattern.aafIsos);
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

        #region Private Methods
        #endregion


        private void calculateMassesForIsotopicProfile(IsotopicProfile iso, MassTag mt)
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


        public override void LoadRunRelatedInfo(ResultCollection results)
        {
            // do nothing
        }
    }
}
