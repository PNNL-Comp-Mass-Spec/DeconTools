using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Workflows;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Workflow_Tests
{
    [TestFixture]
    public class SaturationIMSWorkflowTests
    {
        
        [Test]
        public void saturatedFixingTest1()
        {
            string uimfFile = @"D:\Data\UIMF\Sarc_Main_Study_Controls\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";

            Run run = new RunFactory().CreateRun(uimfFile);
            OldDecon2LSParameters parameters = new OldDecon2LSParameters();

            parameters.PeakProcessorParameters.PeakBackgroundRatio = 4;
            parameters.PeakProcessorParameters.SignalToNoiseThreshold = 3;
            parameters.HornTransformParameters.MaxFit = 0.6;
            parameters.HornTransformParameters.UseScanRange = true;
            parameters.HornTransformParameters.MinScan = 290;
            parameters.HornTransformParameters.MaxScan = 310;
            parameters.HornTransformParameters.SumSpectraAcrossFrameRange = true;
            parameters.HornTransformParameters.SumSpectraAcrossScanRange = true;
            parameters.HornTransformParameters.NumFramesToSumOver = 1;
            parameters.HornTransformParameters.NumScansToSumOver = 3;
            parameters.HornTransformParameters.ZeroFill = true;

            parameters.HornTransformParameters.ScanBasedWorkflowType = "uimf_saturation_repair";



            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.ExportData = false;


            Stopwatch sw=new Stopwatch();
            sw.Start();

            workflow.Execute();
           // return;

            sw.Stop();

            var distinctItems = run.ResultCollection.ResultList.GroupBy(x => x.MSFeatureID).Select(y => y.First()).ToList();

          int minFrame = parameters.HornTransformParameters.MinScan;
            int maxFrame = parameters.HornTransformParameters.MaxScan;

            int minScan = 102;
            int maxScan = 125;

            double targetMass = 860.3987;
            int chargestate = 2;

            //targetMass = 1444.748171;
            //chargestate = 3;

            targetMass = 1079.559447;
            chargestate = 2;

           // targetMass= 1064.485;
           // chargestate = 2;


            //targetMass = 949.454723;
            //chargestate = 1;
            //minScan = 188;
            //maxScan = 205;



            double tolerance = 0.3;
            var featureData =
                (from n in distinctItems
                 where (Math.Abs(n.IsotopicProfile.MonoIsotopicMass - targetMass)) < tolerance &&
                       n.IsotopicProfile.ChargeState == chargestate
                 select n).Select<IsosResult, UIMFIsosResult>(r => (UIMFIsosResult)r).ToList();


            OutputFeatureIntensityData(featureData, minFrame, maxFrame, maxScan, minScan);

            MathUtils mathUtils = new MathUtils();

            var monoMasses = (from n in featureData select n.IsotopicProfile.MonoIsotopicMass).ToList();

            var massVariance = MathUtils.GetStDev(monoMasses);
            Console.WriteLine("Mass variance = " + massVariance);
            Console.WriteLine("Time taken = " + sw.ElapsedMilliseconds);

        }

        [Test]
        public void noFixingTest1()
        {
            string uimfFile = @"D:\Data\UIMF\Sarc_Main_Study_Controls\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";

            Run run = new RunFactory().CreateRun(uimfFile);
            OldDecon2LSParameters parameters = new OldDecon2LSParameters();


            parameters.PeakProcessorParameters.PeakBackgroundRatio = 4;
            parameters.PeakProcessorParameters.SignalToNoiseThreshold = 3;
            parameters.HornTransformParameters.UseScanRange = true;
            parameters.HornTransformParameters.MinScan = 180;
            parameters.HornTransformParameters.MaxScan = 180;
            parameters.HornTransformParameters.SumSpectraAcrossFrameRange = true;
            parameters.HornTransformParameters.SumSpectraAcrossScanRange = true;
            parameters.HornTransformParameters.NumFramesToSumOver = 1;
            parameters.HornTransformParameters.NumScansToSumOver = 3;


            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);

            workflow.Execute();

        }


        [Test]
        public void frameSetCollectionCreatedOKTest1()
        {
            string uimfFile = @"D:\Data\UIMF\Sarc_Main_Study_Controls\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";

            Run run = new RunFactory().CreateRun(uimfFile);
            OldDecon2LSParameters parameters = new OldDecon2LSParameters();

            parameters.PeakProcessorParameters.PeakBackgroundRatio = 4;
            parameters.PeakProcessorParameters.SignalToNoiseThreshold = 3;
            parameters.HornTransformParameters.MaxFit = 0.6;
            parameters.HornTransformParameters.UseScanRange = true;
            parameters.HornTransformParameters.MinScan = 290;
            parameters.HornTransformParameters.MaxScan = 310;
            parameters.HornTransformParameters.SumSpectraAcrossFrameRange = true;
            parameters.HornTransformParameters.SumSpectraAcrossScanRange = true;
            parameters.HornTransformParameters.NumFramesToSumOver = 1;
            parameters.HornTransformParameters.NumScansToSumOver = 3;
            parameters.HornTransformParameters.ZeroFill = true;

            parameters.HornTransformParameters.ScanBasedWorkflowType = "uimf_saturation_repair";



            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);


            Assert.AreEqual(360, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(21, ((UIMFRun)run).FrameSetCollection.FrameSetList.Count);


            parameters.HornTransformParameters.UseScanRange = false;       //refers to LC Scan range (i.e. frame range)
            workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);

            Assert.AreEqual(360, run.ScanSetCollection.ScanSetList.Count);
            Assert.AreEqual(1175, ((UIMFRun)run).FrameSetCollection.FrameSetList.Count);

            
        }




        private static void OutputFeatureIntensityData(List<UIMFIsosResult> featureData, int minFrame, int maxFrame, int maxScan, int minScan)
        {



            StringBuilder sb = new StringBuilder();
            for (int frame = minFrame; frame <= maxFrame; frame++)
            {
                for (int scan = minScan; scan <= maxScan; scan++)
                {
                    var feature =
                        (from n in featureData
                         where n.ScanSet.PrimaryScanNumber == scan && n.FrameSet.PrimaryFrame == frame
                         select n).FirstOrDefault();

                    double intensity;
                    if (feature == null)
                    {
                        intensity = 0;
                    }
                    else
                    {
                        intensity = feature.IsotopicProfile.OriginalIntensity;
                    }

                    sb.Append(intensity.ToString("0"));
                    sb.Append("\t");
                }

                sb.Append(Environment.NewLine);
            }

            Console.WriteLine(sb.ToString());
        }


    }
}
