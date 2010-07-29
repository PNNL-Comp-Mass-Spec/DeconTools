using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.UnitTesting2.QuantificationTests;
using DeconTools.Backend;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.TargetedFeatureFinderTests
{
    [TestFixture]
    public class TFFTests
    {
        [Test]
        public void unlabelled_data_TFFTest1()
        {




        }



        [Test]
        public void n14N15LabelledData_TFFTest1()
        {
            double featureFinderTol = 15;
            bool featureFinderRequiresMonoPeak = false;

            N14N15TestingUtilities n14n15Util = new N14N15TestingUtilities();
            //get sample MS from Test Data
            XYData massSpectrum = n14n15Util.GetSpectrumAMTTag23140708_Z3_Sum3();  //this is the diff b/w previous test and this one 
            MassTag mt23140708 = n14n15Util.CreateMT23140708_Z3();


            //get ms peaks
            DeconToolsPeakDetector peakDet = new DeconToolsPeakDetector(1.3, 2, Globals.PeakFitType.QUADRATIC, false);
            List<IPeak> msPeakList = peakDet.FindPeaks(massSpectrum, 0, 0);

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

            Console.WriteLine(mt23140708.Peptide.GetEmpiricalFormula());


            TestUtilities.DisplayIsotopicProfileData(mt23140708.IsotopicProfile);

            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(mt23140708.IsotopicProfileLabelled);

            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(n14profile);

            Console.WriteLine();
            TestUtilities.DisplayIsotopicProfileData(n15profile);



        }

               


    }
}
