using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.Utilities;
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

		#region Constructors

		public ChromPeakAnalyzerIqWorkflow(Run run, TargetedWorkflowParameters parameters) : base(run, parameters)
		{
			IterativeTFFParameters iterativeTffParameters = new IterativeTFFParameters();
			TargetedMSFeatureFinder = new IterativeTFF(iterativeTffParameters);
            PeakFitter = new PeakLeastSquaresFitter();
		}

		public ChromPeakAnalyzerIqWorkflow(TargetedWorkflowParameters parameters) : base(parameters)
		{
			IterativeTFFParameters iterativeTffParameters = new IterativeTFFParameters();
			TargetedMSFeatureFinder = new IterativeTFF(iterativeTffParameters);
            PeakFitter = new PeakLeastSquaresFitter();
		}

		#endregion

		#region Properties

		protected IterativeTFF TargetedMSFeatureFinder;

	    protected PeakLeastSquaresFitter PeakFitter;

		private ChromPeakUtilities _chromPeakUtilities = new ChromPeakUtilities();

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

			var target = result.Target as ChromPeakIqTarget;
			if (target == null)
			{
				throw new NullReferenceException("The ChromPeakAnalyzerIqWorkflow only works with the ChromPeakIqTarget."
					+ "Due to an inherent shortcomming of the design pattern we used, we were unable to make this a compile time error instead of a runtime error."
					+ "Please change the IqTarget to ChromPeakIqTarget for proper use of the ChromPeakAnalyzerIqWorkflow.");
			}

			//Sums Scan

            //TODO: numMSSummed is currently hardcoded to '1'; Need to use a Parameter for this
			var lcscanset =_chromPeakUtilities.GetLCScanSetForChromPeak(target.ChromPeak, Run, 1);

			//Generate a mass spectrum
			var massSpectrumXYData = MSGenerator.GenerateMS(Run, lcscanset);

			//Find isotopic profile
			List<Peak> mspeakList;
			var observedIso = TargetedMSFeatureFinder.IterativelyFindMSFeature(massSpectrumXYData, target.TheorIsotopicProfile, out mspeakList);

            LeftOfMonoPeakLooker leftOfMonoPeakLooker = new LeftOfMonoPeakLooker();
            var peakToTheLeft = leftOfMonoPeakLooker.LookforPeakToTheLeftOfMonoPeak(target.TheorIsotopicProfile.getMonoPeak(), target.ChargeState, mspeakList);

            bool hasPeakTotheLeft = peakToTheLeft != null;

			if (observedIso == null)
			{
				result.IsotopicProfileFound = false;
			    result.FitScore = 1;
				result.InterferenceScore = 1;
			}
			else
			{

				//Get NET Error
				double NETError = Math.Abs(target.ChromPeak.NETValue - target.ElutionTimeTheor);

				//Get PPM Error
				double MassError = TheorMostIntensePeakMassError(target.TheorIsotopicProfile, observedIso, target.ChargeState);

                //Get fit score
			    List<Peak> observedIsoList = observedIso.Peaklist.Cast<Peak>().ToList();
                double fitScore = PeakFitter.GetFit(target.TheorIsotopicProfile.Peaklist.Select(p=>(Peak)p).ToList(), observedIsoList, 0.05, WorkflowParameters.MSToleranceInPPM);

				//get i_score
				double iscore = InterferenceScorer.GetInterferenceScore(target.TheorIsotopicProfile, mspeakList);

                //Get Isotope Correlation
                int scan = lcscanset.PrimaryScanNumber;
                double chromScanWindowWidth = target.ChromPeak.Width * 2;

				//Determines where to start and stop chromatogram correlation
                int startScan = scan - (int)Math.Round(chromScanWindowWidth / 2, 0);
                int stopScan = scan + (int)Math.Round(chromScanWindowWidth / 2, 0);

                result.CorrelationData = ChromatogramCorrelator.CorrelateData(Run, observedIso, startScan, stopScan);

				result.LCScanSetSelected = new ScanSet(lcscanset.PrimaryScanNumber);
				result.IsotopicProfileFound = true;
				result.FitScore = fitScore;
				result.InterferenceScore = iscore;
				result.ObservedIsotopicProfile = observedIso;
				result.IsIsotopicProfileFlagged = hasPeakTotheLeft;
				result.NETError = NETError;
				result.MassError = MassError;
				result.IqResultDetail.MassSpectrum = massSpectrumXYData;
				result.Abundance = GetAbundance(result);
			}

			Display(result);

        }

		//Writes IqResult Data to Console
		private void Display(IqResult result)
		{
			ChromPeakIqTarget target = result.Target as ChromPeakIqTarget;
			if (target == null)
			{
				throw new NullReferenceException("The ChromPeakAnalyzerIqWorkflow only works with the ChromPeakIqTarget."
					+ "Due to an inherent shortcomming of the design pattern we used, we were unable to make this a compile time error instead of a runtime error."
					+ "Please change the IqTarget to ChromPeakIqTarget for proper use of the ChromPeakAnalyzerIqWorkflow.");
			}

			
			IqLogger.Log.Debug(("\t\t"+ target.ChromPeak.XValue.ToString("0.00") + "\t" + result.NETError.ToString("0.0000") + "\t" + result.MassError.ToString("0.0000") + "\t" + 
				result.FitScore.ToString("0.0000") + "\t" + result.IsIsotopicProfileFlagged));		
		}



	}
}
