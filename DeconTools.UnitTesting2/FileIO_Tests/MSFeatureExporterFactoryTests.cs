using NUnit.Framework;
using DeconTools.Backend.FileIO;
using System.IO;

namespace DeconTools.UnitTesting2.FileIO_Tests
{
    [TestFixture]
    public class MSFeatureExporterFactoryTests
    {
        readonly string exporterFactoryUIMFTextFile1 = FileRefs.TestFileBasePath + @"\FileIOTests\exporterFactory_UIMF_TextFile1.csv";
        readonly string exporterFactoryIMFTextFile1 = FileRefs.TestFileBasePath + @"\FileIOTests\exporterFactory_IMF_TextFile1.csv";
        readonly string exporterFactoryBasicTextFile1 = FileRefs.TestFileBasePath + @"\FileIOTests\exporterFactory_Basic_TextFile1.csv";

        [Test]
        public void create_UIMF_IsosTextExporterTest1()
        {
            if (File.Exists(exporterFactoryUIMFTextFile1))
            {
                File.Delete(exporterFactoryUIMFTextFile1);
            }

            var exporter = MSFeatureExporterFactory.CreateMSFeatureExporter(
                Backend.Globals.ExporterType.Text,
                Backend.Globals.MSFileType.PNNL_UIMF, exporterFactoryUIMFTextFile1);

            Assert.AreEqual(typeof(MSFeatureToTextFileExporterUIMF), exporter.GetType());

            var fileWasCreated = (File.Exists(exporterFactoryUIMFTextFile1));
            Assert.AreEqual(true, fileWasCreated);
        }


        [Test]
        public void create_IMF_IsosTextExporterTest1()
        {
            var x = exporterFactoryIMFTextFile1;

            if (File.Exists(x))
            {
                File.Delete(x);
            }

            var exporter = MSFeatureExporterFactory.CreateMSFeatureExporter(
                Backend.Globals.ExporterType.Text,
                Backend.Globals.MSFileType.PNNL_IMS, x);

            Assert.AreEqual(typeof(MSFeatureToTextFileExporterIMF), exporter.GetType());

            var fileWasCreated = (File.Exists(x));
            Assert.AreEqual(true, fileWasCreated);
        }

        [Test]
        public void create_basic_IsosTextExporterTest1()
        {
            var x = exporterFactoryBasicTextFile1;

            if (File.Exists(x))
            {
                File.Delete(x);
            }

            var exporter = MSFeatureExporterFactory.CreateMSFeatureExporter(
                Backend.Globals.ExporterType.Text,
                Backend.Globals.MSFileType.Thermo_Raw, x);

            Assert.AreEqual(typeof(MSFeatureToTextFileExporterBasic), exporter.GetType());

            var fileWasCreated = (File.Exists(x));
            Assert.AreEqual(true, fileWasCreated);
        }

    }
}
