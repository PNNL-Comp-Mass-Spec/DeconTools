using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.UnitTesting2.QuantificationTests;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Utilities
{
    [TestFixture]
    public class XYDataUtilitiesTests
    {
        [Test]
        public void Test1()
        {

            double featureFinderTol = 15;
            bool featureFinderRequiresMonoPeak = false;

             N14N15TestingUtilities n14n15Util = new N14N15TestingUtilities();
            //get MS
            XYData massSpectrum = n14n15Util.GetSpectrumAMTTag23140708_Z3_Sum3();  //this is the diff b/w previous test and this one 


            PeptideTarget mt23140708 = n14n15Util.CreateMT23140708_Z3();


            JoshTheorFeatureGenerator featureGen2 = new JoshTheorFeatureGenerator();
            featureGen2.GenerateTheorFeature(mt23140708);




            //get ms peaks
            DeconToolsPeakDetector peakDet = new DeconToolsPeakDetector(1.3, 2, Globals.PeakFitType.QUADRATIC, false);
            List<Peak> msPeakList = peakDet.FindPeaks(massSpectrum, 0, 0);

            BasicTFF bff = new BasicTFF(featureFinderTol, featureFinderRequiresMonoPeak);
            IsotopicProfile n14Profile = bff.FindMSFeature(msPeakList, mt23140708.IsotopicProfile, featureFinderTol, featureFinderRequiresMonoPeak);

            XYData theorXYData=   TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData(mt23140708.IsotopicProfile, n14Profile.GetFWHM());


            double theorMaxY = theorXYData.Yvalues.Max();
            double obsMaxY = n14Profile.getMostIntensePeak().Height;


            for (int i = 0; i < theorXYData.Xvalues.Length; i++)
            {
                theorXYData.Yvalues[i] = theorXYData.Yvalues[i] * obsMaxY;
            }

            offsetDistribution(theorXYData, mt23140708.IsotopicProfile, n14Profile);


            var subtracted=   XYDataUtilities.SubtractXYData(massSpectrum, theorXYData, n14Profile.MonoPeakMZ - 1,
                                           n14Profile.MonoPeakMZ + 3, 0.01);



            string outputFileMS = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\TextFile\massSpectrum1.txt";
            string outputFileMSSubtracted = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\TextFile\massSpectrum1_withDataSubtracted.txt";

            TestUtilities.WriteToFile(massSpectrum, outputFileMS);
            TestUtilities.WriteToFile(subtracted, outputFileMSSubtracted);



            // XYDataUtilities.NormalizeXYData()



        }


        private void offsetDistribution(XYData theorXYData, IsotopicProfile theorIsotopicProfile, IsotopicProfile observedIsotopicProfile)
        {
            double offset = 0;
            if (theorIsotopicProfile == null || theorIsotopicProfile.Peaklist == null || theorIsotopicProfile.Peaklist.Count == 0) return;

            MSPeak mostIntensePeak = theorIsotopicProfile.getMostIntensePeak();
            int indexOfMostIntensePeak = theorIsotopicProfile.Peaklist.IndexOf(mostIntensePeak);

            if (observedIsotopicProfile.Peaklist == null || observedIsotopicProfile.Peaklist.Count == 0) return;

            bool enoughPeaksInTarget = (indexOfMostIntensePeak <= observedIsotopicProfile.Peaklist.Count - 1);

            if (enoughPeaksInTarget)
            {
                MSPeak targetPeak = observedIsotopicProfile.Peaklist[indexOfMostIntensePeak];
                offset = targetPeak.XValue - mostIntensePeak.XValue;
                //offset = observedIsotopicProfile.Peaklist[0].XValue - theorIsotopicProfile.Peaklist[0].XValue;   //want to test to see if Thrash is same as rapid

            }
            else
            {
                offset = observedIsotopicProfile.Peaklist[0].XValue - theorIsotopicProfile.Peaklist[0].XValue;
            }

            for (int i = 0; i < theorXYData.Xvalues.Length; i++)
            {
                theorXYData.Xvalues[i] = theorXYData.Xvalues[i] + offset;
            }

            foreach (var peak in theorIsotopicProfile.Peaklist)
            {
                peak.XValue = peak.XValue + offset;

            }


        }



    }
}
