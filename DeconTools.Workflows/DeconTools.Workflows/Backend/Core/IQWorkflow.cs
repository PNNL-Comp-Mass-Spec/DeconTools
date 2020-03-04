using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Utilities;
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
        protected IterativeTFF MsFeatureFinder;
        protected IsotopicProfileFitScoreCalculator FitScoreCalc;
        protected ResultValidatorTask ResultValidator;
        protected IqChromCorrelatorBase ChromatogramCorrelator;
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
                MSGenerator = MSGeneratorFactory.CreateMSGenerator(Run.MSFileType);
                MSGenerator.IsTICRequested = false;
            }
        }




        public abstract TargetedWorkflowParameters WorkflowParameters { get; set; }


        private Run _run;



        public Run Run
        {
            get => _run;
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

        public string Name => ToString();

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

            var pointsInSmoothIsEvenNumber = (WorkflowParameters.ChromSmootherNumPointsInSmooth % 2 == 0);
            if (pointsInSmoothIsEvenNumber)
            {
                throw new Exception("Points in chrom smoother is an even number, but must be an odd number (abstract class IqWorkflow).");
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

            TheorFeatureGen = new JoshTheorFeatureGenerator(DeconTools.Backend.Globals.LabelingType.NONE, 0.005);
            ChromGen = new PeakChromatogramGenerator(WorkflowParameters.ChromGenTolerance, WorkflowParameters.ChromGeneratorMode,
                                                      DeconTools.Backend.Globals.IsotopicProfileType.UNLABELED,
                                                      WorkflowParameters.ChromGenToleranceUnit)
            {
                TopNPeaksLowerCutOff = 0.333,
                ChromWindowWidthForAlignedData = (float)WorkflowParameters.ChromNETTolerance * 2,
                ChromWindowWidthForNonAlignedData = (float)WorkflowParameters.ChromNETTolerance * 2
            };

            ChromSmoother = new SavitzkyGolaySmoother(WorkflowParameters.ChromSmootherNumPointsInSmooth, 2);
            ChromPeakDetector = new ChromPeakDetector(WorkflowParameters.ChromPeakDetectorPeakBR, WorkflowParameters.ChromPeakDetectorSigNoise);
            ChromPeakSelector = CreateChromPeakSelector(WorkflowParameters);

            ChromPeakAnalyzer = new ChromPeakAnalyzer(WorkflowParameters);

            IterativeTffParameters = new IterativeTFFParameters {
                ToleranceInPPM = WorkflowParameters.MSToleranceInPPM
            };

            MsFeatureFinder = new IterativeTFF(IterativeTffParameters);
            FitScoreCalc = new IsotopicProfileFitScoreCalculator();

            InterferenceScorer = new InterferenceScorer();

            ResultValidator = new ResultValidatorTask();
            ChromatogramCorrelator = new IqChromCorrelator(WorkflowParameters.ChromSmootherNumPointsInSmooth, 0.05, WorkflowParameters.ChromGenTolerance);


        }


        /// <summary>
        /// Factory method for creating the key ChromPeakSelector algorithm
        /// </summary>
        /// <param name="workflowParameters"></param>
        /// <returns></returns>
        public virtual ChromPeakSelectorBase CreateChromPeakSelector(TargetedWorkflowParameters workflowParameters)
        {
            ChromPeakSelectorBase chromPeakSelector;
            var chromPeakSelectorParameters = new ChromPeakSelectorParameters
            {
                NETTolerance = (float)workflowParameters.ChromNETTolerance,
                NumScansToSum = workflowParameters.NumMSScansToSum,
                PeakSelectorMode = workflowParameters.ChromPeakSelectorMode,
                SummingMode = workflowParameters.SummingMode,
                AreaOfPeakToSumInDynamicSumming = workflowParameters.AreaOfPeakToSumInDynamicSumming,
                MaxScansSummedInDynamicSumming = workflowParameters.MaxScansSummedInDynamicSumming
            };

            switch (workflowParameters.ChromPeakSelectorMode)
            {
                case DeconTools.Backend.Globals.PeakSelectorMode.ClosestToTarget:
                case DeconTools.Backend.Globals.PeakSelectorMode.MostIntense:
                case DeconTools.Backend.Globals.PeakSelectorMode.N15IntelligentMode:
                case DeconTools.Backend.Globals.PeakSelectorMode.RelativeToOtherChromPeak:
                    chromPeakSelector = new BasicChromPeakSelector(chromPeakSelectorParameters);
                    break;

                case DeconTools.Backend.Globals.PeakSelectorMode.Smart:

                    var smartChromPeakSelectorParameters = new SmartChromPeakSelectorParameters(chromPeakSelectorParameters)
                    {
                        MSFeatureFinderType = DeconTools.Backend.Globals.TargetedFeatureFinderType.ITERATIVE,
                        MSPeakDetectorPeakBR = workflowParameters.MSPeakDetectorPeakBR,
                        MSPeakDetectorSigNoiseThresh = workflowParameters.MSPeakDetectorSigNoise,
                        MSToleranceInPPM = workflowParameters.MSToleranceInPPM,
                        NumChromPeaksAllowed = workflowParameters.NumChromPeaksAllowedDuringSelection,
                        MultipleHighQualityMatchesAreAllowed = workflowParameters.MultipleHighQualityMatchesAreAllowed,
                        IterativeTffMinRelIntensityForPeakInclusion = 0.66
                    };

                    chromPeakSelector = new SmartChromPeakSelector(smartChromPeakSelectorParameters);

                    break;
                case DeconTools.Backend.Globals.PeakSelectorMode.SmartUIMF:
                    var smartUIMFChromPeakSelectorParameters = new SmartChromPeakSelectorParameters(chromPeakSelectorParameters)
                    {
                        MSFeatureFinderType = DeconTools.Backend.Globals.TargetedFeatureFinderType.ITERATIVE,
                        MSPeakDetectorPeakBR = workflowParameters.MSPeakDetectorPeakBR,
                        MSPeakDetectorSigNoiseThresh = workflowParameters.MSPeakDetectorSigNoise,
                        MSToleranceInPPM = workflowParameters.MSToleranceInPPM,
                        NumChromPeaksAllowed = workflowParameters.NumChromPeaksAllowedDuringSelection,
                        MultipleHighQualityMatchesAreAllowed = workflowParameters.MultipleHighQualityMatchesAreAllowed,
                        IterativeTffMinRelIntensityForPeakInclusion = 0.66
                    };

                    chromPeakSelector = new SmartChromPeakSelectorUIMF(smartUIMFChromPeakSelectorParameters);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return chromPeakSelector;


        }



        public virtual void Execute(IqResult result)
        {
            Check.Require(Run != null, "Error in IqWorkflow.Execute: Run has not been defined.");


            if (!IsWorkflowInitialized)
            {
                InitializeWorkflow();
            }
            try
            {
                ExecuteWorkflow(result);
                ExecutePostWorkflowHook(result);
            }
            catch (Exception ex)
            {
                var errorMessage = "Critical error!!!! " + ex.Message + "; processing IqTargetID = " + result.Target.ID + "; charge = " + result.Target.ChargeState +
                                      "; sequence= " + result.Target.Code + "; ScanLC= " + result.Target.ScanLC;

                Console.WriteLine(errorMessage);
                throw new ApplicationException(errorMessage, ex);
            }






        }


        protected virtual void HandleWorkflowError(Exception ex)
        {
            Success = false;
            WorkflowStatusMessage = "Unexpected IQ workflow error. Error info: " + ex.Message;

            if (ex.Message.Contains("COM") || ex.Message.ToLower().Contains(".dll"))
            {
                throw new ApplicationException("There was a critical failure! Error info: " + ex.Message);
            }

            var result = Run.ResultCollection.GetTargetedResult(Run.CurrentMassTag);
            result.ErrorDescription = ex.Message + "\n" + ex.StackTrace;
            result.FailedResult = true;
        }


        public virtual IqResultExporter GetResultExporter()
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



        public virtual void TestTarget(IqTarget target)
        {

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

            var filterOutFlagged = result.Target.TheorIsotopicProfile.GetIndexOfMostIntensePeak() == 0;
            result.ChromPeakSelected = ChromPeakSelector.SelectBestPeak(result.IqResultDetail.ChromPeakQualityData, filterOutFlagged);


            result.LCScanSetSelected = ChromPeakUtilities.GetLCScanSetForChromPeak(result.ChromPeakSelected, Run,
                                                                                 WorkflowParameters.NumMSScansToSum);

            result.LcScanObs = result.LCScanSetSelected?.PrimaryScanNumber ?? -1;

            result.IqResultDetail.MassSpectrum = MSGenerator.GenerateMS(Run, result.LCScanSetSelected);

            TrimData(result.IqResultDetail.MassSpectrum, result.Target.MZTheor, MsLeftTrimAmount, MsRightTrimAmount);

            result.ObservedIsotopicProfile = MsFeatureFinder.IterativelyFindMSFeature(result.IqResultDetail.MassSpectrum, result.Target.TheorIsotopicProfile, out var msPeakList);


            result.FitScore = FitScoreCalc.CalculateFitScore(result.Target.TheorIsotopicProfile, result.ObservedIsotopicProfile,
                                                              result.IqResultDetail.MassSpectrum);

            result.InterferenceScore = InterferenceScorer.GetInterferenceScore(result.ObservedIsotopicProfile, msPeakList);

            //if (_workflowParameters.ChromatogramCorrelationIsPerformed)
            //{
            //    ExecuteTask(_chromatogramCorrelator);
            //}

            result.MonoMassObs = result.ObservedIsotopicProfile?.MonoIsotopicMass ?? 0;

            result.MZObs = result.ObservedIsotopicProfile?.MonoPeakMZ ?? 0;

            result.MZObsCalibrated = result.ObservedIsotopicProfile == null ? 0 : Run.GetAlignedMZ(result.ObservedIsotopicProfile.MonoPeakMZ, result.LcScanObs);
            result.MonoMassObsCalibrated = result.ObservedIsotopicProfile == null
                                               ? 0
                                               : (result.MZObsCalibrated - DeconTools.Backend.Globals.PROTON_MASS) * result.Target.ChargeState;


            var elutionTime = ((ChromPeak)result.ChromPeakSelected)?.NETValue ?? 0d;
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

            var indexMostAbundantPeakTheor = result.Target.TheorIsotopicProfile.GetIndexOfMostIntensePeak();

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
            if (xyData == null) return null;

            if (xyData.Xvalues == null || xyData.Xvalues.Length == 0) return xyData;


            var leftTrimValue = targetVal - leftTrimAmount;
            var rightTrimValue = targetVal + rightTrimAmount;


            return xyData.TrimData(leftTrimValue, rightTrimValue, 0.1);
        }

        /// <summary>
        /// Calculates mass error based on the theoretical most intense peak.
        /// </summary>
        /// <returns> This returns the mass error between a theoretical and observed peak.  NOTE: this is MASS, not m/z
        /// If no peak is detected, we return the mass error 999999.  This should be interpreted as a null value.</returns>
        protected double TheorMostIntensePeakMassError(IsotopicProfile theoreticalIso, IsotopicProfile observedIso, int chargeState)
        {
            var theoreticalMostIntensePeak = theoreticalIso.getMostIntensePeak();

            //find peak in obs data
            var mzTolerance = WorkflowParameters.MSToleranceInPPM * theoreticalMostIntensePeak.XValue / 1e6;
            var foundPeaks = PeakUtilities.GetPeaksWithinTolerance(new List<Peak>(observedIso.Peaklist), theoreticalMostIntensePeak.XValue, mzTolerance);

            if (foundPeaks.Count == 0)
            {
                return 999999;
            }

            var obsXValue = foundPeaks.OrderByDescending(p => p.Height).First().XValue; //order the peaks and take the first (most intense) one.
            return ((theoreticalMostIntensePeak.XValue * chargeState) - (obsXValue * chargeState));
        }

        public abstract IqResultExporter CreateExporter();
    }
}
