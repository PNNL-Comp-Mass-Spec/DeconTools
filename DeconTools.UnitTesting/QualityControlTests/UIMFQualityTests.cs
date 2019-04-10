using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.ZeroFillers;

namespace DeconTools.UnitTesting.QualityControlTests
{
    [TestFixture]
    public class UIMFQualityTests
    {
         string uimfFilepath = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000_V2009_05_28.uimf";
        string imfFrame1200filepath = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000.Accum_1200.IMF";
        private string uimfFilepath2 = "..\\..\\TestFiles\\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf";
        private string imf2Frame391 = "..\\..\\TestFiles\\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.Accum_391.IMF";


        string fpgaUIMFFilePath = @"F:\Gord\Data\UIMF\BSA_Mid_600_50_10_3ppdis_10mstrap.uimf";
        string fpgaUIMFFilePath2 = @"F:\Gord\Data\UIMF\FPGA\TroubleShooting_cases\QC_Shew_MSMS_500_100_fr720_Ek_0000.uimf";


        [Test]
        public void UIMF_and_IMF_Nosumming_Test1()
        {
            Run uimfrun = new UIMFRun(uimfFilepath);
            Run imfRun = new IMFRun(imfFrame1200filepath);

            int startFrame = 1200;
            int stopFrame = 1200;

            int numFramesSummed = 1;
            int numScansSummed = 1;

            FrameSetCollectionCreator framesetCreator = new FrameSetCollectionCreator(uimfrun, startFrame, stopFrame, numFramesSummed, 1);
            framesetCreator.Create();

            ScanSetCollectionCreator scanSetCreator = new ScanSetCollectionCreator(uimfrun, numScansSummed, 1);
            scanSetCreator.Create();

            ResultCollection uimfResults = new ResultCollection(uimfrun);

            //first analyze the UIMF file....
            foreach (FrameSet frameset in ((UIMFRun)uimfrun).FrameSetCollection.FrameSetList)
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

                ((UIMFRun)uimfrun).CurrentFrameSet = frameset;
                foreach (ScanSet scanset in uimfrun.ScanSetCollection.ScanSetList)
                {

                    uimfrun.CurrentScanSet = scanset;
                    Task msgen = new UIMF_MSGenerator(0, 2000);
                    msgen.Execute(uimfResults);

                    Task peakDetector = new DeconToolsPeakDetector();
                    peakDetector.Execute(uimfResults);

                    Task rapid = new RapidDeconvolutor();
                    rapid.Execute(uimfResults);

                    Task scanResultUpdater = new ScanResultUpdater();
                    scanResultUpdater.Execute(uimfResults);
                }
            }

            //next analyze the IMF file for the corresponding frame
            scanSetCreator = new ScanSetCollectionCreator(imfRun, 0, 599, numScansSummed, 1);
            scanSetCreator.Create();

            ResultCollection imfResults = new ResultCollection(imfRun);
            foreach (ScanSet scanset in imfRun.ScanSetCollection.ScanSetList)
            {

                imfRun.CurrentScanSet = scanset;
                Task msgen = new GenericMSGenerator(0, 2000);
                msgen.Execute(imfResults);

                Task peakDetector = new DeconToolsPeakDetector();
                peakDetector.Execute(imfResults);

                Task rapid = new RapidDeconvolutor();
                rapid.Execute(imfResults);

                Task scanResultUpdater = new ScanResultUpdater();
                scanResultUpdater.Execute(imfResults);
            }

            Console.WriteLine("imfScanSetCount = " + imfResults.Run.ScanSetCollection.ScanSetList.Count);
            Console.WriteLine("UIMFScanSetCount = " + uimfResults.Run.ScanSetCollection.ScanSetList.Count);

            Console.WriteLine("IMF scan0 peaks = " + imfResults.ScanResultList.Sum(p => p.NumPeaks));
            Console.WriteLine("UIMF scan0 peaks = " + uimfResults.ScanResultList[0].NumPeaks);

            Console.WriteLine("imfResultCount = " + imfResults.ResultList.Count);
            Console.WriteLine("UIMFResultCount = " + uimfResults.ResultList.Count);


            for (int i = 0; i < uimfResults.ResultList.Count; i++)
            {
                Console.Write(getResultSummary(uimfResults.ResultList[i]));
                Console.Write(getResultSummary(imfResults.ResultList[i]));
                Console.Write("\n");
            }


        }

