using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.UnitTesting
{
    [TestFixture]
    public class UtilityTests
    {
        [Test]
        public void getAssemblyVersionInfoTest1()
        {
            string versionString = AssemblyInfoRetriever.GetVersion(typeof(Task));
            Console.WriteLine("Version = " + versionString);
            Assert.Greater(versionString.Length, 10);
        }




        public int Search(int[] data, int key, int left, int right)
        {
            if (left <= right)
            {
                int middle = (left + right) / 2;
                if (key == data[middle])
                    return middle;
                else if (key < data[middle])
                    return Search(data, key, left, middle - 1);
                else
                    return Search(data, key, middle + 1, right);
            }
            return -1;
        }

        public int Search(double[] data, double key, int left, int right, double tolerance)
        {
            counter++;
            if (left <= right)
            {
                int middle = (left + right) / 2;
                if (Math.Abs(key-data[middle])<=tolerance)
                    return middle;
                else if (key < data[middle])
                    return Search(data, key, left, middle - 1,tolerance);
                else
                    return Search(data, key, middle + 1, right,tolerance);
            }
            return -1;
        }


        public int counter = 0;

        [Test]
        public void test1()
        {

            List<double> testVals = new List<double>();
            Random rnd = new Random();

            for (int i = 0; i < 100000; i++)
            {
                testVals.Add(rnd.NextDouble() * 100d);

            }

            testVals.Sort();

            double[] data = testVals.ToArray();

            
            double target = 23.3;
            double tolerance = 1;
            int idx = Search(data, target, 0, data.Length, tolerance);



            Console.WriteLine(idx);
            Console.WriteLine(counter);

        }


        [Test]
        public void displayMSLevelsTest()
        {
            Run run = new XCaliburRun(@"\\n2.emsl.pnl.gov\dmsarch\LTQ_FT1_3\47066_HPIm_15mM_24h_A_H1197_251007_11Sep08_Roc_08-03-17\47066_HPIm_15mM_24h_A_H1197_251007_11Sep08_Roc_08-03-17.RAW");

            TestUtilities.DisplayMSLevelData(run);

        }





    }
}
