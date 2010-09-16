using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks;
using System.Collections;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.MSPeakDetectionTests
{
    [TestFixture]
    public class MSPeakDetectionTests
    {
        [Test]
        public void DetectPeaksInOrbitrapData()
        {
            double peakBR = 1.3;
            double sigNoise = 2;
            bool isThresholded = true;
            DeconTools.Backend.Globals.PeakFitType peakfitType = DeconTools.Backend.Globals.PeakFitType.QUADRATIC;

            string testFile = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

            Run run = new XCaliburRun(testFile);
            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, 6000, 6015, 1, 1);
            sscc.Create();

            MSGeneratorFactory msFactory = new MSGeneratorFactory();



            Task msgen = msFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsPeakDetector peakDet = new DeconToolsPeakDetector(peakBR, sigNoise, peakfitType, isThresholded);
            peakDet.StorePeakData = true;

            foreach (var scan in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scan;

                msgen.Execute(run.ResultCollection);
                peakDet.Execute(run.ResultCollection);

            }

            StringBuilder sb = new StringBuilder();


            foreach (var peak in run.ResultCollection.MSPeakResultList)
            {
                sb.Append(peak.PeakID);
                sb.Append("\t");
                sb.Append(peak.Scan_num);
                sb.Append("\t");
                sb.Append(peak.MSPeak.XValue);
                sb.Append("\t");
                sb.Append(peak.MSPeak.Height);
                sb.Append(Environment.NewLine);

                
            }


            //IEnumerable<double> mzValues = (from n in run.ResultCollection.MSPeakResultList select n.MSPeak.XValue);

            //pull out mz values
            List<double> mzValues = (from n in run.ResultCollection.MSPeakResultList select n.MSPeak.XValue).ToList();

            //pull out all peaks within an mz range
            var query = (from n in run.ResultCollection.MSPeakResultList where n.MSPeak.XValue > 800 && n.MSPeak.XValue<850 select n);

            //get max of filtered peaks from above
            var max = query.Max(p => p.MSPeak.Height);

            //sort the list
            var sortedList = run.ResultCollection.MSPeakResultList.OrderBy(p => p.MSPeak.XValue);


            foreach (var item in mzValues)
            {
                Console.Write(item);
                Console.Write('\n');
            }
            


            //Console.WriteLine(sb.ToString());




        }


    }
}
