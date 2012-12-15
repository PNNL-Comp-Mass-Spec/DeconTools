using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.ProcessingTasks;
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
            string massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_first10.txt";

            MassTagFromTextFileImporter masstagImporter = new MassTagFromTextFileImporter(massTagFile);
            TargetCollection massTagColl = masstagImporter.Import();

            TargetBase mt24702_charge3 = (from n in massTagColl.TargetList where n.ID == 24702 && n.ChargeState == 3 select n).First();
            TargetBase mt24702_charge4 = (from n in massTagColl.TargetList where n.ID == 24702 && n.ChargeState == 4 select n).First();





        }



        [Test]
        public void n14N15LabelledData_TFFTest1()
        {
            double featureFinderTol = 15;
            bool featureFinderRequiresMonoPeak = false;

            N14N15TestingUtilities n14n15Util = new N14N15TestingUtilities();
            //get sample MS from Test Data
            XYData massSpectrum = n14n15Util.GetSpectrumAMTTag23140708_Z3_Sum3();  //this is the diff b/w previous test and this one 
            PeptideTarget mt23140708 = n14n15Util.CreateMT23140708_Z3();


            //get ms peaks
            DeconToolsPeakDetector peakDet = new DeconToolsPeakDetector(1.3, 2, Globals.PeakFitType.QUADRATIC, false);
            List<Peak> msPeakList = peakDet.FindPeaks(massSpectrum, 0, 0);

            //TestUtilities.DisplayPeaks(msPeakList);

            //generate theor unlabelled profile
            TomTheorFeatureGenerator unlabelledfeatureGen = new TomTheorFeatureGenerator();
            unlabelledfeatureGen.GenerateTheorFeature(mt23140708);

            //generate theor N15-labelled profile
            TomTheorFeatureGenerator n15featureGen = new TomTheorFeatureGenerator(Globals.LabellingType.N15, 0.005);
            n15featureGen.GenerateTheorFeature(mt23140708);


            //find features in experimental data, using the theoretical profiles
            BasicTFF msfeatureFinder = new BasicTFF();
            IsotopicProfile n14profile = msfeatureFinder.FindMSFeature(msPeakList, mt23140708.IsotopicProfile, featureFinderTol, featureFinderRequiresMonoPeak);
            IsotopicProfile n15profile = msfeatureFinder.FindMSFeature(msPeakList, mt23140708.IsotopicProfileLabelled, featureFinderTol, featureFinderRequiresMonoPeak);

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
            N14N15TestingUtilities n14n15Util = new N14N15TestingUtilities();
            //get sample MS from Test Data
            
            RunFactory rf=new RunFactory();
            Run run = rf.CreateRun(N14N15TestingUtilities.MS_AMTTag23085904_z2_sum1_lowN15);
            
            
            run.XYData = N14N15TestingUtilities.GetTestSpectrum(N14N15TestingUtilities.MS_AMTTag23085904_z2_sum1_lowN15);

            PeptideTarget mt23140708 = n14n15Util.CreateMT23085904_Z2();
            run.CurrentMassTag = mt23140708;
            run.ResultCollection.ResultType = Globals.ResultType.N14N15_TARGETED_RESULT;

            TomTheorFeatureGenerator theorN14FeatureGen = new TomTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.NONE, 0.005);
            theorN14FeatureGen.GenerateTheorFeature(mt23140708);

            TomTheorFeatureGenerator theorN15FeatureGen = new TomTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.N15, 0.005);
            theorN15FeatureGen.GenerateTheorFeature(mt23140708);


            IterativeTFFParameters parameters=new IterativeTFFParameters();
            parameters.IsotopicProfileType = IsotopicProfileType.LABELLED;
            parameters.ToleranceInPPM = 30;


            IterativeTFF itff = new IterativeTFF(parameters);

            itff.Execute(run.ResultCollection);
//            IsotopicProfile iso = itff.iterativelyFindMSFeature(run, mt23140708.IsotopicProfileLabelled);

            N14N15_TResult result = (N14N15_TResult)run.ResultCollection.GetTargetedResult(run.CurrentMassTag);



            Assert.IsNotNull(result.IsotopicProfileLabeled);
            Assert.AreEqual(82280, (int)result.IntensityAggregate);
            Assert.AreEqual(3, result.IsotopicProfileLabeled.MonoIsotopicPeakIndex);

            TestUtilities.DisplayIsotopicProfileData(result.IsotopicProfileLabeled);

        }

               


    }
}
