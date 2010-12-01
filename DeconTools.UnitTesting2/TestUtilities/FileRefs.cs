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
            public static string TestFileBasePath = @"..\\..\\..\\TestFiles";


            public static string AgilentDFile1 = FileRefs.RawDataBasePath + @"\AgilentD\BSA_TOF4\BSA_TOF4.D";

            public static string IMFStdFile1 = FileRefs.RawDataBasePath + @"\IMF\50ugpmlBSA_CID_QS_16V_0000.Accum_1.IMF";

            public static string TextFileMS_std1 = FileRefs.RawDataBasePath + @"\MassSpectra_TextFiles\50ugpmlBSA_CID_QS_16V_0000.Accum_1_SCAN233_raw_data.txt";
            public static string TextFileMS_multipleHeaderLines = FileRefs.RawDataBasePath + @"\MassSpectra_TextFiles\sampleIMS_Data_09103034.TXT";
            public static string TextFileMS_multipleDelimiters = FileRefs.RawDataBasePath + @"\MassSpectra_TextFiles\DN_sym5_A1_87.txt";
            
            public static string OrbitrapStdFile1 = FileRefs.RawDataBasePath + "\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
            public static string MZXMLOrbitrapStdFile1 = FileRefs.RawDataBasePath + "\\mzXML\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mzxml";

            public static string UIMFStdFile1 = FileRefs.RawDataBasePath + "\\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf";
            public static string UIMFFileContainingMSMSLevelData = FileRefs.RawDataBasePath + "\\QC_Shew_MSMS_500_100_fr1200_c2_Ek_0000.uimf";

            
            public static string Bruker9TStandardFile1 = FileRefs.RawDataBasePath + "\\SWT_9t_TestDS216_Small";
            public static string Bruker9TStandardFile2 = FileRefs.RawDataBasePath + "\\Bruker\\Bruker_9T\\RSPH_Aonly_01_run1_11Oct07_Andromeda_07-09-02";
            public static string Bruker9TStandardFile1AlternateRef = FileRefs.RawDataBasePath + @"\SWT_9t_TestDS216_Small\0.ser\acqus";

            public static string BrukerSolarix12TFile1 = FileRefs.RawDataBasePath + @"\Bruker\Bruker_Solarix12T\12Ttest_000003";
            public static string BrukerSolarix12T_FID_File1 = FileRefs.RawDataBasePath + @"\Bruker\Bruker_Solarix12T\HVY_000001";
            public static string BrukerSolarix12T_FID_File2 = FileRefs.RawDataBasePath + @"\Bruker\Bruker_Solarix12T\\HVY_MSCAL_000001";
            public static string BrukerSolarix12T_dotD_File1 = FileRefs.RawDataBasePath + @"\Bruker\Bruker_Solarix12T\Oct8_2010_BSA\BSA_10082010_000003.d";

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

            public static string Bruker9T_Scans1000_1010ParamFile = FileRefs.RawDataBasePath + @"\ParameterFiles\Bruker_9T_ParameterFile.xml";

            public static string UIMFFrames800_802 = FileRefs.RawDataBasePath + @"\ParameterFiles\UIMF_frames_peakBR7_800-802.xml";


        }



    }
}
