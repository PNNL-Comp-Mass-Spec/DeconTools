using System;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.Utilities.IqLogger;
using DeconTools.Workflows.Backend.Core.ChromPeakSelection;


namespace DeconTools.Workflows.Backend.Core
{
    /// <summary>
    /// ChromPeakAnalyzerIqWorkflow calculates metrics based on a single peak passed in VIA ChromPeakIqTarget.
    /// MUST BE USED WITH ChromPeakIqTarget
    /// </summary>
    public class ChromPeakAnalyzerIqWorkflow : BasicIqWorkflow
    {
        protected bool _headerLogged;

        #region Constructors

        public ChromPeakAnalyzerIqWorkflow(Run run, TargetedWorkflowParameters parameters) : base(run, parameters)
        {
            var iterativeTffParameters = new IterativeTFFParameters();
            TargetedMSFeatureFinder = new IterativeTFF(iterativeTffParameters);
            PeakFitter = new PeakLeastSquaresFitter();
        }

        public ChromPeakAnalyzerIqWorkflow(TargetedWorkflowParameters parameters) : base(parameters)
        {
            var iterativeTffParameters = new IterativeTFFParameters();
            TargetedMSFeatureFinder = new IterativeTFF(iterativeTffParameters);
            PeakFitter = new PeakLeastSquaresFitter();
        }

        #endregion

        #region Properties

        protected IterativeTFF TargetedMSFeatureFinder;

        protected PeakLeastSquaresFitter PeakFitter;

        protected ChromPeakUtilities _chromPeakUtilities = new ChromPeakUtilities();

        #endregion

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

            MSGenerator.MinMZ = target.MZTheor - 2;
            MSGenerator.MaxMZ = target.MZTheor + 5;

            //Sums Scan

            var lcScanSet =_chromPeakUtilities.GetLCScanSetForChromPeak(target.ChromPeak, Run, WorkflowParameters.NumMSScansToSum);

            //Generate a mass spectrum
            var massSpectrumXYData = MSGenerator.GenerateMS(Run, lcScanSet);

            //massSpectrumXYData = massSpectrumXYData.TrimData(result.Target.MZTheor - 5, result.Target.MZTheor + 15);

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
                //Get fit score
                var observedIsoList = result.ObservedIsotopicProfile.Peaklist.Cast<Peak>().ToList();
                var minIntensityForScore = 0.05;
                var fitScore = PeakFitter.GetFit(target.TheorIsotopicProfile.Peaklist.Select(p => (Peak)p).ToList(), observedIsoList, minIntensityForScore, WorkflowParameters.MSToleranceInPPM);

                //get i_score
                var iScore = InterferenceScorer.GetInterferenceScore(result.ObservedIsotopicProfile, msPeakList);

                //get ppm error
                var massErrorInDaltons = TheorMostIntensePeakMassError(target.TheorIsotopicProfile, result.ObservedIsotopicProfile, target.ChargeState);
                var ppmError = (massErrorInDaltons/target.MonoMassTheor)*1e6;

                //Get Isotope Correlation
                var scan = lcScanSet.PrimaryScanNumber;
                double chromScanWindowWidth = target.ChromPeak.Width * 2;

                //Determines where to start and stop chromatogram correlation
                var startScan = scan - (int)Math.Round(chromScanWindowWidth / 2, 0);
                var stopScan = scan + (int)Math.Round(chromScanWindowWidth / 2, 0);

                result.CorrelationData = ChromatogramCorrelator.CorrelateData(Run, result, startScan, stopScan);
                result.LcScanObs = lcScanSet.PrimaryScanNumber;
                result.ChromPeakSelected = target.ChromPeak;
                result.LCScanSetSelected = new ScanSet(lcScanSet.PrimaryScanNumber);
                result.IsotopicProfileFound = true;
                result.FitScore = fitScore;
                result.InterferenceScore = iScore;
                result.IsIsotopicProfileFlagged = hasPeakToTheLeft;
                result.NETError = netError;
                result.MassErrorBefore = ppmError;
                result.IqResultDetail.MassSpectrum = massSpectrumXYData;
                result.Abundance = GetAbundance(result);
            }

            Display(result);

        }

        //Writes IqResult Data to Console
        private void Display(IqResult result)
        {
            if (!(result.Target is ChromPeakIqTarget target))
            {
                throw new NullReferenceException("The ChromPeakAnalyzerIqWorkflow only works with the ChromPeakIqTarget. "
                    + "Due to an inherent shortcoming of the design pattern we used, we were unable to make this a compile time error instead of a runtime error. "
                    + "Please change the IqTarget to ChromPeakIqTarget for proper use of the ChromPeakAnalyzerIqWorkflow.");
            }

            if (!_headerLogged)
            {
                _headerLogged = true;
                IqLogger.LogDebug(("\t\t" + "ChromPeak.XValue" + "\t" + "NETError" + "\t" + "MassError" + "\t" + "FitScore" + "\t" + "IsIsotopicProfileFlagged"));
            }

            IqLogger.LogDebug(("\t\t"+ target.ChromPeak.XValue.ToString("0.00") + "\t" + result.NETError.ToString("0.0000") + "\t" + result.MassErrorBefore.ToString("0.0000") + "\t" +
                result.FitScore.ToString("0.0000") + "\t" + result.IsIsotopicProfileFlagged));
        }

    }
}
