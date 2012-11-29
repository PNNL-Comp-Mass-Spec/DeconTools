using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.ZeroFillers;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using DeconTools.Utilities;

namespace DeconTools.Backend.Workflows
{
    public class SaturationIMSScanBasedWorkflow : IMSScanBasedWorkflow
    {

        List<IsosResult> _unsummedMSFeatures = new List<IsosResult>();
        private MSGenerator _msGenerator;
        private DeconToolsPeakDetector _peakDetector;
        private HornDeconvolutor _deconvolutor;
        TomIsotopicPattern _tomIsotopicPatternGenerator = new TomIsotopicPattern();
        private BasicTFF _basicFeatureFinder = new BasicTFF();
        private DeconToolsFitScoreCalculator _fitScoreCalculator = new DeconToolsFitScoreCalculator();

        private DeconToolsZeroFiller _zeroFiller = new DeconToolsZeroFiller();

        #region Constructors

        internal SaturationIMSScanBasedWorkflow(OldDecon2LSParameters parameters, Run run, string outputFolderPath = null, BackgroundWorker backgroundWorker = null)
            : base(parameters, run, outputFolderPath, backgroundWorker)
        {
            Check.Require(run is UIMFRun, "Cannot create workflow. Run is required to be a UIMFRun for this type of workflow");

            PeakBRSaturatedPeakDetector = parameters.PeakProcessorParameters.PeakBackgroundRatio * 0.75;

            _msGenerator = new UIMF_MSGenerator();
            _peakDetector = new DeconToolsPeakDetector(PeakBRSaturatedPeakDetector, 3, Globals.PeakFitType.QUADRATIC,
                                                       run.IsDataThresholded);



            _zeroFiller = new DeconToolsZeroFiller();


            _deconvolutor = new HornDeconvolutor(parameters.HornTransformParameters);
            _deconvolutor.MinPeptideBackgroundRatio = PeakBRSaturatedPeakDetector;
            _deconvolutor.MaxFitAllowed = 0.6;




            AdjustMonoIsotopicMasses = true;

            Run.PeakList = new List<Peak>();



        }


        #endregion




        public double PeakBRSaturatedPeakDetector { get; set; }

        /// <summary>
        /// Use non-saturated peaks to adjust the reported monoisotopic mass of the isotopic profile. Normally, the mono
        /// isotopic mass is based on the most abundant peak. But for saturated profiles, this is not a good thing. 
        /// </summary>
        public bool AdjustMonoIsotopicMasses { get; set; }

