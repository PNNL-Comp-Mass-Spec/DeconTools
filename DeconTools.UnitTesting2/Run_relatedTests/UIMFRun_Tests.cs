using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using UIMFLibrary;
using System.Diagnostics;
using DeconTools.Backend.Utilities;
using DeconTools.UnitTesting2;
using System.IO;


namespace DeconTools.UnitTesting.Run_relatedTests
{
    [TestFixture]
    public class UIMFRun_Tests
    {
        private string uimfFilepath = FileRefs.RawDataMSFiles.UIMFStdFile1;

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
            UIMFLibraryAdapter adapter = UIMFLibraryAdapter.getInstance(uimfFilepath);
            DataReader datareader = adapter.Datareader;

            int numFrames = datareader.GetGlobalParameters().NumFrames;

            Console.WriteLine("Number of frames = " + numFrames);
            Assert.AreEqual(1950, numFrames);
        }

        [Test]
        public void UIMFLibrarySumScansTest1()
        {
            UIMFLibraryAdapter adapter = UIMFLibraryAdapter.getInstance(uimfFilepath);
            DataReader datareader = adapter.Datareader;

            double[] xvals = new double[100000];
            int[] yvals = new int[100000];

            int summedTotal = datareader.SumScans(xvals, yvals, 0, 100, 105, 200, 300);
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

                ScanSet scan = new ScanSet(i,i - 2, i + 2);
                FrameSet frame = new FrameSet(501);

                uimfRun.GetMassSpectrum(scan, frame,200,2000);
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

       
        [Test]
        public void memoryTest1()
        {
            UIMFRun uimfRun = new UIMFRun(uimfFilepath);

            uimfRun.CurrentFrameSet = new FrameSet(800);

            Process currentProcess = Process.GetCurrentProcess();
            TestUtilities.DisplayInfoForProcess(currentProcess);

            ScanSetCollectionCreator scanSetCreator = new ScanSetCollectionCreator(uimfRun, 1, 1);
            scanSetCreator.Create();

            foreach (var scanset in uimfRun.ScanSetCollection.ScanSetList)
            {
                //uimfRun.GetMassSpectrum(scanset, uimfRun.CurrentFrameSet, 0, 2000);
                
                uimfRun.GetFramePressureBack(uimfRun.CurrentFrameSet.PrimaryFrame);
                
            }

            currentProcess = Process.GetCurrentProcess();
            TestUtilities.DisplayInfoForProcess(currentProcess);

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
            UIMFLibraryAdapter adapter = UIMFLibraryAdapter.getInstance(uimfFilepath);
            DataReader datareader = adapter.Datareader;

            double avgTOFLength = datareader.GetFrameParameters(1000).AverageTOFLength;

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

            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(uimfRun,500,600, 3, 1);
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



    }
}
