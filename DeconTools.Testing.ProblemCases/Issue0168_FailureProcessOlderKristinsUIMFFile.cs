using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using NUnit.Framework;

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

            int startScan = run.MinLCScan;
            int stopScan = run.MaxLCScan;

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
        public void displayFrameInfo()
        {
            RunFactory runfactory = new RunFactory();

            UIMFRun run = (UIMFRun)runfactory.CreateRun(testFile);

            UIMFRunTester tester = new UIMFRunTester();

            int startScan = run.MinLCScan;
            int stopScan = run.MaxLCScan;

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

            int startScan = run.MinLCScan;
            int stopScan = run.MaxLCScan;

            int startFrame = 1;
            int stopFrame = 10;


            tester.DisplayBasicRunInfo(run);
            tester.GenerateMassSpectraForRanges(run, startFrame, stopFrame, startScan, stopScan);


        }


        [Test]
        public void reopenedRedmineIssue_test1()
        {
            string problemFile = @"\\proto-10\IMS_TOF_3\2010_4\okd_urine_S15_19Nov10_Cheetah_10-08-06_0000\okd_urine_S15_19Nov10_Cheetah_10-08-06_0000.UIMF";

            RunFactory runfactory = new RunFactory();

            UIMFRun run = (UIMFRun)runfactory.CreateRun(problemFile);

            UIMFRunTester tester = new UIMFRunTester();

            int startScan = run.MinLCScan;
            int stopScan = run.MaxLCScan;

            int startFrame = 0;
            int stopFrame = 9;


            tester.DisplayBasicRunInfo(run);
            tester.GenerateMassSpectraForRanges(run, startFrame, stopFrame, startScan, stopScan);

            //Getting error:
            //1	0	FAILED. 	Specified argument was out of the range of valid values.
            // Parameter name: ERROR in SumScansNonCached: frame_index 1 is not in range; should be >= 0 and < 0

        }

        [Test]
        public void frameIndexProblem_test1()
        {
            string problemFile = @"\\proto-10\IMS_TOF_3\2010_4\okd_urine_S15_19Nov10_Cheetah_10-08-06_0000\okd_urine_S15_19Nov10_Cheetah_10-08-06_0000.UIMF";

            RunFactory runfactory = new RunFactory();

            UIMFRun run = (UIMFRun)runfactory.CreateRun(problemFile);


            ScanSet frame = new ScanSet(1);
            ScanSet scan = new ScanSet(200);

            run.CurrentScanSet = frame;
            run.CurrentScanSet = scan;

            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            msgen.Execute(run.ResultCollection);

            //fixed it. DeconTools was hard-coded to get the spectrum for frameType '0'. The above file has frameType properly set to '1'. We are eventually moving to the 
            //format of having MS1 frametype set to '1'  and MS2 set to '2'.  Currently, many datasets have '0' as their frameType. 

        }







    }
}
