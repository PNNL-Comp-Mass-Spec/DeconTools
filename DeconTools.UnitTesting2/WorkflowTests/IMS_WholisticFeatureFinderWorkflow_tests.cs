using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Workflows;
using DeconTools.Backend.Utilities;

namespace DeconTools.UnitTesting2.WorkflowTests
{
    [TestFixture]
    public class IMS_WholisticFeatureFinderWorkflow_tests
    {
        [Test]
        public void test1()
        {
            int frameNum = 2000;

            Run run = new UIMFRun(FileRefs.RawDataMSFiles.sarcUIMFFile1);

            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(run, frameNum, frameNum, 1, 1);
            fscc.Create();

            IMS_WholisticFeatureFinderWorkflow workflow = new IMS_WholisticFeatureFinderWorkflow();
            workflow.ExecuteWorkflow(run);
        }


    }
}
