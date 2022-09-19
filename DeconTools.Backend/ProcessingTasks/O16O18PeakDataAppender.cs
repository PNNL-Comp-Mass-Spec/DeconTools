using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    public class O16O18PeakDataAppender : Task
    {
        const double MASS_UNIT_BETWEEN_ISO = 1.002125;

        public override void Execute(ResultCollection resultList)
        {
            if (resultList.IsosResultBin == null || resultList.IsosResultBin.Count == 0)
            {
                return;
            }

            if (resultList.Run.PeakList == null || resultList.Run.PeakList.Count == 0)
            {
                return;
            }

            AppendO16O18PeakInfo(resultList.Run.PeakList, resultList.IsosResultBin as List<IsosResult>);
        }

        public void AppendO16O18PeakInfo(List<Peak> peakList, List<IsosResult> resultList)
        {
            //iterate over each ms feature
            foreach (var isosResult in resultList)
            {
                if (!(isosResult is O16O18IsosResult msFeature))
                {
                    continue;
                }

                if (msFeature.IsotopicProfile == null)
                {
                    continue;
                }

                var monoMZ = msFeature.IsotopicProfile.GetMZ();

                var mzMinusDaltons = monoMZ - (MASS_UNIT_BETWEEN_ISO * 4 / msFeature.IsotopicProfile.ChargeState);
                var mzPlusDaltons = monoMZ + (MASS_UNIT_BETWEEN_ISO * 4 / msFeature.IsotopicProfile.ChargeState);
                var mzPlusTwoDaltons = monoMZ + (MASS_UNIT_BETWEEN_ISO * 2 / msFeature.IsotopicProfile.ChargeState);

                double toleranceInPPM = 50;

                var toleranceInMZ = toleranceInPPM * monoMZ / 1e6;

                var minusPeaksWithinTol = PeakUtilities.GetPeaksWithinTolerance(peakList, mzMinusDaltons, toleranceInMZ);
                var plusPeaksWithinTol = PeakUtilities.GetPeaksWithinTolerance(peakList, mzPlusDaltons, toleranceInMZ);
                var twoDaltonsPlusPeaksWithinTol = PeakUtilities.GetPeaksWithinTolerance(peakList, mzPlusTwoDaltons, toleranceInMZ);

                var fourDaltonsMinusPeak = GetBestPeak(minusPeaksWithinTol, mzMinusDaltons);
                var fourDaltonsPlusPeak = GetBestPeak(plusPeaksWithinTol, mzPlusDaltons);

                var twoDaltonsPlusPeak = GetBestPeak(twoDaltonsPlusPeaksWithinTol, mzPlusTwoDaltons);

                if (fourDaltonsMinusPeak != null)
                {
                    msFeature.MonoMinus4Abundance = fourDaltonsMinusPeak.Height;
                }
                else
                {
                    msFeature.MonoMinus4Abundance = 0;
                }

                if (fourDaltonsPlusPeak != null)
                {
                    msFeature.MonoPlus4Abundance = fourDaltonsPlusPeak.Height;
                }
                else
                {
                    msFeature.MonoPlus4Abundance = 0;
                }

                if (twoDaltonsPlusPeak != null)
                {
                    msFeature.MonoPlus2Abundance = twoDaltonsPlusPeak.Height;
                }
                else
                {
                    msFeature.MonoPlus2Abundance = 0;
                }
            }
        }

        private Peak GetBestPeak(IReadOnlyList<Peak> peaksWithinTol, double targetMZ)
        {
            if (peaksWithinTol.Count == 0)
            {
                return null;
            }

            if (peaksWithinTol.Count == 1)
            {
                return peaksWithinTol[0];
            }

            var diff = double.MaxValue;
            Peak bestPeak = null;

            foreach (var peak in peaksWithinTol)
            {
                var currentDiff = Math.Abs(peak.XValue - targetMZ);

                if (currentDiff < diff)
                {
                    diff = currentDiff;
                    bestPeak = peak;
                }
            }

            return bestPeak;
        }
    }
}
