using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend;
using DeconTools.Backend.ProcessingTasks;

namespace DeconTools.UnitTesting.UtilitiesTests
{
    [TestFixture]
    public class DriftTimeProfileUtilitiesTests
    {
        

        [Test]
        public void getDriftProfileFromRawDataMZ_771_Test1()
        {
            Run run = TestUtilities.GetStandardUIMFRun();

            UIMFRun uimfRun = (UIMFRun)run;
            DriftTimeProfileExtractor profileExtractor = new DriftTimeProfileExtractor();

            FrameSet frameset=new FrameSet(805,804,806);

            double targetMZ1   = 746.3842;
            double targetMZ2 = 771.6894;


            ScanSetCollectionCreator ssc = new ScanSetCollectionCreator(run,200,350, 9, 1);
            ssc.Create();

            XYData xydata = profileExtractor.ExtractProfileFromRawData(uimfRun, frameset, run.ScanSetCollection, targetMZ2, 10d);


            TestUtilities.DisplayXYValues(xydata);
            


        }


        [Test]
        public void getDriftProfileFromRawDataMZ_967_Test1()
        {
            Run run = TestUtilities.GetStandardUIMFRun();

            UIMFRun uimfRun = (UIMFRun)run;
            DriftTimeProfileExtractor profileExtractor = new DriftTimeProfileExtractor();

            FrameSet frameset = new FrameSet(804, 803, 805);
            uimfRun.CurrentFrameSet = frameset;

            double targetMZ = 967.26;


            ScanSetCollectionCreator ssc = new ScanSetCollectionCreator(run, 200, 350, 9, 1);
            ssc.Create();

            //XYData xydata = profileExtractor.ExtractProfileFromRawData(uimfRun, frameset, run.ScanSetCollection, targetMZ, 30d);

            createPeaks(run);
            XYData xydata = profileExtractor.ExtractProfileFromPeakData(uimfRun, frameset, run.ScanSetCollection, targetMZ, 30d);


            TestUtilities.DisplayXYValues(xydata);
        }


        [Test]
        public void getDriftProfileMZ_796_Test1()
        {
            Run run = TestUtilities.GetStandardUIMFRun();

            UIMFRun uimfRun = (UIMFRun)run;
            DriftTimeProfileExtractor profileExtractor = new DriftTimeProfileExtractor();

            FrameSet frameset = new FrameSet(616, 615, 617);
            uimfRun.CurrentFrameSet = frameset;

            double targetMZ = 1004.1987;


            ScanSetCollectionCreator ssc = new ScanSetCollectionCreator(run, 200, 350, 3, 1);
            ssc.Create();

            XYData xydata = profileExtractor.ExtractProfileFromRawData(uimfRun, frameset, run.ScanSetCollection, targetMZ, 30d);
            TestUtilities.DisplayXYValues(xydata);

            Console.WriteLine("-------------------------------------------------------------------------------------");

            createPeaks(run);
            xydata = profileExtractor.ExtractProfileFromPeakData(uimfRun, frameset, run.ScanSetCollection, targetMZ, 30d);


            TestUtilities.DisplayXYValues(xydata);

        }


        [Test]
        public void getDriftProfileFrom_peakDataTest1()
        {
            Run run = TestUtilities.GetStandardUIMFRun();

            UIMFRun uimfRun = (UIMFRun)run;
            DriftTimeProfileExtractor profileExtractor = new DriftTimeProfileExtractor();

            FrameSet frameset = new FrameSet(805, 804, 806);
            uimfRun.CurrentFrameSet = frameset;

            double targetMZ1 = 746.3842;
            double targetMZ2 = 771.6894;


            ScanSetCollectionCreator ssc = new ScanSetCollectionCreator(run, 200, 350, 9, 1);
            ssc.Create();

            createPeaks(run);

            XYData xydata = profileExtractor.ExtractProfileFromPeakData(uimfRun, frameset, run.ScanSetCollection, targetMZ2, 30d);


            TestUtilities.DisplayXYValues(xydata);



        }

        private void createPeaks(Run run)
        {
            UIMF_MSGenerator msgen = new UIMF_MSGenerator();

            DeconToolsPeakDetector peakDet = new DeconToolsPeakDetector();
            peakDet.StorePeakData = true;
            peakDet.SigNoiseThreshold = 3;
            peakDet.PeakBackgroundRatio = 4;

            for (int i = 0; i < run.ScanSetCollection.ScanSetList.Count; i++)
            {
                ScanSet s = run.ScanSetCollection.ScanSetList[i];
                run.CurrentScanSet = s;

                msgen.Execute(run.ResultCollection);
                peakDet.Execute(run.ResultCollection);

            }

           
        }
    }
}
