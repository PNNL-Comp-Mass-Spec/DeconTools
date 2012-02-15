using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Utilities.IsotopeDistributionCalculation
{
    public class TheorXYDataCalculationUtilities
    {
        #region Public Methods

        public static XYData GetTheorPeakData(IPeak peak, double fwhm)
        {
            return GetTheorPeakData(peak, fwhm, 101);
        }

        public static XYData GetTheorPeakData(IPeak peak, double fwhm, int numPointsPerPeak)
        {
            return GetTheorPeakData(peak.XValue, peak.Height, fwhm, numPointsPerPeak);
        }


        public static XYData GetTheorPeakData(double centerXValue, double peakHeight, double peakWidth, int numPointsPerPeak)
        {
            XYData xydata = new XYData();
            double one_over_sqrt_of_2_pi = 0.3989423;
            double sigma = peakWidth / 2.35;      //   width@half-height =  2.35σ   (Gaussian peak theory)
            double sixsigma = 3 * peakWidth;
            double mz_per_point = sixsigma / (double)(numPointsPerPeak - 1);

            int startPoint = 0 - (numPointsPerPeak - 1) / 2;
            int stopPoint = 0 + (numPointsPerPeak - 1) / 2;

            xydata.Xvalues = new double[numPointsPerPeak];
            xydata.Yvalues = new double[numPointsPerPeak];

            int counter = 0;
            for (int i = startPoint; i <= stopPoint; i++)
            {
                double mz = centerXValue + mz_per_point * (i);
                double intens = (1 / sigma) * one_over_sqrt_of_2_pi * Math.Exp(-1 * ((mz - centerXValue) * (mz - centerXValue)) / (2 * sigma * sigma));

                xydata.Xvalues[counter] = mz;
                xydata.Yvalues[counter] = intens;
                counter++;
            }

            xydata.NormalizeYData();


            for (int i = 0; i < xydata.Yvalues.Length; i++)
            {
                xydata.Yvalues[i] = xydata.Yvalues[i] * peakHeight;
            }
            return xydata;
        }


        public static XYData GetTheoreticalIsotopicProfileXYData(IsotopicProfile isotopicProfile, double fwhm)
        {
            Check.Require(isotopicProfile != null && isotopicProfile.Peaklist != null &&
            isotopicProfile.Peaklist.Count > 0, "Cannot get theor isotopic profile. Input isotopic profile is empty.");

            XYData xydata = new XYData();
            List<double> xvals = new List<double>();
            List<double> yvals = new List<double>();

            for (int i = 0; i < isotopicProfile.Peaklist.Count; i++)
            {
                XYData tempXYData = GetTheorPeakData(isotopicProfile.Peaklist[i], fwhm);
                xvals.AddRange(tempXYData.Xvalues);
                yvals.AddRange(tempXYData.Yvalues);

            }
            xydata.Xvalues = xvals.ToArray();
            xydata.Yvalues = yvals.ToArray();



            return xydata;
        }



        #endregion

     
        public static XYData GetTheoreticalIsotopicProfileXYData(IsotopicProfile theorProfile, double fwhm, double mzOffset)
        {
            throw new NotImplementedException();
        }
    }
}
