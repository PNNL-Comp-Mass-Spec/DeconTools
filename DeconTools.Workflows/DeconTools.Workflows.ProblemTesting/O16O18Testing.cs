using System;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.UnitTesting2;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.ProblemTesting
{
    [TestFixture]
    public class O16O18Testing
    {
        [Test]
        public void Test1()
        {
            string executorParametersFile =
                @"D:\Data\From_Vlad\Bruker\Parameters\ExecutorParameters1 - Copy.xml";

            BasicTargetedWorkflowExecutorParameters executorParameters = new BasicTargetedWorkflowExecutorParameters();
            executorParameters.LoadParameters(executorParametersFile);

            string testDatasetPath =
                @"D:\Data\From_Vlad\Bruker\2013_01_29_ALZ_CTRL_5_0p5_1_01_228.d";

            Run run = new RunFactory().CreateRun(testDatasetPath);
            int numScans = run.GetNumMSScans();

            TargetedWorkflowExecutor executor = new BasicTargetedWorkflowExecutor(executorParameters, testDatasetPath);

            int testTarget = 206675561;   //beta amyloid
            executor.Targets.TargetList =
                executor.Targets.TargetList.Where(p => p.ID == testTarget && p.ChargeState == 2).ToList();

            executor.Execute();

            //TestUtilities.DisplayXYValues(executor.TargetedWorkflow.ChromatogramXYData);
            executor.TargetedWorkflow.Result.DisplayToConsole();

            Console.WriteLine("expected mz= " + executor.TargetedWorkflow.Result.Target.MZ);

            double expectedNET = executor.TargetedWorkflow.Result.Target.NormalizedElutionTime;
            double expectedScan = expectedNET * numScans;

            Console.WriteLine("Expected NET= " + expectedNET + "; Corresponds to scan: " + expectedScan);
        }

        [Test]
        public void GetChromatogramsTest1()
        {
            string testDatasetPath =
              @"D:\Data\From_Vlad\Bruker\2013_01_29_ALZ_CTRL_5_0p5_1_01_228.d";

            Run run = RunUtilities.CreateAndLoadPeaks(testDatasetPath);

            PeakChromatogramGenerator chromGen = new PeakChromatogramGenerator(10);

            double targetMZ = 663.3404;
            int chargeState = 2;
            int peakNum = 4;


            targetMZ = targetMZ + peakNum * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / chargeState;

            double toleranceInPPM = 10;

            int startScan = 230;
            int stopScan = 300;

            chromGen.GenerateChromatogram(run, startScan, stopScan, targetMZ, toleranceInPPM);

            run.XYData = run.XYData.TrimData(startScan, stopScan);

            TestUtilities.DisplayXYValues(run.XYData);



        }



        [Test]
        public void OptimizePeakDetectionSettings()
        {

            string testDatasetPath =
               @"D:\Data\From_Vlad\Bruker\2013_01_29_ALZ_CTRL_5_0p5_1_01_228.d";



            int[] scanList = new int[] { 200 };

            Run run = new RunFactory().CreateRun(testDatasetPath);




            var peakDetector = new DeconToolsPeakDetector();
            peakDetector.PeakToBackgroundRatio = 7;
            peakDetector.SignalToNoiseThreshold = 5;
            peakDetector.IsDataThresholded = false;

            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var decon = new HornDeconvolutor();
            decon.MaxFitAllowed = 0.6;


            double[] peakBRList = new double[] { 5, 7, 9, 11, 15, 21 };

            double[] sigNoiseList = new double[] { 2, 3, 4, 5 };

            var smoother = new SavitzkyGolaySmoother(5, 2);


            foreach (var scan in scanList)
            {
                ScanSet scanSet = new ScanSet(scan);


                run.CurrentScanSet = scanSet;

                msgen.Execute(run.ResultCollection);

                foreach (var peakBRVal in peakBRList)
                {
                    foreach (var sigNoiseVal in sigNoiseList)
                    {
                        run.ResultCollection.IsosResultBin.Clear();

                        smoother.Execute(run.ResultCollection);

                        peakDetector.PeakToBackgroundRatio = peakBRVal;
                        peakDetector.SignalToNoiseThreshold = sigNoiseVal;

                        peakDetector.Execute(run.ResultCollection);
                        decon.Execute(run.ResultCollection);

                        int numFeatures = run.ResultCollection.IsosResultBin.Count;
                        int numPeaks = run.PeakList.Count;
                        int numHQFeatures = run.ResultCollection.IsosResultBin.Count(p => p.IsotopicProfile.Score < 0.25);

                        double ratioFeaturesToPeaks = (double)numHQFeatures / numPeaks;

                        Console.WriteLine(scan + "\t" + peakBRVal + "\t" + sigNoiseVal + "\t" + numPeaks + "\t" + numFeatures + "\t" + numHQFeatures +
                                          "\t" + ratioFeaturesToPeaks.ToString("0.000"));

                    }



                }






            }



        }

    }
}
