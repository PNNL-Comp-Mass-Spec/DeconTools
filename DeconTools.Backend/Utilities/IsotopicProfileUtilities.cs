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

        public static double CalculateAreaOfProfile(IsotopicProfile iso, double[] xvals, double[] yvals, double backgroundIntensity, ref int startingIndex)
        {
            double area = 0;

            foreach (var peak in iso.Peaklist)
            {
                //define start m/z and stop m/z based on peak m/z and width


                double peakSigma = peak.Width / 2.35;      //   width@half-height =  2.35σ   (Gaussian peak theory)
                double startMZ = peak.XValue - peakSigma * 2;   // width at base = 4σ;  
                double stopMZ = peak.XValue + peakSigma * 2;

                for (int i = startingIndex; i < xvals.Length; i++)
                {
                    startingIndex = i;    // move the starting point along so that next peak will start at the end of the last peak

                    if (xvals[i] >= startMZ)
                    {
                        if (xvals[i] <= stopMZ)
                        {
                            if (i == xvals.Length - 1) break;   // rare circumstance that m/z value is the last of the raw data points

                            double x1 = xvals[i];
                            double y1 = yvals[i];
                            double x2 = xvals[i + 1];
                            double y2 = yvals[i + 1];

                            area += (x2 - x1) * (y1 - backgroundIntensity) + (x2 - x1) * (y2 - y1) * 0.5;    //area of square + area of triangle (semi-trapazoid)
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
        /// Returnsa normalized isotopic profile
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="intensityForNormalization"></param>
        /// <returns></returns>
        public static void NormalizeIsotopicProfile(IsotopicProfile profile, float intensityForNormalization = 1.0f)
        {
            Check.Require(profile != null, "Isotopic profile is null");
            Check.Require(profile.Peaklist!=null, "Isotopic profile peaklist is null");

            var maxIntensity = profile.getMostIntensePeak().Height;
            
            for (int i = 0; i < profile.Peaklist.Count; i++)
            {
                profile.Peaklist[i].Height = profile.Peaklist[i].Height/maxIntensity*intensityForNormalization;
            }

           
        }


        public static void DisplayIsotopicProfileData(IsotopicProfile profile)
        {
            StringBuilder sb = new StringBuilder();
            int counter = 0;

            foreach (MSPeak peak in profile.Peaklist)
            {
                sb.Append(counter);
                sb.Append("\t");
                sb.Append(peak.XValue);
                sb.Append("\t");
                sb.Append(peak.Height);
                sb.Append("\t");
                sb.Append(peak.Width);
                sb.Append("\t");
                sb.Append(peak.SN);
                sb.Append("\n");

                counter++;
            }

            Console.Write(sb.ToString());
        }

        public static MSPeak GetPeakAtGivenMZ(IsotopicProfile iso1, double targetMZ, double mzTolerance)
        {

            //given the size of an isotopic distribution, we can use a linear (slow) search
            for (int i = 0; i < iso1.Peaklist.Count; i++)
            {
                if (Math.Abs(iso1.Peaklist[i].XValue-targetMZ)<=mzTolerance)
                {
                    return iso1.Peaklist[i];
                }
            }
            return null;
        }

        public static List<MSPeak> GetTopMSPeaks(List<MSPeak> msPeaklist, double intensityCutoff)
        {
            List<MSPeak> filteredMSPeaklist = new List<MSPeak>();

            foreach (var peak in msPeaklist)
            {

                if (peak.Height >= intensityCutoff) filteredMSPeaklist.Add(peak);
               
            }

            return filteredMSPeaklist;
        }
    }
}
