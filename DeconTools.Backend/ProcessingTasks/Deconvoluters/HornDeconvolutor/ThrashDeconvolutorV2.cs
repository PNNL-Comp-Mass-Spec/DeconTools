using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Algorithms.ChargeStateDetermination.PattersonAlgorithm;
using DeconTools.Backend.Core;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.Utilities;
using DeconTools.Backend.ProcessingTasks.ChargeStateDeciders;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IqLogger;
using System.Diagnostics;
using System.IO;

namespace DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor
{
    public class ThrashDeconvolutorV2 : Deconvolutor
    {
        #region Paul Addition
        static bool doPaulMethod = false;
        #endregion

        private Run _run;

        readonly IsotopicDistributionCalculator _isotopicDistCalculator = IsotopicDistributionCalculator.Instance;
        private readonly AreaFitter _areafitter = new AreaFitter();
        private readonly BasicTFF _targetedFeatureFinder = new BasicTFF();
        private readonly Dictionary<int, IsotopicProfile> _averagineProfileLookupTable = new Dictionary<int, IsotopicProfile>();
        private const int NumPointsPerTheorPeak = 20;

        #region Constructors
        public ThrashDeconvolutorV2(ThrashParameters parameters)
        {
            Parameters = parameters;
        }

        public ThrashDeconvolutorV2() : this(new ThrashParameters()) { }


        #endregion

        #region Properties

        public ThrashParameters Parameters { get; set; }

        /// <summary>
        /// Set to 'True' if you want to use autocorrelation to determine charge state. Default is 'false', in which case the new
        /// algorithm for figuring out charge state is used. In that algorithm, Thrash fitting is done on multiple charge state
        /// candidates and then the best one is selected.
        /// </summary>
        public bool UseAutocorrelationChargeDetermination { get; set; }


        #endregion

        #region Public Methods


        public override void Deconvolute(ResultCollection resultList)
        {
            Check.Require(resultList.Run != null, "Cannot deconvolute. Run is null");
            if (resultList.Run == null)
                return;

            Check.Require(resultList.Run.XYData != null, "Cannot deconvolute. No mass spec XY data found.");
            Check.Require(resultList.Run.PeakList != null, "Cannot deconvolute. Mass spec peak list is empty.");

            if (resultList.Run.PeakList == null)
                return;


            _run = resultList.Run;

            if (resultList.Run.PeakList.Count < 2)
            {
                return;
            }

            var backgroundIntensity = resultList.Run.CurrentBackgroundIntensity;

            //Key step
            var msFeatures = PerformThrash(resultList.Run.XYData, resultList.Run.PeakList,
                                           backgroundIntensity, Parameters.MinMSFeatureToBackgroundRatio);

            //Create DeconTools-type results. Get the representative abundance.
            foreach (var isotopicProfile in msFeatures)
            {
                var result = resultList.CreateIsosResult();
                result.IsotopicProfile = isotopicProfile;

                var theorIso = GetTheoreticalProfile(result.IsotopicProfile.MonoIsotopicMass);
                result.IntensityAggregate = GetReportedAbundance(isotopicProfile, theorIso, Parameters.NumPeaksUsedInAbundance);

                if (isotopicProfile.Score <= Parameters.MaxFit)
                {
                    AddDeconResult(resultList, result);
                }

            }

        }

