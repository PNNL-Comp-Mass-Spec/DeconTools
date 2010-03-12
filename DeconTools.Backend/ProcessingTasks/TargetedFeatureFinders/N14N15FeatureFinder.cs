using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Algorithms;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public class N14N15FeatureFinder : ITargetedFeatureFinder
    {
        #region Constructors
        public N14N15FeatureFinder()
            : this(0.02)
        {

        }

        public N14N15FeatureFinder(double toleranceInMZ)
        {
            this.Tolerance = toleranceInMZ;

        }

        #endregion

        #region Properties

        public IsotopicProfile LabeledTheorProfile { get; set; }
        #endregion

        #region Public Methods
        public override void FindFeature(DeconTools.Backend.Core.ResultCollection resultColl)
        {
            resultColl.MassTagResultType = Globals.MassTagResultType.N14N15_MASSTAG_RESULT;

            base.FindFeature(resultColl);

            MassTagResultBase massTagresult = resultColl.GetMassTagResult(resultColl.Run.CurrentMassTag);



            //if (result == null)
            //{
            //    result = resultColl.CreateMassTagResult(resultColl.Run.CurrentMassTag);
            //}


            // Generate Theor Feature

            //getN15FormulaAsIntArray(resultColl.Run.CurrentMassTag, 0.95d);
            resultColl.Run.CurrentMassTag.CreatePeptideObject();
            int numNitrogens = resultColl.Run.CurrentMassTag.Peptide.GetElementQuantity("N");

            LabeledTheorProfile = TomIsotopicPattern.GetIsotopePattern(resultColl.Run.CurrentMassTag.GetEmpiricalFormulaAsIntArray(), TomIsotopicPattern.aafN15Isos);
            addMZInfoToTheorProfile(LabeledTheorProfile, numNitrogens, resultColl.Run.CurrentMassTag.ChargeState);
            PeakUtilities.TrimIsotopicProfile(LabeledTheorProfile, 0.005);

            BasicMSFeatureFinder bff = new BasicMSFeatureFinder();
            IsotopicProfile labelledIso = bff.FindMSFeature(resultColl.Run.PeakList, LabeledTheorProfile, Tolerance, false);

            massTagresult.AddLabelledIso(labelledIso);






        }

        private void addMZInfoToTheorProfile(IsotopicProfile LabeledTheorProfile, int numNitrogens, int chargeState)
        {
            if (LabeledTheorProfile.Peaklist == null || LabeledTheorProfile.Peaklist.Count < 3) return;

            //assign the baseN15 mass
            LabeledTheorProfile.Peaklist[numNitrogens].XValue = TheorFeature.Peaklist[0].XValue + (Globals.N15_MASS - Globals.N14_MASS) * (double)numNitrogens / (double)chargeState;

            //Assign m/z values to the left of the monoN15Mass
            int counter = 1;
            for (int i = numNitrogens - 1; i >= 0; i--)
            {
                LabeledTheorProfile.Peaklist[i].XValue = LabeledTheorProfile.Peaklist[numNitrogens].XValue - (double)counter * (1.003 / (double)chargeState);
                counter++;
            }

            //Assign m/z values to the right of the monoN15Mass
            counter = 1;
            for (int i = numNitrogens + 1; i < LabeledTheorProfile.Peaklist.Count; i++)
            {
                LabeledTheorProfile.Peaklist[i].XValue = LabeledTheorProfile.Peaklist[numNitrogens].XValue + (double)counter * (1.003 / (double)chargeState);
                counter++;
            }




        }

        private void getN15FormulaAsIntArray(MassTag massTag, double p)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
