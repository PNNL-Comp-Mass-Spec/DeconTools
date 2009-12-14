using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using DeconTools.Backend.DTO;

namespace DeconTools.Backend.Data
{
    public class PeakImporterFromText : IPeakImporter
    {
        private string filename;
        private char delimiter;

        #region Constructors
        public PeakImporterFromText(string filename)
            : this(filename, null)
        {


        }

        public PeakImporterFromText(string filename, BackgroundWorker bw)
        {
            if (!File.Exists(filename)) throw new System.IO.IOException("PeakImporter failed. File doesn't exist.");
            this.filename = filename;
            this.delimiter = '\t';
            this.backgroundWorker = bw;

        }

        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
        public override void ImportPeaks(List<DeconTools.Backend.DTO.MSPeakResult> peakList)
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                reader.ReadLine();    //first line is the header line.   

                int progressCounter = 0;
                while (reader.Peek() != -1)
                {
                    string line = reader.ReadLine();
                    MSPeakResult peak = convertTextToPeakResult(line);
                    peakList.Add(peak);

                    progressCounter++;
                    reportProgress(progressCounter);

                }


            }
        }

        private MSPeakResult convertTextToPeakResult(string line)
        {
            MSPeakResult peakresult = new MSPeakResult();

            //TODO:  this doesn't work for UIMF peak files  (they have an extra column)

            List<string> processedLine = processLine(line);
            peakresult.PeakID = Convert.ToInt32(processedLine[0]);
            peakresult.Scan_num = Convert.ToInt32(processedLine[1]);
            peakresult.MSPeak = new DeconTools.Backend.Core.MSPeak();
            peakresult.MSPeak.MZ = Convert.ToDouble(processedLine[2]);
            peakresult.MSPeak.Intensity = Convert.ToSingle(processedLine[3]);
            peakresult.MSPeak.FWHM = Convert.ToSingle(processedLine[4]);
            peakresult.MSPeak.SN = Convert.ToSingle(processedLine[5]);

            return peakresult;



        }



        private List<string> processLine(string inputLine)
        {
            char[] splitter = { delimiter };
            List<string> returnedList = new List<string>();

            string[] arr = inputLine.Split(splitter);
            foreach (string str in arr)
            {
                returnedList.Add(str);
            }
            return returnedList;
        }
    }
}
