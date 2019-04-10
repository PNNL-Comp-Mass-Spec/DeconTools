using System;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public class LabeledMultiPeakChromGeneratorTask : Task
    {
        readonly TomTheorFeatureGenerator _FeatureGenerator = new TomTheorFeatureGenerator();
        readonly N15IsotopeProfileGenerator _N15IsotopicProfileGenerator = new N15IsotopeProfileGenerator();


        #region Constructors
        public LabeledMultiPeakChromGeneratorTask()
            : this(3, 25)
        {

        }

        public LabeledMultiPeakChromGeneratorTask(int numPeakForGeneratingChrom, double toleranceInPPM)
        {
            NumPeaksForGeneratingChrom = numPeakForGeneratingChrom;
            ToleranceInPPM = toleranceInPPM;

        }
        #endregion

        #region Properties
        public int NumPeaksForGeneratingChrom { get; set; }
        public double ToleranceInPPM { get; set; }
        #endregion

        #region Private Methods
        #endregion

        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList.Run.CurrentMassTag != null, string.Format("{0} failed. Mass tags haven't been defined.", Name));
            if (resultList.Run.CurrentMassTag == null)
                return;

            resultList.ResultType = Globals.ResultType.N14N15_TARGETED_RESULT;

            _FeatureGenerator.GenerateTheorFeature(resultList.Run.CurrentMassTag);   //generate theor profile for unlabeled feature
            var labeledProfile = _N15IsotopicProfileGenerator.GetN15IsotopicProfile(resultList.Run.CurrentMassTag, 0.005);

            var chromExtractor = new IsotopicProfileMultiChromatogramExtractor(
                NumPeaksForGeneratingChrom, ToleranceInPPM);

            var massTagResult = resultList.CurrentTargetedResult;

            if (!(massTagResult is N14N15_TResult n14n15result))
            {
                throw new InvalidOperationException(string.Format("{0} failed. There was a problem with the Result type; massTagResult is not class N14N15_TResult ", Name));
            }

            n14n15result.UnlabeledPeakChromData = chromExtractor.GetChromatogramsForIsotopicProfilePeaks(resultList.MSPeakResultList, resultList.Run.CurrentMassTag.IsotopicProfile);
            n14n15result.LabeledPeakChromData = chromExtractor.GetChromatogramsForIsotopicProfilePeaks(resultList.MSPeakResultList, labeledProfile);



        }
    }
}
