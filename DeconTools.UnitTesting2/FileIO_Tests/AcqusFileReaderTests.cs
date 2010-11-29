using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.FileIO;
using System.IO;

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
            Assert.AreEqual(370370.37037037, reader.DataLookupTable["SW_h"]);
            Assert.AreEqual(144378935.472081, reader.DataLookupTable["ML1"]);
            Assert.AreEqual(20.3413771463121, reader.DataLookupTable["ML2"]);
            Assert.AreEqual(524288, reader.DataLookupTable["TD"]);
            Assert.AreEqual(57741.1186793973, reader.DataLookupTable["FR_low"]);
            
            foreach (var item in reader.DataLookupTable)
            {
                Console.WriteLine(item.Key + "\t" + item.Value);
            }
        }

        [Test]
        public void exceptionTest1()
        {
            var ex = Assert.Throws<FileNotFoundException>(() => new AcqusFileReader(acqusFile1 + ".txt"));
            Assert.That(ex.Message, Is.EqualTo("Could not read file. File not found."));
        }

    }
}
