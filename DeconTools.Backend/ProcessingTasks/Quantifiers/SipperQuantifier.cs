using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.Quantifiers
{
    public class SipperQuantifier : Task
    {
        private const double LabeldistCalcIntensityThreshold = 0.1;
        private double _chromScanWindowWidth;

        private readonly LabelingDistributionCalculator _labelingDistributionCalculator;
        private readonly ChromatogramCorrelatorBase _chromatogramCorrelatorTask;

        private readonly PartialLabelingQuantifier _partialLabelingQuantifier;



        #region Constructors
        public SipperQuantifier()
        {

            ChromToleranceInPPM = 25;
            NumPointsInSmoother = 5;
            MinimumRelativeIntensityForChromCorr = 0.0001;

            _chromatogramCorrelatorTask = new ChromatogramCorrelator(NumPointsInSmoother, (int)ChromToleranceInPPM, MinimumRelativeIntensityForChromCorr);

            _labelingDistributionCalculator = new LabelingDistributionCalculator();
            _partialLabelingQuantifier = new PartialLabelingQuantifier("C", 12, 13);
            _partialLabelingQuantifier.MaxLabelAmount = 15;
            _partialLabelingQuantifier.StepAmountForIterator = 0.25;
            _partialLabelingQuantifier.NumLeftZeroPads = 1;
            _partialLabelingQuantifier.NumRightZeroPads = 1;
            _partialLabelingQuantifier.IsTheoreticalTrimmedDownToObserved = true;


            IsChromatogramCorrelationPerformed = true;
            MaximumFitScoreForFurtherProcessing = 0.50;
            MinimumRatioAreaForFurtherProcessing = 5;


            ChromatogramRSquaredVals = new List<double>();
            RatioVals = new XYData();

            FitScoreData = new Dictionary<decimal, double>();
        }



        #endregion



        #region Properties
        protected double MaximumFitScoreForFurtherProcessing { get; set; }
        protected double MinimumRatioAreaForFurtherProcessing { get; set; }

        public bool IsChromatogramCorrelationPerformed { get; set; }

        private double _chromToleranceInPPM;
        protected double ChromToleranceInPPM
        {
            get { return _chromToleranceInPPM; }
            set
            {
                _chromToleranceInPPM = value;
                if (_chromatogramCorrelatorTask != null) _chromatogramCorrelatorTask.ChromToleranceInPPM = _chromToleranceInPPM;
            }
        }

        private double _minimumRelativeIntensityForChromCorr;
        public double MinimumRelativeIntensityForChromCorr
        {
            get { return _minimumRelativeIntensityForChromCorr; }
            set
            {
                _minimumRelativeIntensityForChromCorr = value;
                if (_chromatogramCorrelatorTask != null)
                    _chromatogramCorrelatorTask.MinimumRelativeIntensityForChromCorr = _minimumRelativeIntensityForChromCorr;
            }
        }


        private int _numPointsInSmoother;
        public int NumPointsInSmoother
        {
            get { return _numPointsInSmoother; }
            set
            {
                _numPointsInSmoother = value;
                if (_chromatogramCorrelatorTask != null) 
                    _chromatogramCorrelatorTask.NumPointsInSmoother = _numPointsInSmoother;
            }
        }


        public List<double> ChromatogramRSquaredVals { get; set; }

        public Dictionary<decimal, double> FitScoreData { get; set; }

        public IsotopicProfile NormalizedIso { get; set; }




        /// <summary>
        /// Normalized Isotopic profile but intensities adjusted using ChromatogramCorrelationData
        /// </summary>
        public IsotopicProfile NormalizedAdjustedIso { get; set; }

        public XYData RatioVals { get; set; }

        protected double MinimumRelativeIntensityForRatioCalc { get; set; }





        #endregion



        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList.CurrentTargetedResult is SipperLcmsTargetedResult, "Sipper Quantifier only works on Sipper-type result objects");
            Check.Require(resultList.Run.CurrentMassTag != null, this.Name + " failed; CurrentMassTag is empty");
            Check.Require(resultList.Run.CurrentMassTag.IsotopicProfile != null, this.Name + " failed; Theor isotopic profile is empty. Run a TheorFeatureGenerator");

            var result = (SipperLcmsTargetedResult)resultList.CurrentTargetedResult;
            result.AreaUnderDifferenceCurve = -9999;
            result.AreaUnderRatioCurve = -9999;



            RatioVals.Xvalues = new double[] { 1, 2, 3, 4, 5, 6, 7 };
            RatioVals.Yvalues = new double[] { 0, 0, 0, 0, 0, 0, 0 };

            ChromatogramRSquaredVals.Clear();
            FitScoreData.Clear();


            Check.Require(result != null, "No MassTagResult has been generated for CurrentMassTag");

            if (result.IsotopicProfile == null || result.IsotopicProfile.Peaklist == null || result.IsotopicProfile.Peaklist.Count < 2)
            {

                return;
            }


            var theorUnlabelledIso = resultList.Run.CurrentMassTag.IsotopicProfile.CloneIsotopicProfile();
            IsotopicProfileUtilities.NormalizeIsotopicProfile(theorUnlabelledIso);

            //PeakUtilities.TrimIsotopicProfile(unlabeledIso, 0.001);





            int indexMostAbundantTheorPeak = theorUnlabelledIso.GetIndexOfMostIntensePeak();

            NormalizedIso = result.IsotopicProfile.CloneIsotopicProfile();
            IsotopicProfileUtilities.NormalizeIsotopicProfileToSpecificPeak(NormalizedIso, indexMostAbundantTheorPeak);

            //            return;


            if (result.Flags.Count > 0)
            {
                string flagstring = "";

                foreach (var resultFlag in result.Flags)
                {
                    flagstring += resultFlag.Description + ";";
                }

                result.ErrorDescription = flagstring.TrimEnd(';');
                result.FailedResult = true;
                if (flagstring.Contains("PeakToTheLeft"))
                {
                    result.FailureType = Globals.TargetedResultFailureType.DeisotopingProblemDetected;
                }
                else
                {
                    result.FailureType = Globals.TargetedResultFailureType.QuantifierFailure;
                }
            }


            bool resultPassesMinimalCriteria = (result.Score < MaximumFitScoreForFurtherProcessing && result.Flags.Count == 0);

            if (resultPassesMinimalCriteria)
            {
                //------------------------------------- Get chromatogramCorrelation data -------------------------------

                if (IsChromatogramCorrelationPerformed)
                {
                    GetChromatogramCorrelationData(result);
                }

                NormalizedAdjustedIso = NormalizedIso.CloneIsotopicProfile();

                //this is experimental!!  it attempts to correct the problems caused by Orbitraps on isotopic profile intensities
                //UpdateIsoIntensitiesUsingChromCorrData(result.ChromCorrelationData, NormalizedAdjustedIso);


                //----------------- create ratio data -------------------------------------------------------
                var ratioData = NormalizedAdjustedIso.CloneIsotopicProfile();
                for (int i = 0; i < NormalizedIso.Peaklist.Count; i++)
                {
                    if (i < theorUnlabelledIso.Peaklist.Count && theorUnlabelledIso.Peaklist[i].Height > MinimumRelativeIntensityForRatioCalc)
                    {
                        ratioData.Peaklist[i].Height = (NormalizedIso.Peaklist[i].Height / theorUnlabelledIso.Peaklist[i].Height - 1);
                    }
                    else
                    {
                        ratioData.Peaklist[i].Height = 0;
                    }
                }

                //trim off zeros from ratio data
                for (int i = ratioData.Peaklist.Count - 1; i >= 0; i--)
                {
                    if (ratioData.Peaklist[i].Height == 0)
                    {
                        ratioData.Peaklist.RemoveAt(i);
                    }
                    else
                    {
                        break;
                    }
                }

                var xvals = ratioData.Peaklist.Select((p, i) => new { peak = p, index = i }).Select(n => (double)n.index).ToList();
                var yvals = ratioData.Peaklist.Select(p => (double)p.Height).ToList();
                result.AreaUnderRatioCurve = yvals.Sum();

                RatioVals.Xvalues = xvals.ToArray();
                RatioVals.Yvalues = yvals.ToArray();

                //------------- subtract unlabelled profile from normalized profile ----------------------
                var subtractedIsoData = NormalizedAdjustedIso.CloneIsotopicProfile();
                for (int i = 0; i < subtractedIsoData.Peaklist.Count; i++)
                {
                    float intensityTheorPeak = 0;
                    if (i < theorUnlabelledIso.Peaklist.Count)
                    {
                        intensityTheorPeak = theorUnlabelledIso.Peaklist[i].Height;
                    }


                    float subtractedIntensity = subtractedIsoData.Peaklist[i].Height - intensityTheorPeak;
                    if (subtractedIntensity < 0)
                    {
                        subtractedIntensity = 0;
                    }

                    subtractedIsoData.Peaklist[i].Height = subtractedIntensity;
                }




                var peaksForLabeledIsoQuant = new List<Peak>(subtractedIsoData.Peaklist.Where(p => p.Height > 0));

                //foreach (var peak in peaksForLabeledIsoQuant)
                //{
                //    Console.WriteLine(peak.XValue + "\t" + peak.Height);
                //}


                //foreach (var msPeak in result.Target.IsotopicProfile.Peaklist)
                //{
                //    Console.WriteLine(msPeak.XValue + "\t" + msPeak.Height);
                //}


                var isoFromPartialLabelingQuantifier = _partialLabelingQuantifier.FindBestLabeledProfile(result.Target, peaksForLabeledIsoQuant);
                FitScoreData = _partialLabelingQuantifier.FitScoreData;

                result.FitScoreLabeledProfile = isoFromPartialLabelingQuantifier.IsotopicProfile == null ? 1.00d : isoFromPartialLabelingQuantifier.IsotopicProfile.Score;
                result.PercentCarbonsLabelled = isoFromPartialLabelingQuantifier.PercentLabeling;

                int numCarbons = result.Target.GetAtomCountForElement("C");
                result.NumCarbonsLabelled = result.PercentCarbonsLabelled * numCarbons / 100;



#if DEBUG
                StringBuilder sb = new StringBuilder();
                sb.Append(result.Target.ID + "\t" + result.Target.MZ.ToString("0.0000") + "\t" + result.Target.ChargeState +
                          "-----------------------------\n");

                int counter = 0;
                foreach (var fsData in _partialLabelingQuantifier.FitScoreData)
                {
                    sb.Append(fsData.Key + "\t" + fsData.Value.ToString("0.000") + "\n");
                    counter++;
                }

                //Console.WriteLine(sb.ToString());

#endif
                result.AreaUnderDifferenceCurve = subtractedIsoData.Peaklist.Select(p => p.Height).Sum();


                //-------------- calculate Label Distribution ------------------------------------------------
                double[] numLabelVals;
                double[] labelDistributionVals;

                var theorIntensityVals = theorUnlabelledIso.Peaklist.Select(p => (double)p.Height).ToList();

                if (isoFromPartialLabelingQuantifier.IsotopicProfile != null)
                {
                    var normalizedCorrectedIntensityVals = NormalizedAdjustedIso.Peaklist.Select(p => (double)p.Height).ToList();

                    int numRightPads = 3;
                    _labelingDistributionCalculator.CalculateLabelingDistribution(theorIntensityVals, normalizedCorrectedIntensityVals,
                                                                                  LabeldistCalcIntensityThreshold,
                                                                                  LabeldistCalcIntensityThreshold,
                                                                                  out numLabelVals,
                                                                                  out labelDistributionVals, true, true, 0, numRightPads, 0, 0);


                    //negative distribution values are zeroed out. And, then the remaining values are adjusted such that they add up to 1. 
                    result.LabelDistributionVals = AdjustLabelDistributionVals(labelDistributionVals);


                    double distFractionUnlabelled, distFractionLabelled, distAverageLabelsIncorporated;
                    _labelingDistributionCalculator.OutputLabelingInfo(result.LabelDistributionVals, out distFractionUnlabelled,
                                                                       out distFractionLabelled,
                                                                       out distAverageLabelsIncorporated);

                    result.PercentPeptideLabelled = distFractionLabelled * 100;
                }
                else
                {
                    result.PercentCarbonsLabelled = 0;
                }




                //-------------- make calculations using inputs from chrom correlation data -------------------

                HighQualitySubtractedProfile = GetIsoDataPassingChromCorrelation(result.ChromCorrelationData, subtractedIsoData);

                IsotopicProfile highQualityRatioProfileData = GetIsoDataPassingChromCorrelation(result.ChromCorrelationData, ratioData);

                if (highQualityRatioProfileData.Peaklist != null && highQualityRatioProfileData.Peaklist.Count > 0)
                {
                    result.AreaUnderDifferenceCurve = HighQualitySubtractedProfile.Peaklist.Select(p => p.Height).Sum();
                }
                else
                {
                    result.AreaUnderDifferenceCurve = 0;
                }

                if (HighQualitySubtractedProfile.Peaklist != null && HighQualitySubtractedProfile.Peaklist.Count > 0)
                {
                    result.AreaUnderRatioCurveRevised = highQualityRatioProfileData.Peaklist.Select(p => p.Height).Sum();
                }
                else
                {
                    result.AreaUnderRatioCurveRevised = 0;
                }

                result.NumHighQualityProfilePeaks = highQualityRatioProfileData.Peaklist != null
                                                      ? highQualityRatioProfileData.Peaklist.Count
                                                      : 0;

                //double numCarbonsLabelled = GetNumCarbonsLabelledUsingAverageMassDifferences(theorUnlabelledIso,HighQualitySubtractedProfile);
                //result.NumCarbonsLabelled = numCarbonsLabelled;
                //result.NumCarbonsLabelled = distAverageLabelsIncorporated;

                //numCarbons = result.Target.GetAtomCountForElement("C");
                //result.PercentCarbonsLabelled = (result.NumCarbonsLabelled / numCarbons) * 100;

                //if (HighQualitySubtractedProfile.Peaklist != null && HighQualitySubtractedProfile.Peaklist.Count > 0)
                //{
                //    result.PercentPeptideLabelled = HighQualitySubtractedProfile.Peaklist.Max(p => p.Height) * 100;
                //}
                //else
                //{
                //    result.PercentPeptideLabelled = 0;
                //}

                //Console.WriteLine();
                //Console.WriteLine(result);
                //foreach (var val in ChromatogramRSquaredVals)
                //{
                //    Console.WriteLine(val);
                //}
            }
            else
            {
                result.FailedResult = true;

                if (result.FailureType == Globals.TargetedResultFailureType.None)
                {
                    result.FailureType = Globals.TargetedResultFailureType.MinimalCriteriaNotMet;
                }
            }
        }

        private void GetChromatogramCorrelationData(SipperLcmsTargetedResult result)
        {
            _chromScanWindowWidth = result.ChromPeakSelected.Width * 2;

            int startScan = result.ScanSet.PrimaryScanNumber - (int)Math.Round(_chromScanWindowWidth / 2, 0);
            int stopScan = result.ScanSet.PrimaryScanNumber + (int)Math.Round(_chromScanWindowWidth / 2, 0);

            result.ChromCorrelationData = _chromatogramCorrelatorTask.CorrelatePeaksWithinIsotopicProfile(result.Run, NormalizedIso, startScan, stopScan);

            ChromatogramRSquaredVals.AddRange(result.ChromCorrelationData.CorrelationDataItems.Select(p => p.CorrelationRSquaredVal.GetValueOrDefault(-1)).ToList());

            //trim off zeros
            for (int i = ChromatogramRSquaredVals.Count - 1; i >= 0; i--)
            {
                if (ChromatogramRSquaredVals[i] <= 0)
                {
                    ChromatogramRSquaredVals.RemoveAt(i);
                }
                else
                {
                    break;
                }
            }

            result.ChromCorrelationMin = ChromatogramRSquaredVals.Min();

            if (ChromatogramRSquaredVals.Count > 1)
            {
                //get the best rsquared value other than the base peak's rsquared value (which is always 1)
                result.ChromCorrelationMax =
                    (from n in ChromatogramRSquaredVals orderby n descending select n).ToList().ElementAt(1);

                result.ChromCorrelationMedian = MathUtils.GetMedian(ChromatogramRSquaredVals);
                result.ChromCorrelationAverage = ChromatogramRSquaredVals.Average();

                if (ChromatogramRSquaredVals.Count > 2)
                {
                    result.ChromCorrelationStdev = MathUtils.GetStDev(ChromatogramRSquaredVals);
                }
                else
                {
                    result.ChromCorrelationStdev = -1;
                }
            }
            else
            {
                result.ChromCorrelationMax = 1;
            }
        }

        private double calculateNumCarbonsFromSubtractedProfile(IsotopicProfile subtractedIsoData)
        {
            var intensityVals = subtractedIsoData.Peaklist.Select(p => p.Height).ToList();

            var sumIntensities = intensityVals.Sum();

            double sumDotProducts = 0;
            for (int peakNum = 0; peakNum < intensityVals.Count; peakNum++)
            {
                var dotProduct = intensityVals[peakNum] * peakNum;
                sumDotProducts += dotProduct;
            }

            return sumDotProducts / sumIntensities;
        }

        private double GetNumCarbonsLabelledUsingAverageMassDifferences(IsotopicProfile theorUnlabelledIso, IsotopicProfile highQualitySubtractedProfile)
        {
            double averageMassTheor = GetAverageMassIso(theorUnlabelledIso);

            double averageMassLabelled = GetAverageMassIso(HighQualitySubtractedProfile);

            if (double.IsNaN(averageMassLabelled))
            {
                return 0;
            }

            return averageMassLabelled - averageMassTheor;
        }


        private List<double> AdjustLabelDistributionVals(double[] labelDistributionVals)
        {
            if (labelDistributionVals == null || labelDistributionVals.Length == 0) return new List<double>();

            //first, will set the negative values to zero. (cannot have a negative label distribution)
            for (int i = 0; i < labelDistributionVals.Length; i++)
            {
                if (labelDistributionVals[i] < 0)
                {
                    labelDistributionVals[i] = 0;
                }

            }


            //because values were zeroed, the distribution now adds up to greater than 1. We need to adjust things so the distribution adds up to 1

            double sumVals = labelDistributionVals.Sum();

            //re-normalize.  
            for (int i = 0; i < labelDistributionVals.Length; i++)
            {
                labelDistributionVals[i] = labelDistributionVals[i] / sumVals;
            }

            return labelDistributionVals.ToList();


        }

        public IsotopicProfile HighQualitySubtractedProfile { get; set; }

        private void UpdateIsoIntensitiesUsingChromCorrData(ChromCorrelationData chromCorrelationData, IsotopicProfile iso)
        {
            int totalChromCorrDataItems = chromCorrelationData.CorrelationDataItems.Count;

            double heightMaxPeak = iso.getMostIntensePeak().Height;

            for (int i = 0; i < iso.Peaklist.Count; i++)
            {
                if (i < totalChromCorrDataItems)
                {

                    var correctedRelIntensity = chromCorrelationData.CorrelationDataItems[i].CorrelationSlope;

                    if (correctedRelIntensity.HasValue)
                    {
                        iso.Peaklist[i].Height = (float)(heightMaxPeak * correctedRelIntensity);
                    }



                }

            }


        }


        public float FractionLabelled { get; set; }

        public double AmountC13Labelling { get; set; }

        private double GetAverageMassIso(IsotopicProfile isotopicProfile)
        {
            if (isotopicProfile.Peaklist == null || isotopicProfile.Peaklist.Count == 0)
            {
                return 0;
            }

            double sumIntensities = isotopicProfile.Peaklist.Sum(p => p.Height);

            double averageMZ = 0;

            for (int i = 0; i < isotopicProfile.Peaklist.Count; i++)
            {
                averageMZ += isotopicProfile.Peaklist[i].XValue * isotopicProfile.Peaklist[i].Height / sumIntensities;
            }


            double averageMass = (averageMZ - Globals.PROTON_MASS) * isotopicProfile.ChargeState;


            return averageMass;

        }

        private IsotopicProfile GetIsoDataPassingChromCorrelation(ChromCorrelationData chromCorrelationData, IsotopicProfile iso)
        {

            IsotopicProfile returnedIso = iso.CloneIsotopicProfile();

            returnedIso.Peaklist.Clear();


            for (int i = 0; i < iso.Peaklist.Count; i++)
            {
                if (i < chromCorrelationData.CorrelationDataItems.Count)
                {
                    if (chromCorrelationData.CorrelationDataItems[i].CorrelationRSquaredVal > MinimumRSquaredValForQuant)
                    {
                        returnedIso.Peaklist.Add(iso.Peaklist[i]);

                    }
                    else
                    {
                        break;
                    }
                }

            }

            return returnedIso;


        }

        protected double MinimumRSquaredValForQuant { get; set; }

    }
}
