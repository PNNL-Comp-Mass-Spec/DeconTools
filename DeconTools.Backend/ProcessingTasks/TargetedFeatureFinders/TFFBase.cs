using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

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

        protected MultiAlignEngine.Alignment.clsAlignmentFunction AlignmentInfo { get; set; }



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


            bool failedResult = false;


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
                    failedResult = true;
                    break;
                }

                if (peaksWithinTol.Count == 0)
                {
                    if (needMonoIsotopicPeak)
                    {
                        //here, we are looking to the left of most intense theor peak.  If we have the prerequisite of finding the monoIsotopic peak and fail here, we'll return a failed result
                        failedResult = true;
                        break;

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


            //------------------------- look right -------------------------------------------
            for (int i = indexOfMaxTheorPeak + 1; i < theorFeature.Peaklist.Count; i++)     //start one peak to the right of the max intense theor peak
            {
                List<IPeak> peaksWithinTol = PeakUtilities.GetPeaksWithinTolerance(peakList, theorFeature.Peaklist[i].XValue, toleranceInMZ);
                if (peaksWithinTol.Count == 0)
                {
                    if (i == indexOfMaxTheorPeak + 1)  // first peak to the right of the max peak.  We need this one or we declare it to be a failure (= null)
                    {
                        failedResult = true;
                        break;
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


            if (failedResult)
            {
                return null;   // return a null Isotopic profile, indicating a failed result
            }
            else
            {
                addMassInfoToIsotopicProfile(theorFeature, outFeature);
                return outFeature;
            }










        }

        private void addMassInfoToIsotopicProfile(IsotopicProfile theorFeature, IsotopicProfile outFeature)
        {
            int indexOfMonoPeak = PeakUtilities.getIndexOfClosestValue(outFeature.Peaklist, theorFeature.MonoPeakMZ, 0, outFeature.Peaklist.Count - 1, 0.1);
            outFeature.MonoIsotopicPeakIndex = indexOfMonoPeak;


            double monopeakMZ = 0;
            double monoIsotopicMass = 0;
            bool monoPeakFoundInObservedIso = (outFeature.MonoIsotopicPeakIndex != -1);
            if (monoPeakFoundInObservedIso)
            {
                MSPeak monoPeak = outFeature.Peaklist[outFeature.MonoIsotopicPeakIndex];

                monopeakMZ = monoPeak.XValue;
                monoIsotopicMass = (monoPeak.XValue - Globals.PROTON_MASS) * outFeature.ChargeState;
               

            }
            else
            {

                int indexOfMostAbundantTheorPeak = theorFeature.getIndexOfMostIntensePeak();
                int indexOfCorrespondingObservedPeak = PeakUtilities.getIndexOfClosestValue(outFeature.Peaklist, theorFeature.Peaklist[indexOfMostAbundantTheorPeak].XValue, 0, outFeature.Peaklist.Count - 1, 0.1);

                if (indexOfCorrespondingObservedPeak != -1)
                {

                    //double mzOffset = outFeature.Peaklist[indexOfCorrespondingObservedPeak].XValue - theorFeature.Peaklist[indexOfMostAbundantTheorPeak].XValue;

                    int locationOfMonoPeakRelativeToTheorMaxPeak = theorFeature.MonoIsotopicPeakIndex - indexOfMostAbundantTheorPeak;

                    double mzOfObservedMostAbundantPeak = outFeature.Peaklist[indexOfCorrespondingObservedPeak].XValue;

                    monopeakMZ = mzOfObservedMostAbundantPeak + (locationOfMonoPeakRelativeToTheorMaxPeak * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / outFeature.ChargeState);
                    monoIsotopicMass = (monopeakMZ - Globals.PROTON_MASS) * outFeature.ChargeState;
                }
            }

            outFeature.MonoPeakMZ = monopeakMZ;
            outFeature.MonoIsotopicMass = monoIsotopicMass;













        }




        public override void Execute(ResultCollection resultColl)
        {
            Check.Require(resultColl != null && resultColl.Run != null, String.Format("{0} failed. Run is empty.", this.Name));
            Check.Require(resultColl.Run.CurrentMassTag != null, String.Format("{0} failed. CurrentMassTag hasn't been defined.", this.Name));

            MassTagResultBase result = resultColl.GetMassTagResult(resultColl.Run.CurrentMassTag);

            IsotopicProfile iso;

            resultColl.IsosResultBin.Clear();

            this.RunIsAligned = resultColl.Run.MassIsAligned;



            IsotopicProfile targetedIso = createTargetIso(resultColl.Run);
            iso = FindMSFeature(resultColl.Run.PeakList, targetedIso, this.ToleranceInPPM, this.NeedMonoIsotopicPeak);


            switch (IsotopicProfileType)
            {
                case IsotopicProfileType.UNLABELLED:
                    result.IsotopicProfile = iso;
                    break;
                case IsotopicProfileType.LABELLED:
                    result.AddLabelledIso(iso);
                    break;
                default:
                    result.IsotopicProfile = iso;
                    break;
            }

            bool isoIsGood = (iso != null && iso.Peaklist != null && iso.Peaklist.Count > 0);
            if (isoIsGood)
            {
                iso.IntensityAggregate = sumPeaks(iso, this.NumPeaksUsedInAbundance, 0);
            }
            else
            {
                result.FailedResult = true;     //note: for labelled isotopic profiles, this error will be assigned to the result if one of the two isotopic profiles is missing 
                result.FailureType = Globals.TargetedResultFailureType.MSFEATURE_NOT_FOUND;
            }

            resultColl.IsosResultBin.Add(result);

        }

        private IsotopicProfile createTargetIso(Run run)
        {
            IsotopicProfile iso;

            switch (this.IsotopicProfileType)
            {
                case IsotopicProfileType.UNLABELLED:
                    iso = run.CurrentMassTag.IsotopicProfile.CloneIsotopicProfile();
                    break;
                case IsotopicProfileType.LABELLED:
                    iso = run.CurrentMassTag.IsotopicProfileLabelled.CloneIsotopicProfile();
                    break;
                default:
                    iso = run.CurrentMassTag.IsotopicProfile.CloneIsotopicProfile();
                    break;
            }

            //adjust the target m/z based on the alignment information
            if (this.RunIsAligned)
            {
                for (int i = 0; i < iso.Peaklist.Count; i++)
                {
                    iso.Peaklist[i].XValue = run.GetTargetMZAligned(iso.Peaklist[i].XValue);

                }

            }


            return iso;

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




        public bool RunIsAligned { get; set; }
    }
}
