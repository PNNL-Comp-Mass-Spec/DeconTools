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
        readonly TomIsotopicPattern _TomIsotopicPatternGenerator = new TomIsotopicPattern();
        readonly IsotopicDistributionCalculator _isotopicDistributionCalculator = IsotopicDistributionCalculator.Instance;

        const int H_ISOTOPE_NUMBER = 1;
        const int D_ISOTOPE_NUMBER = 2;

        #region Constructors

        //default
        public DeuteriumIsotopeProfileGenerator()
        {
            var hLabelingAmount = 0.5;
            var dLabelingAmount = 0.5;
            var sumOfLabelingAmounts = (decimal)(Math.Round(hLabelingAmount, 2) + Math.Round(dLabelingAmount, 2));

            Check.Require(sumOfLabelingAmounts == 1.00m, "H and D labeling amounts do not add up to 1.00 - which they should.");

            this.HLabelingAmount = hLabelingAmount;
            this.DLabelingAmount = dLabelingAmount;
        }

        public DeuteriumIsotopeProfileGenerator(double hLabelingAmount, double dLabelingAmount)
        {
            var sumOfLabelingAmounts = (decimal)(Math.Round(hLabelingAmount, 2) + Math.Round(dLabelingAmount, 2));

            Check.Require(sumOfLabelingAmounts == 1.00m, "H and D labeling amounts do not add up to 1.00 - which they should.");

            this.HLabelingAmount = hLabelingAmount;
            this.DLabelingAmount = dLabelingAmount;
        }

        #endregion

        #region Properties

        /// <summary>
        /// fractional amount (0-1) for amount of H labeling
        /// </summary>
        public double HLabelingAmount { get; set; }

        /// <summary>
        /// fractional amount (0-1) for amount of D labeling
        /// </summary>
        public double DLabelingAmount { get; set; }

        #endregion

        #region Public Methods

        public IsotopicProfile GetDHIsotopicProfile(TargetBase mt, double lowpeakCutoff)
        {
            Check.Require(mt != null, "Mass tag not defined");
            Check.Require(mt.IsotopicProfile != null, "Mass tag's theor isotopic profile not defined");
            Check.Require(mt.ChargeState != 0, "Can't have a charge state of '0'");

            //int numNitrogens = mt.GetAtomCountForElement("N");
            var numDeuterium = 1;

            var labeledTheorProfile = _TomIsotopicPatternGenerator.GetIsotopePattern(mt.EmpiricalFormula, _TomIsotopicPatternGenerator.aafIsos);
            addMZInfoToTheorProfile(mt.IsotopicProfile, labeledTheorProfile, numDeuterium, mt.ChargeState);

            PeakUtilities.TrimIsotopicProfile(labeledTheorProfile, lowpeakCutoff);

            labeledTheorProfile.ChargeState = mt.ChargeState;

            return labeledTheorProfile;
        }

        public IsotopicProfile GetDHIsotopicProfile2(TargetBase mt, double lowpeakCutoff, double fractionLabeling, double molarMixingofH)
        {
            Check.Require(mt != null, "Mass tag not defined");
            Check.Require(mt.IsotopicProfile != null, "Mass tag's theor isotopic profile not defined");
            Check.Require(mt.ChargeState != 0, "Can't have a charge state of '0'");

            //int numNitrogens = mt.GetAtomCountForElement("N");
            var numDeuterium = 0;

            //_isotopicDistributionCalculator.SetLabeling("H", H_ISOTOPE_NUMBER, this.HLabelingAmount, D_ISOTOPE_NUMBER, this.DLabelingAmount);
            var hydrogenTheoreticalProfile = _isotopicDistributionCalculator.GetIsotopePattern(mt.EmpiricalFormula);

            var deuteriumTheoreticalProfile = _isotopicDistributionCalculator.GetIsotopePattern(mt.EmpiricalFormula);

            HLabelingAmount = molarMixingofH;
            DLabelingAmount = 1 - molarMixingofH;

            //convert to floats
            var labelingAmountFraction = Convert.ToSingle(fractionLabeling);
            var HLabelingAmountMix = Convert.ToSingle(HLabelingAmount);
            var DLabelingAmountMix = Convert.ToSingle(DLabelingAmount);

            //initialization
            float maxHeightForNormalization = 0;
            if (hydrogenTheoreticalProfile.Peaklist.Count > 0)
            {
                maxHeightForNormalization = hydrogenTheoreticalProfile.Peaklist[0].Height * HLabelingAmountMix;
            }

            //add deuterated peaks as an offset index
            for (var i = 0; i < hydrogenTheoreticalProfile.Peaklist.Count; i++)
            {
                var peakH = hydrogenTheoreticalProfile.Peaklist[i];
                MSPeak peakD;
                if (i == 0) //initial peak where there is no D contribution
                {
                    peakD = new MSPeak(0);
                }
                else
                {
                    peakD = deuteriumTheoreticalProfile.Peaklist[i - 1];
                }

                var contributionH = peakH.Height * HLabelingAmountMix;
                var contributionD = (1 - labelingAmountFraction) * peakD.Height * DLabelingAmountMix + labelingAmountFraction * peakD.Height * DLabelingAmountMix;

                peakH.Height = contributionH + contributionD;

                //peakH.Height = peakH.Height + (1-Convert.ToSingle(fractionLabeling)) * peakD.Height +Convert.ToSingle(fractionLabeling) * peakD.Height;

                //find true hightes peak in combined distribusion
                if (peakH.Height > maxHeightForNormalization)
                {
                    maxHeightForNormalization = peakH.Height;
                }
            }

            //rename for clarity
            var labeledTheoreticalProfile = hydrogenTheoreticalProfile;

            //normalize to 1
            foreach (var peak in labeledTheoreticalProfile.Peaklist)
            {
                peak.Height /= maxHeightForNormalization;
            }

            //should be good up to here

            addMZInfoToTheorProfile(mt.IsotopicProfile, labeledTheoreticalProfile, numDeuterium, mt.ChargeState);//Keep this as the H mass?

            //_isotopicDistributionCalculator.ResetToUnlabeled();

            PeakUtilities.TrimIsotopicProfile(labeledTheoreticalProfile, lowpeakCutoff);

            labeledTheoreticalProfile.ChargeState = mt.ChargeState;

            return labeledTheoreticalProfile;
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
            var counter = 1;
            for (var i = numDeuteriums - 1; i >= 0; i--)
            {
                labeledTheorProfile.Peaklist[i].XValue = labeledTheorProfile.Peaklist[numDeuteriums].XValue - (double)counter * (1.003 / (double)chargeState);
                counter++;
            }

            //Assign m/z values to the right of the monoDMass
            counter = 1;
            for (var i = numDeuteriums + 1; i < labeledTheorProfile.Peaklist.Count; i++)
            {
                labeledTheorProfile.Peaklist[i].XValue = labeledTheorProfile.Peaklist[numDeuteriums].XValue + (double)counter * (1.003 / (double)chargeState);
                counter++;
            }
        }
        #endregion

    }
}
