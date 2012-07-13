using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Algorithms.ChargeStateDetermination.PattersonAlgorithm;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;

namespace DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor
{
    public class ThrashDeconvolutorV2 : Deconvolutor
    {
        private PattersonChargeStateCalculator _chargeStateCalculator = new PattersonChargeStateCalculator();

        IsotopicDistributionCalculator _isotopicDistCalculator = IsotopicDistributionCalculator.Instance;

        private readonly AreaFitter _areafitter = new AreaFitter();

        private BasicTFF _targetedFeatureFinder = new BasicTFF();

        private Dictionary<int, IsotopicProfile> _averagineProfileLookupTable = null;

            #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        public Dictionary<int,IsotopicProfile>CreateTheoreticalProfilesForMassRange(int startMass=400, int stopMass=5000)
        {

            Dictionary<int, IsotopicProfile> isotopicProfileDictionary = new Dictionary<int, IsotopicProfile>();

            for (int i = startMass; i <= stopMass; i++)
            {
                IsotopicProfile profile=  _isotopicDistCalculator.GetAveraginePattern(startMass);

                isotopicProfileDictionary.Add(i, profile);

            }

            return isotopicProfileDictionary;


        }



        #endregion

        #region Private Methods

        #endregion

        public override void deconvolute(ResultCollection resultList)
        {
            throw new NotImplementedException();
        }



        public List<IsosResult> PerformThrash(XYData originalXYData, List<Peak> mspeakList, double backgroundIntensity = 0, double minPeptideIntensity = 0)
        {
            if (_averagineProfileLookupTable==null)
            {
                _averagineProfileLookupTable = CreateTheoreticalProfilesForMassRange();
            }


            List<IsosResult> isosResults = new List<IsosResult>();


            XYData xyData = new XYData();
            xyData.Xvalues = originalXYData.Xvalues;
            xyData.Yvalues = originalXYData.Yvalues;


            Dictionary<Peak, bool> peaksThatWereProcessedInfo = new Dictionary<Peak, bool>();

            List<Peak> sortedPeaklist = new List<Peak>(mspeakList).OrderByDescending(p => p.Height).ToList();

            List<Peak> peaksAlreadyProcessed = new List<Peak>();



            StringBuilder stringBuilder = new StringBuilder();

            int peakCounter = -1;
            foreach (var msPeak in sortedPeaklist)
            {

                if (peaksAlreadyProcessed.Contains(msPeak))
                {
                    continue;
                    
                }



                peakCounter++;

                if (peakCounter == 465)
                {
                    // Console.WriteLine(peakCounter);
                }

                int chargeState = _chargeStateCalculator.GetChargeState(xyData, mspeakList, msPeak as MSPeak);

                double bestFitVal = 1.0;   // 1.0 is worst fit value. Start with 1.0 and see if we can find better fit value
                if (chargeState == -1)
                {

                }
                else
                {
                    double obsPeakMass = (msPeak.XValue - Globals.PROTON_MASS) * chargeState;

                    int massUsedForLookup = (int)Math.Round(obsPeakMass, 0);

                    IsotopicProfile theorIso;
                    if (_averagineProfileLookupTable.ContainsKey(massUsedForLookup))
                    {
                        theorIso = _averagineProfileLookupTable[massUsedForLookup].CloneIsotopicProfile();
                    }
                    else
                    {
                        theorIso= _isotopicDistCalculator.GetAveraginePattern(obsPeakMass);
                    }
                    
                     
                    theorIso.ChargeState = chargeState;
                    theorIso.MostAbundantIsotopeMass = obsPeakMass;


                    CalculateMassesForIsotopicProfile(theorIso);
                    XYData theorXYData = TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData(theorIso, msPeak.Width);

                    PerformIterativeFittingAndGetAlignedProfile(xyData, theorXYData, chargeState, ref theorIso, ref bestFitVal);

                    var msFeature = _targetedFeatureFinder.FindMSFeature(mspeakList, theorIso, 10, false);

                    string reportstring;
                    if (msFeature!=null)
                    {
                        peaksAlreadyProcessed.AddRange(msFeature.Peaklist);
                        msFeature.Score = bestFitVal;

                        IsosResult isosResult = new StandardIsosResult();
                        isosResult.IsotopicProfile = msFeature;

                        isosResults.Add(isosResult);

                        reportstring = msPeak.XValue.ToString("0.00000") + "\t" +
                                       msFeature.MonoIsotopicMass.ToString("0.0000") + "\t" +
                                       msFeature.ChargeState + "\t" + msFeature.Score;
                    }
                    else
                    {
                        reportstring = msPeak.XValue.ToString("0.00000") + "\tNo profile found.";
                    }


                    //Console.WriteLine(reportstring);

                    stringBuilder.Append(reportstring + "\n");




                }

                
            }

            Console.WriteLine(stringBuilder.ToString());

            return isosResults;

        }

