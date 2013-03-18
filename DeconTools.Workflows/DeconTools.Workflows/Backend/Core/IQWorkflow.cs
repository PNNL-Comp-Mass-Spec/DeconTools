using System;
using System.Collections.Generic;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Utilities;
using DeconTools.Workflows.Backend.Core.ChromPeakSelection;
using DeconTools.Workflows.Backend.FileIO;

namespace DeconTools.Workflows.Backend.Core
{
    public abstract class IqWorkflow
    {
        protected MSGenerator MSGenerator { get; set; }
        protected ITheorFeatureGenerator TheorFeatureGen;
        protected PeakChromatogramGenerator ChromGen;
        protected SavitzkyGolaySmoother ChromSmoother;
        protected ChromPeakDetector ChromPeakDetector;
        protected ChromPeakSelectorBase ChromPeakSelector;
        protected IterativeTFF MsfeatureFinder;
        //protected IsotopicProfileFitScoreCalculator _fitScoreCalc;
        protected IsotopicProfileFitScoreCalculator FitScoreCalc;
        protected ResultValidatorTask ResultValidator;
        protected ChromatogramCorrelatorBase ChromatogramCorrelator;
        protected InterferenceScorer InterferenceScorer;
        protected IterativeTFFParameters IterativeTffParameters = new IterativeTFFParameters();

        protected ChromPeakUtilities ChromPeakUtilities = new ChromPeakUtilities();

        protected ChromPeakAnalyzer ChromPeakAnalyzer;

        public IqWorkflow(Run run, TargetedWorkflowParameters parameters)
        {
            Run = run;
            WorkflowParameters = parameters;

            MsLeftTrimAmount = 1e10;     // set this high so, by default, nothing is trimmed
            MsRightTrimAmount = 1e10;  // set this high so, by default, nothing is trimmed

        }

        public IqWorkflow(TargetedWorkflowParameters parameters)
            : this(null, parameters)
        {

        }


        public virtual void InitializeRunRelatedTasks()
        {
            if (Run != null)
            {
                MSGenerator = MSGeneratorFactory.CreateMSGenerator(this.Run.MSFileType);

            }
        }




        public abstract TargetedWorkflowParameters WorkflowParameters { get; set; }


        private Run _run;



        public Run Run
        {
            get
            {
                return _run;
            }
            set
            {
                if (_run != value)
                {
                    _run = value;

                    if (_run != null)
                    {
                        InitializeRunRelatedTasks();
                    }

                }

            }
        }


        #region Properties

        public bool Success { get; set; }

        public bool IsWorkflowInitialized { get; set; }

        public string WorkflowStatusMessage { get; set; }

        public string Name
        {
            get { return ToString(); }
        }

        /// <summary>
        /// For trimming the final mass spectrum. A value of '2' means 
        /// that the mass spectrum will be trimmed -2 to the given m/z value.
        /// </summary>
        public double MsLeftTrimAmount { get; set; }

        /// <summary>
        /// For trimming the final mass spectrum. A value of '10' means 
        /// that the mass spectrum will be trimmed +10 to the given m/z value.
        /// </summary>
        public double MsRightTrimAmount { get; set; }


        #endregion



        protected virtual void ValidateParameters()
        {
            Check.Require(WorkflowParameters != null, "Cannot validate workflow parameters. Parameters are null");

            bool pointsInSmoothIsEvenNumber = (WorkflowParameters.ChromSmootherNumPointsInSmooth % 2 == 0);
            if (pointsInSmoothIsEvenNumber)
            {
                throw new ArgumentOutOfRangeException("Points in chrom smoother is an even number, but must be an odd number.");
            }
        }


        public virtual void InitializeWorkflow()
        {
            Check.Require(Run != null, "Run is null");

            DoPreInitialization();

            DoMainInitialization();

            DoPostInitialization();

            IsWorkflowInitialized = true;

        }


        protected virtual void DoPreInitialization() { }

        protected virtual void DoPostInitialization() { }


