    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
    using DeconTools.Backend;
    using DeconTools.Backend.Core;
    using DeconTools.Backend.ProcessingTasks;
    using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
    using DeconTools.Backend.ProcessingTasks.PeakDetectors;
    using DeconTools.Backend.ProcessingTasks.Smoothers;
    using DeconTools.Backend.Runs;
    using DeconTools.Backend.Utilities;
    using DeconTools.Backend.Workflows;
    using DeconTools.UnitTesting2;
    using DeconTools.Workflows.Backend.Core;
    using DeconTools.Workflows.Backend.Core.ChromPeakSelection;
    using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class UnitTestTemplate1
    {

        [Test]
        public void createPeaksTest1()
        {
             var testFile =
                @"D:\Data\Orbitrap\BrianIQTesting\QC_Shew_11_02_pt5-b_6Jun11_Sphinx_11-03-27.RAW";
            var run = new RunFactory().CreateRun(testFile);

            var parameters = new PeakDetectAndExportWorkflowParameters();
            TargetedWorkflowParameters deconParam = new BasicTargetedWorkflowParameters();
            deconParam.ChromGenSourceDataPeakBR = 3;


            parameters.PeakBR = deconParam.ChromGenSourceDataPeakBR;
            parameters.PeakFitType = DeconTools.Backend.Globals.PeakFitType.QUADRATIC;
            parameters.SigNoiseThreshold = deconParam.ChromGenSourceDataSigNoise;
            var peakCreator = new PeakDetectAndExportWorkflow(run, parameters);
            peakCreator.Execute();
        }


        [Test]
        public void Test1()
        {


            var testFile =
                @"D:\Data\Orbitrap\BrianIQTesting\QC_Shew_11_02_pt5-b_6Jun11_Sphinx_11-03-27.RAW";
            var peaksTestFile =
                @"D:\Data\Orbitrap\BrianIQTesting\QC_Shew_11_02_pt5-b_6Jun11_Sphinx_11-03-27_peaks.txt";


            var run = RunUtilities.CreateAndLoadPeaks(testFile, peaksTestFile);

            var target = new LcmsFeatureTarget();
            target.ID = 0;


            target.MZ = 715.39214;
            target.ScanLCTarget = 7343;
            target.ElutionTimeUnit = Globals.ElutionTimeUnit.ScanNum;
            run.CurrentMassTag = target;

            var result = run.ResultCollection.GetTargetedResult(run.CurrentMassTag);

            double chromPeakGeneratorTolInPPM = 20;
            var chromGeneratorMode = Globals.ChromatogramGeneratorMode.MZ_BASED;

            var chromGen = new PeakChromatogramGenerator(chromPeakGeneratorTolInPPM, chromGeneratorMode);
            // If we want to use the Execute Command
            //BLL .1 and .5 NET windows work.   .02 NET window does not BR and SN was set to 1 however)
            chromGen.ChromWindowWidthForNonAlignedData = .02F;


            var pointsToSmooth = 5;
            var chromSmoother = new SavitzkyGolaySmoother(pointsToSmooth, 2);

            //BLL We also tried to set the BR and SIG NOISE to 0.  This did not work
            var chromPeakDetectorPeakBR = 0.5;
            var chromPeakDetectorSigNoise = 0.5;
            var chromPeakDetector = new ChromPeakDetector(chromPeakDetectorPeakBR, chromPeakDetectorSigNoise);


            var chromPeakSelectorParameters = new ChromPeakSelectorParameters {
                PeakSelectorMode = Globals.PeakSelectorMode.ClosestToTarget
            };

            var chromPeakSelector = new BasicChromPeakSelector(chromPeakSelectorParameters);

            var smartChromPeakParameters = new SmartChromPeakSelectorParameters();


            var smartChromPeakSelector = new SmartChromPeakSelector(smartChromPeakParameters);


            //this generates an extracted ion chromatogram
            // Since we are not using the built in generator,
            chromGen.Execute(run.ResultCollection);

            //this smooths the data - very important step!

            //BLL. This didnt work for me when we first started, instead of using the NET window above.
            //chromGen.GenerateChromatogram(run,
            //                                target.ScanLCTarget - 300,
            //                                target.ScanLCTarget + 300,
            //                                target.MZ,
            //                                chromPeakGeneratorTolInPPM);
            chromSmoother.Execute(run.ResultCollection);

            //this detects peaks within an extracted ion chromatogram
            chromPeakDetector.Execute(run.ResultCollection);

            //this selects the peak
            chromPeakSelector.Execute(run.ResultCollection);
            //smartChromPeakSelector.Execute(run.ResultCollection);


            //TestUtilities.DisplayXYValues(run.XYData);
            TestUtilities.DisplayPeaks(run.PeakList);




            Console.WriteLine("Number of peaks detected = " + run.PeakList.Count);
            Console.WriteLine("Selected peak= " + result.ChromPeakSelected);
        }

    }
}
