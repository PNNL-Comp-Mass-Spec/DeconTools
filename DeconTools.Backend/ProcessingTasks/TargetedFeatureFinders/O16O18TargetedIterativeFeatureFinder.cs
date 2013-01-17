using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public class O16O18TargetedIterativeFeatureFinder : IterativeTFF
    {

        IterativeTFF _iterativeTFFStandard;

        public O16O18TargetedIterativeFeatureFinder(IterativeTFFParameters parameters) : base(parameters)
        {

            ToleranceInPPM = parameters.ToleranceInPPM;
            _iterativeTFFStandard = new IterativeTFF(parameters);


        }

        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList != null && resultList.Run != null, String.Format("{0} failed. Run is empty.", this.Name));
            Check.Require(resultList.Run.CurrentMassTag != null, String.Format("{0} failed. CurrentMassTag hasn't been defined.", this.Name));
            TargetedResultBase result = resultList.CurrentTargetedResult;


            if (resultList.Run.XYData == null || resultList.Run.XYData.Xvalues == null || resultList.Run.XYData.Xvalues.Length < 4)
            {
                result.IsotopicProfile = null;
                return;
            }

            //TODO: decide whether not to trim or not. Trimming XYData may help with speed. 
            double mzwindowForPeakDetection = 20;
            resultList.Run.XYData = resultList.Run.XYData.TrimData(resultList.Run.CurrentMassTag.MZ - mzwindowForPeakDetection / 2, resultList.Run.CurrentMassTag.MZ + mzwindowForPeakDetection / 2);

            resultList.IsosResultBin.Clear();

            IsotopicProfile o16TheorFeature = resultList.Run.CurrentMassTag.IsotopicProfile;
            IsotopicProfile o16profile = _iterativeTFFStandard.IterativelyFindMSFeature(resultList.Run, o16TheorFeature);

            IsotopicProfile o18TheorProfileSingleLabel = convertO16ProfileToO18(o16TheorFeature, 2);
            IsotopicProfile o18SingleLabelProfile = _iterativeTFFStandard.IterativelyFindMSFeature(resultList.Run, o18TheorProfileSingleLabel);

            IsotopicProfile o18TheorProfileDoubleLabel = convertO16ProfileToO18(o16TheorFeature, 4);
            IsotopicProfile o18DoubleLabelProfile = _iterativeTFFStandard.IterativelyFindMSFeature(resultList.Run, o18TheorProfileDoubleLabel);


            IsotopicProfile foundO16O18Profile;

            if (o16profile==null)
            {

                if (o18DoubleLabelProfile == null)
                {
                    result.IsotopicProfile = null;
                    result.FailedResult = true;
                    result.FailureType = Globals.TargetedResultFailureType.MsfeatureNotFound;
                    return;
                }
                
                foundO16O18Profile = o18DoubleLabelProfile.CloneIsotopicProfile();
                //lookForMissingPeaksAndInsertZeroIntensityPeaksWhenMissing(foundO16O18Profile, o16TheorFeature);
                foundO16O18Profile.MonoIsotopicMass = o18DoubleLabelProfile.MonoIsotopicMass -
                                                      4 * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS;

                foundO16O18Profile.MonoPeakMZ = foundO16O18Profile.MonoIsotopicMass / foundO16O18Profile.ChargeState +
                                                Globals.PROTON_MASS;
            }
            else
            {
                foundO16O18Profile = o16profile.CloneIsotopicProfile();
            }

            //rebuild isotopic peak list
            addIsotopePeaks(foundO16O18Profile, o16profile, 2);
            addIsotopePeaks(foundO16O18Profile, o18SingleLabelProfile, 4);      // add the O18(1) and the O18(2) if present
            addIsotopePeaks(foundO16O18Profile, o18DoubleLabelProfile, 100);    // add as many peaks as possible

            lookForMissingPeaksAndInsertZeroIntensityPeaksWhenMissing(foundO16O18Profile, o16TheorFeature);

            result.IsotopicProfile = foundO16O18Profile;

            resultList.IsosResultBin.Add(result);


        }


        private void lookForMissingPeaksAndInsertZeroIntensityPeaksWhenMissing(IsotopicProfile o16o18Profile, IsotopicProfile theorFeature)
        {
            if (o16o18Profile.Peaklist.Count == 0) return;

            double mzDistanceBetweenIsotopes = 1.003 / o16o18Profile.ChargeState;

            double monoMZ = theorFeature.getMonoPeak().XValue;

            int indexOfLastPeak = o16o18Profile.Peaklist.Count - 1;

            double toleranceInDa = 0.1;


            //this will iterate over the first five expected m/z values of a theoretical profile 
            //and loosely try to the corresponding peak within the observed profile. 
            //If missing, will add one at the expected m/z.  This ensures no missing peaks within the O16O18 profile
            //so that looking up the first peak will always give you the intensity of the O16 peak (even if
            //it never existed in the real data - in this case the intensity is 0);
            for (int i = 0; i < 6; i++)
            {
                double currentMZ = monoMZ + mzDistanceBetweenIsotopes * i;

                List<MSPeak> peaksWithinTol = PeakUtilities.GetMSPeaksWithinTolerance(o16o18Profile.Peaklist, currentMZ, toleranceInDa);
                if (peaksWithinTol.Count == 0)   // 
                {
                    o16o18Profile.Peaklist.Insert(i, new MSPeak(currentMZ, 0, 0, 0));
                }
            }


        }



        private void addIsotopePeaks(IsotopicProfile foundO16O18Profile, IsotopicProfile profileToAdd, int numIsotopePeaksToAdd)
        {
            if (profileToAdd == null || profileToAdd.Peaklist == null || profileToAdd.Peaklist.Count == 0) return;

            for (int i = 0; i < numIsotopePeaksToAdd; i++)
            {
                if (i < profileToAdd.Peaklist.Count)
                {
                    double peakmz = profileToAdd.Peaklist[i].XValue;
                    double toleranceInMZ = this.ToleranceInPPM / 1e6 * peakmz;

                    List<MSPeak> peaksAlreadyThere = PeakUtilities.GetMSPeaksWithinTolerance(foundO16O18Profile.Peaklist, peakmz, toleranceInMZ);

                    if (peaksAlreadyThere == null || peaksAlreadyThere.Count == 0)
                    {
                        foundO16O18Profile.Peaklist.Add(profileToAdd.Peaklist[i]);
                    }
                }
                else    // if profileToAdd doesn't have enough peaks
                {
                    break;
                }

            }


            foundO16O18Profile.Peaklist = foundO16O18Profile.Peaklist.OrderBy(p => p.XValue).ToList();
        }


        private IsotopicProfile convertO16ProfileToO18(IsotopicProfile theorFeature, int numPeaksToShift)
        {
            IsotopicProfile o18Iso = new IsotopicProfile();
            o18Iso.ChargeState = theorFeature.ChargeState;

            o18Iso.Peaklist = new List<MSPeak>();


            double mzBetweenIsotopes = 1.003 / theorFeature.ChargeState;


            foreach (var theorpeak in theorFeature.Peaklist)
            {
                MSPeak peak = new MSPeak(theorpeak.XValue, theorpeak.Height, theorpeak.Width, theorpeak.SignalToNoise);

                peak.XValue += numPeaksToShift * mzBetweenIsotopes;

                o18Iso.Peaklist.Add(peak);

            }

            return o18Iso;
        }


        private IsotopicProfile getTheorProfile(PeptideTarget massTag, Globals.IsotopicProfileType isotopicProfileType)
        {

            switch (isotopicProfileType)
            {
                case Globals.IsotopicProfileType.UNLABELLED:
                    return massTag.IsotopicProfile;

                case Globals.IsotopicProfileType.LABELLED:
                    return massTag.IsotopicProfileLabelled;

                default:
                    return massTag.IsotopicProfile;

            }
        }
    }
}
