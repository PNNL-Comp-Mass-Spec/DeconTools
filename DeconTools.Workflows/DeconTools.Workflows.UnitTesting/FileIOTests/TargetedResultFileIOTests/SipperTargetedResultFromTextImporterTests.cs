using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.FileIOTests.TargetedResultFileIOTests
{
    [TestFixture]
    public class SipperTargetedResultFromTextImporterTests
    {
        [Test]
        public void importTest1()
        {
            string testFile =  FileRefs.ImportedData + Path.DirectorySeparatorChar + "ImportedSipperResults1.txt";
            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(testFile);

            var results=   importer.Import();

            Assert.IsNotNull(results);
            Assert.IsTrue(results.Results.Count > 0);

            var testResult1 = results.Results.First() as SipperLcmsFeatureTargetedResultDTO;

            Assert.IsNotNull(testResult1);
            Assert.IsNotNull(testResult1.LabelDistributionVals);

            Assert.IsTrue(testResult1.LabelDistributionVals.Length > 0);

            Console.WriteLine(testResult1.ToStringWithDetailsAsRow());



        }

    }
}
