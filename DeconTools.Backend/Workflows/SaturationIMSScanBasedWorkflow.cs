using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using DeconTools.Utilities;

namespace DeconTools.Backend.Workflows
{
    public class SaturationIMSScanBasedWorkflow : IMSScanBasedWorkflow
    {

        List<IsosResult> _unsummedMSFeatures = new List<IsosResult>();
        private int _peakCounter = 0;

        private MSGenerator _msGenerator;
        private DeconToolsPeakDetector _peakDetector;
        private HornDeconvolutor _deconvolutor;
        TomIsotopicPattern _tomIsotopicPatternGenerator = new TomIsotopicPattern();
        private BasicTFF _basicFeatureFinder = new BasicTFF();
        private DeconToolsFitScoreCalculator _fitScoreCalculator = new DeconToolsFitScoreCalculator();

        #region Constructors

        internal SaturationIMSScanBasedWorkflow(OldDecon2LSParameters parameters, Run run, string outputFolderPath = null, BackgroundWorker backgroundWorker = null)
            : base(parameters, run, outputFolderPath, backgroundWorker)
        {
            Check.Require(run is UIMFRun, "Cannot create workflow. Run is required to be a UIMFRun for this type of workflow");

            PeakBRSaturatedPeakDetector = parameters.PeakProcessorParameters.PeakBackgroundRatio * 3;  //use high threshold so we don't get the low intensity peaks (which won't be saturated)

            _msGenerator = new UIMF_MSGenerator();
            _peakDetector = new DeconToolsPeakDetector(PeakBRSaturatedPeakDetector, 3, Globals.PeakFitType.QUADRATIC,
                                                       false);

            _deconvolutor = new HornDeconvolutor(parameters.HornTransformParameters);
            _deconvolutor.MinPeptideBackgroundRatio = PeakBRSaturatedPeakDetector;

            IntensityThresholdForSaturation = 1E5;

            AdjustMonoIsotopicMasses = true;

            Run.PeakList = new List<IPeak>();



        }


        #endregion


        public double IntensityThresholdForSaturation { get; set; }

        public double PeakBRSaturatedPeakDetector { get; set; }

        /// <summary>
        /// Use non-saturated peaks to adjust the reported monoisotopic mass of the isotopic profile. Normally, the mono
        /// isotopic mass is based on the most abundant peak. But for saturated profiles, this is not a good thing. 
        /// </summary>
        public bool AdjustMonoIsotopicMasses { get; set; }