        /// <summary>
        /// This test iterates over all the scans of one frame and 
        /// compares XY values for the UIMF data vs. the IMF data
        /// Test asserts that Y values are identical.  
        /// Test asserts that X values are identical
        /// </summary>
        [Test]
        public void compareUIMF_and_IMF_XYDataPoints1()
        {
            Run uimfRun = new UIMFRun(uimfFilepath);
            ResultCollection uimfResults = new ResultCollection(uimfRun);

            Run imfRun = new IMFRun(imfFrame1200filepath);
            ResultCollection imfResults = new ResultCollection(imfRun);

            FrameSet uimfFrame = new FrameSet(1200);


            ((UIMFRun)uimfRun).CurrentFrameSet = uimfFrame;



            Task uimfMSGen = new UIMF_MSGenerator(0, 2000);
            Task imfMSGen = new GenericMSGenerator(0, 2000);

            ScanSetCollectionCreator scanSetCollectionCreator = new ScanSetCollectionCreator(uimfRun, 1, 1);
            scanSetCollectionCreator.Create();

            scanSetCollectionCreator = new ScanSetCollectionCreator(imfRun, 1, 1);
            scanSetCollectionCreator.Create();

            StringBuilder sb = new StringBuilder();
            for (int n = 0; n < imfRun.ScanSetCollection.ScanSetList.Count - 1; n++)
            {
                uimfRun.CurrentScanSet = uimfRun.ScanSetCollection.ScanSetList[n];
                imfRun.CurrentScanSet = imfRun.ScanSetCollection.ScanSetList[n + 1];
                uimfMSGen.Execute(uimfResults);
                imfMSGen.Execute(imfResults);


                Assert.AreEqual(imfResults.Run.XYData.Xvalues.Length, uimfResults.Run.XYData.Xvalues.Length);

                double[] ppmDiffValues = new double[imfResults.Run.XYData.Xvalues.Length];

                for (int i = 0; i < uimfResults.Run.XYData.Xvalues.Length; i++)
                {

                    double imfXval = imfResults.Run.XYData.Xvalues[i];
                    double uimfXval = uimfResults.Run.XYData.Xvalues[i];
                    double imfYval = imfResults.Run.XYData.Yvalues[i];
                    double uimfYval = uimfResults.Run.XYData.Yvalues[i];

                    Assert.AreEqual(imfYval, uimfYval);
                    Assert.AreEqual(Math.Round(imfXval, 8), Math.Round(uimfXval, 8));


                    //double ppmDiff = Math.Abs(imfXval - uimfXval) / imfXval * 1e6;
                    //ppmDiffValues[i] = ppmDiff;

                    //Assert.Less(ppmDiff, 0.25);

                }


                for (int i = 0; i < imfResults.Run.XYData.Xvalues.Length; i++)
                {
                    sb.Append(n);
                    sb.Append("\t");
                    sb.Append(imfResults.Run.XYData.Xvalues[i].ToString("0.00000"));
                    sb.Append("\t");
                    sb.Append(uimfResults.Run.XYData.Xvalues[i].ToString("0.00000"));
                    sb.Append("\t");
                    sb.Append(ppmDiffValues[i].ToString("0.00"));
                    sb.Append("\t");
                    sb.Append(imfResults.Run.XYData.Yvalues[i]);
                    sb.Append("\t");
                    sb.Append(uimfResults.Run.XYData.Yvalues[i]);
                    sb.Append("\n");

                }

            }

            Console.Write(sb.ToString());
        }

        //[Test]
        //public void getXYDataFromUIMF()
        //{
        //    Run uimfrun = new UIMFRun(uimfFilepath2,500,500);



        //    int numFramesSummed = 5;
        //    int numScansSummed = 9;

        //    int targetScanSet = 240;
        //    int targetFrameSet = 500;

        //    ScanSetCollectionCreator scanSetCreator = new ScanSetCollectionCreator(uimfrun, numScansSummed, 1);
        //    scanSetCreator.Create();

        //    FrameSetCollectionCreator frameSetCreator = new FrameSetCollectionCreator(uimfrun, numFramesSummed, 1);
        //    frameSetCreator.Create();


        //    ((UIMFRun)uimfrun).CurrentFrameSet = ((UIMFRun)uimfrun).FrameSetCollection.GetFrameSet(targetFrameSet);
        //    uimfrun.CurrentScanSet = uimfrun.ScanSetCollection.GetScanSet(targetScanSet);

        //    Task msgen = new UIMF_MSGenerator();
        //    msgen.Execute(uimfrun.ResultCollection);

        //    Task zerofill = new DeconToolsZeroFiller(3);
        //    zerofill.Execute(uimfrun.ResultCollection);

        //    string xyvalues = getXYValuesForDisplay(uimfrun.XYData.Xvalues, uimfrun.XYData.Yvalues);
        //    Console.Write(xyvalues);

        //    Console.WriteLine();
        //    Console.WriteLine();

