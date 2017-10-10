using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.Algorithms
{
    public class O16O18FeatureFinder
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods

        public IsotopicProfile FindFeature(List<Peak> peakList, IsotopicProfile theorFeature, double toleranceInPPM, bool requireMonoPeak)
        {

            var basicFeatureFinder = new BasicMSFeatureFinder();

            var o16profile = basicFeatureFinder.FindMSFeature(peakList, theorFeature, toleranceInPPM, false);

            if (o16profile == null) return null;

            o16profile.ChargeState = theorFeature.ChargeState;

            var theorO18SingleLabel = convertO16ProfileToO18(theorFeature, 2);
            var theorO18DoubleLabel = convertO16ProfileToO18(theorFeature, 4);

            var o18SingleLabelProfile = basicFeatureFinder.FindMSFeature(peakList, theorO18SingleLabel, toleranceInPPM, false);
            var o18DoubleLabelprofile = basicFeatureFinder.FindMSFeature(peakList, theorO18DoubleLabel, toleranceInPPM, false);


            //TO BE DELETED:
            //if (o18SingleLabelProfile != null)
            //{
            //    o16profile.Peaklist = o16profile.Peaklist.Union(o18SingleLabelProfile.Peaklist).ToList();
            //}
            //if (o18DoubleLabelprofile != null)
            //{
            //    o16profile.Peaklist = o16profile.Peaklist.Union(o18DoubleLabelprofile.Peaklist).ToList();
            //}


            var foundO16O18Profile = new IsotopicProfile {
                ChargeState = theorFeature.ChargeState
            };

            addIsotopePeaks(foundO16O18Profile, o16profile, 2);
            addIsotopePeaks(foundO16O18Profile, o18SingleLabelProfile, 2);
            addIsotopePeaks(foundO16O18Profile, o18DoubleLabelprofile, 100);    // add as many peaks as possible



            lookForMissingPeaksAndInsertZeroIntensityPeaksWhenMissing(foundO16O18Profile, theorFeature);

            return foundO16O18Profile;


        }

        private void addIsotopePeaks(IsotopicProfile foundO16O18Profile, IsotopicProfile profileToAdd, int numIsotopePeaksToAdd)
        {
            if (profileToAdd?.Peaklist == null || profileToAdd.Peaklist.Count == 0) return;

            for (var i = 0; i < numIsotopePeaksToAdd; i++)
            {
                if (i < profileToAdd.Peaklist.Count - 1)
                {
                    foundO16O18Profile.Peaklist.Add(profileToAdd.Peaklist[i]);
                }
                else    // if profileToAdd doesn't have enough peaks
                {
                    break;
                }

            }
        }

        private void lookForMissingPeaksAndInsertZeroIntensityPeaksWhenMissing(IsotopicProfile o16o18Profile, IsotopicProfile theorFeature)
        {
            if (o16o18Profile.Peaklist.Count == 0) return;

            var mzDistanceBetweenIsotopes = 1.003 / o16o18Profile.ChargeState;

            var monoMZ = theorFeature.getMonoPeak().XValue;

            var toleranceInDa = 0.1;


            //this will iterate over the first five expected m/z values of a theoretical profile
            //and loosely try to the corresponding peak within the observed profile.
            //If missing, will add one at the expected m/z.  This ensures no missing peaks within the O16O18 profile
            //so that looking up the first peak will always give you the intensity of the O16 peak (even if
            //it never existed in the real data - in this case the intensity is 0);
            for (var i = 0; i < 6; i++)
            {
                var currentMZ = monoMZ + mzDistanceBetweenIsotopes * i;

                var peaksWithinTol = PeakUtilities.GetMSPeaksWithinTolerance(o16o18Profile.Peaklist, currentMZ, toleranceInDa);
                if (peaksWithinTol.Count == 0)
                {
                    o16o18Profile.Peaklist.Insert(i, new MSPeak(currentMZ));
                }
            }


        }

        private IsotopicProfile convertO16ProfileToO18(IsotopicProfile theorFeature, int numPeaksToShift)
        {
            var o18Iso = new IsotopicProfile
            {
                ChargeState = theorFeature.ChargeState,
                Peaklist = new List<MSPeak>()
            };



            var mzBetweenIsotopes = 1.003 / theorFeature.ChargeState;


            foreach (var theorpeak in theorFeature.Peaklist)
            {
                var peak = new MSPeak(theorpeak.XValue, theorpeak.Height, theorpeak.Width, theorpeak.SignalToNoise);

                peak.XValue += numPeaksToShift * mzBetweenIsotopes;

                o18Iso.Peaklist.Add(peak);

            }

            return o18Iso;
        }


        #endregion

        #region Private Methods
        #endregion
    }
}
