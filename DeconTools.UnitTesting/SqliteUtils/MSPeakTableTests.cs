using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;
using DeconTools.SqliteUtils;
using System.Diagnostics;

namespace DeconTools.UnitTesting.SqliteUtils
{
    [TestFixture]
    public class MSPeakTableTests
    {
        string outputSqlDBFileName = "..\\..\\Testfiles\\mspeaksDB.sqlite";

        [Test]
        public void buildtableStringTest1()
        {
            DeconTools.SqliteUtils.MSPeakTable mspeakTable = new DeconTools.SqliteUtils.MSPeakTable("T_Peaks");
            Assert.AreEqual("CREATE TABLE T_Peaks (peak_id INTEGER PRIMARY KEY, scan_num INTEGER, mz DOUBLE, intensity FLOAT, fwhm FLOAT);",
                mspeakTable.BuildCreateTableString());

        }

        [Test]
        public void createMSPeaksTableTest1()
        {
            if (File.Exists(outputSqlDBFileName)) File.Delete(outputSqlDBFileName);

            DeconTools.SqliteUtils.MSPeakTable mspeakTable = new DeconTools.SqliteUtils.MSPeakTable("T_peaks");
            List<DeconTools.SqliteUtils.Table> tableList = new List<DeconTools.SqliteUtils.Table>();
            tableList.Add(mspeakTable);

            SqliteDB db = DeconTools.SqliteUtils.SqlDBUtils.CreateDB(outputSqlDBFileName, tableList);
            Assert.AreEqual(System.Data.ConnectionState.Open, db.Conn.State);


            
            db.Conn.Close();
            Assert.AreEqual(System.Data.ConnectionState.Closed, db.Conn.State);
          
            db.Dispose();

        }

        [TearDown]
        public void teardown1()
        {
            
            
        }

        public void createAndFillMSPeaksTableTest()
        {
            if (File.Exists(outputSqlDBFileName)) File.Delete(outputSqlDBFileName);

            DeconTools.SqliteUtils.MSPeakTable mspeakTable = new DeconTools.SqliteUtils.MSPeakTable("T_peaks");
            List<DeconTools.SqliteUtils.Table> tableList = new List<DeconTools.SqliteUtils.Table>();
            tableList.Add(mspeakTable);

            SqliteDB db = DeconTools.SqliteUtils.SqlDBUtils.CreateDB(outputSqlDBFileName, tableList);
            Assert.AreEqual(System.Data.ConnectionState.Open, db.Conn.State);

            //  db.Conn.CreateCommand()


        }



        [Test]
        public void createMSPeaksTableTest2()
        {
            if (File.Exists(outputSqlDBFileName)) File.Delete(outputSqlDBFileName);
            DeconTools.SqliteUtils.MSPeakTable mspeakTable = new DeconTools.SqliteUtils.MSPeakTable("T_peaks");
            List<DeconTools.SqliteUtils.Table> tableList = new List<DeconTools.SqliteUtils.Table>();
            tableList.Add(mspeakTable);

            DeconTools.SqliteUtils.SqlDBUtils.CreateDB(outputSqlDBFileName, tableList);


        }



    }
}
