using System;
using System.IO;
using DeconTools.Backend.Core;
using pwiz.ProteowizardWrapper;

namespace DeconTools.Backend.Runs
{
    public class MzRun : Run
    {
        private MSDataFileReader _reader;

        private double[] _pwizScanTimes;

        //private double[] _pwizScans;

        private byte[] _pwizScanLevels;


        #region Constructors

        public MzRun()
            : base()
        {
            this.IsDataThresholded = true;  //TODO:  this should not be hardcoded, but should be put in parameter file. This property is used by the peak detector
            this.MSFileType = Globals.MSFileType.Finnigan;
            this.ContainsMSMSData = true;
            XYData = new XYData();
        }

        public MzRun(string fileName)
            : this()
        {
            var fileInfo = new FileInfo(fileName);

            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("Cannot initialize Run. File not found: " + fileName);
            }

            var fileExtension = Path.GetExtension(fileName).ToLower();

            switch (fileExtension)
            {
                case ".mzxml":
                    MSFileType = Globals.MSFileType.MZXML_Rawdata;
                    break;
                case ".mzml":
                    MSFileType = Globals.MSFileType.MZML;
                    break;
                case ".mz5":
                    MSFileType = Globals.MSFileType.MZ5;
                    break;
                default:
                    throw new IOException("Cannot initialize Run. File extension " + fileExtension + " isn't supported. The following file extensions are supported: mzXML, mzML, and mz5");
            }

            Filename = fileName;
            var baseFilename = Path.GetFileName(Filename);
            DatasetName = baseFilename.Substring(0, baseFilename.LastIndexOf('.'));
            DataSetPath = Path.GetDirectoryName(Filename);

            _reader = new MSDataFileReader(fileName);

            MinLCScan = GetMinPossibleLCScanNum();
            MaxLCScan = GetMaxPossibleLCScanNum();

        }


        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        public override XYData XYData { get; set; }

        public override int GetNumMSScans()
        {
            return _reader.SpectrumCount;
        }

        public override double GetTime(int scanNum)
        {
            if (_pwizScanTimes == null)
            {
                double[] times;
                byte[] msLevels;
                _reader.GetScanTimesAndMsLevels(out times, out msLevels);

                _pwizScanTimes = times;
                _pwizScanLevels = msLevels;
            }

            if (_pwizScanTimes != null && scanNum < _pwizScanTimes.Length - 1)
            {
                return _pwizScanTimes[scanNum];
            }

            return -1;


        }

        public override int GetMSLevelFromRawData(int scanNum)
        {
            if (_pwizScanLevels == null)
            {
                double[] times;
                byte[] msLevels;
                _reader.GetScanTimesAndMsLevels(out times, out msLevels);

                _pwizScanTimes = times;
                _pwizScanLevels = msLevels;
            }

            if (_pwizScanLevels != null && scanNum < _pwizScanLevels.Length - 1)
            {
                return _pwizScanLevels[scanNum];
            }

            return -1;

        }




        public override XYData GetMassSpectrum(ScanSet scanset)
        {
            double[] xvals;
            double[] yvals;


            if (scanset.PrimaryScanNumber > MaxLCScan)
            {
                throw new ArgumentOutOfRangeException("Cannot get mass spectrum. Exceeded max scan (which = " + MaxLCScan + ")");
            }

            if (scanset.PrimaryScanNumber < MinLCScan)
            {
                throw new ArgumentOutOfRangeException("Cannot get mass spectrum. Exceeded min scan (which = " + MinLCScan + ")");
            }



            _reader.GetSpectrum(scanset.PrimaryScanNumber, out xvals, out yvals);
            var xydata = new XYData {Xvalues = xvals, Yvalues = yvals};
            return xydata;


        }

        public override XYData GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {
            var xydata=   GetMassSpectrum(scanset);

            if (xydata.Xvalues == null || xydata.Xvalues.Length <= 0) return xydata;
            if (minMZ > xydata.Xvalues[0] || maxMZ < xydata.Xvalues[xydata.Xvalues.Length - 1])
            {
                xydata = xydata.TrimData(minMZ, maxMZ);
            }
            return xydata;
        }

        public override int GetMinPossibleLCScanNum()
        {
            return 0;                   // mzRun files are 0-based
        }

        public override int GetMaxPossibleLCScanNum()
        {
            return GetNumMSScans() - 1;    // mzRun files are 0-based
        }
    }
}
