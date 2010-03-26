using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Core;

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

        public double GetRatioBasedOnTopPeaks(IsotopicProfile iso1, IsotopicProfile iso2, 
            IsotopicProfile theorIso1, IsotopicProfile theorIso2, double backgroundIntensity, int numPeaks)
        {


            //Find top n peaks of the theor profile.  Reference them by their peak index so that they can be looked up in the experimental. 
            //List<int> sortedTheorIso1Indexes = getIndexValsOfTopPeaks(theorIso1);
            //List<int> sortedTheorIso2Indexes = getIndexValsOfTopPeaks(theorIso2);

            IList<IPeak> topTheorIso1Peaks = getTopPeaks(theorIso1, numPeaks);
            IList<IPeak> topTheorIso2Peaks = getTopPeaks(theorIso2, numPeaks);

            double toleranceInPPM = 25;

            //get the top n peaks of the experimental profile, based on peaks of the theor profile
            IList<IPeak> topIso1Peaks = getTopPeaksBasedOnTheor(iso1, topTheorIso1Peaks,toleranceInPPM);
            IList<IPeak> topIso2Peaks = getTopPeaksBasedOnTheor(iso2, topTheorIso2Peaks, toleranceInPPM);

            //Since the number of top experimental iso peaks may be less than the number of top theor iso peaks,
            //we have to filter and ensure that they have the same peak numbers, so that the correction factor
            // (applied below) is properly applied.   HOWEVER,  this may induce some differences between runs 
            IList<IPeak> filteredTopTheorIso1Peaks = getTopPeaksBasedOnTheor(theorIso1, topIso1Peaks, toleranceInPPM);
            IList<IPeak> filteredTopTheorIso2Peaks = getTopPeaksBasedOnTheor(theorIso2, topIso2Peaks, toleranceInPPM);

            double summedIso1PeakIntensities = PeakUtilities.GetSumOfIntensities(topIso1Peaks, backgroundIntensity);
            double summedIso2PeakIntensities = PeakUtilities.GetSumOfIntensities(topIso2Peaks, backgroundIntensity);

            //need to find out the contribution of the top n peaks to the overall isotopic envelope.  Base this on the theor profile
            double summedTheorIso1Intensities = filteredTopTheorIso1Peaks.Sum(p => p.Height);
            double summedTheorIso2Intensities = filteredTopTheorIso2Peaks.Sum(p => p.Height);
            
 
            double fractionTheor1 = summedTheorIso1Intensities / theorIso1.Peaklist.Sum(p => p.Height);
            double fractionTheor2 = summedTheorIso2Intensities / theorIso2.Peaklist.Sum(p => p.Height);

            //use the above ratio to correct the intensities based on how much the top n peaks contribute to the profile. 
            double correctedIso1SummedIntens = summedIso1PeakIntensities / fractionTheor1;
            double correctedIso2SummedIntens = summedIso2PeakIntensities / fractionTheor2;

            double ratio = correctedIso1SummedIntens / correctedIso2SummedIntens;


            return ratio; 

        }

        private IList<IPeak> getTopPeaksBasedOnTheor(IsotopicProfile iso1, IList<IPeak> topTheorIso1Peaks, double toleranceInPPM)
        {
            List<IPeak> topPeaks = new List<IPeak>();

            foreach (var item in topTheorIso1Peaks)
            {
                double mzTolerance = toleranceInPPM * item.XValue / 1e6;

                IPeak isoPeak = IsotopicProfileUtilities.GetPeakAtGivenMZ(iso1, item.XValue, mzTolerance);

                if (isoPeak != null)
                {
                    topPeaks.Add(isoPeak);
                }
                else
                {

                }
               
            }

            return topPeaks;
        }

        private IList<IPeak> getTopPeaks(IsotopicProfile theorIso1, int numPeaks)
        {

            IPeak peak = theorIso1.Peaklist[0];

            IList<IPeak> sortedList = new List<IPeak>();
            sortedList.Add(theorIso1.Peaklist[0]);

            for (int i = 1; i < theorIso1.Peaklist.Count; i++)
            {
                if (theorIso1.Peaklist[i].Height > sortedList[0].Height)
                {
                    sortedList.Insert(0, theorIso1.Peaklist[i]);
                }
                else
                {
                    sortedList.Add(theorIso1.Peaklist[i]);
                }
            }

            return sortedList.Take(numPeaks).ToList();


        }

        private double getSummedIntensitiesForPeakIndices(IsotopicProfile iso, List<int> sortedIndexes, int numPeaks)
        {
            double summedIntensities = 0; 
            
            for (int i = 0; i < numPeaks; i++)
            {
                int targetIndexVal = sortedIndexes[i];
                if (iso.Peaklist.Count > targetIndexVal)
                {
                    summedIntensities += iso.Peaklist[targetIndexVal].Height;
                }
            }
            return summedIntensities;
        }

        private List<int> getIndexValsOfTopPeaks(IsotopicProfile theorIso1)
        {
            List<int> indexValsOfTopPeaks = new List<int>();

            indexValsOfTopPeaks.Add(0);
            if (theorIso1.Peaklist.Count > 1)
            {
                for (int i = 1; i < theorIso1.Peaklist.Count; i++)
                {
                    if (theorIso1.Peaklist[i].Height > theorIso1.Peaklist[indexValsOfTopPeaks[0]].Height)
                    {
                        indexValsOfTopPeaks.Insert(0, i);
                    }
                    else
                    {
                        indexValsOfTopPeaks.Add(i);
                    }

                }

            }
            return indexValsOfTopPeaks;

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