        /// <summary>
        /// The main Thrash algorithm.
        /// </summary>
        /// <param name="originalXYData">Mass spec XY data</param>
        /// <param name="mspeakList">Mass spec peak data</param>
        /// <param name="backgroundIntensity"></param>
        /// <param name="minPeptideIntensity"></param>
        /// <param name="minMSFeatureToBackgroundRatio"></param>
        /// <returns>List of isotopic profiles</returns>
        public List<IsotopicProfile> PerformThrash(XYData originalXYData,
            List<Peak> mspeakList,
            double backgroundIntensity = 0,
            double minPeptideIntensity = 0, double
            minMSFeatureToBackgroundRatio = 3)
        {

            var isotopicProfiles = new List<IsotopicProfile>();
            #region Paul Addition

            #endregion

            if (Parameters.AreAllTheoreticalProfilesCachedBeforeStarting)
            {
                CreateAllTheoreticalProfilesForMassRange();
            }


            var minMSFeatureIntensity = backgroundIntensity * minMSFeatureToBackgroundRatio;


            var xyData = new XYData
            {
                Xvalues = originalXYData.Xvalues,
                Yvalues = originalXYData.Yvalues
            };

            var sortedPeaklist = new List<Peak>(mspeakList).OrderByDescending(p => p.Height).ToList();
            var peaksAlreadyProcessed = new HashSet<Peak>();


            var stringBuilder = new StringBuilder();

            var listOfMonoMZs = new SortedDictionary<int, double>();
            var currentUniqueMSFeatureIDNum = 0;


            var peakCounter = -1;
            foreach (var msPeak in sortedPeaklist)
            {

                //if (msPeak.XValue > 579.53 && msPeak.XValue < 579.54)
                //{
                //    int x = 90;
                //}
                var indexOfCurrentPeak = mspeakList.IndexOf(msPeak);

                if (peaksAlreadyProcessed.Contains(msPeak))
                {
                    continue;

                }

                var peakIsBelowIntensityThreshold = (msPeak.Height < minMSFeatureIntensity);
                if (peakIsBelowIntensityThreshold) break;



                peakCounter++;

                if (peakCounter == 465)
                {
                    // Console.WriteLine(peakCounter);
                }


                //get potential charge states
                var ppmTolerance = (msPeak.Width / 2.35) / msPeak.XValue * 1e6;    //   peak's sigma value / mz * 1e6

                HashSet<int> potentialChargeStates;
                if (UseAutocorrelationChargeDetermination)
                {
                    var chargeState = PattersonChargeStateCalculator.GetChargeState(xyData, mspeakList, msPeak as MSPeak);
                    potentialChargeStates = new HashSet<int> {chargeState};
                }
                else
                {   //Paul subtraction
                    IqLogger.LogDebug("MZ value: " + msPeak.XValue + "\n");
                    potentialChargeStates = GetPotentialChargeStates(indexOfCurrentPeak, mspeakList, ppmTolerance);
                    #region Paul Addition
                    var chargeDecider= new ChromCorrelatingChargeDecider(_run);
                    chargeDecider.GetPotentialChargeState(indexOfCurrentPeak, mspeakList, ppmTolerance);

                    #endregion
                }
                var reportString201="potentialChargeStates: ";
                foreach (var charge in potentialChargeStates)
                {
                    reportString201+=charge + "\t";

                }
                IqLogger.LogDebug(reportString201 + "\n");

                var potentialMSFeaturesForGivenChargeState = new List<IsotopicProfile>();
                foreach (var potentialChargeState in potentialChargeStates)
                {
                    var bestFitVal = 1.0;   // 1.0 is worst fit value. Start with 1.0 and see if we can find better fit value


                    //TODO: there could be a problem here
                    var msFeature = GetMSFeature(mspeakList, xyData, potentialChargeState, msPeak, ref bestFitVal, out var theorIso);

                    if (msFeature != null)
                    {
                        msFeature.Score = bestFitVal;
                        msFeature.IntensityMostAbundant = msFeature.getMostIntensePeak().Height;

                        var indexMostAbundantPeakTheor = theorIso.GetIndexOfMostIntensePeak();

                        //Paul edit. "&& indexMostAbundantPeakTheor>=0"
                        if (msFeature.Peaklist.Count > indexMostAbundantPeakTheor && indexMostAbundantPeakTheor>=0)
                        {

                            msFeature.IntensityMostAbundantTheor = msFeature.Peaklist[indexMostAbundantPeakTheor].Height;
                        }
                        else
                        {
                            msFeature.IntensityMostAbundantTheor = msFeature.IntensityMostAbundant;
                        }


                        var msFeatureAlreadyPresentInAnotherChargeState = listOfMonoMZs.ContainsValue(msFeature.MonoPeakMZ);

                        if (!msFeatureAlreadyPresentInAnotherChargeState)
                        {
                            potentialMSFeaturesForGivenChargeState.Add(msFeature);
                        }
                        else
                        {
                            //Console.WriteLine( "Nope... not using this charge state... MSFeature already found with same MonoMZ. \tcurrent peak= \t" +msPeak.XValue.ToString("0.0000") + "\tmsfeature= " + msFeature);
                        }

                    }

                }

                IsotopicProfile msfeature;
                if (potentialMSFeaturesForGivenChargeState.Count == 0)
                {
                    stringBuilder.Append(msPeak.XValue.ToString("0.00000") + "\tNo profile found.\n");
                    msfeature = null;
                }
                else if (potentialMSFeaturesForGivenChargeState.Count == 1)
                {
                    msfeature = potentialMSFeaturesForGivenChargeState[0];

                    stringBuilder.Append(msPeak.XValue.ToString("0.00000") + "\t" +
                                     msfeature.MonoPeakMZ.ToString("0.0000") + "\t" +
                                     msfeature.ChargeState + "\t" + msfeature.Score + "\t" + ppmTolerance + "\n");

                }
                else
                {
                    stringBuilder.Append("Multiple candidates found...." + "\n");

                    foreach (var isotopicProfile in potentialMSFeaturesForGivenChargeState)
                    {
                        stringBuilder.Append(msPeak.XValue.ToString("0.00000") + "\t" +
                                    isotopicProfile.MonoPeakMZ.ToString("0.0000") + "\t" +
                                    isotopicProfile.ChargeState + "\t" + isotopicProfile.Score + "\t" + ppmTolerance + "\n");
                    }
                    stringBuilder.Append(Environment.NewLine);

                    if (Parameters.CheckAllPatternsAgainstChargeState1)
                    {
                        msfeature = potentialMSFeaturesForGivenChargeState.FirstOrDefault(n => n.ChargeState == 1);
                    }
                    else
                    {
                        #region Paul addition
                        //TODO: [Paul]  This is the major means of deciding between charge states and where we need to do better.
                        //We need some test cases to capture this problem.
   var stopwatch = new Stopwatch();

                        if (doPaulMethod)
                        {
                            var peaksNotLoaded = _run.ResultCollection.MSPeakResultList == null ||
                                                  _run.ResultCollection.MSPeakResultList.Count == 0;
                            if (peaksNotLoaded)
                            {
                                stopwatch.Start();
                                LoadPeaks(_run);
                                stopwatch.Stop();
                                IqLogger.LogDebug("stopwatch: " + stopwatch.Elapsed);
                            }
                            var brain = new ChromCorrelatingChargeDecider(_run);
                            msfeature = brain.DetermineCorrectIsotopicProfile(potentialMSFeaturesForGivenChargeState.Where(n => n.Score < .50).ToList()) ??
                                        brain.DetermineCorrectIsotopicProfile(potentialMSFeaturesForGivenChargeState);
                            //hitcounter2++;
                        }
                        else//do it the regular way.
                        {
                        #endregion
                            msfeature = (from n in potentialMSFeaturesForGivenChargeState
                                         where n.Score < 0.15
                                         orderby n.ChargeState descending
                                         select n).FirstOrDefault() ?? (from n in potentialMSFeaturesForGivenChargeState
                                                                        orderby n.Score
                                                                        select n).First();


                            #region Paul Addition
                        }
                        //line outputs.
                        var reportString309 = "\nM/Z = " + msfeature.MonoPeakMZ +
                                "\nCHOSEN CHARGE: " + msfeature.ChargeState + "\n\n";
                        IqLogger.LogDebug(reportString309);

                        //tabular output
                        //string reportString309 = "\tM/Z = \t" + msfeature.MonoPeakMZ +
                        //        "\tCHOSEN CHARGE: \t" + msfeature.ChargeState+ "\n";
                        //IqLogger.Log.Debug(reportString309);
                            #endregion



                    }





                }

                if (msfeature != null)
                {

                    listOfMonoMZs.Add(currentUniqueMSFeatureIDNum, msfeature.MonoPeakMZ);
                    currentUniqueMSFeatureIDNum++;

                    isotopicProfiles.Add(msfeature);
                    //hitcounter++;//Paul Addition


                    foreach (var peak in msfeature.Peaklist)
                    {
                        //For debugging
                        //if (peak.XValue > 534.76515 && peak.XValue < 534.78515) //(peak.XValue>579.62 && peak.XValue<579.65) || (peak.XValue>579.75 && peak.XValue<579.8))
                        //{
                        //    int x = 39843;
                        //}
                        peaksAlreadyProcessed.Add(peak);
                    }
                }



            }//end of foreach peak loop

            //Console.WriteLine(stringBuilder.ToString());

            var uniqueIsotopicProfiles = removeDuplicatesFromFoundMSFeatures(isotopicProfiles);
            #region Paul Addition
            //IqLogger.Log.Debug("Hit counter: " + hitcounter);
            //IqLogger.Log.Debug("Hit counter2: " + hitcounter2);
            //var uniqueOtherIsotopicProfiles = removeDuplicatesFromFoundMSFeatures(isotopicProfiles);

            //IqLogger.Log.Debug("old non unique count: " + otherIsotopicProfiles.Count + "\n" +
            //    "new non unique count: " + myIsotopicProfiles.Count + "\n");
            //var uniqueMyIsotopicProfiles = removeDuplicatesFromFoundMSFeatures(myIsotopicProfiles);

            //IqLogger.Log.Debug("\nOld unique profile count: " + uniqueOtherIsotopicProfiles.Count + "\n" +
            //    "New unique profile count: " + uniqueMyIsotopicProfiles.Count);
            //IqLogger.Log.Debug("\nunique profile count: " + uniqueIsotopicProfiles.Count + "\n");

            #endregion
            //NOTE: we don't need to do the reordering, but I do this so I can compare to the old THRASH
            uniqueIsotopicProfiles = uniqueIsotopicProfiles.OrderByDescending(p => p.IntensityMostAbundantTheor).ToList();
            return uniqueIsotopicProfiles;

        }

