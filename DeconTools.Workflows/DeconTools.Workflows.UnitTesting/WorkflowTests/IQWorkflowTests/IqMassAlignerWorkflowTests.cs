using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests.IQWorkflowTests
{
    [TestFixture]
    public class IqMassAlignerWorkflowTests
    {
        [Test]
        public void Test1()
        {
            string targetsFileName =
                @"\\proto-7\VOrbi05\2013_2\mhp_plat_test_1_14April13_Frodo_12-12-04\MSG201305011339_Auto939903\mhp_plat_test_1_14April13_Frodo_12-12-04_msgfdb_fht.txt";


            string massTagFilename =
                @"\\protoapps\DataPkgs\Public\2013\795_Iq_analysis_of_mouse_O16O18\Targets\MT_Mouse_MHP_O18_Set1_P890_targets.txt";

            string rawFile = @"D:\Data\O16O18\Vlad_Mouse\mhp_plat_test_1_14April13_Frodo_12-12-04.raw";

            WorkflowExecutorBaseParameters parameters = new BasicTargetedWorkflowExecutorParameters();
            parameters.TargetsFilePath = targetsFileName;

            Run run = new RunFactory().CreateRun(rawFile);

            IqMassAlignerWorkflow massAligner = new IqMassAlignerWorkflow(parameters, run);


            massAligner.LoadAndInitializeTargets();

            IqTargetImporter massTagImporter = new BasicIqTargetImporter(massTagFilename);
            var massTagRefs = massTagImporter.Import();

            massAligner.SetMassTagReferences(massTagRefs);

            //massAligner.Targets = massAligner.Targets.Take(1).ToList();

            massAligner.ExecuteMassAlignerWorkflow();

            StringBuilder sb = new StringBuilder();

            List<IqResult> usefulResults = new List<IqResult>();

            foreach (var iqResult in massAligner.Results)
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
                        sb.Append(childresult.MassError.ToString("0.00000"));
                        sb.Append("\t");
                        sb.Append(childresult.FitScore.ToString("0.000"));

                        sb.Append(Environment.NewLine);
                    }
                }

                
            }


            double averageMassError = usefulResults.Average(p => p.MassError);

            double stdev = MathUtils.GetStDev(usefulResults.Select(p => p.MassError).ToList());
            
            Console.WriteLine(sb.ToString());

            Console.WriteLine("avg mass error= " + averageMassError);
            Console.WriteLine("stdev= " + stdev);


        }


        [Test]
        public void JoinTargetsTest1()
        {
            string targetsFileName =
           @"\\proto-7\VOrbi05\2013_2\mhp_plat_test_1_14April13_Frodo_12-12-04\MSG201305011339_Auto939903\mhp_plat_test_1_14April13_Frodo_12-12-04_msgfdb_fht.txt";

            string massTagFilename =
                @"\\protoapps\DataPkgs\Public\2013\795_Iq_analysis_of_mouse_O16O18\Targets\MT_Mouse_MHP_O18_Set1_P890_targets.txt";

            string rawFile = @"D:\Data\O16O18\Vlad_Mouse\mhp_plat_test_1_14April13_Frodo_12-12-04.raw";



            IqTargetImporter massTagImporter = new BasicIqTargetImporter(massTagFilename);
            var massTagRefs = massTagImporter.Import().OrderBy(p=>p.Code).ToList();

            WorkflowExecutorBaseParameters parameters = new BasicTargetedWorkflowExecutorParameters();
            parameters.TargetsFilePath = targetsFileName;

            Run run = new RunFactory().CreateRun(rawFile);

            IqMassAlignerWorkflow massAligner = new IqMassAlignerWorkflow(parameters, run);
            massAligner.LoadAndInitializeTargets();

            double testNET = massAligner.Targets.First().ElutionTimeTheor;

            var query = (from massTag in massTagRefs
                         join target in massAligner.Targets on massTag.Code equals target.Code
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

          double testNETAfter = massAligner.Targets.First().ElutionTimeTheor;
            Console.WriteLine("NET before= " + testNET);
            Console.WriteLine("NET before= " + testNETAfter);

        }


        [Test]
        public void TempTest1()
        {
            string code = "K.LADDVDLEQVANETHGHVGADLAALCSEAALQAIR.K";

            PeptideUtils peptideUtils = new PeptideUtils();

            string empiricalFormula= peptideUtils.GetEmpiricalFormulaForPeptideSequence(code);

            var monomass = peptideUtils.GetMonoIsotopicMassForPeptideSequence(code);

            Console.WriteLine(monomass);


            var formula = EmpiricalFormulaUtilities.ParseEmpiricalFormulaString("H(3) C(2) N O",true);

            var revisedFormula = EmpiricalFormulaUtilities.GetEmpiricalFormulaFromElementTable(formula);


            var iodoMass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(revisedFormula);

            Console.WriteLine("iodomass= " + iodoMass);

           


        }


    }
}
