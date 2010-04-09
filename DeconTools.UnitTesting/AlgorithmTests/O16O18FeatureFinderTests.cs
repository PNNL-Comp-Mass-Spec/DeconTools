using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;

namespace DeconTools.UnitTesting.AlgorithmTests
{
    [TestFixture]
    public class O16O18FeatureFinderTests
    {
        string bsaO16O18file1 = @"F:\Gord\Data\O16O18\Weijun\TechTest_O18_01\TechTest_O18_01_RunA_10Dec09_Doc_09-11-08.RAW";


        [Test]
        public void test1()
        {

            Run run = new XCaliburRun(bsaO16O18file1);

            MSGeneratorFactory msgenFact = new MSGeneratorFactory();
            Task msgen = msgenFact.CreateMSGenerator(run.MSFileType);

            DeconToolsPeakDetector peakDet = new DeconToolsPeakDetector(1.3, 2, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, true);
           
            O16O18FeatureFinder finder = new O16O18FeatureFinder();


            List<MassTag> massTagList = TestUtilities.CreateO16O18TestMassTagList1();

            run.CurrentMassTag = massTagList[0];

            TomTheorFeatureGenerator theorFeatureGen = new TomTheorFeatureGenerator();
            theorFeatureGen.GenerateTheorFeature(run.CurrentMassTag);

            run.CurrentScanSet = new ScanSet(3294);

            msgen.Execute(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);

            IsotopicProfile o16O18profile = finder.FindFeature(run.PeakList, run.CurrentMassTag.IsotopicProfile, 10, true);

            TestUtilities.DisplayIsotopicProfileData(o16O18profile);

            
        }


    }
}
