extern alias RapidEngine;
using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    public class RapidDeconvolutor : Deconvolutor
    {
        RapidEngine.Decon2LS.Peaks.clsPeak[] rapidPeakList;

        DeconTools.Backend.ProcessingTasks.FitScoreCalculators.DeconToolsFitScoreCalculator fitScoreCalculator;

        TomIsotopicPattern _TomIsotopicPatternCreator = new TomIsotopicPattern();


        //The BasicTFF is used to re-find the peaks of the isotopic profile, once Rapid returns the monoIsotopic mass and charge state.  
        //We have to do this since RAPID doesn't return the list of peaks within the MSFeature. 
        BasicTFF targetedFeatureFinder;


        public static string getRapidVersion()
        {
            return AssemblyInfoRetriever.GetVersion(typeof(RapidEngine.Decon2LS.HornTransform.clsHornTransform));
        }


        private double minPeptideToBackgroundRatio;
        #region Properties

        public bool IsNewFitCalculationPerformed { get; set; }

        private DeconResultComboMode resultCombiningMode;

        public DeconResultComboMode ResultCombiningMode
        {
            get { return resultCombiningMode; }
            set { resultCombiningMode = value; }
        }

        private RapidEngine.Decon2LS.HornTransform.clsHornTransform transformer;

        internal RapidEngine.Decon2LS.HornTransform.clsHornTransform Transformer
        {
            get
            {
                if (transformer == null)
                {
                    transformer = new RapidEngine.Decon2LS.HornTransform.clsHornTransform();
                    //transformer.TransformParameters = loadDeconEngineHornParameters();
                }

                return transformer;
            }
            set { transformer = value; }
        }
        #endregion


        #region Constructors
        public RapidDeconvolutor()
            : this(5, DeconResultComboMode.simplyAddIt)
        {

        }

        public RapidDeconvolutor(DeconResultComboMode comboMode)
            : this(5, comboMode)
        {

        }

        public RapidDeconvolutor(double minPeptideToBackgroundRatio, DeconResultComboMode comboMode)
        {
            this.minPeptideToBackgroundRatio = minPeptideToBackgroundRatio;
            this.resultCombiningMode = comboMode;
            this.IsNewFitCalculationPerformed = true;
            this.fitScoreCalculator = new DeconTools.Backend.ProcessingTasks.FitScoreCalculators.DeconToolsFitScoreCalculator();
            this.targetedFeatureFinder = new BasicTFF();
        }

        #endregion


        #region Public Methods
        public override void Deconvolute(ResultCollection resultList)
        {
            float[] xvals = new float[1];
            float[] yvals = new float[1];
            resultList.Run.XYData.GetXYValuesAsSingles(ref xvals, ref yvals);

            int sizeOfRapidArray = 10000;

            int[] chargeResults = new int[sizeOfRapidArray];
            double[] intensityResults = new double[sizeOfRapidArray];
            double[] mzResults = new double[sizeOfRapidArray];
            double[] scoreResults = new double[sizeOfRapidArray];
            double[] avgmassResults = new double[sizeOfRapidArray];
            double[] massResults = new double[sizeOfRapidArray];
            double[] mostAbundantMassResults = new double[sizeOfRapidArray];

            rapidPeakList = ConvertPeakListToRapidPeakList(resultList.Run.DeconToolsPeakList);
            if (rapidPeakList == null || rapidPeakList.Length == 0) return;

            double rapidsBackgroundIntensityParameter = (resultList.Run.CurrentBackgroundIntensity * minPeptideToBackgroundRatio);

            Transformer.PerformTransform_cluster(Convert.ToSingle(rapidsBackgroundIntensityParameter),
                ref xvals, ref yvals, ref rapidPeakList, ref chargeResults,
                ref intensityResults, ref mzResults, ref scoreResults, ref avgmassResults,
                ref massResults, ref mostAbundantMassResults);

            GenerateResults(resultList, ref chargeResults, ref intensityResults,
                ref mzResults, ref scoreResults,
                ref avgmassResults, ref massResults,
                ref mostAbundantMassResults, this.resultCombiningMode);

            if (this.IsNewFitCalculationPerformed)
            {

                //HACK:  RAPID doesn't return the peaks of the isotopic profile. And it's score is meaningless. So will iterate over
                //the results and 1) get the peaks of the isotopic profile  and  2) get a least-squares fit of the isotopic profile.
                foreach (IsosResult result in resultList.IsosResultBin)
                {
                    //create a temporary mass tag, as a data object for storing relevent info, and using the CalculateMassesForIsotopicProfile() method. 
                    PeptideTarget mt = new PeptideTarget();

                    mt.ChargeState = (short)result.IsotopicProfile.ChargeState;
                    mt.MonoIsotopicMass = result.IsotopicProfile.MonoIsotopicMass;
                    mt.MZ = (mt.MonoIsotopicMass / mt.ChargeState) + Globals.PROTON_MASS;

                    mt.EmpiricalFormula = _TomIsotopicPatternCreator.GetClosestAvnFormula(result.IsotopicProfile.MonoIsotopicMass, false);

                    mt.IsotopicProfile = _TomIsotopicPatternCreator.GetIsotopePattern(mt.EmpiricalFormula, _TomIsotopicPatternCreator.aafIsos);
                    mt.CalculateMassesForIsotopicProfile(mt.ChargeState);

                    double toleranceInPPM = calcToleranceInPPMFromIsotopicProfile(result.IsotopicProfile);

                    //this finds the isotopic profile based on the theor. isotopic profile.
                    BasicTFF bff = new BasicTFF(toleranceInPPM);

                    IsotopicProfile iso = bff.FindMSFeature(resultList.Run.PeakList, mt.IsotopicProfile, toleranceInPPM, false);

                    if (iso != null && iso.Peaklist != null && iso.Peaklist.Count > 1)
                    {
                        //start at the second peak... and add the newly found peaks
                        for (int i = 1; i < iso.Peaklist.Count; i++)
                        {
                            result.IsotopicProfile.Peaklist.Add(iso.Peaklist[i]);
                        }

                        //now that we have the peaks, we can get info for MonoPlusTwoAbundance
                        result.IsotopicProfile.MonoPlusTwoAbundance = result.IsotopicProfile.GetMonoPlusTwoAbundance();
                    }

                    XYData theorXYData = TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData(mt.IsotopicProfile, result.IsotopicProfile.GetFWHM());

                    //offset the theor isotopic profile
                    offsetDistribution(theorXYData, mt.IsotopicProfile, result.IsotopicProfile);

                    AreaFitter areafitter = new AreaFitter();
                    double fitval = areafitter.GetFit(theorXYData, result.Run.XYData, 0.1);

                    if (fitval == double.NaN || fitval > 1) fitval = 1;

                    result.IsotopicProfile.Score = fitval;


                }


            }

        }

        private double calcToleranceInPPMFromIsotopicProfile(IsotopicProfile isotopicProfile)
        {
            double toleranceInPPM = 20;
            if (isotopicProfile == null || isotopicProfile.Peaklist == null || isotopicProfile.Peaklist.Count == 0)
            {
                return toleranceInPPM;
            }

            double fwhm = isotopicProfile.GetFWHM();
            double toleranceInMZ = fwhm / 2;

            toleranceInPPM = toleranceInMZ / isotopicProfile.MonoPeakMZ * 1e6;

            return toleranceInPPM;

        }

        private void offsetDistribution(XYData theorXYData, IsotopicProfile theorIsotopicProfile, IsotopicProfile obsIsotopicProfile)
        {
            double offset = 0;
            if (theorIsotopicProfile == null || theorIsotopicProfile.Peaklist == null || theorIsotopicProfile.Peaklist.Count == 0) return;

            MSPeak mostIntensePeak = theorIsotopicProfile.getMostIntensePeak();
            int indexOfMostIntensePeak = theorIsotopicProfile.Peaklist.IndexOf(mostIntensePeak);

            if (obsIsotopicProfile.Peaklist == null || obsIsotopicProfile.Peaklist.Count == 0) return;

            bool enoughPeaksInTarget = (indexOfMostIntensePeak <= obsIsotopicProfile.Peaklist.Count - 1);

            if (enoughPeaksInTarget)
            {
                MSPeak targetPeak = obsIsotopicProfile.Peaklist[indexOfMostIntensePeak];
                offset = targetPeak.XValue - mostIntensePeak.XValue;
                //offset = observedIsotopicProfile.Peaklist[0].XValue - theorIsotopicProfile.Peaklist[0].XValue;   //want to test to see if Thrash is same as rapid

            }
            else
            {
                offset = obsIsotopicProfile.Peaklist[0].XValue - theorIsotopicProfile.Peaklist[0].XValue;
            }

            for (int i = 0; i < theorXYData.Xvalues.Length; i++)
            {
                theorXYData.Xvalues[i] = theorXYData.Xvalues[i] + offset;
            }

            foreach (var peak in theorIsotopicProfile.Peaklist)
            {
                peak.XValue = peak.XValue + offset;

            }
        }

        #endregion


        #region Private Methods
        private int getNumIsotopicProfiles(int[] chargeResults)
        {
            int counter = 0;
            for (int i = 0; i < chargeResults.Length; i++)
            {
                if (chargeResults[i] == 0) continue;
                counter++;
            }
            return counter;
        }

        private void GenerateResults(ResultCollection resultList, ref int[] chargeResults,
            ref double[] intensityResults, ref double[] mzResults, ref double[] scoreResults,
            ref double[] avgmassResults, ref double[] massResults,
            ref double[] mostAbundantMassResults, DeconResultComboMode comboMode)
        {
            resultList.Run.CurrentScanSet.NumIsotopicProfiles = 0;   //reset to 0;

            for (int i = 0; i < chargeResults.Length; i++)
            {
                if (chargeResults[i] == 0) continue;

                double rapidScore = scoreResults[i];
                if ((float)rapidScore == 0.9999999999999f) continue;   // this is an oddity about the Rapid results. For very poor or immeasurable scores, it will give a score of 1.000000000; 

                IsosResult result = resultList.CreateIsosResult();

                IsotopicProfile profile = new IsotopicProfile();
                profile.ChargeState = chargeResults[i];
                profile.IntensityAggregate = intensityResults[i];
                profile.Score = scoreResults[i];
                MSPeak monoPeak = new MSPeak();
                monoPeak.XValue = ConvertMassToMZ(massResults[i], profile.ChargeState);


                //TODO:  make it so that the entire isotopic profile peak list is populated. Right now, just the monoisotopic peak is found. 
                GetIsotopicProfilePeaks(resultList.Run.DeconToolsPeakList, profile.ChargeState, monoPeak.XValue, ref profile);

                if (profile.Peaklist.Count == 0)    // couldn't find original monoIsotopicPeak in the peaklist
                {
                    //So first check and see if it is the most abundant peak (mzResults returns the mz for the most abundant peak); get the m/z from there  (This m/z matches with DeconTools peaklist m/z values)
                    if (Math.Abs(monoPeak.XValue - mzResults[i]) < (1 / profile.ChargeState - 1 / profile.ChargeState * 0.2))
                    {
                        monoPeak.XValue = mzResults[i];
                        profile.Peaklist = new List<MSPeak>();
                        profile.Peaklist.Add(monoPeak);
                    }
                    else   // happens if the mono peak is not the most abundant peak.  Will have to use Rapid's calculated value for the mono peak
                    {
                        profile.Peaklist = new List<MSPeak>();
                        profile.Peaklist.Add(monoPeak);
                    }
                }

                double mostabundantPeakMZ = mzResults[i];
                //Console.WriteLine("mostAbundantPeakMZ = " + mostabundantPeakMZ);
                //Console.WriteLine("calculated mostAbundantMZ = " + ConvertMassToMZ(mostAbundantMassResults[i], profile.ChargeState));


                profile.MonoIsotopicMass = massResults[i];
                profile.MostAbundantIsotopeMass = mostAbundantMassResults[i];
                profile.AverageMass = avgmassResults[i];
                profile.MonoPeakMZ = profile.GetMZ();

                result.IsotopicProfile = profile;


                //resultList.ResultList.Add(result);
                this.CombineDeconResults(resultList, result, comboMode);

                resultList.Run.CurrentScanSet.NumIsotopicProfiles++;
            }

        }

        private List<DeconTools.Backend.Core.IsotopicProfile> ConvertRapidResultsToProfileList(DeconToolsV2.Peaks.clsPeak[] peaklist, ref int[] chargeResults, ref double[] intensityResults,
          ref double[] mzResults, ref double[] scoreResults, ref double[] avgmassResults, ref double[] massResults, ref double[] mostAbundantMassResults)
        {

            List<IsotopicProfile> isotopicProfileList = new List<IsotopicProfile>();

            for (int i = 0; i < chargeResults.Length; i++)
            {
                if (chargeResults[i] == 0) continue;
                IsotopicProfile profile = new IsotopicProfile();
                profile.ChargeState = chargeResults[i];
                profile.IntensityAggregate = intensityResults[i];
                profile.Score = scoreResults[i];
                MSPeak monoPeak = new MSPeak();
                monoPeak.XValue = ConvertMassToMZ(massResults[i], profile.ChargeState);


                GetIsotopicProfilePeaks(peaklist, profile.ChargeState, monoPeak.XValue, ref profile);
                if (profile.Peaklist.Count == 0)    // couldn't find original monoIsotopicPeak in the peaklist
                {
                    //So first check and see if it is the most abundant peak (mzResults returns the mz for the most abundant peak); get the m/z from there  (This m/z matches with DeconTools peaklist m/z values)
                    if (Math.Abs(monoPeak.XValue - mzResults[i]) < (1 / profile.ChargeState - 1 / profile.ChargeState * 0.2))
                    {
                        monoPeak.XValue = mzResults[i];
                        profile.Peaklist = new List<MSPeak>();
                        profile.Peaklist.Add(monoPeak);
                    }
                    else   // happens if the mono peak is not the most abundant peak.  Will have to use Rapid's calculated value for the mono peak
                    {
                        profile.Peaklist = new List<MSPeak>();
                        profile.Peaklist.Add(monoPeak);
                    }
                }



                double mostabundantPeakMZ = mzResults[i];
                Console.WriteLine("mostAbundantPeakMZ = " + mostabundantPeakMZ);
                Console.WriteLine("calculated mostAbundantMZ = " + ConvertMassToMZ(mostAbundantMassResults[i], profile.ChargeState));


                profile.MonoIsotopicMass = massResults[i];
                profile.MostAbundantIsotopeMass = mostAbundantMassResults[i];
                profile.AverageMass = avgmassResults[i];
                profile.MonoPeakMZ = profile.GetMZ();

                isotopicProfileList.Add(profile);
            }
            return isotopicProfileList;
        }


        private void GetIsotopicProfilePeaks(List<Peak> peakList, int chargeState, double monoMSZ, ref IsotopicProfile profile)
        {
            BasicTFF ff = new BasicTFF(20);

        }




        private void GetIsotopicProfilePeaks(DeconToolsV2.Peaks.clsPeak[] peaklist, int chargeState, double monoMZ, ref IsotopicProfile profile)
        {
            profile.Peaklist = new List<MSPeak>();

            double mzVar = 0.01;  //  TODO:  find a more accurate way of defining the mz variability
            DeconToolsV2.Peaks.clsPeak monoPeak = lookupPeak(monoMZ, peaklist, mzVar);
            if (monoPeak == null)       // here, the monoMZ as calculated from Rapid can't be matched with any peaks in the original peaklist; so return Rapid's mz value
            {
                //MSPeak peak = new MSPeak();
                //peak.MZ = monoMZ;
                //profile.Peaklist.Add(peak);
                return;
            }
            else
            {
                MSPeak mspeak = convertDeconPeakToMSPeak(monoPeak);
                profile.Peaklist.Add(mspeak);
                return;
            }

        }

        private MSPeak convertDeconPeakToMSPeak(DeconToolsV2.Peaks.clsPeak monoPeak)
        {

            MSPeak peak = new MSPeak();
            peak.XValue = monoPeak.mdbl_mz;
            peak.Width = (float)monoPeak.mdbl_FWHM;
            peak.SN = (float)monoPeak.mdbl_SN;
            peak.Height = (int)monoPeak.mdbl_intensity;

            return peak;
        }

        private DeconToolsV2.Peaks.clsPeak lookupPeak(double mz, DeconToolsV2.Peaks.clsPeak[] peaklist, double mzVar)
        {
            for (int i = 0; i < peaklist.Length; i++)
            {
                if (Math.Abs(mz - peaklist[i].mdbl_mz) <= mzVar)
                {
                    return peaklist[i];
                }
            }
            return null;      //couldn't find a peak
        }

        private double ConvertMassToMZ(double mass, int charge)
        {
            Check.Require(charge != 0, "Charge state must not be 0");
            return mass / charge + 1.0078250319;     //-0.00054858;     //1.0078250319= monoisotopic mass of hydrogen; 0.00054858 = mass of electron
        }

        private RapidEngine.Decon2LS.Peaks.clsPeak[] ConvertPeakListToRapidPeakList(DeconToolsV2.Peaks.clsPeak[] peaklist)
        {
            if (peaklist == null || peaklist.Length == 0) return null;
            RapidEngine.Decon2LS.Peaks.clsPeak[] rapidPeaklist = new RapidEngine.Decon2LS.Peaks.clsPeak[peaklist.Length];

            for (int i = 0; i < peaklist.Length; i++)
            {
                RapidEngine.Decon2LS.Peaks.clsPeak peak = new RapidEngine.Decon2LS.Peaks.clsPeak();


                peak.mint_peak_index = peaklist[i].mint_peak_index;
                peak.mint_data_index = peaklist[i].mint_data_index;
                peak.mdbl_mz = peaklist[i].mdbl_mz;
                peak.mdbl_intensity = peaklist[i].mdbl_intensity;
                peak.mdbl_SN = peaklist[i].mdbl_SN;
                peak.mdbl_FWHM = peaklist[i].mdbl_FWHM;

                rapidPeaklist[i] = peak;

            }
            return rapidPeaklist;
        }

        #endregion


        public OldDeconFormatResult[] RAPIDDeconvolute(double backgroundIntensity,
            float[] xvals, float[] yvals, DeconToolsV2.Peaks.clsPeak[] peaklist)
        {

            List<OldDeconFormatResult> resultList = new List<OldDeconFormatResult>();
            OldDeconFormatResult testResult = new OldDeconFormatResult();
            testResult.Abundance = 10100;
            testResult.Mz = 500.01;
            resultList.Add(testResult);
            return resultList.ToArray();
        }






    }
}
