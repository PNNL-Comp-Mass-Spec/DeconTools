using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Core;


namespace DeconTools.UnitTesting2.TheoreticalIsotopicProfileTests
{
    [TestFixture]
    public class TomTheorFeatureGeneratorTests
    {
        [Test]
        public void GenerateTheorProfileTest1()
        {
            MassTag mt = TestDataCreationUtilities.CreateN14N15TestMassTagList().First(p => p.ID == 23085473);

            TomTheorFeatureGenerator theorGenerator = new TomTheorFeatureGenerator();
            theorGenerator.GenerateTheorFeature(mt);

            TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfile);

            //TODO: add an Assert, with a manually confirmed number. 

        }

        [Test]
        public void GenerateN15LabelledTheorProfileTest1()
        {
            MassTag mt = TestDataCreationUtilities.CreateN14N15TestMassTagList().First(p => p.ID == 23085473);

            TomTheorFeatureGenerator unlabelledTheorGenerator = new TomTheorFeatureGenerator();
            unlabelledTheorGenerator.GenerateTheorFeature(mt);


            TomTheorFeatureGenerator n15theorGenerator = new TomTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.N15, 0.005);
            n15theorGenerator.GenerateTheorFeature(mt);

            TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfileLabelled);

            //TODO: add an Assert, with a manually confirmed number. 
            
        }


        [Test]
        public void GenerateN15LabelledTheorProfileTest2()
        {
            MassTag mt = TestDataCreationUtilities.CreateN14N15TestMassTagList().First(p => p.ID == 23085470);
           mt.EmpiricalFormula=   mt.GetEmpiricalFormulaFromTargetCode();

            Console.WriteLine("Total nitrogens = "+mt.GetAtomCountForElement("N"));


            TomTheorFeatureGenerator unlabelledTheorGenerator = new TomTheorFeatureGenerator();
            unlabelledTheorGenerator.GenerateTheorFeature(mt);


            TomTheorFeatureGenerator n15theorGenerator = new TomTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.N15, 0.005);
            n15theorGenerator.GenerateTheorFeature(mt);

            TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfile);
            //TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfileLabelled);

            //TODO: add an Assert, with a manually confirmed number. 

        }


    }
}