        protected override void IterateOverScans()
        {
            var uimfRun = (UIMFRun)Run;

            foreach (var frameset in uimfRun.FrameSetCollection.FrameSetList)
            {
                uimfRun.ResultCollection.MSPeakResultList.Clear();
                _unsummedMSFeatures.Clear();


                FrameSet unsummedFrameSet = new FrameSet(frameset.PrimaryFrame);
                //get saturated MSFeatures for unsummed data
                uimfRun.CurrentFrameSet = unsummedFrameSet;



                foreach (var scanset in uimfRun.ScanSetCollection.ScanSetList)
                {
                    uimfRun.ResultCollection.IsosResultBin.Clear();  //clear any previous MSFeatures

                    ScanSet unsummedScanset = new ScanSet(scanset.PrimaryScanNumber);
                    uimfRun.CurrentScanSet = unsummedScanset;

                    _msGenerator.Execute(Run.ResultCollection);

                    _peakDetector.Execute(Run.ResultCollection);

                    _deconvolutor.deconvolute(uimfRun.ResultCollection);     //adds to IsosResultBin

                    _unsummedMSFeatures.AddRange(Run.ResultCollection.IsosResultBin);

                    var saturatedFeatures =
                        uimfRun.ResultCollection.IsosResultBin.Where(
                            p => p.IsotopicProfile.IntensityAggregate > IntensityThresholdForSaturation).ToList();

                    foreach (var saturatedFeature in saturatedFeatures)
                    {
                        //DisplayIsotopicProfile(saturatedFeature);

                        var theorIso = new IsotopicProfile();

                        RebuildSaturatedIsotopicProfile(saturatedFeature, uimfRun.PeakList, out theorIso);

                        //DisplayIsotopicProfile(saturatedFeature);

                        //correct the intensities in peakList
                        AdjustSaturatedIsotopicProfile(saturatedFeature.IsotopicProfile, theorIso, AdjustMonoIsotopicMasses, true);

                        //DisplayIsotopicProfile(saturatedFeature);


                        //add current MSFeature to lookup table of MSFeatures

                    }




                }


                //DisplayMSFeatures(_unsummedMSFeatures);

                //now sum across IMSScans, deconvolute and adjust
                foreach (var scanset in uimfRun.ScanSetCollection.ScanSetList)
                {

                    //get the summed isotopic profile
                    uimfRun.CurrentFrameSet = frameset;
                    uimfRun.CurrentScanSet = scanset;



                    ExecuteTask(MSGenerator);

                    if (OldDecon2LsParameters.HornTransformParameters.ZeroFill)
                    {
                        ExecuteTask(ZeroFiller);
                    }

                    if (OldDecon2LsParameters.HornTransformParameters.UseSavitzkyGolaySmooth)
                    {
                        ExecuteTask(Smoother);
                    }

                    ExecuteTask(PeakDetector);

                    ExecuteTask(Deconvolutor);

                    var msfeaturesSaturated = (from n in Run.ResultCollection.IsosResultBin
                                               where
                                                   n.IsotopicProfile.IntensityAggregate >
                                                   IntensityThresholdForSaturation * 1
                                               select n).ToList();

                    //rebuild incorrectly deisotoped profiles
                    //adjust intensities for saturated profiles
                    //recalculate fit score
                    foreach (var isosResult in msfeaturesSaturated)
                    {
                        var theorIso = new IsotopicProfile();
                        RebuildSaturatedIsotopicProfile(isosResult, Run.PeakList, out theorIso);

                        //this updates the masses of the saturated peaks, based on the mass of an unsaturated peak. 
                        AdjustSaturatedIsotopicProfile(isosResult.IsotopicProfile, theorIso, AdjustMonoIsotopicMasses, false);

                        UpdateReportedSummedPeakIntensities(isosResult, frameset, scanset);

                        if (isosResult.IsotopicProfile.IsSaturated)
                        {
                            GetRebuiltFitScore(isosResult);
                        }

                    }

                    //need to remove any duplicate MSFeatures (this occurs when incorrectly deisotoped profiles are built). 
                    //Will do this by making the MSFeatureID the same. Then the Exporter will ensure that only one MSFeature per MSFeatureID
                    //is exported. This isn't ideal. Better to remove the features but this proves to be quite hard to do without large performance hits. 
                    foreach (var isosResult in Run.ResultCollection.IsosResultBin)
                    {
                        double massTolForDuplicate = 0.0001;

                        var duplicateIsosResults = (from n in Run.ResultCollection.IsosResultBin
                                                    where
                                                        Math.Abs(n.IsotopicProfile.MonoIsotopicMass -
                                                                 isosResult.IsotopicProfile.MonoIsotopicMass) <
                                                        massTolForDuplicate
                                                    select n);

                        int minMSFeatureID = int.MaxValue;
                        foreach (var dup in duplicateIsosResults)
                        {
                            if (dup.MSFeatureID < minMSFeatureID)
                            {

                                minMSFeatureID = dup.MSFeatureID;
                            }
                            else
                            {
                                dup.MSFeatureID = minMSFeatureID;
                            }

                        }
                    }

                    ExecuteTask(ResultValidator);

                    ExecuteTask(ScanResultUpdater);

                    if (OldDecon2LsParameters.HornTransformParameters.ReplaceRAPIDScoreWithHornFitScore)
                    {
                        ExecuteTask(FitScoreCalculator);
                    }

                    //Allows derived classes to execute additional tasks
                    ExecuteOtherTasksHook();


                    if (ExportData)
                    {
                        //the following exporting tasks should be last
                        if (OldDecon2LsParameters.PeakProcessorParameters.WritePeaksToTextFile)
                        {
                            ExecuteTask(PeakToMSFeatureAssociator);
                            ExecuteTask(PeakListExporter);

                        }

                        ExecuteTask(IsosResultExporter);

                        ExecuteTask(ScanResultExporter);
                    }

                    ReportProgress();
                }
            }

        }

