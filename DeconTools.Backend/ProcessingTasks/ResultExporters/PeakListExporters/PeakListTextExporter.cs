using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using System.IO;
using DeconTools.Utilities;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.ProcessingTasks.PeakListExporters
{
    public class PeakListTextExporter : IPeakListExporter
    {
        private char delimiter;
        private ResultCollection results;
        private StreamWriter sw;
        private Globals.MSFileType fileType;


        #region Constructors
        public PeakListTextExporter(Globals.MSFileType fileType, string outputFileName)
            : this(fileType, 100000, outputFileName)      // default allowed MSLevels = 1  
        {
        }


        public PeakListTextExporter(Globals.MSFileType fileType, int triggerValue, string outputFileName)
        {
            try
            {
                sw = new StreamWriter(outputFileName);
            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("IsosResultExporter failed. Details: " + ex.Message, Logger.Instance.OutputFilename);
                throw new Exception("Result exporter failed.  Check to see if it is already open or not.");
            }

            this.TriggerToWriteValue = triggerValue;      //will write out peaks if trigger value is reached
            this.delimiter = '\t';
            this.fileType = fileType;    // need to know filetype so that frame_num can be outputted for UIMF data
            sw.Write(getHeaderLine());
        }

     

        #endregion

        #region Properties

        private int[] mSLevelsToExport;
        public override int[] MSLevelsToExport
        {
            get { return mSLevelsToExport; }
            set { mSLevelsToExport = value; }
        }
        private StreamWriter outputStream;
        public StreamWriter OutputStream
        {
            get { return outputStream; }
            set { outputStream = value; }
        }

        private int triggerValue;
        public override int TriggerToWriteValue
        {
            get { return triggerValue; }
            set { triggerValue = value; }
        }

        #endregion

        #region Public Methods
        public override void WriteOutPeaks(ResultCollection resultList)
        {
            results = resultList;

            string peakdata;

            peakdata = buildPeakString(resultList);


            try
            {
                sw.Write(peakdata);
            }
            catch (Exception ex)
            {
                throw new Exception("There was a problem writing out the peak data. Details: " + ex.Message);

            }
        }

        private string buildPeakString(ResultCollection resultList)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < resultList.MSPeakResultList.Count; i++)
            {
                sb.Append(resultList.MSPeakResultList[i].PeakID);
                sb.Append(delimiter);

                if (resultList.Run is UIMFRun)
                {
                    sb.Append(resultList.MSPeakResultList[i].Frame_num);
                    sb.Append(delimiter);
                }
                else
                {

                }
                sb.Append(resultList.MSPeakResultList[i].Scan_num);
                sb.Append(delimiter);
                sb.Append(resultList.MSPeakResultList[i].MSPeak.XValue.ToString("0.#####"));
                sb.Append(delimiter);
                sb.Append(resultList.MSPeakResultList[i].MSPeak.Height);
                sb.Append(delimiter);
                sb.Append(resultList.MSPeakResultList[i].MSPeak.Width.ToString("0.####"));
                sb.Append(delimiter);
                sb.Append(resultList.MSPeakResultList[i].MSPeak.SN.ToString("0.##"));
                sb.Append("\n");
            }

            return sb.ToString();
        }
        
        public override void Cleanup()
        {
            base.Cleanup();


        }

        private void FlushOutUnwrittenResults(ResultCollection results)
        {
            WriteOutPeaks(results);
        }
        # endregion

        #region Private Methods

        private string getHeaderLine()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("peak_index");
            sb.Append(delimiter);
            if (this.fileType == Globals.MSFileType.PNNL_UIMF)
            {
                sb.Append("frame_num");
                sb.Append(delimiter);
            }
            sb.Append("scan_num");
            sb.Append(delimiter);
            sb.Append("mz");
            sb.Append(delimiter);
            sb.Append("intensity");
            sb.Append(delimiter);
            sb.Append("fwhm");
            sb.Append(delimiter);
            sb.Append("signal_noise");
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }
        #endregion

        protected override void CloseOutputFile()
        {
            if (sw == null) return;
            try
            {
                sw.Flush();
                sw.Close();
            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("IsosResultExporter failed to close the output file properly. Details: " + ex.Message, Logger.Instance.OutputFilename);
            }
        }
    }
}
