using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.DTO;
using System.Diagnostics;
using DeconTools.Backend;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.ChromatogramRelatedTests
{
    [TestFixture]
    public class PeakChromatogramRelatedTests
    {
        [Test]
        public void getPeakChromatogramTest1()
        {
            double chromToleranceInPPM = 10;

            MassTag mt = TestUtilities.GetMassTagStandard(1);

            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            run.CurrentMassTag = mt;

            TomTheorFeatureGenerator unlabelledTheorGenerator = new TomTheorFeatureGenerator();
            unlabelledTheorGenerator.GenerateTheorFeature(mt);

            Task peakChromGen = new PeakChromatogramGenerator(chromToleranceInPPM);
            peakChromGen.Execute(run.ResultCollection);

            Assert.AreEqual(133, run.XYData.Xvalues.Length);
            Assert.AreEqual(5543, (int)run.XYData.Xvalues[35]);
            Assert.AreEqual(7319569, (int)run.XYData.Yvalues[35]);
            run.XYData.Display();
        }


        [Test]
        public void getPeakChromatogramInTheFormOfMSPeakResult_Test1()
        {
            double chromToleranceInPPM = 20;
            double targetMZ = 831.48;


            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);


            ChromatogramGenerator chromGen = new ChromatogramGenerator();
            List<MSPeakResult> filteredMSPeaks = chromGen.GeneratePeakChromatogram(run.ResultCollection.MSPeakResultList, run.MinScan, run.MaxScan, targetMZ, chromToleranceInPPM);

            //Assert.AreEqual(56, filteredMSPeaks.Count);

            TestUtilities.DisplayMSPeakResults(filteredMSPeaks);

        }

        [Test]
        public void getPeakChromAndStoreInPeakChromObject_Test1()
        {
            double chromToleranceInPPM = 20;
            double targetMZ = 831.48;


            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            ChromatogramGenerator chromGen = new ChromatogramGenerator();

            PeakChrom chrom = new BasicPeakChrom();
            chrom.Data = chromGen.GeneratePeakChromatogram(run.ResultCollection.MSPeakResultList, run.MinScan, run.MaxScan, targetMZ, chromToleranceInPPM);

            Assert.AreEqual(59, chrom.Data.Count);

            //for XCalibur data, zeros will be inserted for non-contiguous scans.  Since this dataset has MS/MS, there are many zeros inserted. 
            //these zeros are removed in the PeakChromGenerator - which is coupled to the Run (and thus knows where the MS/MS scans are).  
            //The ChromatogramGenerator is not coupled to the Dataset. 
            Assert.AreEqual(1207, chrom.XYData.Xvalues.Length);


        }





        [Test]
        public void getPeakChromatogramTest2()
        {
            MassTag mt = TestUtilities.GetMassTagStandard(1);

            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            run.CurrentMassTag = mt;

            TomTheorFeatureGenerator unlabelledTheorGenerator = new TomTheorFeatureGenerator();
            unlabelledTheorGenerator.GenerateTheorFeature(mt);

            PeakChromatogramGenerator peakChromGen = new PeakChromatogramGenerator(10, ChromatogramGeneratorMode.TOP_N_PEAKS);
            peakChromGen.TopNPeaksLowerCutOff = 0.4;
            peakChromGen.Execute(run.ResultCollection);

            Assert.AreEqual(133, run.XYData.Xvalues.Length);
            Assert.AreEqual(5543, (int)run.XYData.Xvalues[35]);
            //Assert.AreEqual(7319569, (int)run.XYData.Yvalues[35]);
            run.XYData.Display();


        }


        [Test]
        public void getPeakChromatogramUsingChromGenTest1()
        {
            double targetMZ = 831.48;
            double toleranceInPPM = 20;

            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            ChromatogramGenerator chromGen = new ChromatogramGenerator();
            run.XYData = chromGen.GenerateChromatogram(run.ResultCollection.MSPeakResultList, run.MinScan, run.MaxScan, targetMZ, toleranceInPPM, 1);

            for (int i = 0; i < run.XYData.Xvalues.Length; i++)
            {
                run.XYData.Xvalues[i] = run.GetTime(i);
            }

            // run.XYData.Display();


            int counter = 0;
            StringBuilder sb = new StringBuilder();
            var sortedList = run.ResultCollection.MSPeakResultList.OrderBy(p => p.MSPeak.XValue);
            foreach (var peak in sortedList)
            {
                counter++;
                if (peak.MSPeak.XValue > (targetMZ - 0.1) && peak.MSPeak.XValue < (targetMZ + 0.1))
                {
                    sb.Append(peak.ChromID);
                    sb.Append("\t");
                    sb.Append(peak.PeakID);
                    sb.Append("\t");
                    sb.Append(peak.Scan_num);
                    sb.Append("\t");
                    sb.Append(peak.MSPeak.XValue);
                    sb.Append("\t");
                    sb.Append(peak.MSPeak.Height);
                    sb.Append(Environment.NewLine);

                }



            }

            Console.WriteLine(sb.ToString());




        }


        [Test]
        public void getChromatogramsSpeedTest1()
        {

            double targetMZ = 831.48;
            double toleranceInPPM = 20;

            int totalIterations = 40;


            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile1);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            ChromatogramGenerator chromGen = new ChromatogramGenerator();

            List<long> speedResults = new List<long>();

            Stopwatch sw = new Stopwatch();

            for (int i = 0; i < totalIterations; i++)
            {
                sw.Reset();
                sw.Start();

                run.XYData = chromGen.GenerateChromatogram(run.ResultCollection.MSPeakResultList, run.MinScan, run.MaxScan, (targetMZ + totalIterations), toleranceInPPM, 1);
                sw.Stop();

                speedResults.Add(sw.ElapsedMilliseconds);

            }

            Console.WriteLine("Average time = " + speedResults.Average());





        }




        [Test]
        public void getPeakChromatogramsForManyPeaks_Test1()
        {



            Dictionary<long, int> peakFrequency = new Dictionary<long, int>();

            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile1);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);


            ChromatogramGenerator chromGen = new ChromatogramGenerator();
            // PeakChromatogramGenerator peakChromGen = new PeakChromatogramGenerator(


            long timeForSort = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var sortedList = run.ResultCollection.MSPeakResultList.OrderByDescending(p => p.MSPeak.Height);
            sw.Stop();
            timeForSort = sw.ElapsedMilliseconds;

            long timeToList = 0;
            sw = new Stopwatch();
            sw.Start();


            List<MSPeakResult> sortedMSPeakResultList = sortedList.ToList();
            sw.Stop();
            timeToList = sw.ElapsedMilliseconds;


            List<long> chromGenTimes = new List<long>();




            Stopwatch allChromsStopwatch = new Stopwatch();
            allChromsStopwatch.Start();

            Stopwatch chromGenStopwatch = new Stopwatch();

            //iterate over the MSPeakResults and pull out chromatograms

            int counter = -1;
            foreach (var peakResult in sortedMSPeakResultList)
            {
                counter++;

                if (counter > 1000) break;
                if (peakResult.ChromID != -1)
                {
                    continue;
                }
                chromGenStopwatch.Reset();
                chromGenStopwatch.Start();
                run.XYData = chromGen.GenerateChromatogram(run.ResultCollection.MSPeakResultList, run.MinScan, run.MaxScan, peakResult.MSPeak.XValue, 20, counter);
                chromGenStopwatch.Stop();
                chromGenTimes.Add(chromGenStopwatch.ElapsedMilliseconds);





            }


            allChromsStopwatch.Stop();



            Console.WriteLine("Original peaklist count = " + run.ResultCollection.MSPeakResultList.Count);
            Console.WriteLine("Sort time = " + timeForSort);
            Console.WriteLine("Conversion to List = " + timeToList);
            Console.WriteLine("Number of times chrom generated = " + chromGenTimes.Count);
            Console.WriteLine("Average chrom times = " + chromGenTimes.Average());
            Console.WriteLine("Total chrom time in seconds = " + (double)allChromsStopwatch.ElapsedMilliseconds / 1000d);


        }


        [Test]
        public void getPeakChromatogramsForManyPeaks_Test2()
        {
            Dictionary<long, int> peakFrequency = new Dictionary<long, int>();

            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile1);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            Stopwatch sw = new Stopwatch();
            sw.Start();


            List<int> indexArray = new List<int>();

            for (int i = 3000; i < 5000; i++)
            {
                IEnumerable<MSPeakResult> query = (from n in run.ResultCollection.MSPeakResultList where n.Scan_num == i select n);


                //List<MSPeakResult> filteredResults = query.ToList();
                var indexQuery = (from n in query select n.PeakID);



                foreach (var peak in query)
                {
                }


            }
            sw.Stop();

            Console.WriteLine("time = " + sw.ElapsedMilliseconds);



        }


        [Test]
        public void getPeakChromatogramsForManyPeaks_Test3()
        {
            Dictionary<long, int> peakFrequency = new Dictionary<long, int>();

            Run run = new XCaliburRun(FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(FileRefs.PeakDataFiles.OrbitrapPeakFile1);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            Stopwatch sw = new Stopwatch();
            sw.Start();


            List<int> indexArray = new List<int>();

            for (int i = 3000; i < 5000; i++)
            {
                IEnumerable<MSPeakResult> query = (from n in run.ResultCollection.MSPeakResultList where n.Scan_num == i select n);


                //List<MSPeakResult> filteredResults = query.ToList();
                var indexQuery = (from n in query select n.PeakID);



                foreach (var peak in query)
                {
                }


            }
            sw.Stop();

            Console.WriteLine("time = " + sw.ElapsedMilliseconds);



        }





    }
}
