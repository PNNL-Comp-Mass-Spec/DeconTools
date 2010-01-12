extern alias RapidEngine;
using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Utilities;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    public class RapidDeconvolutor : IDeconvolutor
    {
        RapidEngine.Decon2LS.Peaks.clsPeak[] rapidPeakList;

        public static string getRapidVersion()
        {
            return AssemblyInfoRetriever.GetVersion(typeof(RapidEngine.Decon2LS.HornTransform.clsHornTransform));
        }


        private double minPeptideToBackgroundRatio;
        #region Properties

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
        }

        #endregion


        #region Public Methods
        public override void deconvolute(ResultCollection resultList)
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

            double rapidsBackgroundIntensityParameter = (resultList.Run.CurrentScanSet.BackgroundIntensity * minPeptideToBackgroundRatio);

            Transformer.PerformTransform_cluster(Convert.ToSingle(rapidsBackgroundIntensityParameter),
                ref xvals, ref yvals, ref rapidPeakList, ref chargeResults,
                ref intensityResults, ref mzResults, ref scoreResults, ref avgmassResults,
                ref massResults, ref mostAbundantMassResults);

            GenerateResults(resultList, ref chargeResults, ref intensityResults,
                ref mzResults, ref scoreResults,
                ref avgmassResults, ref massResults,
                ref mostAbundantMassResults,this.resultCombiningMode);

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

        private void GenerateResults2(ResultCollection resultList, ref int[] chargeResults, ref double[] intensityResults, ref double[] mzResults, ref double[] scoreResults, ref double[] avgmassResults, ref double[] massResults, ref double[] mostAbundantMassResults)
        {

            for (int i = 0; i < chargeResults.Length; i++)
            {
                if (chargeResults[i] == 0) continue;

                IsosResult result = resultList.CreateIsosResult();
                IsotopicProfile profile = new IsotopicProfile();
                profile.ChargeState = 1;
                profile.IntensityAggregate = 100;
                profile.Score = 40;
                MSPeak monoPeak = new MSPeak();
                monoPeak.XValue = 555;


                //GetIsotopicProfilePeaks(resultList.Run.DeconToolsPeakList, profile.ChargeState, monoPeak.MZ, ref profile);
                if (profile.Peaklist.Count == 9999999)    // couldn't find original monoIsotopicPeak in the peaklist
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

                double mostabundantPeakMZ = 400;
                //Console.WriteLine("mostAbundantPeakMZ = " + mostabundantPeakMZ);
                //Console.WriteLine("calculated mostAbundantMZ = " + ConvertMassToMZ(mostAbundantMassResults[i], profile.ChargeState));


                profile.MonoIsotopicMass = 500;
                profile.MostAbundantIsotopeMass = 900;
                profile.AverageMass = 9067;

                result.IsotopicProfile = profile;


                resultList.ResultList.Add(result);
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
