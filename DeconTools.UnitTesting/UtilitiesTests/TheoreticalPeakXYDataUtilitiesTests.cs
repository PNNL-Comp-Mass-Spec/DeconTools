using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Runs;
using DeconTools.Backend;
using System.Diagnostics;

namespace DeconTools.UnitTesting.UtilitiesTests
{
    [TestFixture]
    public class TheoreticalPeakXYDataUtilitiesTests
    {
        [Test]
        public void getGaussianPeakTest1()
        {
            MassTag mt = new MassTag();
            mt.ID = 56488;
            mt.MonoIsotopicMass = 2275.1694779;
            mt.PeptideSequence = "TTPSIIAYTDDETIVGQPAKR";
            mt.NETVal = 0.3520239f;
            mt.CreatePeptideObject();
            mt.ChargeState = 2;

            Run run = new XCaliburRun();

            ResultCollection rc = new ResultCollection(run);
            rc.Run.CurrentMassTag = mt;

            Task theorGen = new TomTheorFeatureGenerator();
            theorGen.Execute(rc);

            mt.CalculateMassesForIsotopicProfile(mt.ChargeState);

            TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfile);
            XYData xydata = TheorXYDataCalculationUtilities.GetTheorPeakData(mt.IsotopicProfile.Peaklist[1], 0.007);
            TestUtilities.DisplayXYValues(xydata);

        }


        [Test]
        public void getTheorIsotopicProfileXYDataTest1()
        {
            MassTag mt = new MassTag();
            mt.ID = 56488;
            mt.MonoIsotopicMass = 2275.1694779;
            mt.PeptideSequence = "TTPSIIAYTDDETIVGQPAKR";
            mt.NETVal = 0.3520239f;
            mt.CreatePeptideObject();
            mt.ChargeState = 2;

            Run run = new XCaliburRun();

            ResultCollection rc = new ResultCollection(run);
            rc.Run.CurrentMassTag = mt;

            Task theorGen = new TomTheorFeatureGenerator();
            theorGen.Execute(rc);

            mt.CalculateMassesForIsotopicProfile(mt.ChargeState);

            TestUtilities.DisplayIsotopicProfileData(mt.IsotopicProfile);
       
            XYData xydata = TheorXYDataCalculationUtilities.Get_Theoretical_IsotopicProfileXYData(mt.IsotopicProfile, 0.02);


            TestUtilities.DisplayXYValues(xydata);

        }



    }
}
