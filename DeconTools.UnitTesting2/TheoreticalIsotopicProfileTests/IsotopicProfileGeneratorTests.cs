using System;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using NUnit.Framework;


namespace DeconTools.UnitTesting2.TheoreticalIsotopicProfileTests
{
    [TestFixture]
    public class IsotopicProfileGeneratorTests
    {
        [Test]
        public void TomGenerateTheorProfileTest1()
        {
            PeptideTarget mt = TestDataCreationUtilities.CreateN14N15TestMassTagList().First(p => p.ID == 23085473);

            TomTheorFeatureGenerator theorGenerator = new TomTheorFeatureGenerator();
            theorGenerator.GenerateTheorFeature(mt);

            TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfile);

            //TODO: add an Assert, with a manually confirmed number. 

        }


        [Test]
        public void JoshGenerateTheorProfileTest1()
        {
            PeptideTarget mt = TestDataCreationUtilities.CreateN14N15TestMassTagList().First(p => p.ID == 23085473);

            JoshTheorFeatureGenerator theorGenerator = new JoshTheorFeatureGenerator();
            theorGenerator.GenerateTheorFeature(mt);

            TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfile);

            //TODO: add an Assert, with a manually confirmed number. 

        }

        [Test]
        public void TomGenerateN15LabelledTheorProfileTest1()
        {
            PeptideTarget mt = TestDataCreationUtilities.CreateN14N15TestMassTagList().First(p => p.ID == 23085473);

            TomTheorFeatureGenerator unlabelledTheorGenerator = new TomTheorFeatureGenerator();
            unlabelledTheorGenerator.GenerateTheorFeature(mt);


            TomTheorFeatureGenerator n15theorGenerator = new TomTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.N15, 0.005);
            n15theorGenerator.GenerateTheorFeature(mt);

            TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfileLabelled);

            //TODO: add an Assert, with a manually confirmed number. 
            
        }


        [Test]
        public void TomGenerateN15LabelledTheorProfileTest2()
        {
            PeptideTarget mt = TestDataCreationUtilities.CreateN14N15TestMassTagList().First(p => p.ID == 23085470);
           mt.EmpiricalFormula=   mt.GetEmpiricalFormulaFromTargetCode();

            Console.WriteLine("Total nitrogens = "+mt.GetAtomCountForElement("N"));


            TomTheorFeatureGenerator unlabelledTheorGenerator = new TomTheorFeatureGenerator();
            unlabelledTheorGenerator.GenerateTheorFeature(mt);


            TomTheorFeatureGenerator n15theorGenerator = new TomTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.N15, 0.005);
            n15theorGenerator.GenerateTheorFeature(mt);

            //TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfile);
            TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfileLabelled);

            //TODO: add an Assert, with a manually confirmed number. 

        }

        [Test]
        public void JoshGenerateN15LabelledTheorProfileTest2()
        {
            PeptideTarget mt = TestDataCreationUtilities.CreateN14N15TestMassTagList().First(p => p.ID == 23085470);
            mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();

            Console.WriteLine("Total nitrogens = " + mt.GetAtomCountForElement("N"));


            JoshTheorFeatureGenerator unlabelledTheorGenerator = new JoshTheorFeatureGenerator();
            unlabelledTheorGenerator.GenerateTheorFeature(mt);

            TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfile);
            Console.WriteLine();

            JoshTheorFeatureGenerator n15theorGenerator = new JoshTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.N15, 0.005);
            n15theorGenerator.GenerateTheorFeature(mt);

            unlabelledTheorGenerator.GenerateTheorFeature(mt);

            n15theorGenerator.GenerateTheorFeature(mt);

            unlabelledTheorGenerator.GenerateTheorFeature(mt);

            TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfile);
            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfileLabelled);

            

        }


    }
}
