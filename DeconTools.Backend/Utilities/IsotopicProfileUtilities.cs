using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Utilities
{
    public class IsotopicProfileUtilities
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods

        public static double CalculateAreaOfProfile(IsotopicProfile iso, double[] xVals, double[] yVals, double backgroundIntensity, ref int startingIndex)
        {
            double area = 0;

            foreach (var peak in iso.Peaklist)
            {
                //define start m/z and stop m/z based on peak m/z and width


                var peakSigma = peak.Width / 2.35;      //   width@half-height =  2.35σ   (Gaussian peak theory)
                var startMZ = peak.XValue - peakSigma * 2;   // width at base = 4σ;
                var stopMZ = peak.XValue + peakSigma * 2;

                for (var i = startingIndex; i < xVals.Length; i++)
                {
                    startingIndex = i;    // move the starting point along so that next peak will start at the end of the last peak

                    if (xVals[i] >= startMZ)
                    {
                        if (xVals[i] <= stopMZ)
                        {
                            if (i == xVals.Length - 1) break;   // rare circumstance that m/z value is the last of the raw data points

                            var x1 = xVals[i];
                            var y1 = yVals[i];
                            var x2 = xVals[i + 1];
                            var y2 = yVals[i + 1];

                            area += (x2 - x1) * (y1 - backgroundIntensity) + (x2 - x1) * (y2 - y1) * 0.5;    //area of square + area of triangle (semi-trapezoid)
                        }
                        else
                        {
                            break;    //  went past the stopMZ. So break out and go onto the next peak.
                        }

                    }


                }
            }
            return area;
        }


        #endregion

        #region Private Methods
        #endregion

        /// <summary>
        /// Returns a normalized isotopic profile
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="intensityForNormalization"></param>
        /// <returns></returns>
        public static void NormalizeIsotopicProfile(IsotopicProfile profile, float intensityForNormalization = 1.0f)
        {
            Check.Require(profile != null, "Isotopic profile is null");
            Check.Require(profile.Peaklist != null, "Isotopic profile peaklist is null");

            var maxIntensity = profile.getMostIntensePeak().Height;

            foreach (var dataPoint in profile.Peaklist)
            {
                dataPoint.Height = dataPoint.Height / maxIntensity * intensityForNormalization;
            }


        }


        public static void NormalizeIsotopicProfileToSpecificPeak(IsotopicProfile profile, int indexOfPeakUsedForNormalization, float intensityForNormalization = 1.0f)
        {
            Check.Require(profile != null, "Isotopic profile is null");
            Check.Require(profile.Peaklist != null, "Isotopic profile peaklist is null");
            Check.Require(indexOfPeakUsedForNormalization < profile.Peaklist.Count, "Cannot normalize. Requested index exceeds array bounds.");

            if (indexOfPeakUsedForNormalization >= profile.Peaklist.Count)
            {
                return;
            }

            var intensityTargetPeak = profile.Peaklist[indexOfPeakUsedForNormalization].Height;

            foreach (var dataPoint in profile.Peaklist)
            {
                dataPoint.Height = dataPoint.Height / intensityTargetPeak * intensityForNormalization;
            }

        }


        /// <summary>
        /// Aligns an isotopic profile based on a source isotopic profile.
        /// </summary>
        /// <param name="iso1">Source isotopic profile</param>
        /// <param name="iso2">isotopic profile to be offset</param>
        public static void AlignTwoIsotopicProfiles(IsotopicProfile iso1, IsotopicProfile iso2)
        {
            double offset;
            if (iso2?.Peaklist == null || iso2.Peaklist.Count == 0) return;

            var mostIntensePeak = iso2.getMostIntensePeak();
            var indexOfMostIntensePeak = iso2.Peaklist.IndexOf(mostIntensePeak);

            if (iso1.Peaklist == null || iso1.Peaklist.Count == 0) return;

            var enoughPeaksInTarget = (indexOfMostIntensePeak <= iso1.Peaklist.Count - 1);

            if (enoughPeaksInTarget)
            {
                var targetPeak = iso1.Peaklist[indexOfMostIntensePeak];
                offset = targetPeak.XValue - mostIntensePeak.XValue;
                //offset = observedIsotopicProfile.Peaklist[0].XValue - theorIsotopicProfile.Peaklist[0].XValue;   //want to test to see if Thrash is same as rapid

            }
            else
            {
                offset = iso1.Peaklist[0].XValue - iso2.Peaklist[0].XValue;
            }


            foreach (var peak in iso2.Peaklist)
            {
                peak.XValue = peak.XValue + offset;

            }


        }




        public static void DisplayIsotopicProfileData(IsotopicProfile profile)
        {
            var sb = new StringBuilder();
            var counter = 0;

            foreach (var peak in profile.Peaklist)
            {
                sb.Append(counter);
                sb.Append("\t");
                sb.Append(peak.XValue);
                sb.Append("\t");
                sb.Append(peak.Height);
                sb.Append("\t");
                sb.Append(peak.Width);
                sb.Append("\t");
                sb.Append(peak.SignalToNoise);
                sb.Append("\n");

                counter++;
            }

            Console.Write(sb.ToString());
        }

        public static MSPeak GetPeakAtGivenMZ(IsotopicProfile iso1, double targetMZ, double mzTolerance)
        {
            //given the size of an isotopic distribution, we can use a linear (slow) search
            foreach (var dataPoint in iso1.Peaklist)
            {
                if (Math.Abs(dataPoint.XValue - targetMZ) <= mzTolerance)
                {
                    return dataPoint;
                }
            }

            return null;
        }

        public static List<MSPeak> GetTopMSPeaks(List<MSPeak> msPeakList, double intensityCutoff)
        {
            var filteredMSPeakList = new List<MSPeak>();

            foreach (var peak in msPeakList)
            {

                if (peak.Height >= intensityCutoff)
                    filteredMSPeakList.Add(peak);

            }

            return filteredMSPeakList;
        }

        public static List<double> GetTopNMZValues(List<MSPeak> msPeakList, int topNPeaks)
        {
            var sortedPeakList = msPeakList.OrderByDescending(x => x.Height);
            var mzList = new List<double>();

            var count = 0;
            foreach (var peak in sortedPeakList)
            {
                if (count < topNPeaks)
                {
                    mzList.Add(peak.XValue);
                }
                else
                {
                    break;
                }
                count++;
            }

            return mzList;
        }
    }
}
