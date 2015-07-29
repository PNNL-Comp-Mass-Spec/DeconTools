using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Data;
using DeconTools.Backend.Runs;

namespace DeconTools.UnitTesting.DTO_Tests
{

    [TestFixture]
    public class IsosResultUtilitiesTests
    {

        private string xcaliburThrashIsos1= "..\\..\\TestFiles\\thrashTestIsos1.csv";
        private string largeIsosFile1 = @"C:\Documents and Settings\d3x720\My Documents\Projects\Test_Data\05_NewFitScoreTests\Tests\Test07_thrash_2\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_isos.csv";
        private string uimfIsos1 = "..\\..\\TestFiles\\UIMFIsosTestfile1.csv";
        private string imfIsos1 = "..\\..\\TestFiles\\50ugpmlBSA_CID_QS_16V_0000.Accum_1_CANON_isos.csv";

        private string imfIsosFolder = @"..\\..\\TestFiles\\UIMF_IMF_ValidationFiles\\IMF";

        [Test]
        public void getAverageScoreTest1()
        {
            
            TestUtilities testUtils=new TestUtilities();
            List<IsosResult> thrashResults = testUtils.CreateThrashIsosResults1();

            outputFitValuesToConsole(thrashResults);

            double avgFit = IsosResultUtilities.getAverageScore(thrashResults);

            Assert.AreEqual(0.0482473459193904, (decimal)avgFit);
            
        }

        [Test]
        public void getSpecificChargeStateResultsOnlyTest1()
        {

            TestUtilities testUtils = new TestUtilities();
            List<IsosResult> thrashResults = testUtils.CreateThrashIsosResults1();

            List<IsosResult> chargeTwoResultsOnly = IsosResultUtilities.getIsosResultsByChargeState(thrashResults, 2, IsosResultUtilities.enumLinqOperator.EqualTo);

            foreach (IsosResult result in chargeTwoResultsOnly)
            {
                Assert.AreEqual(2, result.IsotopicProfile.ChargeState);
            }
            Assert.AreEqual(136, chargeTwoResultsOnly.Count);
            



            double avgFit = IsosResultUtilities.getAverageScore(chargeTwoResultsOnly);
            Assert.AreEqual(0.0490575547734101m, (decimal)avgFit);


        }

        [Test]
        public void importIsosResultsAndFilterOnChargeStatesTest1()
        {
            IsosResultUtilities isosUtil = new IsosResultUtilities();
            isosUtil.LoadResults(xcaliburThrashIsos1, DeconTools.Backend.Globals.MSFileType.Finnigan);

            Assert.AreEqual(11610, isosUtil.Results.Count);

            List<IsosResult> chargeOneResultsOnly = IsosResultUtilities.getIsosResultsByChargeState(isosUtil.Results, 1, IsosResultUtilities.enumLinqOperator.EqualTo);
            List<IsosResult> chargeTwoResultsOnly = IsosResultUtilities.getIsosResultsByChargeState(isosUtil.Results, 2, IsosResultUtilities.enumLinqOperator.EqualTo);
            List<IsosResult> chargeThreeResultsOnly = IsosResultUtilities.getIsosResultsByChargeState(isosUtil.Results, 3, IsosResultUtilities.enumLinqOperator.EqualTo);
            List<IsosResult> chargeFourResultsOnly = IsosResultUtilities.getIsosResultsByChargeState(isosUtil.Results, 4, IsosResultUtilities.enumLinqOperator.EqualTo);
            List<IsosResult> chargeFiveResultsOnly = IsosResultUtilities.getIsosResultsByChargeState(isosUtil.Results, 5, IsosResultUtilities.enumLinqOperator.EqualTo);
            List<IsosResult> chargeSixResultsOnly = IsosResultUtilities.getIsosResultsByChargeState(isosUtil.Results, 6, IsosResultUtilities.enumLinqOperator.EqualTo);

            List<IsosResult> resultsWithChargeLessThan4 = IsosResultUtilities.getIsosResultsByChargeState(isosUtil.Results, 4, IsosResultUtilities.enumLinqOperator.lessThan);
            List<IsosResult> resultsWithChargeMoreThan6 = IsosResultUtilities.getIsosResultsByChargeState(isosUtil.Results, 6, IsosResultUtilities.enumLinqOperator.greaterThan);

            Assert.AreEqual(2323, chargeOneResultsOnly.Count);
            Assert.AreEqual(5244, chargeTwoResultsOnly.Count);
            Assert.AreEqual(2866, chargeThreeResultsOnly.Count);
            Assert.AreEqual(946, chargeFourResultsOnly.Count);
            Assert.AreEqual(153, chargeFiveResultsOnly.Count);
            Assert.AreEqual(56, chargeSixResultsOnly.Count);

            Assert.AreEqual(2323 + 5244 + 2866, resultsWithChargeLessThan4.Count);
            Assert.AreEqual(22, resultsWithChargeMoreThan6.Count);



        }