        private void DisplayMSFeatures(List<IsosResult> msfeatures)
        {
            var sb = new StringBuilder();
            foreach (UIMFIsosResult uimfIsosResult in msfeatures)
            {
                sb.Append(uimfIsosResult.FrameSet.PrimaryFrame + "\t" + uimfIsosResult.ScanSet.PrimaryScanNumber + "\t" +
                          uimfIsosResult.IsotopicProfile.MonoIsotopicMass.ToString("0.000") + "\t" +
                          uimfIsosResult.IsotopicProfile.MonoPeakMZ.ToString("0.000") + "\t" +
                          uimfIsosResult.IsotopicProfile.IntensityAggregate + "\n");

            }
            Console.WriteLine(sb.ToString());
        }

        private void GetRebuiltFitScore(IsosResult isosResult)
        {

            isosResult.IsotopicProfile.Score = 0.0099;  //hard-code the fit score
            return;


            //PeptideTarget mt = new PeptideTarget();

            //mt.ChargeState = (short) isosResult.IsotopicProfile.ChargeState;
            //mt.MonoIsotopicMass = isosResult.IsotopicProfile.MonoIsotopicMass;
            //mt.MZ = (mt.MonoIsotopicMass/mt.ChargeState) + Globals.PROTON_MASS;

            ////TODO: use Josh's isotopicDistribution calculator after confirming averagine formula
            //mt.EmpiricalFormula = _tomIsotopicPatternGenerator.GetClosestAvnFormula(
            //    isosResult.IsotopicProfile.MonoIsotopicMass, false);

            //mt.IsotopicProfile = _tomIsotopicPatternGenerator.GetIsotopePattern(mt.EmpiricalFormula,
            //                                                                    _tomIsotopicPatternGenerator.aafIsos);


            //mt.CalculateMassesForIsotopicProfile(mt.ChargeState);

            //XYData theorXYData = TheorXYDataCalculationUtilities.Get_Theoretical_IsotopicProfileXYData(mt.IsotopicProfile,
            //                                                                                           isosResult.
            //                                                                                               IsotopicProfile.
            //                                                                                               GetFWHM());
            //XYData rebuiltXYData =
            //    TheorXYDataCalculationUtilities.Get_Theoretical_IsotopicProfileXYData(isosResult.IsotopicProfile,
            //                                                                          isosResult.IsotopicProfile.GetFWHM());

            //AreaFitter areafitter = new AreaFitter(theorXYData, rebuiltXYData, 0.1);
            //double fitval = areafitter.getFit();

            //if (double.IsNaN(fitval) || fitval > 1)
            //    fitval = 0.01; //if there is a problem, we are artificially going to set it low

            //isosResult.IsotopicProfile.Score = fitval;
        }

        private void DisplayMSPeakResults(List<MSPeakResult> list)
        {

            StringBuilder sb = new StringBuilder();


            foreach (var msPeakResult in list)
            {
                sb.Append(msPeakResult.Frame_num + "\t" + msPeakResult.Scan_num + "\t" +
                          msPeakResult.MSPeak.XValue.ToString("0.0000") + "\t" + msPeakResult.MSPeak.Height + "\n");


            }

            Console.WriteLine(sb.ToString());

        }

        protected override void ExecuteOtherTasksHook()
        {
            ExecuteTask(UimfDriftTimeExtractor);
            ExecuteTask(UimfTicExtractor);

        }


