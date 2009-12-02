using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Data
{
    public class IsosExporterFactory
    {
        DeconTools.Backend.Globals.ExporterType exporterType;

        public IsosExporter CreateIsosExporter(Globals.MSFileType filetype, DeconTools.Backend.Globals.ExporterType exporterType, string outputFileName)
        {
            IsosExporter isosExporter;

            switch (filetype)
            {
                case Globals.MSFileType.Undefined:
                    isosExporter = new BasicIsosExporter(outputFileName);
                    break;
                case Globals.MSFileType.Agilent_TOF:
                    isosExporter = new BasicIsosExporter(outputFileName);
                    break;
                case Globals.MSFileType.Ascii:
                    isosExporter = new BasicIsosExporter(outputFileName);
                    break;
                case Globals.MSFileType.Bruker:
                    isosExporter = new BasicIsosExporter(outputFileName);
                    break;
                case Globals.MSFileType.Bruker_Ascii:
                    isosExporter = new BasicIsosExporter(outputFileName);
                    break;
                case Globals.MSFileType.Finnigan:
                    isosExporter = new BasicIsosExporter(outputFileName);
                    break;
                case Globals.MSFileType.ICR2LS_Rawdata:
                    isosExporter = new BasicIsosExporter(outputFileName);
                    break;
                case Globals.MSFileType.Micromass_Rawdata:
                    isosExporter = new BasicIsosExporter(outputFileName);
                    break;
                case Globals.MSFileType.MZXML_Rawdata:
                    isosExporter = new BasicIsosExporter(outputFileName);
                    break;
                case Globals.MSFileType.PNNL_IMS:
                    isosExporter = new IMFIsosExporter(outputFileName);
                    break;
                case Globals.MSFileType.PNNL_UIMF:
                    switch (exporterType)
                    {
                        case Globals.ExporterType.TEXT:
                            isosExporter = new UIMFIsosExporter(outputFileName);
                            break;
                        case Globals.ExporterType.SQLite:
                            isosExporter = new UIMFSQLiteIsosExporter(outputFileName);
                            break;
                        default:
                            isosExporter = new UIMFIsosExporter(outputFileName);
                            break;
                    }
                    break;
                case Globals.MSFileType.SUNEXTREL:
                    isosExporter = new BasicIsosExporter(outputFileName);
                    break;
                default:
                    isosExporter = new BasicIsosExporter(outputFileName);
                    break;
            }
            return isosExporter;

        }


    }
}
