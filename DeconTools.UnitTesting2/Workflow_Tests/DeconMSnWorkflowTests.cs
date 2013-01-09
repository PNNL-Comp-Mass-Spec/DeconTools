using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Workflows;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Workflow_Tests
{
    [TestFixture]
    public class DeconMSnWorkflowTests
    {
        private string testFile1 = FileRefs.RawDataMSFiles.OrbitrapStdFile1;

        [Test]
        public void WorkflowTest1()
        {
            var parameters = new DeconToolsParameters();
            //parameters.MSGeneratorParameters.UseLCScanRange = true;
            //parameters.MSGeneratorParameters.MinLCScan = 6005;
            //parameters.MSGeneratorParameters.MaxLCScan = 9000;

            parameters.PeakDetectorParameters.PeakToBackgroundRatio = 1.3;
            parameters.PeakDetectorParameters.SignalToNoiseThreshold = 2;
            parameters.PeakDetectorParameters.IsDataThresholded = true;

            parameters.ThrashParameters.MinIntensityForScore = 1;
            parameters.ThrashParameters.MinMSFeatureToBackgroundRatio = 1;

            var run = new RunFactory().CreateRun(testFile1);

            var workflow = new DeconMSnWorkflow(parameters, run);
            workflow.Execute();


        }



        [Test]
        public void WorkflowTesting1()
        {
            var parameters = new DeconToolsParameters();
            parameters.MSGeneratorParameters.UseLCScanRange = true;
            parameters.MSGeneratorParameters.MinLCScan = 6012;
            parameters.MSGeneratorParameters.MaxLCScan = 6019;

            parameters.PeakDetectorParameters.PeakToBackgroundRatio = 1.3;
            parameters.PeakDetectorParameters.SignalToNoiseThreshold = 2;
            parameters.PeakDetectorParameters.IsDataThresholded = true;

            parameters.ThrashParameters.MinIntensityForScore = 1;
            parameters.ThrashParameters.MinMSFeatureToBackgroundRatio = 1;

            var run = new RunFactory().CreateRun(testFile1);

            var workflow = new DeconMSnWorkflow(parameters, run);
            workflow.Execute();


        }

    }
}