        private void UpdateReportedSummedPeakIntensities(IsosResult profile, FrameSet frameSet, ScanSet scanSet)
        {

            UIMFIsosResult uimfIsosResult = (UIMFIsosResult)profile;

            int minFrame = frameSet.getLowestFrameNumber();
            int maxFrame = frameSet.getHighestFrameNumber();

            int minScan = scanSet.getLowestScanNumber();
            int maxScan = scanSet.getHighestScanNumber();

            double massTolerance = 0.2;

            var filteredUnsummedMSFeatures = (from n in _unsummedMSFeatures
                                              where ((UIMFIsosResult)n).FrameSet.PrimaryFrame >= minFrame &&
                                                    ((UIMFIsosResult)n).FrameSet.PrimaryFrame <= maxFrame &&
                                                    n.ScanSet.PrimaryScanNumber >= minScan &&
                                                    n.ScanSet.PrimaryScanNumber <= maxScan &&
                                                    Math.Abs(n.IsotopicProfile.MonoIsotopicMass -
                                                             profile.IsotopicProfile.MonoIsotopicMass) < massTolerance
                                              select n).ToList();


            double averageMonoMass = profile.IsotopicProfile.MonoIsotopicMass;     //initialize and assign original value
            double averageMonoMZ = profile.IsotopicProfile.MonoPeakMZ;
            double averageMostAbundantPeakMonoMass = profile.IsotopicProfile.MostAbundantIsotopeMass;

            //if (filteredUnsummedMSFeatures.Count > 2)
            //{
            //    var higherAbundanceFeatures =
            //        (from n in filteredUnsummedMSFeatures where n.IsotopicProfile.IntensityAggregate > 100000 select n).
            //            ToList();

            //    if (higherAbundanceFeatures.Count>0)
            //    {
            //        averageMonoMass = filteredUnsummedMSFeatures.Select(p => p.IsotopicProfile.MonoIsotopicMass).Average();
            //        averageMonoMZ = filteredUnsummedMSFeatures.Select(p => p.IsotopicProfile.MonoPeakMZ).Average();
            //        averageMostAbundantPeakMonoMass =
            //            filteredUnsummedMSFeatures.Select(p => p.IsotopicProfile.MostAbundantIsotopeMass).Average();
            //    }

               
            //}

           

            var adjustedIntensity = filteredUnsummedMSFeatures.Sum(p => p.IsotopicProfile.IntensityAggregate);

            //var peaksToSum = (from n in Run.ResultCollection.MSPeakResultList
            //                  where n.Frame_num >= minFrame &&
            //                        n.Frame_num <= maxFrame &&
            //                        n.Scan_num >= minScan &&
            //                        n.Scan_num <= maxScan &&
            //                        n.XValue > minMZ &&
            //                        n.XValue < maxMZ
            //                  select n).ToList();

            //double adjustedIntensity = peaksToSum.Sum(p => p.Height);

            var unsummedAdjustedMSFeature = (from n in filteredUnsummedMSFeatures
                                             where
                                                 ((UIMFIsosResult)n).FrameSet.PrimaryFrame == frameSet.PrimaryFrame &&
                                                 n.ScanSet.PrimaryScanNumber == scanSet.PrimaryScanNumber
                                             select n).FirstOrDefault();

            profile.IsotopicProfile.OriginalIntensity = unsummedAdjustedMSFeature == null
                                                            ? 0
                                                            : unsummedAdjustedMSFeature.IsotopicProfile.
                                                                  IntensityAggregate;


            if (adjustedIntensity > profile.IsotopicProfile.IntensityAggregate)
            {
                profile.IsotopicProfile.IsSaturated = true;
                profile.IsotopicProfile.IntensityAggregate = adjustedIntensity;

                if (filteredUnsummedMSFeatures.Count>0)
                {
                    profile.IsotopicProfile.MonoIsotopicMass = averageMonoMass;
                    profile.IsotopicProfile.MonoPeakMZ = averageMonoMZ;
                    profile.IsotopicProfile.MostAbundantIsotopeMass = averageMostAbundantPeakMonoMass;
                }
            }

        }


