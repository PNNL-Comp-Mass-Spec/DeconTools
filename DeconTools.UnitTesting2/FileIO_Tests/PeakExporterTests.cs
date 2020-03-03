using NUnit.Framework;
using DeconTools.Backend.ProcessingTasks.PeakListExporters;
using System.IO;

namespace DeconTools.UnitTesting2.FileIO_Tests
{
    [TestFixture]
    public class PeakExporterTests
    {


        [Test]
        public void ExportPeakDataToTextFileTest1()
        {
            var outputFile = FileRefs.OutputDirectoryPath + "ExportPeakDataToTextFileTest1.txt";

            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            var run = TestDataCreationUtilities.CreatePeakDataFromStandardOrbitrapData();
            Assume.That(6115 == run.ResultCollection.MSPeakResultList.Count);

            var peakExporter = new PeakListTextExporter(run.MSFileType, outputFile);
            peakExporter.WriteOutPeaks(run.ResultCollection.MSPeakResultList);

            var fi = new FileInfo(outputFile);

            Assert.IsTrue(fi.Exists);
            Assert.AreNotEqual(0, fi.Length);

        }

        [Test]
        [Ignore("Not implemented")]
        public void ExportUIMFPeakDataToTextFileTest1()
        {
            //TODO:  finish this
        }


        [Test]
        [Ignore("Not implemented")]
        public void ExportToSQLiteFileTest1()
        {
            //TODO: finish this

        }


        [Test]
        [Ignore("Not implemented")]
        public void ExportUIMFPeakDataToSQLiteFileTest1()
        {
            //TODO: finish this

        }




    }
}
