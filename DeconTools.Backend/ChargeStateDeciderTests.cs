using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    class ChargeStateDeciderTests
    {
        [Test]
        public void EasyDecision()
        {
            string fileName =
                 @"\\pnl\projects\MSSHARE\Gord\For_Paul\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            Run run = new RunFactory().CreateRun(fileName);


            //TODO: This is where we will test the first feature found in scan 6005. it needs to be charge state 2.
            IsotopicProfile msFeature = new IsotopicProfile();
            //la la la 

            // Assert.AreEqual(msFeature.ChargeState, 2);


        }

        [Test]
        public void MediumDecision()
        {

            //Assert.AreEqual(msFeature.ChargeState, 2);
        }

        [Test]
        public void HardDecision()
        {

            //Assert.AreEqual(msFeature.ChargeState, 2);

        }


    }
}
