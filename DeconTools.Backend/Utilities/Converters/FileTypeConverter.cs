using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Utilities.Converters
{
#if !Disable_DeconToolsV2

    public static class FileTypeConverter
    {
        public static DeconTools.Backend.Globals.MSFileType ConvertDeconEngineFileType(DeconToolsV2.Readers.FileType filetype)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            switch (filetype)
            {
                case DeconToolsV2.Readers.FileType.AGILENT_TOF:
                    return Globals.MSFileType.Agilent_WIFF;
                case DeconToolsV2.Readers.FileType.ASCII:
                    return Globals.MSFileType.Ascii;
                case DeconToolsV2.Readers.FileType.BRUKER:
                    return Globals.MSFileType.Bruker;
                case DeconToolsV2.Readers.FileType.BRUKER_ASCII:
                    return Globals.MSFileType.Bruker_Ascii;
                case DeconToolsV2.Readers.FileType.FINNIGAN:
                    return Globals.MSFileType.Thermo_Raw;
                case DeconToolsV2.Readers.FileType.ICR2LSRAWDATA:
                    return Globals.MSFileType.ICR2LS_Rawdata;
                case DeconToolsV2.Readers.FileType.MICROMASSRAWDATA:
                    return Globals.MSFileType.Micromass_Rawdata;
                case DeconToolsV2.Readers.FileType.MZXMLRAWDATA:
                    return Globals.MSFileType.MZXML_Rawdata;
                case DeconToolsV2.Readers.FileType.PNNL_IMS:
                    return Globals.MSFileType.PNNL_IMS;
                case DeconToolsV2.Readers.FileType.PNNL_UIMF:
                    return Globals.MSFileType.PNNL_UIMF;
                case DeconToolsV2.Readers.FileType.SUNEXTREL:
                    return Globals.MSFileType.SUNEXTREL;
                case DeconToolsV2.Readers.FileType.UNDEFINED:
                    return Globals.MSFileType.Undefined;
                default:
                    return Globals.MSFileType.Undefined;
            }
#pragma warning restore CS0618 // Type or member is obsolete

        }

    }
#endif
}