        private void LoadPeaks(Run run)
        {
            var sourcePeaksFile = Path.Combine(run.DataSetPath, run.DatasetName + "_peaks.txt");

            RunUtilities.GetPeaks(run, sourcePeaksFile);

            //create / load
            //TODO: adjust the RunUtilities class so that it can simply take a run and create the _peaks and load them.
        }

        /// <summary>
        /// Returns list of potential charge states for a given peak; Use indexOfCurrentPeak to indicate the peak.
        /// </summary>
        /// <param name="indexOfCurrentPeak"></param>
        /// <param name="mspeakList"></param>
        /// <param name="toleranceInPPM"></param>
        /// <param name="maxCharge"></param>
        /// <returns></returns>
        public HashSet<int> GetPotentialChargeStates(int indexOfCurrentPeak, List<Peak> mspeakList, double toleranceInPPM, double maxCharge = 10)
        {
            var potentialChargeStates = new HashSet<int>();

            var basePeak = mspeakList[indexOfCurrentPeak];
            // determine max charge state possible by getting nearest candidate peak
            for (var i = indexOfCurrentPeak - 1; i >= 0; i--)
            {
                var comparePeak = mspeakList[i];

                if (Math.Abs(comparePeak.XValue - basePeak.XValue) > 1.1)
                {
                    break;
                }

                for (var j = 1; j <= maxCharge; j++)
                {
                    var expectedMZ = basePeak.XValue - Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / j;

                    var diff = Math.Abs(comparePeak.XValue - expectedMZ);
                    var toleranceInMZ = toleranceInPPM * expectedMZ / 1e6;

                    if (diff < toleranceInMZ)
                    {
                        potentialChargeStates.Add(j);
                    }

                }

            }

            for (var i = indexOfCurrentPeak + 1; i < mspeakList.Count; i++)
            {
                var comparePeak = mspeakList[i];

                if (Math.Abs(comparePeak.XValue - basePeak.XValue) > 1.1)
                {
                    break;
                }

                for (var j = 1; j <= maxCharge; j++)
                {
                    var expectedMZ = basePeak.XValue + Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / j;

                    var diff = Math.Abs(comparePeak.XValue - expectedMZ);
                    var toleranceInMZ = toleranceInPPM * expectedMZ / 1e6;

                    if (diff < toleranceInMZ)
                    {
                        potentialChargeStates.Add(j);
                    }

                }
            }

            return potentialChargeStates;

            //

        }


