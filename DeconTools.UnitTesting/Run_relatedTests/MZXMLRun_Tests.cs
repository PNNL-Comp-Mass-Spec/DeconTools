using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.UnitTesting.Run_relatedTests
{
    [TestFixture]
    public class MZXMLRun_Tests
    {
        private string mzxmlfile1 = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_Scans6000-7000.mzXML";
    


        [Test]
        public void GetSpectrumTest1()
        {
            int scanNum = 68;    // this corresponds to scan 6067 of the original RAW data file
            
            Run run = new MZXMLRun(mzxmlfile1);
            run.GetMassSpectrum(new ScanSet(scanNum), 200, 2000);

            int numscans = run.GetNumMSScans();
            Assert.AreEqual(1001, numscans);

            float[] xvals = new float[1];
            float[] yvals = new float[1];
            run.XYData.GetXYValuesAsSingles(ref xvals, ref yvals);


            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < xvals.Length; i++)
            {
                sb.Append(Convert.ToDouble(xvals[i]));
                sb.Append("\t");
                sb.Append(Convert.ToDouble(yvals[i]));
                sb.Append(Environment.NewLine);
            }
            Console.Write(sb.ToString());

            Assert.AreEqual(27053, xvals.Length);
            Assert.AreEqual(27053, yvals.Length);
            Assert.AreEqual(745.871520996094,(decimal)run.XYData.Xvalues[18082]);
            Assert.AreEqual(12589364, (decimal)run.XYData.Yvalues[18082]);

            Assert.AreEqual(1, run.GetMSLevel(scanNum));


        }

        [Test]
        [ExpectedException(ExpectedMessage = "Summing is not yet supported for MZXML files.")]
        public void GetSummedSpectrumTest1()
        {
            int scanNum = 68;    // this corresponds to scan 6067 of the original RAW data file

            Run run = new MZXMLRun(mzxmlfile1);
            run.GetMassSpectrum(new ScanSet(scanNum,scanNum-1,scanNum+1), 200, 2000);
        }


        [Test]
        public void GetSpectrumTypesTest1()
        {
            Run run = new MZXMLRun(mzxmlfile1);
            Assert.AreEqual(1, run.GetMSLevel(68));
            Assert.AreEqual(2, run.GetMSLevel(69));
            Assert.AreEqual(2, run.GetMSLevel(70));
            Assert.AreEqual(2, run.GetMSLevel(71));
        }

        [Test]
        public void ConstructorTest1()
        {
            Run run = new MZXMLRun(mzxmlfile1);

            Assert.AreEqual(1, run.MinScan);
            Assert.AreEqual(1000, run.MaxScan);

            run = null;
        }

        [Test]
        public void ConstructorTest2()
        {
            Run run = new MZXMLRun(mzxmlfile1,100,300);

            Assert.AreEqual(100, run.MinScan);
            Assert.AreEqual(300, run.MaxScan);

            run = null;
        }


        [Test]
        public void GetTICTest1()
        {
            int scanNum = 68; 

            Run run = new MZXMLRun(mzxmlfile1, 100, 300);

            run.GetMassSpectrum(new ScanSet(scanNum), 800, 2000);

            float ticVal = run.GetTIC(400, 2000);

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            ticVal = run.GetTIC(400, 2000);
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);

            Assert.AreEqual(3558996000, (decimal)ticVal);
        }


    }
}
