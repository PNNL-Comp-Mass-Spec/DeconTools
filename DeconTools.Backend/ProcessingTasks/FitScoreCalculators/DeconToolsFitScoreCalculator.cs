using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;



namespace DeconTools.Backend.ProcessingTasks.FitScoreCalculators
{
    public class DeconToolsFitScoreCalculator : IFitScoreCalculator
    {
        MercuryDistributionCreator distributionCreator;

        PeakUtilities peakUtil = new PeakUtilities();

        public double MZVar { get; set; }
        public IsotopicProfile TheorIsotopicProfile { get; set; }

        public DeconToolsFitScoreCalculator()
        {
            this.distributionCreator = new MercuryDistributionCreator();
        }

        public DeconToolsFitScoreCalculator(double mzVar)
            : this()
        {
            this.MZVar = mzVar;
        }


        public override void GetFitScores(ResultCollection resultList)
        {
            Check.Require(resultList != null, "FitScoreCalculator failed. ResultCollection is null");
            Check.Require(resultList.Run != null, "FitScoreCalculator failed. Run is null");
            Check.Require(resultList.Run.CurrentScanSet != null, "FitScoreCalculator failed. Current scanset has not been defined");


            foreach (IsosResult result in resultList.IsosResultBin)
            {
                //create a temporary mass tag, as a data object for storing relevent info, and using the CalculateMassesForIsotopicProfile() method. 
                MassTag mt = new MassTag();

                mt.ChargeState = (short)result.IsotopicProfile.ChargeState;
                mt.MonoIsotopicMass = result.IsotopicProfile.MonoIsotopicMass;
                mt.MZ = (mt.MonoIsotopicMass / mt.ChargeState) + Globals.PROTON_MASS;


                int[] empircalFormulaAsIntArray = TomIsotopicPattern.GetClosestAvnFormula(result.IsotopicProfile.MonoIsotopicMass, false);



                mt.IsotopicProfile = TomIsotopicPattern.GetIsotopePattern(empircalFormulaAsIntArray, TomIsotopicPattern.aafIsos);
                this.TheorIsotopicProfile = mt.IsotopicProfile;

                mt.CalculateMassesForIsotopicProfile(mt.ChargeState);

                XYData theorXYData = TheorXYDataCalculationUtilities.Get_Theoretical_IsotopicProfileXYData(mt.IsotopicProfile, result.IsotopicProfile.GetFWHM());

                offsetDistribution(theorXYData, mt.IsotopicProfile, result.IsotopicProfile);

                //double resolution = result.IsotopicProfile.GetMZofMostAbundantPeak() / result.IsotopicProfile.GetFWHM();
                //distributionCreator.CreateDistribution(result.IsotopicProfile.MonoIsotopicMass, result.IsotopicProfile.ChargeState, resolution);
                //distributionCreator.OffsetDistribution(result.IsotopicProfile);
                //XYData theorXYData = distributionCreator.Data;

                AreaFitter areafitter = new AreaFitter(theorXYData, result.Run.XYData, 0.1);
                double fitval = areafitter.getFit();

                if (fitval == double.NaN || fitval > 1) fitval = 1;

                result.IsotopicProfile.Score = fitval;


            }
        }

        private void offsetDistribution(XYData theorXYData, IsotopicProfile theorIsotopicProfile, IsotopicProfile observedIsotopicProfile)
        {
            double offset = 0;


            if (theorIsotopicProfile == null || theorIsotopicProfile.Peaklist == null || theorIsotopicProfile.Peaklist.Count == 0) return;

            MSPeak mostIntensePeak = theorIsotopicProfile.getMostIntensePeak();
            int indexOfMostIntensePeak = theorIsotopicProfile.Peaklist.IndexOf(mostIntensePeak);

            if (observedIsotopicProfile.Peaklist == null || observedIsotopicProfile.Peaklist.Count == 0) return;

            bool enoughPeaksInTarget = (indexOfMostIntensePeak <= observedIsotopicProfile.Peaklist.Count - 1);

            if (enoughPeaksInTarget)
            {
                MSPeak targetPeak = observedIsotopicProfile.Peaklist[indexOfMostIntensePeak];
                offset = targetPeak.XValue - mostIntensePeak.XValue;
                //offset = observedIsotopicProfile.Peaklist[0].XValue - theorIsotopicProfile.Peaklist[0].XValue;   //want to test to see if Thrash is same as rapid

            }
            else
            {
                offset = observedIsotopicProfile.Peaklist[0].XValue - theorIsotopicProfile.Peaklist[0].XValue;
            }

            for (int i = 0; i < theorXYData.Xvalues.Length; i++)
            {
                theorXYData.Xvalues[i] = theorXYData.Xvalues[i] + offset;
            }

            foreach (var peak in theorIsotopicProfile.Peaklist)
            {
                peak.XValue = peak.XValue + offset;
                
            }


        }

        public override void Cleanup()
        {
            base.Cleanup();

        }



    }
}
