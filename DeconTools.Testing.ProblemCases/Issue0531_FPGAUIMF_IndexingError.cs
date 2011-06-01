using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using DeconTools.UnitTesting2;
using DeconTools.Backend;

namespace DeconTools.Testing.ProblemCases
{
    [TestFixture]
    public class Issue0531_FPGAUIMF_IndexingError
    {
        [Test]
        public void Test1()
        {
            string testFile = @"D:\Data\UIMF\Problem_datasets\FPGA\QCSHEW_FPGA_watersLC_50min_002.UIMF";

            RunFactory rf = new RunFactory();
            UIMFRun run = (UIMFRun)rf.CreateRun(testFile);

            UIMFRunTester tester = new UIMFRunTester();
            tester.DisplayBasicRunInfo(run);

            //UIMFLibrary.DataReader dr = new UIMFLibrary.DataReader();
            //dr.OpenUIMF(testFile);
            tester.DisplayFrameParameters(run, run.MinFrame, run.MaxFrame);

            


            //tester.DisplayFrameParameters(run, 0, run.MaxFrame);
        }


        [Test]
        public void test2()
        {
            string testFile = @"D:\Data\UIMF\Problem_datasets\FPGA\QCSHEW_FPGA_99min_0003.UIMF";

            RunFactory rf = new RunFactory();
            UIMFRun run = (UIMFRun)rf.CreateRun(testFile);

            UIMFRunTester tester = new UIMFRunTester();
            tester.DisplayBasicRunInfo(run);

            //UIMFLibrary.DataReader dr = new UIMFLibrary.DataReader();
            //dr.OpenUIMF(testFile);
            tester.DisplayFrameParameters(run, run.MinFrame, run.MaxFrame);

        }


        [Test]
        public void oldSchoolTest1()
        {

            string testFile = @"D:\Data\UIMF\Problem_datasets\FPGA\QCSHEW_FPGA_99min_0003.UIMF";
            string paramFile = @"D:\Data\UIMF\Problem_datasets\FPGA\IMS_UIMF_PeakBR4_PeptideBR4_SN3_SumScansAll_SumFrames3_RAPID_2011-02-09_temp.xml";

            RunFactory rf = new RunFactory();
            UIMFRun run = (UIMFRun)rf.CreateRun(testFile);

            OldSchoolProcRunner proc = new OldSchoolProcRunner(testFile, run.MSFileType, paramFile);
            proc.Execute();

        }



    }
}
