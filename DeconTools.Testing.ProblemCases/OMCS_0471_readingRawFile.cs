using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.UnitTesting2;
using NUnit.Framework;

namespace DeconTools.Testing.ProblemCases
{
    [TestFixture]
    public class OMCS_0471_readingRawFile
    {
        [Test]
        public void Test1()
        {

            var run =
                new RunFactory().CreateRun(
                    @"\\proto-7\VOrbiETD01\2012_3\Click_chemistry_labelfree_4Jul12_Lynx_12-2-32\Click_chemistry_labelfree_4Jul12_Lynx_12-2-32.raw");

            Assert.IsNotNull(run);

            var msGen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);


            run.CurrentScanSet = new ScanSet(20000);
            msGen.Execute(run.ResultCollection);

            ScanSetCollection scansetcollection = ScanSetCollection.Create(run,10000,11000, 1, 1, true);

            Console.WriteLine("scan\tz\tinfo");
            foreach (var scanSet in scansetcollection.ScanSetList)
            {
                run.CurrentScanSet = scanSet;

                Console.WriteLine(scanSet.PrimaryScanNumber + "\t"+ run.GetMSLevel(scanSet.PrimaryScanNumber) + "\t"+  run.GetScanInfo(scanSet.PrimaryScanNumber) );
            }

           // TestUtilities.DisplayXYValues(run.XYData);

        }

    }
}
