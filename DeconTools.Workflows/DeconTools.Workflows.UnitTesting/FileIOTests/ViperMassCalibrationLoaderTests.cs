using System;
using DeconTools.Workflows.Backend.FileIO;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.FileIOTests
{
    [TestFixture]
    public class ViperMassCalibrationLoaderTests
    {
        [Test]
        public void LoadCalibrationDataTest1()
        {
            string testFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\AlignmentInfo\LNA_A_Stat_Sample_SC_23_LNA_ExpA_Expo_Stat_SeattleBioMed_15Feb13_Cougar_12-12-35_MassAndGANETErrors_BeforeRefinement.txt";

            ViperMassCalibrationLoader loader = new ViperMassCalibrationLoader(testFile);

            var calibrationData=  loader.ImportMassCalibrationData();


            foreach (var item in calibrationData)
            {
                Console.WriteLine(item.ToStringWithDetails());
            }


        }

    }
}
