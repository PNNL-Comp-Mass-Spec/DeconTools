using System;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
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

            var saturationDetector = new SaturationDetector();

            var testFile = FileRefs.RawDataMSFiles.UIMFStdFile3;
          

            var run = (UIMFRun)new RunFactory().CreateRun(testFile);


            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);
            var zeroFiller = new DeconToolsZeroFiller();
         

           var peakDetector =new DeconToolsPeakDetectorV2(4,3,Globals.PeakFitType.QUADRATIC,false);
        
            var decon = new HornDeconvolutor();
            decon.LeftFitStringencyFactor = 2.5;
            decon.RightFitStringencyFactor = 0.5;


            var startScan = 100;
            var stopScan = 200;


            for (var i = startScan; i < stopScan; i++)
            {

                var frame = new ScanSet(500);

                var primaryScan = i;

                var scan = new ScanSet(primaryScan, primaryScan - 3 , primaryScan +3);


                run.CurrentScanSet = frame;
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
                                       result.IntensityAggregate + "\t" +
                                       result.IsotopicProfile.IntensityAggregateAdjusted);
                 }
            }


           

           


            //TestUtilities.DisplayIsotopicProfileData(testIso1);
            //TestUtilities.DisplayPeaks(run.PeakList);




        }

    }
}
