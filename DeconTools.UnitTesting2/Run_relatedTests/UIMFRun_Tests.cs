using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using UIMFLibrary;
using System.Diagnostics;
using DeconTools.Backend.Utilities;
using DeconTools.UnitTesting2;
using System.IO;
using DeconTools.Backend;


namespace DeconTools.UnitTesting.Run_relatedTests
{
    [TestFixture]
    public class UIMFRun_Tests
    {
        private string uimfFilepath = FileRefs.RawDataMSFiles.UIMFStdFile1;
        private string uimfFileContainingMSMSScans = FileRefs.RawDataMSFiles.UIMFFileContainingMSMSLevelData;

        [Test]
        public void getUIMFFileTest1()
        {
            UIMFRun uimfRun = new UIMFRun(uimfFilepath);

            Assert.AreEqual(1, uimfRun.MinFrame);
            Assert.AreEqual(1950, uimfRun.MaxFrame);

            Assert.AreEqual(0, uimfRun.MinScan);
            Assert.AreEqual(499, uimfRun.MaxScan);

            Assert.AreEqual("35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000", uimfRun.DatasetName);

            string currentUIMFFilePath = Path.GetDirectoryName(uimfFilepath);
            Assert.AreEqual(currentUIMFFilePath, uimfRun.DataSetPath);
            Assert.AreEqual(uimfFilepath, uimfRun.Filename);

        }

        [Test]
        public void checkSummingOfMS1LevelData_inFileContainingMSMSData()
        {
            UIMFRun uimfRun = new UIMFRun(uimfFileContainingMSMSScans);


            //int[] frameArray = {1,5,9,13,17};


            //FrameSet frameSet = new FrameSet(9, frameArray);
            //ScanSet scanSet = new ScanSet(300, 100, 500);
            //uimfRun.GetMassSpectrum(scanSet, frameSet, 0, 50000);


            //UIMFLibrary.DataReader reader = new DataReader();
            //reader.OpenUIMF(uimfFileContainingMSMSScans);

            //GlobalParameters gp= reader.GetGlobalParameters();

            //int numBins = gp.Bins;

            //XYData xydata = new XYData();

            //double[] xvals = new double[numBins];
            //int[] yvals = new int[numBins];

            //List<double> tempXvalues = new List<double>();
            //List<double> tempYValues = new List<double>();

            //reader.SumScansRange(xvals,yvals, 1, 9, 2, 100, 500);

            //for (int i = 0; i < xvals.Length; i++)
            //{
            //    tempXvalues.Add(xvals[i]);
            //    tempYValues.Add(yvals[i]);
            //}

            //xydata.Xvalues = tempXvalues.ToArray();
            //xydata.Yvalues = tempYValues.ToArray();

            //xydata.Display();






        }

        [Test]
        public void GetNumberOfFramesTest()
        {
            UIMFRun test = new UIMFRun();
            UIMFRun uimfRun = new UIMFRun(uimfFilepath);
            int numframes = uimfRun.GetNumFrames();
            int numScans = uimfRun.GetNumMSScans();


            Console.WriteLine("Number of frames = " + numframes);
            Console.WriteLine("Number of scans = " + numScans);
            Assert.AreEqual(1950, numframes);
            Assert.AreEqual(975000, numScans);


        }

        [Test]
        public void UIMFLibraryDirectAccessTest1()
        {
            int numFrames = 0;

            UIMFLibrary.DataReader reader = new UIMFLibrary.DataReader();
            reader.OpenUIMF(FileRefs.RawDataMSFiles.UIMFStdFile1);
            numFrames = reader.GetGlobalParameters().NumFrames;

            Console.WriteLine("Number of frames = " + numFrames);
            Assert.AreEqual(1950, numFrames);
        }

        [Test]
        public void UIMFLibrarySumScansTest1()
        {
            double[] xvals = new double[100000];
            int[] yvals = new int[100000];
            int summedTotal = 0;

            UIMFLibrary.DataReader reader = new UIMFLibrary.DataReader();

            reader.OpenUIMF(FileRefs.RawDataMSFiles.UIMFStdFile1);
            summedTotal = reader.SumScansNonCached(xvals, yvals, 0, 100, 105, 200, 300);
            Assert.AreEqual(91949, summedTotal);
        }

        [Test]
        public void getNumMSScansTest1()
        {
            Run uimfRun = new UIMFRun(uimfFilepath);
            int numScans = uimfRun.GetNumMSScans();

            int numScans2 = uimfRun.GetNumMSScans();

            Assert.AreEqual(975000, numScans);
            Assert.AreEqual(975000, numScans2);
        }