        //    DeconToolsV2.Peaks.clsPeakProcessorParameters detectorParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
        //    detectorParams.PeakBackgroundRatio = 4;
        //    detectorParams.PeakFitType = DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC;
        //    detectorParams.SignalToNoiseThreshold = 3;
        //    detectorParams.ThresholdedData = false;

        //    Task peakdetector = new DeconToolsPeakDetector(detectorParams);
        //    peakdetector.Execute(uimfrun.ResultCollection);


        //    DeconToolsV2.HornTransform.clsHornTransformParameters hornparams=new DeconToolsV2.HornTransform.clsHornTransformParameters();
        //    hornparams.CompleteFit=true;
        //    hornparams.PeptideMinBackgroundRatio = 4;
        //    hornparams.LeftFitStringencyFactor = 2.5;
        //    hornparams.RightFitStringencyFactor = 0.5;
        //    hornparams.ThrashOrNot = true;
        //    hornparams.MaxFit = 0.4;

        //    Task decon = new HornDeconvolutor(hornparams);
        //    decon.Execute(uimfrun.ResultCollection);


        //    Console.WriteLine("numpeaks = " + uimfrun.MSPeakList.Count);
        //    Console.WriteLine("numFeatures = " + uimfrun.CurrentScanSet.NumIsotopicProfiles);


        //}


        [Test]
        public void compareUIMF_and_IMF_XYDataPoints2()
        {
            Run uimfRun = new UIMFRun(uimfFilepath2);
            ResultCollection uimfResults = new ResultCollection(uimfRun);

            Run imfRun = new IMFRun(imf2Frame391);
            ResultCollection imfResults = new ResultCollection(imfRun);

            FrameSet uimfFrame = new FrameSet(391);


            ((UIMFRun)uimfRun).CurrentFrameSet = uimfFrame;



            Task uimfMSGen = new UIMF_MSGenerator();
            Task imfMSGen = new GenericMSGenerator();

            

            ScanSetCollectionCreator scanSetCollectionCreator = new ScanSetCollectionCreator(uimfRun, 9, 1);
            scanSetCollectionCreator.Create();

            scanSetCollectionCreator = new ScanSetCollectionCreator(imfRun, 9, 1);
            scanSetCollectionCreator.Create();

            int scanStart = 200;
            int scanStop = 400;

            StringBuilder sb = new StringBuilder();
            for (int n = scanStart; n < scanStop; n++)
            {
                uimfRun.CurrentScanSet = uimfRun.ScanSetCollection.ScanSetList[n];
                imfRun.CurrentScanSet = imfRun.ScanSetCollection.ScanSetList[n + 1];
                uimfMSGen.Execute(uimfResults);
                imfMSGen.Execute(imfResults);


                Assert.AreEqual(imfResults.Run.XYData.Xvalues.Length, uimfResults.Run.XYData.Xvalues.Length);

                double[] ppmDiffValues = new double[imfResults.Run.XYData.Xvalues.Length];

                for (int i = 0; i < uimfResults.Run.XYData.Xvalues.Length; i++)
                {

                    double imfXval = imfResults.Run.XYData.Xvalues[i];
                    double uimfXval = uimfResults.Run.XYData.Xvalues[i];
                    double imfYval = imfResults.Run.XYData.Yvalues[i];
                    double uimfYval = uimfResults.Run.XYData.Yvalues[i];

                    Assert.AreEqual(imfYval, uimfYval);
                    Assert.AreEqual(Math.Round(imfXval, 8), Math.Round(uimfXval, 8));


                    //double ppmDiff = Math.Abs(imfXval - uimfXval) / imfXval * 1e6;
                    //ppmDiffValues[i] = ppmDiff;

                    //Assert.Less(ppmDiff, 0.25);

                }


                for (int i = 0; i < imfResults.Run.XYData.Xvalues.Length; i++)
                {
                    sb.Append(n);
                    sb.Append("\t");
                    sb.Append(imfResults.Run.XYData.Xvalues[i].ToString("0.00000"));
                    sb.Append("\t");
                    sb.Append(uimfResults.Run.XYData.Xvalues[i].ToString("0.00000"));
                    sb.Append("\t");
                    sb.Append(ppmDiffValues[i].ToString("0.00"));
                    sb.Append("\t");
                    sb.Append(imfResults.Run.XYData.Yvalues[i]);
                    sb.Append("\t");
                    sb.Append(uimfResults.Run.XYData.Yvalues[i]);
                    sb.Append("\n");

                }

            }

            Console.Write(sb.ToString());
        }


