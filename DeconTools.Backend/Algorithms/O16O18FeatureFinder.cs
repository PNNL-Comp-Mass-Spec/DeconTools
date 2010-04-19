﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public IsotopicProfile FindFeature(List<IPeak> peakList, IsotopicProfile theorFeature, double toleranceInPPM, bool requireMonoPeak)
        {

            BasicMSFeatureFinder basicFeatureFinder = new BasicMSFeatureFinder();

            IsotopicProfile o16profile = basicFeatureFinder.FindMSFeature(peakList, theorFeature, toleranceInPPM, false);

            if (o16profile == null) return null;

            o16profile.ChargeState = theorFeature.ChargeState;

            IsotopicProfile theorO18SingleLabel = convertO16ProfileToO18(theorFeature, 2);
            IsotopicProfile theorO18DoubleLabel = convertO16ProfileToO18(theorFeature, 4);

            IsotopicProfile o18SingleLabelProfile = basicFeatureFinder.FindMSFeature(peakList, theorO18SingleLabel, toleranceInPPM, false);
            IsotopicProfile o18DoubleLabelprofile = basicFeatureFinder.FindMSFeature(peakList, theorO18DoubleLabel, toleranceInPPM, false);


            //TO BE DELETED:
            //if (o18SingleLabelProfile != null)
            //{
            //    o16profile.Peaklist = o16profile.Peaklist.Union(o18SingleLabelProfile.Peaklist).ToList();
            //}
            //if (o18DoubleLabelprofile != null)
            //{
            //    o16profile.Peaklist = o16profile.Peaklist.Union(o18DoubleLabelprofile.Peaklist).ToList();
            //}


            IsotopicProfile foundO16O18Profile = new IsotopicProfile();
            foundO16O18Profile.ChargeState = theorFeature.ChargeState;

            addIsotopePeaks(foundO16O18Profile, o16profile, 2);
            addIsotopePeaks(foundO16O18Profile, o18SingleLabelProfile, 2);
            addIsotopePeaks(foundO16O18Profile, o18DoubleLabelprofile, 100);    // add as many peaks as possible
            


            lookForMissingPeaksAndInsertZeroIntensityPeaksWhenMissing(foundO16O18Profile, theorFeature);

            return foundO16O18Profile;


        }

        private void addIsotopePeaks(IsotopicProfile foundO16O18Profile, IsotopicProfile profileToAdd, int numIsotopePeaksToAdd)
        {
            if (profileToAdd == null || profileToAdd.Peaklist == null || profileToAdd.Peaklist.Count == 0) return;

            for (int i = 0; i < numIsotopePeaksToAdd; i++)
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

        private IsotopicProfile convertO16ProfileToO18(IsotopicProfile theorFeature, int numPeaksToShift)
        {
            IsotopicProfile o18Iso = new IsotopicProfile();
            o18Iso.ChargeState = theorFeature.ChargeState;

            o18Iso.Peaklist = new List<MSPeak>();


            double mzBetweenIsotopes = 1.003 / theorFeature.ChargeState;


            foreach (var theorpeak in theorFeature.Peaklist)
            {
                MSPeak peak = new MSPeak(theorpeak.XValue, theorpeak.Height, theorpeak.Width, theorpeak.SN);

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
