using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingTasksTests
{
    [TestFixture]
    public class DeconToolsSavitzkyGolaySmootherTests
    {
   
        [Test]
        public void smootherTest1()
        {
            Run run = new MSScanFromTextFileRun(FileRefs.RawDataMSFiles.TextFileMS_std1);
      
            Task msgen = new GenericMSGenerator();
            msgen.Execute(run.ResultCollection);

            Task peakdetector = new DeconToolsPeakDetector(3, 3, Globals.PeakFitType.QUADRATIC, false);
            peakdetector.Execute(run.ResultCollection);

            Assert.AreEqual(82, run.PeakList.Count);

            Task smoother = new DeconToolsSavitzkyGolaySmoother(3, 3, 2);
            smoother.Execute(run.ResultCollection);

            peakdetector.Execute(run.ResultCollection);

            Assert.AreEqual(67, run.PeakList.Count);
        }



        [Test]
        public void SmoothWithOldDeconToolsSmootherTest1()
        {
            string sampleXYDataFile = FileRefs.TestFileBasePath + "\\" + "sampleXYData1.txt";

            Assert.IsTrue(File.Exists(sampleXYDataFile));

            DeconToolsSavitzkyGolaySmoother smoother = new DeconToolsSavitzkyGolaySmoother(3, 3, 1);

            var xydata =TestUtilities.LoadXYDataFromFile(sampleXYDataFile);
            var smoothedXYData = smoother.Smooth(xydata);


            Assert.AreEqual(xydata.Xvalues.Length , smoothedXYData.Xvalues.Length);


            StringBuilder sb = new StringBuilder();
            sb.Append("xval\tnonSmoothed_Y\tsmoothed_Y\n");
            for (int i = 0; i < xydata.Xvalues.Length; i++)
            {

                sb.Append(xydata.Xvalues[i] + "\t" + xydata.Yvalues[i] + "\t" + smoothedXYData.Yvalues[i] + Environment.NewLine);

            }

            Console.WriteLine(sb.ToString());



        }


        [Test]
        public void SmoothWithNewDeconToolsSmootherTest1()
        {
            string sampleXYDataFile = FileRefs.TestFileBasePath + "\\" + "sampleXYData1.txt";

            Assert.IsTrue(File.Exists(sampleXYDataFile));

            var smoother = new SavitzkyGolaySmoother(7, 2);

            var xydata = TestUtilities.LoadXYDataFromFile(sampleXYDataFile);

            int numSmooths = 2000;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();


            XYData smoothedXYData=new XYData(); 
            for (int i = 0; i < numSmooths; i++ )
            {
                smoothedXYData = smoother.Smooth(xydata);
            }

            stopwatch.Stop();


            Console.WriteLine("Average time for smoothing (milliseconds) = " + stopwatch.ElapsedMilliseconds/(double)numSmooths);

            Assert.AreEqual(xydata.Xvalues.Length, smoothedXYData.Xvalues.Length);


            StringBuilder sb = new StringBuilder();
            sb.Append("xval\tnonSmoothed_Y\tsmoothed_Y\n");
            for (int i = 0; i < xydata.Xvalues.Length; i++)
            {

                sb.Append(xydata.Xvalues[i] + "\t" + xydata.Yvalues[i] + "\t" + smoothedXYData.Yvalues[i] + Environment.NewLine);

            }

           // Console.WriteLine(sb.ToString());



        }



       
    }
}
