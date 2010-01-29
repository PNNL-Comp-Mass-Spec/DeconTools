using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Utilities;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public class BasicTFeatureFinder : ITargetedFeatureFinder
    {
        #region Constructors
        public BasicTFeatureFinder()
            : this(0.005)     // default mzTolerance
        {

        }

        public BasicTFeatureFinder(double mzTolerance)
        {
            this.MZTolerance = mzTolerance;
        }

        #endregion

        #region Properties
        public double MZTolerance { get; set; }
        public bool NeedMonoIsotopicPeak { get; set; }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
        public override void FindFeature(DeconTools.Backend.Core.ResultCollection resultColl)
        {

            Check.Require(resultColl.Run.PeakList != null, "Targeted Feature finder failed. Peak list has not been established. You need to run a peak detector.");
            Check.Require(resultColl.Run.CurrentMassTag != null, "Targeted Feature finder failed. Mass Tag must be defined but it isn't.");
            Check.Require(resultColl.Run.CurrentMassTag.IsotopicProfile != null, "Targeted feature finder failed. Theoretical Isotopic Profile must be assigned to Mass Tag. Use a 'TheorFeatureGeneratorTask'");

            IMassTagResult result = resultColl.GetMassTagResult(resultColl.Run.CurrentMassTag);
            if (result == null)
            {
                result = resultColl.CreateMassTagResult(resultColl.Run.CurrentMassTag);
            }

            if (result.ScanSet == null)
            {
                result.ScanSet = resultColl.Run.CurrentScanSet;

            }


            IsotopicProfile theor = resultColl.Run.CurrentMassTag.IsotopicProfile;


            int indexOfMaxTheorPeak = theor.getIndexOfMostIntensePeak();

            //start with max theor peak and go left, looking in experimental data for matching peaks
            // ---------------- look left --------------------
            double massDefect = 0;
            bool foundMatchingMaxPeak = false;

            for (int i = indexOfMaxTheorPeak; i >= 0; i--)
            {
                //find experimental peak(s) within range
                List<MSPeak> peaksWithinTol = getPeaksWithinTol(resultColl.Run.PeakList, theor.Peaklist[i].XValue, MZTolerance);

                if (i == indexOfMaxTheorPeak)
                {
                    foundMatchingMaxPeak = peaksWithinTol.Count > 0;
                }

                if (!foundMatchingMaxPeak)   // can't even find the observed peak that matches the most intense theor peak. 
                {
                    result.IsotopicProfile = null;     //no matching isotopic profile found for this mass tag
                    break;   //   give up... 
                }

                if (peaksWithinTol.Count == 0)
                {
                    if (NeedMonoIsotopicPeak)
                    {
                        result.IsotopicProfile = null;    //here, we are looking to the left of most intense theor peak.  If we have the prerequisite of finding the monoIsotopic peak and fail here, we'll return a null isotopic profile
                        break;
                    }
                    else
                    {
                        break;  // stop looking to the left of the most intense peak. 
                    }
                }
                else if (peaksWithinTol.Count == 1)
                {
                    if (result.IsotopicProfile.Peaklist.Count == 0)
                    {
                        result.IsotopicProfile.Peaklist.Add(peaksWithinTol[0]);
                    }
                    else
                    {
                        result.IsotopicProfile.Peaklist.Insert(0, peaksWithinTol[0]);
                    }
                }
                else    // when we have several peaks within tolerance, we'll need to decide what to do 
                {

                    MSPeak bestPeak;
                    if (i == indexOfMaxTheorPeak)   //when matching to most intense peak, we will use the most intense peak
                    {
                        bestPeak = findMostIntensePeak(peaksWithinTol, theor.Peaklist[i].XValue);
                    }
                    else
                    {
                        bestPeak = findClosestToMZPeak(peaksWithinTol, theor.Peaklist[i].XValue, massDefect);
                    }

                    if (result.IsotopicProfile.Peaklist.Count == 0)
                    {
                        result.IsotopicProfile.Peaklist.Add(bestPeak);
                    }
                    else
                    {
                        result.IsotopicProfile.Peaklist.Insert(0, bestPeak);

                    }
                }

                if (i == indexOfMaxTheorPeak)   //when matching to most intense peak, we will use the most intense peak
                {
                    massDefect = theor.Peaklist[i].XValue - result.IsotopicProfile.Peaklist[0].XValue;
                }
            }

            if (result.IsotopicProfile == null) return;   // above has failed.  Don't bother looking anymore.  No feature found for this mass tag.


            //------------------------- look right -------------------------------------------
            for (int i = indexOfMaxTheorPeak + 1; i < theor.Peaklist.Count; i++)     //start one peak to the right of the max intense theor peak
            {
                List<MSPeak> peaksWithinTol = getPeaksWithinTol(resultColl.Run.PeakList, theor.Peaklist[i].XValue, MZTolerance);
                if (peaksWithinTol.Count == 0)
                {
                    if (i == indexOfMaxTheorPeak + 1)  // first peak to the right of the max peak.  We need this one or we declare it to be a failure (= null)
                    {
                        result.IsotopicProfile = null;
                        return;
                    }
                    break;    // finished.  Exit loop. 
                }
                else if (peaksWithinTol.Count == 1)
                {

                    result.IsotopicProfile.Peaklist.Add(peaksWithinTol[0]);   //here, we tack peaks onto the profile
                }
                else    //two or more peaks are within tolerance. Need to get the best one, which is based on the distance from the 
                {
                    result.IsotopicProfile.Peaklist.Add(findClosestToMZPeak(peaksWithinTol, theor.Peaklist[i].XValue, massDefect));
                }
            }

            addInfoToResult(result);



            return;
        }

        private void addInfoToResult(IMassTagResult result)
        {
            if (result.IsotopicProfile != null)
            {
                result.IsotopicProfile.ChargeState = result.MassTag.ChargeState;
                result.IsotopicProfile.MonoIsotopicMass = (result.IsotopicProfile.GetMZ() - Globals.PROTON_MASS) * result.MassTag.ChargeState;
                result.IsotopicProfile.IntensityAggregate = result.IsotopicProfile.getMostIntensePeak().Height;     // may need to change this to sum the top n peaks. 
            }

        }

        private List<MSPeak> getPeaksWithinTol(List<IPeak> list, double targetMZ, double mztol)
        {

            //TODO: this should be optimized.  I'm simply going through the peaklist from start to the point where the m/z value exceeds the targetMZ. 
            //We should use a smarter approach. 

            List<MSPeak> peaksWithinTol = new List<MSPeak>();

            for (int n = 0; n < list.Count; n++)
            {
                bool observedPeakIsWithinTolerance = ((Math.Abs(list[n].XValue - targetMZ)) / targetMZ * 1e6 <= mztol);
                if (observedPeakIsWithinTolerance)
                {
                    peaksWithinTol.Add((MSPeak)list[n]);
                }
                else
                {
                    if (list[n].XValue > targetMZ) break;  //don't bother iterating through the rest of the list. 
                }
            }
            return peaksWithinTol;
        }

        private MSPeak findClosestToMZPeak(List<MSPeak> peaksWithinTol, double targetMZ, double massDefect)
        {
            double diff = double.MaxValue;
            MSPeak mostIntensePeak = new MSPeak();

            for (int i = 0; i < peaksWithinTol.Count; i++)
            {

                double obsDiff = Math.Abs(peaksWithinTol[i].XValue - targetMZ + massDefect);

                if (obsDiff < diff)
                {
                    diff = obsDiff;
                    mostIntensePeak = peaksWithinTol[i];
                }

            }

            return mostIntensePeak;
        }

        private MSPeak findMostIntensePeak(List<MSPeak> peaksWithinTol, double targetMZ)
        {
            double maxIntensity = 0;
            MSPeak mostIntensePeak = new MSPeak();

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

        private MSPeak findMostIntensePeak(List<MSPeak> peaksWithinTol)
        {
            throw new NotImplementedException();
        }
    }
}
