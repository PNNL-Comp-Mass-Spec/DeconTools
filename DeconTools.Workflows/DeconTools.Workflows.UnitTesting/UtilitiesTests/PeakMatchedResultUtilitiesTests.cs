﻿using System;
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
            var dbserver = "pogo";
            var dbname = "MT_Yellowstone_Communities_P627";
            var tableName = @"[MT_Yellowstone_Communities_P627].[PNL\D3X720].[T_Tmp_Slysz_All_PeakMatchingResults]";

            var testDataset = "Yellow_C13_070_23Mar10_Griffin_10-01-28";

            var util = new PeakMatchedResultUtilities(dbserver, dbname, tableName);

            var importedResults = util.LoadResultsForDataset(testDataset);

            Assert.IsTrue(importedResults.Count > 0);
            Assert.AreEqual(4184, importedResults.Count);

            var mtImporter = new MassTagFromSqlDbImporter(dbname,dbserver,importedResults.Select(p=>(long)p.MatchedMassTagID).Distinct().ToList());
            var targetCollection=  mtImporter.Import();

            var sb=new StringBuilder();
            foreach (var target in targetCollection.TargetList )
            {
                sb.Append(target.ID).Append('\t').Append(target.Code).Append('\t').Append(target.EmpiricalFormula).Append('\n');
            }

            Console.WriteLine(sb.ToString());
        }
    }
}
