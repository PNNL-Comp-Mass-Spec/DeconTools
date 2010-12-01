using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend;
using DeconTools.Backend.ProcessingTasks.ZeroFillers;
using DeconTools.Backend.Utilities;


namespace DeconTools.UnitTesting2.ProcessingTasksTests
{
    [TestFixture]
    public class DeconToolsZeroFillerTests
    {
  
        string imfStrangeOneFilepath = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\IMF\7peptides_1uM_600_50_4t_114Vpp_0000.Accum_1_recal.imf";

        [Test]
        public void test1()
        {
            Run run = new MSScanFromTextFileRun(FileRefs.RawDataMSFiles.TextFileMS_std1);
            ResultCollection resultcollection = new ResultCollection(run);

            Task msgen = new GenericMSGenerator();
            msgen.Execute(resultcollection);

            Task peakdetector = new DeconToolsPeakDetector(3,3, Globals.PeakFitType.QUADRATIC,false);
            peakdetector.Execute(resultcollection);

            Assert.AreEqual(82, resultcollection.Run.PeakList.Count);

            Task zeroFiller = new DeconToolsZeroFiller(3);
            zeroFiller.Execute(resultcollection);

            peakdetector.Execute(resultcollection);

            //need to verify if this is working properly
            Assert.AreEqual(81, resultcollection.Run.PeakList.Count);
        }


        
        /// <summary>
        /// The following tests were created to deal with a zero filling issue that occurred
        /// with TOF data (IMF) and when the algorithm encountered low m/z data ( less than 100)
        /// This was fixed in Decon2LS, DeconEngineV2, version 1.3.2 (June 18, 2009);  
        /// </summary>
        [Test]
        public void zeroFillingWeirdness1()
        {
            Run run = new IMFRun(imfStrangeOneFilepath);
            ResultCollection results = new ResultCollection(run);

            ScanSetCollectionCreator scanSetCreator = new ScanSetCollectionCreator(run, 238, 245,7,1);
            scanSetCreator.Create();

            run.CurrentScanSet = run.ScanSetCollection.GetScanSet(239);
            Task msgen = new GenericMSGenerator(0,2000);
            msgen.Execute(results);

            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 5;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;

            Task peakdetector = new DeconToolsPeakDetector(detectorParams);
            peakdetector.Execute(results);

            Assert.AreEqual(21, results.Run.PeakList.Count);
            //Assert.AreEqual(1382, results.Run.XYData.Xvalues.Length);

            StringBuilder sb = new StringBuilder();
            TestUtilities.GetXYValuesToStringBuilder(sb, results.Run.XYData.Xvalues, results.Run.XYData.Yvalues);
            //Console.Write(sb.ToString());
            Console.WriteLine();
            Console.WriteLine();


            DeconToolsZeroFiller zerofiller = new DeconToolsZeroFiller(3);
            zerofiller.Execute(results);

            sb = new StringBuilder();
            TestUtilities.GetXYValuesToStringBuilder(sb, results.Run.XYData.Xvalues, results.Run.XYData.Yvalues);
            Console.Write(sb.ToString());

            peakdetector.Execute(results);
            Assert.AreEqual(21, results.Run.PeakList.Count);
            Assert.AreEqual(3732, results.Run.XYData.Xvalues.Length);


            
            
            
            
            
            //run.CurrentScanSet = run.ScanSetCollection.GetScanSet(238);
            //msgen = new GenericMSGenerator(0,2000);
            //msgen.Execute(results);
            
            //peakdetector.Execute(results);

            //Assert.AreEqual(31, results.Run.MSPeakList.Count);
            //Assert.AreEqual(1389, results.Run.XYData.Xvalues.Length);
           
            //zerofiller.Execute(results);
            
            //peakdetector.Execute(results);
            //Assert.AreEqual(31, results.Run.MSPeakList.Count);
            //Assert.AreEqual(3794, results.Run.XYData.Xvalues.Length);


            //run.CurrentScanSet = run.ScanSetCollection.GetScanSet(245);
            //msgen = new GenericMSGenerator();
            //msgen.Execute(results);

            //peakdetector.Execute(results);

            //Assert.AreEqual(31, results.Run.MSPeakList.Count);
            //Assert.AreEqual(1389, results.Run.XYData.Xvalues.Length);

            //zerofiller.Execute(results);

            //peakdetector.Execute(results);
            //Assert.AreEqual(31, results.Run.MSPeakList.Count);
            //Assert.AreEqual(3794, results.Run.XYData.Xvalues.Length);


        }

        [Test]
        public void zeroFillingWeirdness2()
        {
            Run run = new IMFRun(imfStrangeOneFilepath);
            ResultCollection results = new ResultCollection(run);

            ScanSetCollectionCreator scanSetCreator = new ScanSetCollectionCreator(run, 238, 245, 7, 1);
            scanSetCreator.Create();

            run.CurrentScanSet = run.ScanSetCollection.GetScanSet(239);

            Task msgen;
            
            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 5;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;

            Task peakdetector = new DeconToolsPeakDetector(detectorParams);
            DeconToolsZeroFiller zerofiller = new DeconToolsZeroFiller(3);


            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 1000; i++)
            {
                msgen = new GenericMSGenerator(i, 2000);
                msgen.Execute(results);

                zerofiller.Execute(results);

                peakdetector.Execute(results);

                sb.Append(i);
                sb.Append("\t");
                sb.Append(results.Run.XYData.Xvalues.Length);
                sb.Append("\t");
                sb.Append(results.Run.PeakList.Count);
                sb.Append("\n");
            }

            Console.Write(sb.ToString());
            
           

        }


        [Test]
        public void zeroFillingWeirdness3()
        {
            Run run = new IMFRun(imfStrangeOneFilepath);

            ScanSetCollectionCreator scanSetCreator = new ScanSetCollectionCreator(run, 0, 599, 7, 1);
            scanSetCreator.Create();



            DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detectorParams.PeakBackgroundRatio = 5;
            detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
            detectorParams.SignalToNoiseThreshold = 3;
            detectorParams.ThresholdedData = false;

            Task peakdetector = new DeconToolsPeakDetector(detectorParams);
            DeconToolsZeroFiller zerofiller = new DeconToolsZeroFiller(3);


            StringBuilder sb = new StringBuilder();

            Task msgen;
            ResultCollection results = new ResultCollection(run);


            foreach (ScanSet scan in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scan;
                msgen = new GenericMSGenerator(0, 2000);
                msgen.Execute(results);

                bool containsPoints = doesXYDataContainPointsInRange(results.Run.XYData.Xvalues, 0, 100);

                
                sb.Append(scan.PrimaryScanNumber);
                sb.Append("\t");
                sb.Append(containsPoints);
                sb.Append("\t");

                peakdetector.Execute(results);
                sb.Append(results.Run.PeakList.Count);
                sb.Append("\t");

                zerofiller.Execute(results);
                peakdetector.Execute(results);

                sb.Append(results.Run.PeakList.Count);

                sb.Append("\n");

            }




           


            
            Console.Write(sb.ToString());



        }

        private bool doesXYDataContainPointsInRange(double[]xvals, double lowerMZ, double upperMZ)
        {
            for (int i = 0; i < xvals.Length; i++)
            {
                if (xvals[i] >= lowerMZ && xvals[i] <= upperMZ)
                {
                    return true;
                }

                
            }
            return false;
        }

    }
}
