using System;
using System.Collections.Generic;
using System.Linq;
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

            MaxPeaksToInclude = 30;
            MinRelIntensityForPeakInclusion = 0.02;


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

        /// <summary>
        /// When finding peaks in the isotopic profile, this number represents the maximum number of peaks added
        /// </summary>
        public int MaxPeaksToInclude { get; set; }

        /// <summary>
        /// When finding peaks in the isotopic profile, this number represents the minimum relative intensity of peaks to be added. Ranges between 0 and 1
        /// </summary>
        public double MinRelIntensityForPeakInclusion { get; set; }


        #endregion

        #region Public Methods
        public override void Execute(Core.ResultCollection resultList)
        {

            Check.Require(resultList != null && resultList.Run != null, String.Format("{0} failed. Run is empty.", this.Name));
            Check.Require(resultList.Run.CurrentMassTag != null, String.Format("{0} failed. CurrentMassTag hasn't been defined.", this.Name));

            TargetedResultBase result = resultList.GetTargetedResult(resultList.Run.CurrentMassTag);

            IsotopicProfile theorFeature = CreateTargetIso(resultList.Run);
            resultList.IsosResultBin.Clear();

            IsotopicProfile iso = IterativelyFindMSFeature(resultList.Run, theorFeature);


            AddFeatureToResult(result, iso);


            bool isoIsGood = (iso != null && iso.Peaklist != null && iso.Peaklist.Count > 0);
            if (isoIsGood)
            {
                iso.IntensityAggregate = sumPeaks(iso, this.NumPeaksUsedInAbundance, 0);
            }

            resultList.IsosResultBin.Add(result);


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

                iso = FindMSFeature(run.PeakList, theorIso, this.ToleranceInPPM, this.NeedMonoIsotopicPeak);

                bool isoIsGoodEnough;

                if (iso==null)
                {
                    isoIsGoodEnough = false;
                }
                else if (iso.Peaklist.Count<2)
                {
                    isoIsGoodEnough = false;
                }
                else
                {
                    double maxIntensity = iso.getMostIntensePeak().Height;
                    double minIntensityPeak = iso.Peaklist.Min(p => p.Height);

                    if (minIntensityPeak/maxIntensity< MinRelIntensityForPeakInclusion)
                    {
                        isoIsGoodEnough = true;
                    }
                    else
                    {
                        isoIsGoodEnough = false;
                    }

                }

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
