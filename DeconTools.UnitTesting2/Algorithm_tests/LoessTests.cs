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
        public void LoessOnMassAlignmentData1()
        {

            var testFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\MassAlignmentTesting\scanPpmErrorValues1.txt";

            var xyData=  TestUtilities.LoadXYDataFromFile(testFile);



            var loessInterpolator = new LoessInterpolator();

            var smoothedData= loessInterpolator.Smooth(xyData.Xvalues, xyData.Yvalues);

            for (var i = 0; i < xyData.Xvalues.Length; i++)
            {
                Console.WriteLine(xyData.Xvalues[i] +"\t"+  xyData.Yvalues[i] +  "\t" + smoothedData[i]);
            }
        }


        [Test]
        public void LoessOnNETAlignmentData1()
        {

            var testFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\MassAlignmentTesting\scanToNetTheorData1.txt";

            var xyData = TestUtilities.LoadXYDataFromFile(testFile);

            var scanNetVals = new Dictionary<decimal, double>();

            for (var i = 0; i < xyData.Xvalues.Length; i++)
            {
                var xval =  (decimal) Math.Round(xyData.Xvalues[i],1);
                var yval = xyData.Yvalues[i];

                if (!scanNetVals.ContainsKey(xval))
                {
                    scanNetVals.Add(xval, yval);
                }
                
            
            }

            var xyDataForInterpolator = new XYData();
            xyDataForInterpolator.Xvalues = scanNetVals.Keys.Select(p => (double) p).ToArray();
            xyDataForInterpolator.Yvalues = scanNetVals.Values.ToArray();

            var bandwidth = 0.005;
            var robustIter = 4;
            var loessInterpolator = new LoessInterpolator(bandwidth, robustIter);
            

            var smoothedData = loessInterpolator.Smooth(xyDataForInterpolator.Xvalues, xyDataForInterpolator.Yvalues);

            var sb = new StringBuilder();

            for (var i = 0; i < xyDataForInterpolator.Xvalues.Length; i++)
            {
                sb.Append(xyDataForInterpolator.Xvalues[i] + "\t" + xyDataForInterpolator.Yvalues[i] + "\t" + smoothedData[i]);
                sb.Append(Environment.NewLine);
            }

            Console.WriteLine(sb.ToString());
        }



        [Test]
        [Ignore("Local testing only")]
        public void LoessOnNETAlignmentData2()
        {

            var testFile =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2013\IQ\2013_06_04_Using MSGF for IQ alignment - part2\QCShew_ScanVsTheorNetValues.txt";

            var xyData = TestUtilities.LoadXYDataFromFile(testFile);

            var scanNetVals = new Dictionary<decimal, double>();

            for (var i = 0; i < xyData.Xvalues.Length; i++)
            {
                var xval = (decimal)Math.Round(xyData.Xvalues[i], 1);
                var yval = xyData.Yvalues[i];

                if (!scanNetVals.ContainsKey(xval))
                {
                    scanNetVals.Add(xval, yval);
                }


            }

            var xyDataForInterpolator = new XYData();
            xyDataForInterpolator.Xvalues = scanNetVals.Keys.Select(p => (double)p).ToArray();
            xyDataForInterpolator.Yvalues = scanNetVals.Values.ToArray();

            var bandwidth = 0.1;
            var robustIter = 2;
            var loessInterpolator = new LoessInterpolator(bandwidth, robustIter);


            var smoothedData = loessInterpolator.Smooth(xyDataForInterpolator.Xvalues, xyDataForInterpolator.Yvalues);

            var sb = new StringBuilder();

            for (var i = 0; i < xyDataForInterpolator.Xvalues.Length; i++)
            {
                sb.Append(xyDataForInterpolator.Xvalues[i] + "\t" + xyDataForInterpolator.Yvalues[i] + "\t" + smoothedData[i]);
                sb.Append(Environment.NewLine);
            }

            Console.WriteLine(sb.ToString());
        }




    }
}
