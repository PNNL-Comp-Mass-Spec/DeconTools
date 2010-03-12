using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Linq;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.ResultExporters.MassTagResultExporters;

namespace DeconTools.UnitTesting.ProcessingTasksTests.TargetedAnalysisTests
{
    [TestFixture]
    public class MassTagResultExporterTests
    {
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        private string exporterOutputFile1 = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_MassTag_isos.db3";


        [Test]
        public void test1()
        {
            Run run = new XCaliburRun(xcaliburTestfile);

            List<MassTag> mass_tagList = TestUtilities.CreateTestMassTagList();
            MassTag mt = mass_tagList[0];
            
            run.CurrentScanSet = new ScanSet(9017, new int[] { 9010, 9017, 9024 });
            run.CurrentMassTag = mt;


            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);

            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters(2, 1.3, true, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            Task mspeakDet = new DeconToolsPeakDetector(peakParams);

            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task targetedFeatureFinder = new BasicTFeatureFinder(0.01);

            MassTagFitScoreCalculator fitScoreCalc = new MassTagFitScoreCalculator();
            Task exporter = new BasicMTResultSQLiteExporter(exporterOutputFile1);





            msgen.Execute(run.ResultCollection);

            //run.XYData.Display();

            mspeakDet.Execute(run.ResultCollection);
            theorFeatureGen.Execute(run.ResultCollection);
            targetedFeatureFinder.Execute(run.ResultCollection);
            fitScoreCalc.Execute(run.ResultCollection);
            exporter.Execute(run.ResultCollection);
            exporter.Cleanup();

            MassTagResultBase result = run.ResultCollection.GetMassTagResult(mt);
            TestUtilities.DisplayIsotopicProfileData(result.IsotopicProfile);



        }

    }
}
