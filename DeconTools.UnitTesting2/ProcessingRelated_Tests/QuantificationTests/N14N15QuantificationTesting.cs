using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using DeconTools.Backend.Algorithms.Quantifiers;


namespace DeconTools.UnitTesting2.QuantificationTests
{
    [TestFixture]
    public class N14N15QuantificationTesting
    {
     
        [Test]
        public void test01_mt230140708_z3()
        {
            double featureFinderTol = 15;
            bool featureFinderRequiresMonoPeak = false;
            int numPeaksUsedInQuant = 3;

            N14N15TestingUtilities n14n15Util = new N14N15TestingUtilities();
            //get MS
            XYData massSpectrum = n14n15Util.GetSpectrumAMTTag23140708_Z3_Sum3();  //this is the diff b/w previous test and this one 


            DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(1, 1, 2);
            massSpectrum = smoother.Smooth(massSpectrum); 
            

            //get target MT
            MassTag mt23140708 = n14n15Util.CreateMT23140708_Z3();


            //get ms peaks
            DeconToolsPeakDetector peakDet = new DeconToolsPeakDetector(1.3, 2, Globals.PeakFitType.QUADRATIC, false);
            List<IPeak> msPeakList = peakDet.FindPeaks(massSpectrum, 0, 0);

            //TestUtilities.DisplayPeaks(msPeakList);

            TomTheorFeatureGenerator featureGen = new TomTheorFeatureGenerator();
            featureGen.GenerateTheorFeature(mt23140708);


            IsotopicProfile theorN15Profile = N15IsotopeProfileGenerator.GetN15IsotopicProfile(mt23140708, 0.005);



            BasicMSFeatureFinder bff = new BasicMSFeatureFinder();
            IsotopicProfile n14Profile = bff.FindMSFeature(msPeakList, mt23140708.IsotopicProfile, featureFinderTol, featureFinderRequiresMonoPeak);


            IsotopicProfile n15Profile = bff.FindMSFeature(msPeakList, theorN15Profile, featureFinderTol, featureFinderRequiresMonoPeak);

            Console.WriteLine(mt23140708.Peptide.GetEmpiricalFormula());


            TestUtilities.DisplayIsotopicProfileData(mt23140708.IsotopicProfile);

            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(theorN15Profile);

            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(n14Profile);

            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(n15Profile);


            BasicN14N15Quantifier quant = new BasicN14N15Quantifier();
            double ratioContribIso1;
            double ratioContribIso2;
            double ratio;
            
            quant.GetRatioBasedOnTopPeaks(n14Profile, n15Profile, mt23140708.IsotopicProfile, theorN15Profile, peakDet.BackGroundIntensity, 
                numPeaksUsedInQuant, out ratio, out ratioContribIso1, out ratioContribIso2);

            Console.WriteLine("--------------------------------");
            Console.WriteLine("Background intensity = " + peakDet.BackGroundIntensity);

            Console.WriteLine("--------------------------------");
            Console.WriteLine("Ratio = " + ratio);
            Console.WriteLine("Ratio contrib Iso1 = " + ratioContribIso1);
            Console.WriteLine("Ratio contrib Iso2 = " + ratioContribIso2);
            Console.WriteLine("--------------------------------");

            Assert.AreEqual(1.458769m, (decimal)Math.Round(ratio, 6));      //see 'N14N15Quantification_manualValidation_of_algorithm.xls' for manual validation


        }

        [Test]
        public void test01_mt230140708_z2()
        {
            double featureFinderTol = 15;
            bool featureFinderRequiresMonoPeak = false;
            int numPeaksUsedInQuant = 3;

            N14N15TestingUtilities n14n15Util = new N14N15TestingUtilities();
            //get MS
            XYData massSpectrum = n14n15Util.GetSpectrumAMTTag23140708_Z2_Sum3();  //scan 1428 of 

            //get target MT
            MassTag mt23140708 = n14n15Util.CreateMT23140708_Z2();


            DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(1, 1, 2);
            massSpectrum = smoother.Smooth(massSpectrum);


            //get ms peaks
            DeconToolsPeakDetector peakDet = new DeconToolsPeakDetector(1.3, 2, Globals.PeakFitType.QUADRATIC, false);
            List<IPeak> msPeakList = peakDet.FindPeaks(massSpectrum, 0, 0);




            TestUtilities.DisplayPeaks(msPeakList);

            TomTheorFeatureGenerator featureGen = new TomTheorFeatureGenerator();
            featureGen.GenerateTheorFeature(mt23140708);


            IsotopicProfile theorN15Profile = N15IsotopeProfileGenerator.GetN15IsotopicProfile(mt23140708, 0.005);



            BasicMSFeatureFinder bff = new BasicMSFeatureFinder();
            IsotopicProfile n14Profile = bff.FindMSFeature(msPeakList, mt23140708.IsotopicProfile, featureFinderTol, featureFinderRequiresMonoPeak);


            IsotopicProfile n15Profile = bff.FindMSFeature(msPeakList, theorN15Profile, featureFinderTol, featureFinderRequiresMonoPeak);

            Console.WriteLine(mt23140708.Peptide.GetEmpiricalFormula());


            TestUtilities.DisplayIsotopicProfileData(mt23140708.IsotopicProfile);

            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(theorN15Profile);

            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(n14Profile);

            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(n15Profile);


            BasicN14N15Quantifier quant = new BasicN14N15Quantifier();
            double ratioContribIso1;
            double ratioContribIso2;
            double ratio;
            quant.GetRatioBasedOnTopPeaks(n14Profile, n15Profile, mt23140708.IsotopicProfile, theorN15Profile, peakDet.BackGroundIntensity, numPeaksUsedInQuant,
                out ratio, out ratioContribIso1, out ratioContribIso2);

            Console.WriteLine("--------------------------------");
            Console.WriteLine("Ratio = " + ratio);
            Console.WriteLine("Ratio contrib Iso1 = " + ratioContribIso1);
            Console.WriteLine("Ratio contrib Iso2 = " + ratioContribIso2);

            Console.WriteLine("Background intensity = " + peakDet.BackGroundIntensity);
            

        }


    }
}