        #endregion

        #region Private Methods

        /// <summary>
        /// Method for creating all theoretical profiles in advance. There's not much advantage in doing this...
        /// </summary>
        /// <param name="startMass"></param>
        /// <param name="stopMass"></param>
        private void CreateAllTheoreticalProfilesForMassRange(int startMass = 400, int stopMass = 5000)
        {
            for (var i = startMass; i <= stopMass; i++)
            {
                var profile = _isotopicDistCalculator.GetAveraginePattern(i);

                _averagineProfileLookupTable.Add(i, profile);

            }

        }

        /// <summary>
        /// Take a grouping of peaks. Retains their index values. Then sorts by descending intensity. And returns indexes of top n peaks.
        /// </summary>
        /// <param name="iso"></param>
        /// <param name="numPeaks"></param>
        /// <returns></returns>
        private List<int> GetIndexesOfTopPeaks(IsotopicProfile iso, int numPeaks)
        {
            var peakList = new Dictionary<int, Peak>();

            for (var i = 0; i < iso.Peaklist.Count; i++)
            {
                peakList.Add(i, iso.Peaklist[i]);
            }

            var topIndexes = peakList.OrderByDescending(p => p.Value.Height).Take(numPeaks).Select(p => p.Key).ToList();
            return topIndexes;

        }

