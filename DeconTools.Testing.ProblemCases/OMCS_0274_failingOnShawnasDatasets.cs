using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Workflows;
using DeconTools.UnitTesting2;
using NUnit.Framework;

namespace DeconTools.Testing.ProblemCases
{
    public class OMCS_0274_failingOnShawnasDatasets
    {
        private string _testdatafile =
            @"D:\Data\JIRA_issues\Hengel_UBQ_DI_ISO_1224_IW_2.raw";
      
        [Test]
        public void getRunInfoTest1()
        {

            var run = new RunFactory().CreateRun(_testdatafile);

            Console.WriteLine(TestUtilities.DisplayRunInformation(run));

            OldDecon2LSParameters parameters=new OldDecon2LSParameters();
            parameters.PeakProcessorParameters.WritePeaksToTextFile = true;

            ScanBasedWorkflow workflow = ScanBasedWorkflow.CreateWorkflow(run, new OldDecon2LSParameters());
            workflow.Execute();


        }


        [Test]
        public void msgenTest1()
        {
            var run = new RunFactory().CreateRun(_testdatafile);
            var msGen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            ScanSet scan = new ScanSet(1);

            run.CurrentScanSet = scan;

            msGen.Execute(run.ResultCollection);

            Console.WriteLine("scan\tmsLevel");
            for (int i = run.MinScan; i < run.MaxScan; i++)
            {
                Console.WriteLine(i + "\t" + run.GetMSLevel(i));
            }
            


            //TestUtilities.DisplayXYValues(run.XYData);



        }


    }
}
