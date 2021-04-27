using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Utilities;

namespace DeconTools.Backend.Data
{
    //NOTE:  2012_11_15 - The importer imports UIMF peaks as if they were Orbitrap peaks.  All IMS scan info is ignored
    public class PeakImporterFromText : IPeakImporter
    {
        private readonly string _filename;
        private readonly char _delimiter;
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
            if (!File.Exists(filename)) throw new IOException("PeakImporter failed. File doesn't exist: " + DiagnosticUtilities.GetFullPathSafe(filename));

            var fi = new FileInfo(filename);
            numRecords = (int)(fi.Length / 1000 * 24);   // a way of approximating how many peaks there are... only for use with the backgroundWorker

            _filename = filename;
            _delimiter = '\t';
            backgroundWorker = bw;
            peakProgressInfo = new PeakProgressInfo();
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
        //        while (!reader.EndOfStream)
        //        {
        //            string line = reader.ReadLine();
        //            IPeak peak = ConvertTextToPeakUIMFResult(line);
        //            //peak.SortOnKey = IPeak.SortKey.INTENSITY;
        //            tree.Add(peak);
        //            progressCounter++;
        //            reportProgress(progressCounter);

        //        }
        //    }

        //}

        public override void ImportPeaks(List<MSPeakResult> peakList)
        {
            using (var reader = new StreamReader(new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                _header = reader.ReadLine();    //first line is the header line.

                _peaksAreFromUIMF = _header != null && _header.Contains("frame");

                _containsMSFeatureIDColumn = _header != null && _header.Contains("MSFeatureID");

                var progressCounter = 0;
                var lastReportProgress = DateTime.UtcNow;
                var lastReportProgressConsole = DateTime.UtcNow;

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var peak = ConvertTextToPeakResult(line);
                    peakList.Add(peak);

                    progressCounter++;
                    reportProgress(progressCounter, ref lastReportProgress, ref lastReportProgressConsole);
                }
            }
        }

        #endregion

        //TODO: make this so that it works with UIMF data
        //TODO: use column header lookup instead of hard coded values
        private MSPeakResult ConvertTextToPeakResult(string line)
        {
            var peakResult = new MSPeakResult();

            var columnCounter = 0;

            var processedLine = ProcessLine(line);
            peakResult.PeakID = int.Parse(processedLine[columnCounter]);

            //NOTE - for UIMF data the frame column is loaded into the 'Scan_num' property.  This is kind of ugly since there is
            //already a FrameNum property. I'm doing this so that we can process UIMF files in IQ.  We need to fix this later.
            peakResult.Scan_num = int.Parse(processedLine[++columnCounter]);

            //UIMF peak data contains an extra column
            if (_peaksAreFromUIMF) ++columnCounter;

            var mz = double.Parse(processedLine[++columnCounter], CultureInfo.InvariantCulture);
            var intensity = float.Parse(processedLine[++columnCounter], CultureInfo.InvariantCulture);
            var fwhm = float.Parse(processedLine[++columnCounter], CultureInfo.InvariantCulture);
            var sn = float.Parse(processedLine[++columnCounter], CultureInfo.InvariantCulture);

            peakResult.MSPeak = new MSPeak(mz, intensity, fwhm, sn);

            if (_containsMSFeatureIDColumn)
            {
                var currentCounter = ++columnCounter;
                peakResult.MSPeak.MSFeatureID = int.Parse(processedLine[currentCounter]);
            }

            return peakResult;
        }

        private MSPeakResult ConvertTextToPeakUIMFResult(string line)
        {
            var peakResult = new MSPeakResult();
            var processedLine = ProcessLine(line);
            if (processedLine.Count < 7)
            {
                throw new IOException("Trying to import peak data into UIMF data object, but not enough columns are present in the source text file");
            }

            peakResult.PeakID = int.Parse(processedLine[0]);
            peakResult.FrameNum = int.Parse(processedLine[1]);
            peakResult.Scan_num = int.Parse(processedLine[2]);

            var mz = double.Parse(processedLine[3], CultureInfo.InvariantCulture);
            var intensity = float.Parse(processedLine[4], CultureInfo.InvariantCulture);
            var fwhm = float.Parse(processedLine[5], CultureInfo.InvariantCulture);
            var sn = float.Parse(processedLine[6], CultureInfo.InvariantCulture);

            peakResult.MSPeak = new MSPeak(mz, intensity, fwhm, sn);

            if (processedLine.Count > 7)
            {
                peakResult.MSPeak.MSFeatureID = int.Parse(processedLine[7]);
            }

            return peakResult;
        }

        private List<string> ProcessLine(string inputLine)
        {
            char[] splitter = { _delimiter };
            var returnedList = new List<string>();

            var arr = inputLine.Split(splitter);
            foreach (var str in arr)
            {
                returnedList.Add(str);
            }
            return returnedList;
        }
    }
}
