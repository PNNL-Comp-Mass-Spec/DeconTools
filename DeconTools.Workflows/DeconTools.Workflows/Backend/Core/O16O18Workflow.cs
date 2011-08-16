﻿using System;
using System.Collections.Generic;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.Quantifiers;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;

namespace DeconTools.Workflows.Backend.Core
{
    public class O16O18Workflow : TargetedWorkflow
    {
        private TomTheorFeatureGenerator theorFeatureGen;
        private PeakChromatogramGenerator chromGen;
        private DeconToolsSavitzkyGolaySmoother chromSmoother;
        private ChromPeakDetector chromPeakDetector;
        private SmartO16O18ChromPeakSelector chromPeakSelector;


        private DeconToolsPeakDetector msPeakDetector;
        private O16O18TargetedIterativeFeatureFinder o16o18FeatureFinder;
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

            try
            {

                this.Result = this.Run.ResultCollection.GetMassTagResult(this.Run.CurrentMassTag);


                ExecuteTask(theorFeatureGen);
                ExecuteTask(chromGen);
                ExecuteTask(chromSmoother);
                updateChromDataXYValues(this.Run.XYData);

                ExecuteTask(chromPeakDetector);
                updateChromDetectedPeaks(this.Run.PeakList);

                ExecuteTask(chromPeakSelector);
                this.ChromPeakSelected = this.Result.ChromPeakSelected;

                ExecuteTask(MSGenerator);
                updateMassSpectrumXYValues(this.Run.XYData);

                ExecuteTask(msPeakDetector);
                ExecuteTask(o16o18FeatureFinder);

                ExecuteTask(quant);


            }
            catch (Exception ex)
            {
                MassTagResultBase result = this.Run.ResultCollection.GetMassTagResult(this.Run.CurrentMassTag);
                result.ErrorDescription = ex.Message + "\n" + ex.StackTrace;

                return;
            }

        }


        public override void InitializeWorkflow()
        {

            ValidateParameters();

            theorFeatureGen = new TomTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.NONE, 0.005);

            chromGen = new PeakChromatogramGenerator(_workflowParameters.ChromToleranceInPPM, _workflowParameters.ChromGeneratorMode);
            chromGen.TopNPeaksLowerCutOff = 0.333;

            int pointsToSmooth = (_workflowParameters.ChromSmootherNumPointsInSmooth + 1) / 2;   // adding 0.5 prevents rounding problems
            chromSmoother = new DeconToolsSavitzkyGolaySmoother(pointsToSmooth, pointsToSmooth, 2);
            chromPeakDetector = new ChromPeakDetector(_workflowParameters.ChromPeakDetectorPeakBR, _workflowParameters.ChromPeakDetectorSigNoise);

            chromPeakSelector = new SmartO16O18ChromPeakSelector(_workflowParameters.ChromNETTolerance, _workflowParameters.NumMSScansToSum);

            msPeakDetector = new DeconToolsPeakDetector(_workflowParameters.MSPeakDetectorPeakBR, _workflowParameters.MSPeakDetectorSigNoise, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, false);

            IterativeTFFParameters iterativeTFFParameters = new IterativeTFFParameters();
            iterativeTFFParameters.ToleranceInPPM = _workflowParameters.MSToleranceInPPM;

            o16o18FeatureFinder = new O16O18TargetedIterativeFeatureFinder(iterativeTFFParameters);

            quant = new O16O18QuantifierTask();

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