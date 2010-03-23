using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace DeconConsole.UnitTesting
{
    [TestFixture]
    public class DeconConsoleTests
    {
        string koreaImsTestFile1 = @"F:\Gord\Data\Sang-Won\sampleIMS_Data_09103034.TXT";
        string koreaParameterFile1 = @"F:\Gord\Data\Sang-Won\decon_params.xml";
        
        
        [Test]
        public void test1()
        {

            string[] args = new string[4];
            args[0] = koreaImsTestFile1;
            args[1] = "ascii";
            args[2] = koreaParameterFile1;
            args[3] = "korea_ims_custom1";

            DeconConsole.Program.Main(args);





        }


    }
}
