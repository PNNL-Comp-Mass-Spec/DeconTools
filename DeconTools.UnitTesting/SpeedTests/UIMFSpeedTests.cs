using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks;
using System.Linq;

namespace DeconTools.UnitTesting.SpeedTests
{


    public struct timingResult
    {
        public int frameNum;
        public int scanNum;
        public long msGenTime;
        public long peakDetectorTime;
        public long deconTime;
        public long resultUpdaterTime;
        public long ticExtractorTime;
        public long driftTimeExtractorTime;
    }

    public struct frameAggregateTimingResult
    {
        public int frameNum;

        public long sumMSGen;
        public long sumPeakDet;
        public long sumDecon;
        public long sumResultUpdater;
        public long sumTicExtractor;
    }


    [TestFixture]
    public class UIMFSpeedTests
    {
        public string uimfFilepath = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000_V2009_05_28.uimf";
        public string uimfFilepath2 = "..\\..\\TestFiles\\SMSL_8_rep2_30minF_c4_500_20_fr1725_0000.uimf";
        string uimfFilePath3 = "..\\..\\TestFiles\\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf";

        [Test]
        public void msGeneratorSummingTest1()
        {

            
            Run run = new UIMFRun(uimfFilePath3);
            int startFrame = 800;
            int stopFrame = 802;

            int numFramesSummed = 3;
            int numScansSummed = 9;


            FrameSetCollectionCreator framesetCreator = new FrameSetCollectionCreator(run, startFrame, stopFrame, numFramesSummed, 1);
            framesetCreator.Create();

            ScanSetCollectionCreator scanSetCreator = new ScanSetCollectionCreator(run, numScansSummed, 1);
            scanSetCreator.Create();

            ResultCollection results = new ResultCollection(run);
            List<timingResult> timingResults = new List<timingResult>();

            Task msgen = new UIMF_MSGenerator();
            Task peakDetector = new DeconToolsPeakDetector();
            Task decon = new HornDeconvolutor();
            Task scanResultUpdater = new ScanResultUpdater();
            Task uimfTicExtractor = new UIMF_TICExtractor();
            Task driftTimeExtractor = new DeconTools.Backend.ProcessingTasks.UIMFDriftTimeExtractor();
           
            
            ((HornDeconvolutor)decon).MinPeptideBackgroundRatio = 4;

            foreach (FrameSet frameset in ((UIMFRun)run).FrameSetCollection.FrameSetList)
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

 
              

                ((UIMFRun)run).CurrentFrameSet = frameset;
                foreach (ScanSet scanset in run.ScanSetCollection.ScanSetList)
                {

                    
                    sw.Reset();
                    timingResult timeresult = new timingResult();
                    timeresult.frameNum = frameset.PrimaryFrame;
                    timeresult.scanNum = scanset.PrimaryScanNumber;
                    sw.Start();

                    run.CurrentScanSet = scanset;
                    
                    msgen.Execute(results);
                    timeresult.msGenTime = sw.ElapsedMilliseconds;

                    sw.Reset();
                    sw.Start();
                    peakDetector.Execute(results);
                    timeresult.peakDetectorTime = sw.ElapsedMilliseconds;

                    sw.Reset();
                    sw.Start();
                    decon.Execute(results);
                    timeresult.deconTime = sw.ElapsedMilliseconds;

                    sw.Reset();
                    sw.Start();
                    scanResultUpdater.Execute(results);
                    timeresult.resultUpdaterTime = sw.ElapsedMilliseconds;

                    sw.Reset();
                    sw.Start();
                    uimfTicExtractor.Execute(results);
                    timeresult.ticExtractorTime = sw.ElapsedMilliseconds;


                    sw.Reset();
                    sw.Start();
                    driftTimeExtractor.Execute(results);

                    timeresult.driftTimeExtractorTime = sw.ElapsedMilliseconds;

                    timingResults.Add(timeresult);

                    

                }
                
 


            }

            //reportTimingEachFrameEachScan(timingResults);

            Console.WriteLine("Total _isos = " + results.ResultList.Count);
            Console.WriteLine("PeptideBR = " + ((HornDeconvolutor)decon).MinPeptideBackgroundRatio);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            reportTiming_FrameAggregate(timingResults, startFrame, stopFrame);
            Project.Reset();
        }