        protected override void IterateOverScans()
        {
            var uimfRun = (UIMFRun)Run;

            foreach (var lcScanSet in uimfRun.ScanSetCollection.ScanSetList)
            {
                uimfRun.ResultCollection.MSPeakResultList.Clear();
                _unsummedMSFeatures.Clear();


                ScanSet unsummedFrameSet = new ScanSet(lcScanSet.PrimaryScanNumber);
                //get saturated MSFeatures for unsummed data
                uimfRun.CurrentFrameSet = unsummedFrameSet;



                foreach (var scanset in uimfRun.IMSScanSetCollection.ScanSetList)
                {
                    uimfRun.ResultCollection.IsosResultBin.Clear();  //clear any previous MSFeatures

                    var unsummedIMSScanset = new IMSScanSet(scanset.PrimaryScanNumber);
                    uimfRun.CurrentIMSScanSet = unsummedIMSScanset;

                    _msGenerator.Execute(Run.ResultCollection);

                    _zeroFiller.Execute(Run.ResultCollection);

                    _peakDetector.Execute(Run.ResultCollection);

                    _deconvolutor.Deconvolute(uimfRun.ResultCollection);     //adds to IsosResultBin

                    //Note: the deconvolutor automatically increases the MSFeatureCounter. 
                    //Here, we don't want this, since this data is used only for saturation correction,
                    //not for generating the official MSFeatures list. So we need to 
                    //correct the MSFeatureCounter value. 
                    Run.ResultCollection.MSFeatureCounter = Run.ResultCollection.MSFeatureCounter -
                                                            Run.ResultCollection.IsosResultBin.Count;

                    _unsummedMSFeatures.AddRange(Run.ResultCollection.IsosResultBin);


                    //iterate over unsummed MSFeatures and check for saturation
                    foreach (var isosResult in uimfRun.ResultCollection.IsosResultBin)
                    {

                        bool isPossiblySaturated = isosResult.IsotopicProfile.IntensityAggregate >
                                                   OldDecon2LsParameters.HornTransformParameters.SaturationThreshold;


                        //UIMFIsosResult tempIsosResult = (UIMFIsosResult) isosResult;

                        //if (tempIsosResult.FrameSet.PrimaryFrame==202 && tempIsosResult.ScanSet.PrimaryScanNumber==108)
                        //{
                        //    Console.WriteLine(tempIsosResult + "\t being processed!");
                        //}


                        if (isPossiblySaturated)
                        {
                            var theorIso = new IsotopicProfile();

                            RebuildSaturatedIsotopicProfile(isosResult, uimfRun.PeakList, out theorIso);
                            AdjustSaturatedIsotopicProfile(isosResult.IsotopicProfile, theorIso, AdjustMonoIsotopicMasses, true);
                        }
                        

                    }
                }


                //DisplayMSFeatures(_unsummedMSFeatures);

                //now sum across IMSScans, deconvolute and adjust
                foreach (IMSScanSet scanset in uimfRun.IMSScanSetCollection.ScanSetList)
                {
                    uimfRun.ResultCollection.IsosResultBin.Clear();  //clear any previous MSFeatures


                    //get the summed isotopic profile
                    uimfRun.CurrentFrameSet = lcScanSet;
                    uimfRun.CurrentIMSScanSet = scanset;




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

                    GatherPeakStatistics();

                    ExecuteTask(Deconvolutor);


                    foreach (var isosResult in Run.ResultCollection.IsosResultBin)
                    {

                        bool isPossiblySaturated = isosResult.IsotopicProfile.IntensityAggregate >
                                                      OldDecon2LsParameters.HornTransformParameters.SaturationThreshold;

                       

                        if (isPossiblySaturated)
                        {
                            var theorIso = new IsotopicProfile();

                            RebuildSaturatedIsotopicProfile(isosResult, Run.PeakList, out theorIso);
                            AdjustSaturatedIsotopicProfile(isosResult.IsotopicProfile, theorIso, AdjustMonoIsotopicMasses, false);
                            UpdateReportedSummedPeakIntensities(isosResult, lcScanSet, scanset);
                        }
                        else
                        {

                        }
                        
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
                        double ppmToleranceForDuplicate = 20;
                        double massTolForDuplicate = ppmToleranceForDuplicate*
                                                     isosResult.IsotopicProfile.MonoIsotopicMass/1e6;

                        

                        var duplicateIsosResults = (from n in Run.ResultCollection.IsosResultBin
                                                    where
                                                        Math.Abs(n.IsotopicProfile.MonoIsotopicMass -
                                                                 isosResult.IsotopicProfile.MonoIsotopicMass) <
                                                        massTolForDuplicate && n.IsotopicProfile.ChargeState== isosResult.IsotopicProfile.ChargeState
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
                                //here we have found a duplicate
                                dup.MSFeatureID = minMSFeatureID;

                                //because there are duplicates, we need to maintain the MSFeatureCounter so it doesn't skip values, as will 
                                //happen when there are duplicates
                                //Run.ResultCollection.MSFeatureCounter--;
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

        //private void DisplayMSFeatures(List<IsosResult> msfeatures)
        //{
        //    var sb = new StringBuilder();
        //    foreach (UIMFIsosResult uimfIsosResult in msfeatures)
        //    {
        //        sb.Append(uimfIsosResult.FrameSet.PrimaryFrame + "\t" + uimfIsosResult.ScanSet.PrimaryScanNumber + "\t" +
        //                  uimfIsosResult.IsotopicProfile.MonoIsotopicMass.ToString("0.000") + "\t" +
        //                  uimfIsosResult.IsotopicProfile.MonoPeakMZ.ToString("0.000") + "\t" +
        //                  uimfIsosResult.IsotopicProfile.IntensityAggregate + "\n");

        //    }
        //    Console.WriteLine(sb.ToString());
        //}

        private void GetRebuiltFitScore(IsosResult isosResult)
        {

            isosResult.IsotopicProfile.Score = 0.0099;  //hard-code the fit score. We can't re-fit since the raw xy data wasn't changed
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
                sb.Append(msPeakResult.FrameNum + "\t" + msPeakResult.Scan_num + "\t" +
                          msPeakResult.MSPeak.XValue.ToString("0.0000") + "\t" + msPeakResult.MSPeak.Height + "\n");


            }

            Console.WriteLine(sb.ToString());

        }

        protected override void ExecuteOtherTasksHook()
        {
            ExecuteTask(UimfDriftTimeExtractor);
            ExecuteTask(UimfTicExtractor);

        }


        private void UpdateReportedSummedPeakIntensities(IsosResult profile, ScanSet lcScanSet, ScanSet imsScanSet)
        {

            UIMFIsosResult uimfIsosResult = (UIMFIsosResult)profile;

            int minFrame = lcScanSet.getLowestScanNumber();
            int maxFrame = lcScanSet.getHighestScanNumber();

            int minIMSScan = imsScanSet.getLowestScanNumber();
            int maxIMSScan = imsScanSet.getHighestScanNumber();

            double massTolerance = 0.2;

            var filteredUnsummedMSFeatures = (from n in _unsummedMSFeatures
                                              where n.ScanSet.PrimaryScanNumber >= minFrame &&
                                                    n.ScanSet.PrimaryScanNumber <= maxFrame &&
                                                    ((UIMFIsosResult)n).IMSScanSet.PrimaryScanNumber >= minIMSScan &&
                                                    ((UIMFIsosResult)n).IMSScanSet.PrimaryScanNumber <= maxIMSScan &&
                                                    n.IsotopicProfile.ChargeState == profile.IsotopicProfile.ChargeState &&
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
                                                 n.ScanSet.PrimaryScanNumber == lcScanSet.PrimaryScanNumber &&
                                                 ((UIMFIsosResult)n).IMSScanSet.PrimaryScanNumber == imsScanSet.PrimaryScanNumber
                                             select n).FirstOrDefault();



            profile.IsotopicProfile.OriginalIntensity = unsummedAdjustedMSFeature == null
                                                            ? 0
                                                            : unsummedAdjustedMSFeature.IsotopicProfile.
                                                                  IntensityAggregate;



            var adjustedIntensity = filteredUnsummedMSFeatures.Sum(p => p.IsotopicProfile.IntensityAggregate);

            //TODO: remove this debug code later
            //if (unsummedAdjustedMSFeature == null)
            //{
            //    Console.WriteLine(profile.IsotopicProfile.MonoPeakMZ.ToString("0.000") + "\t" + profile.IsotopicProfile.ChargeState + "\t" + frameSet.PrimaryFrame + "\t" + scanSet.PrimaryScanNumber + "\t" + profile.IsotopicProfile.IntensityAggregate + "\t" + adjustedIntensity + "\t" +  "0");
            //}
            //else
            //{
            //    Console.WriteLine(profile.IsotopicProfile.MonoPeakMZ.ToString("0.000") + "\t" + profile.IsotopicProfile.ChargeState + "\t" + frameSet.PrimaryFrame + "\t" + scanSet.PrimaryScanNumber + "\t" + profile.IsotopicProfile.IntensityAggregate + "\t" + adjustedIntensity +"\t" + unsummedAdjustedMSFeature.IsotopicProfile.IntensityAggregate);
            //}

            if (adjustedIntensity > profile.IsotopicProfile.IntensityAggregate)
            {
                profile.IsotopicProfile.IsSaturated = true;
                profile.IsotopicProfile.IntensityAggregate = adjustedIntensity;

                if (filteredUnsummedMSFeatures.Count > 0)
                {
                    profile.IsotopicProfile.MonoIsotopicMass = averageMonoMass;
                    profile.IsotopicProfile.MonoPeakMZ = averageMonoMZ;
                    profile.IsotopicProfile.MostAbundantIsotopeMass = averageMostAbundantPeakMonoMass;
                }
            }

        }


        public void AdjustSaturatedIsotopicProfile(IsotopicProfile iso, IsotopicProfile theorIsotopicProfile, bool updatePeakMasses = true, bool updatePeakIntensities = true)
        {
            
            //get index of suitable peak on which to base the intensity adjustment
            int indexOfPeakUsedInExtrapolation = 0;
            

            for (int i = 0; i < iso.Peaklist.Count; i++)
            {
                var currentPeak = iso.Peaklist[i];

                //double idealRatioMin = 0.2;
                //double idealRatioMax = 0.8;

                //double peakRatio = currentPeak.Height / mostAbundantPeak.Height;

                if (currentPeak.Height < OldDecon2LsParameters.HornTransformParameters.SaturationThreshold)
                {
                    indexOfPeakUsedInExtrapolation = i;
                    break;
                }

                //if none are below the saturation threshold, use the last peak
                indexOfPeakUsedInExtrapolation = iso.Peaklist.Count - 1;

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
                if (iso.Peaklist[i].Height > OldDecon2LsParameters.HornTransformParameters.SaturationThreshold)
                {
                    if (updatePeakIntensities)
                    {
                        
                        if (i>=theorIsotopicProfile.Peaklist.Count)
                        {
                            iso.Peaklist[i].Height = 0;

                        }
                        else
                        {
                            iso.Peaklist[i].Height = theorIsotopicProfile.Peaklist[i].Height *
                                                 intensityObsPeakForExtrapolation /
                                                 intensityTheorPeakForExtrapolation;
                        }
                        

                        iso.Peaklist[i].Width = iso.Peaklist[indexOfPeakUsedInExtrapolation].Width;    //repair the width too, because it can get huge. Width can be used in determining tolerances.

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
            sb.Append(uimfFeature.MSFeatureID + "\t" + uimfFeature.ScanSet.PrimaryScanNumber + "\t" + uimfFeature.IMSScanSet.PrimaryScanNumber + "\t" +
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

        private void RebuildSaturatedIsotopicProfile(IsosResult saturatedFeature, List<Peak> peakList, out IsotopicProfile theorIso)
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
            iso.MonoIsotopicMass = (iso.getMonoPeak().XValue - Globals.PROTON_MASS) * iso.ChargeState;
            iso.MonoPeakMZ = iso.getMonoPeak().XValue;
            iso.MostAbundantIsotopeMass = (iso.getMostIntensePeak().XValue - Globals.PROTON_MASS) * iso.ChargeState;
        }


        private void AssignMissingPeaksToSaturatedProfile(List<Peak> peakList, IsotopicProfile isotopicProfile, IsotopicProfile theorIsotopicProfile)
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

            //NOTE: This is critical to choosing the optimum peak of the observed isotopic profile
            //A value of 0.001 will leave more peaks in the theor profile. This
            //can be bad with co-eluting peptides, so that a peak of the interfering peptide
            //is used to correct the intensity of our target peptide. 
            //A value of 0.01 helps prevent this (by trimming the peaks of the theor profile,
            //and reducing the peaks to be considered for peak intensity extrapolation of the target peptide. 
            PeakUtilities.TrimIsotopicProfile(theorTarget.IsotopicProfile, 0.01);    

            return theorTarget.IsotopicProfile;
        }

        private MSPeak GetPeakToTheLeftIfExists(IsotopicProfile isotopicProfile, IEnumerable<Peak> peakList)
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
