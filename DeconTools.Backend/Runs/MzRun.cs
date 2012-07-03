using System;
using System.IO;
using DeconTools.Backend.Core;
using pwiz.ProteowizardWrapper;

namespace DeconTools.Backend.Runs
{
    public class MzRun : Run
    {
        private pwiz.ProteowizardWrapper.MSDataFileReader _reader;

        private double[] _pwizScanTimes;

        private double[] _pwizScans;

        private byte[] _pwizScanLevels;


        #region Constructors

        public MzRun()
            : base()
        {
            this.MSParameters = new DeconTools.Backend.Parameters.MSParameters();
            this.IsDataThresholded = true;  //TODO:  this should not be hardcoded, but should be put in parameter file. This property is used by the peak detector
            this.MSFileType = Globals.MSFileType.Finnigan;
            this.ContainsMSMSData = true;
            XYData = new XYData();
        }

        public MzRun(string fileName)
            : this()
        {

            
            

            FileInfo fileInfo = new FileInfo(fileName);

            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("Cannot initialize Run. File not found.");
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
                    throw new IOException("Cannot initialize Run. File extension isn't supported. The following file extensions are supported: mzXML, mzML, and mz5");
            }

            Filename = fileName;
            string baseFilename = Path.GetFileName(Filename);
            DatasetName = baseFilename.Substring(0, baseFilename.LastIndexOf('.'));
            DataSetPath = Path.GetDirectoryName(Filename);

            


            _reader = new MSDataFileReader(fileName);

            MinScan = GetMinPossibleScanNum();
            MaxScan = GetMaxPossibleScanNum();

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




        public override void GetMassSpectrum(ScanSet scanset)
        {
            double[] xvals;
            double[] yvals;


            if (scanset.PrimaryScanNumber > MaxScan)
            {
                throw new ArgumentOutOfRangeException("Cannot get mass spectrum. Exceeded max scan (which = " + MaxScan + ")");
            }

            if (scanset.PrimaryScanNumber < MinScan)
            {
                throw new ArgumentOutOfRangeException("Cannot get mass spectrum. Exceeded min scan (which = " + MinScan + ")");
            }



            _reader.GetSpectrum(scanset.PrimaryScanNumber, out xvals, out yvals);

            XYData.Xvalues = xvals;
            XYData.Yvalues = yvals;

        }

        public override void GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {
            GetMassSpectrum(scanset);

            if (XYData.Xvalues == null || XYData.Xvalues.Length <= 0) return;
            if (minMZ > XYData.Xvalues[0] || maxMZ < XYData.Xvalues[XYData.Xvalues.Length - 1])
            {
                XYData = XYData.TrimData(minMZ, maxMZ);
            }
        }

        public override int GetMinPossibleScanNum()
        {
            return 0;                   // mzRun files are 0-based
        }

        public override int GetMaxPossibleScanNum()
        {
            return GetNumMSScans() - 1;    // mzRun files are 0-based
        }
    }
}