        [Test]
        public void msGeneratorsum600scansTest1()
        {
            Run run = new UIMFRun(uimfFilepath);
            int startFrame = 1200;
            int stopFrame = 1201;

            int numFramesSummed = 1;
            int numScansSummed = 600;

            FrameSetCollectionCreator framesetCreator = new FrameSetCollectionCreator(run, startFrame, stopFrame, numFramesSummed, 1);
            framesetCreator.Create();

          
            ScanSet sixHundredScanset = new ScanSet(300, 0, 599);   

            ResultCollection results = new ResultCollection(run);
            List<timingResult> timingResults = new List<timingResult>();

            foreach (FrameSet frameset in ((UIMFRun)run).FrameSetCollection.FrameSetList)
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

                ((UIMFRun)run).CurrentFrameSet = frameset;
                
                    sw.Reset();
                    timingResult timeresult = new timingResult();
                    timeresult.frameNum = frameset.PrimaryFrame;
                    timeresult.scanNum = sixHundredScanset.PrimaryScanNumber;
                    sw.Start();

                    run.CurrentScanSet = sixHundredScanset;
                    Task msgen = new UIMF_MSGenerator();
                    msgen.Execute(results);
                    timeresult.msGenTime = sw.ElapsedMilliseconds;

                    sw.Reset();
                    sw.Start();
                    Task peakDetector = new DeconToolsPeakDetector();
                    peakDetector.Execute(results);
                    timeresult.peakDetectorTime = sw.ElapsedMilliseconds;

                    sw.Reset();
                    sw.Start();
                    Task rapid = new RapidDeconvolutor();
                    rapid.Execute(results);
                    timeresult.deconTime = sw.ElapsedMilliseconds;

                    sw.Reset();
                    sw.Start();
                    //Task scanResultUpdater = new ScanResultUpdater();
                    //scanResultUpdater.Execute(results);
                    //timeresult.resultUpdaterTime = sw.ElapsedMilliseconds;

                    timingResults.Add(timeresult);
                


            }

            reportTimingEachFrameEachScan(timingResults);

            //reportTiming_FrameAggregate(timingResults, startFrame, stopFrame);

        }




        private void reportTiming_FrameAggregate(List<timingResult> timingResults, int startFrame, int stopFrame)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("frameNum\tmsgen\tpeakDet\tdecon\tupdater\tTICExtractor\n");

            List<frameAggregateTimingResult> frameAggList = new List<frameAggregateTimingResult>();

            for (int i = startFrame; i <= stopFrame; i++)
            {
                frameAggregateTimingResult frameAgg = new frameAggregateTimingResult();

                var frameResult = from n in timingResults
                                  where n.frameNum == i
                                  select n;

                frameAgg.frameNum = i;
                frameAgg.sumMSGen = frameResult.Sum(p => p.msGenTime);
                frameAgg.sumPeakDet = frameResult.Sum(p => p.peakDetectorTime);
                frameAgg.sumDecon = frameResult.Sum(p => p.deconTime);
                frameAgg.sumResultUpdater = frameResult.Sum(p => p.resultUpdaterTime);
                frameAgg.sumTicExtractor = frameResult.Sum(p => p.ticExtractorTime);

                frameAggList.Add(frameAgg);
            }

            foreach (frameAggregateTimingResult result in frameAggList)
            {
                sb.Append(result.frameNum);
                sb.Append("\t");
                sb.Append(result.sumMSGen);
                sb.Append("\t");
                sb.Append(result.sumPeakDet);
                sb.Append("\t");
                sb.Append(result.sumDecon);
                sb.Append("\t");
                sb.Append(result.sumResultUpdater);
                sb.Append("\t");
                sb.Append(result.sumTicExtractor);
                sb.Append("\n");

            }

            Console.WriteLine(sb.ToString());




        }

        private void reportTimingEachFrameEachScan(List<timingResult> timingResults)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("frameNum\tscanNum\tmsgen\tpeakDet\tdecon\tupdater\n");
            foreach (timingResult timeresult in timingResults)
            {
                sb.Append(timeresult.frameNum);
                sb.Append("\t");
                sb.Append(timeresult.scanNum);
                sb.Append("\t");
                sb.Append(timeresult.msGenTime);
                sb.Append("\t");
                sb.Append(timeresult.peakDetectorTime);
                sb.Append("\t");
                sb.Append(timeresult.deconTime);
                sb.Append("\t");
                sb.Append(timeresult.resultUpdaterTime);
                sb.Append("\n");

            }
            Console.WriteLine(sb.ToString());
        }


    }
}
