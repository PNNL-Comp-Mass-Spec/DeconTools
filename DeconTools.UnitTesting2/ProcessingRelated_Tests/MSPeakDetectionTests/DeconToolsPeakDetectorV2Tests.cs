using System;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.MSPeakDetectionTests
{
    [TestFixture]
    public class DeconToolsPeakDetectorV2Tests
    {
        public void OldPeakDetectorTest1()
        {

            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSet scan = new ScanSet(6005);


            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsPeakDetector peakDetector = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);
            peakDetector.IsDataThresholded = true;

            run.CurrentScanSet = scan;

            msgen.Execute(run.ResultCollection);

            var peaks= peakDetector.FindPeaks(run.XYData, 481.1, 481.4);

            TestUtilities.DisplayPeaks(peaks);


        }



        public void NewPeakDetectorTest1()
        {

            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSet scan = new ScanSet(6005);




            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsPeakDetectorV2 peakDetector = new DeconToolsPeakDetectorV2();
            peakDetector.PeakToBackgroundRatio = 1.3;
            peakDetector.SignalToNoiseThreshold = 2;
            peakDetector.PeakFitType = Globals.PeakFitType.QUADRATIC;
            peakDetector.IsDataThresholded = true;


            DeconToolsPeakDetector oldPeakDetector = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);
            peakDetector.IsDataThresholded = true;



            run.CurrentScanSet = scan;

            msgen.Execute(run.ResultCollection);

            var peaks = peakDetector.FindPeaks(run.XYData.Xvalues,run.XYData.Yvalues, 481.1, 481.4);

            var oldPeaks = oldPeakDetector.FindPeaks(run.XYData, 481.1, 481.4);


            Assert.AreEqual(oldPeaks.Count, peaks.Count);

            for (int i = 0; i < oldPeaks.Count; i++)
            {
                var oldpeakMz = (decimal) Math.Round(oldPeaks[i].XValue, 4);
                var newPeakMz = (decimal)Math.Round(peaks[i].XValue, 4);

                var oldPeakIntensity = (decimal)Math.Round(oldPeaks[i].Height, 0);
                var newPeakIntensity = (decimal)Math.Round(peaks[i].Height, 0);

                var oldPeakWidth = (decimal)Math.Round(oldPeaks[i].Width, 4);
                var newPeakWidth = (decimal)Math.Round(peaks[i].Width, 4);

                var oldPeakMaxIndex = oldPeaks[i].DataIndex;
                var newPeakMaxIndex = peaks[i].DataIndex;


                Assert.AreEqual(oldpeakMz, newPeakMz);
                Assert.AreEqual(oldPeakIntensity, newPeakIntensity);
                Assert.AreEqual(oldPeakWidth, newPeakWidth);
                Assert.AreEqual(oldPeakMaxIndex, newPeakMaxIndex);
                


            }



            TestUtilities.DisplayPeaks(peaks);

            TestUtilities.DisplayPeaks(oldPeaks);

            //TestUtilities.DisplayXYValues(run.XYData);
        }

        public void NewPeakDetectorTest2()
        {

            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSet scan = new ScanSet(6005);

            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsPeakDetectorV2 peakDetector = new DeconToolsPeakDetectorV2();
            peakDetector.PeakToBackgroundRatio = 1.3;
            peakDetector.SignalToNoiseThreshold = 2;
            peakDetector.PeakFitType = Globals.PeakFitType.QUADRATIC;
            peakDetector.IsDataThresholded = true;


            DeconToolsPeakDetector oldPeakDetector = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);
            peakDetector.IsDataThresholded = true;

            run.CurrentScanSet = scan;
            msgen.Execute(run.ResultCollection);

            var peaks = peakDetector.FindPeaks(run.XYData.Xvalues, run.XYData.Yvalues, 400, 1000);
            var oldPeaks = oldPeakDetector.FindPeaks(run.XYData, 400, 1000);

            StringBuilder sb = new StringBuilder();

            var sourcePeaks = peaks;
            var comparePeaks = oldPeaks;


            foreach (var peak in sourcePeaks)
            {

                sb.Append(peak.DataIndex + "\t" +  peak.XValue + "\t" + peak.Height + "\t" + peak.Width.ToString("0.0000") + "\t");

                 bool found = false;
                 foreach (var oldPeak in comparePeaks)
                {
                   

                    if (Math.Abs(oldPeak.XValue-peak.XValue)<0.01)
                    {
                        bool widthIsDifferent = Math.Abs(oldPeak.Width-peak.Width)>0.001;

                        sb.Append("true" + "\t" + oldPeak.Width.ToString("0.0000") + "\t" + widthIsDifferent);
                        found = true;
                        break;
                    }
                   
                }

                if (!found)
                {
                    sb.Append("FALSE");
                }

                sb.Append(Environment.NewLine);

            }

            Console.WriteLine(sb.ToString());


            //Assert.AreEqual(oldPeaks.Count, peaks.Count);

            //for (int i = 0; i < oldPeaks.Count; i++)
            //{
            //    var oldpeakMz = (decimal)Math.Round(oldPeaks[i].XValue, 4);
            //    var newPeakMz = (decimal)Math.Round(peaks[i].XValue, 4);

            //    var oldPeakIntensity = (decimal)Math.Round(oldPeaks[i].Height, 0);
            //    var newPeakIntensity = (decimal)Math.Round(peaks[i].Height, 0);

            //    var oldPeakWidth = (decimal)Math.Round(oldPeaks[i].Width, 4);
            //    var newPeakWidth = (decimal)Math.Round(peaks[i].Width, 4);

            //    var oldPeakMaxIndex = oldPeaks[i].DataIndex;
            //    var newPeakMaxIndex = peaks[i].DataIndex;


            //    Assert.AreEqual(oldpeakMz, newPeakMz);
            //    Assert.AreEqual(oldPeakIntensity, newPeakIntensity);
            //    Assert.AreEqual(oldPeakWidth, newPeakWidth);
            //    Assert.AreEqual(oldPeakMaxIndex, newPeakMaxIndex);
            //}


            //TestUtilities.DisplayPeaks(peaks);

            //TestUtilities.DisplayPeaks(oldPeaks);



            //TestUtilities.DisplayXYValues(run.XYData);
        }


        public void OldPeakDetectorUsingMatrixBasedFittingRoutineTest1()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSet scan = new ScanSet(6005);




            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsPeakDetectorV2 peakDetector = new DeconToolsPeakDetectorV2();
            peakDetector.PeakToBackgroundRatio = 1.3;
            peakDetector.SignalToNoiseThreshold = 2;
            peakDetector.PeakFitType = Globals.PeakFitType.QUADRATIC;
            peakDetector.IsDataThresholded = true;


            DeconToolsPeakDetector oldPeakDetector = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);
            peakDetector.IsDataThresholded = true;

            run.CurrentScanSet = scan;

            msgen.Execute(run.ResultCollection);

            double mzStart = 585.30;

            double mzStop = 585.35;


            var peaks = peakDetector.FindPeaks(run.XYData.Xvalues, run.XYData.Yvalues, mzStart, mzStop);

            var oldPeaks = oldPeakDetector.FindPeaks(run.XYData, mzStart, mzStop);


            TestUtilities.DisplayPeaks(peaks);

            TestUtilities.DisplayPeaks(oldPeaks);


        }


        public void CalcOfFwhmByOldPeakdetector_Test1()
        {
            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSet scan = new ScanSet(6005);

            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsPeakDetectorV2 peakDetector = new DeconToolsPeakDetectorV2();
            peakDetector.PeakToBackgroundRatio = 1.3;
            peakDetector.SignalToNoiseThreshold = 2;
            peakDetector.PeakFitType = Globals.PeakFitType.QUADRATIC;
            peakDetector.IsDataThresholded = true;


            DeconToolsPeakDetector oldPeakDetector = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);
            peakDetector.IsDataThresholded = true;

            run.CurrentScanSet = scan;

            msgen.Execute(run.ResultCollection);

            double mzStart = 692.8;

            double mzStop = 693.2;


            var peaks = peakDetector.FindPeaks(run.XYData.Xvalues, run.XYData.Yvalues, mzStart, mzStop);

            var oldPeaks = oldPeakDetector.FindPeaks(run.XYData, mzStart, mzStop);


            TestUtilities.DisplayPeaks(peaks);

            TestUtilities.DisplayPeaks(oldPeaks);
        }

    }
}
