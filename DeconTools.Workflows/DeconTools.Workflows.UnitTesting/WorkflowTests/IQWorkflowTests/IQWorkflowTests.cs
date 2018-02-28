using System;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests.IQWorkflowTests
{
    [TestFixture]
    public class IQWorkflowTests
    {
        [Test]
        public void IqWorkflowTest1()
        {
            //see  https://jira.pnnl.gov/jira/browse/OMCS-709

            var testFile = DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            var peaksTestFile = DeconTools.UnitTesting2.FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
            var massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";

            var run = RunUtilities.CreateAndAlignRun(testFile, peaksTestFile);

            var mtc = new TargetCollection();
            var mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            var testMassTagID = 24800;
            var oldStyleTarget = (from n in mtc.TargetList where n.ID == testMassTagID && n.ChargeState == 2 select n).First();

            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            parameters.ChromatogramCorrelationIsPerformed = true;
            parameters.ChromSmootherNumPointsInSmooth = 9;
            parameters.ChromPeakDetectorPeakBR = 1;
            parameters.ChromPeakDetectorSigNoise = 1;

            IqWorkflow iqWorkflow = new BasicIqWorkflow(run, parameters);

            IqTarget target = new IqChargeStateTarget(iqWorkflow);

            target.ID = oldStyleTarget.ID;
            target.MZTheor = oldStyleTarget.MZ;
            target.ElutionTimeTheor = oldStyleTarget.NormalizedElutionTime;
            target.MonoMassTheor = oldStyleTarget.MonoIsotopicMass;
            target.EmpiricalFormula = oldStyleTarget.EmpiricalFormula;
            target.ChargeState = oldStyleTarget.ChargeState;

            Console.WriteLine(target.EmpiricalFormula + "\t" + target.ChargeState);


            target.DoWorkflow();
            var result=  target.GetResult();

            Assert.IsNotNull(result, "result is null");

            var reportString = result.ToStringWithDetailedReport();
            Console.WriteLine(reportString);

        }


        [Test]
        public void FailureToFindAnyChromatogramDataTest1()
        {
            var testFile = DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1;
            var peaksTestFile = DeconTools.UnitTesting2.FileRefs.PeakDataFiles.OrbitrapPeakFile_scans5500_6500;
            // Unused: var massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\QCShew_Formic_MassTags_Bin10_all.txt";

            var run = RunUtilities.CreateAndLoadPeaks(testFile, peaksTestFile);


            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            parameters.ChromGenTolerance = 1;    //very narrow tolerance



            IqWorkflow iqWorkflow = new BasicIqWorkflow(run, parameters);
            IqTarget target = new IqChargeStateTarget(iqWorkflow);

            target.EmpiricalFormula = DeconTools.Backend.Utilities.IsotopeDistributionCalculation.IsotopicDistributionCalculator.Instance.GetAveragineFormulaAsString(1399);
            target.ElutionTimeTheor = 0.3;
            target.ChargeState = 1;


            target.DoWorkflow();
            var result = target.GetResult();

            Assert.IsTrue(result.Target.TheorIsotopicProfile != null);
            Assert.IsTrue(result != null);
            Assert.IsTrue(result.IqResultDetail.Chromatogram == null);

            var reportString = result.ToStringWithDetailedReport();
            Console.WriteLine(reportString);


        }

    }
}
