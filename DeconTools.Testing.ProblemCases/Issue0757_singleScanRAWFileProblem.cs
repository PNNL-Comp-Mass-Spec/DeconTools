using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using DeconTools.UnitTesting2;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks;

namespace DeconTools.Testing.ProblemCases
{
    [TestFixture]
    public class Issue0757_singleScanRAWFileProblem
    {
        string rawDatafile = @"D:\Data\Redmine_issues\Issue0757_SingleScanRawProblem\08232011_UBQ_500MS_AGC_1E6_857_IW_3_03.raw";

        [Test]
        public void procRunnerTest()
        {
      
            string parameterFile = @"\\Gigasax\DMS_Parameter_Files\Decon2LS\LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1.xml";

            OldSchoolProcRunner runner = new OldSchoolProcRunner(rawDatafile, Globals.MSFileType.Finnigan, parameterFile);
            runner.Execute();


        }


        [Test]
        public void run_initializeTest()
        {
            Run run = new RunFactory().CreateRun(rawDatafile);
            Console.WriteLine(TestUtilities.DisplayRunInformation(run));


            MSGeneratorFactory msfact = new MSGeneratorFactory();

            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            ScanSet scan = new ScanSet(1);
            run.CurrentScanSet = scan;

            Console.WriteLine("Scan MSLevel = " + run.GetMSLevel(1));

            msgen.Execute(run.ResultCollection);

            //TestUtilities.DisplayXYValues(run.XYData);

            
           

        }


    }
}
