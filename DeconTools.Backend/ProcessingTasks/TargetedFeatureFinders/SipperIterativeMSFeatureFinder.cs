using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public class SipperIterativeMSFeatureFinder : IterativeTFF
    {
        private int _maxPeaksToInclude = 30;
        private float _valueForAppendedTheorPeaks = 0.0001f;

        #region Constructors
        public SipperIterativeMSFeatureFinder(IterativeTFFParameters parameters)
            : base(parameters)
        {
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



            switch (this.IsotopicProfileType)
            {
                case IsotopicProfileType.UNLABELLED:
                    Check.Require(run.CurrentMassTag.IsotopicProfile != null, "Target's theoretical isotopic profile has not been established");
                    iso = run.CurrentMassTag.IsotopicProfile.CloneIsotopicProfile();

                    break;
                case IsotopicProfileType.LABELLED:
                    Check.Require(run.CurrentMassTag.IsotopicProfileLabelled != null, "Target's labelled theoretical isotopic profile has not been established");
                    iso = run.CurrentMassTag.IsotopicProfileLabelled.CloneIsotopicProfile();
                    break;
                default:
                    iso = run.CurrentMassTag.IsotopicProfile.CloneIsotopicProfile();
                    break;
            }


            for (int i = 0; i < _maxPeaksToInclude; i++)
            {
                if (i >= iso.Peaklist.Count)
                {
                    MSPeak lastPeak = iso.Peaklist[iso.Peaklist.Count - 1];

                    MSPeak newPeak = new MSPeak();
                    newPeak.XValue = lastPeak.XValue + Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / iso.ChargeState;
                    newPeak.Height = _valueForAppendedTheorPeaks;
                    newPeak.Width = lastPeak.Width;
                    newPeak.MSFeatureID = lastPeak.MSFeatureID;

                    iso.Peaklist.Add(newPeak);
                }
            }




            //adjust the target m/z based on the alignment information
            if (run.MassIsAligned)
            {
                for (int i = 0; i < iso.Peaklist.Count; i++)
                {
                    iso.Peaklist[i].XValue = run.GetTargetMZAligned(iso.Peaklist[i].XValue);


                }

            }


            return iso;
        }


        // Sipper's key method. Sipper will try to pull out low level C13-related peaks. So 
        // it will not stop as early as the BasicTFF, but will iterate more times to pull
        // out the smaller intensity C13-related peaks
        public override IsotopicProfile IterativelyFindMSFeature(Run run, IsotopicProfile theorIso)
        {

            IsotopicProfile foundIso = null;

            //start with high PeakBR and rachet it down, so as to detect more peaks with each pass.  Stop when you find the isotopic profile. 
            for (double d = PeakDetectorPeakBR; d >= PeakBRMin; d = d - PeakBRStep)
            {
                MSPeakDetector.PeakBackgroundRatio = d;
                MSPeakDetector.Execute(run.ResultCollection);

                IsotopicProfile iso = FindMSFeature(run.PeakList, theorIso, ToleranceInPPM, this.NeedMonoIsotopicPeak);

                if (foundIso == null)
                {
                    foundIso = iso;
                }

                if (foundIso != null)
                {
                    combineIsotopicProfiles(foundIso, iso);
                }

                bool isoIsGoodEnough = (foundIso != null && foundIso.Peaklist.Count>=_maxPeaksToInclude );

                if (isoIsGoodEnough)
                {
                    break;
                }

            }

            return foundIso;
        }

        private void combineIsotopicProfiles(IsotopicProfile baseIso, IsotopicProfile addedIso)
        {
            

            foreach (var msPeak in addedIso.Peaklist)
            {
                int indexOfPeakInBaseIos = PeakUtilities.getIndexOfClosestValue(baseIso.Peaklist, msPeak.XValue, 0, baseIso.Peaklist.Count - 1,
                                                     msPeak.Width);

                if (indexOfPeakInBaseIos == -1)
                {
                    baseIso.Peaklist.Add(msPeak);
                }
            }

        }

        #endregion

        #region Private Methods

        #endregion

    }
}
