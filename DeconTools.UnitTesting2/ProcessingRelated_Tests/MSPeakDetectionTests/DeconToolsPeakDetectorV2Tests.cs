using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
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

            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSet scan = new ScanSet(6005);


            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsPeakDetector peakDetector = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);
            peakDetector.IsDataThresholded = true;

            run.CurrentScanSet = scan;

            msgen.Execute(run.ResultCollection);

            var peaks = peakDetector.FindPeaks(run.XYData, 481.1, 481.4);

            TestUtilities.DisplayPeaks(peaks);


        }

        [Test]
        public void NewPeakDetectorTest2()
        {

            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSet scan = new ScanSet(6005);

            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsPeakDetectorV2 peakDetector = new DeconToolsPeakDetectorV2();
            peakDetector.PeakToBackgroundRatio = 1.3;
            peakDetector.SignalToNoiseThreshold = 2;
            peakDetector.PeakFitType = Globals.PeakFitType.QUADRATIC;
            peakDetector.IsDataThresholded = true;

            run.CurrentScanSet = scan;
            msgen.Execute(run.ResultCollection);

            var peaks = peakDetector.FindPeaks(run.XYData.Xvalues, run.XYData.Yvalues, 400, 1000);
            var expectedPeaks = new List<Peak>();

            // 3 most abundant peaks
            expectedPeaks.Add(new Peak(481.274, 13084440F, 0.0079F));
            expectedPeaks.Add(new Peak(639.822, 9063344F, 0.0119F));
            expectedPeaks.Add(new Peak(579.63, 7917913F, 0.0104F));

            // 3 medium abundance peaks
            expectedPeaks.Add(new Peak(907.961, 416092.1F, 0.0187F));
            expectedPeaks.Add(new Peak(607.305, 415606.4F, 0.0114F));
            expectedPeaks.Add(new Peak(515.945, 415311F, 0.0086F));

            // 3 least abundant peaks
            expectedPeaks.Add(new Peak(861.366, 199058.3F, 0.0192F));
            expectedPeaks.Add(new Peak(479.754, 198565.2F, 0.0071F));
            expectedPeaks.Add(new Peak(935.447, 198530.2F, 0.0206F));

            StringBuilder sb = new StringBuilder();

            sb.Append("Looking for " + expectedPeaks.Count + " expected peaks" + Environment.NewLine);
            sb.Append("Index\tXVal    \tYVal    \tWidth" + Environment.NewLine);

            // Look for each expected peak in the actual peaks
            foreach (var expectedPeak in (from peak in expectedPeaks orderby peak.XValue select peak))
            {
                bool found = false;
                foreach (var peak in peaks)
                {
                    if (Math.Abs(expectedPeak.XValue - peak.XValue) < 0.01)
                    {
                        bool widthIsDifferent = Math.Abs(expectedPeak.Width - peak.Width) > 0.001;

                        sb.Append(peak.DataIndex + "\t" + peak.XValue.ToString("0.000").PadRight(8) + "\t" + peak.Height.ToString("0").PadRight(8) + "\t" + peak.Width.ToString("0.0000") + Environment.NewLine);
                        found = true;
                        break;
                    }

                }

                if (!found)
                {
                    sb.Append("Expected peak not found: ");
                    sb.Append(expectedPeak.DataIndex + "\t" + expectedPeak.XValue + "\t" + expectedPeak.Height + "\t" + expectedPeak.Width.ToString("0.0000"));
                }

                Assert.IsTrue(found);
            }

            Console.WriteLine(sb.ToString());

            // Console.WriteLine("Full peak list");
            // TestUtilities.DisplayPeaks(peaks);
        
        }

        [Test]
        public void NewPeakDetectorExecuteTest1()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSet scan = new ScanSet(6005);

            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsPeakDetectorV2 peakDetector = new DeconToolsPeakDetectorV2();
            peakDetector.PeakToBackgroundRatio = 1.3;
            peakDetector.SignalToNoiseThreshold = 2;
            peakDetector.PeakFitType = Globals.PeakFitType.QUADRATIC;
            peakDetector.IsDataThresholded = true;

            run.CurrentScanSet = scan;

            msgen.Execute(run.ResultCollection);
            peakDetector.Execute(run.ResultCollection);

            Assert.IsTrue(run.CurrentBackgroundIntensity > 0);
            Assert.AreEqual(run.CurrentBackgroundIntensity, peakDetector.BackgroundIntensity);

        }

        public void CheckBackgroundIntensityTest1()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

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

            peakDetector.Execute(run.ResultCollection);
            oldPeakDetector.Execute(run.ResultCollection);

            //Assert.AreEqual(peakDetector.BackgroundIntensity, oldPeakDetector.BackgroundIntensity);

        }

        public void OldPeakDetectorUsingMatrixBasedFittingRoutineTest1()
        {
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

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
            Run run = new XCaliburRun2(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

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

        [Test]
        public void PeakDetectorOnCentroidedDataTest1()
        {
            Run run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSet scan = new ScanSet(6006);   //this is a centroided MS2 scan


            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var peakDetector = new DeconToolsPeakDetectorV2(0, 0, Globals.PeakFitType.QUADRATIC, true);
            peakDetector.RawDataType = Globals.RawDataType.Centroided;
            peakDetector.IsDataThresholded = true;

            run.CurrentScanSet = scan;

            msgen.Execute(run.ResultCollection);

            peakDetector.Execute(run.ResultCollection);

            Console.WriteLine("Num XY datapoints in mass spectrum= " + run.XYData.Xvalues.Length);
            Console.WriteLine("numPeaks = " + run.PeakList.Count);

            Assert.AreEqual(run.XYData.Xvalues.Length, run.PeakList.Count);
            //TestUtilities.DisplayXYValues(run.XYData);
            //TestUtilities.DisplayPeaks(run.PeakList);


        }

        [Test]
        public void PeakDetectorOnCentroidedDataTest2()
        {
            Run run = new RunFactory().CreateRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            ScanSet scan = new ScanSet(6006);   //this is a MS2 scan


            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            
            // Note that setting the PeakFitType to Apex will not matter since RawDataType is Centroided
            // It would only matter if the RawDataType was Profile

            var peakDetector = new DeconToolsPeakDetectorV2(0, 0, Globals.PeakFitType.APEX, true);
            peakDetector.RawDataType = Globals.RawDataType.Centroided;
            peakDetector.SignalToNoiseThreshold = 0;
            peakDetector.PeakToBackgroundRatio = 0;
            peakDetector.IsDataThresholded = true;

            run.CurrentScanSet = scan;

            msgen.Execute(run.ResultCollection);

            peakDetector.Execute(run.ResultCollection);

            Console.WriteLine("Num XY datapoints in mass spectrum= " + run.XYData.Xvalues.Length);
            Console.WriteLine("numPeaks = " + run.PeakList.Count);

            Assert.AreEqual(run.XYData.Xvalues.Length, run.PeakList.Count);
            //TestUtilities.DisplayXYValues(run.XYData);
            //TestUtilities.DisplayPeaks(run.PeakList);


        }
    }
}
