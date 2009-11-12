using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using System.IO;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.PeakListExporters
{
    public class PeakListTextExporter : IPeakListExporter
    {
        private char delimiter;

        private int[] mSLevelsToWrite;

        public int[] MSLevelsToWrite
        {
            get { return mSLevelsToWrite; }
            set { mSLevelsToWrite = value; }
        }

        public PeakListTextExporter(StreamWriter sw)
            : this(sw, new int[] { 1 })      // default allowed MSLevels = 1  
        {

        }

        public PeakListTextExporter(StreamWriter sw, int[] msLevelsToWrite)
        {
            this.OutputStream = sw;
            this.mSLevelsToWrite = msLevelsToWrite;
            this.delimiter = '\t';

            sw.Write(getHeaderLine());

        }

        private string getHeaderLine()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("scan_num");
            sb.Append(delimiter);
            sb.Append("peak_index");
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



        public override void WriteToStream(ResultCollection resultList)
        {
            List<MSPeak> peakList = resultList.Run.MSPeakList;
            ScanSet scanSet = resultList.Run.CurrentScanSet;

            int msLevel = resultList.Run.GetMSLevel(scanSet.PrimaryScanNumber);
            if (!this.mSLevelsToWrite.Contains(msLevel))    //check if current ms level is within the allowed ms levels for writing
            {
                return;
            }


            string peakdata;
            if (resultList.Run is UIMFRun)
            {
                //TODO: disable this for UIMF
                peakdata = buildPeakString(peakList, scanSet);
            }
            else
            {
                peakdata = buildPeakString(peakList, scanSet);
            }


            try
            {
                this.OutputStream.Write(peakdata);
            }
            catch (Exception ex)
            {
                throw new Exception("There was a problem writing out the peak data. Details: " + ex.Message);

            }


        }

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

    }
}
