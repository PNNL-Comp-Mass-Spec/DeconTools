using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

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
            
            IsotopicProfile theorO18SingleLabel = convertO16ProfileToO18(theorFeature, 2);
            IsotopicProfile theorO18DoubleLabel = convertO16ProfileToO18(theorFeature, 4);

            IsotopicProfile o18SingleLabelProfile = basicFeatureFinder.FindMSFeature(peakList, theorO18SingleLabel, toleranceInPPM, false);
            IsotopicProfile o18DoubleLabelprofile = basicFeatureFinder.FindMSFeature(peakList, theorO18DoubleLabel, toleranceInPPM, false);

            o16profile.Peaklist = o16profile.Peaklist.Union(o18SingleLabelProfile.Peaklist).ToList();
            o16profile.Peaklist = o16profile.Peaklist.Union(o18DoubleLabelprofile.Peaklist).ToList();

            return o16profile;


        }

        private IsotopicProfile convertO16ProfileToO18(IsotopicProfile theorFeature, int numPeaksToShift)
        {
            IsotopicProfile o18Iso = new IsotopicProfile();
            o18Iso.ChargeState = theorFeature.ChargeState;

            o18Iso.Peaklist = new List<MSPeak>(theorFeature.Peaklist);
            
            double mzBetweenIsotopes = 1.003 / theorFeature.ChargeState;


            foreach (var peak in o18Iso.Peaklist)
            {
                peak.XValue += numPeaksToShift * mzBetweenIsotopes;
               
            }

            return o18Iso;
        }


        #endregion

        #region Private Methods
        #endregion
    }
}