        private double GetReportedAbundance(IsotopicProfile profile, IsotopicProfile theoIsotopicProfile, int numPeaksUsedInAbundance = 1, int defaultVal = 0)
        {
            if (profile.Peaklist == null || profile.Peaklist.Count == 0) return defaultVal;

            Check.Require(numPeaksUsedInAbundance > 0, "NumPeaksUsedInAbundance must greater than 0. Currently it is = " + numPeaksUsedInAbundance);

            var peakIndicesToSum = GetIndexesOfTopPeaks(theoIsotopicProfile, numPeaksUsedInAbundance);

            double summedIntensities = 0;
            foreach (var i in peakIndicesToSum)
            {
                if (profile.Peaklist.Count > i)
                {
                    summedIntensities += profile.Peaklist[i].Height;
                }

            }

            return summedIntensities;

        }

        private IsotopicProfile GetMSFeature(List<Peak> mspeakList, XYData xyData, int chargeState, Peak msPeak, ref double bestFitVal, out IsotopicProfile theorIso)
        {
            var obsPeakMass = (msPeak.XValue - Globals.PROTON_MASS) * chargeState;
            theorIso = GetTheoreticalProfile(obsPeakMass);
            theorIso.ChargeState = chargeState;
            theorIso.MostAbundantIsotopeMass = obsPeakMass;

            CalculateMassesForIsotopicProfile(theorIso);
            var theorXYData = GetTheoreticalIsotopicProfileXYData(theorIso, msPeak.Width, Parameters.MinIntensityForScore / 100);

            //TODO: check how many theor peaks are being used in the fit score calculation. We should reduce this to a reasonable number.
            PerformIterativeFittingAndGetAlignedProfile(xyData, theorXYData, chargeState, ref theorIso, ref bestFitVal);

            var ppmTolerance = (msPeak.Width / 2.35) / msPeak.XValue * 1e6;  //fwhm / 2.35= sigma

            _targetedFeatureFinder.ToleranceInPPM = ppmTolerance;
            _targetedFeatureFinder.NeedMonoIsotopicPeak = false;

            //TODO: there could be a problem here with there being too many theorIso peaks and thus, too many observed iso peaks are being pulled out.
            var msFeature = _targetedFeatureFinder.FindMSFeature(mspeakList, theorIso);
            return msFeature;
        }


