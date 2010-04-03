using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.Algorithms
{
    public class BasicMSFeatureFinder
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods

        public IsotopicProfile FindMSFeature(List<IPeak> peakList, IsotopicProfile theorFeature, double toleranceInPPM, bool requireMonoPeak)
        {
            Check.Require(theorFeature != null, "Theoretical feature hasn't been defined.");
            Check.Require(theorFeature.Peaklist != null && theorFeature.Peaklist.Count > 0, "Theoretical feature hasn't been defined.");



            IsotopicProfile outFeature = new IsotopicProfile();

            int indexOfMaxTheorPeak = theorFeature.getIndexOfMostIntensePeak();


            double toleranceInMZ = theorFeature.getMonoPeak().XValue * toleranceInPPM / 1e6;

            bool foundMatchingMaxPeak = false;
            double massDefect = 0;   // this is the m/z diff between the max peak of theor feature and the max peak of the experimental feature



            for (int i = indexOfMaxTheorPeak; i >= 0; i--)
            {
                //find experimental peak(s) within range
                List<IPeak> peaksWithinTol = PeakUtilities.GetPeaksWithinTolerance(peakList, theorFeature.Peaklist[i].XValue, toleranceInMZ);

                if (i == indexOfMaxTheorPeak)
                {
                    foundMatchingMaxPeak = peaksWithinTol.Count > 0;
                }

                if (!foundMatchingMaxPeak)   // can't even find the observed peak that matches the most intense theor peak. 
                {
                    return null;
                }

                if (peaksWithinTol.Count == 0)
                {
                    if (requireMonoPeak)
                    {
                        return null;    //here, we are looking to the left of most intense theor peak.  If we have the prerequisite of finding the monoIsotopic peak and fail here, we'll return a null isotopic profile
                    }
                    else
                    {
                        break;  // stop looking to the left of the most intense peak. 
                    }
                }
                else if (peaksWithinTol.Count == 1)
                {
                    if (outFeature.Peaklist.Count == 0)
                    {
                        outFeature.Peaklist.Add((MSPeak)peaksWithinTol[0]);
                    }
                    else
                    {
                        outFeature.Peaklist.Insert(0, (MSPeak)peaksWithinTol[0]);
                    }
                }
                
                else    // when we have several peaks within tolerance, we'll need to decide what to do 
                {

                    MSPeak bestPeak;
                    if (i == indexOfMaxTheorPeak)   //when matching to most intense peak, we will use the most intense peak
                    {
                        bestPeak = (MSPeak)findMostIntensePeak(peaksWithinTol, theorFeature.Peaklist[i].XValue);
                    }
                    else
                    {
                        bestPeak = (MSPeak)findClosestToXValue(peaksWithinTol, theorFeature.Peaklist[i].XValue + massDefect);
                    }

                    if (outFeature.Peaklist.Count == 0)
                    {
                        outFeature.Peaklist.Add(bestPeak);
                    }
                    else
                    {
                        outFeature.Peaklist.Insert(0, bestPeak);

                    }
                }


                if (i == indexOfMaxTheorPeak)   //when matching to most intense peak, we will use the most intense peak
                {
                    massDefect = theorFeature.Peaklist[i].XValue - outFeature.Peaklist[0].XValue;
                }

            }

            if (outFeature == null) return null;   // above has failed.  Don't bother looking anymore.  No feature found for this mass tag.

            //------------------------- look right -------------------------------------------
            for (int i = indexOfMaxTheorPeak + 1; i < theorFeature.Peaklist.Count; i++)     //start one peak to the right of the max intense theor peak
            {
                List<IPeak> peaksWithinTol = PeakUtilities.GetPeaksWithinTolerance(peakList, theorFeature.Peaklist[i].XValue, toleranceInMZ);
                if (peaksWithinTol.Count == 0)
                {
                    if (i == indexOfMaxTheorPeak + 1)  // first peak to the right of the max peak.  We need this one or we declare it to be a failure (= null)
                    {
                        return null;
                    }
                    break;    // finished.  Exit loop. 
                }
                else if (peaksWithinTol.Count == 1)
                {

                    outFeature.Peaklist.Add((MSPeak)peaksWithinTol[0]);   //here, we tack peaks onto the profile
                }
                else    //two or more peaks are within tolerance. Need to get the best one, which is based on the distance from the 
                {
                    outFeature.Peaklist.Add((MSPeak)findClosestToXValue(peaksWithinTol, theorFeature.Peaklist[i].XValue + massDefect));
                }
            }

            return outFeature;



        }

        private IPeak findMostIntensePeak(List<IPeak> peaksWithinTol, double targetMZ)
        {
            double maxIntensity = 0;
            IPeak mostIntensePeak = null;

            for (int i = 0; i < peaksWithinTol.Count; i++)
            {
                float obsIntensity = peaksWithinTol[i].Height;
                if (obsIntensity > maxIntensity)
                {
                    maxIntensity = obsIntensity;
                    mostIntensePeak = peaksWithinTol[i];
                }
            }
            return mostIntensePeak;
        }


        private IPeak findClosestToXValue(List<IPeak> peaksWithinTol, double targetVal)
        {
            double diff = double.MaxValue;
            IPeak closestPeak = null;

            for (int i = 0; i < peaksWithinTol.Count; i++)
            {

                double obsDiff = Math.Abs(peaksWithinTol[i].XValue - targetVal);

                if (obsDiff < diff)
                {
                    diff = obsDiff;
                    closestPeak = peaksWithinTol[i];
                }

            }

            return closestPeak;
        }




        #endregion

        #region Private Methods
        #endregion
    }
}
