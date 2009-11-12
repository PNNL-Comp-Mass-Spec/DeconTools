using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Linq;
using DeconTools.Backend.Utilities;

namespace DeconTools.UnitTesting.UtilitiesTests
{
    [TestFixture]
    public class Logger_ImporterTests
    {
        string logtestfile = "..\\..\\TestFiles\\QC_Shew_0 5mg_12hr_1 8_600_100_fr3600_0000 uimf_log.txt";
        
        [Test]
        public void test1()
        {
            Logger_Importer.displayLogFile(logtestfile);
        }


    }
}
