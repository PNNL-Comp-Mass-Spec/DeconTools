using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.LabeledIsotopicDistUtilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.Quantifiers
{
    public class PartialLabelingQuantifier
    {

        private List<IsotopicProfile> _theorLabeledProfiles;

        private IterativeTFF _iterativeTff;

        private readonly LabeledIsotopicProfileUtilities _isoCreator = new LabeledIsotopicProfileUtilities();
        private MassTagFitScoreCalculator _fitScoreCalculator = new MassTagFitScoreCalculator();


        private string _elementLabeled;
        private int _lightIsotope;
        private int _heavyIsotope;

        public PartialLabelingQuantifier(string element, int lightIsotope, int heavyIsotope)
        {
            MinLabelAmount = 0;
            MaxLabelAmount = 100;
            StepAmountForIterator = 1;

            _elementLabeled = element;
            _lightIsotope = lightIsotope;
            _heavyIsotope = heavyIsotope;

            _theorLabeledProfiles = new List<IsotopicProfile>();

            IterativeTFFParameters tffParameters= new IterativeTFFParameters();
            tffParameters.ToleranceInPPM = 30;
            _iterativeTff = new IterativeTFF(tffParameters);

        }


        public double MinLabelAmount { get; set; }
        public double MaxLabelAmount { get; set; }
        public double StepAmountForIterator { get; set; }

        public string ElementLabeled { get; set; }

        public int LightIsotope { get; set; }

        public List<double> CurrentFitScores { get; set; } 

        public IsotopicProfile FindBestLabeledProfile(TargetBase target, XYData massSpectrumXYData, List<Peak> massSpectrumPeakList)
        {

            IsotopicProfile bestIso = null;
            double bestFitScore = 1.0;

            Check.Require(target != null, "Target is null. You need a valid target in order to quantify it.");
            Check.Require(target.IsotopicProfile != null, "Target's theoretical isotopic profile is null. You need to create it first.");

            //create theor profiles to iterate over and use in fitting
            _theorLabeledProfiles.Clear();
            for (double labelAmount = MinLabelAmount; labelAmount < MaxLabelAmount; labelAmount = labelAmount + StepAmountForIterator)
            {
                var theorIso = _isoCreator.CreateIsotopicProfileFromEmpiricalFormula(target.EmpiricalFormula, _elementLabeled, _lightIsotope, _heavyIsotope, labelAmount);


                int indexOfMostAbundantTheorPeak = theorIso.GetIndexOfMostIntensePeak();
                int indexOfCorrespondingObservedPeak = PeakUtilities.getIndexOfClosestValue(massSpectrumPeakList,
                    theorIso.getMostIntensePeak().XValue, 0, massSpectrumPeakList.Count - 1, 0.1);

                double fitScore;
                if (indexOfCorrespondingObservedPeak < 0)      // most abundant peak isn't present in the actual theoretical profile... problem!
                {
                    fitScore = 1;
                }
                else
                {
                    double mzOffset = massSpectrumPeakList[indexOfCorrespondingObservedPeak].XValue - theorIso.Peaklist[indexOfMostAbundantTheorPeak].XValue;

                    XYData theorXYData = theorIso.GetTheoreticalIsotopicProfileXYData(massSpectrumPeakList[indexOfCorrespondingObservedPeak].Width);

                    theorXYData.OffSetXValues(mzOffset);     //May want to avoid this offset if the masses have been aligned using LCMS Warp

                    //theorXYData.Display();

                    AreaFitter areafitter = new AreaFitter();
                    fitScore = areafitter.GetFit(theorXYData, massSpectrumXYData, 0.1);

                    if (double.IsNaN(fitScore) || fitScore > 1) fitScore = 1;
                    

                }

                if (fitScore<bestFitScore)
                {
                    bestFitScore = fitScore;
                    bestIso = _iterativeTff.IterativelyFindMSFeature(massSpectrumXYData, theorIso);
                    bestIso.Score = fitScore;
                }

                CurrentFitScores.Add(fitScore);

                _theorLabeledProfiles.Add(theorIso);
            }




            return bestIso;
        }




    }
}
