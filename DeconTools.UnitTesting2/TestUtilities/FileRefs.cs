using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.UnitTesting2
{
    public class FileRefs
    {
        public static string RawDataBasePath = @"\\protoapps\UserData\Slysz\DeconTools_TestFiles\";

        public static string OrbitrapStdFile1 = FileRefs.RawDataBasePath + "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        public static string UIMFStdFile1 = FileRefs.RawDataBasePath + "35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf";

        public static string TestFileBasePath = @"..\\..\\..\\TestFiles";

        public static string Bruker9TStandardFile1 = FileRefs.RawDataBasePath + "SWT_9t_TestDS216_Small";

        public static string YAFMSStandardFile1 = FileRefs.RawDataBasePath + "QC_Shew_09_01_pt5_a_20Mar09_Earth_09-01-01.yafms";



        public class ParameterFiles
        {
            public static string YAFMSParameterFileScans6000_6050 = FileRefs.RawDataBasePath + "LTQ_Orb_SN2_PeakBR1pt3_PeptideBR1_Thrash_scans6000-6050.xml";
        }



    }
}
