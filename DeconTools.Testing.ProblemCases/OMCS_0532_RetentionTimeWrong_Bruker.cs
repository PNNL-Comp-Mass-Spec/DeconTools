using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.UnitTesting2;
using NUnit.Framework;

namespace DeconTools.Testing.ProblemCases
{
    [TestFixture]
    public class OMCS_0532_RetentionTimeWrong_Bruker
    {

        //see https://jira.pnnl.gov/jira/browse/OMCS-532

        [Test]
        public void Test1()
        {
            string dataset = @"\\proto-7\Maxis_01\2012_2\2012_05_15_MN9_A_000114\2012_05_15_MN9_A_000114.d";

            var run = new RunFactory().CreateRun(dataset);

            run.ScanSetCollection.Create(run, 1, 1, true);

            foreach (var scanSet in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scanSet;

                var rt = run.GetTime(scanSet.PrimaryScanNumber);

                Console.WriteLine(scanSet.PrimaryScanNumber + "\t" + rt);
            }


            TestUtilities.DisplayRunInformation(run);

        }

    }
}
