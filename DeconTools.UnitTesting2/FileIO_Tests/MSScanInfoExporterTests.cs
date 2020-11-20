using System;
using NUnit.Framework;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;

namespace DeconTools.UnitTesting2.FileIO_Tests
{
    [TestFixture]
    public class MSScanInfoExporterTests
    {
        readonly string exportedMSScanInfoToTextFileFromOrbitrapFile1 = FileRefs.TestFileBasePath + @"\FileIOTests\exportedMSScanInfoToTextFileFromOrbitrapFile1.csv";
        readonly string exportedMSScanInfoToSQLiteFromOrbitrapFile1 = FileRefs.TestFileBasePath + @"\FileIOTests\exportedMSScanInfoToSQLiteFromOrbitrapFile1.db3";
        readonly string exportedMSScanInfoToSQLiteFromUIMFFile1 = FileRefs.TestFileBasePath + @"\FileIOTests\exportedMSScanInfoToSQLiteFromUIMFFile1.db3";
        readonly string exportedMSScanInfoToTextFileFromUIMFFile1 = FileRefs.TestFileBasePath + @"\FileIOTests\exportedMSScanInfoToTextFileFromUIMFFile1.csv";

        [Test]
        public void outputToText_xcaliburData_Test1()
        {
            var exportedFile = exportedMSScanInfoToTextFileFromOrbitrapFile1;

            if (File.Exists(exportedFile))
            {
                File.Delete(exportedFile);
            }

            //create run and get some results

            ExporterBase<ScanResult> exporter = new MSScanInfoToTextFileExporterBasic(exportedFile);
            var run = TestDataCreationUtilities.CreateResultsFromThreeScansOfStandardOrbitrapData();

            exporter.ExportResults(run.ResultCollection.ScanResultList);
            Assert.AreEqual(true, File.Exists(exportedFile));

            var fi = new FileInfo(exportedFile);
            Assert.IsTrue(fi.Length > 200);

            Console.WriteLine("Created " + fi.FullName);

            Console.WriteLine("File length: {0} bytes", fi.Length);
        }

        [Test]
        public void ouputToSQLite_xcaliburData_Test1()
        {
            var exportedFile = exportedMSScanInfoToSQLiteFromOrbitrapFile1;

            if (File.Exists(exportedFile))
            {
                File.Delete(exportedFile);
            }

            ExporterBase<ScanResult> exporter = new MSScanInfoToSQLiteExporterBasic(exportedFile);

            var run = TestDataCreationUtilities.CreateResultsFromThreeScansOfStandardOrbitrapData();

            exporter.ExportResults(run.ResultCollection.ScanResultList);

            Assert.AreEqual(true, File.Exists(exportedFile));

            var fi = new FileInfo(exportedFile);
            Assert.AreEqual(2048, fi.Length);
            Console.Write(fi.Length);
        }

        [Test]
        [Ignore("Local testing only")]
        public void outputToText_UIMFData_Test1()
        {
            var exportedFile = exportedMSScanInfoToTextFileFromUIMFFile1;

            if (File.Exists(exportedFile))
            {
                File.Delete(exportedFile);
            }

            //create run and get some results
            var run = TestDataCreationUtilities.CreateResultsFromTwoFramesOfStandardUIMFData();

            ExporterBase<ScanResult> exporter = new MSScanInfoToTextFileExporterUIMF(exportedFile);
            exporter.ExportResults(run.ResultCollection.ScanResultList);

            Assert.AreEqual(true, File.Exists(exportedFile));

            var fi = new FileInfo(exportedFile);
            Assert.AreEqual(220, fi.Length);
            Console.Write(fi.Length);
        }

        [Test]
        [Ignore("Local testing only")]
        public void ouputToSQLite_UIMFData_Test1()
        {
            var exportedFile = exportedMSScanInfoToSQLiteFromUIMFFile1;

            if (File.Exists(exportedFile))
            {
                File.Delete(exportedFile);
            }

            ExporterBase<ScanResult> exporter = new MSScanInfoToSQLiteExporterUIMF(exportedFile);

            var run = TestDataCreationUtilities.CreateResultsFromTwoFramesOfStandardUIMFData();

            exporter.ExportResults(run.ResultCollection.ScanResultList);

            Assert.AreEqual(true, File.Exists(exportedFile));

            var fi = new FileInfo(exportedFile);
            Assert.AreEqual(2048, fi.Length);
            Console.Write(fi.Length);
        }
    }
}
