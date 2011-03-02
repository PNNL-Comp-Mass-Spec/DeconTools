using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Runs;

namespace DeconTools.Testing.ProblemCases
{
    [TestFixture]
    public class Issue0168_FailureProcessOlderKristinsUIMFFile
    {
        string testFile = @"D:\Data\Redmine_issues\Issue0168_FailureOnKristinsOldUIMFFiles\Dey_KO_8721_02_17Nov10_10-09-23_0000.UIMF";


        string standardFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf";


        [Test]
        public void getMassSpectra()
        {
            RunFactory runfactory = new RunFactory();

            UIMFRun run = (UIMFRun)runfactory.CreateRun(testFile);

            UIMFRunTester tester = new UIMFRunTester();

            int startScan = run.MinScan;
            int stopScan = run.MaxScan;

            int startFrame = 6;
            int stopFrame = 6;


            tester.DisplayBasicRunInfo(run);
            tester.GenerateMassSpectraForRanges(run, startFrame, stopFrame, startScan, stopScan);

           // tester.DisplayFrameParameters(run, startFrame, stopFrame);


        }

        [Test]
        public void isolatedProblem_Test1()
        {
            RunFactory runfactory = new RunFactory();

            UIMFRun run = (UIMFRun)runfactory.CreateRun(testFile);

            UIMFRunTester tester = new UIMFRunTester();

            int startScan = 283;
            int stopScan = 286;

            int startFrame = 6;
            int stopFrame = 6;


            tester.DisplayBasicRunInfo(run);
            tester.GenerateMassSpectraForRanges(run, startFrame, stopFrame, startScan, stopScan);

            // tester.DisplayFrameParameters(run, startFrame, stopFrame);


        }


        [Test]
        public void isolatedProblem_Test2()
        {
            UIMFLibrary.DataReader dr = new UIMFLibrary.DataReader();
            dr.OpenUIMF(testFile);

            UIMFLibrary.GlobalParameters gp=dr.GetGlobalParameters();

            int numBins= gp.Bins;

            double[] mzs = new double[numBins];
            int[]intensities = new int[numBins];

            int startFrame = 6;
            int stopFrame = 6;


            //the following throws an error
            dr.SumScansNonCached(mzs, intensities, 0, startFrame, stopFrame, 285, 285);

            // tester.DisplayFrameParameters(run, startFrame, stopFrame);


        }



        [Test]
        public void displayFrameInfo()
        {
            RunFactory runfactory = new RunFactory();

            UIMFRun run = (UIMFRun)runfactory.CreateRun(testFile);

            UIMFRunTester tester = new UIMFRunTester();

            int startScan = run.MinScan;
            int stopScan = run.MaxScan;

            int startFrame = 1;
            int stopFrame = 7;


            tester.DisplayBasicRunInfo(run);
            
            tester.DisplayFrameParameters(run, startFrame, stopFrame);


        }


        [Test]
        public void test_usingStandardFile()
        {
            RunFactory runfactory = new RunFactory();

            UIMFRun run = (UIMFRun)runfactory.CreateRun(standardFile);

            UIMFRunTester tester = new UIMFRunTester();

            int startScan = run.MinScan;
            int stopScan = run.MaxScan;

            int startFrame = 1;
            int stopFrame = 10;


            tester.DisplayBasicRunInfo(run);
            tester.GenerateMassSpectraForRanges(run, startFrame, stopFrame, startScan, stopScan);


        }


    }
}
