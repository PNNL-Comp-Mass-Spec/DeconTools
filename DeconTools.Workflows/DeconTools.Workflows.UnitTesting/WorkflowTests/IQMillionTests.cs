using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Core.ChromPeakSelection;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    class IQMillionTests
    {
        [Test]
        public void TestIQMillion()
        {
            const short minChargeState = 1;
            const short maxChargeState = 4;
            const int minPrecursorNominalMass = 400;
            const int maxPrecursorNominalMass = 2500;

            const string datasetPath = @"\\protoapps\UserData\Crowell\ForSangtae\BSA\BSA_10ugml_IMS6_TOF03_CID_27Aug12_Frodo_Collision_Energy_Collapsed.UIMF";

            var executorParameters = CreateWorkflowExecutorParameters();
            var workflowParameters = CreateWorkflowParameters();

            // Create workflow for precursors
            var precursorExecutor = new UIMFTargetedWorkflowExecutor(executorParameters, workflowParameters, datasetPath)
            {
                Targets = new TargetCollection()
            };

            var targetList = new List<TargetBase>();
            for (int precursorNominalMass = minPrecursorNominalMass;
                 precursorNominalMass <= maxPrecursorNominalMass;
                 precursorNominalMass++)
            {
                for (short chargeState = minChargeState;
                     chargeState <= maxChargeState;
                     chargeState++)
                {
                    targetList.Add(new NominalMassTarget(nominalMass: precursorNominalMass, chargeState: chargeState, msLevel: 1));
                }
            }

            precursorExecutor.Targets.TargetList = targetList;
            precursorExecutor.Execute();
            Console.WriteLine("Precursor caching done.");

            List<TargetedResultBase> precursorResultList =
                precursorExecutor.TargetedWorkflow.Run.ResultCollection.GetMassTagResults();
            foreach (TargetedResultBase precursorTargetedResultBase in precursorResultList)
            {
                var precursorTarget = precursorTargetedResultBase.Target as NominalMassTarget;
                if (precursorTarget != null)
                    Console.WriteLine("***{0} {1}", precursorTarget.NominalMass, precursorTarget.ChargeState);

                if (precursorTargetedResultBase.ErrorDescription != null &&
                    precursorTargetedResultBase.ErrorDescription.Any())
                {
                    Console.WriteLine("Precursor error: " + precursorTargetedResultBase.ErrorDescription);
                    continue;
                }
                if (precursorTargetedResultBase.FailedResult)
                {
                    Console.WriteLine("Precursor failed result: " + precursorTargetedResultBase.FailureType);
                    continue;
                }

            }

            //const int minFragmentNominalMass = 20;
            //const int maxFragmentNominalMass = maxPrecursorNominalMass-50;
            //const short minFragmentChargeState = 1;
            //const short maxFragmentChargeState = 3;

            //var fragmentExecutor = new UIMFTargetedWorkflowExecutor(executorParameters, workflowParameters, datasetPath)
            //                           {
            //                               Targets = new TargetCollection()
            //                           };

            //var fragmentTargetList = new List<TargetBase>();
            //for (int fragmentNominalMass = minFragmentNominalMass;
            //     fragmentNominalMass <= maxFragmentNominalMass;
            //     fragmentNominalMass++)
            //{
            //    for (short chargeState = minFragmentChargeState;
            //         chargeState <= maxFragmentChargeState;
            //         chargeState++)
            //    {
            //        fragmentTargetList.Add(new NominalMassTarget(nominalMass: fragmentNominalMass, chargeState: chargeState, msLevel: 2));
            //    }
            //}

            //fragmentExecutor.Targets.TargetList = fragmentTargetList;
            //fragmentExecutor.Execute();
            //Console.WriteLine("Fragment caching done.");
        }

        private static BasicTargetedWorkflowExecutorParameters CreateWorkflowExecutorParameters()
        {
            string currentDirectory = Directory.GetCurrentDirectory();

            var executorParameters = new BasicTargetedWorkflowExecutorParameters();

            executorParameters.CopyRawFileLocal = false;
            executorParameters.DeleteLocalDatasetAfterProcessing = false;
            executorParameters.TargetedAlignmentIsPerformed = false;

            return executorParameters;
        }

        private TargetedWorkflowParameters CreateWorkflowParameters()
        {
            var workflowParameters = new IQMillionWorkflowParameters();

            workflowParameters.AreaOfPeakToSumInDynamicSumming = 2;
            workflowParameters.ChromatogramCorrelationIsPerformed = false;
            workflowParameters.ChromGeneratorMode = Globals.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK;
            workflowParameters.ChromGenSourceDataPeakBR = 2;
            workflowParameters.ChromGenSourceDataSigNoise = 3;
            workflowParameters.ChromNETTolerance = 0.2;
            workflowParameters.ChromPeakDetectorPeakBR = 1;
            workflowParameters.ChromPeakDetectorSigNoise = 1;
            workflowParameters.ChromPeakSelectorMode = Globals.PeakSelectorMode.SmartUIMF;
            workflowParameters.ChromSmootherNumPointsInSmooth = 9;
            workflowParameters.ChromGenTolerance = 1.0005 / 2;
            //workflowParameters.ChromGenTolerance = 0.5;
            workflowParameters.ChromGenToleranceUnit = Globals.ToleranceUnit.MZ;

            workflowParameters.MaxScansSummedInDynamicSumming = 100;
            workflowParameters.MSPeakDetectorPeakBR = 1.3;
            workflowParameters.MSPeakDetectorSigNoise = 3;
            workflowParameters.MSToleranceInPPM = 25;
            workflowParameters.MultipleHighQualityMatchesAreAllowed = true;
            workflowParameters.NumMSScansToSum = 1;
            workflowParameters.NumChromPeaksAllowedDuringSelection = int.MaxValue;
            workflowParameters.ProcessMsMs = true;
            workflowParameters.ResultType = Globals.ResultType.BASIC_TARGETED_RESULT;
            workflowParameters.SummingMode = SummingModeEnum.SUMMINGMODE_STATIC;

            return workflowParameters;
        }

    }
}
