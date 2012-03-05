using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public class IterativeTFF : TFFBase
    {


        
        private bool _peakDetectorIsDataThresholded;
        private double _peakDetectorSigNoiseRatioThreshold;
        
        private Globals.PeakFitType _peakDetectorPeakFitType;

        #region Constructors
        public IterativeTFF(IterativeTFFParameters parameters)
        {
            PeakBRStep = parameters.PeakBRStep;
            _peakDetectorIsDataThresholded = parameters.PeakDetectorIsDataThresholded;
            _peakDetectorSigNoiseRatioThreshold = parameters.PeakDetectorSigNoiseRatioThreshold;
            PeakDetectorPeakBR = parameters.PeakDetectorPeakBR;
            _peakDetectorPeakFitType = parameters.PeakDetectorPeakFitType;
            PeakBRMin = 0.5;

            this.NeedMonoIsotopicPeak = parameters.RequiresMonoIsotopicPeak;
            this.ToleranceInPPM = parameters.ToleranceInPPM;

            this.NumPeaksUsedInAbundance = parameters.NumPeaksUsedInAbundance;
            this.IsotopicProfileType = parameters.IsotopicProfileType;


            this.MSPeakDetector = new DeconToolsPeakDetector(PeakDetectorPeakBR, _peakDetectorSigNoiseRatioThreshold,
                 _peakDetectorPeakFitType, _peakDetectorIsDataThresholded);
        }


        #endregion

        #region Properties
        public double PeakDetectorPeakBR { get; set; }
        public DeconToolsPeakDetector MSPeakDetector { get; set; }
        public double PeakBRMin { get; set; }
        public double PeakBRStep { get; set; }

        #endregion

        #region Public Methods
        public override void Execute(Core.ResultCollection resultColl)
        {

            Check.Require(resultColl != null && resultColl.Run != null, String.Format("{0} failed. Run is empty.", this.Name));
            Check.Require(resultColl.Run.CurrentMassTag != null, String.Format("{0} failed. CurrentMassTag hasn't been defined.", this.Name));

            TargetedResultBase result = resultColl.CurrentTargetedResult;

            IsotopicProfile theorFeature = CreateTargetIso(resultColl.Run);
            resultColl.IsosResultBin.Clear();

            IsotopicProfile iso = IterativelyFindMSFeature(resultColl.Run, theorFeature);


            AddFeatureToResult(result, iso);


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




        public virtual IsotopicProfile IterativelyFindMSFeature(Run run, IsotopicProfile theorIso)
        {


            IsotopicProfile iso = null;

            //start with high PeakBR and rachet it down, so as to detect more peaks with each pass.  Stop when you find the isotopic profile. 
            for (double d = PeakDetectorPeakBR; d >= PeakBRMin; d = d - PeakBRStep)
            {


                this.MSPeakDetector.PeakBackgroundRatio = d;
                this.MSPeakDetector.Execute(run.ResultCollection);

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



        private void AddFeatureToResult(TargetedResultBase result, IsotopicProfile iso)
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

        #endregion


    }
}
