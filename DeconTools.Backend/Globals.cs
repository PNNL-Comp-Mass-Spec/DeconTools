using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend
{
    public class Globals
    {


        public enum XYDataFileType       //these filetypes are for files that contain a single set of XY values  (i.e. single MS Scan)
        {
            Undefined,
            Textfile,
            CSV_File
        }

        
        public enum MSFileType
        {
            Undefined,
            Agilent_TOF,
            Ascii,
            Bruker,
            Bruker_Ascii,
            Finnigan,
            ICR2LS_Rawdata,
            Micromass_Rawdata,
            MZXML_Rawdata,
            PNNL_IMS,
            PNNL_UIMF,
            SUNEXTREL

        }

        public enum PeakFitType
        {
            Undefined,
            APEX,
            LORENTZIAN,
            QUADRATIC
        }

        public enum IsotopicProfileFitType
        {
            Undefined,
            AREA,
            CHISQ,
            PEAK
        }
        public enum DeconState
        {
            IDLE = 0,
            RUNNING_DECON,
            RUNNING_TIC,
            DONE,
            ERROR
        }

        public enum DeconvolutionType
        {
            THRASH,
            RAPID,
            THRASH_then_RAPID
        }


        public enum ExporterType
        {
            TEXT,
            SQLite
        }


        public enum PeakSelectorMode            //for selecting the best peak from a list of peaks
        {
            CLOSEST_TO_TARGET,
            MOST_INTENSE,
            INTELLIGENT_MODE
        }

    }
}