        [Test]
        public void importLargeIsosFileTest1()
        {
            IsosResultUtilities isosUtil = new IsosResultUtilities();
            isosUtil.LoadResults(largeIsosFile1, DeconTools.Backend.Globals.MSFileType.Finnigan);

            Assert.AreEqual(217847, isosUtil.Results.Count);

            int count = IsosResultUtilities.getCount(isosUtil.Results);
            Assert.AreEqual(200665, count);

            List<IsosResult> chargeOneResultsOnly = IsosResultUtilities.getIsosResultsByChargeState(isosUtil.Results, 1, IsosResultUtilities.enumLinqOperator.EqualTo);
            List<IsosResult> chargeTwoResultsOnly = IsosResultUtilities.getIsosResultsByChargeState(isosUtil.Results, 2, IsosResultUtilities.enumLinqOperator.EqualTo);
            List<IsosResult> chargeThreeResultsOnly = IsosResultUtilities.getIsosResultsByChargeState(isosUtil.Results, 3, IsosResultUtilities.enumLinqOperator.EqualTo);


            Assert.AreEqual(27541, chargeOneResultsOnly.Count);

            double avgScore = IsosResultUtilities.getAverageScore(chargeOneResultsOnly);
            double stdev = IsosResultUtilities.getStdDevScore(chargeOneResultsOnly);
            Console.WriteLine(avgScore);
            Console.WriteLine(stdev);


        }

        [Test]
        public void loadUIMFIsosResultsTest1()
        {
            IsosResultUtilities isosUtil = new IsosResultUtilities();
            isosUtil.LoadResults(uimfIsos1, DeconTools.Backend.Globals.MSFileType.PNNL_UIMF);

            Assert.AreEqual(1346, isosUtil.Results.Count);
            
            UIMFIsosResult uimfTestResult1=(UIMFIsosResult)isosUtil.Results[3];

            Assert.AreEqual(1200, uimfTestResult1.FrameSet.PrimaryFrame);
            Assert.AreEqual(250, uimfTestResult1.ScanSet.PrimaryScanNumber);
            Assert.AreEqual(3, uimfTestResult1.IsotopicProfile.ChargeState);
            Assert.AreEqual(1288, uimfTestResult1.IsotopicProfile.GetAbundance());
            //Assert.AreEqual(432.92416, uimfTestResult1.IsotopicProfile.GetMZofMostAbundantPeak());
            Assert.AreEqual(0.1458, uimfTestResult1.IsotopicProfile.Score);
            //Assert.AreEqual(1296.54608, uimfTestResult1.IsotopicProfile.AverageMass);
            Assert.AreEqual(1295.75229, uimfTestResult1.IsotopicProfile.MonoIsotopicMass);
            Assert.AreEqual(0.0548,Math.Round(uimfTestResult1.IsotopicProfile.GetFWHM(),4));
            Assert.AreEqual(92, uimfTestResult1.IsotopicProfile.GetSignalToNoise());

            
            
            //Assert.AreEqual(1200, uimfTestResult1.FrameSet.PrimaryFrame);

            //250	3	1288	432.92416	0.1458	1296.54608	1295.75229	1295.75229	0.0548	92


        }

