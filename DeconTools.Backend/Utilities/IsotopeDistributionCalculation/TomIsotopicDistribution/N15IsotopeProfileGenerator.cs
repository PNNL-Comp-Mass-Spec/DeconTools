using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution
{
    public class N15IsotopeProfileGenerator
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods

        public static IsotopicProfile GetN15IsotopicProfile(MassTag mt, double lowpeakCutoff)
        {
            Check.Require(mt != null, "Mass tag not defined");
            Check.Require(mt.IsotopicProfile != null, "Mass tag's theor isotopic profile not defined");
            Check.Require(mt.ChargeState != 0, "Can't have a charge state of '0'");


            if (mt.Peptide == null) mt.CreatePeptideObject();

            int numNitrogens = mt.Peptide.GetElementQuantity("N");

            IsotopicProfile labeledTheorProfile = TomIsotopicPattern.GetIsotopePattern(mt.GetEmpiricalFormulaAsIntArray(), TomIsotopicPattern.aafN15Isos);
            addMZInfoToTheorProfile(mt.IsotopicProfile,labeledTheorProfile, numNitrogens, mt.ChargeState);
            PeakUtilities.TrimIsotopicProfile(labeledTheorProfile, lowpeakCutoff);
            
            labeledTheorProfile.ChargeState = mt.ChargeState;

            return labeledTheorProfile;

        }


        #endregion

        #region Private Methods

        private static void addMZInfoToTheorProfile(IsotopicProfile unlabeledProfile, IsotopicProfile labeledTheorProfile, int numNitrogens, int chargeState)
        {
            if (labeledTheorProfile.Peaklist == null || labeledTheorProfile.Peaklist.Count < 3) return;

            //assign the baseN15 mass
            labeledTheorProfile.Peaklist[numNitrogens].XValue = unlabeledProfile.Peaklist[0].XValue + (Globals.N15_MASS - Globals.N14_MASS) * (double)numNitrogens / (double)chargeState;

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
