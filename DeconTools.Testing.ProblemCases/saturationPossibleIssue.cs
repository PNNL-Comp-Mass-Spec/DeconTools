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

namespace DeconTools.Testing.ProblemCases
{
    [TestFixture]
    public class saturationPossibleIssue
    {
        [Test]
        public void saturatedFixingTest1()
        {
            string uimfFile =
                @"D:\Data\UIMF\Sarc_Main_Study_Controls\Sarc_P08_G02_0746_7Dec11_Cheetah_11-09-04.uimf";

            Run run = new RunFactory().CreateRun(uimfFile);
            OldDecon2LSParameters parameters = new OldDecon2LSParameters();
            parameters.Load(@"\\gigasax\DMS_Parameter_Files\Decon2LS\IMS_UIMF_PeakBR4_PeptideBR4_SN3_SumScans3_NoLCSum_Sat50000_2012-01-30.xml");

            parameters.HornTransformParameters.UseScanRange = true;
            parameters.HornTransformParameters.MinScan = 403;
            parameters.HornTransformParameters.MaxScan = 409;
            
            parameters.HornTransformParameters.ScanBasedWorkflowType = "uimf_saturation_repair";

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.ExportData = true;

            var sw = new Stopwatch();
            sw.Start();

            workflow.Execute();
            return;

            sw.Stop();

            var distinctItems = run.ResultCollection.ResultList.GroupBy(x => x.MSFeatureID).Select(y => y.First()).ToList();

            int minFrame = parameters.HornTransformParameters.MinScan;
            int maxFrame = parameters.HornTransformParameters.MaxScan;

            int minScan = 102;
            int maxScan = 125;

            
            double targetMass = 1642.85958;
            int chargestate = 4;


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
        public void saturatedFixingTest2()
        {
            string uimfFile =
                @"D:\Data\UIMF\Sarc_Main_Study_Controls\Sarc_P08_A01_0673_21Nov11_Cheetah_11-09-03.uimf";

            Run run = new RunFactory().CreateRun(uimfFile);
            OldDecon2LSParameters parameters = new OldDecon2LSParameters();
            parameters.Load(@"\\gigasax\DMS_Parameter_Files\Decon2LS\IMS_UIMF_PeakBR4_PeptideBR4_SN3_SumScans3_NoLCSum_Sat50000_2012-01-30.xml");

            parameters.HornTransformParameters.UseScanRange = true;
            parameters.HornTransformParameters.MinScan = 397;
            parameters.HornTransformParameters.MaxScan = 408;

            parameters.HornTransformParameters.ScanBasedWorkflowType = "uimf_saturation_repair";

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.ExportData = true;

            var sw = new Stopwatch();
            sw.Start();

            workflow.Execute();
            return;

            sw.Stop();

            var distinctItems = run.ResultCollection.ResultList.GroupBy(x => x.MSFeatureID).Select(y => y.First()).ToList();

            int minFrame = parameters.HornTransformParameters.MinScan;
            int maxFrame = parameters.HornTransformParameters.MaxScan;

            int minScan = 102;
            int maxScan = 125;


            double targetMass = 1642.85958;
            int chargestate = 4;


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




        private static void OutputFeatureIntensityData(List<UIMFIsosResult> featureData, int minFrame, int maxFrame, int maxScan, int minScan)
        {
            StringBuilder sb = new StringBuilder();
            for (int scan = minScan; scan <= maxScan; scan++)
            {
                for (int frame = minFrame; frame <= maxFrame; frame++)
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
