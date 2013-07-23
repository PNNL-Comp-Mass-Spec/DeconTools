using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.UnitTesting2;
using DeconTools.Backend;
using DeconTools.Workflows.Backend.Core;
using System.IO;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks.ChargeStateDeciders;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor;

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
            //Run run = RunUtilities.CreateAndLoadPeaks(fileName);
            Run run = new RunFactory().CreateRun(fileName);
            List<IsotopicProfile> potentialFeatures;
            EasyDecisionSetUp(out potentialFeatures);
            ChargeStateDecider chargestatedecider = new ChromCorrelatingChargeDecider(run);
            var msFeature = chargestatedecider.DetermineCorrectIsotopicProfile(potentialFeatures);

            Assert.AreEqual(msFeature.ChargeState, 2);
        }
        private void EasyDecisionSetUp( out List<IsotopicProfile> potentialFeatures)
        {
            

            MSPeak peak1_iso1 = MakeMSPeak(0.007932149, 481.27410112055895);
            MSPeak peak2_iso1 = MakeMSPeak(0.007846226, 481.775417927883);
            MSPeak peak3_iso1 = MakeMSPeak(0.00768206455, 482.27682748526291);
            List<MSPeak> peaklist1 = MakeMSPeakList(peak1_iso1, peak2_iso1, peak3_iso1);
            IsotopicProfile iso1 = MakePotentialFeature(2, 481.27410112055895, 0, peaklist1);

            MSPeak peak1_iso2 = MakeMSPeak(0.007932149, 481.27410112055895);
            MSPeak peak2_iso2 = MakeMSPeak(0.00768206455, 482.27682748526291);
            List<MSPeak> peaklist2 = MakeMSPeakList(peak1_iso2, peak2_iso2);
            IsotopicProfile iso2 = MakePotentialFeature(1, 481.27410112055895, 0, peaklist2);

          

            potentialFeatures = MakePotentialFeaturesList(iso1, iso2);
        }

        [Test]
        public void MediumDecision()
        {
            string fileName =
                                @"\\pnl\projects\MSSHARE\Gord\For_Paul\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            //Run run = RunUtilities.CreateAndLoadPeaks(fileName);
            Run run = new RunFactory().CreateRun(fileName);

            List<IsotopicProfile> potentialFeatures;
            MediumDecisionSetup(out potentialFeatures);

            ChargeStateDecider chargestatedecider = new ChromCorrelatingChargeDecider(run);
            var msFeature = chargestatedecider.DetermineCorrectIsotopicProfile(potentialFeatures);

            //Assert something here.

        }

        private void MediumDecisionSetup(out List<IsotopicProfile> potentialFeatures)
        {
            

            MSPeak peak1_iso1 = MakeMSPeak(0.0151630929, 746.70851219111921);
            MSPeak peak2_iso1 = MakeMSPeak(0.0151641443, 746.87333076347306);
            List<MSPeak> peaklist1 = MakeMSPeakList(peak1_iso1, peak2_iso1);
            IsotopicProfile iso1 = MakePotentialFeature(6, 746.5392140968064, -1, peaklist1);

            MSPeak peak1_iso2 = MakeMSPeak(0.0151641443, 746.87333076347306);
            MSPeak peak2_iso2 = MakeMSPeak(0.01580638, 747.37507455840785);
            MSPeak peak3_iso2 = MakeMSPeak(0.0153686106, 747.87687140741934);
            List<MSPeak> peaklist2 = MakeMSPeakList(peak1_iso2, peak2_iso2, peak3_iso2);
            IsotopicProfile iso2 = MakePotentialFeature(2, 746.87333076347306, 0, peaklist2);

            MSPeak peak1_iso3 = MakeMSPeak(0.0151641443, 746.87333076347306);
            MSPeak peak2_iso3 = MakeMSPeak(0.0153686106, 747.87687140741934);
            List<MSPeak> peaklist3 = MakeMSPeakList(peak1_iso2, peak2_iso2);
            IsotopicProfile iso3 = MakePotentialFeature(1, 746.87333076347306, 0, peaklist3);

            potentialFeatures = MakePotentialFeaturesList(iso1, iso2, iso3);
        }

        [Test]
        public void HardDecision()
        {

            //Assert.AreEqual(msFeature.ChargeState, 2);

        }

        [Test]
        public void testPeakGeneration()
        {
            string fileName =
                 @"\\pnl\projects\MSSHARE\Gord\For_Paul\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            Run run = new RunFactory().CreateRun(fileName);

            PeakDetectAndExportWorkflowParameters parameters = new PeakDetectAndExportWorkflowParameters();
            parameters.LCScanMin = 5000;
            parameters.LCScanMax = 7000;


            string expectedPeaksFile = run.DataSetPath + "\\" + run.DatasetName + "_peaks.txt";
            if (File.Exists(expectedPeaksFile)) File.Delete(expectedPeaksFile);


            PeakDetectAndExportWorkflow workflow = new PeakDetectAndExportWorkflow(run, parameters);
            workflow.Execute();

            var fileinfo = new FileInfo(expectedPeaksFile);
            Assert.IsTrue(fileinfo.Exists);
            Assert.IsTrue(fileinfo.Length > 1000000);

        }
        [Test]
        public void GetPeakChromatogram_IQStyle_Test1()
        {

            //Run run = RunUtilities.CreateAndLoadPeaks(FileRefs.RawDataMSFiles.OrbitrapStdFile1,
            //                                          FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500);
            string fileName =
                 @"\\pnl\projects\MSSHARE\Gord\For_Paul\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            Run run = RunUtilities.CreateAndLoadPeaks(fileName);

            var target = TestUtilities.GetIQTargetStandard(1);


            //TestUtilities.DisplayIsotopicProfileData(target.TheorIsotopicProfile);

            var chromGen = new PeakChromatogramGenerator();
            chromGen.ChromatogramGeneratorMode = Globals.ChromatogramGeneratorMode.TOP_N_PEAKS;
            //chromGen.ChromatogramGeneratorMode = Globals.ChromatogramGeneratorMode.;
            chromGen.TopNPeaksLowerCutOff = 0.4;
            chromGen.Tolerance = 10;


            //var chromXYData = chromGen.GenerateChromatogram(run, target.TheorIsotopicProfile, target.ElutionTimeTheor);
            // var chromXYData = chromGen.GenerateChromatogram(run, new List<double> { 481.27410, 481.77542, 482.27683 }, 4000, 6500, 0.009, Globals.ToleranceUnit.MZ);
            //   var chromXYData = chromGen.GenerateChromatogram(run,500,7000,490.26483,0.005,Globals.ToleranceUnit.MZ);


            //  Assert.IsNotNull(chromXYData);

            Console.WriteLine("481.27410");
            var chromXYData1 = chromGen.GenerateChromatogram(run, 500, 7000, 481.27410, 0.005, Globals.ToleranceUnit.MZ); TestUtilities.DisplayXYValues(chromXYData1);
            //  TestUtilities.DisplayXYValues(chromXYData);
            double minx1 = chromXYData1.Xvalues.Min();
            double maxx1 = chromXYData1.Xvalues.Max();
            Console.WriteLine("481.77542");
            // chromXYData1.
            var chromXYData2 = chromGen.GenerateChromatogram(run, 500, 7000, 481.77542, 0.005, Globals.ToleranceUnit.MZ); TestUtilities.DisplayXYValues(chromXYData2);
            // TestUtilities.DisplayXYValues(chromXYData);
            double minx2 = chromXYData2.Xvalues.Min();
            double maxx2 = chromXYData2.Xvalues.Max();

            Console.WriteLine("482.27683");
            var chromXYData3 = chromGen.GenerateChromatogram(run, 500, 7000, 482.27683, 0.005, Globals.ToleranceUnit.MZ); TestUtilities.DisplayXYValues(chromXYData3);


            TestUtilities.DisplayXYValues(chromXYData3);
            double minx3 = chromXYData3.Xvalues.Min();
            double maxx3 = chromXYData3.Xvalues.Max();

            double minxhelper = Math.Max(minx1, minx2);
            double minx = Math.Max(minxhelper, minx3);
            double maxxhelper = Math.Min(maxx1, maxx2);
            double maxX = Math.Min(maxxhelper, maxx3);

            int c1start = chromXYData1.GetClosestXVal(minx);
            int c2start = chromXYData2.GetClosestXVal(minx);
            int c3start = chromXYData3.GetClosestXVal(minx);
            int c1stop = chromXYData1.GetClosestXVal(maxX);
            int c2stop = chromXYData2.GetClosestXVal(maxX);
            int c3stop = chromXYData3.GetClosestXVal(maxX);
            chromXYData1.NormalizeYData();
            chromXYData2.NormalizeYData();
            chromXYData3.NormalizeYData();
            double[] c1 = new double[c1stop - c1start + 1];
            double[] c2 = new double[c2stop - c2start + 1];
            double[] c3 = new double[c3stop - c3start + 1];


            for (int i = c1start, j = 0; i <= c1stop; i++, j++)
            {
                c1[j] = chromXYData1.Yvalues[i];

            }
            for (int i = c2start, j = 0; i <= c2stop; i++, j++)
            {
                c2[j] = chromXYData2.Yvalues[i];

            }
            for (int i = c3start, j = 0; i <= c3stop; i++, j++)
            {
                c3[j] = chromXYData3.Yvalues[i];

            }

            for (int i = 0; i < c1.Length; i++)
            {
                Console.WriteLine("{0}\t{1}\t{2}", c1[i], c2[i], c3[i]);

            }

            // double c1chromEven= chromXYData1.Xvalues
            double corr1 = MathNet.Numerics.Statistics.Correlation.Pearson(c1, c2);
            double corr2 = MathNet.Numerics.Statistics.Correlation.Pearson(c1, c3);
            Console.WriteLine("HERE IS THE CORRELATION: " + corr1);
            Console.WriteLine("HERE IS THE CORRELATION: " + corr2);





            //bool alldone = false, done1 = false, done2 = false, done3 = false;
            //int onestarting=0,twostarting=0,threestarting=0;
            //while (!alldone)
            //{
            //    if (!done1)
            //    {
            //        if (chromXYData1.Xvalues[onestarting]==minx)
            //        {
            //            done1 = true;
            //        }
            //        else
            //        {

            //        }

            //    }
            //    if (!done2)
            //    {

            //    }
            //    if (!done3)
            //    {

            //    }
            //    alldone = done1 && done2 && done3;
            //}



        }



        private MSPeak MakeMSPeak(double width, double xValue)
        {
            MSPeak mspeak = new MSPeak();
            mspeak.Width = (float)width;
            mspeak.XValue = xValue;
            return mspeak;
        }

        private List<MSPeak> MakeMSPeakList(params MSPeak[] msPeaks)
        {
            return msPeaks.ToList();
        }

        private List<IsotopicProfile> MakePotentialFeaturesList(params IsotopicProfile[] isotopicProfiles)
        {
            return isotopicProfiles.ToList(); //cool...
        }

        private IsotopicProfile MakePotentialFeature(int chargeState, double monoPeakMZ, int monoIsotpoicPeakIndex, params MSPeak[] peakList)
        {
           return MakePotentialFeature(chargeState, monoPeakMZ, monoIsotpoicPeakIndex, peakList.ToList());
        }
        private IsotopicProfile MakePotentialFeature(int chargeState, double monoPeakMZ, int monoIsotpoicPeakIndex, List<MSPeak> peakList)
        {
            IsotopicProfile isopo = new IsotopicProfile();
            isopo.ChargeState = chargeState;
            isopo.MonoPeakMZ = monoPeakMZ;
            isopo.MonoIsotopicPeakIndex = monoIsotpoicPeakIndex;
            isopo.Peaklist = peakList;
            return isopo;
        }
    }

}