        protected virtual void DoMainInitialization()
        {
            ValidateParameters();

            TheorFeatureGen = new JoshTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.NONE, 0.005);
            ChromGen = new PeakChromatogramGenerator(WorkflowParameters.ChromGenTolerance, WorkflowParameters.ChromGeneratorMode,
                                                      DeconTools.Backend.Globals.IsotopicProfileType.UNLABELLED,
                                                      WorkflowParameters.ChromGenToleranceUnit)
            {
                TopNPeaksLowerCutOff = 0.333,
                ChromWindowWidthForAlignedData = (float)WorkflowParameters.ChromNETTolerance * 2,
                ChromWindowWidthForNonAlignedData = (float)WorkflowParameters.ChromNETTolerance * 2
            };

            //only

            bool allowNegativeValues = false;
            ChromSmoother = new SavitzkyGolaySmoother(WorkflowParameters.ChromSmootherNumPointsInSmooth, 2, allowNegativeValues);
            ChromPeakDetector = new ChromPeakDetector(WorkflowParameters.ChromPeakDetectorPeakBR, WorkflowParameters.ChromPeakDetectorSigNoise);
            ChromPeakSelector = CreateChromPeakSelector(WorkflowParameters);

            ChromPeakAnalyzer = new ChromPeakAnalyzer(WorkflowParameters);

            IterativeTffParameters = new IterativeTFFParameters();
            IterativeTffParameters.ToleranceInPPM = WorkflowParameters.MSToleranceInPPM;

            MsfeatureFinder = new IterativeTFF(IterativeTffParameters);
            FitScoreCalc = new IsotopicProfileFitScoreCalculator();

            InterferenceScorer = new InterferenceScorer();

            ResultValidator = new ResultValidatorTask();
            ChromatogramCorrelator = new ChromatogramCorrelator(WorkflowParameters.ChromSmootherNumPointsInSmooth, 0.01, WorkflowParameters.ChromGenTolerance);


        }


