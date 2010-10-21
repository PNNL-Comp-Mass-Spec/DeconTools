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

        string agilentDFile1 = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\AgilentD\BSA_TOF4\BSA_TOF4.d";
        string agilentDParamFile = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\AgilentD_BSA_Scans1-10.xml";
        
        [Test]
        public void test1()
        {

            string[] args = new string[3];
            args[0] = agilentDFile1;
            args[1] = "Agilent_D";
            args[2] = agilentDParamFile;

            DeconConsole.Program.Main(args);





        }


        [Test]
        public void test2()
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
