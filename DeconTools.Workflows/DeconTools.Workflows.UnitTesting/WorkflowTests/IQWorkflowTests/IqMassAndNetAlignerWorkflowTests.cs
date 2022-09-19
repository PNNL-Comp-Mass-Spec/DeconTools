using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.UnitTesting2;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests.IQWorkflowTests
{
    [TestFixture]
    public class IqMassAndNetAlignerWorkflowTests
    {
        [Test]
        public void AlignUsingMsgfOutputFullProcessingTest1()
        {
            var targetsFileName =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\AlignmentInfo\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_msgfdb_fht.txt";

            var massTagFilename =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Targets\QCShew_ProdTest_Formic_P823_PMT3.txt";

            var rawFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

            WorkflowExecutorBaseParameters parameters = new BasicTargetedWorkflowExecutorParameters();
            parameters.TargetsFilePath = targetsFileName;
            parameters.ReferenceTargetsFilePath = massTagFilename;

            var run = new RunFactory().CreateRun(rawFile);

            var massAndNetAligner = new IqMassAndNetAligner(parameters, run);
            massAndNetAligner.LoadAndInitializeTargets();

            massAndNetAligner.Targets = massAndNetAligner.Targets.Take(2000).ToList();

            var exportedAlignmentResultsFile =
             @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\AlignmentInfo\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_iqAlignmentResults.txt";

            if (!File.Exists(exportedAlignmentResultsFile)) File.Delete(exportedAlignmentResultsFile);

            IqTargetImporter massTagImporter = new BasicIqTargetImporter(massTagFilename);
            var massTagRefs = massTagImporter.Import();

            massAndNetAligner.SetMassTagReferences(massTagRefs);

            //massAligner.Targets = massAligner.Targets.Take(1).ToList();

            massAndNetAligner.ExecuteAlignment();

            var sb = new StringBuilder();

            var usefulResults = new List<IqResult>();

            massAndNetAligner.ExportResults(exportedAlignmentResultsFile);

            foreach (var iqResult in massAndNetAligner.Results)
            {
                var childresults = iqResult.ChildResults();
                foreach (var childresult in childresults)
                {
                    if (childresult.ObservedIsotopicProfile == null) continue;

                    sb.Append(childresult.Target.ID);
                    sb.Append("\t");

                    if (childresult.ObservedIsotopicProfile == null)
                    {
                        sb.Append("[null]");
                        sb.Append(Environment.NewLine);
                    }
                    else
                    {
                        usefulResults.Add(childresult);

                        sb.Append(childresult.ChromPeakSelected.XValue);
                        sb.Append("\t");
                        sb.Append(childresult.ElutionTimeObs);
                        sb.Append("\t");
                        sb.Append(childresult.ParentResult.Target.ElutionTimeTheor);
                        sb.Append("\t");
                        sb.Append(childresult.MZObs.ToString("0.00000"));
                        sb.Append("\t");
                        sb.Append(childresult.MonoMassObs.ToString("0.00000"));
                        sb.Append("\t");
                        sb.Append(childresult.MassErrorBefore.ToString("0.00000"));
                        sb.Append("\t");
                        sb.Append(childresult.FitScore.ToString("0.000"));

                        sb.Append(Environment.NewLine);
                    }
                }
            }

            var averageMassError = usefulResults.Average(p => p.MassErrorBefore);

            var stdev = MathUtils.GetStDev(usefulResults.Select(p => p.MassErrorBefore).ToList());

            Console.WriteLine(sb.ToString());

            Console.WriteLine("avg mass error= " + averageMassError);
            Console.WriteLine("stdev= " + stdev);

            Console.WriteLine("----------- mass alignment ------------");
            TestUtilities.DisplayXYValues(massAndNetAligner.MassAlignmentInfo.ScanAndPpmShiftVals);

            Console.WriteLine("------------NET alignment --------------");
            DisplayNetAlignmentInfo(massAndNetAligner.NetAlignmentInfo);
        }

        [Test]
        public void AlignUsingMsgfOutputFullProcessingTest2()
        {
            var targetsFileName =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\AlignmentInfo\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_msgfdb_fht.txt";

            var massTagFilename =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Targets\QCShew_ProdTest_Formic_P823_PMT3.txt";

            var rawFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

            WorkflowExecutorBaseParameters parameters = new BasicTargetedWorkflowExecutorParameters();
            parameters.TargetsFilePath = targetsFileName;
            parameters.ReferenceTargetsFilePath = massTagFilename;

            var run = new RunFactory().CreateRun(rawFile);

            var massAndNetAligner = new IqMassAndNetAligner(parameters, run);
            massAndNetAligner.LoadAndInitializeTargets();

            massAndNetAligner.Targets = massAndNetAligner.Targets.Where(p => p.Code.Contains("FEQDGENYTGTIDGNMGAYAR")).ToList();

            IqTargetImporter massTagImporter = new BasicIqTargetImporter(massTagFilename);
            var massTagRefs = massTagImporter.Import();

            var exportedAlignmentResultsFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\AlignmentInfo\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_iqAlignmentResults.txt";

            if (!File.Exists(exportedAlignmentResultsFile)) File.Delete(exportedAlignmentResultsFile);

            massAndNetAligner.SetMassTagReferences(massTagRefs);

            //massAligner.Targets = massAligner.Targets.Take(1).ToList();

            massAndNetAligner.ExecuteAlignment();

            var sb = new StringBuilder();

            var usefulResults = new List<IqResult>();

            massAndNetAligner.ExportResults(exportedAlignmentResultsFile);

            foreach (var iqResult in massAndNetAligner.Results)
            {
                var childresults = iqResult.ChildResults();
                foreach (var childresult in childresults)
                {
                    if (childresult.ObservedIsotopicProfile == null) continue;

                    sb.Append(childresult.Target.ID);
                    sb.Append("\t");

                    if (childresult.ObservedIsotopicProfile == null)
                    {
                        sb.Append("[null]");
                        sb.Append(Environment.NewLine);
                    }
                    else
                    {
                        usefulResults.Add(childresult);

                        sb.Append(childresult.ChromPeakSelected.XValue);
                        sb.Append("\t");
                        sb.Append(childresult.ElutionTimeObs);
                        sb.Append("\t");
                        sb.Append(childresult.ParentResult.Target.ElutionTimeTheor);
                        sb.Append("\t");
                        sb.Append(childresult.MZObs.ToString("0.00000"));
                        sb.Append("\t");
                        sb.Append(childresult.MonoMassObs.ToString("0.00000"));
                        sb.Append("\t");
                        sb.Append(childresult.MassErrorBefore.ToString("0.00000"));
                        sb.Append("\t");
                        sb.Append(childresult.FitScore.ToString("0.000"));

                        sb.Append(Environment.NewLine);
                    }
                }
            }

            var averageMassError = usefulResults.Average(p => p.MassErrorBefore);

            var stdev = MathUtils.GetStDev(usefulResults.Select(p => p.MassErrorBefore).ToList());

            Console.WriteLine(sb.ToString());

            Console.WriteLine("avg mass error= " + averageMassError);
            Console.WriteLine("stdev= " + stdev);

            Console.WriteLine("----------- mass alignment ------------");
            TestUtilities.DisplayXYValues(massAndNetAligner.MassAlignmentInfo.ScanAndPpmShiftVals);

            Console.WriteLine("------------NET alignment --------------");
            DisplayNetAlignmentInfo(massAndNetAligner.NetAlignmentInfo);
        }

        [Test]
        public void AlignUsingPreviouslyProcessedOutputTest1()
        {
            var rawFile =
               @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

            var previouslyProcessedResultsFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\AlignmentInfo\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_iqAlignmentResults.txt";

            WorkflowExecutorBaseParameters parameters = new BasicTargetedWorkflowExecutorParameters();
            var run = new RunFactory().CreateRun(rawFile);

            var massAndNetAligner = new IqMassAndNetAligner(parameters, run);
            massAndNetAligner.LoadPreviousIqResults(previouslyProcessedResultsFile);

            massAndNetAligner.ExecuteAlignment();

            var baseFilenameForImageExport =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\AlignmentInfo\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18";
            massAndNetAligner.ExportGraphs(baseFilenameForImageExport);
        }

        [Test]
        public void Test1()
        {
            var targetsFileName =
                @"\\proto-7\VOrbi05\2013_2\mhp_plat_test_1_14April13_Frodo_12-12-04\MSG201305011339_Auto939903\mhp_plat_test_1_14April13_Frodo_12-12-04_msgfdb_fht.txt";

            var massTagFilename =
                @"\\protoapps\DataPkgs\Public\2013\795_Iq_analysis_of_mouse_O16O18\Targets\MT_Mouse_MHP_O18_Set1_P890_targets.txt";

            var rawFile = @"D:\Data\O16O18\Vlad_Mouse\mhp_plat_test_1_14April13_Frodo_12-12-04.raw";

            WorkflowExecutorBaseParameters parameters = new BasicTargetedWorkflowExecutorParameters();
            parameters.TargetsFilePath = targetsFileName;

            var run = new RunFactory().CreateRun(rawFile);

            var massAndNetAligner = new IqMassAndNetAligner(parameters, run);

            massAndNetAligner.LoadAndInitializeTargets();

            IqTargetImporter massTagImporter = new BasicIqTargetImporter(massTagFilename);
            var massTagRefs = massTagImporter.Import();

            massAndNetAligner.SetMassTagReferences(massTagRefs);

            //massAligner.Targets = massAligner.Targets.Take(1).ToList();

            massAndNetAligner.ExecuteAlignment();

            var sb = new StringBuilder();

            var usefulResults = new List<IqResult>();

            foreach (var iqResult in massAndNetAligner.Results)
            {
                var childresults=  iqResult.ChildResults();
                foreach (var childresult in childresults)
                {
                    if (childresult.ObservedIsotopicProfile == null) continue;

                    sb.Append(childresult.Target.ID);
                    sb.Append("\t");

                    if (childresult.ObservedIsotopicProfile == null)
                    {
                        sb.Append("[null]");
                        sb.Append(Environment.NewLine);
                    }
                    else
                    {
                        usefulResults.Add(childresult);

                        sb.Append(childresult.ChromPeakSelected.XValue);
                        sb.Append("\t");
                        sb.Append(childresult.ElutionTimeObs);
                        sb.Append("\t");
                        sb.Append(childresult.ParentResult.Target.ElutionTimeTheor);
                        sb.Append("\t");
                        sb.Append(childresult.MZObs.ToString("0.00000"));
                        sb.Append("\t");
                        sb.Append(childresult.MonoMassObs.ToString("0.00000"));
                        sb.Append("\t");
                        sb.Append(childresult.MassErrorBefore.ToString("0.00000"));
                        sb.Append("\t");
                        sb.Append(childresult.FitScore.ToString("0.000"));

                        sb.Append(Environment.NewLine);
                    }
                }
            }

            var averageMassError = usefulResults.Average(p => p.MassErrorBefore);

            var stdev = MathUtils.GetStDev(usefulResults.Select(p => p.MassErrorBefore).ToList());

            //Console.WriteLine(sb.ToString());

            Console.WriteLine("avg mass error= " + averageMassError);
            Console.WriteLine("stdev= " + stdev);

            Console.WriteLine("----------- mass alignment ------------");
            TestUtilities.DisplayXYValues(massAndNetAligner.MassAlignmentInfo.ScanAndPpmShiftVals);

            Console.WriteLine("------------NET alignment --------------");

            DisplayNetAlignmentInfo(massAndNetAligner.NetAlignmentInfo);
        }

        private static void DisplayNetAlignmentInfo(NetAlignmentInfo netAlignmentInfo)
        {
            var stringBuilder = new StringBuilder();
            foreach (var pair in netAlignmentInfo.ScanToNETAlignmentData)
            {
                stringBuilder.Append(pair.Key).Append('\t').Append(pair.Value).Append(Environment.NewLine);
            }

            Console.WriteLine(stringBuilder.ToString());
        }

        [Test]
        public void JoinTargetsTest1()
        {
            var targetsFileName =
           @"\\proto-7\VOrbi05\2013_2\mhp_plat_test_1_14April13_Frodo_12-12-04\MSG201305011339_Auto939903\mhp_plat_test_1_14April13_Frodo_12-12-04_msgfdb_fht.txt";

            var massTagFilename =
                @"\\protoapps\DataPkgs\Public\2013\795_Iq_analysis_of_mouse_O16O18\Targets\MT_Mouse_MHP_O18_Set1_P890_targets.txt";

            var rawFile = @"D:\Data\O16O18\Vlad_Mouse\mhp_plat_test_1_14April13_Frodo_12-12-04.raw";

            IqTargetImporter massTagImporter = new BasicIqTargetImporter(massTagFilename);
            var massTagRefs = massTagImporter.Import().OrderBy(p=>p.Code).ToList();

            WorkflowExecutorBaseParameters parameters = new BasicTargetedWorkflowExecutorParameters();
            parameters.TargetsFilePath = targetsFileName;

            var run = new RunFactory().CreateRun(rawFile);

            var massAndNetAligner = new IqMassAndNetAligner(parameters, run);
            massAndNetAligner.LoadAndInitializeTargets();

            var testNET = massAndNetAligner.Targets.First().ElutionTimeTheor;

            var query = (from massTag in massTagRefs
                         join target in massAndNetAligner.Targets on massTag.Code equals target.Code
                         select new
                                    {
                                        MassTag = massTag,
                                        MSGFTarget = target
                                    }).ToList();

            foreach (var thing in query)
            {
                thing.MSGFTarget.ID = thing.MassTag.ID;
                thing.MSGFTarget.ElutionTimeTheor = thing.MassTag.ElutionTimeTheor;
            }

            var targets = query.Select(p => p.MSGFTarget).ToList();

          var testNETAfter = massAndNetAligner.Targets.First().ElutionTimeTheor;
            Console.WriteLine("NET before= " + testNET);
            Console.WriteLine("NET before= " + testNETAfter);
        }

        [Test]
        public void TempTest1()
        {
            var code = "FEQDGENYTGTIDGNMGAYAR";

            var peptideUtils = new PeptideUtils();

            var empiricalFormula= peptideUtils.GetEmpiricalFormulaForPeptideSequence(code);

            var monomass = peptideUtils.GetMonoIsotopicMassForPeptideSequence(code);

            var mztheo = monomass/2 + DeconTools.Backend.Globals.PROTON_MASS;

            Console.WriteLine(monomass + "\t" + mztheo);

            var formula = EmpiricalFormulaUtilities.ParseEmpiricalFormulaString("H(3) C(2) N O");

            var revisedFormula = EmpiricalFormulaUtilities.GetEmpiricalFormulaFromElementTable(formula);

            var iodoMass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(revisedFormula);

            Console.WriteLine("iodomass= " + iodoMass);
        }
    }
}
