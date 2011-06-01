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
    public class IterativeTFF : TFFBase
    {
        private DeconToolsPeakDetector msPeakDetector;
        private double peakBRMin;
        private double peakBRStep;
        private bool peakDetectorIsDataThresholded;
        private double peakDetectorSigNoiseRatioThreshold;
        private double peakDetectorPeakBR;
        private Globals.PeakFitType peakDetectorPeakFitType;

        #region Constructors
        public IterativeTFF(IterativeTFFParameters parameters)
        {
            peakBRStep = parameters.PeakBRStep;
            peakDetectorIsDataThresholded = parameters.PeakDetectorIsDataThresholded;
            peakDetectorSigNoiseRatioThreshold = parameters.PeakDetectorSigNoiseRatioThreshold;
            peakDetectorPeakBR = parameters.PeakDetectorPeakBR;
            peakDetectorPeakFitType = parameters.PeakDetectorPeakFitType;
            peakBRMin = 0.5;

            this.NeedMonoIsotopicPeak = parameters.RequiresMonoIsotopicPeak;
            this.ToleranceInPPM = parameters.ToleranceInPPM;

            this.NumPeaksUsedInAbundance = parameters.NumPeaksUsedInAbundance;
            this.IsotopicProfileType = parameters.IsotopicProfileType;


            this.msPeakDetector = new DeconToolsPeakDetector(peakDetectorPeakBR, peakDetectorSigNoiseRatioThreshold,
                 peakDetectorPeakFitType, peakDetectorIsDataThresholded);
        }


        #endregion

        #region Properties

        #endregion

        #region Public Methods
        public override void Execute(Core.ResultCollection resultColl)
        {

            Check.Require(resultColl != null && resultColl.Run != null, String.Format("{0} failed. Run is empty.", this.Name));
            Check.Require(resultColl.Run.CurrentMassTag != null, String.Format("{0} failed. CurrentMassTag hasn't been defined.", this.Name));

            MassTagResultBase result = resultColl.GetMassTagResult(resultColl.Run.CurrentMassTag);

            IsotopicProfile theorFeature = getTheorProfile(resultColl.Run.CurrentMassTag, this.IsotopicProfileType);
            resultColl.IsosResultBin.Clear();

            IsotopicProfile iso = iterativelyFindMSFeature(resultColl.Run, theorFeature);


            addFeatureToResult(result, iso);


            bool isoIsGood = (iso != null && iso.Peaklist != null && iso.Peaklist.Count > 0);
            if (isoIsGood)
            {
                iso.IntensityAggregate = sumPeaks(iso, this.NumPeaksUsedInAbundance, 0);
            }

            resultColl.IsosResultBin.Add(result);


        }

        private double sumPeaks(IsotopicProfile profile, int numPeaksUsedInAbundance, int defaultVal)
        {
            if (profile.Peaklist == null || profile.Peaklist.Count < numPeaksUsedInAbundance) return defaultVal;
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




        public IsotopicProfile iterativelyFindMSFeature(Run run, IsotopicProfile theorIso)
        {
            

            IsotopicProfile iso = null;

            //start with high PeakBR and rachet it down, so as to detect more peaks with each pass.  Stop when you find the isotopic profile. 
            for (double d = peakDetectorPeakBR; d >= peakBRMin; d = d - this.peakBRStep)
            {


                this.msPeakDetector.PeakBackgroundRatio = d;
                this.msPeakDetector.Execute(run.ResultCollection);

                //Console.WriteLine("PeakBR= " + d + "; NumPeaks= " + run.PeakList.Count);


                iso = FindMSFeature(run.PeakList, theorIso, this.ToleranceInPPM, this.NeedMonoIsotopicPeak);

                bool isoIsGoodEnough = (iso != null && iso.Peaklist.Count > 1);

                if (isoIsGoodEnough)
                {
                    break;
                }

            }

            return iso;

        }



        private void addFeatureToResult(MassTagResultBase result, IsotopicProfile iso)
        {

            //TODO:   the IsotopicProfileType is wrong...

            switch (this.IsotopicProfileType)
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
        }

        private IsotopicProfile getTheorProfile(MassTag massTag, IsotopicProfileType isotopicProfileType)
        {

            switch (isotopicProfileType)
            {
                case IsotopicProfileType.UNLABELLED:
                    return massTag.IsotopicProfile;

                case IsotopicProfileType.LABELLED:
                    return massTag.IsotopicProfileLabelled;

                default:
                    return massTag.IsotopicProfile;

            }
        }

        #endregion

        #region Private Methods

        #endregion



    }
}
