using NUnit.Framework;

namespace DeconConsole.UnitTesting
{
    [TestFixture]
    public class DeconConsoleTests
    {
        private string koreaImsTestFile1 = @"F:\Gord\Data\Sang-Won\sampleIMS_Data_09103034.TXT";
        private string koreaParameterFile1 = @"F:\Gord\Data\Sang-Won\decon_params.xml";

        private string agilentDFile1 = @"\\proto-2\unitTest_Files\DeconTools_TestFiles\AgilentD\BSA_TOF4\BSA_TOF4.d";
        private string agilentDOutputFolder = @"\\proto-2\unitTest_Files\DeconTools_TestFiles\AgilentD\";
        private string agilentDParamFile = @"\\proto-2\unitTest_Files\DeconTools_TestFiles\ParameterFiles\AgilentD_BSA_Scans1-10.xml";

        [Test]
        public void test1()
        {

            var args = new string[3];
            args[0] = agilentDFile1;
            args[1] = agilentDParamFile;
            args[2] = agilentDOutputFolder;

            DeconConsole.Program.Main(args);

        }


        [Test]
        [Ignore("Local files")]
        public void test2()
        {
            var args = new string[4];
            args[0] = koreaImsTestFile1;
            args[1] = "ascii";
            args[2] = koreaParameterFile1;
            args[3] = "korea_ims_custom1";

            DeconConsole.Program.Main(args);


        }




    }
}
