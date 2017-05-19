using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Runs;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.ProcessingRelated_Tests.O16O18TraditionalProcessingTests
{
    [TestFixture]
    public class O16O18TraditionalProcessingTests
    {
        [Test]
        public void Test1()
        {
            var rawfilename = @"D:\Data\O16O18\GlueGrant\GG_MO_Trauma_374020_20May11_Sphinx_11-03-28.RAW";
            var exportedIsos = Path.Combine(Path.GetDirectoryName(rawfilename),  Path.GetFileName(rawfilename).Replace(".RAW", "_test_isos.csv"));

            if (File.Exists(exportedIsos)) File.Delete(exportedIsos);

            var run = new RunFactory().CreateRun(rawfilename);

            run.ResultCollection.ResultType = Backend.Globals.ResultType.O16O18_TRADITIONAL_RESULT;

            run.ScanSetCollection .Create(run, 5000, 5000, 1, 1);
            

            run.CurrentScanSet = run.ScanSetCollection.ScanSetList[0];

            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var peakDet = new DeconToolsPeakDetectorV2();
            peakDet.PeakToBackgroundRatio = 1.3;
            peakDet.SignalToNoiseThreshold = 2;

            var decon = new HornDeconvolutor();
            decon.IsO16O18Data = true;

            var appender = new O16O18PeakDataAppender();

            var exporter = IsosExporterFactory.CreateIsosExporter(run.ResultCollection.ResultType, Backend.Globals.ExporterType.Text, exportedIsos);

            msgen.Execute(run.ResultCollection);
            peakDet.Execute(run.ResultCollection);
            decon.Execute(run.ResultCollection);
            appender.Execute(run.ResultCollection);

            var testResult = (O16O18IsosResult)run.ResultCollection.ResultList[1];

            Assert.AreEqual(DeconTools.Backend.Globals.ResultType.O16O18_TRADITIONAL_RESULT, run.ResultCollection.ResultType);
            Assert.AreEqual(5905390, testResult.IsotopicProfile.GetMonoAbundance());
            Assert.AreEqual(3017899, testResult.MonoPlus2Abundance);
            Assert.AreEqual(162389, testResult.MonoPlus4Abundance);
            exporter.Execute(run.ResultCollection);

            run.Close();
        }

    }
}
