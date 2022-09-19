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
        private readonly List<IsotopicProfile> _theorLabeledProfiles;
        readonly PeakLeastSquaresFitter _leastSquaresFitter = new PeakLeastSquaresFitter();
        private readonly IterativeTFF _iterativeTff;

        private readonly LabeledIsotopicProfileUtilities _isoCreator = new LabeledIsotopicProfileUtilities();
        private IsotopicProfileFitScoreCalculator _fitScoreCalculator = new IsotopicProfileFitScoreCalculator();

        private readonly string _elementLabeled;
        private readonly int _lightIsotope;
        private readonly int _heavyIsotope;
        private readonly double _toleranceInPPM = 30;

        //TODO: for testing. delete later
        public Dictionary<decimal, double> FitScoreData = new Dictionary<decimal, double>();

        public PartialLabelingQuantifier(string element, int lightIsotope, int heavyIsotope, int numLeftZeroPads = 1, int numRightZeroPads = 1, bool isTheorProfileTrimmed = false)
        {
            MinLabelAmount = 0;
            MaxLabelAmount = 100;
            StepAmountForIterator = 1;

            _elementLabeled = element;
            _lightIsotope = lightIsotope;
            _heavyIsotope = heavyIsotope;

            _theorLabeledProfiles = new List<IsotopicProfile>();

            var tffParameters = new IterativeTFFParameters {
                ToleranceInPPM = 30
            };

            _iterativeTff = new IterativeTFF(tffParameters);

            NumLeftZeroPads = numLeftZeroPads;
            NumRightZeroPads = numRightZeroPads;

            IsTheoreticalTrimmedDownToObserved = isTheorProfileTrimmed;
        }

        public double MinLabelAmount { get; set; }
        public double MaxLabelAmount { get; set; }
        public double StepAmountForIterator { get; set; }

        public string ElementLabeled { get; set; }

        public int LightIsotope { get; set; }

        /// <summary>
        /// Default = false.  If true, the theoretical is trimmed such that it has only +/- 1 peak more than the observed.
        /// This helps with low intensity obs data, which might have several '0' peaks on the left and
        /// right edges of the isotopic profile, resulting in a poor fit score
        /// </summary>
        public bool IsTheoreticalTrimmedDownToObserved { get; set; }

        public int NumLeftZeroPads { get; set; }

        public int NumRightZeroPads { get; set; }

        public IsotopicProfileComponent FindBestLabeledProfile(TargetBase target, List<Peak> massSpectrumPeakList, XYData massSpectrumXYData = null)
        {
            IsotopicProfile bestIso = null;
            var bestFitScore = 1.0;
            double bestLabelAmount = -1;

            Check.Require(target != null, "Target is null. You need a valid target in order to quantify it.");
            Check.Require(target.IsotopicProfile != null, "Target's theoretical isotopic profile is null. You need to create it first.");

            //create theor profiles to iterate over and use in fitting
            _theorLabeledProfiles.Clear();
            FitScoreData.Clear();

            for (var labelAmount = MinLabelAmount; labelAmount < MaxLabelAmount; labelAmount += StepAmountForIterator)
            {
                var theorIso = _isoCreator.CreateIsotopicProfileFromEmpiricalFormula(target.EmpiricalFormula,
                                                                                     _elementLabeled, _lightIsotope,
                                                                                     _heavyIsotope, labelAmount,
                                                                                     target.ChargeState);

                var indexOfMostAbundantTheorPeak = theorIso.GetIndexOfMostIntensePeak();
                var indexOfCorrespondingObservedPeak = PeakUtilities.getIndexOfClosestValue(massSpectrumPeakList,
                    theorIso.getMostIntensePeak().XValue, 0, massSpectrumPeakList.Count - 1, 0.1);

                double fitScore;

                var obsPeakListForFitter = new List<Peak>();
                if (indexOfCorrespondingObservedPeak < 0)      // most abundant peak isn't present in the actual theoretical profile... problem!
                {
                    fitScore = 1;
                }
                else
                {
                    //double mzOffset = massSpectrumPeakList[indexOfCorrespondingObservedPeak].XValue - theorIso.Peaklist[indexOfMostAbundantTheorPeak].XValue;

                    var minIntensityForFitting = 0.02;
                    var theorPeakListForFitter = new List<Peak>(theorIso.Peaklist).Where(p => p.Height > minIntensityForFitting).ToList();

                    obsPeakListForFitter = FilterPeaksBasedOnBasePeakList(theorPeakListForFitter, massSpectrumPeakList);

                    AddLeftZeroPads(obsPeakListForFitter, NumLeftZeroPads, theorIso.ChargeState);
                    AddRightZeroPads(obsPeakListForFitter, NumRightZeroPads, theorIso.ChargeState);

                    if (IsTheoreticalTrimmedDownToObserved)
                    {
                        theorPeakListForFitter = FilterPeaksBasedOnBasePeakList(obsPeakListForFitter, theorPeakListForFitter);
                    }

                    //foreach (var peak in obsPeakListForFitter)
                    //{
                    //    Console.WriteLine(peak.XValue + "\t" + peak.Height);
                    //}

                    //foreach (var peak in theorPeakListForFitter)
                    //{
                    //    Console.WriteLine(peak.XValue + "\t" + peak.Height);
                    //}

                    const int numPeaksToTheLeftForScoring = 0;
                    fitScore = _leastSquaresFitter.GetFit(theorPeakListForFitter, obsPeakListForFitter, 0, 30, numPeaksToTheLeftForScoring, out var ionCountUsed);

                    //if (double.IsNaN(fitScore) || fitScore > 1) fitScore = 1;
                }

                if (fitScore < bestFitScore)
                {
                    bestFitScore = fitScore;

                    if (massSpectrumXYData == null)
                    {
                        bestIso = new IsotopicProfile {
                            Peaklist = new List<MSPeak>()
                        };

                        foreach (var peak in obsPeakListForFitter)
                        {
                            bestIso.Peaklist.Add(new MSPeak(peak.XValue,peak.Height,peak.Width,0));
                        }

                        bestIso.ChargeState = theorIso.ChargeState;
                    }
                    else
                    {
                        bestIso = _iterativeTff.IterativelyFindMSFeature(massSpectrumXYData, theorIso);
                    }

                    if (bestIso != null) bestIso.Score = fitScore;
                    bestLabelAmount = labelAmount;
                }

                FitScoreData.Add((decimal)labelAmount, fitScore);

                _theorLabeledProfiles.Add(theorIso);
            }

            var isoComponent = new IsotopicProfileComponent(bestIso, 0, bestLabelAmount);
            return isoComponent;
        }

        private List<Peak> FilterPeaksBasedOnBasePeakList(List<Peak> basePeaklist, List<Peak> inputPeakList)
        {
            var trimmedPeakList = new List<Peak>();

            foreach (var peak in basePeaklist)
            {
                var mzTolerance = _toleranceInPPM * peak.XValue / 1e6;
                var foundPeaks = PeakUtilities.GetPeaksWithinTolerance(inputPeakList, peak.XValue,
                                                                       mzTolerance);

                if (foundPeaks.Count == 1)
                {
                    trimmedPeakList.Add(foundPeaks.First());
                }
                else if (foundPeaks.Count > 1)
                {
                    trimmedPeakList.Add(foundPeaks.OrderByDescending(p => p.Height).First());
                }
            }

            //if we can't find any observed peaks, we won't trim anything. Just return the original list
            if (!trimmedPeakList.Any())
            {
                return basePeaklist;
            }
            return trimmedPeakList;
        }

        private void AddRightZeroPads(ICollection<Peak> theorPeakListForFitter, int numRightZeroPads, int chargeState)
        {
            var mzInitial = theorPeakListForFitter.Last().XValue;

            for (var i = 0; i < numRightZeroPads; i++)
            {
                var mzForAddedPeak = mzInitial + (i + 1) * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / chargeState;
                theorPeakListForFitter.Add(new Peak(mzForAddedPeak, 0, 0));
            }
        }

        private void AddLeftZeroPads(IList<Peak> theorPeakListForFitter, int numLeftZeroPads, int chargeState)
        {
            var mzInitial = theorPeakListForFitter.First().XValue;

            for (var i = 0; i < numLeftZeroPads; i++)
            {
                var mzForAddedPeak = mzInitial - (i + 1) * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / chargeState;
                theorPeakListForFitter.Insert(0, new Peak(mzForAddedPeak, 0, 0));
            }
        }

        private List<Peak> ApplySmartTrimming(List<Peak> theorPeakList, List<Peak> massSpectrumPeakList)
        {
            var trimmedPeakList = new List<Peak>();

            foreach (var peak in theorPeakList)
            {
                var mzTolerance = _toleranceInPPM * peak.XValue / 1e6;
                var foundPeaks = PeakUtilities.GetPeaksWithinTolerance(massSpectrumPeakList, peak.XValue,
                                                                       mzTolerance);
                if (foundPeaks.Any())
                {
                    trimmedPeakList.Add(peak);
                }
            }

            //if we can't find any observed peaks, we won't trim anything. Just return the original list
            if (!trimmedPeakList.Any())
            {
                return theorPeakList;
            }
            return trimmedPeakList;
        }
    }
}