        public IsotopicProfile GetTheoreticalProfile(double mass)
        {
            var massUsedForLookup = (int)Math.Round(mass, 0);

            if (!_averagineProfileLookupTable.ContainsKey(massUsedForLookup))
            {
                var newtheorIso = _isotopicDistCalculator.GetAveraginePattern(mass);
                _averagineProfileLookupTable.Add(massUsedForLookup, newtheorIso);
            }

            var theorIso = _averagineProfileLookupTable[massUsedForLookup].CloneIsotopicProfile();
            return theorIso;
        }

        /// <summary>
        /// This checks if MSFeatures have the same monoisotopic mass and charge state and removes
        /// the ones of higher fit score
        /// </summary>
        /// <param name="isotopicProfiles"></param>
        /// <returns></returns>
        private List<IsotopicProfile> removeDuplicatesFromFoundMSFeatures(List<IsotopicProfile> isotopicProfiles)
        {
            if (!isotopicProfiles.Any()) return isotopicProfiles;

            var lastindex = isotopicProfiles.Count - 1;

            var sortedIsos = isotopicProfiles.OrderBy(p => p.MonoIsotopicMass).ThenBy(p => p.ChargeState).ThenBy(p => p.Score).ToList();

            for (var i = lastindex; i > 0; i--)
            {
                if (Math.Abs(sortedIsos[i].MonoIsotopicMass - sortedIsos[i - 1].MonoIsotopicMass) < double.Epsilon && sortedIsos[i].ChargeState == sortedIsos[i - 1].ChargeState)
                {
                    sortedIsos.RemoveAt(i);
                }
            }

            return sortedIsos;

        }




        private void PerformIterativeFittingAndGetAlignedProfile(XYData xyData, XYData theorXYData, int chargeState, ref IsotopicProfile theorIso, ref double bestFitVal)
        {
            if (xyData == null || xyData.Xvalues.Length == 0)
            {
                bestFitVal = 1;
                return;
            }

            double relIntensityUseForFitting = 0;

            var fitval = _areafitter.GetFit(theorXYData, xyData, relIntensityUseForFitting, out var _);

            if (fitval < bestFitVal)
            {
                bestFitVal = fitval;
            }

            double bestOffsetForTheorProfile = 0;

            // move fitting window to the left
            for (var numPeaksToTheLeft = 1; numPeaksToTheLeft < 10; numPeaksToTheLeft++)
            {
                var offsetForTheorProfile = -1 * numPeaksToTheLeft * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / chargeState;
                //negative offset

                fitval = _areafitter.GetFit(theorXYData, xyData, relIntensityUseForFitting, out _, offsetForTheorProfile);

                if (fitval > bestFitVal || fitval >= 1 || double.IsNaN(fitval))
                {
                    break;
                }

                bestFitVal = fitval;
                bestOffsetForTheorProfile = offsetForTheorProfile;
            }

            //move fitting window to the right
            for (var numPeaksToTheRight = 1; numPeaksToTheRight < 10; numPeaksToTheRight++)
            {
                var offsetForTheorProfile = numPeaksToTheRight * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / chargeState;



                fitval = _areafitter.GetFit(theorXYData, xyData, relIntensityUseForFitting, out _, offsetForTheorProfile);

                if (fitval >= bestFitVal || fitval >= 1 || double.IsNaN(fitval))
                {
                    break;
                }

                bestFitVal = fitval;
                bestOffsetForTheorProfile = offsetForTheorProfile;
            }


            foreach (var theorMSPeak in theorIso.Peaklist)
            {
                theorMSPeak.XValue = theorMSPeak.XValue + bestOffsetForTheorProfile;
            }

            theorIso.MonoPeakMZ = theorIso.getMonoPeak().XValue;

            theorIso.MonoIsotopicMass = (theorIso.MonoPeakMZ - Globals.PROTON_MASS) * chargeState;
            theorIso.MostAbundantIsotopeMass = (theorIso.getMostIntensePeak().XValue - Globals.PROTON_MASS) * chargeState;

        }

