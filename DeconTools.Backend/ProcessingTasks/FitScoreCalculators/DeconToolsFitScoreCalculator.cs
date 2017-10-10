using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;

namespace DeconTools.Backend.ProcessingTasks.FitScoreCalculators
{
    public class DeconToolsFitScoreCalculator : IFitScoreCalculator
    {
        readonly TomIsotopicPattern _tomIsotopicPatternGenerator = new TomIsotopicPattern();


        public double MZVar { get; set; }
        public IsotopicProfile TheorIsotopicProfile { get; set; }

        public DeconToolsFitScoreCalculator()
        {

        }

        public DeconToolsFitScoreCalculator(double mzVar)
            : this()
        {
            MZVar = mzVar;
        }


        public override void GetFitScores(IEnumerable<IsosResult> isosResults)
        {

            foreach (var result in isosResults)
            {
                //create a temporary mass tag, as a data object for storing relevent info, and using the CalculateMassesForIsotopicProfile() method.
                var mt = new PeptideTarget
                {
                    ChargeState = (short)result.IsotopicProfile.ChargeState,
                    MonoIsotopicMass = result.IsotopicProfile.MonoIsotopicMass
                };

                mt.MZ = (mt.MonoIsotopicMass / mt.ChargeState) + Globals.PROTON_MASS;

                //TODO: use Josh's isotopicDistribution calculator after confirming averagine formula
                mt.EmpiricalFormula = _tomIsotopicPatternGenerator.GetClosestAvnFormula(result.IsotopicProfile.MonoIsotopicMass, false);

                mt.IsotopicProfile = _tomIsotopicPatternGenerator.GetIsotopePattern(mt.EmpiricalFormula, _tomIsotopicPatternGenerator.aafIsos);
                TheorIsotopicProfile = mt.IsotopicProfile;

                mt.CalculateMassesForIsotopicProfile(mt.ChargeState);

                var theorXYData = mt.IsotopicProfile.GetTheoreticalIsotopicProfileXYData(result.IsotopicProfile.GetFWHM());

                offsetDistribution(theorXYData, mt.IsotopicProfile, result.IsotopicProfile);

                // Obsolete Classwide variable: MercuryDistributionCreator distributionCreator;
                //
                //double resolution = result.IsotopicProfile.GetMZofMostAbundantPeak() / result.IsotopicProfile.GetFWHM();
                //distributionCreator.CreateDistribution(result.IsotopicProfile.MonoIsotopicMass, result.IsotopicProfile.ChargeState, resolution);
                //distributionCreator.OffsetDistribution(result.IsotopicProfile);
                //XYData theorXYData = distributionCreator.Data;

                var areafitter = new AreaFitter();
                var fitval = areafitter.GetFit(theorXYData, result.Run.XYData, 0.1, out var _);

                if (double.IsNaN(fitval) || fitval > 1) fitval = 1;

                result.IsotopicProfile.Score = fitval;


            }
        }

        private void offsetDistribution(XYData theorXYData, IsotopicProfile theorIsotopicProfile, IsotopicProfile observedIsotopicProfile)
        {
            double offset;
            if (theorIsotopicProfile?.Peaklist == null || theorIsotopicProfile.Peaklist.Count == 0) return;

            var mostIntensePeak = theorIsotopicProfile.getMostIntensePeak();
            var indexOfMostIntensePeak = theorIsotopicProfile.Peaklist.IndexOf(mostIntensePeak);

            if (observedIsotopicProfile.Peaklist == null || observedIsotopicProfile.Peaklist.Count == 0) return;

            var enoughPeaksInTarget = (indexOfMostIntensePeak <= observedIsotopicProfile.Peaklist.Count - 1);

            if (enoughPeaksInTarget)
            {
                var targetPeak = observedIsotopicProfile.Peaklist[indexOfMostIntensePeak];
                offset = targetPeak.XValue - mostIntensePeak.XValue;
                //offset = observedIsotopicProfile.Peaklist[0].XValue - theorIsotopicProfile.Peaklist[0].XValue;   //want to test to see if Thrash is same as rapid

            }
            else
            {
                offset = observedIsotopicProfile.Peaklist[0].XValue - theorIsotopicProfile.Peaklist[0].XValue;
            }

            for (var i = 0; i < theorXYData.Xvalues.Length; i++)
            {
                theorXYData.Xvalues[i] = theorXYData.Xvalues[i] + offset;
            }

            foreach (var peak in theorIsotopicProfile.Peaklist)
            {
                peak.XValue = peak.XValue + offset;

            }


        }


    }
}
