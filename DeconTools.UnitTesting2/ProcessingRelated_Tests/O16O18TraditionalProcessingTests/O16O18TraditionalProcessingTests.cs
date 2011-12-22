using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.O16O18TraditionalProcessingTests
{
    [TestFixture]
    public class O16O18TraditionalProcessingTests
    {
        [Test]
        public void Test1()
        {
            string rawfilename = @"D:\Data\O16O18\GlueGrant\GG_MO_Trauma_374020_20May11_Sphinx_11-03-28.RAW";
            string exportedIsos = Path.GetDirectoryName(rawfilename) + "\\" + Path.GetFileName(rawfilename).Replace(".RAW", "_test_isos.csv");

            if (File.Exists(exportedIsos)) File.Delete(exportedIsos);

            Run run = new RunFactory().CreateRun(rawfilename);

            run.ResultCollection.ResultType = Backend.Globals.ResultType.O16O18_TRADITIONAL_RESULT;

            run.ScanSetCollection = ScanSetCollection.Create(run, 5000, 5000, 1, 1);
            

            run.CurrentScanSet = run.ScanSetCollection.ScanSetList[0];

            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            DeconToolsPeakDetector peakDet = new DeconToolsPeakDetector();
            peakDet.PeakBackgroundRatio = 1.3;
            peakDet.SigNoiseThreshold = 2;

            HornDeconvolutor decon = new HornDeconvolutor();
            decon.IsO16O18Data = true;

            O16O18PeakDataAppender appender = new O16O18PeakDataAppender();

            var exporter = IsosExporterFactory.CreateIsosExporter(run.ResultCollection.ResultType, Backend.Globals.ExporterType.TEXT, exportedIsos);

            msgen.Execute(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);
            appender.Execute(run.ResultCollection);

            O16O18IsosResult testResult = (O16O18IsosResult)run.ResultCollection.ResultList[1];

            Assert.AreEqual(DeconTools.Backend.Globals.ResultType.O16O18_TRADITIONAL_RESULT, run.ResultCollection.ResultType);
            Assert.AreEqual(5905390, testResult.IsotopicProfile.GetMonoAbundance());
            Assert.AreEqual(3017899, testResult.MonoPlus2Abundance);
            Assert.AreEqual(162389, testResult.MonoPlus4Abundance);
            exporter.Execute(run.ResultCollection);

            run.Close();
        }

    }
}
