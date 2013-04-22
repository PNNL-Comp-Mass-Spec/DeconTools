using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.FileIO;
using DeconTools.Workflows.Backend.Utilities;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.UtilitiesTests
{
    [TestFixture]
    public class PeakMatchedResultUtilitiesTests
    {
        [Test]
        public void loadresultsForDatasetTest1()
        {
            string dbserver = "pogo";
            string dbname = "MT_Yellowstone_Communities_P627";
            string tableName = @"[MT_Yellowstone_Communities_P627].[PNL\D3X720].[T_Tmp_Slysz_All_PeakMatchingResults]";

            string testDataset = "Yellow_C13_070_23Mar10_Griffin_10-01-28";

            PeakMatchedResultUtilities util = new PeakMatchedResultUtilities(dbserver, dbname, tableName);

            var importedResults = util.LoadResultsForDataset(testDataset);

            Assert.IsTrue(importedResults.Count > 0);
            Assert.AreEqual(4184, importedResults.Count);


            MassTagFromSqlDbImporter mtImporter = new MassTagFromSqlDbImporter(dbname,dbserver,importedResults.Select(p=>(long)p.MatchedMassTagID).Distinct().ToList());
            var targetCollection=  mtImporter.Import();

            StringBuilder sb=new StringBuilder();
            foreach (var target in targetCollection.TargetList )
            {
                sb.Append(target.ID + "\t" + target.Code + "\t" + target.EmpiricalFormula + "\n");
            }

            Console.WriteLine(sb.ToString());


        }

    }
}
