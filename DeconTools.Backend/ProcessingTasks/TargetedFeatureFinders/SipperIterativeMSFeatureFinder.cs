using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public class SipperIterativeMSFeatureFinder : IterativeTFF
    {
        private float _valueForAppendedTheorPeaks = 0.0001f;

        #region Constructors
        public SipperIterativeMSFeatureFinder(IterativeTFFParameters parameters)
            : base(parameters)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            ToleranceInPPM = parameters.ToleranceInPPM;
        }

        #endregion

        #region Properties

        #endregion

        #region Public Methods

        protected override IsotopicProfile CreateTargetIso(Run run)
        {
            IsotopicProfile iso;
            Check.Require(run.CurrentMassTag != null, "Run's 'CurrentMassTag' has not been declared");

            switch (IsotopicProfileType)
            {
                case Globals.IsotopicProfileType.UNLABELED:
                    if (run.CurrentMassTag?.IsotopicProfile == null)
                        return null;

                    Check.Require(run.CurrentMassTag.IsotopicProfile != null, "Target's theoretical isotopic profile has not been established");
                    iso = run.CurrentMassTag.IsotopicProfile.CloneIsotopicProfile();
                    break;

                case Globals.IsotopicProfileType.LABELED:
                    if (run.CurrentMassTag?.IsotopicProfileLabeled == null)
                        return null;

                    Check.Require(run.CurrentMassTag.IsotopicProfileLabeled != null, "Target's labeled theoretical isotopic profile has not been established");
                    iso = run.CurrentMassTag.IsotopicProfileLabeled.CloneIsotopicProfile();
                    break;

                default:
                    if (run.CurrentMassTag?.IsotopicProfile == null)
                        return null;

                    iso = run.CurrentMassTag.IsotopicProfile.CloneIsotopicProfile();
                    break;
            }

            for (var i = 0; i < MaxPeaksToInclude; i++)
            {
                if (i < iso.Peaklist.Count) continue;

                var lastPeak = iso.Peaklist[iso.Peaklist.Count - 1];

                var mz = lastPeak.XValue + Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / iso.ChargeState;
                var intensity = _valueForAppendedTheorPeaks;
                var fwhm = lastPeak.Width;

                var newPeak = new MSPeak(mz, intensity, fwhm)
                {
                    MSFeatureID = lastPeak.MSFeatureID
                };

                iso.Peaklist.Add(newPeak);
            }

            //adjust the target m/z based on the alignment information
            if (run.MassIsAligned)
            {
                foreach (var item in iso.Peaklist)
                {
                    item.XValue = run.GetTargetMZAligned(item.XValue);
                }
            }

            return iso;
        }

        // Sipper's key method. Sipper will try to pull out low level C13-related peaks. So
        // it will not stop as early as the BasicTFF, but will iterate more times to pull
        // out the smaller intensity C13-related peaks
        public override IsotopicProfile IterativelyFindMSFeature(XYData massSpecXyData, IsotopicProfile theorIso, out List<Peak> peakList)
        {
            IsotopicProfile foundIso = null;

            //start with high PeakBR and ratchet it down, so as to detect more peaks with each pass.  Stop when you find the isotopic profile.

            peakList = new List<Peak>();
            for (var d = PeakDetectorPeakBR; d >= PeakBRMin; d = d - PeakBRStep)
            {
                MSPeakDetector.PeakToBackgroundRatio = d;

                peakList = MSPeakDetector.FindPeaks(massSpecXyData.Xvalues, massSpecXyData.Yvalues);
                var iso = FindMSFeature(peakList, theorIso);

                if (foundIso == null)
                {
                    foundIso = iso;
                }

                if (foundIso != null)
                {
                    double toleranceInPPM = 20;
                    combineIsotopicProfiles(foundIso, iso, toleranceInPPM);
                }

                //TODO: decide that iso is good enough when a peak is found that is less than a certain relative intensity

                var isoIsGoodEnough = (foundIso != null && foundIso.Peaklist.Count >= MaxPeaksToInclude);

                if (isoIsGoodEnough)
                {
                    break;
                }
            }

            if (foundIso != null)
            {
                AddZeroIntensityPeaks(foundIso, theorIso, 0.02d);
            }

            return foundIso;
        }

        private void AddZeroIntensityPeaks(IsotopicProfile foundIso, IsotopicProfile theorIso, double minIntensityThreshold)
        {
            var clonedTheor = theorIso.CloneIsotopicProfile();

            clonedTheor.Peaklist = clonedTheor.Peaklist.Where(p => p.Height > minIntensityThreshold).ToList();

            foreach (var msPeak in clonedTheor.Peaklist)
            {
                msPeak.Height = 0;
            }

            combineIsotopicProfiles(foundIso, clonedTheor);
        }

        private void combineIsotopicProfiles(IsotopicProfile baseIso, IsotopicProfile addedIso, double toleranceInPPM = 50)
        {
            var toleranceInMZ = toleranceInPPM * baseIso.MonoPeakMZ / 1e6;

            foreach (var msPeak in addedIso.Peaklist)
            {
                var indexOfPeakInBaseIos = PeakUtilities.getIndexOfClosestValue(baseIso.Peaklist, msPeak.XValue, 0, baseIso.Peaklist.Count - 1,
                                                     toleranceInMZ);

                if (indexOfPeakInBaseIos == -1)
                {
                    baseIso.Peaklist.Add(msPeak);
                }
            }

            baseIso.Peaklist = baseIso.Peaklist.OrderBy(p => p.XValue).ToList();
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
