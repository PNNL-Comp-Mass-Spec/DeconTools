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


        #region Constructors
        public PeakListTextExporter(string fileName)
            : this(fileName, 100000)      // default allowed MSLevels = 1  
        {
        }

  
        public PeakListTextExporter(string fileName, int triggerValue)
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

            this.TriggerToWriteValue = triggerValue;      //will write out peaks if trigger value is reached
            this.delimiter = '\t';
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
                sb.Append(i + 1);      //output is 1-based, not zero-based. 
                sb.Append(delimiter);
                sb.Append(resultList.MSPeakResultList[i].MSPeak.MZ.ToString("0.#####"));
                sb.Append(delimiter);
                sb.Append(resultList.MSPeakResultList[i].MSPeak.Intensity);
                sb.Append(delimiter);
                sb.Append(resultList.MSPeakResultList[i].MSPeak.FWHM.ToString("0.####"));
                sb.Append(delimiter);
                sb.Append(resultList.MSPeakResultList[i].MSPeak.SN.ToString("0.##"));
                sb.Append("\n");
            }

            return sb.ToString();
        }

        public override void Cleanup()
        {
            bool needToFlushResults = (results.MSPeakResultList != null && results.MSPeakResultList.Count > 0);
            if (needToFlushResults)
            {
                WriteOutPeaks(results);
                
            }
            CloseOutputFile();


        }

        private void FlushOutUnwrittenResults(ResultCollection results)
        {
            WriteOutPeaks(results);
        }
        # endregion

        #region Private Methods

        private string buildPeakString(List<MSPeak> peakList, ScanSet scanSet)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < peakList.Count; i++)
            {
                sb.Append(scanSet.PrimaryScanNumber);
                sb.Append(delimiter);
                sb.Append(i + 1);      //output is 1-based, not zero-based. 
                sb.Append(delimiter);
                sb.Append(peakList[i].MZ.ToString("0.#####"));
                sb.Append(delimiter);
                sb.Append(peakList[i].Intensity);
                sb.Append(delimiter);
                sb.Append(peakList[i].FWHM.ToString("0.####"));
                sb.Append(delimiter);
                sb.Append(peakList[i].SN.ToString("0.##"));
                sb.Append("\n");
            }

            return sb.ToString();

        }
        private string getHeaderLine()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("peak_index");
            sb.Append(delimiter);
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
