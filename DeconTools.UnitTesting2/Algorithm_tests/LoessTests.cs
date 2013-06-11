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

            string testFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\MassAlignmentTesting\scanPpmErrorValues1.txt";

            XYData xydata=  TestUtilities.LoadXYDataFromFile(testFile);



            LoessInterpolator loessInterpolator = new LoessInterpolator();

            var smoothedData= loessInterpolator.Smooth(xydata.Xvalues, xydata.Yvalues);

            for (int i = 0; i < xydata.Xvalues.Length; i++)
            {
                Console.WriteLine(xydata.Xvalues[i] +"\t"+  xydata.Yvalues[i] +  "\t" + smoothedData[i]);
            }
        }


        [Test]
        public void LoessOnNETAlignmentData1()
        {

            string testFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\MassAlignmentTesting\scanToNetTheorData1.txt";

            XYData xydata = TestUtilities.LoadXYDataFromFile(testFile);

            Dictionary<decimal, double> scanNetVals = new Dictionary<decimal, double>();

            for (int i = 0; i < xydata.Xvalues.Length; i++)
            {
                decimal xval =  (decimal) Math.Round(xydata.Xvalues[i],1);
                double yval = xydata.Yvalues[i];

                if (!scanNetVals.ContainsKey(xval))
                {
                    scanNetVals.Add(xval, yval);
                }
                
            
            }

            XYData xyDataForInterpolator = new XYData();
            xyDataForInterpolator.Xvalues = scanNetVals.Keys.Select(p => (double) p).ToArray();
            xyDataForInterpolator.Yvalues = scanNetVals.Values.ToArray();

            double bandwidth = 0.005;
            int robustIter = 4;
            LoessInterpolator loessInterpolator = new LoessInterpolator(bandwidth, robustIter);
            

            var smoothedData = loessInterpolator.Smooth(xyDataForInterpolator.Xvalues, xyDataForInterpolator.Yvalues);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < xyDataForInterpolator.Xvalues.Length; i++)
            {
                sb.Append(xyDataForInterpolator.Xvalues[i] + "\t" + xyDataForInterpolator.Yvalues[i] + "\t" + smoothedData[i]);
                sb.Append(Environment.NewLine);
            }

            Console.WriteLine(sb.ToString());
        }



        [Test]
        public void LoessOnNETAlignmentData2()
        {

            string testFile =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2013\IQ\2013_06_04_Using MSGF for IQ alignment - part2\QCShew_ScanVsTheorNetValues.txt";

            XYData xydata = TestUtilities.LoadXYDataFromFile(testFile);

            Dictionary<decimal, double> scanNetVals = new Dictionary<decimal, double>();

            for (int i = 0; i < xydata.Xvalues.Length; i++)
            {
                decimal xval = (decimal)Math.Round(xydata.Xvalues[i], 1);
                double yval = xydata.Yvalues[i];

                if (!scanNetVals.ContainsKey(xval))
                {
                    scanNetVals.Add(xval, yval);
                }


            }

            XYData xyDataForInterpolator = new XYData();
            xyDataForInterpolator.Xvalues = scanNetVals.Keys.Select(p => (double)p).ToArray();
            xyDataForInterpolator.Yvalues = scanNetVals.Values.ToArray();

            double bandwidth = 0.1;
            int robustIter = 2;
            LoessInterpolator loessInterpolator = new LoessInterpolator(bandwidth, robustIter);


            var smoothedData = loessInterpolator.Smooth(xyDataForInterpolator.Xvalues, xyDataForInterpolator.Yvalues);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < xyDataForInterpolator.Xvalues.Length; i++)
            {
                sb.Append(xyDataForInterpolator.Xvalues[i] + "\t" + xyDataForInterpolator.Yvalues[i] + "\t" + smoothedData[i]);
                sb.Append(Environment.NewLine);
            }

            Console.WriteLine(sb.ToString());
        }




    }
}
