using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;


namespace DeconTools.Backend.ProcessingTasks.ResultValidators
{
    public class LabeledIsotopicProfileScorer : ResultValidator
    {
        private readonly InterferenceScorer iScorer;
        private AreaFitter areaFitter;

        #region Constructors
        public LabeledIsotopicProfileScorer(double minRelativeIntensityForScore = 0.2)
        {
            iScorer = new InterferenceScorer(minRelativeIntensityForScore);
        }
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        public override IsosResult CurrentResult { get; set; }

        public override void ValidateResult(ResultCollection resultColl, IsosResult currentResult)
        {
            Check.Require(currentResult is N14N15_TResult, "Currently, this is only implemented for N14N15 results");

            var n14n15result = (N14N15_TResult)currentResult;
            Check.Require(n14n15result.Target.IsotopicProfileLabeled != null, "Cannot validate labeled isotopic profile. Theoretical profile was not defined.");

            // stop, but don't throw an error if there is no labeled isotopic profile.
            if (n14n15result.IsotopicProfileLabeled == null) return;

            var isoN15 = n14n15result.IsotopicProfileLabeled;


            var scanPeaks = resultColl.Run.PeakList.Select(i => (MSPeak)i).ToList();

            // get i_score
            n14n15result.InterferenceScoreN15 = getIScore(scanPeaks, isoN15);

            // get fit score

            var fitVal = getFitValue(resultColl.Run.XYData, n14n15result.Target.IsotopicProfileLabeled, isoN15);
            n14n15result.ScoreN15 = fitVal;

        }

        private double getFitValue(XYData rawXYData, IsotopicProfile theorIso, IsotopicProfile isoN15)
        {
            var indexOfMostAbundantTheorPeak = theorIso.GetIndexOfMostIntensePeak();
            var indexOfCorrespondingObservedPeak = PeakUtilities.getIndexOfClosestValue(isoN15.Peaklist,
                theorIso.getMostIntensePeak().XValue, 0, isoN15.Peaklist.Count - 1, 0.1);

            var mzOffset = isoN15.Peaklist[indexOfCorrespondingObservedPeak].XValue - theorIso.Peaklist[indexOfMostAbundantTheorPeak].XValue;
            var fwhm = isoN15.GetFWHM();

            var theorXYData = theorIso.GetTheoreticalIsotopicProfileXYData(isoN15.GetFWHM());
            theorXYData.OffSetXValues(mzOffset);     //May want to avoid this offset if the masses have been aligned using LCMS Warp

            areaFitter = new AreaFitter();
            var fitVal = areaFitter.GetFit(theorXYData, rawXYData, 0.1);

            if (double.IsNaN(fitVal) || fitVal > 1)
                return 1;

            return fitVal;
        }

        private double getIScore(List<MSPeak> peakList, IsotopicProfile iso)
        {
            var monoPeak = iso.getMonoPeak();
            var lastPeak = iso.Peaklist[iso.Peaklist.Count - 1];

            var leftMZBoundary = monoPeak.XValue - 1.1;
            var rightMZBoundary = lastPeak.XValue + lastPeak.Width / 2.35 * 2;      // 2 sigma

            var interferenceVal = iScorer.GetInterferenceScore(peakList, iso.Peaklist, leftMZBoundary, rightMZBoundary);
            return interferenceVal;
        }
    }
}
