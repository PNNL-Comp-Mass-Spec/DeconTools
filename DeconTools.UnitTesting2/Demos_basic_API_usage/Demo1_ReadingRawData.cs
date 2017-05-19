using System;
using System.Text;
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

            var sb = new StringBuilder();
            sb.Append("scan\tRT\n");

            for (var i = run.MinLCScan; i <= run.MaxLCScan; i++)
            {
                var retentionTime = run.GetTime(i);
                sb.Append(i);
                sb.Append("\t");
                sb.Append(retentionTime.ToString("0.000"));
                sb.Append(Environment.NewLine);
                
            }

            Console.WriteLine(sb.ToString());

        }


    }
}
