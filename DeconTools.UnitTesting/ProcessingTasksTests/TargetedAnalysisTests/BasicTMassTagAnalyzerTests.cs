using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;

namespace DeconTools.UnitTesting.ProcessingTasksTests.TargetedAnalysisTests
{
    [TestFixture]
    public class BasicTMassTagAnalyzerTests
    {
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

        [Test]
        public void test1()
        {


            MassTag mt = new MassTag();
            mt.ID = 86963986;
            mt.MonoIsotopicMass = 1516.791851;
            mt.PeptideSequence = "AAKEGISCEIIDLR";
            mt.NETVal = 0.2284955f;
            mt.CreatePeptideObject();
            mt.ChargeState = 2;

            Run run = new XCaliburRun(xcaliburTestfile);

            run.CurrentMassTag = mt;

         

            ScanSet scan = new ScanSet(5509);    //see '5512' elsewhere for how I get this number.
            run.CurrentScanSet = scan;

            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            peakParams.PeakBackgroundRatio = 1.3;
            peakParams.SignalToNoiseThreshold = 2.0;
            peakParams.ThresholdedData = true;
            Task peakDet = new DeconToolsPeakDetector(peakParams);
            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task targetedFeatureFinder = new BasicTFeatureFinder();



            msgen.Execute(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);
            theorFeatureGen.Execute(run.ResultCollection);
            targetedFeatureFinder.Execute(run.ResultCollection);

            Assert.AreEqual(1, run.ResultCollection.MassTagResultList.Count);

            IMassTagResult result = run.ResultCollection.GetMassTagResult(mt);
            Assert.AreNotEqual(null, result);
            Assert.AreEqual(mt, result.MassTag);
            Assert.AreEqual(759.403435061313m, (decimal)result.IsotopicProfile.Peaklist[0].XValue);
            Assert.AreEqual(14191370m, (decimal)result.IsotopicProfile.Peaklist[0].Height);
            Assert.AreEqual(0.0163658764213324m, (decimal)result.IsotopicProfile.GetFWHM());


        }


    }
}
