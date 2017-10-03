using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
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

            var peakdetector = new DeconToolsPeakDetectorV2(3, 3, Globals.PeakFitType.QUADRATIC, true);
            peakdetector.Execute(run.ResultCollection);

            Assert.AreEqual(84, run.PeakList.Count);

            Task smoother = new SavitzkyGolaySmoother(7, 2,false);
            smoother.Execute(run.ResultCollection);

            peakdetector.Execute(run.ResultCollection);

            Assert.AreEqual(46, run.PeakList.Count);
        }



        [Test]
        public void SmoothWithOldDeconToolsSmootherTest1()
        {
            var sampleXYDataFile = Path.Combine(FileRefs.TestFileBasePath, "sampleXYData1.txt");

            Assert.IsTrue(File.Exists(sampleXYDataFile));

            var smoother = new SavitzkyGolaySmoother(3, 3, true);

            var xydata =TestUtilities.LoadXYDataFromFile(sampleXYDataFile);
            var smoothedXYData = smoother.Smooth(xydata);


            Assert.AreEqual(xydata.Xvalues.Length , smoothedXYData.Xvalues.Length);


            var sb = new StringBuilder();
            sb.Append("xval\tnonSmoothed_Y\tsmoothed_Y\n");
            for (var i = 0; i < xydata.Xvalues.Length; i++)
            {

                sb.Append(xydata.Xvalues[i] + "\t" + xydata.Yvalues[i] + "\t" + smoothedXYData.Yvalues[i] + Environment.NewLine);

            }

            Console.WriteLine(sb.ToString());



        }


        [Test]
        public void SmoothWithNewDeconToolsSmootherTest1()
        {
            var sampleXYDataFile = Path.Combine(FileRefs.TestFileBasePath, "sampleXYData1.txt");

            Assert.IsTrue(File.Exists(sampleXYDataFile));

            var smoother = new SavitzkyGolaySmoother(7, 2);

            var xydata = TestUtilities.LoadXYDataFromFile(sampleXYDataFile);

            var numSmooths = 2000;

            var stopwatch = new Stopwatch();
            stopwatch.Start();


            var smoothedXYData=new XYData(); 
            for (var i = 0; i < numSmooths; i++ )
            {
                smoothedXYData = smoother.Smooth(xydata);
            }

            stopwatch.Stop();


            Console.WriteLine("Average time for smoothing (milliseconds) = " + stopwatch.ElapsedMilliseconds/(double)numSmooths);

            Assert.AreEqual(xydata.Xvalues.Length, smoothedXYData.Xvalues.Length);


            var sb = new StringBuilder();
            sb.Append("xval\tnonSmoothed_Y\tsmoothed_Y\n");
            for (var i = 0; i < xydata.Xvalues.Length; i++)
            {

                sb.Append(xydata.Xvalues[i] + "\t" + xydata.Yvalues[i] + "\t" + smoothedXYData.Yvalues[i] + Environment.NewLine);

            }

           // Console.WriteLine(sb.ToString());



        }



       
    }
}
