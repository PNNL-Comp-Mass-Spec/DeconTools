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

        public static XYData GetTheorPeakData(Peak peak, double fwhm)
        {
            return GetTheorPeakData(peak, fwhm, 101);
        }

        public static XYData GetTheorPeakData(Peak peak, double fwhm, int numPointsPerPeak)
        {
            return GetTheorPeakData(peak.XValue, peak.Height, fwhm, numPointsPerPeak);
        }

        public static XYData GetTheorPeakData(double centerXValue, double peakHeight, double peakWidth, int numPointsPerPeak)
        {
            var xydata = new XYData();
            var one_over_sqrt_of_2_pi = 0.3989423;
            var sigma = peakWidth / 2.35;      //   width@half-height =  2.35σ   (Gaussian peak theory)
            var sixsigma = 3 * peakWidth;
            var mz_per_point = sixsigma / (double)(numPointsPerPeak - 1);

            var startPoint = 0 - (numPointsPerPeak - 1) / 2;
            var stopPoint = 0 + (numPointsPerPeak - 1) / 2;

            xydata.Xvalues = new double[numPointsPerPeak];
            xydata.Yvalues = new double[numPointsPerPeak];

            var counter = 0;
            for (var i = startPoint; i <= stopPoint; i++)
            {
                var mz = centerXValue + mz_per_point * (i);
                var intens = (1 / sigma) * one_over_sqrt_of_2_pi * Math.Exp(-1 * ((mz - centerXValue) * (mz - centerXValue)) / (2 * sigma * sigma));

                xydata.Xvalues[counter] = mz;
                xydata.Yvalues[counter] = intens;
                counter++;
            }

            xydata.NormalizeYData();

            for (var i = 0; i < xydata.Yvalues.Length; i++)
            {
                xydata.Yvalues[i] *= peakHeight;
            }
            return xydata;
        }

        public static XYData GetTheoreticalIsotopicProfileXYData(IsotopicProfile isotopicProfile, double fwhm)
        {
            Check.Require(isotopicProfile != null && isotopicProfile.Peaklist?.Count > 0, "Cannot get theor isotopic profile. Input isotopic profile is empty.");

            var xydata = new XYData();
            var xvals = new List<double>();
            var yvals = new List<double>();

            for (var i = 0; i < isotopicProfile.Peaklist.Count; i++)
            {
                var tempXYData = GetTheorPeakData(isotopicProfile.Peaklist[i], fwhm);
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
