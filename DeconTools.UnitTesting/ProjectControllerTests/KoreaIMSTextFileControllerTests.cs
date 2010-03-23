using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.ProjectControllers;

namespace DeconTools.UnitTesting.ProjectControllerTests
{
    [TestFixture]
    public class KoreaIMSTextFileControllerTests
    {

        string testfile1 = @"F:\Gord\Data\Sang-Won\sampleIMS_Data_09103034.TXT";

        string paramFile1 = @"F:\Gord\Data\Sang-Won\decon_params.xml";

        [Test]
        public void test1()
        {

            KoreaIMSTextFileProjectController controller = new KoreaIMSTextFileProjectController(testfile1, DeconTools.Backend.Globals.MSFileType.Ascii, paramFile1, null);
            controller.Execute();


        }


    }
}
