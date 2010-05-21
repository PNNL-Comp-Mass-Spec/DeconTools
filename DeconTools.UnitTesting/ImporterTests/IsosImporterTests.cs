using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.Data;
using DeconTools.Backend.Core;

namespace DeconTools.UnitTesting.ImporterTests
{
    [TestFixture]
    public class IsosImporterTests
    {

        string xcaliburIsosResults1 = "..\\..\\TestFiles\\xcaliburScan6005-6050_isos.csv";
        string uimfIsosResults1 = "..\\..\\TestFiles\\uimfFrame400_isos.csv";



        [Test]
        public void xcaliburResultsTest1()
        {
            List<IsosResult>results=new List<IsosResult>();
            IsosImporter importer = new IsosImporter(xcaliburIsosResults1, DeconTools.Backend.Globals.MSFileType.Finnigan);
            importer.Import(results);

            List<IsosResult> flaggedResults = (from n in results where n.Flags.Count > 0 select n).ToList();
            Assert.AreEqual(119, flaggedResults.Count);
        }

        [Test]
        public void uimfResultsTest1()
        {
            List<IsosResult> results = new List<IsosResult>();
            IsosImporter importer = new IsosImporter(uimfIsosResults1, DeconTools.Backend.Globals.MSFileType.PNNL_UIMF);
            importer.Import(results);

            List<IsosResult> flaggedResults = (from n in results where n.Flags.Count > 0 select n).ToList();

            Assert.AreEqual(3177, results.Count);
            Assert.AreEqual(1253, flaggedResults.Count);
        }



    }
}
