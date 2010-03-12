using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;

namespace DeconTools.UnitTesting.ProcessingTasksTests.TargetedAnalysisTests
{
    [TestFixture]
    public class MassTagFitScoreCalculatorTests
    {
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

        [Test]
        public void test1()
        {
            Run run = new XCaliburRun(xcaliburTestfile);
            
            List<MassTag> mass_tagList = TestUtilities.CreateTestMassTagList();
            MassTag mt = mass_tagList[0];
            
            run.CurrentScanSet = new ScanSet(9017, new int[] { 9010, 9017, 9024 });
            run.CurrentMassTag = mt;


            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);

            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters(2, 1.3, true, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            Task mspeakDet = new DeconToolsPeakDetector(peakParams);

            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task targetedFeatureFinder = new BasicTFeatureFinder(0.01);

            MassTagFitScoreCalculator fitScoreCalc = new MassTagFitScoreCalculator();

            msgen.Execute(run.ResultCollection);

            //run.XYData.Display();

            mspeakDet.Execute(run.ResultCollection);
            theorFeatureGen.Execute(run.ResultCollection);
            targetedFeatureFinder.Execute(run.ResultCollection);
            fitScoreCalc.Execute(run.ResultCollection);

            MassTagResultBase result = run.ResultCollection.GetMassTagResult(mt);
            TestUtilities.DisplayIsotopicProfileData(result.IsotopicProfile);
            Console.WriteLine("Fit val = " + result.IsotopicProfile.Score);

            /*
             * 
             * 
             * ------------------- MassTag = 24769---------------------------
monoMass = 2086.0595; monoMZ = 1044.0370; ChargeState = 2; NET = 0.452; Sequence = DFNEALVHQVVVAYAANAR

****** Match ******
NET = 	0.452
ChromPeak ScanNum = 9016.48992535631
ChromPeak NETVal = 0.453
ScanSet = { 9010, 9017, 9024, } 
Observed MZ and intensity = 1044.03290771556	1.269842E+07
------------------------------ end --------------------------
             * 
             * 
             * 
             * 
             * 
             */



        }

        public MassTag createMassTag1()
        {
            MassTag mt = new MassTag();
            mt.ID = 24769;
            mt.MonoIsotopicMass = 2086.0595;
            mt.ChargeState = 2;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;
            mt.PeptideSequence = "DFNEALVHQVVVAYAANAR";
            mt.CreatePeptideObject();

            return mt;


        }


    }
}
