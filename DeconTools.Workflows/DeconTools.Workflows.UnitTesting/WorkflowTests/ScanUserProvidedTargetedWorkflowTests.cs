﻿using System;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class ScanUserProvidedTargetedWorkflowTests
    {
        [Test]
        public void Test1()
        {
            string testFile = @"\\protoapps\UserData\Slysz\Data\O16O18\BSA\BSA_18O_99_8Jan11_Falcon_10-12-09.RAW";

            string massTagFile =
                @"\\protoapps\UserData\Slysz\Data\O16O18\BSA\Targets\BSAmassTags_MinimalInfo_withScans.txt";

            string peakTestFile = testFile.Replace(".RAW", "_peaks.txt");

            var run = RunUtilities.CreateAndLoadPeaks(testFile, peakTestFile);


            var mtc = new TargetCollection();
            var mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            int testMassTagID = 3;
            run.CurrentMassTag = (from n in mtc.TargetList where n.ID == testMassTagID && n.ChargeState == 2 select n).First();

            TargetedWorkflowParameters parameters = new O16O18WorkflowParameters();
            parameters.ChromNETTolerance = 0.2;
            parameters.MSToleranceInPPM = 10;
            parameters.ChromPeakSelectorMode = Globals.PeakSelectorMode.MostIntense;
            parameters.ChromGeneratorMode = ChromatogramGeneratorMode.O16O18_THREE_MONOPEAKS;
            parameters.ChromSmootherNumPointsInSmooth = 31;

            O16O18Workflow workflow = new O16O18Workflow(run, parameters);
            workflow.Execute();

            Assert.IsNotNull(workflow.ChromPeakSelected);

            Assert.AreEqual(10622, (int)workflow.ChromPeakSelected.XValue);
            Console.WriteLine("ChromPeak scan value = " + workflow.ChromPeakSelected.XValue);


        }

    }
}
