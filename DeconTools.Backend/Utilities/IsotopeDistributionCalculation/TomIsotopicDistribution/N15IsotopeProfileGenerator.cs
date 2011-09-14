﻿using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution
{
    public class N15IsotopeProfileGenerator
    {

        TomIsotopicPattern _isotopicPatternGenerator = new TomIsotopicPattern();

          #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods

        public IsotopicProfile GetN15IsotopicProfile(TargetBase mt, double lowpeakCutoff)
        {
            Check.Require(mt != null, "Mass tag not defined");
            Check.Require(mt.IsotopicProfile != null, "Mass tag's theor isotopic profile not defined");
            Check.Require(mt.ChargeState != 0, "Can't have a charge state of '0'");

            int numNitrogens = mt.GetAtomCountForElement("N");

            IsotopicProfile labeledTheorProfile = _isotopicPatternGenerator.GetIsotopePattern(mt.EmpiricalFormula, _isotopicPatternGenerator.aafN15Isos);
            addMZInfoToTheorProfile(mt.IsotopicProfile,labeledTheorProfile, numNitrogens, mt.ChargeState);
            
            PeakUtilities.TrimIsotopicProfile(labeledTheorProfile, lowpeakCutoff);

            labeledTheorProfile.ChargeState = mt.ChargeState;

            return labeledTheorProfile;

        }


        #endregion

        #region Private Methods

        private void addMZInfoToTheorProfile(IsotopicProfile unlabeledProfile, IsotopicProfile labeledTheorProfile, int numNitrogens, int chargeState)
        {
            if (labeledTheorProfile.Peaklist == null || labeledTheorProfile.Peaklist.Count < 3) return;

            //assign the baseN15 mass
            labeledTheorProfile.MonoPeakMZ = unlabeledProfile.Peaklist[0].XValue + (Globals.N15_MASS - Globals.N14_MASS) * (double)numNitrogens / (double)chargeState;

            labeledTheorProfile.Peaklist[numNitrogens].XValue = labeledTheorProfile.MonoPeakMZ;
            labeledTheorProfile.MonoIsotopicMass = (labeledTheorProfile.MonoPeakMZ - Globals.PROTON_MASS) * chargeState;




            //Assign m/z values to the left of the monoN15Mass
            int counter = 1;
            for (int i = numNitrogens - 1; i >= 0; i--)
            {
                labeledTheorProfile.Peaklist[i].XValue = labeledTheorProfile.Peaklist[numNitrogens].XValue - (double)counter * (1.003 / (double)chargeState);
                counter++;
            }

            //Assign m/z values to the right of the monoN15Mass
            counter = 1;
            for (int i = numNitrogens + 1; i < labeledTheorProfile.Peaklist.Count; i++)
            {
                labeledTheorProfile.Peaklist[i].XValue = labeledTheorProfile.Peaklist[numNitrogens].XValue + (double)counter * (1.003 / (double)chargeState);
                counter++;
            }




        }
        #endregion
    }
}
