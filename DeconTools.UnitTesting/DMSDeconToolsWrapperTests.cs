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
        string uimfFilepath2 = "..\\..\\TestFiles\\QC_Shew_0.25mg_4T_1.6_600_335_50ms_fr2400_adc_0000_V2009_05_28.uimf";
        string uimfFile3 = "..\\..\\TestFiles\\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf";

        string uimfParameterFile2 = "..\\..\\TestFiles\\uimfParameterFile_DMSWrapper1.xml";
        string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        string xcaliburParameterFile3 = "..\\..\\TestFiles\\xcaliburParameterFile3.xml";

        string outputfilename = "..\\..\\..\\TestFiles\\wrapperOutputFilename1.csv";

        [Test]
        public void UIMFProcessing_test1()
        {
            DMSDecon2LSWrapper wrapper = new DMSDecon2LSWrapper();
            wrapper.DataFile = uimfFile3;
            wrapper.ParameterFile = uimfParameterFile2;
            wrapper.FileType = DeconToolsV2.Readers.FileType.PNNL_UIMF;

            wrapper.OutFile = outputfilename;
            wrapper.Deconvolute();

        }

        [Test]
        public void test2()
        {


            DMSDecon2LSWrapper wrapper = new DMSDecon2LSWrapper();
            wrapper.DataFile = xcaliburTestfile;
            wrapper.ParameterFile = xcaliburParameterFile3;
            wrapper.FileType = DeconToolsV2.Readers.FileType.FINNIGAN;
            wrapper.OutFile = outputfilename;
            wrapper.Deconvolute();

        }


    }
}
