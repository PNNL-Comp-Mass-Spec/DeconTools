using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Utilities;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution
{
    public class DeuteriumIsotopeProfileGenerator
    {
        TomIsotopicPattern _TomIsotopicPatternGenerator = new TomIsotopicPattern();
        IsotopicDistributionCalculator _isotopicDistributionCalculator = IsotopicDistributionCalculator.Instance;

        const int H_ISOTOPE_NUMBER = 1;
        const int D_ISOTOPE_NUMBER = 2;

        #region Constructors

        public DeuteriumIsotopeProfileGenerator(double hLabelingAmount = 0.5, double dLabelingAmount = 0.5)
        {
            decimal sumOfLabelingAmounts = (decimal)(Math.Round(hLabelingAmount, 2) + Math.Round(dLabelingAmount, 2));

            Check.Require(sumOfLabelingAmounts == 1.00m, "H and D labelling amounts do not add up to 1.00 - which they should.");

            this.HLabellingAmount = hLabelingAmount;
            this.DLabellingAmount = dLabelingAmount;
        }

        #endregion

        #region Properties

        /// <summary>
        /// fractional amount (0-1) for amount of H labelling
        /// </summary>
        public double HLabellingAmount { get; set; }

        /// <summary>
        /// fractional amount (0-1) for amount of D labelling
        /// </summary>
        public double DLabellingAmount { get; set; }

        #endregion


        #region Public Methods

        public IsotopicProfile GetDHIsotopicProfile(TargetBase mt, double lowpeakCutoff)
        {
            Check.Require(mt != null, "Mass tag not defined");
            Check.Require(mt.IsotopicProfile != null, "Mass tag's theor isotopic profile not defined");
            Check.Require(mt.ChargeState != 0, "Can't have a charge state of '0'");

            //int numNitrogens = mt.GetAtomCountForElement("N");
            int numDeuterium = 1;

            IsotopicProfile labeledTheorProfile = _TomIsotopicPatternGenerator.GetIsotopePattern(mt.EmpiricalFormula, _TomIsotopicPatternGenerator.aafIsos);
            addMZInfoToTheorProfile(mt.IsotopicProfile, labeledTheorProfile, numDeuterium, mt.ChargeState);

            PeakUtilities.TrimIsotopicProfile(labeledTheorProfile, lowpeakCutoff);

            labeledTheorProfile.ChargeState = mt.ChargeState;

            return labeledTheorProfile;

        }



        public IsotopicProfile GetDHIsotopicProfile2(TargetBase mt, double lowpeakCutoff, double fractionLabeling)
        {
            Check.Require(mt != null, "Mass tag not defined");
            Check.Require(mt.IsotopicProfile != null, "Mass tag's theor isotopic profile not defined");
            Check.Require(mt.ChargeState != 0, "Can't have a charge state of '0'");

            //int numNitrogens = mt.GetAtomCountForElement("N");
            int numDeuterium = 1;

            _isotopicDistributionCalculator.SetLabeling("H", H_ISOTOPE_NUMBER, this.HLabellingAmount, D_ISOTOPE_NUMBER, this.DLabellingAmount);
            IsotopicProfile labeledTheorProfile = _isotopicDistributionCalculator.GetIsotopePattern(mt.EmpiricalFormula);
            addMZInfoToTheorProfile(mt.IsotopicProfile, labeledTheorProfile, numDeuterium, mt.ChargeState);

            _isotopicDistributionCalculator.ResetToUnlabeled();

            PeakUtilities.TrimIsotopicProfile(labeledTheorProfile, lowpeakCutoff);

            labeledTheorProfile.ChargeState = mt.ChargeState;

            return labeledTheorProfile;

        }


        #endregion

        #region Private Methods

        private void addMZInfoToTheorProfile(IsotopicProfile unlabeledProfile, IsotopicProfile labeledTheorProfile, int numDeuteriums, int chargeState)
        {
            if (labeledTheorProfile.Peaklist == null || labeledTheorProfile.Peaklist.Count < 3) return;

            //assign the baseD mass
            labeledTheorProfile.MonoPeakMZ = unlabeledProfile.Peaklist[0].XValue + (Globals.Deuterium_MASS - Globals.Hydrogen_MASS) * (double)numDeuteriums / (double)chargeState;

            labeledTheorProfile.Peaklist[numDeuteriums].XValue = labeledTheorProfile.MonoPeakMZ;
            labeledTheorProfile.MonoIsotopicMass = (labeledTheorProfile.MonoPeakMZ - Globals.PROTON_MASS) * chargeState;




            //Assign m/z values to the left of the monoDMass
            int counter = 1;
            for (int i = numDeuteriums - 1; i >= 0; i--)
            {
                labeledTheorProfile.Peaklist[i].XValue = labeledTheorProfile.Peaklist[numDeuteriums].XValue - (double)counter * (1.003 / (double)chargeState);
                counter++;
            }

            //Assign m/z values to the right of the monoDMass
            counter = 1;
            for (int i = numDeuteriums + 1; i < labeledTheorProfile.Peaklist.Count; i++)
            {
                labeledTheorProfile.Peaklist[i].XValue = labeledTheorProfile.Peaklist[numDeuteriums].XValue + (double)counter * (1.003 / (double)chargeState);
                counter++;
            }




        }
        #endregion

    }
}
