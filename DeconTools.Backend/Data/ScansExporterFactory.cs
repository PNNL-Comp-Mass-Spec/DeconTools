using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Data
{
    public class ScansExporterFactory
    {
        DeconTools.Backend.Globals.ExporterType exporterType;

        public ScansExporter CreateScansExporter(Globals.MSFileType fileType, string outputFileName)
        {
            ScansExporter scansExporter;
            switch (fileType)
            {
                case Globals.MSFileType.Undefined:
                    scansExporter = new BasicScansExporter(outputFileName);
                    break;
                case Globals.MSFileType.Agilent_TOF:
                    scansExporter = new BasicScansExporter(outputFileName);
                    break;
                case Globals.MSFileType.Ascii:
                    scansExporter = new BasicScansExporter(outputFileName);
                    break;
                case Globals.MSFileType.Bruker:
                    scansExporter = new BasicScansExporter(outputFileName);
                    break;
                case Globals.MSFileType.Bruker_Ascii:
                    scansExporter = new BasicScansExporter(outputFileName);
                    break;
                case Globals.MSFileType.Finnigan:
                    scansExporter = new BasicScansExporter(outputFileName);
                    break;
                case Globals.MSFileType.ICR2LS_Rawdata:
                    scansExporter = new BasicScansExporter(outputFileName);
                    break;
                case Globals.MSFileType.Micromass_Rawdata:
                    scansExporter = new BasicScansExporter(outputFileName);
                    break;
                case Globals.MSFileType.MZXML_Rawdata:
                    scansExporter = new BasicScansExporter(outputFileName);
                    break;
                case Globals.MSFileType.PNNL_IMS:
                    scansExporter = new BasicScansExporter(outputFileName);
                    break;
                case Globals.MSFileType.PNNL_UIMF:

                    switch (exporterType)
                    {
                        case Globals.ExporterType.TYPICAL:
                            scansExporter = new UIMFScansExporter(outputFileName);
                            break;
                        case Globals.ExporterType.ANOOP_OrigIntensityExporter:
                            scansExporter = new UIMFScansExporter(outputFileName);
                            break;
                        case Globals.ExporterType.SQLite:
                            scansExporter = new UIMFSQLiteScansExporter(outputFileName);
                            break;
                        default:
                            scansExporter = new UIMFScansExporter(outputFileName);
                            break;
                    }
                    break;
                case Globals.MSFileType.SUNEXTREL:
                    scansExporter = new BasicScansExporter(outputFileName);
                    break;
                default:
                    scansExporter = new BasicScansExporter(outputFileName);
                    break;
            }
            return scansExporter;

        }



    }
}
