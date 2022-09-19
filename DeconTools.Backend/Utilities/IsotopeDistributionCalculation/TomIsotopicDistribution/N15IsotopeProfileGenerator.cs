using DeconTools.Backend.Core;
using DeconTools.Utilities;
using System;

namespace DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution
{
    public class N15IsotopeProfileGenerator
    {
        TomIsotopicPattern _TomIsotopicPatternGenerator = new TomIsotopicPattern();
        IsotopicDistributionCalculator _isotopicDistributionCalculator = IsotopicDistributionCalculator.Instance;

        const int N14ISOTOPE_NUMBER = 14;
        const int N15ISOTOPE_NUMBER = 15;

        #region Constructors

        public N15IsotopeProfileGenerator(double N14LabelingAmount = 0.02, double N15LabelingAmount = 0.98)
        {
            var sumOfLabelingAmounts = (decimal)(Math.Round(N14LabelingAmount, 2) + Math.Round(N15LabelingAmount, 2));

            Check.Require(sumOfLabelingAmounts == 1.00m, "N14 and N15 labeling amounts do not add up to 1.00 - which they should.");

            this.N14LabelingAmount = N14LabelingAmount;
            this.N15LabelingAmount = N15LabelingAmount;
        }

        #endregion

        #region Properties

        /// <summary>
        /// fractional amount (0-1) for amount of N14 labeling
        /// </summary>
        public double N14LabelingAmount { get; set; }

        /// <summary>
        /// fractional amount (0-1) for amount of N15 labeling
        /// </summary>
        public double N15LabelingAmount { get; set; }

        #endregion

        #region Public Methods

        public IsotopicProfile GetN15IsotopicProfile(TargetBase mt, double lowpeakCutoff)
        {
            Check.Require(mt != null, "Mass tag not defined");
            Check.Require(mt.IsotopicProfile != null, "Mass tag's theor isotopic profile not defined");
            Check.Require(mt.ChargeState != 0, "Can't have a charge state of '0'");

            var numNitrogens = mt.GetAtomCountForElement("N");

            var labeledTheorProfile = _TomIsotopicPatternGenerator.GetIsotopePattern(mt.EmpiricalFormula, _TomIsotopicPatternGenerator.aafN15Isos);
            addMZInfoToTheorProfile(mt.IsotopicProfile, labeledTheorProfile, numNitrogens, mt.ChargeState);

            PeakUtilities.TrimIsotopicProfile(labeledTheorProfile, lowpeakCutoff);

            labeledTheorProfile.ChargeState = mt.ChargeState;

            return labeledTheorProfile;
        }

        public IsotopicProfile GetN15IsotopicProfile2(TargetBase mt, double lowPeakCutoff)
        {
            Check.Require(mt != null, "Mass tag not defined");
            Check.Require(mt.IsotopicProfile != null, "Mass tag's theor isotopic profile not defined");
            Check.Require(mt.ChargeState != 0, "Can't have a charge state of '0'");

            var nitrogenCount = mt.GetAtomCountForElement("N");

            _isotopicDistributionCalculator.SetLabeling("N", N14ISOTOPE_NUMBER, this.N14LabelingAmount, N15ISOTOPE_NUMBER, this.N15LabelingAmount);
            var labeledTheorProfile = _isotopicDistributionCalculator.GetIsotopePattern(mt.EmpiricalFormula);
            addMZInfoToTheorProfile(mt.IsotopicProfile, labeledTheorProfile, nitrogenCount, mt.ChargeState);

            _isotopicDistributionCalculator.ResetToUnlabeled();

            PeakUtilities.TrimIsotopicProfile(labeledTheorProfile, lowPeakCutoff);

            labeledTheorProfile.ChargeState = mt.ChargeState;

            return labeledTheorProfile;
        }

        #endregion

        #region Private Methods

        private void addMZInfoToTheorProfile(IsotopicProfile unlabeledProfile, IsotopicProfile labeledTheorProfile, int numNitrogens, int chargeState)
        {
            if (labeledTheorProfile.Peaklist == null || labeledTheorProfile.Peaklist.Count < 3)
            {
                return;
            }

            //assign the baseN15 mass
            labeledTheorProfile.MonoPeakMZ = unlabeledProfile.Peaklist[0].XValue + (Globals.N15_MASS - Globals.N14_MASS) * (double)numNitrogens / (double)chargeState;

            labeledTheorProfile.Peaklist[numNitrogens].XValue = labeledTheorProfile.MonoPeakMZ;
            labeledTheorProfile.MonoIsotopicMass = (labeledTheorProfile.MonoPeakMZ - Globals.PROTON_MASS) * chargeState;

            //Assign m/z values to the left of the monoN15Mass
            var counter = 1;
            for (var i = numNitrogens - 1; i >= 0; i--)
            {
                labeledTheorProfile.Peaklist[i].XValue = labeledTheorProfile.Peaklist[numNitrogens].XValue - (double)counter * (1.003 / (double)chargeState);
                counter++;
            }

            //Assign m/z values to the right of the monoN15Mass
            counter = 1;
            for (var i = numNitrogens + 1; i < labeledTheorProfile.Peaklist.Count; i++)
            {
                labeledTheorProfile.Peaklist[i].XValue = labeledTheorProfile.Peaklist[numNitrogens].XValue + (double)counter * (1.003 / (double)chargeState);
                counter++;
            }
        }
        #endregion
    }
}
