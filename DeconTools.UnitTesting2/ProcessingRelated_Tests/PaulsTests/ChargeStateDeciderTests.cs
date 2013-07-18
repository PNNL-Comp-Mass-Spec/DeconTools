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

            #region SetUpFakePotentialFeatures
            List<IsotopicProfile> potentialFeatures = new List<IsotopicProfile>();
              
              IsotopicProfile iso1 = new IsotopicProfile();
              iso1.ChargeState = 2;
              iso1.MonoPeakMZ = 481.27410112055895;
              iso1.MonoIsotopicPeakIndex = 0;
              List<MSPeak> peaklist1 = new List<MSPeak>();

                  MSPeak peak1_iso1 = new MSPeak();
                  peak1_iso1.Width = 0.007932149f;
                  peak1_iso1.XValue = 481.27410112055895;
                  peaklist1.Add(peak1_iso1);

                  MSPeak peak2_iso1 = new MSPeak();
                  peak2_iso1.Width = 0.007846226f;
                  peak2_iso1.XValue = 481.775417927883;
                  peaklist1.Add(peak2_iso1);

                  MSPeak peak3_iso1 = new MSPeak();
                  peak3_iso1.Width = 0.00768206455f;
                  peak3_iso1.XValue = 482.27682748526291;
                  peaklist1.Add(peak3_iso1);
              iso1.Peaklist = peaklist1;
              potentialFeatures.Add(iso1);


              IsotopicProfile iso2 = new IsotopicProfile();
              iso2.ChargeState = 1;
              iso2.MonoPeakMZ = 481.27410112055895;
              iso2.MonoIsotopicMass = 480.266824630559;
              iso2.MonoIsotopicPeakIndex = 0;
                  List<MSPeak> peakList2 = new List<MSPeak>();
                  MSPeak peak1_iso2 = new MSPeak();
                  peak1_iso2.DataIndex = 3769;
                  peak1_iso2.Height = 13084442.0f;
                  peak1_iso2.Width = 0.007932149f;
                  peak1_iso2.XValue = 481.27410112055895;
                  peakList2.Add(peak1_iso2);


                  MSPeak peak2_iso2 = new MSPeak();
                  peak2_iso2.DataIndex = 3846;
                  peak2_iso2.Height = 1746590.88f;
                  peak2_iso2.Width = 0.00768206455f;
                  peak2_iso2.XValue = 482.27682748526291;
                  peakList2.Add(peak2_iso2);
              iso2.Peaklist = peakList2;
              potentialFeatures.Add(iso2);
            #endregion

              ChargeStateDecider chargestatedecider = new XICCorrelatingChargeDecider(run);
              var msFeature= chargestatedecider.DetermineCorrectIsotopicProfile(potentialFeatures);

              Assert.AreEqual(msFeature.ChargeState, 2);

        }

        [Test]
        public void MediumDecision()
        {
            string fileName =
                    @"\\pnl\projects\MSSHARE\Gord\For_Paul\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            //Run run = RunUtilities.CreateAndLoadPeaks(fileName);
            Run run = new RunFactory().CreateRun(fileName);
            #region SetUpFakePotentialFeatures
            List<IsotopicProfile> potentialFeatures = new List<IsotopicProfile>();

            IsotopicProfile iso1 = new IsotopicProfile();

            iso1.ChargeState = 6;
            iso1.MonoPeakMZ =746.5392140968064 ;
            iso1.MonoIsotopicPeakIndex = -1;
            List<MSPeak> peaklist1 = new List<MSPeak>();

                MSPeak peak1_iso1 = new MSPeak();
                peak1_iso1.Width = 0.0151630929f;
                peak1_iso1.XValue =746.70851219111921 ;
                peaklist1.Add(peak1_iso1);

                MSPeak peak2_iso1 = new MSPeak();
                peak2_iso1.Width =0.0151641443f ;
                peak2_iso1.XValue = 746.87333076347306;
                peaklist1.Add(peak2_iso1);
            iso1.Peaklist = peaklist1;
            potentialFeatures.Add(iso1);


            IsotopicProfile iso2 = new IsotopicProfile();
            iso2.ChargeState = 2;
            iso2.MonoPeakMZ = 746.87333076347306;
            iso2.MonoIsotopicPeakIndex = 0;
            List<MSPeak> peakList2 = new List<MSPeak>();
                MSPeak peak1_iso2 = new MSPeak();
                peak1_iso2.Width = 0.0151641443f;
                peak1_iso2.XValue = 746.87333076347306;
                peakList2.Add(peak1_iso2);


                MSPeak peak2_iso2 = new MSPeak();
                peak2_iso2.Width = 0.01580638f;
                peak2_iso2.XValue = 747.37507455840785;
                peakList2.Add(peak2_iso2);

                MSPeak peak3_iso2 = new MSPeak();
                peak3_iso2.Width = 0.0153686106f;
                peak3_iso2.XValue = 747.87687140741934;
                peakList2.Add(peak3_iso2);

            iso2.Peaklist = peakList2;
            potentialFeatures.Add(iso2);

            IsotopicProfile iso3 = new IsotopicProfile();
            iso3.ChargeState = 1;
            iso3.MonoPeakMZ = 746.87333076347306;
            iso3.MonoIsotopicPeakIndex =0 ;
            List<MSPeak> peakList3 = new List<MSPeak>();
                MSPeak peak1_iso3 = new MSPeak();
                peak1_iso3.Width = 0.0151641443f;
                peak1_iso3.XValue = 746.87333076347306;
                peakList3.Add(peak1_iso3);


                MSPeak peak2_iso3 = new MSPeak();
                peak2_iso3.Width = 0.0153686106f;
                peak2_iso3.XValue = 747.87687140741934;
                peakList3.Add(peak2_iso3);

            iso3.Peaklist = peakList2;
            potentialFeatures.Add(iso3);
            #endregion


            ChargeStateDecider chargestatedecider = new XICCorrelatingChargeDecider(run);
            var msFeature = chargestatedecider.DetermineCorrectIsotopicProfile(potentialFeatures);

            //Assert something here.
        
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
            double[] c1= new double[c1stop-c1start+1];
            double[] c2= new double[c2stop-c2start+1];
            double[] c3= new double[c3stop-c3start+1];

           
            for (int i = c1start,j=0; i <= c1stop ; i++, j++)
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
                Console.WriteLine("{0}\t{1}\t{2}",c1[i],c2[i],c3[i]);
                
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

    }
}
