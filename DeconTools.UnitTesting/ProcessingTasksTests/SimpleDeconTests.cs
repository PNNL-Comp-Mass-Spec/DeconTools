using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.Deconvoluters;
using System.Linq;
using DeconTools.Backend;

namespace DeconTools.UnitTesting.ProcessingTasksTests
{
    [TestFixture]
    public class SimpleDeconTests
    {
        public string lowmwRawfile = "..\\..\\TestFiles\\C2-6 Stock MeOH 50-1000 pos FTMS.raw";
        public string xcaliburParameterFile1 = "..\\..\\TestFiles\\xcaliburParameterFile1.xml";


        [Test]
        public void test1()
        {

            Run run = new XCaliburRun(lowmwRawfile);

            ResultCollection results = new ResultCollection(run);

            run.CurrentScanSet = new ScanSet(1);
            Task msgen = new GenericMSGenerator(0, 1000);
            msgen.Execute(results);

            DeconToolsPeakDetector peakdetector = new DeconToolsPeakDetector();
            peakdetector.PeakBackgroundRatio = 0.5;
            peakdetector.PeakFitType = DeconTools.Backend.Globals.PeakFitType.QUADRATIC;
            peakdetector.SigNoiseThreshold = 3;
            peakdetector.IsDataThresholded = true;
            peakdetector.Execute(results);

            //Task rapidDecon = new RapidDeconvolutor();
            //rapidDecon.Execute(results);

            Task simpleDecon = new SimpleDecon(0.0005);
            simpleDecon.Execute(results);

            Console.WriteLine(results.Run.MSPeakList.Count);
            Console.WriteLine(results.ResultList.Count);


            string reportedResults = reportResults(results.ResultList);

            Console.WriteLine(reportedResults);

        }


        [Test]
        public void oldSchoolProcRunnerTest1()
        {
            OldSchoolProcRunner runner = new OldSchoolProcRunner(lowmwRawfile, Globals.MSFileType.Finnigan, xcaliburParameterFile1);
            Assert.AreEqual(false, runner.Project.Parameters.OldDecon2LSParameters.HornTransformParameters.UseScanRange);
            runner.Execute();


        }

        private string reportResults(List<IsosResult> resultlist)
        {
            
            
            StringBuilder sb = new StringBuilder();
            sb.Append("Z");
            sb.Append("\t");
            sb.Append("m/z");
            sb.Append("\t");
            sb.Append("intens");
            sb.Append("\t");
            sb.Append("s/n");
            sb.Append("\t");
            sb.Append("score");
            sb.Append("\n");

            foreach (IsosResult result in resultlist)
            {
                sb.Append(result.IsotopicProfile.ChargeState);
                sb.Append("\t");
                sb.Append(result.IsotopicProfile.Peaklist[0].MZ);
                sb.Append("\t");
                sb.Append(result.IsotopicProfile.Peaklist[0].Intensity);
                sb.Append("\t");
                sb.Append(result.IsotopicProfile.Peaklist[0].SN);
                sb.Append("\t");
                sb.Append(result.IsotopicProfile.Score);
                sb.Append("\n");


            }
            return sb.ToString();
        }


    }
}
