using System.IO;
using DeconTools.Backend.Parameters;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.FileIO_Tests
{
    [TestFixture]
    public class DeconToolsParameterTests
    {
        [Test]
        public void Test1()
        {
            string parameterFile = "..\\..\\..\\..\\SampleParameterFile.xml";

            Assert.IsTrue(File.Exists(parameterFile));
            DeconToolsParameters parameters = new DeconToolsParameters();
            parameters.LoadFromOldDeconToolsParameterFile(parameterFile);

            Assert.IsNotNullOrEmpty(parameters.ThrashParameters.AveragineFormula);
            Assert.AreEqual("C4.9384 H7.7583 N1.3577 O1.4773 S0.0417", parameters.ThrashParameters.AveragineFormula);
            Assert.AreEqual("AREA", parameters.ThrashParameters.IsotopicProfileFitType.ToString());
            Assert.AreEqual(1.00727649, parameters.ThrashParameters.ChargeCarrierMass);
            Assert.AreEqual(1, parameters.ThrashParameters.MinIntensityForDeletion);
            Assert.AreEqual(0.3, parameters.ThrashParameters.MaxFit);
            Assert.AreEqual(10, parameters.ThrashParameters.MaxCharge);
            Assert.AreEqual(1, parameters.ThrashParameters.MinMSFeatureToBackgroundRatio);
            Assert.AreEqual(10000, parameters.ThrashParameters.MaxMass);
            Assert.AreEqual(false, parameters.ThrashParameters.IsO16O18Data);
            Assert.AreEqual(false, parameters.ThrashParameters.CheckAllPatternsAgainstChargeState1);
            Assert.AreEqual(true, parameters.ThrashParameters.IsThrashUsed);

            Assert.AreEqual("Text", parameters.ScanBasedWorkflowParameters.ExportFileType.ToString());
            Assert.AreEqual("standard", parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName.ToLower());
            Assert.AreEqual(true, parameters.ScanBasedWorkflowParameters.ExportPeakData);

            Assert.AreEqual(-2147483648, parameters.MSGeneratorParameters.MinLCScan);
            Assert.AreEqual(2147483647, parameters.MSGeneratorParameters.MaxLCScan);
            Assert.AreEqual(1, parameters.MSGeneratorParameters.NumLCScansToSum);
            Assert.AreEqual(true, parameters.MSGeneratorParameters.UseMZRange);
            Assert.AreEqual(100, parameters.MSGeneratorParameters.MinMZ);
            Assert.AreEqual(2000, parameters.MSGeneratorParameters.MaxMZ);

            Assert.AreEqual("quadratic", parameters.PeakDetectorParameters.PeakFitType.ToString().ToLower());

            Assert.AreEqual(1, parameters.MSGeneratorParameters.NumLCScansToSum);

        }


        [Test]
        public void LoadParametersTest2()
        {
            string parameterFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\IMS_UIMF_PeakBR4_PeptideBR4_SN3_SumScans3_NoLCSum_Sat90000_newFormat.xml";

            Assert.IsTrue(File.Exists(parameterFile));
            DeconToolsParameters parameters = new DeconToolsParameters();
            parameters.LoadFromOldDeconToolsParameterFile(parameterFile);

            Assert.IsNotNullOrEmpty(parameters.ThrashParameters.AveragineFormula);
            Assert.AreEqual("C4.9384 H7.7583 N1.3577 O1.4773 S0.0417", parameters.ThrashParameters.AveragineFormula);
            Assert.AreEqual("AREA", parameters.ThrashParameters.IsotopicProfileFitType.ToString());
            Assert.AreEqual(1.00727649, parameters.ThrashParameters.ChargeCarrierMass);
            Assert.AreEqual(10, (decimal)parameters.ThrashParameters.MinIntensityForDeletion);
            Assert.AreEqual(0.4, parameters.ThrashParameters.MaxFit);
            Assert.AreEqual(10, parameters.ThrashParameters.MaxCharge);
            Assert.AreEqual(4, parameters.ThrashParameters.MinMSFeatureToBackgroundRatio);
            Assert.AreEqual(10000, parameters.ThrashParameters.MaxMass);
            Assert.AreEqual(false, parameters.ThrashParameters.IsO16O18Data);
            Assert.AreEqual(false, parameters.ThrashParameters.CheckAllPatternsAgainstChargeState1);
            Assert.AreEqual(true, parameters.ThrashParameters.IsThrashUsed);

            Assert.AreEqual("Text", parameters.ScanBasedWorkflowParameters.ExportFileType.ToString());
            Assert.AreEqual("uimf_saturation_repair", parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName.ToLower());
            Assert.AreEqual(false, parameters.ScanBasedWorkflowParameters.ExportPeakData);

            Assert.AreEqual(500, parameters.MSGeneratorParameters.MinLCScan);
            Assert.AreEqual(510, parameters.MSGeneratorParameters.MaxLCScan);
            Assert.AreEqual(false, parameters.MSGeneratorParameters.UseMZRange);
            Assert.AreEqual(100, parameters.MSGeneratorParameters.MinMZ);
            Assert.AreEqual(3000, parameters.MSGeneratorParameters.MaxMZ);

            Assert.AreEqual("quadratic", parameters.PeakDetectorParameters.PeakFitType.ToString().ToLower());

            Assert.AreEqual(1, parameters.MSGeneratorParameters.NumLCScansToSum);
            Assert.AreEqual(7, parameters.MSGeneratorParameters.NumImsScansToSum);
            Assert.AreEqual(true, parameters.MSGeneratorParameters.SumSpectraAcrossIms);
            Assert.AreEqual(true, parameters.MSGeneratorParameters.SumSpectraAcrossLC);

        }

    }
}
