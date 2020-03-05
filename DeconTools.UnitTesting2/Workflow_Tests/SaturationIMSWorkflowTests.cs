using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Parameters;
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
        public void saturatedFixingOnMelittinTest0()
        {
            var uimfFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\DRsample_30ms_16Apr13_0004.UIMF";

            var run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters
            {
                PeakDetectorParameters =
                {
                    PeakToBackgroundRatio = 4,
                    SignalToNoiseThreshold = 3
                },
                ThrashParameters = {MaxFit = 0.6},
                MSGeneratorParameters =
                {
                    UseLCScanRange = true,
                    MinLCScan = 11,
                    MaxLCScan = 11,
                    UseMZRange = true,
                    MinMZ = 710,
                    MaxMZ = 715,
                    SumSpectraAcrossLC = true,
                    SumSpectraAcrossIms = true,
                    NumLCScansToSum = 1,
                    NumImsScansToSum = 1
                },
                MiscMSProcessingParameters = {UseZeroFilling = true}
            };


            parameters.ThrashParameters.MinIntensityForDeletion = 10;
            parameters.MiscMSProcessingParameters.SaturationThreshold = 12000;

            parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName = "standard";


            Console.WriteLine("---------------- No saturation correction ----------------");
            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.ExportData = false;
            workflow.Execute();

            var distinctItems = run.ResultCollection.ResultList.GroupBy(x => x.MSFeatureID).Select(y => y.First()).ToList();

            var minFrame = parameters.MSGeneratorParameters.MinLCScan;
            var maxFrame = parameters.MSGeneratorParameters.MaxLCScan;

            var minScan = 100;
            var maxScan = 160;

            var melettinMonoMass = 2844.75417;
            var chargeState = 4;

            var tolerance = 0.3;
            var featureData =
                (from n in distinctItems
                 where (Math.Abs(n.IsotopicProfile.MonoIsotopicMass - melettinMonoMass)) < tolerance &&
                       n.IsotopicProfile.ChargeState == chargeState
                 select n).Select(r => (UIMFIsosResult)r).ToList();

            OutputFeatureIntensityData(featureData, minFrame, maxFrame, maxScan, minScan);

            var monoMasses = (from n in featureData select n.IsotopicProfile.MonoIsotopicMass).ToList();

            var massVariance = MathUtils.GetStDev(monoMasses);
            Console.WriteLine("Mass variance = " + massVariance);


            run.ResultCollection.ClearAllResults();


            Console.WriteLine("---------------- saturation corrected ----------------");
            parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName = "uimf_saturation_repair";
            workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.ExportData = false;
            workflow.Execute();



            distinctItems = run.ResultCollection.ResultList.GroupBy(x => x.MSFeatureID).Select(y => y.First()).ToList();


            featureData = (from n in distinctItems
                           where (Math.Abs(n.IsotopicProfile.MonoIsotopicMass - melettinMonoMass)) < tolerance &&
                                 n.IsotopicProfile.ChargeState == chargeState
                           select n).Select(r => (UIMFIsosResult)r).ToList();

            OutputFeatureIntensityData(featureData, minFrame, maxFrame, maxScan, minScan);

            monoMasses = (from n in featureData select n.IsotopicProfile.MonoIsotopicMass).ToList();

            massVariance = MathUtils.GetStDev(monoMasses);
            Console.WriteLine("Mass variance = " + massVariance);


        }


        [Test]
        public void saturatedFixingOnLeucinEncaphalinTest0()
        {
            var uimfFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\DRsample_30ms_16Apr13_0004.UIMF";

            var run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters
            {
                PeakDetectorParameters =
                {
                    PeakToBackgroundRatio = 4,
                    SignalToNoiseThreshold = 3
                },
                ThrashParameters = {MaxFit = 0.6},
                MSGeneratorParameters =
                {
                    UseLCScanRange = true,
                    MinLCScan = 11,
                    MaxLCScan = 11,
                    UseMZRange = true,
                    MinMZ = 550,
                    MaxMZ = 562,
                    SumSpectraAcrossLC = true,
                    SumSpectraAcrossIms = true,
                    NumLCScansToSum = 1,
                    NumImsScansToSum = 1
                },
                MiscMSProcessingParameters = {UseZeroFilling = true}
            };


            parameters.ThrashParameters.MinIntensityForDeletion = 10;
            parameters.MiscMSProcessingParameters.SaturationThreshold = 12000;

            parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName = "standard";


            Console.WriteLine("---------------- No saturation correction ----------------");
            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.ExportData = false;
            workflow.Execute();

            var distinctItems = run.ResultCollection.ResultList.GroupBy(x => x.MSFeatureID).Select(y => y.First()).ToList();

            var minFrame = parameters.MSGeneratorParameters.MinLCScan;
            var maxFrame = parameters.MSGeneratorParameters.MaxLCScan;

            var minScan = 130;
            var maxScan = 200;

            var melettinMonoMass = 555.2693;
            var chargeState = 1;

            var tolerance = 0.3;
            var featureData =
                (from n in distinctItems
                 where (Math.Abs(n.IsotopicProfile.MonoIsotopicMass - melettinMonoMass)) < tolerance &&
                       n.IsotopicProfile.ChargeState == chargeState
                 select n).Select(r => (UIMFIsosResult)r).ToList();

            OutputFeatureIntensityData(featureData, minFrame, maxFrame, maxScan, minScan);

            var monoMasses = (from n in featureData select n.IsotopicProfile.MonoIsotopicMass).ToList();

            var massVariance = MathUtils.GetStDev(monoMasses);
            Console.WriteLine("Mass variance = " + massVariance);


            run.ResultCollection.ClearAllResults();


            Console.WriteLine("---------------- saturation corrected ----------------");
            parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName = "uimf_saturation_repair";
            workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.ExportData = false;
            workflow.Execute();



            distinctItems = run.ResultCollection.ResultList.GroupBy(x => x.MSFeatureID).Select(y => y.First()).ToList();


            featureData = (from n in distinctItems
                           where (Math.Abs(n.IsotopicProfile.MonoIsotopicMass - melettinMonoMass)) < tolerance &&
                                 n.IsotopicProfile.ChargeState == chargeState
                           select n).Select(r => (UIMFIsosResult)r).ToList();

            OutputFeatureIntensityData(featureData, minFrame, maxFrame, maxScan, minScan);

            monoMasses = (from n in featureData select n.IsotopicProfile.MonoIsotopicMass).ToList();

            massVariance = MathUtils.GetStDev(monoMasses);
            Console.WriteLine("Mass variance = " + massVariance);


        }

        [Test]
        public void GetDataForAngioTensin1()
        {
            var uimfFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\DRsample_30ms_16Apr13_0004.UIMF";

            var run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters
            {
                PeakDetectorParameters =
                {
                    PeakToBackgroundRatio = 0.0,
                    SignalToNoiseThreshold = 0.0
                },
                ThrashParameters = {MaxFit = 0.6},
                MSGeneratorParameters =
                {
                    UseLCScanRange = true,
                    MinLCScan = 11,
                    MaxLCScan = 11,
                    UseMZRange = true,
                    MinMZ = 430,
                    MaxMZ = 440,
                    SumSpectraAcrossLC = true,
                    SumSpectraAcrossIms = true,
                    NumLCScansToSum = 1,
                    NumImsScansToSum = 7
                },
                MiscMSProcessingParameters = {UseZeroFilling = true}
            };


            parameters.ThrashParameters.MinIntensityForDeletion = 10;
            parameters.ThrashParameters.MinIntensityForScore = 10;
            parameters.MiscMSProcessingParameters.SaturationThreshold = 12000;
            parameters.ThrashParameters.MinMSFeatureToBackgroundRatio = 0.5;
            parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName = "standard";


            Console.WriteLine("---------------- No saturation correction ----------------");
            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.ExportData = false;
            workflow.Execute();

            var distinctItems = run.ResultCollection.ResultList.GroupBy(x => x.MSFeatureID).Select(y => y.First()).ToList();

            var minFrame = parameters.MSGeneratorParameters.MinLCScan;
            var maxFrame = parameters.MSGeneratorParameters.MaxLCScan;

            var minScan = 90;
            var maxScan = 150;

            var peptideMonoMass = 1295.67749;
            var chargeState = 3;

            var tolerance = 0.3;
            var featureData =
                (from n in distinctItems
                 where (Math.Abs(n.IsotopicProfile.MonoIsotopicMass - peptideMonoMass)) < tolerance &&
                       n.IsotopicProfile.ChargeState == chargeState
                 select n).Select(r => (UIMFIsosResult)r).ToList();

            OutputFeatureIntensityData(featureData, minFrame, maxFrame, maxScan, minScan);

            var monoMasses = (from n in featureData select n.IsotopicProfile.MonoIsotopicMass).ToList();

            var massVariance = MathUtils.GetStDev(monoMasses);
            Console.WriteLine("Mass variance = " + massVariance);


            run.ResultCollection.ClearAllResults();


            Console.WriteLine("---------------- saturation corrected ----------------");
            parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName = "uimf_saturation_repair";
            workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.ExportData = false;
            workflow.Execute();



            distinctItems = run.ResultCollection.ResultList.GroupBy(x => x.MSFeatureID).Select(y => y.First()).ToList();


            featureData = (from n in distinctItems
                           where (Math.Abs(n.IsotopicProfile.MonoIsotopicMass - peptideMonoMass)) < tolerance &&
                                 n.IsotopicProfile.ChargeState == chargeState
                           select n).Select(r => (UIMFIsosResult)r).ToList();

            OutputFeatureIntensityData(featureData, minFrame, maxFrame, maxScan, minScan);

            monoMasses = (from n in featureData select n.IsotopicProfile.MonoIsotopicMass).ToList();

            massVariance = MathUtils.GetStDev(monoMasses);
            Console.WriteLine("Mass variance = " + massVariance);


            TestUtilities.DisplayMSFeatures(run.ResultCollection.ResultList);

        }



        [Test]
        public void TempSaturationFixingTestOnYehiaBSAData()
        {
            var uimfFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\BSA_30N2_30ms_gate_10tof_Padjust_0000.UIMF";

            var run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters
            {
                PeakDetectorParameters =
                {
                    PeakToBackgroundRatio = 4,
                    SignalToNoiseThreshold = 3
                },
                ThrashParameters = {MaxFit = 0.6},
                MSGeneratorParameters =
                {
                    UseLCScanRange = true,
                    MinLCScan = 2,
                    MaxLCScan = 2,
                    UseMZRange = true,
                    MinMZ = 652,
                    MaxMZ = 657,
                    SumSpectraAcrossLC = true,
                    SumSpectraAcrossIms = true,
                    NumLCScansToSum = 1,
                    NumImsScansToSum = 7
                },
                MiscMSProcessingParameters = {UseZeroFilling = true}
            };


            parameters.ThrashParameters.MinIntensityForDeletion = 10;
            parameters.MiscMSProcessingParameters.SaturationThreshold = 6000;

            parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName = "uimf_saturation_repair";

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            //workflow.ExportData = false;
            workflow.Execute();
        }

        [Test]
        public void saturatedFixingTest1()
        {
            var uimfFile = @"D:\Data\UIMF\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";

            var run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters
            {
                PeakDetectorParameters =
                {
                    PeakToBackgroundRatio = 2,
                    SignalToNoiseThreshold = 3,
                    IsDataThresholded = true
                },
                ThrashParameters = {MaxFit = 0.8},
                MSGeneratorParameters =
                {
                    UseLCScanRange = true,
                    MinLCScan = 180,
                    MaxLCScan = 180,
                    SumSpectraAcrossLC = true,
                    SumSpectraAcrossIms = true,
                    NumLCScansToSum = 1,
                    NumImsScansToSum = 7,
                    UseMZRange = true,
                    MinMZ = 475,
                    MaxMZ = 476
                },
                MiscMSProcessingParameters = {UseZeroFilling = true}
            };



            parameters.ThrashParameters.MinIntensityForDeletion = 10;
            parameters.ScanBasedWorkflowParameters.IsRefittingPerformed = false;

            parameters.MiscMSProcessingParameters.SaturationThreshold = 50000;
            parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName = "uimf_saturation_repair";

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.ExportData = true;

            var sw = new Stopwatch();
            sw.Start();

            workflow.Execute();
            return;

#pragma warning disable 162
            sw.Stop();

            var distinctItems = run.ResultCollection.ResultList.GroupBy(x => x.MSFeatureID).Select(y => y.First()).ToList();

            var minFrame = parameters.MSGeneratorParameters.MinLCScan;
            var maxFrame = parameters.MSGeneratorParameters.MaxLCScan;

            var minScan = 102;
            var maxScan = 125;

            //this one elutes ScanLC at 180 - 195
            var targetMass = 860.3987;
            var chargeState = 2;

            //targetMass = 1444.748171;
            //chargeState = 3;

            //targetMass = 1079.559447;
            //chargeState = 2;

            //non-saturated feature:
            //targetMass = 1064.485;
            //chargeState = 2;

            //targetMass = 1224.5497;
            //chargeState = 1;


            //targetMass = 949.454723;
            //chargeState = 1;
            //minScan = 220;
            //maxScan = 228;

            var tolerance = 0.3;
            var featureData =
                (from n in distinctItems
                 where (Math.Abs(n.IsotopicProfile.MonoIsotopicMass - targetMass)) < tolerance &&
                       n.IsotopicProfile.ChargeState == chargeState
                 select n).Select(r => (UIMFIsosResult)r).ToList();

            OutputFeatureIntensityData(featureData, minFrame, maxFrame, maxScan, minScan);

            var monoMasses = (from n in featureData select n.IsotopicProfile.MonoIsotopicMass).ToList();

            var massVariance = MathUtils.GetStDev(monoMasses);
            Console.WriteLine("Mass variance = " + massVariance);
            Console.WriteLine("Time taken = " + sw.ElapsedMilliseconds);
#pragma warning restore 162

        }

        [Test]
        public void saturatedFixing_TempTest1()
        {
            var uimfFile = @"D:\Data\UIMF\QC_Shew_13_04_Run-06_12Jul13_Methow_13-05-25.uimf";

            var run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters
            {
                PeakDetectorParameters =
                {
                    PeakToBackgroundRatio = 2,
                    SignalToNoiseThreshold = 3,
                    IsDataThresholded = true
                },
                ThrashParameters = {MaxFit = 0.8},
                MSGeneratorParameters =
                {
                    UseLCScanRange = true,
                    MinLCScan = 784,
                    MaxLCScan = 794,
                    SumSpectraAcrossLC = true,
                    SumSpectraAcrossIms = true,
                    NumLCScansToSum = 1,
                    NumImsScansToSum = 7,
                    UseMZRange = false,
                    MinMZ = 525.2,
                    MaxMZ = 525.4
                },
                MiscMSProcessingParameters = {UseZeroFilling = true}
            };

            parameters.ThrashParameters.MinIntensityForDeletion = 10;
            parameters.ScanBasedWorkflowParameters.IsRefittingPerformed = false;

            parameters.MiscMSProcessingParameters.SaturationThreshold = 10000;
            parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName = "uimf_saturation_repair";

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.ExportData = true;

            var sw = new Stopwatch();
            sw.Start();

            workflow.Execute();
            return;

#pragma warning disable 162
            sw.Stop();

            var distinctItems = run.ResultCollection.ResultList.GroupBy(x => x.MSFeatureID).Select(y => y.First()).ToList();

            var minFrame = parameters.MSGeneratorParameters.MinLCScan;
            var maxFrame = parameters.MSGeneratorParameters.MaxLCScan;

            var minScan = 102;
            var maxScan = 125;

            //this one elutes ScanLC at 180 - 195
            var targetMass = 860.3987;
            var chargeState = 2;

            //targetMass = 1444.748171;
            //chargeState = 3;

            //targetMass = 1079.559447;
            //chargeState = 2;

            //non-saturated feature:
            //targetMass = 1064.485;
            //chargeState = 2;

            //targetMass = 1224.5497;
            //chargeState = 1;


            //targetMass = 949.454723;
            //chargeState = 1;
            //minScan = 220;
            //maxScan = 228;

            var tolerance = 0.3;
            var featureData =
                (from n in distinctItems
                 where (Math.Abs(n.IsotopicProfile.MonoIsotopicMass - targetMass)) < tolerance &&
                       n.IsotopicProfile.ChargeState == chargeState
                 select n).Select(r => (UIMFIsosResult)r).ToList();

            OutputFeatureIntensityData(featureData, minFrame, maxFrame, maxScan, minScan);

            var monoMasses = (from n in featureData select n.IsotopicProfile.MonoIsotopicMass).ToList();

            var massVariance = MathUtils.GetStDev(monoMasses);
            Console.WriteLine("Mass variance = " + massVariance);
            Console.WriteLine("Time taken = " + sw.ElapsedMilliseconds);
#pragma warning restore 162

        }



        [Test]
        public void saturatedFixingTest2()
        {
            var uimfFile = @"D:\Data\UIMF\Sarc_Main_Study_Controls\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";

            var run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters();

            parameters.LoadFromOldDeconToolsParameterFile(@"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\IMS_UIMF_PeakBR2_PeptideBR3_SN3_SumScans3_NoLCSum_Sat50000_2012-02-27_frames_180_195.xml");

            parameters.MSGeneratorParameters.UseLCScanRange = true;
            parameters.MSGeneratorParameters.MinLCScan = 375;
            parameters.MSGeneratorParameters.MaxLCScan = 420;

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.ExportData = true;

            var sw = new Stopwatch();
            sw.Start();

            workflow.Execute();
            //return;

            sw.Stop();

            var distinctItems = run.ResultCollection.ResultList.GroupBy(x => x.MSFeatureID).Select(y => y.First()).ToList();

            var minFrame = parameters.MSGeneratorParameters.MinLCScan;
            var maxFrame = parameters.MSGeneratorParameters.MaxLCScan;

            var minScan = 102;
            var maxScan = 125;

            var targetMass = 819.48169;    //elutes at 199 - 205
            var chargeState = 2;



            var tolerance = 0.3;
            var featureData =
                (from n in distinctItems
                 where (Math.Abs(n.IsotopicProfile.MonoIsotopicMass - targetMass)) < tolerance &&
                       n.IsotopicProfile.ChargeState == chargeState
                 select n).Select(r => (UIMFIsosResult)r).ToList();

            OutputFeatureIntensityData(featureData, minFrame, maxFrame, maxScan, minScan);

            var monoMasses = (from n in featureData select n.IsotopicProfile.MonoIsotopicMass).ToList();

            var massVariance = MathUtils.GetStDev(monoMasses);
            Console.WriteLine("Mass variance = " + massVariance);
            Console.WriteLine("Time taken = " + sw.ElapsedMilliseconds);

        }


        [Test]
        public void saturatedFixingTest_peaksNotExporting()
        {
            var uimfFile = @"D:\Data\UIMF\Sarc_Main_Study_Controls\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";

            var run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters();

            parameters.LoadFromOldDeconToolsParameterFile(@"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\IMS_UIMF_PeakBR2_PeptideBR3_SN3_SumScans3_NoLCSum_Sat50000_2012-02-27_frames_180_195.xml");

            parameters.MSGeneratorParameters.UseLCScanRange = true;
            parameters.MSGeneratorParameters.MinLCScan = 375;
            parameters.MSGeneratorParameters.MaxLCScan = 378;
            parameters.ScanBasedWorkflowParameters.ExportPeakData = true;

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.ExportData = true;

            var sw = new Stopwatch();
            sw.Start();

            workflow.Execute();
            //return;

            sw.Stop();

            var distinctItems = run.ResultCollection.ResultList.GroupBy(x => x.MSFeatureID).Select(y => y.First()).ToList();

            var minFrame = parameters.MSGeneratorParameters.MinLCScan;
            var maxFrame = parameters.MSGeneratorParameters.MaxLCScan;

            var minScan = 102;
            var maxScan = 125;

            var targetMass = 819.48169;    //elutes at 199 - 205
            var chargeState = 2;



            var tolerance = 0.3;
            var featureData =
                (from n in distinctItems
                 where (Math.Abs(n.IsotopicProfile.MonoIsotopicMass - targetMass)) < tolerance &&
                       n.IsotopicProfile.ChargeState == chargeState
                 select n).Select(r => (UIMFIsosResult)r).ToList();

            OutputFeatureIntensityData(featureData, minFrame, maxFrame, maxScan, minScan);

            var monoMasses = (from n in featureData select n.IsotopicProfile.MonoIsotopicMass).ToList();

            var massVariance = MathUtils.GetStDev(monoMasses);
            Console.WriteLine("Mass variance = " + massVariance);
            Console.WriteLine("Time taken = " + sw.ElapsedMilliseconds);

        }

        [Test]
        public void saturatedFixingTest3()
        {
            var uimfFile = @"D:\Data\UIMF\Sarc_Main_Study_Controls\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";

            var run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters();

            parameters.LoadFromOldDeconToolsParameterFile(
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\IMS_UIMF_PeakBR2_PeptideBR3_SN3_SumScans3_NoLCSum_Sat50000_2012-02-27_frames_180_195.xml");

            parameters.MSGeneratorParameters.UseLCScanRange = true;
            parameters.MSGeneratorParameters.MinLCScan = 394;
            parameters.MSGeneratorParameters.MaxLCScan = 404;


            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.ExportData = false;

            var sw = new Stopwatch();
            sw.Start();

            workflow.Execute();
            //return;

            sw.Stop();

            var distinctItems = run.ResultCollection.ResultList.GroupBy(x => x.MSFeatureID).Select(y => y.First()).ToList();

            var minFrame = parameters.MSGeneratorParameters.MinLCScan;
            var maxFrame = parameters.MSGeneratorParameters.MaxLCScan;

            var minScan = 102;
            var maxScan = 127;

            var targetMass = 1059.55169;
            var chargeState = 2;



            var tolerance = 0.3;
            var featureData =
                (from n in distinctItems
                 where (Math.Abs(n.IsotopicProfile.MonoIsotopicMass - targetMass)) < tolerance &&
                       n.IsotopicProfile.ChargeState == chargeState
                 select n).Select(r => (UIMFIsosResult)r).ToList();

            OutputFeatureIntensityData(featureData, minFrame, maxFrame, maxScan, minScan);

            var monoMasses = (from n in featureData select n.IsotopicProfile.MonoIsotopicMass).ToList();

            var massVariance = MathUtils.GetStDev(monoMasses);
            Console.WriteLine("Mass variance = " + massVariance);
            Console.WriteLine("Time taken = " + sw.ElapsedMilliseconds);

        }


        [Test]
        public void saturatedFixing_RedmineIssue966()
        {
            var uimfFile = @"D:\Data\UIMF\Sarc_Main_Study_Controls\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";

            var run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters();

            var parameterFile =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\IMS_UIMF_PeakBR4_PeptideBR4_SN3_SumScans3_NoLCSum_saturationRepair_Frame_1-500.xml";

            parameters.LoadFromOldDeconToolsParameterFile(parameterFile);

            parameters = new DeconToolsParameters
            {
                PeakDetectorParameters =
                {
                    PeakToBackgroundRatio = 4,
                    SignalToNoiseThreshold = 3
                },
                ThrashParameters = {MaxFit = 0.6},
                MSGeneratorParameters =
                {
                    UseLCScanRange = true,
                    MinLCScan = 320,
                    MaxLCScan = 328,
                    SumSpectraAcrossLC = true,
                    SumSpectraAcrossIms = true,
                    NumLCScansToSum = 1,
                    NumImsScansToSum = 3,
                    UseMZRange = false
                },
                MiscMSProcessingParameters = {UseZeroFilling = true}
            };
            parameters.ThrashParameters.MinIntensityForDeletion = 10;

            parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName = "uimf_saturation_repair";
            parameters.Save(@"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\IMS_UIMF_PeakBR4_PeptideBR4_SN3_SumScans3_NoLCSum_saturationRepair_Frame_1-500_copy.xml");

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            workflow.ExportData = true;

            var sw = new Stopwatch();
            sw.Start();

            workflow.Execute();
            //return;

            sw.Stop();

            var distinctItems = run.ResultCollection.ResultList.GroupBy(x => x.MSFeatureID).Select(y => y.First()).ToList();

            var minFrame = parameters.MSGeneratorParameters.MinLCScan;
            var maxFrame = parameters.MSGeneratorParameters.MaxLCScan;

            var minScan = 117;
            var maxScan = 131;

            var chargeState = 2;
            var targetMass = (605.82 - 1.00727649) * chargeState;

            var tolerance = 0.3;
            var featureData =
                (from n in distinctItems
                 where (Math.Abs(n.IsotopicProfile.MonoIsotopicMass - targetMass)) < tolerance &&
                       n.IsotopicProfile.ChargeState == chargeState
                 select n).Select(r => (UIMFIsosResult)r).ToList();

            OutputFeatureIntensityData(featureData, minFrame, maxFrame, maxScan, minScan);

            var monoMasses = (from n in featureData select n.IsotopicProfile.MonoIsotopicMass).ToList();

            var massVariance = MathUtils.GetStDev(monoMasses);
            Console.WriteLine("Mass variance = " + massVariance);
            Console.WriteLine("Time taken = " + sw.ElapsedMilliseconds);

        }




        [Test]
        public void noFixingTest1()
        {
            var uimfFile = @"D:\Data\UIMF\Sarc_Main_Study_Controls\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";

            var run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters
            {
                PeakDetectorParameters =
                {
                    PeakToBackgroundRatio = 4,
                    SignalToNoiseThreshold = 3
                },
                MSGeneratorParameters =
                {
                    UseLCScanRange = true,
                    MinLCScan = 180,
                    MaxLCScan = 180,
                    SumSpectraAcrossLC = true,
                    SumSpectraAcrossIms = true,
                    NumLCScansToSum = 1,
                    NumImsScansToSum = 3
                }
            };




            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);

            workflow.Execute();

        }


        [Test]
        public void WorkflowTypeIsCorrectTest1()
        {
            var uimfFile = @"D:\Data\UIMF\Sarc_Main_Study_Controls\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";

            var run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters {
                ScanBasedWorkflowParameters = {
                    ScanBasedWorkflowName = "standard"
                }
            };

            var workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            Assert.IsTrue(workflow is IMSScanBasedWorkflow);

            parameters.ScanBasedWorkflowParameters.ScanBasedWorkflowName = "uimf_saturation_repair";
            workflow = ScanBasedWorkflow.CreateWorkflow(run, parameters);
            Assert.IsTrue(workflow is SaturationIMSScanBasedWorkflow);



        }


        [Test]
        public void WorkflowTypeIsWrongTest1()
        {
            var uimfFile = @"D:\Data\UIMF\Sarc_Main_Study_Controls\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";

            var run = new RunFactory().CreateRun(uimfFile);
            var parameters = new DeconToolsParameters {
                ScanBasedWorkflowParameters = {
                    ScanBasedWorkflowName = "incorrectTextProblem"
                }
            };

            try
            {
                ScanBasedWorkflow.CreateWorkflow(run, parameters);
            }
            catch (ArgumentOutOfRangeException)
            {
                Assert.Pass("ArgumentOutOfRangeException caught; this is expected");
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception caught of type {0}: {1}", ex.GetType(), ex.Message);
            }

            Assert.Fail("Exception was not caught; we expected an ArgumentOutOfRangeException to be thrown");
        }

        private static void OutputFeatureIntensityData(IReadOnlyCollection<UIMFIsosResult> featureData, int minFrame, int maxFrame, int maxScan, int minScan)
        {




            var sb = new StringBuilder();
            for (var scan = minScan; scan <= maxScan; scan++)
            {



                for (var frame = minFrame; frame <= maxFrame; frame++)
                {

                    if (frame == minFrame)
                    {
                        sb.Append(scan);
                        sb.Append("\t");
                    }



                    var feature =
                        (from n in featureData
                         where n.IMSScanSet.PrimaryScanNumber == scan && n.ScanSet.PrimaryScanNumber == frame
                         select n).FirstOrDefault();

                    double intensity;
                    if (feature == null)
                    {
                        intensity = 0;
                    }
                    else
                    {
                        intensity = feature.IntensityAggregate;
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
