using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public abstract class TFFBase : Task
    {
        protected TFFBase(double toleranceInPPM = 20, bool needMonoIsotopicPeak = false)

        {
            ToleranceInPPM = toleranceInPPM;
            NeedMonoIsotopicPeak = needMonoIsotopicPeak;
        }

        #region Properties

        public double ToleranceInPPM { get; set; }

        /// <summary>
        /// If true, then FeatureFinder must find the monoIsotopic peak or no feature is reported. (Useful for most peptides or small MassTags)
        /// </summary>
        public bool NeedMonoIsotopicPeak { get; set; }

        /// <summary>
        /// Each MassTag has two possible isotopic profiles (unlabeled and labeled).
        /// This property specifies which of the two are to be targeted in the real data.
        /// This property is mainly used in workflows that follow a Task-based implementation.
        /// </summary>
        public Globals.IsotopicProfileType IsotopicProfileType { get; set; }

        public int NumPeaksUsedInAbundance { get; set; }

        #endregion

        #region Public Methods
        public virtual IsotopicProfile FindMSFeature(List<Peak> peakList, IsotopicProfile theorFeature)
        {
            Check.Require(theorFeature != null, "Theoretical feature hasn't been defined.");
            if (theorFeature == null)
            {
                return null;
            }

            Check.Require(theorFeature.Peaklist?.Count > 0, "Theoretical feature hasn't been defined.");

            var outFeature = new IsotopicProfile
            {
                ChargeState = theorFeature.ChargeState
            };

            var indexOfMaxTheorPeak = theorFeature.GetIndexOfMostIntensePeak();

            var toleranceInMZ = theorFeature.getMonoPeak().XValue * ToleranceInPPM / 1e6;

            var foundMatchingMaxPeak = false;
            double massDefect = 0;   // this is the m/z diff between the max peak of theor feature and the max peak of the experimental feature

            var failedResult = false;

            for (var i = indexOfMaxTheorPeak; i >= 0; i--)
            {
                //find experimental peak(s) within range
                var peaksWithinTol = PeakUtilities.GetPeaksWithinTolerance(peakList, theorFeature.Peaklist[i].XValue, toleranceInMZ);

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
                    if (NeedMonoIsotopicPeak)
                    {
                        //here, we are looking to the left of most intense theor peak.  If we have the prerequisite of finding the monoIsotopic peak and fail here, we'll return a failed result
                        failedResult = true;
                    }
                    break;  // stop looking to the left of the most intense peak.
                }

                if (peaksWithinTol.Count == 1)
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
                        bestPeak = (MSPeak)findMostIntensePeak(peaksWithinTol);
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

                if (i == indexOfMaxTheorPeak)   //when matching to most intense peak, we will get the mass defect using the most intense peak
                {
                    massDefect = theorFeature.Peaklist[i].XValue - outFeature.Peaklist[0].XValue;
                }
            }

            //------------------------- look right -------------------------------------------
            for (var i = indexOfMaxTheorPeak + 1; i < theorFeature.Peaklist.Count; i++)     //start one peak to the right of the max intense theor peak
            {
                var peaksWithinTol = PeakUtilities.GetPeaksWithinTolerance(peakList, theorFeature.Peaklist[i].XValue, toleranceInMZ);
                if (peaksWithinTol.Count == 0)
                {
                    if (i == indexOfMaxTheorPeak + 1)  // first peak to the right of the max peak.  We need this one or we declare it to be a failure (= null)
                    {
                        failedResult = true;
                    }
                    break;    // finished.  Exit loop.
                }

                if (peaksWithinTol.Count == 1)
                {
                    outFeature.Peaklist.Add((MSPeak)peaksWithinTol[0]);   //here, we tack peaks onto the profile
                }
                else    //two or more peaks are within tolerance. Need to get the best one, which is based on the distance from the
                {
                    outFeature.Peaklist.Add((MSPeak)findClosestToXValue(peaksWithinTol, theorFeature.Peaklist[i].XValue - massDefect));
                }
            }

            //for higher mass peptides, we will return the profile if there is 2 or more peaks, regardless if none are found to the right of the most abundant
            if (indexOfMaxTheorPeak > 0 && outFeature.Peaklist.Count > 1)
            {
                failedResult = false;
            }

            if (failedResult)
            {
                return null;   // return a null Isotopic profile, indicating a failed result
            }

            addMassInfoToIsotopicProfile(theorFeature, outFeature);
            return outFeature;
        }

        private void addMassInfoToIsotopicProfile(IsotopicProfile theorFeature, IsotopicProfile outFeature)
        {
            var indexOfMonoPeak = PeakUtilities.getIndexOfClosestValue(outFeature.Peaklist, theorFeature.MonoPeakMZ, 0, outFeature.Peaklist.Count - 1, 0.1);
            outFeature.MonoIsotopicPeakIndex = indexOfMonoPeak;

            double monoIsotopicPeakMZ = 0;
            double monoIsotopicMass = 0;
            var monoPeakFoundInObservedIso = (outFeature.MonoIsotopicPeakIndex != -1);
            if (monoPeakFoundInObservedIso)
            {
                var monoPeak = outFeature.Peaklist[outFeature.MonoIsotopicPeakIndex];

                monoIsotopicPeakMZ = monoPeak.XValue;
                monoIsotopicMass = (monoPeak.XValue - Globals.PROTON_MASS) * outFeature.ChargeState;
            }
            else
            {
                var indexOfMostAbundantTheorPeak = theorFeature.GetIndexOfMostIntensePeak();
                var indexOfCorrespondingObservedPeak = PeakUtilities.getIndexOfClosestValue(outFeature.Peaklist, theorFeature.Peaklist[indexOfMostAbundantTheorPeak].XValue, 0, outFeature.Peaklist.Count - 1, 0.1);

                if (indexOfCorrespondingObservedPeak != -1)
                {
                    //double mzOffset = outFeature.Peaklist[indexOfCorrespondingObservedPeak].XValue - theorFeature.Peaklist[indexOfMostAbundantTheorPeak].XValue;

                    var locationOfMonoPeakRelativeToTheorMaxPeak = theorFeature.MonoIsotopicPeakIndex - indexOfMostAbundantTheorPeak;

                    var mzOfObservedMostAbundantPeak = outFeature.Peaklist[indexOfCorrespondingObservedPeak].XValue;

                    monoIsotopicPeakMZ = mzOfObservedMostAbundantPeak + (locationOfMonoPeakRelativeToTheorMaxPeak * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / outFeature.ChargeState);
                    monoIsotopicMass = (monoIsotopicPeakMZ - Globals.PROTON_MASS) * outFeature.ChargeState;
                }
            }

            outFeature.MonoPeakMZ = monoIsotopicPeakMZ;
            outFeature.MonoIsotopicMass = monoIsotopicMass;
        }

        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList?.Run != null, string.Format("{0} failed. Run is empty.", Name));
            if (resultList?.Run == null)
            {
                return;
            }

            Check.Require(resultList.Run.CurrentMassTag != null, string.Format("{0} failed. CurrentMassTag hasn't been defined.", Name));

            var result = resultList.CurrentTargetedResult;

            resultList.IsosResultBin.Clear();

            RunIsAligned = resultList.Run.MassIsAligned;

            var targetedIso = CreateTargetIso(resultList.Run);
            var iso = FindMSFeature(resultList.Run.PeakList, targetedIso);

            switch (IsotopicProfileType)
            {
                case Globals.IsotopicProfileType.UNLABELED:
                    result.IsotopicProfile = iso;
                    break;
                case Globals.IsotopicProfileType.LABELED:
                    result.AddLabeledIso(iso);
                    break;
                default:
                    result.IsotopicProfile = iso;
                    break;
            }

            var isoIsGood = (iso?.Peaklist != null && iso.Peaklist.Count > 0);
            if (isoIsGood)
            {
                //GORD: check this later
                result.IntensityAggregate = sumPeaks(iso, NumPeaksUsedInAbundance, 0);
            }
            else
            {
                result.FailedResult = true;     //note: for labeled isotopic profiles, this error will be assigned to the result if one of the two isotopic profiles is missing
                result.FailureType = Globals.TargetedResultFailureType.MsfeatureNotFound;
            }

            resultList.IsosResultBin.Add(result);
        }

        protected virtual IsotopicProfile CreateTargetIso(Run run)
        {
            IsotopicProfile iso;
            Check.Require(run.CurrentMassTag != null, "Run's 'CurrentMassTag' has not been declared");

            if (run.CurrentMassTag == null)
            {
                return null;
            }

            Check.Require(run.CurrentMassTag.IsotopicProfile != null, "Run's 'IsotopicProfile' has not been declared");
            if (run.CurrentMassTag?.IsotopicProfile == null)
            {
                return null;
            }

            switch (IsotopicProfileType)
            {
                case Globals.IsotopicProfileType.UNLABELED:
                    Check.Require(run.CurrentMassTag.IsotopicProfile != null, "Target's theoretical isotopic profile has not been established");
                    iso = run.CurrentMassTag.IsotopicProfile.CloneIsotopicProfile();
                    break;

                case Globals.IsotopicProfileType.LABELED:
                    Check.Require(run.CurrentMassTag.IsotopicProfileLabeled != null, "Target's labeled theoretical isotopic profile has not been established");
                    if (run.CurrentMassTag?.IsotopicProfileLabeled == null)
                    {
                        return null;
                    }

                    iso = run.CurrentMassTag.IsotopicProfileLabeled.CloneIsotopicProfile();
                    break;

                default:
                    iso = run.CurrentMassTag.IsotopicProfile.CloneIsotopicProfile();
                    break;
            }

            //adjust the target m/z based on the alignment information
            if (run.MassIsAligned)
            {
                foreach (var peak in iso.Peaklist)
                {
                    peak.XValue = run.GetTargetMZAligned(peak.XValue);
                }
            }

            return iso;
        }
        #endregion

        #region Private Methods

        //private void addInfoToResult(IsotopicProfile iso, TargetBase mt)
        //{
        //    if (iso != null)
        //    {
        //        iso.ChargeState = mt.ChargeState;
        //        iso.MonoIsotopicMass = (iso.GetMZ() - Globals.PROTON_MASS) * mt.ChargeState;
        //        iso.IntensityMostAbundant = iso.getMostIntensePeak().Height;     // may need to change this to sum the top n peaks.
        //    }
        //}

        private Peak findMostIntensePeak(IReadOnlyList<Peak> peaksWithinTol)
        {
            double maxIntensity = 0;
            Peak mostIntensePeak = null;

            foreach (var peak in peaksWithinTol)
            {
                var obsIntensity = peak.Height;
                if (obsIntensity > maxIntensity)
                {
                    maxIntensity = obsIntensity;
                    mostIntensePeak = peak;
                }
            }
            return mostIntensePeak;
        }

        //private MSPeak findClosestToXValue(List<MSPeak> list, double p)
        //{
        //    throw new NotImplementedException();
        //}

        private Peak findClosestToXValue(IReadOnlyList<Peak> peaksWithinTol, double targetVal)
        {
            var diff = double.MaxValue;
            Peak closestPeak = null;

            foreach (var peak in peaksWithinTol)
            {
                var obsDiff = Math.Abs(peak.XValue - targetVal);

                if (obsDiff < diff)
                {
                    diff = obsDiff;
                    closestPeak = peak;
                }
            }

            return closestPeak;
        }

        private double sumPeaks(IsotopicProfile profile, int numPeaksUsedInAbundance, int defaultVal)
        {
            if (profile.Peaklist == null || profile.Peaklist.Count == 0)
            {
                return defaultVal;
            }

            var peakListIntensities = new List<float>();
            foreach (var peak in profile.Peaklist)
            {
                peakListIntensities.Add(peak.Height);
            }
            peakListIntensities.Sort();
            peakListIntensities.Reverse();    // i know... this isn't the best way to do this!
            double summedIntensities = 0;

            for (var i = 0; i < peakListIntensities.Count; i++)
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
