using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.ZeroFillers;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests
{
    [TestFixture]
    public class SaturationDetectorTests
    {
        [Test]
        public void Test1()
        {

            SaturationDetector saturationDetector = new SaturationDetector();

            string testFile = FileRefs.RawDataMSFiles.UIMFStdFile3;
          

            var run = (UIMFRun)new RunFactory().CreateRun(testFile);


            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsZeroFiller zeroFiller = new DeconToolsZeroFiller();
         

           DeconToolsPeakDetector peakDetector =new DeconToolsPeakDetector(4,3,Globals.PeakFitType.QUADRATIC,false);
        
            HornDeconvolutor decon = new HornDeconvolutor();
            decon.LeftFitStringencyFactor = 2.5;
            decon.RightFitStringencyFactor = 0.5;


            int startScan = 100;
            int stopScan = 200;


            for (int i = startScan; i < stopScan; i++)
            {

                FrameSet frame = new FrameSet(500);

                int primaryScan = i;

                ScanSet scan = new ScanSet(primaryScan, primaryScan - 3 , primaryScan +3);


                run.CurrentFrameSet = frame;
                run.CurrentScanSet = scan;


                msgen.Execute(run.ResultCollection);

                zeroFiller.Execute(run.ResultCollection);

                peakDetector.Execute(run.ResultCollection);

                decon.Execute(run.ResultCollection);

                saturationDetector.GetUnsummedIntensitiesAndDetectSaturation(run, run.ResultCollection.IsosResultBin);

            
            }


            var msfeatureElution = (from n in run.ResultCollection.ResultList
                                    where
                                        n.IsotopicProfile.MonoIsotopicMass > 1253.71 &&
                                        n.IsotopicProfile.MonoIsotopicMass < 1253.73
                                    select n);


            foreach (var result in msfeatureElution)
            {
                 if (result.IsotopicProfile.IsSaturated ||true)
                 {
                     Console.WriteLine(result.ScanSet.PrimaryScanNumber +"\t" +
                                       result.IsotopicProfile.MonoPeakMZ.ToString("0.0000") + "\t" +
                                       result.IsotopicProfile.IntensityAggregate + "\t" +
                                       result.IsotopicProfile.IntensityAggregateAdjusted);
                 }
            }


           

           


            //TestUtilities.DisplayIsotopicProfileData(testIso1);
            //TestUtilities.DisplayPeaks(run.PeakList);




        }

    }
}
