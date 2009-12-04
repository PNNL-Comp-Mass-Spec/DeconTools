using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Diagnostics;
using DeconTools.Backend.ProcessingTasks.PeakListExporters;
using DeconTools.Utilities.SqliteUtils;

namespace DeconTools.UnitTesting.SqliteUtils
{
    [TestFixture]
    public class MSPeakTableTests
    {
        string outputSqlDBFileName = "..\\..\\Testfiles\\mspeaksDB.sqlite";

        [Test]
        public void buildtableStringTest1()
        {
            Table mspeakTable = new MSPeakTable("T_Peaks");
            Assert.AreEqual("CREATE TABLE T_Peaks (peak_id INTEGER PRIMARY KEY, scan_num INTEGER, mz DOUBLE, intensity FLOAT, fwhm FLOAT);",
                mspeakTable.BuildCreateTableString());

        }

    
        public void createAndFillMSPeaksTableTest()
        {
            if (File.Exists(outputSqlDBFileName)) File.Delete(outputSqlDBFileName);

            MSPeakTable mspeakTable =new MSPeakTable("T_peaks");
            List<Table> tableList = new List<Table>();
            tableList.Add(mspeakTable);

    
        }



        [Test]
        public void createMSPeaksTableTest2()
        {
            if (File.Exists(outputSqlDBFileName)) File.Delete(outputSqlDBFileName);
            MSPeakTable mspeakTable = new MSPeakTable("T_peaks");
            List<Table> tableList = new List<Table>();
            tableList.Add(mspeakTable);


        }



    }
}
