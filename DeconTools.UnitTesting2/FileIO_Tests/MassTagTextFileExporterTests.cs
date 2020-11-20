using System.IO;
using DeconTools.Backend.FileIO;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.FileIO_Tests
{
    [TestFixture]
    [Ignore("Missing files")]
    public class MassTagTextFileExporterTests
    {
        readonly string testOutput1 = @"..\..\..\TestFiles\FileIOTests\exportedMassTags.txt";

        [Test]
        public void exportMassTagsToTextFile_Test1()
        {
            //first, import some data
            var massTagTestFile1 = @"..\..\..\TestFiles\FileIOTests\top40MassTags.txt";
            var massTagImporter = new MassTagFromTextFileImporter(massTagTestFile1);
            var mtc = massTagImporter.Import();

            //second, export it

            //but first delete any existing output file
            if (File.Exists(testOutput1))
            {
                File.Delete(testOutput1);
            }

            var exporter = new MassTagTextFileExporter(testOutput1);
            exporter.ExportResults(mtc.TargetList);

            var fi = new FileInfo(testOutput1);
            Assert.IsTrue(fi.Exists);

            //Assert.AreEqual(13716, fi.Length);

        }
    }
}
