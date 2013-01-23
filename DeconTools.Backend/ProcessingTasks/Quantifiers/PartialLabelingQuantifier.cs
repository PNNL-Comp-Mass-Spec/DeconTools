using System.Collections.Generic;
using System.Linq;
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
        readonly PeakLeastSquaresFitter _leastSquaresFitter = new PeakLeastSquaresFitter();
        private IterativeTFF _iterativeTff;

        private readonly LabeledIsotopicProfileUtilities _isoCreator = new LabeledIsotopicProfileUtilities();
        private MassTagFitScoreCalculator _fitScoreCalculator = new MassTagFitScoreCalculator();


        private string _elementLabeled;
        private int _lightIsotope;
        private int _heavyIsotope;
        private double _toleranceInPPM = 30;

        //TODO: for testing. delete later 
        public List<double> CurrentFitScores = new List<double>();

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

       

        public IsotopicProfile FindBestLabeledProfile(TargetBase target, List<Peak> massSpectrumPeakList, XYData massSpectrumXYData = null)
        {

            IsotopicProfile bestIso = null;
            double bestFitScore = 1.0;

            Check.Require(target != null, "Target is null. You need a valid target in order to quantify it.");
            Check.Require(target.IsotopicProfile != null, "Target's theoretical isotopic profile is null. You need to create it first.");

            //create theor profiles to iterate over and use in fitting
            _theorLabeledProfiles.Clear();
            for (double labelAmount = MinLabelAmount; labelAmount < MaxLabelAmount; labelAmount = labelAmount + StepAmountForIterator)
            {
                var theorIso = _isoCreator.CreateIsotopicProfileFromEmpiricalFormula(target.EmpiricalFormula,
                                                                                     _elementLabeled, _lightIsotope,
                                                                                     _heavyIsotope, labelAmount,
                                                                                     target.ChargeState);


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
                    //double mzOffset = massSpectrumPeakList[indexOfCorrespondingObservedPeak].XValue - theorIso.Peaklist[indexOfMostAbundantTheorPeak].XValue;

                    double minIntensityForFitting =0.02  ;
                    var theorPeakListForFitter = new List<Peak>(theorIso.Peaklist).Where(p=>p.Height>minIntensityForFitting).ToList();

                    var mzForZeroIntensityPeak = theorIso.getMonoPeak().XValue -
                                                 Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS/theorIso.ChargeState;

                    var zeroIntensityPeak = new Peak(mzForZeroIntensityPeak, float.Epsilon, 0);

                    theorPeakListForFitter.Insert(0, zeroIntensityPeak);

                    fitScore = _leastSquaresFitter.GetFit(theorPeakListForFitter, massSpectrumPeakList, 0, 30);

                    if (double.IsNaN(fitScore) || fitScore > 1) fitScore = 1;
                }

                if (fitScore<bestFitScore)
                {
                    bestFitScore = fitScore;

                   
                    if (massSpectrumXYData==null)
                    {
                        //create isotopic profile from peakList


                        var peakListForCreatedIso = new List<MSPeak>();

                        foreach (var peak in theorIso.Peaklist)
                        {
                            double mzTolerance = _toleranceInPPM * peak.XValue / 1e6;
                            var foundPeaks = PeakUtilities.GetPeaksWithinTolerance(massSpectrumPeakList, peak.XValue,
                                                                                   mzTolerance);
                            MSPeak mspeak;
                            if (foundPeaks.Count==0)
                            {
                                mspeak = new MSPeak(peak.XValue, 0, 0, 0);
                            }
                            else if (foundPeaks.Count==1)
                            {
                                var foundPeak = foundPeaks.First();
                                mspeak = new MSPeak(foundPeak.XValue, foundPeak.Height, foundPeak.Width, 0);

                            }
                            else
                            {
                                var foundPeak = foundPeaks.OrderByDescending(p => p.Height).First();
                                mspeak = new MSPeak(foundPeak.XValue, foundPeak.Height, foundPeak.Width, 0);
                            }

                            peakListForCreatedIso.Add(mspeak);

                        }

                        bestIso = new IsotopicProfile();
                        bestIso.Peaklist = peakListForCreatedIso;
                        bestIso.ChargeState = theorIso.ChargeState;
                        bestIso.Score = fitScore;

                    }
                    else
                    {
                        bestIso = _iterativeTff.IterativelyFindMSFeature(massSpectrumXYData, theorIso);
                        bestIso.Score = fitScore;    
                    }

                    
                }

                CurrentFitScores.Add(fitScore);

                _theorLabeledProfiles.Add(theorIso);
            }

            return bestIso;
        }




    }
}