        [Test]
        public void loadIMFIsosResultTest1()
        {
            IsosResultUtilities isosUtil = new IsosResultUtilities();
            isosUtil.LoadResults(imfIsos1, DeconTools.Backend.Globals.MSFileType.PNNL_IMS);

            Assert.AreEqual(43, isosUtil.Results.Count);

            IsosResult imfTestResult1 = isosUtil.Results[3];

            Assert.AreEqual(231, imfTestResult1.ScanSet.PrimaryScanNumber);
            Assert.AreEqual(2, imfTestResult1.IsotopicProfile.ChargeState);
            Assert.AreEqual(7768, imfTestResult1.IsotopicProfile.GetAbundance());
            //Assert.AreEqual(432.92416, uimfTestResult1.IsotopicProfile.GetMZofMostAbundantPeak());
            Assert.AreEqual(0.0098, imfTestResult1.IsotopicProfile.Score);
            //Assert.AreEqual(1142.4074, uimfTestResult1.IsotopicProfile.AverageMass);
            Assert.AreEqual(1141.7091, imfTestResult1.IsotopicProfile.MonoIsotopicMass);
            Assert.AreEqual(0.0633,Math.Round(imfTestResult1.IsotopicProfile.GetFWHM(),4));
            Assert.AreEqual(112.58, Math.Round(imfTestResult1.IsotopicProfile.GetSignalToNoise(),2));

            //231,2,7768,571.8613,0.0098,1142.4074,1141.7091,1141.7091,0.0633,112.58,7768,1640

        }

        [Test]
        public void loadIMFIsosResultTest2()
        {

        }

        [Test]
        public void loadUIMFIsosResults_loadOneFrameOnly_Test1()
        {
            IsosResultUtilities isosUtil = new IsosResultUtilities();
            isosUtil.LoadResults(uimfIsos1, DeconTools.Backend.Globals.MSFileType.PNNL_UIMF,500);

            Assert.AreEqual(1346, isosUtil.Results.Count);

            UIMFIsosResult uimfTestResult1 = (UIMFIsosResult)isosUtil.Results[3];

            Assert.AreEqual(1200, uimfTestResult1.FrameSet.PrimaryFrame);
            Assert.AreEqual(250, uimfTestResult1.ScanSet.PrimaryScanNumber);
            Assert.AreEqual(3, uimfTestResult1.IsotopicProfile.ChargeState);
            Assert.AreEqual(1288, uimfTestResult1.IsotopicProfile.GetAbundance());
            //Assert.AreEqual(432.92416, uimfTestResult1.IsotopicProfile.GetMZofMostAbundantPeak());
            Assert.AreEqual(0.1458, uimfTestResult1.IsotopicProfile.Score);
            //Assert.AreEqual(1296.54608, uimfTestResult1.IsotopicProfile.AverageMass);
            Assert.AreEqual(1295.75229, uimfTestResult1.IsotopicProfile.MonoIsotopicMass);
            Assert.AreEqual(0.0548, Math.Round(uimfTestResult1.IsotopicProfile.GetFWHM(), 4));
            Assert.AreEqual(92, uimfTestResult1.IsotopicProfile.GetSignalToNoise());



            //Assert.AreEqual(1200, uimfTestResult1.FrameSet.PrimaryFrame);

            //250	3	1288	432.92416	0.1458	1296.54608	1295.75229	1295.75229	0.0548	92


        }

        [Test]
        public void loadIMFResultsAndThenExport()
        {
            IsosResultUtilities iru = new IsosResultUtilities();
            List<IsosResult>results = iru.getIMFResults(imfIsosFolder, 500, 600);

            UIMFIsosExporter exporter = new UIMFIsosExporter(Path.Combine(imfIsosFolder, mergedIsosResults.csv"));

            ResultCollection rc = new ResultCollection(new IMFRun());
            rc.ResultList = results;
            exporter.Export(rc);



        }



        private void outputFitValuesToConsole(List<IsosResult> results)
        {
            StringBuilder sb = new StringBuilder();

            foreach (IsosResult result in results)
            {
                sb.Append(result.IsotopicProfile.Score);
                sb.Append("\n");
            }
            Console.Write(sb.ToString());
        }


    }
}
