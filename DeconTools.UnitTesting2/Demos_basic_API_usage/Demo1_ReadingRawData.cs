using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;

namespace DeconTools.UnitTesting2.Demos_basic_API_usage
{
    [TestFixture]
    public class Demo1_ReadingRawData
    {
        [Test]
        public void Test01_getRetentionTimesFromOrbitrapData()
        {
            Run run = new XCaliburRun(FileRefs.OrbitrapStdFile1);

            StringBuilder sb = new StringBuilder();
            sb.Append("scan\tRT\n");

            for (int i = run.MinScan; i <= run.MaxScan; i++)
            {
                double retentionTime = run.GetTime(i);
                sb.Append(i);
                sb.Append("\t");
                sb.Append(retentionTime.ToString("0.000"));
                sb.Append(Environment.NewLine);
                
            }

            Console.WriteLine(sb.ToString());

        }


    }
}
