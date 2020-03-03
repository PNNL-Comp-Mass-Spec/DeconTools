using System;
using System.IO;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Runs
{
    public sealed class MzRun : Run
    {
        private readonly pwiz.ProteowizardWrapper.MSDataFileReader _reader;

        private double[] _pwizScanTimes;

        //private double[] _pwizScans;

        private byte[] _pwizScanLevels;


        #region Constructors

        public MzRun()
        {
            IsDataThresholded = true;  //TODO:  this should not be hardcoded, but should be put in parameter file. This property is used by the peak detector
            MSFileType = Globals.MSFileType.Thermo_Raw;
            ContainsMSMSData = true;
            XYData = new XYData();
        }

        public MzRun(string datasetFilePath)
            : this()
        {
            var fileInfo = new FileInfo(datasetFilePath);

            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("Cannot initialize Run. File not found: " + datasetFilePath);
            }

            var fileExtension = Path.GetExtension(datasetFilePath).ToLower();

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

            DatasetFileOrDirectoryPath = datasetFilePath;
            var baseFilename = Path.GetFileName(DatasetFileOrDirectoryPath);
            DatasetName = baseFilename.Substring(0, baseFilename.LastIndexOf('.'));
            DatasetDirectoryPath = Path.GetDirectoryName(DatasetFileOrDirectoryPath);

            _reader = new pwiz.ProteowizardWrapper.MSDataFileReader(datasetFilePath);

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
                _reader.GetScanTimesAndMsLevels(out var times, out var msLevels);

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
                _reader.GetScanTimesAndMsLevels(out var times, out var msLevels);

                _pwizScanTimes = times;
                _pwizScanLevels = msLevels;
            }

            if (_pwizScanLevels != null && scanNum < _pwizScanLevels.Length - 1)
            {
                return _pwizScanLevels[scanNum];
            }

            return -1;

        }

        public override XYData GetMassSpectrum(ScanSet scanSet)
        {
            if (scanSet.PrimaryScanNumber > MaxLCScan)
            {
                throw new ArgumentOutOfRangeException("Cannot get mass spectrum. Exceeded max scan (which = " + MaxLCScan + ")");
            }

            if (scanSet.PrimaryScanNumber < MinLCScan)
            {
                throw new ArgumentOutOfRangeException("Cannot get mass spectrum. Exceeded min scan (which = " + MinLCScan + ")");
            }

            _reader.GetSpectrum(scanSet.PrimaryScanNumber, out var xVals, out var yVals);
            var xyData = new XYData { Xvalues = xVals, Yvalues = yVals };
            return xyData;
        }

        public override XYData GetMassSpectrum(ScanSet scanSet, double minMZ, double maxMZ)
        {
            var xyData = GetMassSpectrum(scanSet);

            if (xyData.Xvalues == null || xyData.Xvalues.Length <= 0) return xyData;
            if (minMZ > xyData.Xvalues[0] || maxMZ < xyData.Xvalues[xyData.Xvalues.Length - 1])
            {
                xyData = xyData.TrimData(minMZ, maxMZ);
            }
            return xyData;
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
