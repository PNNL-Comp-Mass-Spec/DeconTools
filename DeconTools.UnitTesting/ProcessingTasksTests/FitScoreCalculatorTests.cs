using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;


namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    public class FitScoreCalculatorTests
    {
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        
        
        /// <summary>
        /// Test first extracts Horn score for a profile @ 481.2 (+2) of Scan 6005
        /// Then extracts RAPID's score for same profile
        /// Then uses DeconToolsFitScoreCalculator to get the score following RAPID deconvolution
        /// </summary>
        [Test]
        public void turnRAPIDScoreIntoHornScoreAndCompareTest1()
        {

            Run run = new XCaliburRun(xcaliburTestfile, 6000, 6050);


            int numScansSummed = 1;

            ScanSetCollectionCreator scanSetCreator = new ScanSetCollectionCreator(run, run.MinScan, run.MaxScan, numScansSummed, 1, false);
            scanSetCreator.Create();

            Task msgen = new GenericMSGenerator();

            DeconToolsV2.Peaks.clsPeakProcessorParameters detParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            detParams.PeakBackgroundRatio = 3;
            detParams.SignalToNoiseThreshold = 3;

            Task peakDet = new DeconToolsPeakDetector(detParams);

            Task rapid = new RapidDeconvolutor();
            Task horn = new HornDeconvolutor();

            Task fitcalc = new DeconToolsFitScoreCalculator();

            run.CurrentScanSet = run.ScanSetCollection.GetScanSet(6005);
            msgen.Execute(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);
            horn.Execute(run.ResultCollection);

            //string isosData = reportIsotopicProfiles(run);
            //Console.Write(isosData);

            //First evaluate HornTransform score
            Assert.AreEqual(481.274105402604, (decimal)run.ResultCollection.ResultList[0].IsotopicProfile.GetMZ());
            Assert.AreEqual(0.0101245114907111, (decimal)run.ResultCollection.ResultList[0].IsotopicProfile.Score);

            run.ResultCollection.ClearAllResults();
            
            //Run RAPID and evaluate the score Rapid gives it
            rapid.Execute(run.ResultCollection);
            Assert.AreEqual(481.274105402604, (decimal)run.ResultCollection.ResultList[7].IsotopicProfile.GetMZ());
            Assert.AreEqual(1.52062147024669, (decimal)run.ResultCollection.ResultList[7].IsotopicProfile.Score);

            
            //Run the DeconToolsFitScoreCalculator
            fitcalc.Execute(run.ResultCollection);

            //Evaluate the score given by the fitscore calculator
            Assert.AreNotEqual(1.52062147024669, (decimal)run.ResultCollection.ResultList[7].IsotopicProfile.Score);
            Assert.AreEqual(0.00743059540799006m, (decimal)run.ResultCollection.ResultList[7].IsotopicProfile.Score);
        }

        private string reportIsotopicProfiles(Run run)
        {
            StringBuilder sb = new StringBuilder();

            int counter = 0;
            foreach (IsosResult result in run.ResultCollection.ResultList)
            {
                sb.Append(counter);
                sb.Append("\t");
                sb.Append(result.IsotopicProfile.getMonoPeak().MZ.ToString("0.000"));
                sb.Append("\t");
                sb.Append(result.IsotopicProfile.ChargeState);
                sb.Append("\t");
                sb.Append(result.IsotopicProfile.GetAbundance().ToString("0.0"));
                sb.Append("\t");
                sb.Append(result.IsotopicProfile.Score.ToString("0.00"));
                sb.Append("\n");

                counter++;

            }
            return sb.ToString();
        }


    }
}
