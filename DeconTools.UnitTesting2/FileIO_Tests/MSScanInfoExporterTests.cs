using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;

namespace DeconTools.UnitTesting2.FileIO_Tests
{
    [TestFixture]
    public class MSScanInfoExporterTests
    {
        string exportedMSScanInfoToTextFileFromOrbitrapFile1 = FileRefs.TestFileBasePath + "\\FileIOTests\\exportedMSScanInfoToTextFileFromOrbitrapFile1.csv";
        string exportedMSScanInfoToSQLiteFromOrbitrapFile1 = FileRefs.TestFileBasePath + "\\FileIOTests\\exportedMSScanInfoToSQLiteFromOrbitrapFile1.db3";
        string exportedMSScanInfoToSQLiteFromUIMFFile1 = FileRefs.TestFileBasePath + "\\FileIOTests\\exportedMSScanInfoToSQLiteFromUIMFFile1.db3";
        string exportedMSScanInfoToTextFileFromUIMFFile1 = FileRefs.TestFileBasePath + "\\FileIOTests\\exportedMSScanInfoToTextFileFromUIMFFile1.csv";

        [Test]
        public void outputToText_xcaliburData_Test1()
        {
            string exportedFile = exportedMSScanInfoToTextFileFromOrbitrapFile1;


            if (File.Exists(exportedFile))
            {
                File.Delete(exportedFile);
            }

            //create run and get some results

            ExporterBase<ScanResult> exporter = new DeconTools.Backend.FileIO.MSScanInfoToTextFileExporterBasic(exportedFile);
            Run run = TestDataCreationUtilities.CreateResultsFromThreeScansOfStandardOrbitrapData();

            exporter.ExportResults(run.ResultCollection.ScanResultList);
            Assert.AreEqual(true, File.Exists(exportedFile));

            FileInfo fi = new FileInfo(exportedFile);
            Assert.IsTrue(fi.Length>200);
            Console.Write(fi.Length);

        }

        [Test]
        public void ouputToSQLite_xcaliburData_Test1()
        {
            string exportedFile = exportedMSScanInfoToSQLiteFromOrbitrapFile1;

            if (File.Exists(exportedFile))
            {
                File.Delete(exportedFile);
            }

            ExporterBase<ScanResult> exporter = new MSScanInfoToSQLiteExporterBasic(exportedFile);

            Run run = TestDataCreationUtilities.CreateResultsFromThreeScansOfStandardOrbitrapData();

            exporter.ExportResults(run.ResultCollection.ScanResultList);

            Assert.AreEqual(true, File.Exists(exportedFile));

            FileInfo fi = new FileInfo(exportedFile);
            Assert.AreEqual(2048, fi.Length);
            Console.Write(fi.Length);


        }


        [Test]
        public void outputToText_UIMFData_Test1()
        {
            string exportedFile = exportedMSScanInfoToTextFileFromUIMFFile1;


            if (File.Exists(exportedFile))
            {
                File.Delete(exportedFile);
            }

            //create run and get some results
            Run run = TestDataCreationUtilities.CreateResultsFromTwoFramesOfStandardUIMFData();

            ExporterBase<ScanResult> exporter = new DeconTools.Backend.FileIO.MSScanInfoToTextFileExporterUIMF(exportedFile);
            exporter.ExportResults(run.ResultCollection.ScanResultList);

            Assert.AreEqual(true, File.Exists(exportedFile));

            FileInfo fi = new FileInfo(exportedFile);
            Assert.AreEqual(220, fi.Length);
            Console.Write(fi.Length);
        }

        [Test]
        public void ouputToSQLite_UIMFData_Test1()
        {
            string exportedFile = exportedMSScanInfoToSQLiteFromUIMFFile1;

            if (File.Exists(exportedFile))
            {
                File.Delete(exportedFile);
            }

            ExporterBase<ScanResult> exporter = new MSScanInfoToSQLiteExporterUIMF(exportedFile);

            Run run = TestDataCreationUtilities.CreateResultsFromTwoFramesOfStandardUIMFData();

            exporter.ExportResults(run.ResultCollection.ScanResultList);

            Assert.AreEqual(true, File.Exists(exportedFile));

            FileInfo fi = new FileInfo(exportedFile);
            Assert.AreEqual(2048, fi.Length);
            Console.Write(fi.Length);


        }



    }
}
