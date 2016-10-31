using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;

namespace DeconTools.Backend.Data
{

    //NOTE:  2012_11_15 - The importer imports UIMF peaks as if they were orbi peaks.  All IMS scan info is ignored
    public class PeakImporterFromText : IPeakImporter
    {
        private string filename;
        private char delimiter;
        private string _header;
        private bool _peaksAreFromUIMF;
        private bool _containsMSFeatureIDColumn;

        #region Constructors
        public PeakImporterFromText(string filename)
            : this(filename, null)
        {


        }

        public PeakImporterFromText(string filename, BackgroundWorker bw)
        {
            if (!File.Exists(filename)) throw new IOException("PeakImporter failed. File doesn't exist: " + Utilities.DiagnosticUtilities.GetFullPathSafe(filename));

            FileInfo fi = new FileInfo(filename);
            numRecords = (int)(fi.Length / 1000 * 24);   // a way of approximating how many peaks there are... only for use with the backgroundWorker

            this.filename = filename;
            this.delimiter = '\t';
            this.backgroundWorker = bw;
            this.peakProgressInfo = new PeakProgressInfo();
        }

        #endregion

        #region Properties
        #endregion

        #region Public Methods

        //public void ImportUIMFPeaksIntoTree(Data.Structures.BinaryTree<IPeak> tree)
        //{
        //    using (StreamReader reader = new StreamReader(filename))
        //    {
        //        reader.ReadLine();    //first line is the header line.   

        //        int progressCounter = 0;
        //        while (reader.Peek() != -1)
        //        {
        //            string line = reader.ReadLine();
        //            IPeak peak = convertTextToPeakUIMFResult(line);
        //            //peak.SortOnKey = IPeak.SortKey.INTENSITY;
        //            tree.Add(peak);
        //            progressCounter++;
        //            reportProgress(progressCounter);

        //        }
        //    }

        //}

     
        public override void ImportPeaks(List<DeconTools.Backend.DTO.MSPeakResult> peakList)
        {
            using (StreamReader reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                _header =reader.ReadLine();    //first line is the header line.  

                _peaksAreFromUIMF = _header != null && _header.Contains("frame");

                _containsMSFeatureIDColumn = _header != null && _header.Contains("MSFeatureID");

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
                    peakProgressInfo.ProgressInfoString = "Loading Peaks ";
                    backgroundWorker.ReportProgress(percentProgress, peakProgressInfo);
                }
                else
                {
                    if (progressCounter % 50000 == 0) Console.WriteLine("Peak importer progress (%) = " + percentProgress);

                }

                return;
            }
        }

        //TODO: make this so that it works with UIMF data
        //TODO: use column header lookup instead of hard coded values
        private MSPeakResult convertTextToPeakResult(string line)
        {
            MSPeakResult peakresult = new MSPeakResult();

            int columnCounter = 0;

            List<string> processedLine = processLine(line);
            peakresult.PeakID = Convert.ToInt32(processedLine[columnCounter]);
            
            
            //NOTE - for UIMF data the frame column is loaded into the 'Scan_num' property.  This is kind of ugly since there is
            //already a FrameNum property. I'm doing this so that we can process UIMF files in IQ.  We need to fix this later. 
            peakresult.Scan_num = Convert.ToInt32(processedLine[++columnCounter]);
            
            
            peakresult.MSPeak = new DeconTools.Backend.Core.MSPeak();

            //UIMF peak data contains an extra column
            if (_peaksAreFromUIMF) ++columnCounter;

            peakresult.MSPeak.XValue = Convert.ToDouble(processedLine[++columnCounter]);
            peakresult.MSPeak.Height = Convert.ToSingle(processedLine[++columnCounter]);
            peakresult.MSPeak.Width = Convert.ToSingle(processedLine[++columnCounter]);
            peakresult.MSPeak.SignalToNoise = Convert.ToSingle(processedLine[++columnCounter]);


            if (_containsMSFeatureIDColumn)
            {
                int currentCounter = ++columnCounter;
                peakresult.MSPeak.MSFeatureID = Convert.ToInt32(processedLine[currentCounter]);
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
            peakresult.FrameNum = Convert.ToInt32(processedLine[1]);
            peakresult.Scan_num = Convert.ToInt32(processedLine[2]);
            peakresult.MSPeak = new DeconTools.Backend.Core.MSPeak();
            peakresult.MSPeak.XValue = Convert.ToDouble(processedLine[3]);
            peakresult.MSPeak.Height = Convert.ToSingle(processedLine[4]);
            peakresult.MSPeak.Width = Convert.ToSingle(processedLine[5]);
            peakresult.MSPeak.SignalToNoise = Convert.ToSingle(processedLine[6]);

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
