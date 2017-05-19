using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.DTO;

namespace DeconTools.UnitTesting2.FileIO_Tests
{
    [TestFixture]
    public class PeakImporterFromOldDecon2LSDatFileTests
    {
        [Test]
        public void importDataFromOldDatFileTest1()
        {
            var testFile = FileRefs.PeakDataFiles.OrbitrapOldDecon2LSPeakFile;

            List<MSPeakResult>msPeakList=new List<MSPeakResult>();

            DeconTools.Backend.Data.Importers.PeakImporterFromOldPeakDatFile importer = new DeconTools.Backend.Data.Importers.PeakImporterFromOldPeakDatFile(testFile);
            importer.ImportPeaks(msPeakList);

            Assert.AreEqual(1934153, msPeakList.Count);

            var peaksForScan6005 = (from p in msPeakList where p.Scan_num==6005 select p).ToList();

            var sb = new StringBuilder();
            foreach (var peak in peaksForScan6005)
            {
                sb.Append(peak.PeakID);
                sb.Append("\t");
                sb.Append(peak.Scan_num);
                sb.Append("\t");
                sb.Append(peak.MSPeak.XValue.ToString("0.0000"));
                sb.Append("\t");
                sb.Append(peak.MSPeak.Height);
                sb.Append(Environment.NewLine);
            }
            Console.Write(sb.ToString());
        }


    }
}