        [Test]
        public void getSummedFramesMSTest1()
        {
            UIMFRun uimfRun = new UIMFRun(uimfFilepath);

            uimfRun.GetMassSpectrum(new ScanSet(300, 299, 301), new FrameSet(1200, 1199, 1201), 100, 2000);
            Console.WriteLine(uimfRun.XYData.Xvalues.Length);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < uimfRun.XYData.Xvalues.Length; i++)
            {
                sb.Append(uimfRun.XYData.Xvalues[i]);
                sb.Append("\t");
                sb.Append(uimfRun.XYData.Yvalues[i]);
                sb.Append(Environment.NewLine);
            }
            Console.WriteLine(sb.ToString());
            Assert.AreEqual(2331, uimfRun.XYData.Xvalues.Length);
            Assert.AreEqual(2331, uimfRun.XYData.Yvalues.Length);
        }

        [Test]
        public void getFrameStartTimeTest1()
        {
            UIMFRun run = new UIMFRun(uimfFilepath);

            double frametime1 = run.GetTime(1);

            Assert.AreEqual(35.48137, frametime1);

        }



        [Test]
        public void getSummedFramesMSTest2()
        {
            UIMFRun uimfRun = new UIMFRun(uimfFilepath);

            for (int i = 1200; i < (600 * 2400 / 4); i = i + 600)
            {
                uimfRun.GetMassSpectrum(new ScanSet(i), new FrameSet(1200, 1199, 1201), 100, 2000);
                //Console.WriteLine(uimfRun.XYData.Xvalues.Length);
            }
        }


        [Test]
        public void getOneFrameOneScanTest1()
        {
            UIMFRun uimfRun = new UIMFRun(uimfFilepath);

            for (int i = 250; i < 251; i++)
            {

                ScanSet scan = new ScanSet(i, i - 2, i + 2);
                FrameSet frame = new FrameSet(501);

                uimfRun.GetMassSpectrum(scan, frame, 200, 2000);
                //Console.WriteLine(uimfRun.XYData.Xvalues.Length);
            }





        }


