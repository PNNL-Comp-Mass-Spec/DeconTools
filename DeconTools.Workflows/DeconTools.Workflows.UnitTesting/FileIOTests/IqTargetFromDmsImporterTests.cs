﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.FileIOTests
{
    [TestFixture]
    public class IqTargetFromDmsImporterTests
    {
        [Test]
        public void ImportMassTagsByPmtQualityScore()
        {
            string server = "pogo";
            string db = "MT_Mouse_MHP_O18_Set1_P890";

            IqTargetFromDmsImporter importer = new IqTargetFromDmsImporter(server, db);

            List<IqTarget> targets=  importer.Import();

            int[] testTargets = {20822620, 47328056};

            var duplicateTargets = (from n in targets where testTargets.Contains(n.ID) select n).ToList();

            foreach (var duplicateTarget in duplicateTargets)
            {
                Console.WriteLine(duplicateTarget);
            }

            var duplicatesRemoved = (from n in targets
                                     group n by new
                                                    {
                                                        n.Code,
                                                        n.EmpiricalFormula
                                                    }
                                     into grp
                                     select grp.First()).ToList();


            string targetsFilename =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\O16O18_standard_testing\" + db + "_targets.txt";

            importer.SaveIqTargetsToFile(targetsFilename, duplicatesRemoved.ToList());

        }

    }
}
