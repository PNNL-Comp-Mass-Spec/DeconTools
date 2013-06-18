using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Workflows.Backend.Core.ChromPeakSelection;


namespace DeconTools.Workflows.Backend.Core
{
    public class O16O18ChromPeakAnalyzerIqWorkflow:ChromPeakAnalyzerIqWorkflow
    {
        public O16O18ChromPeakAnalyzerIqWorkflow(Run run, TargetedWorkflowParameters parameters) : base(run, parameters)
        {
          




        }

        public O16O18ChromPeakAnalyzerIqWorkflow(TargetedWorkflowParameters parameters) : this(null,parameters)
        {
        }

        protected override void DoPostInitialization()
        {
            base.DoPostInitialization();
            double minRelIntensityForCorr = 0.025;
            this.ChromatogramCorrelator = new O16O18ChromCorrelator(WorkflowParameters.ChromSmootherNumPointsInSmooth, minRelIntensityForCorr,
                                                                    WorkflowParameters.ChromGenTolerance,
                                                                    WorkflowParameters.ChromGenToleranceUnit);
        }


        /// <summary>
        /// Calculates Metrics based on ChromPeakIqTarget
        /// NET Error, Mass Error, Isotopic Fit, & Isotope Correlation
        /// </summary>
        protected override void ExecuteWorkflow(IqResult result)
        {
            result.IsExported = false;

            if (MSGenerator == null)
            {
                MSGenerator = MSGeneratorFactory.CreateMSGenerator(Run.MSFileType);
                MSGenerator.IsTICRequested = false;
            }

            var target = result.Target as ChromPeakIqTarget;
            if (target == null)
            {
                throw new NullReferenceException("The ChromPeakAnalyzerIqWorkflow only works with the ChromPeakIqTarget.");
            }

            
            var lcscanset = _chromPeakUtilities.GetLCScanSetForChromPeak(target.ChromPeak, Run, WorkflowParameters.SmartChromPeakSelectorNumMSSummed);

            //Generate a mass spectrum
            var massSpectrumXYData = MSGenerator.GenerateMS(Run, lcscanset);

            massSpectrumXYData = massSpectrumXYData.TrimData(result.Target.MZTheor - 5, result.Target.MZTheor + 15);

            //Find isotopic profile
            List<Peak> mspeakList;
            result.ObservedIsotopicProfile = TargetedMSFeatureFinder.IterativelyFindMSFeature(massSpectrumXYData, target.TheorIsotopicProfile, out mspeakList);

            //Default Worst Scores
            double fitScore = 1;
            double iscore = 1;

            //Get NET Error
            double netError = target.ChromPeak.NETValue - target.ElutionTimeTheor;


            LeftOfMonoPeakLooker leftOfMonoPeakLooker = new LeftOfMonoPeakLooker();
            var peakToTheLeft = leftOfMonoPeakLooker.LookforPeakToTheLeftOfMonoPeak(target.TheorIsotopicProfile.getMonoPeak(), target.ChargeState, mspeakList);

            bool hasPeakTotheLeft = peakToTheLeft != null;

            if (result.ObservedIsotopicProfile == null)
            {
                result.IsotopicProfileFound = false;
                result.FitScore = 1;
            }
            else
            {
                //Get fit score
                List<Peak> observedIsoList = result.ObservedIsotopicProfile.Peaklist.Cast<Peak>().ToList();
                fitScore = PeakFitter.GetFit(target.TheorIsotopicProfile.Peaklist.Select(p => (Peak)p).ToList(), observedIsoList, 0.05, WorkflowParameters.MSToleranceInPPM);

                //get i_score
                iscore = InterferenceScorer.GetInterferenceScore(result.ObservedIsotopicProfile, mspeakList);

                //get ppm error
                double massErrorInDaltons = TheorMostIntensePeakMassError(target.TheorIsotopicProfile, result.ObservedIsotopicProfile, target.ChargeState);
                double ppmError = (massErrorInDaltons / target.MonoMassTheor) * 1e6;

                //Get Isotope Correlation
                int scan = lcscanset.PrimaryScanNumber;

                double sigma = target.ChromPeak.Width/2.35;
                double chromScanWindowWidth = 4 * sigma;

                //Determines where to start and stop chromatogram correlation
                int startScan = scan - (int)Math.Round(chromScanWindowWidth / 2, 0);
                int stopScan = scan + (int)Math.Round(chromScanWindowWidth / 2, 0);

                result.CorrelationData = ChromatogramCorrelator.CorrelateData(Run, result, startScan, stopScan);
                result.LcScanObs = lcscanset.PrimaryScanNumber;
                result.ChromPeakSelected = target.ChromPeak;
                result.LCScanSetSelected = new ScanSet(lcscanset.PrimaryScanNumber);
                result.IsotopicProfileFound = true;
                result.FitScore = fitScore;
                result.InterferenceScore = iscore;
                result.IsIsotopicProfileFlagged = hasPeakTotheLeft;
                result.NETError = netError;
                result.MassErrorBefore = ppmError;
                result.IqResultDetail.MassSpectrum = massSpectrumXYData;
                result.Abundance = GetAbundance(result);
            }

            

        }




    }
}
