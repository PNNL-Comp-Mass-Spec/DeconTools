﻿using System;
using System.Collections.Generic;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.Quantifiers;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;

namespace DeconTools.Workflows.Backend.Core
{
    public class O16O18Workflow : TargetedWorkflow
    {
        private JoshTheorFeatureGenerator theorFeatureGen;
        private PeakChromatogramGenerator chromGen;
        private DeconToolsSavitzkyGolaySmoother chromSmoother;
        private ChromPeakDetector chromPeakDetector;

        private ChromPeakSelectorBase chromPeakSelector;
        
        //private SmartChromPeakSelector chromPeakSelector;


        private DeconToolsPeakDetector msPeakDetector;
        private O16O18TargetedIterativeFeatureFinder o16o18FeatureFinder;
        private MassTagFitScoreCalculator fitScoreCalc;
        private O16O18QuantifierTask quant;

        #region Constructors

        public O16O18Workflow(Run run, TargetedWorkflowParameters parameters)
        {
            this.WorkflowParameters = parameters;

            this.Run = run;
            InitializeWorkflow();
        }

        public O16O18Workflow(TargetedWorkflowParameters parameters)
            : this(null, parameters)
        {

        }

        #endregion

        #region Properties

        #endregion

        #region IWorkflow Members

        public override void Execute()
        {

            ResetStoredData();

            this.Run.ResultCollection.ResultType = DeconTools.Backend.Globals.ResultType.O16O18_TARGETED_RESULT;



            try
            {
                Result = Run.ResultCollection.GetTargetedResult(Run.CurrentMassTag);
                Result.ResetResult();


                ExecuteTask(theorFeatureGen);
                ExecuteTask(chromGen);
                ExecuteTask(chromSmoother);
                updateChromDataXYValues(Run.XYData);

                ExecuteTask(chromPeakDetector);
                updateChromDetectedPeaks(Run.PeakList);

                ExecuteTask(chromPeakSelector);
                ChromPeakSelected = Result.ChromPeakSelected;


                Result.ResetMassSpectrumRelatedInfo();


                ExecuteTask(MSGenerator);
                updateMassSpectrumXYValues(Run.XYData);

                ExecuteTask(o16o18FeatureFinder);
                ExecuteTask(fitScoreCalc);
                ExecuteTask(resultValidator);

                ExecuteTask(quant);


            }
            catch (Exception ex)
            {
                TargetedResultBase result = this.Run.ResultCollection.GetTargetedResult(this.Run.CurrentMassTag);
                result.ErrorDescription = ex.Message + "\n" + ex.StackTrace;

                return;
            }

        }


        public override void InitializeWorkflow()
        {

            ValidateParameters();

            theorFeatureGen = new JoshTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.NONE, 0.002);

            chromGen = new PeakChromatogramGenerator(_workflowParameters.ChromToleranceInPPM, _workflowParameters.ChromGeneratorMode);
            chromGen.TopNPeaksLowerCutOff = 0.333;

            int pointsToSmooth = (_workflowParameters.ChromSmootherNumPointsInSmooth + 1) / 2;   // adding 0.5 prevents rounding problems
            chromSmoother = new DeconToolsSavitzkyGolaySmoother(pointsToSmooth, pointsToSmooth, 2);
            chromPeakDetector = new ChromPeakDetector(_workflowParameters.ChromPeakDetectorPeakBR, _workflowParameters.ChromPeakDetectorSigNoise);


            chromPeakSelector = CreateChromPeakSelector(_workflowParameters);


            msPeakDetector = new DeconToolsPeakDetector(_workflowParameters.MSPeakDetectorPeakBR, _workflowParameters.MSPeakDetectorSigNoise, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, false);

            IterativeTFFParameters iterativeTFFParameters = new IterativeTFFParameters();
            iterativeTFFParameters.ToleranceInPPM = _workflowParameters.MSToleranceInPPM;

            o16o18FeatureFinder = new O16O18TargetedIterativeFeatureFinder(iterativeTFFParameters);

            quant = new O16O18QuantifierTask();
            fitScoreCalc = new MassTagFitScoreCalculator();
            resultValidator = new ResultValidatorTask();

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

            //add parameter validation

        }

        string _name;
        public string Name
        {
            get
            { return this.ToString(); }
            set
            {
                _name = value;
            }

        }

        #endregion
        private ResultValidatorTask resultValidator;

        TargetedWorkflowParameters _workflowParameters;
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
    }
}
