using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.FitScoreCalculators
{
    public class IsotopicPeakFitScoreCalculator : Task
    {
        #region properties
        /// <summary>
        /// score experimental profile to labeled or unlabeled theoretical profile
        /// </summary>
        public Globals.IsotopicProfileType IsotopicProfileType { get; set; }

        /// <summary>
        /// Penalize FitScore  based on this any peaks to the left of the monoisotopic peak.  Zeroes need to be added to the theoretical isotope profile
        /// </summary>
        public int NumberOfPeaksToLeftForPenalty { get; set; }

        public IsotopicPeakFitScoreCalculator()
        {
            IsotopicProfileType = Globals.IsotopicProfileType.UNLABELED;
            NumberOfPeaksToLeftForPenalty = 0;
        }

        public IsotopicPeakFitScoreCalculator(Globals.IsotopicProfileType labelType, int numberOfPeaksToLeftForPenalty)
        {
            IsotopicProfileType = labelType;
            NumberOfPeaksToLeftForPenalty = numberOfPeaksToLeftForPenalty;
        }

        public IsotopicPeakFitScoreCalculator(int numberOfPeaksToLeftForPenalty = 0)
        {
            IsotopicProfileType = Globals.IsotopicProfileType.UNLABELED;
            NumberOfPeaksToLeftForPenalty = numberOfPeaksToLeftForPenalty;
        }

        #endregion

        #region Public Methods
        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList.Run.CurrentMassTag != null, this.Name + " failed; CurrentMassTag is empty");
            if (resultList.Run.CurrentMassTag == null)
            {
                return;
            }

            Check.Require(resultList.Run.XYData?.Xvalues != null && resultList.Run.XYData.Xvalues.Length > 0, Name + " failed; Run's XY data is empty. Need to Run an MSGenerator");
            Check.Require(resultList.CurrentTargetedResult != null, "No MassTagResult has been generated for CurrentMassTag");
            if (resultList.CurrentTargetedResult == null)
            {
                return;
            }

            IsotopicProfile theorProfile;
            switch (IsotopicProfileType)
            {
                case Globals.IsotopicProfileType.UNLABELED:
                    Check.Require(resultList.Run.CurrentMassTag.IsotopicProfile != null, "Target's theoretical isotopic profile has not been established");
                    theorProfile = resultList.Run.CurrentMassTag.IsotopicProfile;
                    break;
                case Globals.IsotopicProfileType.LABELED:
                    //Check.Require(resultList.Run.CurrentMassTag.IsotopicProfileLabeled != null, this.Name + " failed; Theor isotopic profile is empty. Run a TheorFeatureGenerator");
                    Check.Require(resultList.Run.CurrentMassTag.IsotopicProfileLabeled != null, "Target's labeled theoretical isotopic profile has not been established");
                    theorProfile = resultList.Run.CurrentMassTag.IsotopicProfileLabeled;
                    break;
                default:
                    theorProfile = resultList.Run.CurrentMassTag.IsotopicProfile;
                    break;
            }

            resultList.CurrentTargetedResult.Score = CalculateFitScore(theorProfile, resultList.CurrentTargetedResult.IsotopicProfile, resultList.Run.XYData, NumberOfPeaksToLeftForPenalty);
        }
        #endregion

        public double CalculateFitScore(IsotopicProfile theorProfile, IsotopicProfile observedProfile, XYData massSpecXYData, int numberOfPeaksToLeftForPenalty = 0, double massErrorPPMBetweenPeaks = 15)
        {
            if (observedProfile?.Peaklist == null || observedProfile.Peaklist.Count == 0)
            {
                return 1.0;   // this is the worst possible fit score. ( 0.000 is the best possible fit score);  Maybe we want to return a '-1' to indicate a failure...
            }

            var indexOfMostAbundantTheorPeak = theorProfile.GetIndexOfMostIntensePeak();
            var indexOfCorrespondingObservedPeak = PeakUtilities.getIndexOfClosestValue(observedProfile.Peaklist, theorProfile.getMostIntensePeak().XValue, 0, observedProfile.Peaklist.Count - 1, 0.1);

            if (indexOfCorrespondingObservedPeak < 0)      // most abundant peak isn't present in the actual theoretical profile... problem!
            {
                return 1.0;
            }

            var mzOffset = observedProfile.Peaklist[indexOfCorrespondingObservedPeak].XValue - theorProfile.Peaklist[indexOfMostAbundantTheorPeak].XValue;

            var observedPeakList = observedProfile.Peaklist.Cast<Peak>().ToList();
            var theorPeakList = theorProfile.Peaklist.Cast<Peak>().ToList();

            foreach (var peak in theorPeakList)//May want to avoid this offset if the masses have been aligned using LCMS Warp
            {
                peak.XValue += mzOffset;
            }

            const double minCutoffTheorPeakIntensityFraction = 0.1;

            var peakFitter = new PeakLeastSquaresFitter();

            var fitVal = peakFitter.GetFit(
                theorPeakList,
                observedPeakList,
                minCutoffTheorPeakIntensityFraction,
                massErrorPPMBetweenPeaks,
                numberOfPeaksToLeftForPenalty,
                out _);

            if (double.IsNaN(fitVal) || fitVal > 1)
            {
                return 1;
            }

            return fitVal;
        }
    }
}
