using System;
using System.Linq;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.FileIOTests
{
    [TestFixture]
    public class IqTargetFromMsgfFirstHitsImporterTests
    {
        [Test]
        public void Test1()
        {
            var filename =
                @"\\proto-7\VOrbi05\2013_2\mhp_plat_test_1_14April13_Frodo_12-12-04\MSG201305011339_Auto939903\mhp_plat_test_1_14April13_Frodo_12-12-04_msgfdb_fht.txt";

            var importer = new IqTargetsFromFirstHitsFileImporter(filename);

            var targets = importer.Import().Take(50).ToList();

            foreach (IqTargetMsgfFirstHit iqTarget in targets)
            {
                Console.WriteLine(iqTarget.ID + "\t" + iqTarget.ScanLC + "\t" + iqTarget.MassError);
            }
        }
    }
}
