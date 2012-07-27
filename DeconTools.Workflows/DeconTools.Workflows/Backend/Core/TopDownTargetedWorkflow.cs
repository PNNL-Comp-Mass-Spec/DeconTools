using System;
using System.Collections.Generic;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Utilities;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.Core
{
	public class TopDownTargetedWorkflow : TargetedWorkflow
	{
		public Dictionary<int, TargetedResultBase> TargetResults { get; set; }

		private TargetedWorkflowParameters _workflowParameters;
		private JoshTheorFeatureGenerator _theorFeatureGen;
		private PeakChromatogramGenerator _chromGen;
		private DeconToolsSavitzkyGolaySmoother _chromSmoother;
		private ChromPeakDetector _chromPeakDetector;
		private ChromPeakSelectorBase _chromPeakSelector;
		private DeconToolsPeakDetector _msPeakDetector;
		private IterativeTFF _msfeatureFinder;
		private MassTagFitScoreCalculator _fitScoreCalc;
		private ResultValidatorTask _resultValidator;
		private ChromatogramCorrelatorTask _chromatogramCorrelatorTask;

		public TopDownTargetedWorkflow(Run run, TargetedWorkflowParameters parameters)
		{
			Run = run;
            _workflowParameters = parameters;

            InitializeWorkflow();
        }

		public TopDownTargetedWorkflow(TargetedWorkflowParameters parameters) : this(null, parameters)
		{

		}

		public override WorkflowParameters WorkflowParameters
		{
			get
			{
				return _workflowParameters;
			}
			set
			{
				_workflowParameters = value as TargetedWorkflowParameters;
			}
		}

		public override void InitializeWorkflow()
		{
			ValidateParameters();

			TargetResults = new Dictionary<int, TargetedResultBase>();

			_theorFeatureGen = new JoshTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.NONE, 0.005);

			_chromGen = new PeakChromatogramGenerator(_workflowParameters.ChromToleranceInPPM,
			                                          _workflowParameters.ChromGeneratorMode)
			            	{
			            		TopNPeaksLowerCutOff = 0.333,
			            		NETWindowWidthForAlignedData = (float) _workflowParameters.ChromNETTolerance*2
			            	};
			//only

			int pointsToSmooth = (_workflowParameters.ChromSmootherNumPointsInSmooth + 1) / 2;   // adding 0.5 prevents rounding problems
			_chromSmoother = new DeconToolsSavitzkyGolaySmoother(pointsToSmooth, pointsToSmooth, 2);
			_chromPeakDetector = new ChromPeakDetector(_workflowParameters.ChromPeakDetectorPeakBR, _workflowParameters.ChromPeakDetectorSigNoise);

			_chromPeakSelector = CreateChromPeakSelector(_workflowParameters);

			_msPeakDetector = new DeconToolsPeakDetector(_workflowParameters.MSPeakDetectorPeakBR, _workflowParameters.MSPeakDetectorSigNoise, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, false);

			var iterativeTffParameters = new IterativeTFFParameters {ToleranceInPPM = _workflowParameters.MSToleranceInPPM};
		    iterativeTffParameters.MinimumRelIntensityForForPeakInclusion = 0.4;


			_msfeatureFinder = new IterativeTFF(iterativeTffParameters);

			_fitScoreCalc = new MassTagFitScoreCalculator();

			_resultValidator = new ResultValidatorTask();

			_chromatogramCorrelatorTask = new ChromatogramCorrelatorTask
			                              	{ChromToleranceInPPM = _workflowParameters.ChromToleranceInPPM};

			ChromatogramXYData = new XYData();
			MassSpectrumXYData = new XYData();
			ChromPeaksDetected = new List<ChromPeak>();
		}

		private void ValidateParameters()
		{
			bool pointsInSmoothIsEvenNumber = (_workflowParameters.ChromSmootherNumPointsInSmooth % 2 == 0);
			if (pointsInSmoothIsEvenNumber)
			{
				throw new ArgumentOutOfRangeException("Points in chrom smoother is an even number, but must be an odd number.");
			}
		}

		public override void Execute()
		{
			Check.Require(Run != null, "Run has not been defined.");
			
			Run.ResultCollection.ResultType = DeconTools.Backend.Globals.ResultType.TOPDOWN_TARGETED_RESULT;

			ResetStoredData();

			try
			{
				Result = Run.ResultCollection.GetTargetedResult(Run.CurrentMassTag);
				Result.ResetResult();

				ExecuteTask(_theorFeatureGen);
				ExecuteTask(_chromGen);
				ExecuteTask(_chromSmoother);
				updateChromDataXYValues(Run.XYData);

				ExecuteTask(_chromPeakDetector);
				updateChromDetectedPeaks(Run.PeakList);

				ExecuteTask(_chromPeakSelector);
				ChromPeakSelected = Result.ChromPeakSelected;

				Result.ResetMassSpectrumRelatedInfo();

				ExecuteTask(MSGenerator);
				updateMassSpectrumXYValues(Run.XYData);

				ExecuteTask(_msfeatureFinder);

				ExecuteTask(_fitScoreCalc);
				ExecuteTask(_resultValidator);

				if (_workflowParameters.ChromatogramCorrelationIsPerformed)
				{
					ExecuteTask(_chromatogramCorrelatorTask);
				}

				// Save targeted result data
				Result.ChromValues = new XYData {Xvalues = ChromatogramXYData.Xvalues, Yvalues = ChromatogramXYData.Yvalues};
				TargetResults.Add(Run.CurrentMassTag.ID, Result);
			}
			catch (Exception ex)
			{
				var result = Run.ResultCollection.GetTargetedResult(Run.CurrentMassTag);
				result.ErrorDescription = ex.Message + "\n" + ex.StackTrace;
				result.FailedResult = true;
			}
		}
	}
}
