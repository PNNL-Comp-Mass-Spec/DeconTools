﻿using System.IO;

namespace DeconTools.UnitTesting2
{
    public class FileRefs
    {
        public static string RawDataBasePath = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles";
        public static string TestFileBasePath = @"..\..\..\..\TestFiles";
        public static string OutputDirectoryPath = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\Output";

        public class RawDataMSFiles
        {
            public static string AgilentDFile1 = Path.Combine(RawDataBasePath, "AgilentD", "BSA_TOF4", "BSA_TOF4.D");

            public static string IMFStdFile1 = Path.Combine(RawDataBasePath, "IMF", "50ugpmlBSA_CID_QS_16V_0000.Accum_1.IMF");

            public static string TextFileMS_std1 = Path.Combine(RawDataBasePath, "MassSpectra_TextFiles", "50ugpmlBSA_CID_QS_16V_0000.Accum_1_SCAN233_raw_data.txt");
            public static string TextFileMS_multipleHeaderLines = Path.Combine(RawDataBasePath, "MassSpectra_TextFiles", "sampleIMS_Data_09103034.TXT");
            public static string TextFileMS_multipleDelimiters = Path.Combine(RawDataBasePath, "MassSpectra_TextFiles", "DN_sym5_A1_87.txt");

            public static string OrbitrapStdFile1 = Path.Combine(RawDataBasePath, "Orbitrap", "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW");
            public static string MZXMLOrbitrapStdFile1 = Path.Combine(RawDataBasePath, "mzXML", "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.mzxml");

            public static string UIMFStdFile1 = Path.Combine(RawDataBasePath, "35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf");
            public static string UIMFStdFile2 = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\Sarc_MS_90_21Aug10_Cheetah_10-08-02_0000.uimf";
            public static string UIMFStdFile3 = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\Sarc_MS2_90_6Apr11_Cheetah_11-02-19.uimf";

            public static string UIMFStdFile4 =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\Sarc_P09_B06_0786_20Jul11_Cheetah_11-05-31.uimf";

            public static string UIMFFileContainingMSMSLevelData =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\MSMS_Testing\PepMix_MSMS_4msSA.UIMF";

            public static string Bruker9TStandardFile1 = Path.Combine(RawDataBasePath, "SWT_9t_TestDS216_Small");
            public static string Bruker9TStandardFile2 = Path.Combine(RawDataBasePath, "Bruker", "Bruker_9T", "RSPH_Aonly_01_run1_11Oct07_Andromeda_07-09-02");
            public static string Bruker9TStandardFile1AlternateRef = Path.Combine(RawDataBasePath, "SWT_9t_TestDS216_Small", "0.ser", "acqus");

            public static string BrukerSolarix12TFile1 = Path.Combine(RawDataBasePath, "Bruker", "Bruker_Solarix12T", "12Ttest_000003");
            public static string BrukerSolarix12T_FID_File1 = Path.Combine(RawDataBasePath, "Bruker", "Bruker_Solarix12T", "HVY_000001");
            public static string BrukerSolarix12T_FID_File2 = Path.Combine(RawDataBasePath, "Bruker", "Bruker_Solarix12T", "HVY_MSCAL_000001");
            public static string BrukerSolarix12T_dotD_File1 = Path.Combine(RawDataBasePath, "Bruker", "Bruker_Solarix12T", "Oct8_2010_BSA", "BSA_10082010_000003.d");

            public static string Bruker15TFile1 = Path.Combine(RawDataBasePath, "Bruker", "Bruker_15T", "092410_ubiquitin_AutoCID_000004");

            public static string YAFMSStandardFile1 = Path.Combine(RawDataBasePath, "QC_Shew_09_01_pt5_a_20Mar09_Earth_09-01-01.yafms");
            public static string YAFMSStandardFile2 = Path.Combine(RawDataBasePath, "metabolite_eqd.yafms");
            public static string YAFMSStandardFile3 = Path.Combine(RawDataBasePath, "3_c_elegans_eqd.yafms");

            public static string sarcUIMFFile1 = @"D:\Data\UIMF\Sarc\the_10_testDatasets\Sarc_MS_90_21Aug10_Cheetah_10-08-02_0000.uimf";

            //public static string BrukerSolarixFile1 =@"D:\Data\12Ttest_000003\ser";

            public static string VOrbiFile1 = Path.Combine(RawDataBasePath, "Orbitrap", "Vorbi", "Yellow_C12_099_18Mar10_Griffin_10-01-13.raw");
        }

        public class PeakDataFiles
        {
            public static string OrbitrapOldDecon2LSPeakFile = Path.Combine(RawDataBasePath, "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_peaks.dat");
            public static string OrbitrapPeakFile1 = Path.Combine(RawDataBasePath, "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_FOR_REF_ONLY_peaks.txt");
            public static string OrbitrapPeakFile_scans5500_6500 = Path.Combine(RawDataBasePath, "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_scans5500-6500_peaks.txt");
        }

        public class ParameterFiles
        {
            public static string YAFMSParameterFileScans4000_4050 =
                @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\ParameterFiles\LTQ_Orb_SN2_PeakBR5_PeptideBR1_Thrash_scans4000_4050.xml";

            public static string Bruker12TSolarixScans4_8ParamFile = Path.Combine(RawDataBasePath, "ParameterFiles", "Bruker_12T_Solarix_Scans4-8.xml");

            public static string Bruker9T_Scans1000_1010ParamFile = Path.Combine(RawDataBasePath, "ParameterFiles", "Bruker_9T_ParameterFile.xml");

            public static string UIMFFrames800_802 = Path.Combine(RawDataBasePath, "ParameterFiles", "UIMF_frames_peakBR7_800-802.xml");

            public static string Orbitrap_Scans6000_6050ParamFile = Path.Combine(RawDataBasePath, "ParameterFiles", "LTQ_Orb", "LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1_Thrash_scans6000_6050.xml");
        }

        public static string BasicTargetedWorkflowPARAMETERS_EXPORTED_TESTFILE1 = Path.Combine(TestFileBasePath, "BasicTargetedWorkflowParameters_exported1.xml");
    }
}
