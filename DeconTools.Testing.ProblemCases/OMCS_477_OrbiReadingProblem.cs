using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.Testing.ProblemCases
{
    [TestFixture]
    public class OMCS_477_OrbiReadingProblem
    {

        //testing for issue:   https://jira.pnnl.gov/jira/browse/OMCS-477

        [Test]
        public void Test1()
        {

            var run =
                new RunFactory().CreateRun(
                    @"\\proto-7\VOrbiETD01\2012_3\QC_Shew_12_02_Run-03_26Jul12_Roc_12-04-08\QC_Shew_12_02_Run-03_26Jul12_Roc_12-04-08.raw");

            Assert.IsNotNull(run);

            var msGen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);


            run.CurrentScanSet = new ScanSet(20000);
           

            ScanSetCollection scansetcollection = ScanSetCollection.Create(run, 3000, 3010, 1, 1, true);

            Console.WriteLine("scan\tz\tinfo");
            foreach (var scanSet in scansetcollection.ScanSetList)
            {
                run.CurrentScanSet = scanSet;
                msGen.Execute(run.ResultCollection);
                Console.WriteLine(scanSet.PrimaryScanNumber + "\t" + run.GetMSLevel(scanSet.PrimaryScanNumber) + "\t" + run.GetScanInfo(scanSet.PrimaryScanNumber));
            }

        }

    }
}
