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
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods

        //TODO:  make this more generic!  doesn't need to be linked to IPeak or fwhm.... 
        public static XYData getTheorPeakData(IPeak peak, double fwhm)
        {
            return getTheorPeakData(peak, fwhm, 101);
        }

        public static XYData getTheorPeakData(IPeak peak, double fwhm, int numPointsPerPeak)
        {
            XYData xydata = new XYData();
            double one_over_sqrt_of_2_pi = 0.3989423;
            double sigma = fwhm / 2.35;      //   width@half-height =  2.35σ   (Gaussian peak theory)
            double centerXVal = peak.XValue;

            double sixsigma = 3 * fwhm;



            double mz_per_point = sixsigma / (double)(numPointsPerPeak - 1);

            int startPoint = 0 - (numPointsPerPeak - 1) / 2;
            int stopPoint = 0 + (numPointsPerPeak - 1) / 2;

            xydata.Xvalues = new double[numPointsPerPeak];
            xydata.Yvalues = new double[numPointsPerPeak];

            int counter = 0;
            for (int i = startPoint; i <= stopPoint; i++)
            {
                double mz = centerXVal + mz_per_point * (i);
                double intens = (1 / sigma) * one_over_sqrt_of_2_pi * Math.Exp(-1 * ((mz - centerXVal) * (mz - centerXVal)) / (2 * sigma * sigma));

                xydata.Xvalues[counter] = mz;
                xydata.Yvalues[counter] = intens;
                counter++;
            }

            xydata.NormalizeYData();


            for (int i = 0; i < xydata.Yvalues.Length; i++)
            {
                xydata.Yvalues[i] = xydata.Yvalues[i] * peak.Height;
            }
            return xydata;
        }


        public static XYData Get_Theoretical_IsotopicProfileXYData(IsotopicProfile isotopicProfile, double fwhm)
        {
            Check.Require(isotopicProfile != null && isotopicProfile.Peaklist != null &&
            isotopicProfile.Peaklist.Count > 0, "Cannot get theor isotopic profile. Input isotopic profile is empty.");

            XYData xydata = new XYData();
            List<double> xvals = new List<double>();
            List<double> yvals = new List<double>();

            for (int i = 0; i < isotopicProfile.Peaklist.Count; i++)
            {
                XYData tempXYData = getTheorPeakData(isotopicProfile.Peaklist[i], fwhm);
                xvals.AddRange(tempXYData.Xvalues);
                yvals.AddRange(tempXYData.Yvalues);

            }
            xydata.Xvalues = xvals.ToArray();
            xydata.Yvalues = yvals.ToArray();

            

            return xydata;
        }



        #endregion

        #region Private Methods
        #endregion

        public static XYData Get_Theoretical_IsotopicProfileXYData(IsotopicProfile theorProfile, double fwhm, double mzOffset)
        {
            throw new NotImplementedException();
        }
    }
}
