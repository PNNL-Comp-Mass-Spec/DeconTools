using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Demos_basic_API_usage
{
    [TestFixture]
    public class Demo1_ReadingRawData
    {
        [Test]
        public void Test01_getRetentionTimesFromOrbitrapData()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            Console.WriteLine("{0}\t{1}", "Scan", "RT");

            for (var i = run.MinLCScan; i <= run.MaxLCScan; i++)
            {
                var retentionTime = run.GetTime(i);
                if (i % 100 == 0)
                {
                    Console.WriteLine("{0}\t{1:F3}", i, retentionTime);
                }
            }
        }
    }
}
