using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.PeakProcessing;

namespace DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.FitScoring
{
    /// <summary>
    /// TODO: remove this, since it's not actually used?
    /// </summary>
    public class ChiSqFitScorer : IsotopicProfileFitScorer
    {
        /// <summary>
        ///     calculates the fit score between the theoretical distribution stored and the observed data. Normalizes the observed
        ///     intensity by specified intensity.
        /// </summary>
        /// <param name="peakData"> variable which stores the data itself</param>
        /// <param name="chargeState"> charge state at which we want to compute the peak.</param>
        /// <param name="peak"> peak for which we want to compute the fit function.</param>
        /// <param name="mzDelta">specifies the mass delta between theoretical and observed m/z with the best fit so far.</param>
        /// <param name="minIntensityForScore">minimum intensity for score</param>
        /// <param name="pointsUsed">number of points used</param>
        /// <param name="debug">debug output flag</param>
        public override double FitScore(PeakData peakData, short chargeState, ThrashV1Peak peak, double mzDelta,
            double minIntensityForScore, out int pointsUsed, bool debug = false)
        {
            pointsUsed = 0;
            var numPoints = TheoreticalDistMzs.Count;
            if (numPoints < 3)
                return 1;

            double fit = 0;
            double sum = 0;
            for (var pointNum = 0; pointNum < numPoints; pointNum++)
            {
                var mz = TheoreticalDistMzs[pointNum] + mzDelta;
                var theoreticalIntensity = TheoreticalDistIntensities[pointNum];

                if (theoreticalIntensity >= minIntensityForScore)
                {
                    // observed intensities have to be normalized so that the maximum intensity is 100,
                    // like that for theoretical intensities.
                    var observedIntensity = 100 *
                                            GetPointIntensity(mz, peakData.MzList, peakData.IntensityList) /
                                            peak.Intensity;
                    var intensityDiff = observedIntensity - theoreticalIntensity;
                    var intensitySum = observedIntensity + theoreticalIntensity;
                    fit += intensityDiff * intensityDiff / intensitySum;
                    sum += theoreticalIntensity * observedIntensity;
                    pointsUsed++;
                }
            }

            return fit / (sum + 0.001);
        }

        /// <summary>
        ///     calculates the fit score between the theoretical distribution stored and the observed data. Normalizes the observed
        ///     intensity by specified intensity.
        /// </summary>
        /// <param name="peakData"> variable which stores the data itself</param>
        /// <param name="chargeState"> charge state at which we want to compute the peak.</param>
        /// <param name="intensityNormalizer">
        ///     intensity to normalize the peaks to. assumes that if peak with intensity = normalizer was
        ///     present, it would be normalized to 100
        /// </param>
        /// <param name="mzDelta">
        ///     specifies the mass delta between theoretical and observed m/z. The we are looking to score
        ///     against the feature in the observed data at theoeretical m/z + mzDelta
        /// </param>
        /// <param name="minIntensityForScore">minimum intensity for score</param>
        /// <param name="debug">prints debugging information if this is set to true.</param>
        public override double FitScore(PeakData peakData, short chargeState, double intensityNormalizer, double mzDelta,
            double minIntensityForScore, bool debug = false)
        {
            var numPoints = TheoreticalDistMzs.Count;
            if (numPoints < 3)
                return 1;

            double fit = 0;
            double sum = 0;
            for (var pointNum = 0; pointNum < numPoints; pointNum++)
            {
                var mz = TheoreticalDistMzs[pointNum] + mzDelta;
                var theoreticalIntensity = TheoreticalDistIntensities[pointNum];

                if (theoreticalIntensity >= minIntensityForScore)
                {
                    // observed intensities have to be normalized so that the maximum intensity is 100,
                    // like that for theoretical intensities.
                    var observedIntensity = 100 *
                                            GetPointIntensity(mz, peakData.MzList, peakData.IntensityList) /
                                            intensityNormalizer;
                    var intensityDiff = observedIntensity - theoreticalIntensity;
                    var intensitySum = observedIntensity + theoreticalIntensity;
                    fit += intensityDiff * intensityDiff / intensitySum;
                    sum += theoreticalIntensity * observedIntensity;
                }
            }

            return fit / (sum + 0.001);
        }
    }
}