using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.UnitTesting2
{
    public class FileRefs
    {
        public static string RawDataBasePath = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles";
        public static string TestFileBasePath = @"..\\..\\..\\TestFiles";
        public static string OutputFolderPath = @"..\\..\\..\\TestFiles\OutputtedData";

        public class RawDataMSFiles
        {
            public static string AgilentDFile1 = FileRefs.RawDataBasePath + @"\AgilentD\BSA_TOF4\BSA_TOF4.D";
            public static string OrbitrapStdFile1 = FileRefs.RawDataBasePath + "\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            public static string MZXMLOrbitrapStdFile1 = FileRefs.RawDataBasePath + "\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mzxml";

            public static string UIMFStdFile1 = FileRefs.RawDataBasePath + "\\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf";
            public static string UIMFFileContainingMSMSLevelData = FileRefs.RawDataBasePath + "\\QC_Shew_MSMS_500_100_fr1200_c2_Ek_0000.uimf";

            public static string TestFileBasePath = @"..\\..\\..\\TestFiles";
            public static string Bruker9TStandardFile1 = FileRefs.RawDataBasePath + "\\SWT_9t_TestDS216_Small";
            public static string Bruker9TStandardFile1AlternateRef = FileRefs.RawDataBasePath + @"\SWT_9t_TestDS216_Small\0.ser\acqus";

            public static string BrukerSolarix12TFile1 = FileRefs.RawDataBasePath + @"\Bruker\Bruker_Solarix12T\12Ttest_000003";
            public static string BrukerSolarix12T_FID_File1 = FileRefs.RawDataBasePath + @"\Bruker\Bruker_Solarix12T\HVY_000001";
            public static string BrukerSolarix12T_FID_File2 = FileRefs.RawDataBasePath + @"\Bruker\Bruker_Solarix12T\\HVY_MSCAL_000001";

            public static string Bruker15TFile1 = FileRefs.RawDataBasePath + @"\Bruker\Bruker_15T\092410_ubiquitin_AutoCID_000004";

            public static string YAFMSStandardFile1 = FileRefs.RawDataBasePath + "\\QC_Shew_09_01_pt5_a_20Mar09_Earth_09-01-01.yafms";
            public static string YAFMSStandardFile2 = FileRefs.RawDataBasePath + "\\metabolite_eqd.yafms";
            public static string YAFMSStandardFile3 = FileRefs.RawDataBasePath + "\\3_c_elegans_eqd.yafms";


            public static string sarcUIMFFile1 = @"D:\Data\UIMF\Sarc\Sarc_MS_75_24Aug10_Cheetah_10-08-02_0000.uimf";

            //public static string BrukerSolarixFile1 =@"D:\Data\12Ttest_000003\ser";

        }

        public class PeakDataFiles
        {
            public static string OrbitrapOldDecon2LSPeakFile = FileRefs.RawDataBasePath + "\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_peaks.dat";
            public static string OrbitrapPeakFile1 = FileRefs.RawDataBasePath + "\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_FOR_REF_ONLY_peaks.txt";
            public static string OrbitrapPeakFile_scans5500_6500 = FileRefs.RawDataBasePath + "\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_scans5500-6500_peaks.txt";

        }





        public class ParameterFiles
        {
            public static string YAFMSParameterFileScans4000_4050 = FileRefs.RawDataBasePath + "\\LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1_Thrash_scans4000-4050.xml";

            public static string Bruker12TSolarixScans4_8ParamFile = FileRefs.RawDataBasePath + @"\ParameterFiles\Bruker_Solarix_SampleParameterFile.xml";

        }



    }
}