        public void getSummedFramesAndScansMSTest1()
        {
            UIMFRun uimfRun = new UIMFRun(uimfFilepath);

            uimfRun.GetMassSpectrum(new ScanSet(300, 1, 599), new FrameSet(1200, 1199, 1201), 100, 2000);
            Console.WriteLine(uimfRun.XYData.Xvalues.Length);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < uimfRun.XYData.Xvalues.Length; i++)
            {
                sb.Append(uimfRun.XYData.Xvalues[i]);
                sb.Append("\t");
                sb.Append(uimfRun.XYData.Yvalues[i]);
                sb.Append(Environment.NewLine);

            }
            //Console.WriteLine(sb.ToString());
            Assert.AreEqual(23505, uimfRun.XYData.Xvalues.Length);
            Assert.AreEqual(23505, uimfRun.XYData.Yvalues.Length);
        }

        [Test]
        public void getFramePressureTest1()
        {
            UIMFRun uimfRun = new UIMFRun(uimfFilepath);

            double framePressure = uimfRun.GetFramePressure(1000);

            Assert.AreNotEqual(0, framePressure);
            Assert.AreEqual(4.058m, (Decimal)framePressure);

        }


        //this is a useful test to help show memory issues when contantly using the UIMFLibrary
        [Test]
        public void memoryTest1()
        {
            UIMFRun uimfRun = new UIMFRun(uimfFilepath);

            uimfRun.CurrentFrameSet = new FrameSet(800);

            Process currentProcess = Process.GetCurrentProcess();
            TestUtilities.DisplayInfoForProcess(currentProcess);

            long privateMemorySizeBeforeProcessing = currentProcess.PrivateMemorySize64;


            ScanSetCollectionCreator scanSetCreator = new ScanSetCollectionCreator(uimfRun, 1, 1);
            scanSetCreator.Create();

            foreach (var scanset in uimfRun.ScanSetCollection.ScanSetList)
            {
                //uimfRun.GetMassSpectrum(scanset, uimfRun.CurrentFrameSet, 0, 2000);

                uimfRun.GetFramePressureBack(uimfRun.CurrentFrameSet.PrimaryFrame);

            }


            currentProcess = Process.GetCurrentProcess();
            long privateMemorySizeAfterProcessing = currentProcess.PrivateMemorySize64; 

            TestUtilities.DisplayInfoForProcess(currentProcess);

            long numBytesAdded = privateMemorySizeAfterProcessing - privateMemorySizeBeforeProcessing;
            Console.WriteLine("Number of bytes added by processing = " + numBytesAdded);


        }


        [Test]
        public void GetFrameParameters_Test1()
        {
            UIMFRun uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile1);
            int frameStart = 800;
            int frameStop = 810;
            int numFramesSummed = 3;


            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(uimfRun, frameStart, frameStop, numFramesSummed, 1);
            fscc.Create();

            uimfRun.GetFrameDataAllFrameSets();

        }



        [Test]
        public void getDriftTimesTest1()
        {
            UIMFRun uimfRun = new UIMFRun(uimfFilepath);
            int startScan = 0;
            int stopScan = 599;

            double[] driftTimes = new double[stopScan - startScan + 1];


            StringBuilder sb = new StringBuilder();
            for (int i = startScan; i <= stopScan; i++)
            {
                driftTimes[i - startScan] = uimfRun.GetDriftTime(1300, i);
                sb.Append(driftTimes[i - startScan]);
                sb.Append(Environment.NewLine);

            }


            Assert.AreEqual(1.08005941278066m, (decimal)driftTimes[10]);


            //Console.WriteLine(sb.ToString());
        }

        [Test]
        public void getAvgTOFLengthTest1()
        {
            double avgTOFLength = 0;
            UIMFLibrary.DataReader reader = new UIMFLibrary.DataReader();
            reader.OpenUIMF(FileRefs.RawDataMSFiles.UIMFStdFile1);
            avgTOFLength = reader.GetFrameParameters(1000).AverageTOFLength;

            Assert.AreEqual(99488.2, (decimal)avgTOFLength);

        }

        [Test]
        public void getDriftTimeForScanZero()
        {
            //UIMF scan numbers are 0-based.  But first scan should have a drifttime above 0. 

            UIMFRun uimfRun = new UIMFRun(uimfFilepath);

            double driftTime = uimfRun.GetDriftTime(1300, 0);
            Assert.Greater((decimal)driftTime, 0);
        }

        [Test]
        public void GetMSLevelTest1()
        {
            Run run = new UIMFRun(uimfFilepath);
            Assert.AreEqual(1, run.GetMSLevel(233));
            Assert.AreEqual(1, run.GetMSLevel(234));
            Assert.AreEqual(1, run.GetMSLevel(2000));
        }

        [Test]
        public void GetNumBinsTest1()
        {
            UIMFRun uimfRun = new UIMFRun(uimfFilepath);
            int numBins = uimfRun.GetNumBins();
            Assert.AreEqual(92000, numBins);
        }


        [Test]
        public void GetFrameDataForUIMFRunTest1()
        {
            UIMFRun uimfRun = new UIMFRun(uimfFilepath);

            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(uimfRun, 500, 600, 3, 1);
            fscc.Create();

            uimfRun.GetFrameDataAllFrameSets();

            StringBuilder sb = new StringBuilder();
            foreach (var item in uimfRun.FrameSetCollection.FrameSetList)
            {
                sb.Append(item.PrimaryFrame);
                sb.Append("\t");
                sb.Append(item.FramePressure.ToString("0.######"));
                sb.Append("\t");
                sb.Append(item.AvgTOFLength.ToString("0.###"));
                sb.Append(Environment.NewLine);
            }

            Console.Write(sb.ToString());

        }


        [Test]
        public void SimpleSpeedTest1()
        {
            UIMFRun uimfRun = new UIMFRun(FileRefs.RawDataMSFiles.UIMFStdFile1);
            int numFramesSummed = 3;
            int numScansSummed = 9;
            double minMZ = 300;
            double maxMZ = 2000;


            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(uimfRun, 500, 510, numFramesSummed, 1);
            fscc.Create();

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(uimfRun, 300, 310, numScansSummed, 1);
            sscc.Create();

            List<long> times = new List<long>();
            Stopwatch sw = new Stopwatch();


            foreach (var frame in uimfRun.FrameSetCollection.FrameSetList)
            {
                uimfRun.CurrentFrameSet = frame;
                foreach (var scan in uimfRun.ScanSetCollection.ScanSetList)
                {
                    uimfRun.CurrentScanSet = scan;
                    sw.Start();
                    uimfRun.GetMassSpectrum(scan, frame, minMZ, maxMZ);
                    sw.Stop();

                    times.Add(sw.ElapsedMilliseconds);
                    sw.Reset();

                }

            }

            Console.WriteLine("NumMSScans= " + times.Count);
            Console.WriteLine("Time (ms) per scan = " + times.Average());



        }


        [Test]
        public void getAllFrameParametersTest1()
        {
            string testFile = @"\\protoapps\UserData\FPGA\Data\112310-QCShew_1sec_frames_100min_run\1sec_frames_70_out_theshold\QCShew_15coadds_10kframes_380scans_7in_70out_2.UIMF";


            UIMFRun uimfRun = new UIMFRun(testFile);

            Console.WriteLine("Num frames= " + uimfRun.GetNumFrames());

            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(uimfRun, 1, 1);
            fscc.Create();
            uimfRun.GetFrameDataAllFrameSets();


        }



    }
}
