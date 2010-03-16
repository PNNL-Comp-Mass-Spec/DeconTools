using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.Algorithms.Quantifiers
{
    public class BasicN14N15Quantifier : N14N15Quantifier
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        public override double GetRatio(double[] xvals, double[] yvals,
            DeconTools.Backend.Core.IsotopicProfile iso1, DeconTools.Backend.Core.IsotopicProfile iso2, 
            double backgroundIntensity)
        {
            double returnVal = -1;
            returnVal = GetRatioBasedOnAreaUnderPeaks(xvals, yvals, iso1, iso2, backgroundIntensity);
            return returnVal;


        }



        public double GetRatioBasedOnAreaUnderPeaks(double[] xvals, double[] yvals, 
            DeconTools.Backend.Core.IsotopicProfile iso1, DeconTools.Backend.Core.IsotopicProfile iso2, 
            double backgroundIntensity)
        {
            double returnRatio = -1;

            //define starting m/z value based on peak m/z and width
            double leftMostMZ = iso1.Peaklist[0].XValue - 1;  //    4σ = peak with at base;  '-1' ensures we are starting outside the peak. 

            //find starting point (use binary search)
            int indexOfStartingPoint = Utilities.MathUtils.BinarySearchWithTolerance(xvals, leftMostMZ, 0, xvals.Length - 1, 0.1);
            if (indexOfStartingPoint == -1) indexOfStartingPoint = 0;


            double area1 = IsotopicProfileUtilities.CalculateAreaOfProfile(iso1, xvals, yvals, backgroundIntensity, ref indexOfStartingPoint);
            double area2 = IsotopicProfileUtilities.CalculateAreaOfProfile(iso2, xvals, yvals, backgroundIntensity, ref indexOfStartingPoint);

            if (area1 < 0) area1 = 0;


            if (area2 > 0)
            {
                returnRatio = area1 / area2;
            }
            else
            {
                returnRatio = 9999;    //  this will indicate the problem of the second isotope having a 0 or negative area. 
            }

            return returnRatio;









            //iterate over peaks from isotopic profile 1


            //iterate over raw m/z values. 

            //If within peak m/z range, calculate area (trapazoid)


            //repeat the above with isotopic profile 2



        }




        public double GetRatioBasedOnMostIntensePeak(DeconTools.Backend.Core.IsotopicProfile iso1, DeconTools.Backend.Core.IsotopicProfile iso2)
        {
            double returnVal = -1;

            if (iso1 == null || iso2 == null) return returnVal;

            double iso1MaxIntens = iso1.getMostIntensePeak().Height;
            double iso2MaxIntens = iso2.getMostIntensePeak().Height;

            return returnVal;

        }
    }
}
