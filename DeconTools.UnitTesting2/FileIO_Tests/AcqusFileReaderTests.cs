using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.FileIO;

namespace DeconTools.UnitTesting2.FileIO_Tests
{
    [TestFixture]
    public class AcqusFileReaderTests
    {
        string acqusFile1 = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Bruker\Bruker_9T\RSPH_Aonly_01_run1_11Oct07_Andromeda_07-09-02\acqus";

        [Test]
        public void loadAcqusFile_Test1()
        {
            AcqusFileReader reader = new AcqusFileReader(acqusFile1);

            foreach (var item in reader.DataLookupTable)
            {
                Console.WriteLine(item.Key + "\t" + item.Value);

                
            }
        }

    }
}