        public void AdjustSaturatedIsotopicProfile(IsotopicProfile iso, IsotopicProfile theorIsotopicProfile, bool updatePeakMasses = true, bool updatePeakIntensities = true)
        {
            const double minRelIntensityForExtrapolation = 0.8;
            int indexOfObsMostAbundantPeak = iso.GetIndexOfMostIntensePeak();

            //get index of suitable peak on which to base the intensity adjustment
            int indexOfPeakUsedInExtrapolation = 0;
            var mostAbundantPeak = iso.Peaklist[indexOfObsMostAbundantPeak];

            for (int i = indexOfObsMostAbundantPeak; i < iso.Peaklist.Count; i++)
            {
                var currentPeak = iso.Peaklist[i];

                if (currentPeak.Height / mostAbundantPeak.Height < minRelIntensityForExtrapolation)
                {
                    indexOfPeakUsedInExtrapolation = i;
                    break;
                }
            }

            //ensure targetPeak is within range
            if (indexOfPeakUsedInExtrapolation >= theorIsotopicProfile.Peaklist.Count)
            {
                return;
            }

            var intensityObsPeakForExtrapolation = iso.Peaklist[indexOfPeakUsedInExtrapolation].Height;
            var intensityTheorPeakForExtrapolation =
                theorIsotopicProfile.Peaklist[indexOfPeakUsedInExtrapolation].Height;

            for (int i = 0; i < iso.Peaklist.Count; i++)
            {
                if (iso.Peaklist[i].Height > IntensityThresholdForSaturation)
                {
                    if (updatePeakIntensities)
                    {
                        iso.Peaklist[i].Height = theorIsotopicProfile.Peaklist[i].Height *
                                                 intensityObsPeakForExtrapolation /
                                                 intensityTheorPeakForExtrapolation;
                    }


                    //correct the m/z value, to more accurately base it on the non-saturated peak.  See Chernushevich et al. 2001 http://onlinelibrary.wiley.com/doi/10.1002/jms.207/abstract
                    if (updatePeakMasses)
                    {
                        iso.Peaklist[i].XValue = iso.Peaklist[indexOfPeakUsedInExtrapolation].XValue -                  // formula is  MZ0 = MZ3 - (1.003/z)*n
                                              ((Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS) / iso.ChargeState *            // where MZ0 is the m/z of the saturated peak and MZ3 the m/z of the nonSaturated peak
                                               (indexOfPeakUsedInExtrapolation - i));                                // and z is charge state and n is the difference in peak number
                    }

                }
            }

            iso.IntensityAggregate = iso.getMostIntensePeak().Height;
            UpdateMonoisotopicMassData(iso);

        }



        private void DisplayIsotopicProfile(IsosResult saturatedFeature)
        {
            var uimfFeature = (UIMFIsosResult)saturatedFeature;

            StringBuilder sb = new StringBuilder();

            sb.Append(Environment.NewLine);
            sb.Append(uimfFeature.MSFeatureID + "\t" + uimfFeature.FrameSet.PrimaryFrame + "\t" + uimfFeature.ScanSet.PrimaryScanNumber + "\t" +
                      uimfFeature.IsotopicProfile.MonoIsotopicMass.ToString("0.000") + "\t" +
                      uimfFeature.IsotopicProfile.ChargeState);

            sb.Append(Environment.NewLine);

            foreach (var msPeak in uimfFeature.IsotopicProfile.Peaklist)
            {
                sb.Append("\t" + msPeak.XValue.ToString("0.000") + "\t" + msPeak.Height);
                sb.Append(Environment.NewLine);
            }


            Logger.Instance.AddEntry(sb.ToString(), Logger.Instance.OutputFilename);
        }

        private void RebuildSaturatedIsotopicProfile(IsosResult saturatedFeature, List<IPeak> peakList, out IsotopicProfile theorIso)
        {
            //check for peak to the left


            bool needToTestForPeakToTheLeft = true;

            //this loop starts at monoisotopic peak and keeps looking left for a peak, 
            //as long as peak-to-the-left has a relative intensity > 0.5 
            while (needToTestForPeakToTheLeft)
            {
                MSPeak peakToTheLeft = GetPeakToTheLeftIfExists(saturatedFeature.IsotopicProfile,
                                                                Run.PeakList);

                if (peakToTheLeft == null)
                {
                    needToTestForPeakToTheLeft = false;
                }
                else
                {
                    saturatedFeature.IsotopicProfile.Peaklist.Insert(0, peakToTheLeft);
                }
            }

            //update monoMass and monoMZ properties for updated monoPeak
            UpdateMonoisotopicMassData(saturatedFeature.IsotopicProfile);

            //create the theoretical isotopic profile. Will need this to help rebuild and then correct the saturated
            //peaks
            theorIso = GetTheorIsotopicProfile(saturatedFeature);

            //now, starting at the updated monoisotopic peak, will re-find the other peaks and add them
            AssignMissingPeaksToSaturatedProfile(Run.PeakList, saturatedFeature.IsotopicProfile, theorIso);





        }

