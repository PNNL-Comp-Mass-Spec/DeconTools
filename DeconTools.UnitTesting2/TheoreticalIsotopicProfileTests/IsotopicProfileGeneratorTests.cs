using System;
using System.Linq;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Utilities;
using NUnit.Framework;


namespace DeconTools.UnitTesting2.TheoreticalIsotopicProfileTests
{
    [TestFixture]
    public class IsotopicProfileGeneratorTests
    {
        [Test]
        public void GenerateIsotopicProfileTest1()
        {
            var peptideUtils = new PeptideUtils();


            var empiricalFormula = peptideUtils.GetEmpiricalFormulaForPeptideSequence("SAMPLERSAMPLER");


            var featureGenerator = new JoshTheorFeatureGenerator();


        }



        [Test]
        public void TomGenerateTheorProfileTest1()
        {
            var mt = TestDataCreationUtilities.CreateN14N15TestMassTagList().First(p => p.ID == 23085473);

            var theorGenerator = new TomTheorFeatureGenerator();
            theorGenerator.GenerateTheorFeature(mt);

            TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfile);

            //TODO: add an Assert, with a manually confirmed number.

        }


        [Test]
        public void JoshGenerateTheorProfileTest1()
        {
            var mt = TestDataCreationUtilities.CreateN14N15TestMassTagList().First(p => p.ID == 23085473);

            var theorGenerator = new JoshTheorFeatureGenerator();
            theorGenerator.GenerateTheorFeature(mt);

            TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfile);

            //TODO: add an Assert, with a manually confirmed number.

        }

        [Test]
        public void TomGenerateN15LabeledTheorProfileTest1()
        {
            var mt = TestDataCreationUtilities.CreateN14N15TestMassTagList().First(p => p.ID == 23085473);

            var unlabeledTheorGenerator = new TomTheorFeatureGenerator();
            unlabeledTheorGenerator.GenerateTheorFeature(mt);

            var n15theorGenerator = new TomTheorFeatureGenerator(DeconTools.Backend.Globals.LabelingType.N15, 0.005);
            n15theorGenerator.GenerateTheorFeature(mt);

            TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfileLabeled);

            //TODO: add an Assert, with a manually confirmed number.

        }


        [Test]
        public void TomGenerateN15LabeledTheorProfileTest2()
        {
            var mt = TestDataCreationUtilities.CreateN14N15TestMassTagList().First(p => p.ID == 23085470);
            mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();

            Console.WriteLine("Total nitrogen count = " + mt.GetAtomCountForElement("N"));


            var unlabeledTheorGenerator = new TomTheorFeatureGenerator();
            unlabeledTheorGenerator.GenerateTheorFeature(mt);


            var n15theorGenerator = new TomTheorFeatureGenerator(DeconTools.Backend.Globals.LabelingType.N15, 0.005);
            n15theorGenerator.GenerateTheorFeature(mt);

            //TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfile);
            TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfileLabeled);

            //TODO: add an Assert, with a manually confirmed number.

        }

        [Test]
        public void JoshGenerateN15LabeledTheorProfileTest2()
        {
            var mt = TestDataCreationUtilities.CreateN14N15TestMassTagList().First(p => p.ID == 23085470);
            mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();

            Console.WriteLine("Total nitrogen count = " + mt.GetAtomCountForElement("N"));


            var unlabeledTheorGenerator = new JoshTheorFeatureGenerator();
            unlabeledTheorGenerator.GenerateTheorFeature(mt);

            TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfile);
            Console.WriteLine();

            var n15theorGenerator = new JoshTheorFeatureGenerator(DeconTools.Backend.Globals.LabelingType.N15, 0.005);
            n15theorGenerator.GenerateTheorFeature(mt);

            unlabeledTheorGenerator.GenerateTheorFeature(mt);

            n15theorGenerator.GenerateTheorFeature(mt);

            unlabeledTheorGenerator.GenerateTheorFeature(mt);

            TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfile);
            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfileLabeled);



        }


    }
}
