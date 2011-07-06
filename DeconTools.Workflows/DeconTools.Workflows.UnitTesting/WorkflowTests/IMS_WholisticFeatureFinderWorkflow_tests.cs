using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.UnitTesting2;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class IMS_WholisticFeatureFinderWorkflow_tests
    {
        [Test]
        public void test1()
        {

            //string masterPeaksFilepath = @"\\protoapps\UserData\Shah\TestFiles\Sarc_MS_90_21Aug10_Cheetah_10-08-02_0000_filtered_peaks.txt";
    //        string masterPeaksFilepath = @"\\protoapps\UserData\Shah\TestFiles\Sarc_MS_90_21Aug10_Cheetah_10-08-02_0000_filteredIssue383_peaks.txt";

            string masterPeaksFilepath = @"\\protoapps\UserData\Shah\TestFiles\Sarc_MS_90_21Aug10_Cheetah_10-08-02_0000_MZfiltered_peaks.txt";


            Run run = new UIMFRun(FileRefs.RawDataMSFiles.sarcUIMFFile1);


            IMS_SmartFeatureFinderWorkflow workflow = new IMS_SmartFeatureFinderWorkflow(run, masterPeaksFilepath);
            workflow.Execute();



        }



    }
}
