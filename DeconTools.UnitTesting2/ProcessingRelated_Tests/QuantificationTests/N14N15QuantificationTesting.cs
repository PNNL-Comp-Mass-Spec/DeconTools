using System;
using DeconTools.Backend.Algorithms.Quantifiers;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using NUnit.Framework;


namespace DeconTools.UnitTesting2.QuantificationTests
{
    [TestFixture]
    public class N14N15QuantificationTesting
    {

        [Test]
        public void test01_mt230140708_z3()
        {
            double featureFinderTol = 15;
            var numPeaksUsedInQuant = 3;

            var n14n15Util = new N14N15TestingUtilities();
            //get MS
            var massSpectrum = n14n15Util.GetSpectrumAMTTag23140708_Z3_Sum3();  //this is the diff b/w previous test and this one


           var smoother = new Backend.ProcessingTasks.Smoothers.SavitzkyGolaySmoother(3, 2);
            massSpectrum = smoother.Smooth(massSpectrum);


            //get target MT
            var mt23140708 = n14n15Util.CreateMT23140708_Z3();


            //get ms peaks
            var peakDet = new DeconToolsPeakDetectorV2(1.3, 2);
            var msPeakList = peakDet.FindPeaks(massSpectrum);

            //TestUtilities.DisplayPeaks(msPeakList);

            var featureGen = new TomTheorFeatureGenerator();
            featureGen.GenerateTheorFeature(mt23140708);

            TestUtilities.DisplayIsotopicProfileData(mt23140708.IsotopicProfile);

            var featureGen2 = new JoshTheorFeatureGenerator();
            featureGen2.GenerateTheorFeature(mt23140708);

            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(mt23140708.IsotopicProfile);

            var gen = new N15IsotopeProfileGenerator();

            var theorN15Profile = gen.GetN15IsotopicProfile(mt23140708, 0.005);




            var bff = new BasicTFF(featureFinderTol, requiresMonoPeak: false);
            var n14Profile = bff.FindMSFeature(msPeakList, mt23140708.IsotopicProfile);


            var n15Profile = bff.FindMSFeature(msPeakList, theorN15Profile);

            Console.WriteLine(mt23140708.GetEmpiricalFormulaFromTargetCode());


            TestUtilities.DisplayIsotopicProfileData(mt23140708.IsotopicProfile);

            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(theorN15Profile);

            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(n14Profile);

            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(n15Profile);


            var quant = new BasicN14N15Quantifier(featureFinderTol);

            quant.GetRatioBasedOnTopPeaks(n14Profile, n15Profile, mt23140708.IsotopicProfile, theorN15Profile, peakDet.BackgroundIntensity,
                numPeaksUsedInQuant, out var ratio, out var ratioContribIso1, out var ratioContribIso2);

            Console.WriteLine("--------------------------------");
            Console.WriteLine("Background intensity = " + peakDet.BackgroundIntensity);

            Console.WriteLine("--------------------------------");
            Console.WriteLine("Ratio = " + ratio);
            Console.WriteLine("Ratio contrib Iso1 = " + ratioContribIso1);
            Console.WriteLine("Ratio contrib Iso2 = " + ratioContribIso2);
            Console.WriteLine("--------------------------------");

            Assert.AreEqual(1.458267m, (decimal)Math.Round(ratio, 6));      //see 'N14N15Quantification_manualValidation_of_algorithm.xls' for manual validation


        }

        [Test]
        public void test01_mt230140708_z2()
        {
            double featureFinderTol = 15;
            var numPeaksUsedInQuant = 3;

            var n14n15Util = new N14N15TestingUtilities();
            //get MS
            var massSpectrum = n14n15Util.GetSpectrumAMTTag23140708_Z2_Sum3();  //scan 1428 of

            //get target MT
            var mt23140708 = n14n15Util.CreateMT23140708_Z2();


            var smoother = new Backend.ProcessingTasks.Smoothers.SavitzkyGolaySmoother(3, 2);
            massSpectrum = smoother.Smooth(massSpectrum);


            //get ms peaks
            var peakDet = new DeconToolsPeakDetectorV2(1.3, 2);
            var msPeakList = peakDet.FindPeaks(massSpectrum);




            TestUtilities.DisplayPeaks(msPeakList);

            var featureGen = new TomTheorFeatureGenerator();
            featureGen.GenerateTheorFeature(mt23140708);

            var profileGen = new N15IsotopeProfileGenerator();
            var theorN15Profile = profileGen.GetN15IsotopicProfile(mt23140708, 0.005);



            var bff = new BasicTFF(featureFinderTol, requiresMonoPeak: false);
            var n14Profile = bff.FindMSFeature(msPeakList, mt23140708.IsotopicProfile);
            var n15Profile = bff.FindMSFeature(msPeakList, theorN15Profile);

            Console.WriteLine(mt23140708.GetEmpiricalFormulaFromTargetCode());


            TestUtilities.DisplayIsotopicProfileData(mt23140708.IsotopicProfile);

            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(theorN15Profile);

            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(n14Profile);

            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(n15Profile);


            var quant = new BasicN14N15Quantifier(featureFinderTol);
            quant.GetRatioBasedOnTopPeaks(n14Profile, n15Profile, mt23140708.IsotopicProfile, theorN15Profile, peakDet.BackgroundIntensity, numPeaksUsedInQuant,
                out var ratio, out var ratioContribIso1, out var ratioContribIso2);

            Console.WriteLine("--------------------------------");
            Console.WriteLine("Ratio = " + ratio);
            Console.WriteLine("Ratio contrib Iso1 = " + ratioContribIso1);
            Console.WriteLine("Ratio contrib Iso2 = " + ratioContribIso2);

            Console.WriteLine("Background intensity = " + peakDet.BackgroundIntensity);


        }


    }
}