        private void PerformIterativeFittingAndGetAlignedProfile(XYData xyData, XYData theorXYData, int chargeState, ref IsotopicProfile theorIso, ref double bestFitVal)
        {
            if (xyData==null || xyData.Xvalues.Length==0)
            {
                bestFitVal = 1;
                return;
            }





            double fitval = _areafitter.GetFit(theorXYData, xyData, 0.1);

            if (fitval < bestFitVal)
            {
                bestFitVal = fitval;
            }

            double bestOffsetForTheorProfile = 0;

            // move fitting window to the left
            for (int numPeaksToTheLeft = 1; numPeaksToTheLeft < 10; numPeaksToTheLeft++)
            {
                double offsetForTheorProfile = -1 * numPeaksToTheLeft * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / chargeState;
                //negative offset

                fitval = _areafitter.GetFit(theorXYData, xyData, 0.1, offsetForTheorProfile);

                if (fitval > bestFitVal || fitval>=1)
                {
                    break;
                }
                
                bestFitVal = fitval;
                bestOffsetForTheorProfile = offsetForTheorProfile;
            }

            //move fitting window to the right
            for (int numPeaksToTheRight = 1; numPeaksToTheRight < 10; numPeaksToTheRight++)
            {
                double offsetForTheorProfile = numPeaksToTheRight * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / chargeState;

                

                fitval = _areafitter.GetFit(theorXYData, xyData, 0.1, offsetForTheorProfile);

                if (fitval >= bestFitVal || fitval >= 1)
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


        public void CalculateMassesForIsotopicProfile(IsotopicProfile iso)
        {
            if (iso == null || iso.Peaklist == null) return;

            //start with most abundant peak.

            int indexMostAbundantPeak = iso.GetIndexOfMostIntensePeak();


            double mzMostAbundantPeak = iso.MostAbundantIsotopeMass / iso.ChargeState + Globals.PROTON_MASS;

            //start with most abundant peak and move to the left and calculate m/z values
            for (int peakIndex = indexMostAbundantPeak; peakIndex >= 0; peakIndex--)
            {

                int numPeaksToLeft = indexMostAbundantPeak - peakIndex;
                double calcMZ = mzMostAbundantPeak + numPeaksToLeft * 1.00235 / iso.ChargeState;

                iso.Peaklist[peakIndex].XValue = calcMZ;

            }


            //move to the right and calculate m/z values
            for (int peakIndex = indexMostAbundantPeak + 1; peakIndex < iso.Peaklist.Count; peakIndex++)
            {

                int numPeaksToRight = peakIndex - indexMostAbundantPeak;
                double calcMZ = mzMostAbundantPeak + numPeaksToRight * 1.00235 / iso.ChargeState;

                iso.Peaklist[peakIndex].XValue = calcMZ;

            }

            iso.MonoPeakMZ = iso.getMonoPeak().XValue;
            iso.MonoIsotopicMass = (iso.MonoPeakMZ - Globals.PROTON_MASS) * iso.ChargeState;




        }
    }
}
