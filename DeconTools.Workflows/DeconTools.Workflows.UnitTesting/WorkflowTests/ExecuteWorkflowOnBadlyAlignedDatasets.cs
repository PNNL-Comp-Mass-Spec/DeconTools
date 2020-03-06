using System;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class ExecuteWorkflowOnBadlyAlignedDatasets
    {
        [Test]
        public void findFeatureIn_dataset_with_bad_massCalibration_test1()
        {
            //mass error in this dataset is typically ~50ppm.

            var datasetFile = @"D:\Data\Orbitrap\Subissue01\QC_Shew_10_01-pt5-1_8Feb10_Doc_09-12-24.RAW";

            var massTagFile = @"\\protoapps\UserData\Slysz\Data\QCShew_MassiveTargeted\MassTags\QCShew_Formic_MassTags_Bin10_first10.txt";
            var workflowParameterFile = @"\\protoapps\UserData\Slysz\Data\QCShew_MassiveTargeted\WorkflowParameterFiles\UnlabeledTargeted_WorkflowParameters_noSum.xml";


            var run = RunUtilities.CreateAndLoadPeaks(datasetFile, datasetFile.Replace(".RAW", "_peaks.txt"));
            RunUtilities.AlignRunUsingAlignmentInfoInFiles(run);

            Assert.IsTrue(run.MassIsAligned);
            Assert.IsTrue(run.NETIsAligned);


            var parameters=new BasicTargetedWorkflowParameters();
            parameters.LoadParameters(workflowParameterFile);
            var workflow = new BasicTargetedWorkflow(parameters);


            workflow.Run = run;

            var mtc = new TargetCollection();
            var mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            var mt1 = (from n in mtc.TargetList where n.ID == 24702 && n.ChargeState == 4 select n).First();

            run.CurrentMassTag = mt1;

            workflow.Execute();

            var repo = new TargetedResultRepository();
            repo.AddResult(workflow.Result);

           
            Console.WriteLine("theor mono mass = " + mt1.MonoIsotopicMass);
            Console.WriteLine("theorMZ = " + mt1.MZ);
            Console.WriteLine("theorNET = " + mt1.NormalizedElutionTime);
            Console.WriteLine("obsScan = " + repo.Results[0].ScanLC);
            Console.WriteLine("obsNET = " + repo.Results[0].NET);
            Console.WriteLine("NETError = " + repo.Results[0].NETError);
            Console.WriteLine("obsMZ = " + repo.Results[0].MonoMZ);
            Console.WriteLine("monoMass = " + repo.Results[0].MonoMass);
            Console.WriteLine("obsMonoMassCalibrated = "+repo.Results[0].MonoMassCalibrated);

            Console.WriteLine("MassErrorBeforeAlignment = " + (mt1.MZ - repo.Results[0].MonoMZ) / mt1.MZ * 1e6);
            Console.WriteLine("MassErrorAfterAlignment = " + repo.Results[0].MassErrorBeforeCalibration);


        }


        [Test]
        public void check_calibrated_masses_test1()
        {
            //mass error in this dataset is typically ~50ppm.

            var datasetFile = @"D:\Data\Orbitrap\Issue0725_badAlignment\QC_Shew_10_03-2_100min_06May10_Tiger_10-04-08.RAW";

            var massTagFile = @"\\protoapps\UserData\Slysz\Data\QCShew_MassiveTargeted\MassTags\QCShew_Formic_MassTags_Bin10_first10.txt";
            var workflowParameterFile = @"\\protoapps\UserData\Slysz\Data\QCShew_MassiveTargeted\WorkflowParameterFiles\UnlabeledTargeted_WorkflowParameters_noSum.xml";

            var run = RunUtilities.CreateAndLoadPeaks(datasetFile, datasetFile.Replace(".RAW", "_peaks.txt"));
            RunUtilities.AlignRunUsingAlignmentInfoInFiles(run);

            Assert.IsTrue(run.MassIsAligned);
            Assert.IsTrue(run.NETIsAligned);

            var parameters = new BasicTargetedWorkflowParameters();
            parameters.LoadParameters(workflowParameterFile);
            var workflow = new BasicTargetedWorkflow(parameters);

            workflow.Run = run;

            var mtc = new TargetCollection();
            var mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            var mt1 = (from n in mtc.TargetList where n.ID == 24702 && n.ChargeState == 4 select n).First();

            run.CurrentMassTag = mt1;

            workflow.Execute();

            var repo = new TargetedResultRepository();
            repo.AddResult(workflow.Result);

            Console.WriteLine("theor mono mass = " + mt1.MonoIsotopicMass);
            Console.WriteLine("theorMZ = " + mt1.MZ);
            Console.WriteLine("theorNET = " + mt1.NormalizedElutionTime);
            Console.WriteLine("obsScan = " + repo.Results[0].ScanLC);
            Console.WriteLine("obsNET = " + repo.Results[0].NET);
            Console.WriteLine("NETError = " + repo.Results[0].NETError);
            Console.WriteLine("obsMZ = " + repo.Results[0].MonoMZ);
            Console.WriteLine("monoMass = " + repo.Results[0].MonoMass);
            Console.WriteLine("obsMonoMassCalibrated = " + repo.Results[0].MonoMassCalibrated);

            Console.WriteLine("MassErrorBeforeAlignment = " + (mt1.MZ - repo.Results[0].MonoMZ) / mt1.MZ * 1e6);
            Console.WriteLine("MassErrorAfterAlignment = " + repo.Results[0].MassErrorBeforeCalibration);
        }

    }
}
