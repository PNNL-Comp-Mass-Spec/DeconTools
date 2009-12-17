using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Data;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using System.Linq;
using System.Diagnostics;

namespace DeconTools.UnitTesting
{
    [TestFixture]
    public class PeakDataImportersTests
    {
        private string sqlitefileName1 = "..\\..\\TestFiles\\TestPeakData_ForImportTests.sqlite";
        private string sqlitefileName2 = @"F:\Gord\Data\N14N15\HuttlinTurnover\RSPH_Aonly_22_run2_13Jan08_Raptor_07-11-11\RSPH_Aonly_22_run2_13Jan08_Raptor_07-11-11_peaks.sqlite";

        private string textFilename = "..\\..\\TestFiles\\PeakListImporterTest1.txt";
        private string textFilename2 = @"F:\Gord\Data\N14N15\HuttlinTurnover\RSPH_Aonly_22_run2_13Jan08_Raptor_07-11-11\RSPH_Aonly_22_run2_13Jan08_Raptor_07-11-11_peaks.txt";

        [Test]
        public void importPeaksFromSqlite()
        {
            string testFile = sqlitefileName1;
         
            List<MSPeakResult>peakList=new List<MSPeakResult>();

            PeakImporterFromSQLite importer = new PeakImporterFromSQLite(testFile);
            importer.ImportPeaks(peakList);

            Assert.AreEqual(1096, peakList[1095].PeakID);
            Assert.AreEqual(6005, peakList[1095].Scan_num);
            Assert.AreEqual(754.37393, (decimal)peakList[1095].MSPeak.MZ);
            Assert.AreEqual(3266908, (decimal)peakList[1095].MSPeak.Intensity);
            Assert.AreEqual(0.01484067, (decimal)peakList[1095].MSPeak.FWHM);


            Assert.AreEqual(4088, peakList.Count);
        }

        [Test]
        public void importPeaksFromText()
        {
            string testFile = textFilename;

            List<MSPeakResult> peakList = new List<MSPeakResult>();

            PeakImporterFromText importer = new PeakImporterFromText(testFile);
            importer.ImportPeaks(peakList);

            Assert.AreEqual(1096, peakList[1095].PeakID);
            Assert.AreEqual(6005, peakList[1095].Scan_num);
            Assert.AreEqual(754.37393, (decimal)peakList[1095].MSPeak.MZ);
            Assert.AreEqual(3266908,(decimal)peakList[1095].MSPeak.Intensity);
            Assert.AreEqual(0.0148,(decimal) peakList[1095].MSPeak.FWHM);
            Assert.AreEqual(32.92,(decimal) peakList[1095].MSPeak.SN);


            Assert.AreEqual(4088, peakList.Count);
        }


        [Test]
        public void importPeaksLargeFileTest1()
        {
            string testFile = sqlitefileName2;

            List<MSPeakResult> peakList = new List<MSPeakResult>();

            PeakImporterFromSQLite importer = new PeakImporterFromSQLite(testFile);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            importer.ImportPeaks(peakList);
            sw.Stop();
            Console.WriteLine("Import time = " + sw.ElapsedMilliseconds);
            sw.Reset();
            Assert.AreEqual(4852790, peakList.Count);
            sw.Start();
            List<MSPeakResult>testList= peakList.Where(p => p.MSPeak.MZ > 700.01 && p.MSPeak.MZ < 700.03).ToList();
            sw.Stop();

            Console.WriteLine("test chrom time = " + sw.ElapsedMilliseconds);
            sw.Reset();
            sw.Start();
            List<MSPeakResult> testList2 = peakList.Where(p => p.MSPeak.MZ > 891.01 && p.MSPeak.MZ < 891.03).ToList();
            sw.Stop();
            Console.WriteLine("test chrom time = " + sw.ElapsedMilliseconds);

            reportChromatogram(testList);

        }

        [Test]
        public void importPeaksLargeFileTest2()
        {
            string testFile = textFilename2;

            List<MSPeakResult> peakList = new List<MSPeakResult>();

            PeakImporterFromText importer = new PeakImporterFromText(testFile);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            importer.ImportPeaks(peakList);
            sw.Stop();
            Console.WriteLine("Import time = " + sw.ElapsedMilliseconds);
            sw.Reset();
            Assert.AreEqual(4852790, peakList.Count);
            sw.Start();
            List<MSPeakResult> testList = peakList.Where(p => p.MSPeak.MZ > 700.01 && p.MSPeak.MZ < 700.03).ToList();
            sw.Stop();
            Console.WriteLine("test chrom time = " + sw.ElapsedMilliseconds);
            sw.Reset();
            sw.Start();
            List<MSPeakResult> testList2 = peakList.Where(p => p.MSPeak.MZ > 891.01 && p.MSPeak.MZ < 891.03).ToList();
            sw.Stop();
            Console.WriteLine("test chrom time = " + sw.ElapsedMilliseconds);

            reportChromatogram(testList);

        }


        private void reportChromatogram(List<MSPeakResult> testList)
        {
            Console.WriteLine();
            Console.WriteLine("------------------------------------------------------------------------");
            StringBuilder sb = new StringBuilder();
            sb.Append("scan\tintens\n");

            foreach (MSPeakResult peak in testList)
            {
                sb.Append(peak.Scan_num);
                sb.Append("\t");
                sb.Append(peak.MSPeak.Intensity);
                sb.Append(Environment.NewLine);

                
            }
            Console.WriteLine(sb.ToString());
            Console.WriteLine("------------------------------------------------------------------------");

        }


    }
}
