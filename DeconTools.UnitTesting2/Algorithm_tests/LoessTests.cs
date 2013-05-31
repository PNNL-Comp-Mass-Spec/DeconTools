using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Algorithms;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Algorithm_tests
{
    [TestFixture]
    public class LoessTests
    {
        [Test]
        public void Test1()
        {

            string testFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\MassAlignmentTesting\scanPpmErrorValues1.txt";

            XYData xydata=  TestUtilities.LoadXYDataFromFile(testFile);



            LoessInterpolator loessInterpolator = new LoessInterpolator();

            var smoothedData= loessInterpolator.Smooth(xydata.Xvalues, xydata.Yvalues);

            for (int i = 0; i < xydata.Xvalues.Length; i++)
            {
                Console.WriteLine(xydata.Xvalues[i] +"\t"+  xydata.Yvalues[i] +  "\t" + smoothedData[i]);
            }
        }

    }
}