        /// <summary>
        /// Factory method for creating the key ChromPeakSelector algorithm
        /// </summary>
        /// <param name="workflowParameters"></param>
        /// <returns></returns>
        public static ChromPeakSelectorBase CreateChromPeakSelector(TargetedWorkflowParameters workflowParameters)
        {
            ChromPeakSelectorBase chromPeakSelector;
            ChromPeakSelectorParameters chromPeakSelectorParameters = new ChromPeakSelectorParameters();
            chromPeakSelectorParameters.NETTolerance = (float)workflowParameters.ChromNETTolerance;
            chromPeakSelectorParameters.NumScansToSum = workflowParameters.NumMSScansToSum;
            chromPeakSelectorParameters.PeakSelectorMode = workflowParameters.ChromPeakSelectorMode;
            chromPeakSelectorParameters.SummingMode = workflowParameters.SummingMode;
            chromPeakSelectorParameters.AreaOfPeakToSumInDynamicSumming = workflowParameters.AreaOfPeakToSumInDynamicSumming;
            chromPeakSelectorParameters.MaxScansSummedInDynamicSumming = workflowParameters.MaxScansSummedInDynamicSumming;



            switch (workflowParameters.ChromPeakSelectorMode)
            {
                case DeconTools.Backend.Globals.PeakSelectorMode.ClosestToTarget:
                case DeconTools.Backend.Globals.PeakSelectorMode.MostIntense:
                case DeconTools.Backend.Globals.PeakSelectorMode.N15IntelligentMode:
                case DeconTools.Backend.Globals.PeakSelectorMode.RelativeToOtherChromPeak:
                    chromPeakSelector = new BasicChromPeakSelector(chromPeakSelectorParameters);
                    break;

                case DeconTools.Backend.Globals.PeakSelectorMode.Smart:

                    var smartchrompeakSelectorParameters = new SmartChromPeakSelectorParameters(chromPeakSelectorParameters);
                    smartchrompeakSelectorParameters.MSFeatureFinderType = DeconTools.Backend.Globals.TargetedFeatureFinderType.ITERATIVE;
                    smartchrompeakSelectorParameters.MSPeakDetectorPeakBR = workflowParameters.MSPeakDetectorPeakBR;
                    smartchrompeakSelectorParameters.MSPeakDetectorSigNoiseThresh = workflowParameters.MSPeakDetectorSigNoise;
                    smartchrompeakSelectorParameters.MSToleranceInPPM = workflowParameters.MSToleranceInPPM;
                    smartchrompeakSelectorParameters.NumChromPeaksAllowed = workflowParameters.NumChromPeaksAllowedDuringSelection;
                    smartchrompeakSelectorParameters.MultipleHighQualityMatchesAreAllowed = workflowParameters.MultipleHighQualityMatchesAreAllowed;
                    smartchrompeakSelectorParameters.IterativeTffMinRelIntensityForPeakInclusion = 0.66;

                    chromPeakSelector = new SmartChromPeakSelector(smartchrompeakSelectorParameters);

                    break;
                case DeconTools.Backend.Globals.PeakSelectorMode.SmartUIMF:
                    var smartUIMFchrompeakSelectorParameters = new SmartChromPeakSelectorParameters(chromPeakSelectorParameters);
                    smartUIMFchrompeakSelectorParameters.MSFeatureFinderType = DeconTools.Backend.Globals.TargetedFeatureFinderType.ITERATIVE;
                    smartUIMFchrompeakSelectorParameters.MSPeakDetectorPeakBR = workflowParameters.MSPeakDetectorPeakBR;
                    smartUIMFchrompeakSelectorParameters.MSPeakDetectorSigNoiseThresh = workflowParameters.MSPeakDetectorSigNoise;
                    smartUIMFchrompeakSelectorParameters.MSToleranceInPPM = workflowParameters.MSToleranceInPPM;
                    smartUIMFchrompeakSelectorParameters.NumChromPeaksAllowed = workflowParameters.NumChromPeaksAllowedDuringSelection;
                    smartUIMFchrompeakSelectorParameters.MultipleHighQualityMatchesAreAllowed = workflowParameters.MultipleHighQualityMatchesAreAllowed;
                    smartUIMFchrompeakSelectorParameters.IterativeTffMinRelIntensityForPeakInclusion = 0.66;

                    chromPeakSelector = new SmartChromPeakSelectorUIMF(smartUIMFchrompeakSelectorParameters);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return chromPeakSelector;


        }



        public virtual void Execute(IqResult result)
        {
            Check.Require(this.Run != null, "Run has not been defined.");


            if (!IsWorkflowInitialized)
            {
                InitializeWorkflow();
            }
            
            ExecuteWorkflow(result);

            ExecutePostWorkflowHook(result);
         
            

        }


        protected virtual void HandleWorkflowError(Exception ex)
        {
            Success = false;
            WorkflowStatusMessage = "Unexpected IQ workflow error. Error info: " + ex.Message;

            if (ex.Message.Contains("COM") || ex.Message.ToLower().Contains(".dll"))
            {
                throw new ApplicationException("There was a critical failure! Error info: " + ex.Message);
            }

            TargetedResultBase result = Run.ResultCollection.GetTargetedResult(Run.CurrentMassTag);
            result.ErrorDescription = ex.Message + "\n" + ex.StackTrace;
            result.FailedResult = true;
        }


        public virtual ResultExporter GetResultExporter()
        {
            return new IqLabelFreeResultExporter();
        }

        protected virtual void ExecutePostWorkflowHook(IqResult result)
        {
            if (result != null && Success)
            {
                WorkflowStatusMessage = "IqTarget " + result.Target.ID + "; m/z= " + result.Target.Code;

                //if (Result.FailedResult == false)
                //{
                //    if (Result.IsotopicProfile != null)
                //    {
                //        WorkflowStatusMessage = WorkflowStatusMessage + "; Target FOUND!";

                //    }

                //}
                //else
                //{
                //    WorkflowStatusMessage = WorkflowStatusMessage + "; Target NOT found. Reason: " + Result.FailureType;
                //}


            }

        }

        protected virtual void ExecuteWorkflow(IqResult result)
        {

           
            result.Target.TheorIsotopicProfile = TheorFeatureGen.GenerateTheorProfile(result.Target.EmpiricalFormula, result.Target.ChargeState);

            result.IqResultDetail.Chromatogram = ChromGen.GenerateChromatogram(Run, result.Target.TheorIsotopicProfile, result.Target.ElutionTimeTheor);

            result.IqResultDetail.Chromatogram = ChromSmoother.Smooth(result.IqResultDetail.Chromatogram);

            result.ChromPeakList = ChromPeakDetector.FindPeaks(result.IqResultDetail.Chromatogram);

            ChromPeakDetector.CalculateElutionTimes(Run, result.ChromPeakList);

            ChromPeakDetector.FilterPeaksOnNET(WorkflowParameters.ChromNETTolerance, result.Target.ElutionTimeTheor, result.ChromPeakList);

            result.IqResultDetail.ChromPeakQualityData = ChromPeakAnalyzer.GetChromPeakQualityData(Run, result.Target, result.ChromPeakList);

            bool filterOutFlagged = result.Target.TheorIsotopicProfile.GetIndexOfMostIntensePeak() == 0;
            result.ChromPeakSelected = ChromPeakSelector.SelectBestPeak(result.IqResultDetail.ChromPeakQualityData, filterOutFlagged);

            result.LCScanSetSelected = ChromPeakUtilities.GetLCScanSetForChromPeak(result.ChromPeakSelected, Run,
                                                                                 WorkflowParameters.NumMSScansToSum);

            result.IqResultDetail.MassSpectrum = MSGenerator.GenerateMS(Run, result.LCScanSetSelected);

            TrimData(result.IqResultDetail.MassSpectrum, result.Target.MZTheor, MsLeftTrimAmount, MsRightTrimAmount);

            List<Peak> mspeakList;
            result.ObservedIsotopicProfile = MsfeatureFinder.IterativelyFindMSFeature(result.IqResultDetail.MassSpectrum, result.Target.TheorIsotopicProfile, out mspeakList);

            
            result.FitScore = FitScoreCalc.CalculateFitScore(result.Target.TheorIsotopicProfile, result.ObservedIsotopicProfile,
                                                              result.IqResultDetail.MassSpectrum);

            result.InterferenceScore = InterferenceScorer.GetInterferenceScore(result.ObservedIsotopicProfile, mspeakList);

            //if (_workflowParameters.ChromatogramCorrelationIsPerformed)
            //{
            //    ExecuteTask(_chromatogramCorrelator);
            //}

            result.MonoMassObs = result.ObservedIsotopicProfile == null ? 0 : result.ObservedIsotopicProfile.MonoIsotopicMass;

            result.MZObs = result.ObservedIsotopicProfile == null ? 0 : result.ObservedIsotopicProfile.MonoPeakMZ;

            var elutionTime = result.ChromPeakSelected == null ? 0d : ((ChromPeak)result.ChromPeakSelected).NETValue;
            result.ElutionTimeObs = elutionTime;

            result.Abundance = GetAbundance(result);

        }

        //TODO:  later will make this abstract/virtual.  Workflow creates the type of IqResult we want
        protected internal virtual IqResult CreateIQResult(IqTarget target)
        {
            return new IqResult(target);
        }


        protected virtual float GetAbundance(IqResult result)
        {
            if (result.ObservedIsotopicProfile == null) return 0;

            int indexMostAbundantPeakTheor = result.Target.TheorIsotopicProfile.GetIndexOfMostIntensePeak();

            if (result.ObservedIsotopicProfile.Peaklist.Count > indexMostAbundantPeakTheor)
            {
                result.ObservedIsotopicProfile.IntensityMostAbundantTheor = result.ObservedIsotopicProfile.Peaklist[indexMostAbundantPeakTheor].Height;
            }
            else
            {
                result.ObservedIsotopicProfile.IntensityMostAbundantTheor = result.ObservedIsotopicProfile.IntensityMostAbundant;
            }

            return result.ObservedIsotopicProfile.IntensityMostAbundantTheor;
        }


        protected virtual XYData TrimData(XYData xyData, double targetVal, double leftTrimAmount, double rightTrimAmount)
        {
            if (xyData == null) return xyData;

            if (xyData.Xvalues == null || xyData.Xvalues.Length == 0) return xyData;


            double leftTrimValue = targetVal - leftTrimAmount;
            double rightTrimValue = targetVal + rightTrimAmount;


            return xyData.TrimData(leftTrimValue, rightTrimValue, 0.1);




        }



    }
}