        private void UpdateMonoisotopicMassData(IsotopicProfile iso)
        {
            iso.MonoIsotopicMass = (iso.getMonoPeak().XValue - Globals.PROTON_MASS)* iso.ChargeState;
            iso.MonoPeakMZ = iso.getMonoPeak().XValue;
            iso.MostAbundantIsotopeMass = (iso.getMostIntensePeak().XValue - Globals.PROTON_MASS)* iso.ChargeState;
        }


        private void AssignMissingPeaksToSaturatedProfile(List<IPeak> peakList, IsotopicProfile isotopicProfile, IsotopicProfile theorIsotopicProfile)
        {
            double toleranceInPPM = calcToleranceInPPMFromIsotopicProfile(isotopicProfile);

            //this finds the isotopic profile based on the theor. isotopic profile.
            _basicFeatureFinder.ToleranceInPPM = toleranceInPPM;

            IsotopicProfile iso = _basicFeatureFinder.FindMSFeature(peakList, theorIsotopicProfile, toleranceInPPM, false);

            if (iso != null && iso.Peaklist != null && iso.Peaklist.Count > 1)
            {
                //add the newly found peaks to the saturated isotopic profile
                for (int i = isotopicProfile.Peaklist.Count; i < iso.Peaklist.Count; i++)
                {
                    isotopicProfile.Peaklist.Add(iso.Peaklist[i]);
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

        private IsotopicProfile GetTheorIsotopicProfile(IsosResult saturatedFeature)
        {
            var theorTarget = new PeptideTarget();

            theorTarget.ChargeState = (short)saturatedFeature.IsotopicProfile.ChargeState;
            theorTarget.MonoIsotopicMass = saturatedFeature.IsotopicProfile.MonoIsotopicMass;
            theorTarget.MZ = (theorTarget.MonoIsotopicMass / theorTarget.ChargeState) + Globals.PROTON_MASS;

            var averagineFormula =
                _tomIsotopicPatternGenerator.GetClosestAvnFormula(saturatedFeature.IsotopicProfile.MonoIsotopicMass, false);
            theorTarget.IsotopicProfile = _tomIsotopicPatternGenerator.GetIsotopePattern(averagineFormula,
                                                                                         _tomIsotopicPatternGenerator.aafIsos);
            theorTarget.EmpiricalFormula = averagineFormula;
            theorTarget.CalculateMassesForIsotopicProfile(saturatedFeature.IsotopicProfile.ChargeState);

            return theorTarget.IsotopicProfile;
        }

        private MSPeak GetPeakToTheLeftIfExists(IsotopicProfile isotopicProfile, IEnumerable<IPeak> peakList)
        {
            if (isotopicProfile == null) return null;
            MSPeak monoPeak = isotopicProfile.getMonoPeak();

            const double maxPossiblePeakWidth = 0.1;

            double mzTol = Math.Min(maxPossiblePeakWidth, monoPeak.Width);

            double targetMZ = monoPeak.XValue - (1.003 / (double)isotopicProfile.ChargeState);

            MSPeak peakToTheLeft = null;

            foreach (MSPeak peak in peakList)
            {
                if (Math.Abs(peak.XValue - targetMZ) < mzTol)
                {
                    peakToTheLeft = peak;
                    break;
                }

            }

            if (peakToTheLeft == null) return null;  // no peak found... so no problem.

            //peak to the left height must be greater than half the mono peak
            if (peakToTheLeft.Height > monoPeak.Height * 0.5)    //if peak-to-the-left exceeds min Ratio, then consider it
            {
                return peakToTheLeft;
            }
            return null;
        }




    }
}
