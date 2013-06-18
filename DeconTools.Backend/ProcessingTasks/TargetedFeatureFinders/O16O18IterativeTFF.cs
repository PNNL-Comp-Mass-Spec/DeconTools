using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public class O16O18IterativeTff:IterativeTFF
    {
        readonly IterativeTFF _iterativeTffStandard;
        #region Constructors
        public O16O18IterativeTff(IterativeTFFParameters parameters)
            : base(parameters)
        {
            _iterativeTffStandard = new IterativeTFF(parameters);
        }

        #endregion

        #region Properties

        #endregion

        #region Public Methods

        public override IsotopicProfile IterativelyFindMSFeature(XYData xydata, IsotopicProfile theorIso)
        {
            IsotopicProfile o16TheorFeature = theorIso;
            IsotopicProfile o16Profile = _iterativeTffStandard.IterativelyFindMSFeature(xydata, o16TheorFeature);
            
            IsotopicProfile o18TheorProfileSingleLabel = convertO16ProfileToO18(o16TheorFeature, 2);
            IsotopicProfile o18SingleLabelProfile = _iterativeTffStandard.IterativelyFindMSFeature(xydata, o18TheorProfileSingleLabel);
         
            IsotopicProfile o18TheorProfileDoubleLabel = convertO16ProfileToO18(o16TheorFeature, 4);
            IsotopicProfile o18DoubleLabelProfile = _iterativeTffStandard.IterativelyFindMSFeature(xydata, o18TheorProfileDoubleLabel);

           IsotopicProfile foundO16O18Profile;

            if (o16Profile == null)
            {

                if (o18DoubleLabelProfile == null)
                {
                    return null;
                    
                }

                foundO16O18Profile = o18DoubleLabelProfile.CloneIsotopicProfile();
                foundO16O18Profile.MonoIsotopicMass = o18DoubleLabelProfile.MonoIsotopicMass - 4 * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS;
                foundO16O18Profile.MonoPeakMZ = foundO16O18Profile.MonoIsotopicMass / foundO16O18Profile.ChargeState +
                                                Globals.PROTON_MASS;
            }
            else
            {
                foundO16O18Profile = o16Profile.CloneIsotopicProfile();
            }

            //rebuild isotopic peak list
            AddIsotopePeaks(foundO16O18Profile, o16Profile, 2);
            AddIsotopePeaks(foundO16O18Profile, o18SingleLabelProfile, 4);      // add the O18(1) and the O18(2) if present
            AddIsotopePeaks(foundO16O18Profile, o18DoubleLabelProfile, 100);    // add as many peaks as possible

            lookForMissingPeaksAndInsertZeroIntensityPeaksWhenMissing(foundO16O18Profile, o16TheorFeature);

            return foundO16O18Profile;


        }



        #endregion

        #region Private Methods
        private IsotopicProfile convertO16ProfileToO18(IsotopicProfile theorFeature, int numPeaksToShift)
        {
            var o18Iso = new IsotopicProfile {ChargeState = theorFeature.ChargeState, Peaklist = new List<MSPeak>()};
            double mzBetweenIsotopes = 1.003 / theorFeature.ChargeState;

            foreach (var theorpeak in theorFeature.Peaklist)
            {
                var peak = new MSPeak(theorpeak.XValue, theorpeak.Height, theorpeak.Width, theorpeak.SignalToNoise);

                peak.XValue += numPeaksToShift * mzBetweenIsotopes;

                o18Iso.Peaklist.Add(peak);

            }

            return o18Iso;
        }
        private void AddIsotopePeaks(IsotopicProfile foundO16O18Profile, IsotopicProfile profileToAdd, int numIsotopePeaksToAdd)
        {
            if (profileToAdd == null || profileToAdd.Peaklist == null || profileToAdd.Peaklist.Count == 0) return;

            for (int i = 0; i < numIsotopePeaksToAdd; i++)
            {
                if (i < profileToAdd.Peaklist.Count)
                {
                    double peakmz = profileToAdd.Peaklist[i].XValue;
                    double toleranceInMz = ToleranceInPPM / 1e6 * peakmz;

                    List<MSPeak> peaksAlreadyThere = PeakUtilities.GetMSPeaksWithinTolerance(foundO16O18Profile.Peaklist, peakmz, toleranceInMz);

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
        private void lookForMissingPeaksAndInsertZeroIntensityPeaksWhenMissing(IsotopicProfile o16O18Profile, IsotopicProfile theorFeature)
        {
            if (o16O18Profile.Peaklist.Count == 0) return;

            double mzDistanceBetweenIsotopes = 1.003 / o16O18Profile.ChargeState;

            double monoMz = theorFeature.getMonoPeak().XValue;

            const double toleranceInDa = 0.1;


            //this will iterate over the first five expected m/z values of a theoretical profile 
            //and loosely try to the corresponding peak within the observed profile. 
            //If missing, will add one at the expected m/z.  This ensures no missing peaks within the O16O18 profile
            //so that looking up the first peak will always give you the intensity of the O16 peak (even if
            //it never existed in the real data - in this case the intensity is 0);
            for (int i = 0; i < 6; i++)
            {
                double currentMz = monoMz + mzDistanceBetweenIsotopes * i;

                List<MSPeak> peaksWithinTol = PeakUtilities.GetMSPeaksWithinTolerance(o16O18Profile.Peaklist, currentMz, toleranceInDa);
                if (peaksWithinTol.Count == 0)   // 
                {
                    o16O18Profile.Peaklist.Insert(i, new MSPeak(currentMz, 0, 0, 0));
                }
            }


        }

        #endregion

       
    }
}
