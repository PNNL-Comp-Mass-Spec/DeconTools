using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DMSDeconToolsV2;


namespace DeconTools.UnitTesting
{
    [TestFixture]
    public class DMSDeconToolsWrapperTests
    {
        public string uimfFilepath2 = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000_V2009_05_28.uimf";
        public string uimfParameterFile2 = "..\\..\\TestFiles\\oldSchoolProcRunnerParameterTestFile2.xml";

        public string outputfilename = "..\\..\\..\\TestFiles\\wrapperOutputFilename1.csv";

        [Test]
        public void test1()
        {
            DMSDecon2LSWrapper wrapper = new DMSDecon2LSWrapper();
            wrapper.DataFile = uimfFilepath2;
            wrapper.ParameterFile = uimfParameterFile2;
            wrapper.FileType = DeconToolsV2.Readers.FileType.PNNL_UIMF;

            wrapper.OutFile = outputfilename;
            wrapper.Deconvolute();

        }


    }
}