        [Test]
        public void examineFPGAUIMFTest1()
        {

            Run run = new DeconTools.Backend.Runs.UIMFRun(fpgaUIMFFilePath);

            FrameSetCollectionCreator ffcc = new FrameSetCollectionCreator(run, 1, 1, 1, 1);
            ffcc.Create();

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 285, 285, 9, 1);
            sscc.Create();

            MSGeneratorFactory factory = new MSGeneratorFactory();
            Task msgen = factory.CreateMSGenerator(run.MSFileType);

            DeconToolsPeakDetector peakDet = new DeconToolsPeakDetector();
            peakDet.PeakBackgroundRatio = 6;
            peakDet.SigNoiseThreshold = 3;

            HornDeconvolutor decon = new HornDeconvolutor();
            decon.MinPeptideBackgroundRatio = 5;


            foreach (var scanset in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scanset;
                ((UIMFRun)run).CurrentFrameSet = new FrameSet(1);
                msgen.Execute(run.ResultCollection);

                peakDet.Execute(run.ResultCollection);

                decon.Execute(run.ResultCollection);

            }

            


        }


        [Test]
        public void criticalErrorInPeakDetectorTest1()
        {
            UIMFRun run = new DeconTools.Backend.Runs.UIMFRun(fpgaUIMFFilePath2);

            FrameSetCollectionCreator ffcc = new FrameSetCollectionCreator(run, 425, 432, 3, 1);
            ffcc.Create();

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 1,500, 9, 1);
            sscc.Create();

            MSGeneratorFactory factory = new MSGeneratorFactory();
            Task msgen = factory.CreateMSGenerator(run.MSFileType);

            DeconToolsPeakDetector peakDet = new DeconToolsPeakDetector();
            peakDet.PeakBackgroundRatio = 4;
            peakDet.SigNoiseThreshold = 3;


            foreach (var frame in run.FrameSetCollection.FrameSetList)
            {
                run.CurrentFrameSet = frame;

                foreach (var scan in run.ScanSetCollection.ScanSetList)
                {
                    run.CurrentScanSet = scan;

                    msgen.Execute(run.ResultCollection);
                    peakDet.Execute(run.ResultCollection);
                }

                
            }


        }


        [Test]
        public void checkIntegrityOfUIMF_RawDataTest1()
        {
            UIMFRun run = new DeconTools.Backend.Runs.UIMFRun(fpgaUIMFFilePath2);

            FrameSetCollectionCreator ffcc = new FrameSetCollectionCreator(run, 696, 696, 3, 1);
            ffcc.Create();

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 350, 500, 9, 1);
            sscc.Create();

            MSGeneratorFactory factory = new MSGeneratorFactory();
            Task msgen = factory.CreateMSGenerator(run.MSFileType);

            DeconToolsPeakDetector peakDet = new DeconToolsPeakDetector();
            peakDet.PeakBackgroundRatio = 6;
            peakDet.SigNoiseThreshold = 3;


            StringBuilder sb = new StringBuilder();


            sb.Append("frame\tscan\tmaxIntens\tnumZeros\n");

            foreach (var frame in run.FrameSetCollection.FrameSetList)
            {
                run.CurrentFrameSet = frame;

                foreach (var scan in run.ScanSetCollection.ScanSetList)
                {
                    run.CurrentScanSet = scan;

                    msgen.Execute(run.ResultCollection);

                    List<int> indices = getIndicesOf0MZValues(run.XYData.Xvalues);

                    double maxY = run.XYData.GetMaxY();


                    if (indices.Count > -1)
                    {
                        sb.Append(frame.PrimaryFrame);
                        sb.Append("\t");
                        sb.Append(scan.PrimaryScanNumber);
                        sb.Append("\t");
                        sb.Append(maxY);
                        sb.Append("\t");
                        sb.Append(indices.Count);
                        sb.Append("\n");
                    }
               }


            }

            Console.WriteLine(sb.ToString());

        }

        private List<int> getIndicesOf0MZValues(double[] xvals)
        {
            List<int> indexList = new List<int>();

            for (int i = 0; i < xvals.Length; i++)
            {
                if ((int)xvals[i] == 0)
                {
                    indexList.Add(i);

                }
            }

            return indexList;
        }




        private string getXYValuesForDisplay(double[] xvals, double[] yvals)
        {
            if (xvals == null || yvals == null) return null;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < xvals.Length; i++)
            {
                sb.Append(xvals[i]);
                sb.Append("\t");
                sb.Append(yvals[i]);
                sb.Append("\n");

            }
            return sb.ToString();
        }



        private string getResultSummary(IsosResult isosResult)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(isosResult.ScanSet.PrimaryScanNumber);
            sb.Append("\t");
            sb.Append(isosResult.ScanSet.NumPeaks);
            sb.Append("\t");

            return sb.ToString();
        }




    }
}
