using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Utilities;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.FileIO
{
    public class MSScanInfoExporterFactory
    {
        #region Constructors
        public MSScanInfoExporterFactory()
        {

        }
        #endregion

        #region Public Methods
        public static ExporterBase<ScanResult> CreateMSScanInfoExporterFactory(Globals.ExporterType exporterType, 
            Globals.MSFileType msFileType, string outputFileName)
        {
            Check.Assert(outputFileName != null || outputFileName.Length > 0, "MSScanInfoExporterFactory cannot work. OutputFileName is empty - which is bad.");
            ExporterBase<ScanResult> msFeatureExporter;


            switch (exporterType)
            {
                case Globals.ExporterType.Text:

                    switch (msFileType)
                    {
                        case Globals.MSFileType.PNNL_UIMF:
                            msFeatureExporter = new MSScanInfoToTextFileExporterUIMF(outputFileName);
                            break;
                        default:
                            msFeatureExporter = new MSScanInfoToTextFileExporterBasic(outputFileName);
                            break;
                    }

                    break;
                case Globals.ExporterType.Sqlite:

                    switch (msFileType)
                    {
                        case Globals.MSFileType.PNNL_UIMF:
                            msFeatureExporter = new MSScanInfoToSQLiteExporterUIMF(outputFileName);
                            break;
                        default:
                            msFeatureExporter = new MSScanInfoToSQLiteExporterBasic(outputFileName);
                            break;
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            return msFeatureExporter;


        }
        #endregion

        #region Private Methods
        #endregion
    }
}
