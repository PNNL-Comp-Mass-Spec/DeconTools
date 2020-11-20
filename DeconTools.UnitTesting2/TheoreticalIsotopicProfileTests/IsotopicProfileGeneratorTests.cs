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
            var peptideSequenceA = "SamplePeptide".ToUpper();
            var peptideSequenceB = "K." + peptideSequenceA + ".S";

            var empiricalFormulaA = peptideUtils.GetEmpiricalFormulaForPeptideSequence(peptideSequenceA);
            Console.WriteLine("Empirical formula for {0} is {1}", peptideSequenceA, empiricalFormulaA);

            var empiricalFormulaB = peptideUtils.GetEmpiricalFormulaForPeptideSequence(peptideSequenceB);
            Console.WriteLine("Empirical formula for {0} is {1}", peptideSequenceB, empiricalFormulaB);

            Assert.AreEqual(empiricalFormulaA, empiricalFormulaB, "Empirical formulas do not match");

            var featureGenerator = new JoshTheorFeatureGenerator();
            var profile = featureGenerator.GenerateTheorProfile(empiricalFormulaA, 2);

            const double fwhm = 0.5;

            // Note: the data returned by GetTheoreticalIsotopicProfileXYData()
            // Lists each peak's m/z and intensity values sequentially
            // If fwhm is relatively large (greater than 0.05) there will be overlap between the peaks

            var xyData = profile.GetTheoreticalIsotopicProfileXYData(fwhm);
            var xStart = xyData.Xvalues[0];

            Console.WriteLine();
            Console.WriteLine("{0,-10} {1,-10}", "m/z", "Intensity");

            for (var i = 0; i < xyData.Xvalues.Length; i += 5)
            {
                if (xyData.Xvalues[i] > xStart + 2.5)
                    break;

                Console.WriteLine("{0,-10:F4} {1,-10:F4}", xyData.Xvalues[i], xyData.Yvalues[i]);
            }
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
