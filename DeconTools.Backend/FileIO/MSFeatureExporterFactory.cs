using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.FileIO
{

    /// <summary>
    /// This helper class will create the appropriate MS Feature (isosResult) Exporter when given info on
    /// fileType and dataset type. 
    /// </summary>
    public class MSFeatureExporterFactory
    {
     
        #region Constructors
        public MSFeatureExporterFactory()
        {
            
        }

        #endregion

        #region Properties
        #endregion

        #region Public Methods
        public static ExporterBase<IsosResult> CreateMSFeatureExporter(Globals.ExporterType exporterType, Globals.MSFileType msFileType, string outputFileName)
        {
            Check.Assert(!string.IsNullOrEmpty(outputFileName), "MSFeatureExporterFactory cannot work. OutputFileName is empty - which is bad.");
            ExporterBase<IsosResult> msFeatureExporter;

            switch (exporterType)
            {
                case Globals.ExporterType.Text:

                    //IMS and UIMF filetypes have their own special export formats. All others will use a standard format. 
                    switch (msFileType)
                    {
                        case Globals.MSFileType.PNNL_IMS:
                            msFeatureExporter = new MSFeatureToTextFileExporterIMF(outputFileName);
                            break;
                        case Globals.MSFileType.PNNL_UIMF:
                            msFeatureExporter = new MSFeatureToTextFileExporterUIMF(outputFileName);
                            break;
                        default:
                            msFeatureExporter = new MSFeatureToTextFileExporterBasic(outputFileName);
                            break;
                    }
                    break;

                case Globals.ExporterType.Sqlite:
                    switch (msFileType)
                    {
                        case Globals.MSFileType.PNNL_UIMF:
                            msFeatureExporter = new MSFeatureToSQLiteExporterUIMF(outputFileName);
                            break;
                        default:
                            msFeatureExporter = new MSFeatureToSQLiteExporterBasic(outputFileName);
                            break;
                    }
                    break;

                default:
                    throw new NotImplementedException();
                    //break;
            }

            return msFeatureExporter;

        }


        #endregion

        #region Private Methods
        #endregion
    }
}
