using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;

namespace DeconTools.UnitTesting
{
    [TestFixture]
    public class BonesProjectControllerTests
    {

        string testFile1 = @"F:\Gord\Data\Laskin\20_E_luna_neg.RAW";
        string scans100_120ParamFile = @"F:\Gord\Data\Laskin\LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1.xml";


        [Test]
        public void test1()
        {
            List<string>runList = new List<string>();
            runList.Add(testFile1);

            BonesProjectController controller = new BonesProjectController(runList, DeconTools.Backend.Globals.MSFileType.Finnigan, scans100_120ParamFile,3);
            
            Project.getInstance().Parameters.NumScansSummed = 9;

            controller.Execute();

            


        }


    }
}
