using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    public class O16O18PeakDataAppender : Task
    {
        
        const double MASSUNIT_BETWEEN_ISO = 1.002125;

        public override void Execute(ResultCollection resultColl)
        {
            if (resultColl.IsosResultBin == null || resultColl.IsosResultBin.Count == 0) return;

            if (resultColl.Run.PeakList == null || resultColl.Run.PeakList.Count == 0) return;

            AppendO16O18PeakInfo(resultColl.Run.PeakList, resultColl.IsosResultBin as List<IsosResult>);
        }



        public void AppendO16O18PeakInfo(List<IPeak> peakList, List<IsosResult> resultList)
        {
            //iterate over each ms feature
            foreach (O16O18IsosResult msFeature in resultList)
            {
                if (msFeature.IsotopicProfile == null) continue;

                double monoMZ = msFeature.IsotopicProfile.GetMZ();

                double mzMinusDaltons = monoMZ - (MASSUNIT_BETWEEN_ISO * 4 / msFeature.IsotopicProfile.ChargeState);
                double mzPlusDaltons = monoMZ + (MASSUNIT_BETWEEN_ISO *4 / msFeature.IsotopicProfile.ChargeState);
                double mzPlusTwoDaltons = monoMZ + (MASSUNIT_BETWEEN_ISO *2 / msFeature.IsotopicProfile.ChargeState);

                double toleranceInPPM = 50;


                double toleranceInMZ = toleranceInPPM * monoMZ / 1e6;


                var minusPeaksWithinTol = PeakUtilities.GetPeaksWithinTolerance(peakList, mzMinusDaltons, toleranceInMZ);
                var plusPeaksWithinTol = PeakUtilities.GetPeaksWithinTolerance(peakList, mzPlusDaltons, toleranceInMZ);
                var twoDaltonsPlusPeaksWithinTol = PeakUtilities.GetPeaksWithinTolerance(peakList,mzPlusTwoDaltons,toleranceInMZ);



                IPeak fourDaltonsMinusPeak = GetBestPeak(minusPeaksWithinTol, mzMinusDaltons);
                IPeak fourDaltonsPlusPeak = GetBestPeak(plusPeaksWithinTol, mzPlusDaltons);

                IPeak twoDaltonsPlusPeak = GetBestPeak(twoDaltonsPlusPeaksWithinTol, mzPlusTwoDaltons);

                if (fourDaltonsMinusPeak!=null)
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

        private IPeak GetBestPeak(List<IPeak> peaksWithinTol, double targetMZ)
        {
            if (peaksWithinTol.Count == 0) return null;

            if (peaksWithinTol.Count == 1) return peaksWithinTol[0];


            double diff = double.MaxValue;
            IPeak bestPeak = null;

            foreach (var peak in peaksWithinTol)
            {
                double currentDiff = Math.Abs(peak.XValue - targetMZ);

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
