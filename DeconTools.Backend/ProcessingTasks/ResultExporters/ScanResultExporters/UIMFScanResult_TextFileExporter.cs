using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using System.IO;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ResultExporters.ScanResultExporters
{
    public class UIMFScanResult_TextFileExporter:ScanResult_TextFileExporter
    {
        private char delimiter;

        #region Constructors
        public UIMFScanResult_TextFileExporter(string fileName)
        {
            try
            {
                sw = new StreamWriter(fileName);
            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("IsosResultExporter failed. Details: " + ex.Message, Logger.Instance.OutputFilename);
                throw new Exception("Result exporter failed.  Check to see if it is already open or not.");
            }

            this.delimiter = ',';

            sw.Write(buildHeaderLine());

        }
        #endregion

 


        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        protected override string buildScansResultOutput(ScanResult result)
        {
            StringBuilder sb = new StringBuilder();

            UIMFScanResult uimfScanResult=(UIMFScanResult)result;
            sb.Append(uimfScanResult.Frameset.PrimaryFrame);
            sb.Append(delimiter);
            sb.Append(uimfScanResult.ScanTime.ToString("0.###"));
            sb.Append(delimiter);
            sb.Append(result.SpectrumType);
            sb.Append(delimiter);
            sb.Append(uimfScanResult.BasePeak.Intensity);
            sb.Append(delimiter);
            sb.Append(uimfScanResult.BasePeak.MZ.ToString("0.#####"));
            sb.Append(delimiter);
            sb.Append(uimfScanResult.TICValue);
            sb.Append(delimiter);
            sb.Append(uimfScanResult.NumPeaks);
            sb.Append(delimiter);
            sb.Append(uimfScanResult.NumIsotopicProfiles);
            sb.Append(delimiter);
            sb.Append(uimfScanResult.FramePressureFront.ToString("0.####"));
            sb.Append(delimiter);
            sb.Append(uimfScanResult.FramePressureBack.ToString("0.####"));

            return sb.ToString();


        }

        protected override string buildHeaderLine()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("scan_num");
            sb.Append(Delimiter);
            sb.Append("scan_time");
            sb.Append(Delimiter);
            sb.Append("type");
            sb.Append(Delimiter);
            sb.Append("bpi");
            sb.Append(Delimiter);
            sb.Append("bpi_mz");
            sb.Append(Delimiter);
            sb.Append("tic");
            sb.Append(Delimiter);
            sb.Append("num_peaks");
            sb.Append(Delimiter);
            sb.Append("num_deisotoped");
            sb.Append(Delimiter);
            sb.Append("frame_pressure_front");
            sb.Append(Delimiter);
            sb.Append("frame_pressure_back");
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }


        public override char Delimiter
        {
            get
            {
                return delimiter;
            }
            set
            {
                delimiter = value;
            }
        }
    }
}