        private void CalculateMassesForIsotopicProfile(IsotopicProfile iso)
        {
            if (iso?.Peaklist == null) return;

            //start with most abundant peak.

            var indexMostAbundantPeak = iso.GetIndexOfMostIntensePeak();


            var mzMostAbundantPeak = iso.MostAbundantIsotopeMass / iso.ChargeState + Globals.PROTON_MASS;

            //start with most abundant peak and move to the LEFT and calculate m/z values
            for (var peakIndex = indexMostAbundantPeak; peakIndex >= 0; peakIndex--)
            {

                var numPeaksToLeft = indexMostAbundantPeak - peakIndex;
                var calcMZ = mzMostAbundantPeak - numPeaksToLeft * 1.00235 / iso.ChargeState;

                iso.Peaklist[peakIndex].XValue = calcMZ;

            }


            //move to the RIGHT and calculate m/z values
            for (var peakIndex = indexMostAbundantPeak + 1; peakIndex < iso.Peaklist.Count; peakIndex++)
            {

                var numPeaksToRight = peakIndex - indexMostAbundantPeak;
                var calcMZ = mzMostAbundantPeak + numPeaksToRight * 1.00235 / iso.ChargeState;

                iso.Peaklist[peakIndex].XValue = calcMZ;

            }

            iso.MonoPeakMZ = iso.getMonoPeak().XValue;
            iso.MonoIsotopicMass = (iso.MonoPeakMZ - Globals.PROTON_MASS) * iso.ChargeState;




        }

        private XYData GetTheoreticalIsotopicProfileXYData(IsotopicProfile iso, double fwhm, double minRelIntensity)
        {

            var xydata = new XYData();
            var xvals = new List<double>();
            var yvals = new List<double>();


            var mspeaks = new List<MSPeak>(iso.Peaklist);

            var mz = iso.Peaklist[0].XValue - 1 * 1.00235 / iso.ChargeState;
            var intensity = 0;

            var zeroIntensityPeakToTheLeft = new MSPeak(mz, intensity);

            mspeaks.Insert(0, zeroIntensityPeakToTheLeft);

            //TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData()


            for (var peakIndex = 0; peakIndex < mspeaks.Count; peakIndex++)
            {
                var msPeak = mspeaks[peakIndex];
                var tempXYData = TheorXYDataCalculationUtilities.GetTheorPeakData(msPeak, fwhm, NumPointsPerTheorPeak);

                for (var j = 0; j < tempXYData.Xvalues.Length; j++)
                {
                    //First peak is a zero-intensity peak. We always want to add that one. For the others,
                    //add intensity points that are above a certain intensity
                    if (peakIndex > 0)
                    {
                        if (tempXYData.Yvalues[j] >= minRelIntensity)
                        {
                            xvals.Add(tempXYData.Xvalues[j]);
                            yvals.Add(tempXYData.Yvalues[j]);
                        }

                    }
                    else
                    {
                        xvals.Add(tempXYData.Xvalues[j]);
                        yvals.Add(tempXYData.Yvalues[j]);
                    }

                }
            }
            xydata.Xvalues = xvals.ToArray();
            xydata.Yvalues = yvals.ToArray();



            return xydata;
        }



        #endregion


    }
}
