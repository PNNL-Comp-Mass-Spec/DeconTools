using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Run_relatedTests
{
    [TestFixture]
    public class UIMFRun_Tests
    {
       
       [Test]
        public void getUIMFFileTest1()
        {
            var fileName = FileRefs.RawDataMSFiles.UIMFStdFile3;

            var uimfRun = new UIMFRun(fileName);

            Assert.AreEqual(1, uimfRun.MinLCScan);
            Assert.AreEqual(1175, uimfRun.MaxLCScan);

            Assert.AreEqual(0, uimfRun.MinIMSScan);
            Assert.AreEqual(359, uimfRun.MaxIMSScan);

            Assert.AreEqual("Sarc_MS2_90_6Apr11_Cheetah_11-02-19", uimfRun.DatasetName);

            var currentUIMFFilePath = Path.GetDirectoryName(fileName);
            Assert.AreEqual(currentUIMFFilePath, uimfRun.DataSetPath);
            Assert.AreEqual(fileName, uimfRun.Filename);

        }

        [Test]
        public void GetNumberOfFramesTest()
        {
            var test = new UIMFRun();
            var uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);
            var numframes = uimfRun.GetNumFrames();
            var numScans = uimfRun.GetNumMSScans();


            Console.WriteLine("Number of frames = " + numframes);
            Console.WriteLine("Number of scans = " + numScans);
            Assert.AreEqual(1175, numframes);
            Assert.AreEqual(423000, numScans);


        }

        [Test]
        public void getFrameStartTimeTest1()
        {
            var run = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            var frameNum = 162;
            var frametime1 = run.GetTime(frameNum);

            Assert.AreEqual(474.3, (decimal) Math.Round(frametime1, 1));

        }

        [Test]
        public void GetMassSpectrumNoSummingTest1()
        {
            var run = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            var frame = new ScanSet(162);
            var scan = new IMSScanSet(121);

            var xydata=  run.GetMassSpectrum(frame, scan, 0, 50000);

            var testMZ = 627.2655682;
            var maxIntensityForTestMZ = 0;
            for (var i = 0; i < xydata.Xvalues.Length; i++)
            {

                if (xydata.Xvalues[i] > (testMZ - 0.1) && xydata.Xvalues[i] < (testMZ + 0.1))
                {
                    if (xydata.Yvalues[i] > maxIntensityForTestMZ) maxIntensityForTestMZ = (int)xydata.Yvalues[i];
                }
            }

            Assert.AreEqual(35845, maxIntensityForTestMZ);
            //TestUtilities.DisplayXYValues(xydata);
        }

        [Test]
        public void GetMassSpectrumWithSummingTest1()
        {
            var run = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            var frame = new ScanSet(163,162,164);
            var scan = new IMSScanSet(121);

            var xydata=  run.GetMassSpectrum(frame, scan, 0, 50000);

            var testMZ = 627.2655682;
            var maxIntensityForTestMZ = 0;

            
            for (var i = 0; i < xydata.Xvalues.Length; i++)
            {

                if (xydata.Xvalues[i] > (testMZ - 0.1) && xydata.Xvalues[i] < (testMZ + 0.1))
                {
                    if (xydata.Yvalues[i] > maxIntensityForTestMZ) maxIntensityForTestMZ = (int)xydata.Yvalues[i];
                }
            }

            Assert.AreEqual(126568, maxIntensityForTestMZ);
            
        }


        [Test]
        public void InitializeUIMFContainingMSMSDataTest1()
        {
           // var runContainsMSMS = new UIMFRun(FileRefs.RawDataMSFiles.UIMFFileContainingMSMSLevelData);
            var runNoMSMS = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            //Assert.IsTrue(runContainsMSMS.ContainsMSMSData);
            Assert.IsFalse(runNoMSMS.ContainsMSMSData);
        }


        [Test]
        public void getFramePressureTest1()
        {
            var uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            var testFrame = 162;
            var framePressure = uimfRun.GetFramePressure(testFrame);

            Assert.AreNotEqual(0, framePressure);
            Assert.AreEqual(4.02672m, (Decimal)framePressure);

        }


        [Test]
        public void GetFrameParameters_Test1()
        {
            var uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);
            var frameStart = 162;
            var frameStop = 172;
            var numFramesSummed = 3;


            uimfRun.ScanSetCollection .Create(uimfRun, frameStart, frameStop, numFramesSummed, 1);

            uimfRun.GetFrameDataAllFrameSets();

        }



      
        [Test]
        public void getDriftTimeForScanZero()
        {
            //UIMF scan numbers are 0-based.  But first scan should have a drifttime above 0. 

            var uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            var driftTime = uimfRun.GetDriftTime(162, 0);
            Assert.Greater((decimal)driftTime, 0);
        }

        [Test]
        public void GetMSLevelTest1()
        {
            Run run = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);
            Assert.AreEqual(1, run.GetMSLevel(233));
            Assert.AreEqual(1, run.GetMSLevel(234));
            Assert.AreEqual(1, run.GetMSLevel(1173));
        }

        [Test]
        public void GetNumBinsTest1()
        {
            var uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);
            var numBins = uimfRun.GetNumBins();
            Assert.AreEqual(148000, numBins);
        }

        
        [Test]
        public void SimpleSpeedTest1()
        {
            var uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);
            var numFramesSummed = 1;
            var numScansSummed = 7;
            double minMZ = 300;
            double maxMZ = 2000;


            uimfRun.ScanSetCollection.Create(uimfRun, 300, 310, numFramesSummed, 1);

            uimfRun.IMSScanSetCollection.Create(uimfRun, 120, 125, numScansSummed, 1);


            var times = new List<long>();
            var sw = new Stopwatch();


            foreach (var frame in uimfRun.ScanSetCollection.ScanSetList)
            {
                uimfRun.CurrentScanSet = frame;
                foreach (IMSScanSet scan in uimfRun.IMSScanSetCollection.ScanSetList)
                {
                    uimfRun.CurrentIMSScanSet = scan;
                    sw.Start();
                    uimfRun.GetMassSpectrum(frame, scan, minMZ, maxMZ);
                    sw.Stop();

                    times.Add(sw.ElapsedMilliseconds);
                    sw.Reset();

                }

            }

            Console.WriteLine("NumMSScans= " + times.Count);
            Console.WriteLine("Time (ms) per scan = " + times.Average());



        }



        [Test]
        public void getLCProfileTest1()
        {
            var uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            var startFrame = 155;
            var stopFrame = 175;
            var startScan = 122;
            var stopScan = 122;


            var targetMZ = 627.27;
            double toleranceInPPM = 25;

           var chromXYData=   uimfRun.GetChromatogram(startFrame, stopFrame, startScan, stopScan, targetMZ, toleranceInPPM);

           chromXYData.Display();
            //TODO: actually test something
        }



        [Test]
        public void getDriftTimeProfile()
        {
            var uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            var frame = 163;
           
            var startScan = 100;
            var stopScan = 140;


            var targetMZ = 627.27;
            double toleranceInPPM = 5;

            var chromXYData = uimfRun.GetDriftTimeProfile(frame, startScan, stopScan, targetMZ, toleranceInPPM);

            chromXYData.Display();
            //TODO: actually test something
        }





        [Test]
        public void getPressureLastFrameTest1()
        {
            var uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            var lastFrame = uimfRun.MaxLCScan;

            var pressureLastFrame = uimfRun.GetFramePressure(lastFrame);


            Console.WriteLine("pressure on last frame = " + pressureLastFrame);

        }


        [Test]
        public void getSmoothedFramePressuresTest1()
        {
            var uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);
            var startFrame = 1000;
            var stopFrame = 1175;

            var pressuresBeforeAveraging = new List<double>();
            for (var frame = startFrame; frame <= stopFrame; frame++)
            {
                pressuresBeforeAveraging.Add(uimfRun.GetFramePressure(frame));
                
            }

            uimfRun.ScanSetCollection .Create(uimfRun, startFrame, stopFrame, 1, 1);

            uimfRun.GetFrameDataAllFrameSets();

            uimfRun.SmoothFramePressuresInFrameSets();


            var pressuresAfterAveraging = new List<double>();
            for (var frame = startFrame; frame <= stopFrame; frame++)
            {
                var lcScanset = (LCScanSetIMS) uimfRun.ScanSetCollection.ScanSetList.First(p => p.PrimaryScanNumber == frame);
                pressuresAfterAveraging.Add(lcScanset.FramePressureSmoothed);
            }

            var testScanset1 = (LCScanSetIMS) uimfRun.ScanSetCollection.ScanSetList.First(p => p.PrimaryScanNumber == 1079);

            var testScanset2 =(LCScanSetIMS) uimfRun.ScanSetCollection.ScanSetList.First(p => p.PrimaryScanNumber == 1175);

            Assert.AreEqual(4.0091461, (decimal)testScanset1.FramePressureSmoothed);
            Assert.AreEqual(4.008275, (decimal)testScanset2.FramePressureSmoothed);

        }

    

        [Test]
        public void getSmoothedFramePressuresTest2()
        {
            var uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);
           
            var startFrame = uimfRun.MaxLCScan - 200;
            var stopFrame = uimfRun.MaxLCScan;

            uimfRun.ScanSetCollection .Create(uimfRun, startFrame, stopFrame, 1, 1);

            var pressuresBeforeAveraging = new Dictionary<int, double>();
            for (var frame = startFrame; frame <= stopFrame; frame++)
            {
                pressuresBeforeAveraging.Add(frame, uimfRun.GetFramePressure(frame));
            }
            uimfRun.GetFrameDataAllFrameSets();
            uimfRun.SmoothFramePressuresInFrameSets();


            var pressuresAfterAveraging = new Dictionary<int, double>();
            for (var frame = startFrame; frame <= stopFrame; frame++)
            {
                var scanset =(LCScanSetIMS) uimfRun.ScanSetCollection.ScanSetList.First(p => p.PrimaryScanNumber == frame);

                var pressure = scanset.FramePressureSmoothed;

                pressuresAfterAveraging.Add(frame, pressure);
            }

            var sb = new StringBuilder();
            for (var frame = startFrame; frame <= stopFrame; frame++)
            {
                sb.Append(frame);
                sb.Append("\t");
                sb.Append(pressuresBeforeAveraging[frame]);
                sb.Append("\t");
                sb.Append(pressuresAfterAveraging[frame]);
                sb.Append(Environment.NewLine);
            }

            //Console.WriteLine(sb.ToString());

            Assert.AreEqual(4.008936m, (decimal)pressuresAfterAveraging[1100]);




        }


        [Test]
        public void GetMSLevelFromMSMSDatasetTest1()
        {
            var msmsDatafile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\MSMS_Testing\PepMix_MSMS_4msSA.UIMF";

            var uimfRun = new UIMFRun(msmsDatafile);

            var numFrames =  uimfRun.GetNumFrames();
            var testFrame = 10;

            var  mslevel= uimfRun.GetMSLevel(testFrame);

            Assert.AreEqual(2, mslevel);



        }


        [Test]
        public void GetMSLevelInfoTest1()
        {
            var uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile4);

            Assert.IsTrue(uimfRun.MS1Frames.Count > 0);
            Assert.AreEqual(1175, uimfRun.MS1Frames.Count);
            Assert.IsTrue(!uimfRun.MS1Frames.Contains(26));    //frame 26 is FrameType 3 - calibration frame
            Assert.IsTrue(uimfRun.MS2Frames.Count == 0);
        }


        [Test]
        public void GetMassSpectrumFromMSMSDatasetTest1()
        {
            var msmsDatafile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\MSMS_Testing\PepMix_MSMS_4msSA.UIMF";

            var uimfRun = new UIMFRun(msmsDatafile);

            var testFrame = 2;
            var startScan = 1;
            var stopScan = 300;

            var frame = new ScanSet(testFrame);
            var scanset = new ScanSet(150, startScan, stopScan);

            var xydata=uimfRun.GetMassSpectrum(frame, scanset, 0, 50000);

            Assert.IsNotNull(xydata);
            Assert.IsNotNull(xydata.Xvalues);
            TestUtilities.DisplayXYValues(xydata);


        }

        [Test]
        public void GetClosestMS1SpectrumTest1()
        {

            var msmsDatafile =
               @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\MSMS_Testing\PepMix_MSMS_4msSA.UIMF";

            var uimfRun = new UIMFRun(msmsDatafile);

            var testFrame = 5;
            var closestMS1=  uimfRun.GetClosestMS1Frame(testFrame);

            Assert.AreEqual(6, closestMS1);


        }


    }
}
