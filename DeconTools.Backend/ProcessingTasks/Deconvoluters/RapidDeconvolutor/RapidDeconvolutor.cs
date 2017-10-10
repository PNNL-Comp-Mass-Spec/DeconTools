#if INCLUDE_RAPID
extern alias RapidEngine;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    // To include support for Rapid, you must add a reference to DeconEngine.dll, which was compiled with Visual Studio 2003 and uses MSVCP71.dll
    // Note that DeconEngine.dll also depends on xerces-c_2_7.dll while DeconEngineV2.dll depends on xerces-c_2_8.dll
#if INCLUDE_RAPID

    /// <summary>
    /// This class uses the Rapid method for deconvoluting peaks
    /// The Rapid method is implemented in Visual C++ 2003 code in DeconEngine.dll
    /// That DLL depends on DLLs MSVCP71.dll and MSVCR71.dll, plus also xerces-c_2_8.dll
    /// </summary>
    /// <remarks>
    /// By default, this class will not be compiled when DeconTools.Backend.dll is compiled
    /// To enable compilation, define Conditional Compilation Constant INCLUDE_RAPID
    /// </remarks>
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

            if (resultList.Run.PeakList == null || resultList.Run.PeakList.Count == 0) return;

            rapidPeakList = ConvertPeakListToRapidPeakList(resultList.Run.PeakList);
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
                    BasicTFF bff = new BasicTFF(toleranceInPPM, false);

                    IsotopicProfile iso = bff.FindMSFeature(resultList.Run.PeakList, mt.IsotopicProfile);

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

                    XYData theorXYData = mt.IsotopicProfile.GetTheoreticalIsotopicProfileXYData(result.IsotopicProfile.GetFWHM());

                    //offset the theor isotopic profile
                    offsetDistribution(theorXYData, mt.IsotopicProfile, result.IsotopicProfile);

                    AreaFitter areafitter = new AreaFitter();
                    int ionCountUsed;
                    double fitval = areafitter.GetFit(theorXYData, result.Run.XYData, 0.1, out ionCountUsed);

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
            ScanSet currentScanset;
            if (resultList.Run is UIMFRun)
            {
                currentScanset = ((UIMFRun)resultList.Run).CurrentIMSScanSet;
            }
            else
            {
                currentScanset = resultList.Run.CurrentScanSet;
            }

            currentScanset.NumIsotopicProfiles = 0;   //reset to 0;

            for (int i = 0; i < chargeResults.Length; i++)
            {
                if (chargeResults[i] == 0) continue;

                double rapidScore = scoreResults[i];
                if ((float)rapidScore == 0.9999999999999f) continue;   // this is an oddity about the Rapid results. For very poor or immeasurable scores, it will give a score of 1.000000000;

                IsosResult result = resultList.CreateIsosResult();
                result.IntensityAggregate = intensityResults[i];

                IsotopicProfile profile = new IsotopicProfile();
                profile.ChargeState = chargeResults[i];
                profile.Score = scoreResults[i];
                MSPeak monoPeak = new MSPeak();
                monoPeak.XValue = ConvertMassToMZ(massResults[i], profile.ChargeState);



                //TODO:  make it so that the entire isotopic profile peak list is populated. Right now, just the monoisotopic peak is found.
                GetIsotopicProfilePeaks(resultList.Run.PeakList, profile.ChargeState, monoPeak.XValue, ref profile);

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
                this.AddDeconResult(resultList, result, comboMode);

                currentScanset.NumIsotopicProfiles++;
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
                profile.IntensityMostAbundant = (float)intensityResults[i];
                profile.IntensityMostAbundantTheor = profile.IntensityMostAbundant;
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


        private void GetIsotopicProfilePeaks(List<Peak> peakList, int chargeState, double monoMass, ref IsotopicProfile inputProfile)
        {
            double toleranceInPPM = 20;
            var tff = new BasicTFF(toleranceInPPM);

            var theorProfile = new IsotopicProfile();
            theorProfile.MonoIsotopicMass = monoMass;
            theorProfile.ChargeState = chargeState;
            theorProfile.MonoPeakMZ = monoMass / chargeState + Globals.PROTON_MASS;

            //a hack to guess how many peaks to include in the theor isotopic profile
            int numPeaksToIncludeInProfile = (int)Math.Round(Math.Max(3, 3 + (monoMass - 1000) / 1000));

            double monoPeakMZ = monoMass / chargeState + Globals.PROTON_MASS;
            for (int i = 0; i < numPeaksToIncludeInProfile; i++)
            {
                var peak = new MSPeak();
                peak.XValue = monoPeakMZ + i * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / chargeState;

                if (i == 0)
                {
                    peak.Height = 1;
                }
                else
                {
                    peak.Height = 0;
                }


                theorProfile.Peaklist.Add(peak);
            }

            var foundIso = tff.FindMSFeature(peakList, theorProfile);

            if (foundIso==null)
            {
                var monoPeak = PeakUtilities.GetPeaksWithinTolerance(peakList, monoPeakMZ, toleranceInPPM).OrderByDescending(p => p.Height).FirstOrDefault();


                if (monoPeak!=null)
                {
                    inputProfile.Peaklist.Add((MSPeak) monoPeak);
                }



            }
            else
            {
                inputProfile.Peaklist = new List<MSPeak>(foundIso.Peaklist);
            }



        }




        private void GetIsotopicProfilePeaks(DeconToolsV2.Peaks.clsPeak[] peaklist, int chargeState, double monoMZ, ref IsotopicProfile profile)
        {
            profile.Peaklist = new List<MSPeak>();

            double mzVar = 0.01;  //  TODO:  find a more accurate way of defining the mz variability
            DeconToolsV2.Peaks.clsPeak monoPeak = lookupPeak(monoMZ, peaklist, mzVar);
            if (monoPeak != null)
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
            peak.SignalToNoise = (float)monoPeak.mdbl_SN;
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

        private RapidEngine.Decon2LS.Peaks.clsPeak[] ConvertPeakListToRapidPeakList(List<Peak> peaklist)
        {
            if (peaklist == null || peaklist.Count == 0) return null;
            var rapidPeaklist = new RapidEngine.Decon2LS.Peaks.clsPeak[peaklist.Count];

            for (int i = 0; i < peaklist.Count; i++)
            {
                RapidEngine.Decon2LS.Peaks.clsPeak peak = new RapidEngine.Decon2LS.Peaks.clsPeak();


                peak.mint_peak_index = i;
                peak.mint_data_index = peaklist[i].DataIndex;
                peak.mdbl_mz = peaklist[i].XValue;
                peak.mdbl_intensity = peaklist[i].Height;
                peak.mdbl_SN = 100;
                peak.mdbl_FWHM = peaklist[i].Width;

                rapidPeaklist[i] = peak;

            }
            return rapidPeaklist;
        }

    #endregion


    }

#endif

}
