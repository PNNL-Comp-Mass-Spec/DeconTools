using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Core;

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

            FileInfo fi = new FileInfo(filename);
            numRecords = (int)(fi.Length / 1000 * 24);   // a way of approximating how many peaks there are... only for use with the backgroundWorker

            this.filename = filename;
            this.delimiter = '\t';
            this.backgroundWorker = bw;

        }

        #endregion

        #region Properties
        #endregion

        #region Public Methods

        public void ImportUIMFPeaksIntoTree(Data.Structures.BinaryTree<IPeak> tree)
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                reader.ReadLine();    //first line is the header line.   

                int progressCounter = 0;
                while (reader.Peek() != -1)
                {
                    string line = reader.ReadLine();
                    IPeak peak = convertTextToPeakUIMFResult(line);
                    peak.SortOnKey = IPeak.SortKey.INTENSITY;
                    tree.Add(peak);
                    progressCounter++;
                    reportProgress(progressCounter);

                }
            }

        }

        public void ImportUIMFPeaks(List<IPeak> peakList)
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                reader.ReadLine();    //first line is the header line.   

                int progressCounter = 0;
                while (reader.Peek() != -1)
                {
                    string line = reader.ReadLine();
                    IPeak peak = convertTextToPeakUIMFResult(line);
                    peakList.Add(peak);

                    progressCounter++;
                    reportProgress(progressCounter);

                }
            }

        }

     
        
        
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

        #endregion

       
        protected override void reportProgress(int progressCounter)
        {
            if (numRecords == 0) return;
            if (progressCounter % 10000 == 0)
            {

                int percentProgress = (int)((double)progressCounter / (double)numRecords * 100);

                if (this.backgroundWorker != null)
                {
                    backgroundWorker.ReportProgress(percentProgress);
                }
                else
                {
                    if (progressCounter % 50000 == 0) Console.WriteLine("Peak importer progress (%) = " + percentProgress);

                }

                return;
            }
        }

        private MSPeakResult convertTextToPeakResult(string line)
        {
            MSPeakResult peakresult = new MSPeakResult();

            List<string> processedLine = processLine(line);
            peakresult.PeakID = Convert.ToInt32(processedLine[0]);
            peakresult.Scan_num = Convert.ToInt32(processedLine[1]);
            peakresult.MSPeak = new DeconTools.Backend.Core.MSPeak();
            peakresult.MSPeak.XValue = Convert.ToDouble(processedLine[2]);
            peakresult.MSPeak.Height = Convert.ToSingle(processedLine[3]);
            peakresult.MSPeak.Width = Convert.ToSingle(processedLine[4]);
            peakresult.MSPeak.SN = Convert.ToSingle(processedLine[5]);


            if (processedLine.Count > 6)
            {
                peakresult.MSPeak.MSFeatureID = Convert.ToInt32(processedLine[6]);
            }


            return peakresult;



        }


        private MSPeakResult convertTextToPeakUIMFResult(string line)
        {
            MSPeakResult peakresult = new MSPeakResult();
            List<string> processedLine = processLine(line);
            if (processedLine.Count < 7)
            {
                throw new System.IO.IOException("Trying to import peak data into UIMF data object, but not enough columns are present in the source text file");
            }

            peakresult.PeakID = Convert.ToInt32(processedLine[0]);
            peakresult.Frame_num = Convert.ToInt32(processedLine[1]);
            peakresult.Scan_num = Convert.ToInt32(processedLine[2]);
            peakresult.MSPeak = new DeconTools.Backend.Core.MSPeak();
            peakresult.MSPeak.XValue = Convert.ToDouble(processedLine[3]);
            peakresult.MSPeak.Height = Convert.ToSingle(processedLine[4]);
            peakresult.MSPeak.Width = Convert.ToSingle(processedLine[5]);
            peakresult.MSPeak.SN = Convert.ToSingle(processedLine[6]);

            if (processedLine.Count > 7)
            {
                peakresult.MSPeak.MSFeatureID = Convert.ToInt32(processedLine[7]);
            }

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
