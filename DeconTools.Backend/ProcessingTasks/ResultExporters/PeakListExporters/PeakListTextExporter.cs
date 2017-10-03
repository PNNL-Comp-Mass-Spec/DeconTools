using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.PeakListExporters
{
    public class PeakListTextExporter : IPeakListExporter
    {
        private char m_delimiter;
        private Globals.MSFileType m_FileType;
        private int[] m_msLevelsToExport;
        private int m_triggerValue;


        #region Constructors
        public PeakListTextExporter(Globals.MSFileType fileType, string outputFileName)
            : this(fileType, 100000, outputFileName)      // default allowed MSLevels = 1
        {
        }


        public PeakListTextExporter(Globals.MSFileType fileType, int triggerValue, string outputFileName)
        {

            FileName = outputFileName;
            TriggerToWriteValue = triggerValue;      //will write out peaks if trigger value is reached
            m_delimiter = '\t';
            m_FileType = fileType;    // need to know filetype so that frame_num can be outputted for UIMF data


            bool fileExists;
            try
            {
                fileExists = File.Exists(outputFileName);
            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry("PeakExporter failed. Details: " + ex.Message, Logger.Instance.OutputFilename);
                throw new Exception("Peak exporter failed.  Check to see if output file is already open.");
            }


            if (fileExists)
            {
                try
                {
                    File.Delete(outputFileName);
                }
                catch (Exception ex)
                {
                    Logger.Instance.AddEntry("PeakExporter failed. Details: " + ex.Message, Logger.Instance.OutputFilename);
                    throw new Exception("Peak exporter failed.  Check to see if output file is already open.");

                }
            }

            initializeAndWriteHeader();
        }



        #endregion

        #region Properties

        public string FileName { get; set; }

        public override int[] MSLevelsToExport
        {
            get { return m_msLevelsToExport; }
            set { m_msLevelsToExport = value; }
        }

        public override int TriggerToWriteValue
        {
            get { return m_triggerValue; }
            set { m_triggerValue = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Append the peaks to the _peaks.txt file
        /// </summary>
        /// <param name="peakList">Peak list to write</param>
        public override void WriteOutPeaks(List<MSPeakResult> peakList)
        {
            using (var sw = new StreamWriter(new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.Read)))
            {
                WriteOutPeaks(sw, peakList);
                sw.Close();
            }
        }

        /// <summary>
        /// Append the peaks to the _peaks.txt file
        /// </summary>
        /// <param name="sw">Filestream object</param>
        /// <param name="peakList">Peak list to write</param>
        public override void WriteOutPeaks(StreamWriter sw, List<MSPeakResult> peakList)
        {

            foreach (var peak in peakList)
            {
                var lineOfPeakData = buildPeakString(peak);
                sw.WriteLine(lineOfPeakData);
            }
            sw.Flush();

        }

        # endregion

        #region Private Methods
        private string buildPeakString(MSPeakResult peak)
        {
            var data = new List<string> {
                peak.PeakID.ToString()
            };

            if (m_FileType == Globals.MSFileType.PNNL_UIMF)
            {
                data.Add(peak.FrameNum.ToString());
            }

            data.Add(peak.Scan_num.ToString());
            data.Add(DblToString(peak.MSPeak.XValue, 5));
            data.Add(DblToString(peak.MSPeak.Height, 4, true));
            data.Add(DblToString(peak.MSPeak.Width, 4));
            data.Add(DblToString(peak.MSPeak.SignalToNoise, 2));
            data.Add(peak.MSPeak.MSFeatureID.ToString());

            return string.Join(m_delimiter.ToString(), data);
        }

        private string buildHeaderLine()
        {
            var data = new List<string> {
                "peak_index"
            };

            if (m_FileType == Globals.MSFileType.PNNL_UIMF)
            {
                data.Add("frame_num");
            }

            data.Add("scan_num");
            data.Add("mz");
            data.Add("intensity");
            data.Add("fwhm");
            data.Add("signal_noise");
            data.Add("MSFeatureID");

            return string.Join(m_delimiter.ToString(), data);
        }
        protected void initializeAndWriteHeader()
        {

            Check.Assert(!string.IsNullOrEmpty(FileName), string.Format("{0} failed. Export file's FileName wasn't declared.", Name));

            try
            {
                if (File.Exists(FileName))
                {
                    File.Delete(FileName);
                }

            }
            catch (Exception ex)
            {
                Logger.Instance.AddEntry(string.Format("{0} failed. Details: " + ex.Message +
                    "; STACKTRACE = " + PRISM.clsStackTraceFormatter.GetExceptionStackTraceMultiLine(ex), Name), Logger.Instance.OutputFilename);
                throw;
            }


            using (var writer = new StreamWriter(new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.Read)))
            {
                var headerLine = buildHeaderLine();
                writer.WriteLine(headerLine);
                writer.Flush();
                writer.Close();
            }
        }

        #endregion


    }
}
