using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.Core;
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
            var outputFile = FileRefs.OutputFolderPath + "ExportPeakDataToTextFileTest1.txt";

            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            var run = TestDataCreationUtilities.CreatePeakDataFromStandardOrbitrapData();
            NUnit.Framework.Assume.That(5608==run.ResultCollection.MSPeakResultList.Count);

            var peakExporter = new PeakListTextExporter(run.MSFileType, outputFile);
            peakExporter.WriteOutPeaks(run.ResultCollection.MSPeakResultList);

            var fi=new FileInfo(outputFile);

            Assert.AreEqual(true, fi.Exists);
            Assert.AreNotEqual(0, fi.Length);
            
            

        }

        [Test]
        public void ExportUIMFPeakDataToTextFileTest1()
        {
            //TODO:  finish this
        }


        [Test]
        public void ExportToSQLiteFileTest1()
        {
            //TODO: finish this
        
        }


        [Test]
        public void ExportUIMFPeakDataToSQLiteFileTest1()
        {
            //TODO: finish this

        }




    }
}
