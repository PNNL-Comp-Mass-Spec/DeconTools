using System;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Runs;
using DeconTools.UnitTesting2.QuantificationTests;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.TargetedFeatureFinderTests
{
    [TestFixture]
    public class TFFTests
    {
        [Test]
        public void unlabelled_data_TFFTest1()
        {
            var massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_first10.txt";

            var masstagImporter = new MassTagFromTextFileImporter(massTagFile);
            var massTagColl = masstagImporter.Import();

            var mt24702_charge3 = (from n in massTagColl.TargetList where n.ID == 24702 && n.ChargeState == 3 select n).First();
            var mt24702_charge4 = (from n in massTagColl.TargetList where n.ID == 24702 && n.ChargeState == 4 select n).First();

        }



        [Test]
        public void n14N15LabelledData_TFFTest1()
        {
            double featureFinderTol = 15;

            var n14n15Util = new N14N15TestingUtilities();
            //get sample MS from Test Data
            var massSpectrum = n14n15Util.GetSpectrumAMTTag23140708_Z3_Sum3();  //this is the diff b/w previous test and this one
            var mt23140708 = n14n15Util.CreateMT23140708_Z3();


            //get ms peaks
            var peakDet = new DeconToolsPeakDetectorV2(1.3, 2);
            var msPeakList = peakDet.FindPeaks(massSpectrum);

            //TestUtilities.DisplayPeaks(msPeakList);

            //generate theor unlabelled profile
            var unlabelledfeatureGen = new TomTheorFeatureGenerator();
            unlabelledfeatureGen.GenerateTheorFeature(mt23140708);

            //generate theor N15-labelled profile
            var n15featureGen = new TomTheorFeatureGenerator(Globals.LabellingType.N15, 0.005);
            n15featureGen.GenerateTheorFeature(mt23140708);


            //find features in experimental data, using the theoretical profiles
            var msfeatureFinder = new BasicTFF
            {
                ToleranceInPPM = featureFinderTol,
                NeedMonoIsotopicPeak = false
            };

            var n14profile = msfeatureFinder.FindMSFeature(msPeakList, mt23140708.IsotopicProfile);
            var n15profile = msfeatureFinder.FindMSFeature(msPeakList, mt23140708.IsotopicProfileLabelled);

            Console.WriteLine(mt23140708.GetEmpiricalFormulaFromTargetCode());


            TestUtilities.DisplayIsotopicProfileData(mt23140708.IsotopicProfile);

            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(mt23140708.IsotopicProfileLabelled);

            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(n14profile);

            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(n15profile);



        }



        [Test]
        public void getVeryLowN15ProfileWithIterativeTFFTest1()
        {
            var n14n15Util = new N14N15TestingUtilities();
            //get sample MS from Test Data

            var rf=new RunFactory();
            var run = rf.CreateRun(N14N15TestingUtilities.MS_AMTTag23085904_z2_sum1_lowN15);


            run.XYData = N14N15TestingUtilities.GetTestSpectrum(N14N15TestingUtilities.MS_AMTTag23085904_z2_sum1_lowN15);

            var mt23140708 = n14n15Util.CreateMT23085904_Z2();
            run.CurrentMassTag = mt23140708;
            run.ResultCollection.ResultType = Globals.ResultType.N14N15_TARGETED_RESULT;

            var theorN14FeatureGen = new TomTheorFeatureGenerator(Globals.LabellingType.NONE, 0.005);
            theorN14FeatureGen.GenerateTheorFeature(mt23140708);

            var theorN15FeatureGen = new TomTheorFeatureGenerator(Globals.LabellingType.N15, 0.005);
            theorN15FeatureGen.GenerateTheorFeature(mt23140708);


            var parameters = new IterativeTFFParameters
            {
                IsotopicProfileType = Globals.IsotopicProfileType.LABELLED,
                ToleranceInPPM = 30
            };


            var itff = new IterativeTFF(parameters);

            itff.Execute(run.ResultCollection);
//            IsotopicProfile iso = itff.iterativelyFindMSFeature(run, mt23140708.IsotopicProfileLabelled);

            var result = (N14N15_TResult)run.ResultCollection.GetTargetedResult(run.CurrentMassTag);



            Assert.IsNotNull(result.IsotopicProfileLabeled);
            Assert.AreEqual(82280, (int)result.IntensityAggregate);
            Assert.AreEqual(3, result.IsotopicProfileLabeled.MonoIsotopicPeakIndex);

            TestUtilities.DisplayIsotopicProfileData(result.IsotopicProfileLabeled);

        }




    }
}
