using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Workflows;
using DeconTools.UnitTesting2;
using NUnit.Framework;

namespace DeconTools.Testing.ProblemCases
{
    [TestFixture]
    public class OMCS_0482_UIMFScanResultProblem
    {
        [Test]
        public void Test1()
        {



            string testFile =
                @"\\proto-10\IMS_TOF_4\2012_3\Sarc_P22_B06_2034_23Jul12_Cheetah_11-12-23\Sarc_P22_B06_2034_23Jul12_Cheetah_11-12-23.uimf";

            testFile = @"D:\Data\UIMF\Problem_datasets\Sarc_P22_B06_2034_23Jul12_Cheetah_11-12-23.uimf";

            string parameterFile =
                @"\\gigasax\DMS_Parameter_Files\Decon2LS\IMS_UIMF_PeakBR2_PeptideBR3_SN3_SumScans3_NoLCSum_Sat50000_2012-02-27.xml";

            string expectedIsosOutput = Path.GetDirectoryName(testFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(testFile) + "_isos.csv";
            string expectedScansOutput = Path.GetDirectoryName(testFile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(testFile) + "_scans.csv";


            if (File.Exists(expectedIsosOutput))
            {
                File.Delete(expectedIsosOutput);
            }

            if (File.Exists(expectedScansOutput))
            {
                File.Delete(expectedScansOutput);
            }


            var workflow = ScanBasedWorkflow.CreateWorkflow(testFile, parameterFile);
            
            workflow.Execute();

        }

    }
}
