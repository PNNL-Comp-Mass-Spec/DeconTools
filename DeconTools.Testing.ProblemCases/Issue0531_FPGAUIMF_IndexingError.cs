using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using DeconTools.UnitTesting2;

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

    }
}
