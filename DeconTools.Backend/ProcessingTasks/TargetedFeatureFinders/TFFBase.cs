using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Algorithms;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public abstract class TFFBase : Task
    {
        #region Properties

        public virtual double ToleranceInPPM { get; set; }

        /// <summary>
        /// If true, then FeatureFinder must find the monoIsotopic peak or no feature is reported. (Useful for most peptides or small MassTags)
        /// </summary>
        public virtual bool NeedMonoIsotopicPeak { get; set; }

        /// <summary>
        /// Each MassTag has two possible isotopic profiles (unlabelled and labelled). 
        /// This property specifies which of the two are to be targeted in the real data. 
        /// This property is mainly used in workflows that follow a Task-based implementation. 
        /// </summary>
        public IsotopicProfileType IsotopicProfileType { get; set; }


        public int NumPeaksUsedInAbundance { get; set; }

        #endregion

        #region Public Methods
        public virtual IsotopicProfile FindMSFeature(List<IPeak> peakList, IsotopicProfile theorFeature, double toleranceInPPM, bool needMonoIsotopicPeak)
        {
            Check.Require(theorFeature != null, "Theoretical feature hasn't been defined.");
            Check.Require(theorFeature.Peaklist != null && theorFeature.Peaklist.Count > 0, "Theoretical feature hasn't been defined.");

            IsotopicProfile outFeature = new IsotopicProfile();
            outFeature.ChargeState = theorFeature.ChargeState;

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
                    if (needMonoIsotopicPeak)
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
                        bestPeak = (MSPeak)findClosestToXValue(peaksWithinTol, theorFeature.Peaklist[i].XValue - massDefect);
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
                    outFeature.Peaklist.Add((MSPeak)findClosestToXValue(peaksWithinTol, theorFeature.Peaklist[i].XValue - massDefect));
                }
            }

            //outFeature.


            addMassInfoToIsotopicProfile(theorFeature, outFeature);

            return outFeature;




        }

        private void addMassInfoToIsotopicProfile(IsotopicProfile theorFeature, IsotopicProfile outFeature)
        {
            int indexOfTheorMono = PeakUtilities.getIndexOfClosestValue(theorFeature.Peaklist, theorFeature.MonoPeakMZ, 0, outFeature.Peaklist.Count - 1, 0.05);
            bool theorMonoPeakNotFound = (indexOfTheorMono == -1);
            if (theorMonoPeakNotFound) return;

            MSPeak targetTheorPeak = theorFeature.Peaklist[indexOfTheorMono];

            int indexOfMonoPeak = PeakUtilities.getIndexOfClosestValue(outFeature.Peaklist, targetTheorPeak.XValue, 0, outFeature.Peaklist.Count - 1, 0.1);

            outFeature.MonoIsotopicPeakIndex = indexOfMonoPeak;

            bool monoPeakFoundInObservedIso = (outFeature.MonoIsotopicPeakIndex != -1);
            if (monoPeakFoundInObservedIso)
            {
                MSPeak monoPeak = outFeature.Peaklist[outFeature.MonoIsotopicPeakIndex];

                outFeature.MonoPeakMZ = monoPeak.XValue;
                outFeature.MonoIsotopicMass = (monoPeak.XValue - Globals.PROTON_MASS) * outFeature.ChargeState;



            }




        }




        public override void Execute(ResultCollection resultColl)
        {
            Check.Require(resultColl != null && resultColl.Run != null, String.Format("{0} failed. Run is empty.", this.Name));
            Check.Require(resultColl.Run.CurrentMassTag != null, String.Format("{0} failed. CurrentMassTag hasn't been defined.", this.Name));

            MassTagResultBase result = resultColl.GetMassTagResult(resultColl.Run.CurrentMassTag);

            IsotopicProfile iso;

            resultColl.IsosResultBin.Clear();


            switch (IsotopicProfileType)
            {
                case IsotopicProfileType.UNLABELLED:
                    iso = FindMSFeature(resultColl.Run.PeakList, resultColl.Run.CurrentMassTag.IsotopicProfile, this.ToleranceInPPM, this.NeedMonoIsotopicPeak);
                    result.IsotopicProfile = iso;
                    break;
                case IsotopicProfileType.LABELLED:
                    iso = FindMSFeature(resultColl.Run.PeakList, resultColl.Run.CurrentMassTag.IsotopicProfileLabelled, this.ToleranceInPPM, this.NeedMonoIsotopicPeak);
                    result.AddLabelledIso(iso);


                    break;
                default:
                    iso = FindMSFeature(resultColl.Run.PeakList, resultColl.Run.CurrentMassTag.IsotopicProfile, this.ToleranceInPPM, this.NeedMonoIsotopicPeak);
                    result.IsotopicProfile = iso;
                    break;
            }

            bool isoIsGood = (iso != null && iso.Peaklist != null && iso.Peaklist.Count > 0);
            if (isoIsGood)
            {
                iso.IntensityAggregate = sumPeaks(iso, this.NumPeaksUsedInAbundance, 0);
            }

            resultColl.IsosResultBin.Add(result);

        }
        #endregion

        #region Private Methods
        private void addInfoToResult(IsotopicProfile iso, MassTag mt)
        {
            if (iso != null)
            {
                iso.ChargeState = mt.ChargeState;
                iso.MonoIsotopicMass = (iso.GetMZ() - Globals.PROTON_MASS) * mt.ChargeState;
                iso.IntensityAggregate = iso.getMostIntensePeak().Height;     // may need to change this to sum the top n peaks. 
            }
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


        private MSPeak findClosestToXValue(List<MSPeak> list, double p)
        {
            throw new NotImplementedException();
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


        private double sumPeaks(IsotopicProfile profile, int numPeaksUsedInAbundance, int defaultVal)
        {
            if (profile.Peaklist == null || profile.Peaklist.Count == 0) return defaultVal;
            List<float> peakListIntensities = new List<float>();
            foreach (MSPeak peak in profile.Peaklist)
            {
                peakListIntensities.Add(peak.Height);

            }
            peakListIntensities.Sort();
            peakListIntensities.Reverse();    // i know... this isn't the best way to do this!
            double summedIntensities = 0;

            for (int i = 0; i < peakListIntensities.Count; i++)
            {
                if (i < numPeaksUsedInAbundance)
                {
                    summedIntensities += peakListIntensities[i];
                }


            }

            return summedIntensities;

        }


        #endregion



    }
}
