using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Core;
using System.IO;

namespace DeconTools.UnitTesting2.FileIO_Tests
{
    [TestFixture]
    public class MSFeatureExporterFactoryTests
    {
        string exporterFactoryUIMFTextFile1 = FileRefs.TestFileBasePath + "\\FileIOTests\\exporterFactory_UIMF_TextFile1.csv";
        string exporterFactoryIMFTextFile1 = FileRefs.TestFileBasePath + "\\FileIOTests\\exporterFactory_IMF_TextFile1.csv";
        string exporterFactoryBasicTextFile1 = FileRefs.TestFileBasePath + "\\FileIOTests\\exporterFactory_Basic_TextFile1.csv";
        [Test]
        public void create_UIMF_IsosTextExporterTest1()
        {
            if (File.Exists(exporterFactoryUIMFTextFile1))
            {
                File.Delete(exporterFactoryUIMFTextFile1);
            }
            
            ExporterBase<IsosResult> exporter =
                    MSFeatureExporterFactory.CreateMSFeatureExporter(DeconTools.Backend.Globals.ExporterType.TEXT,
                    DeconTools.Backend.Globals.MSFileType.PNNL_UIMF, exporterFactoryUIMFTextFile1);

            Assert.AreEqual(typeof(MSFeatureToTextFileExporterUIMF), exporter.GetType());

            bool fileWasCreated = (File.Exists(exporterFactoryUIMFTextFile1));
            Assert.AreEqual(true, fileWasCreated);
        }


        [Test]
        public void create_IMF_IsosTextExporterTest1()
        {
            string x = exporterFactoryIMFTextFile1;

            if (File.Exists(x))
            {
                File.Delete(x);
            }

            ExporterBase<IsosResult> exporter = 
                MSFeatureExporterFactory.CreateMSFeatureExporter(DeconTools.Backend.Globals.ExporterType.TEXT,
                DeconTools.Backend.Globals.MSFileType.PNNL_IMS, x);

            Assert.AreEqual(typeof(MSFeatureToTextFileExporterIMF), exporter.GetType());

            bool fileWasCreated = (File.Exists(x));
            Assert.AreEqual(true, fileWasCreated);
        }

        [Test]
        public void create_basic_IsosTextExporterTest1()
        {
            string x = exporterFactoryBasicTextFile1;

            if (File.Exists(x))
            {
                File.Delete(x);
            }

            ExporterBase<IsosResult> exporter =
                MSFeatureExporterFactory.CreateMSFeatureExporter(DeconTools.Backend.Globals.ExporterType.TEXT,
                DeconTools.Backend.Globals.MSFileType.Finnigan, x);

            Assert.AreEqual(typeof(MSFeatureToTextFileExporterBasic), exporter.GetType());

            bool fileWasCreated = (File.Exists(x));
            Assert.AreEqual(true, fileWasCreated);
        }

    }
}
