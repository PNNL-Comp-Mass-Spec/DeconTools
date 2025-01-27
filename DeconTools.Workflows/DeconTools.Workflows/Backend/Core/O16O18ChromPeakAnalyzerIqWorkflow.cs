﻿using System;
using System.Linq;
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
            var minRelIntensityForCorr = 0.025;
            ChromatogramCorrelator = new O16O18ChromCorrelator(WorkflowParameters.ChromSmootherNumPointsInSmooth, minRelIntensityForCorr,
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

            if (!(result.Target is ChromPeakIqTarget target))
            {
                throw new NullReferenceException("The ChromPeakAnalyzerIqWorkflow only works with the ChromPeakIqTarget.");
            }

            var lcScanSet = _chromPeakUtilities.GetLCScanSetForChromPeak(target.ChromPeak, Run, WorkflowParameters.SmartChromPeakSelectorNumMSSummed);

            //Generate a mass spectrum
            var massSpectrumXYData = MSGenerator.GenerateMS(Run, lcScanSet);

            massSpectrumXYData = massSpectrumXYData.TrimData(result.Target.MZTheor - 5, result.Target.MZTheor + 15);

            //Find isotopic profile
            result.ObservedIsotopicProfile = TargetedMSFeatureFinder.IterativelyFindMSFeature(massSpectrumXYData, target.TheorIsotopicProfile, out var msPeakList);

            //Get NET Error
            var netError = target.ChromPeak.NETValue - target.ElutionTimeTheor;

            var leftOfMonoPeakLooker = new LeftOfMonoPeakLooker();
            var peakToTheLeft = leftOfMonoPeakLooker.LookforPeakToTheLeftOfMonoPeak(target.TheorIsotopicProfile.getMonoPeak(), target.ChargeState, msPeakList);

            var hasPeakToTheLeft = peakToTheLeft != null;

            if (result.ObservedIsotopicProfile == null)
            {
                result.IsotopicProfileFound = false;
                result.FitScore = 1;
            }
            else
            {
                //Get fit score O16 profile
                var observedIsoList = result.ObservedIsotopicProfile.Peaklist.Cast<Peak>().Take(4).ToList();    //first 4 peaks excludes the O18 double label peak (fifth peak)
                var theoreticalPeakList = target.TheorIsotopicProfile.Peaklist.Cast<Peak>().Take(4).ToList();
                result.FitScore = PeakFitter.GetFit(theoreticalPeakList, observedIsoList, 0.05, WorkflowParameters.MSToleranceInPPM);

                // fit score O18 profile
                var o18Iso = ((O16O18IqResult)result).ConvertO16ProfileToO18(target.TheorIsotopicProfile, 4);
                theoreticalPeakList = o18Iso.Peaklist.Cast<Peak>().ToList();
                observedIsoList = result.ObservedIsotopicProfile.Peaklist.Cast<Peak>().Skip(4).ToList();    //skips the first 4 peaks and thus includes the O18 double label isotopic profile
                ((O16O18IqResult) result).FitScoreO18Profile = PeakFitter.GetFit(theoreticalPeakList, observedIsoList, 0.05, WorkflowParameters.MSToleranceInPPM);

                //get i_score (1 is the worst possible score)
                var iScore = InterferenceScorer.GetInterferenceScore(result.ObservedIsotopicProfile, msPeakList);

                //get ppm error
                var massErrorInDaltons = TheorMostIntensePeakMassError(target.TheorIsotopicProfile, result.ObservedIsotopicProfile, target.ChargeState);
                var ppmError = (massErrorInDaltons / target.MonoMassTheor) * 1e6;

                //Get Isotope Correlation
                var scan = lcScanSet.PrimaryScanNumber;

                var sigma = target.ChromPeak.Width/2.35;
                var chromScanWindowWidth = 4 * sigma;

                //Determines where to start and stop chromatogram correlation
                var startScan = scan - (int)Math.Round(chromScanWindowWidth / 2, 0);
                var stopScan = scan + (int)Math.Round(chromScanWindowWidth / 2, 0);

                result.CorrelationData = ChromatogramCorrelator.CorrelateData(Run, result, startScan, stopScan);
                result.LcScanObs = lcScanSet.PrimaryScanNumber;
                result.ChromPeakSelected = target.ChromPeak;
                result.LCScanSetSelected = new ScanSet(lcScanSet.PrimaryScanNumber);
                result.IsotopicProfileFound = true;
                result.InterferenceScore = iScore;
                result.IsIsotopicProfileFlagged = hasPeakToTheLeft;
                result.NETError = netError;
                result.MassErrorBefore = ppmError;
                result.IqResultDetail.MassSpectrum = massSpectrumXYData;
                result.Abundance = GetAbundance(result);
            }
        }

        protected internal override IqResult CreateIQResult(IqTarget target)
        {
            return new O16O18IqResult(target);
        }
    }
}
