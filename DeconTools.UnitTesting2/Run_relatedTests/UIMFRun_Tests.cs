using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
            string fileName = FileRefs.RawDataMSFiles.UIMFStdFile3;

            UIMFRun uimfRun = new UIMFRun(fileName);

            Assert.AreEqual(1, uimfRun.MinFrame);
            Assert.AreEqual(1175, uimfRun.MaxFrame);

            Assert.AreEqual(0, uimfRun.MinScan);
            Assert.AreEqual(359, uimfRun.MaxScan);

            Assert.AreEqual("Sarc_MS2_90_6Apr11_Cheetah_11-02-19", uimfRun.DatasetName);

            string currentUIMFFilePath = Path.GetDirectoryName(fileName);
            Assert.AreEqual(currentUIMFFilePath, uimfRun.DataSetPath);
            Assert.AreEqual(fileName, uimfRun.Filename);

        }

        [Test]
        public void GetNumberOfFramesTest()
        {
            UIMFRun test = new UIMFRun();
            UIMFRun uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);
            int numframes = uimfRun.GetNumFrames();
            int numScans = uimfRun.GetNumMSScans();


            Console.WriteLine("Number of frames = " + numframes);
            Console.WriteLine("Number of scans = " + numScans);
            Assert.AreEqual(1175, numframes);
            Assert.AreEqual(423000, numScans);


        }

        [Test]
        public void getFrameStartTimeTest1()
        {
            UIMFRun run = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            int frameNum = 162;
            double frametime1 = run.GetTime(frameNum);

            Assert.AreEqual(474.3, (decimal) Math.Round(frametime1, 1));

        }

        [Test]
        public void GetMassSpectrumNoSummingTest1()
        {
            UIMFRun run = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            FrameSet frame = new FrameSet(162);
            ScanSet scan = new ScanSet(121);

            run.GetMassSpectrum(frame, scan, 0, 50000);

            double testMZ = 627.2655682;
            int maxIntensityForTestMZ = 0;
            for (int i = 0; i < run.XYData.Xvalues.Length; i++)
            {

                if (run.XYData.Xvalues[i] > (testMZ - 0.1) && run.XYData.Xvalues[i] < (testMZ + 0.1))
                {
                    if (run.XYData.Yvalues[i] > maxIntensityForTestMZ) maxIntensityForTestMZ = (int)run.XYData.Yvalues[i];
                }
            }

            Assert.AreEqual(35845, maxIntensityForTestMZ);
            //TestUtilities.DisplayXYValues(run.XYData);
        }

        [Test]
        public void GetMassSpectrumWithSummingTest1()
        {
            UIMFRun run = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            FrameSet frame = new FrameSet(163,162,164);
            ScanSet scan = new ScanSet(121);

            run.GetMassSpectrum(frame, scan, 0, 50000);

            double testMZ = 627.2655682;
            int maxIntensityForTestMZ = 0;
            for (int i = 0; i < run.XYData.Xvalues.Length; i++)
            {

                if (run.XYData.Xvalues[i] > (testMZ - 0.1) && run.XYData.Xvalues[i] < (testMZ + 0.1))
                {
                    if (run.XYData.Yvalues[i] > maxIntensityForTestMZ) maxIntensityForTestMZ = (int)run.XYData.Yvalues[i];
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
            UIMFRun uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            int testFrame = 162;
            double framePressure = uimfRun.GetFramePressure(testFrame);

            Assert.AreNotEqual(0, framePressure);
            Assert.AreEqual(4.02672m, (Decimal)framePressure);

        }


        [Test]
        public void GetFrameParameters_Test1()
        {
            UIMFRun uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);
            int frameStart = 162;
            int frameStop = 172;
            int numFramesSummed = 3;


            uimfRun.FrameSetCollection = FrameSetCollection.Create(uimfRun, frameStart, frameStop, numFramesSummed, 1);

            uimfRun.GetFrameDataAllFrameSets();

        }



      
        [Test]
        public void getDriftTimeForScanZero()
        {
            //UIMF scan numbers are 0-based.  But first scan should have a drifttime above 0. 

            UIMFRun uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            double driftTime = uimfRun.GetDriftTime(162, 0);
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
            UIMFRun uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);
            int numBins = uimfRun.GetNumBins();
            Assert.AreEqual(148000, numBins);
        }

        
        [Test]
        public void SimpleSpeedTest1()
        {
            UIMFRun uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);
            int numFramesSummed = 1;
            int numScansSummed = 7;
            double minMZ = 300;
            double maxMZ = 2000;


            uimfRun.FrameSetCollection = FrameSetCollection.Create(uimfRun, 300, 310, numFramesSummed, 1);

            uimfRun.ScanSetCollection = ScanSetCollection.Create(uimfRun, 120, 125, numScansSummed, 1);


            List<long> times = new List<long>();
            Stopwatch sw = new Stopwatch();


            foreach (var frame in uimfRun.FrameSetCollection.FrameSetList)
            {
                uimfRun.CurrentFrameSet = frame;
                foreach (var scan in uimfRun.ScanSetCollection.ScanSetList)
                {
                    uimfRun.CurrentScanSet = scan;
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
            UIMFRun uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            int startFrame = 155;
            int stopFrame = 175;
            int startScan = 122;
            int stopScan = 122;


            double targetMZ = 627.27;
            double toleranceInPPM = 25;

            uimfRun.GetChromatogram(startFrame, stopFrame, startScan, stopScan, targetMZ, toleranceInPPM);

            uimfRun.XYData.Display();
        }


        [Test]
        public void getPressureLastFrameTest1()
        {
            UIMFRun uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);

            int lastFrame = uimfRun.MaxFrame;

            double pressureLastFrame = uimfRun.GetFramePressure(lastFrame);


            Console.WriteLine("pressure on last frame = " + pressureLastFrame);

        }


        [Test]
        public void getSmoothedFramePressuresTest1()
        {
            UIMFRun uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);
            int startFrame = 1000;
            int stopFrame = 1175;

            List<double> pressuresBeforeAveraging = new List<double>();
            for (int frame = startFrame; frame <= stopFrame; frame++)
            {
                pressuresBeforeAveraging.Add(uimfRun.GetFramePressure(frame));
                
            }

            uimfRun.FrameSetCollection = FrameSetCollection.Create(uimfRun, startFrame, stopFrame, 1, 1);

            uimfRun.GetFrameDataAllFrameSets();

            uimfRun.SmoothFramePressuresInFrameSets();


            List<double> pressuresAfterAveraging = new List<double>();
            for (int frame = startFrame; frame <= stopFrame; frame++)
            {
                pressuresAfterAveraging.Add(uimfRun.FrameSetCollection.FrameSetList.Where(p => p.PrimaryFrame == frame).First().FramePressure);

            }


            //for (int i = 0; i < pressuresBeforeAveraging.Count; i++)
            //{
            //    Console.WriteLine(pressuresBeforeAveraging[i] + "\t" + pressuresAfterAveraging[i]);
            //}

            Assert.AreEqual(4.0091461, (decimal)uimfRun.FrameSetCollection.FrameSetList.Where(p => p.PrimaryFrame == 1079).First().FramePressure);
            Assert.AreEqual(4.008275, (decimal)uimfRun.FrameSetCollection.FrameSetList.Where(p => p.PrimaryFrame == 1175).First().FramePressure);

        }

    

        [Test]
        public void getSmoothedFramePressuresTest2()
        {
            UIMFRun uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile3);
           
            int startFrame = uimfRun.MaxFrame - 200;
            int stopFrame = uimfRun.MaxFrame;

            uimfRun.FrameSetCollection = FrameSetCollection.Create(uimfRun, startFrame, stopFrame, 1, 1);

            Dictionary<int, double> pressuresBeforeAveraging = new Dictionary<int, double>();
            for (int frame = startFrame; frame <= stopFrame; frame++)
            {
                pressuresBeforeAveraging.Add(frame, uimfRun.GetFramePressure(frame));
            }
            uimfRun.GetFrameDataAllFrameSets();
            uimfRun.SmoothFramePressuresInFrameSets();


            Dictionary<int, double> pressuresAfterAveraging = new Dictionary<int, double>();
            for (int frame = startFrame; frame <= stopFrame; frame++)
            {
                double pressure = uimfRun.FrameSetCollection.FrameSetList.Where(p => p.PrimaryFrame == frame).First().FramePressure;

                pressuresAfterAveraging.Add(frame, pressure);
            }

            StringBuilder sb = new StringBuilder();
            for (int frame = startFrame; frame <= stopFrame; frame++)
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
            string msmsDatafile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\MSMS_Testing\PepMix_MSMS_4msSA.UIMF";

            UIMFRun uimfRun = new UIMFRun(msmsDatafile);

            int numFrames =  uimfRun.GetNumFrames();
            int testFrame = 10;

            int  mslevel= uimfRun.GetMSLevel(testFrame);

            Assert.AreEqual(2, mslevel);



        }


        [Test]
        public void GetMSLevelInfoTest1()
        {
            UIMFRun uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile4);

            Assert.IsTrue(uimfRun.MS1Frames.Count > 0);
            Assert.AreEqual(1175, uimfRun.MS1Frames.Count);
            Assert.IsTrue(!uimfRun.MS1Frames.Contains(26));    //frame 26 is FrameType 3 - calibration frame
            Assert.IsTrue(uimfRun.MS2Frames.Count == 0);
        }


        [Test]
        public void GetMassSpectrumFromMSMSDatasetTest1()
        {
            string msmsDatafile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\MSMS_Testing\PepMix_MSMS_4msSA.UIMF";

            UIMFRun uimfRun = new UIMFRun(msmsDatafile);

            int testFrame = 2;
            int startScan = 1;
            int stopScan = 300;

            var frame = new FrameSet(testFrame);
            var scanset = new ScanSet(150, startScan, stopScan);

            uimfRun.GetMassSpectrum(frame, scanset, 0, 50000);

            Assert.IsNotNull(uimfRun.XYData);
            Assert.IsNotNull(uimfRun.XYData.Xvalues);
            TestUtilities.DisplayXYValues(uimfRun.XYData);


        }

        [Test]
        public void GetClosestMS1SpectrumTest1()
        {

            string msmsDatafile =
               @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\MSMS_Testing\PepMix_MSMS_4msSA.UIMF";

            UIMFRun uimfRun = new UIMFRun(msmsDatafile);

            int testFrame = 5;
            int closestMS1=  uimfRun.GetClosestMS1Frame(testFrame);

            Assert.AreEqual(6, closestMS1);


        }


    }
}
