using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Utilities;

namespace DeconTools.UnitTesting.ProcessingTasksTests.TargetedAnalysisTests
{
    [TestFixture]
    public class TheorFeatureGeneratorTests
    {
        [Test]
        public void test1()
        {
            MassTag mt = new MassTag();
            mt.ID = 56488;
            mt.MonoIsotopicMass = 2275.1694779;
            mt.PeptideSequence = "TTPSIIAYTDDETIVGQPAKR";
            mt.NETVal = 0.3520239f;
            mt.CreatePeptideObject();
            mt.ChargeState = 2;

            Run run=new XCaliburRun();

            ResultCollection rc = new ResultCollection(run);
            rc.Run.CurrentMassTag = mt;

            Task theorGen = new TomTheorFeatureGenerator();
            theorGen.Execute(rc);

            mt.CalculateMassesForIsotopicProfile(mt.ChargeState);

            MercuryDistributionCreator distcreator = new MercuryDistributionCreator();
            distcreator.CreateDistribution(mt.MonoIsotopicMass, mt.ChargeState, 40000);
            distcreator.getIsotopicProfile();


            TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfile);
            //TestUtilities.DisplayIsotopicProfileData(distcreator.IsotopicProfile);
            Assert.AreEqual(7, mt.IsotopicProfile.Peaklist.Count);

            Assert.AreEqual(1138.59201544m, (decimal)mt.IsotopicProfile.Peaklist[0].XValue);
            Assert.AreEqual((1138.59201544m + 1.00235m / 2m), (decimal)mt.IsotopicProfile.Peaklist[1].XValue);



        }


    }
}
