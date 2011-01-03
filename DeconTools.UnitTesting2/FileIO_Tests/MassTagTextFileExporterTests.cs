using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data.Importers;
using System.IO;

namespace DeconTools.UnitTesting2.FileIO_Tests
{
    [TestFixture]
    public class MassTagTextFileExporterTests
    {

        string testOutput1 = "..\\..\\..\\TestFiles\\FileIOTests\\exportedMassTags.txt";


        [Test]
        public void exportMassTagsToTextFile_Test1()
        {
            //first, import some data
            MassTagCollection mtc = new MassTagCollection();
            string massTagTestFile1 = "..\\..\\..\\TestFiles\\FileIOTests\\top40MassTags.txt";
            MassTagFromTextFileImporter massTagImporter = new MassTagFromTextFileImporter(massTagTestFile1);
            mtc = massTagImporter.Import();

            //second, export it

            //but first delete any existing output file
            if (File.Exists(testOutput1))
            {
                File.Delete(testOutput1);
            }
            
            MassTagTextFileExporter exporter = new MassTagTextFileExporter(testOutput1);
            exporter.ExportResults(mtc.MassTagList);

            FileInfo fi = new FileInfo(testOutput1);
            Assert.IsTrue(fi.Exists);

            Assert.AreEqual(12178, fi.Length);

        }


    }
}
