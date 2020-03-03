using System;
using System.IO;
using DeconTools.Backend.Core;
using EDAL;

namespace DeconTools.Backend.Runs
{
    public sealed class BrukerTOF : Run
    {

        private readonly IMSAnalysis _msAnalysis;
        private readonly MSSpectrumCollection _spectrumCollection;

        public BrukerTOF()
        {
            XYData = new XYData();
            IsDataThresholded = true;
            MSFileType = Globals.MSFileType.Bruker;
            ContainsMSMSData = false;
        }


        public BrukerTOF(string directoryPath)
            : this()
        {

            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException(
                    "Could not create Bruker dataset.  Path " + directoryPath + " does not exist. Ensure you are pointing to a directory that contains the raw MS files (e.g. analysis.baf)");

            }

            bool isDirectory;

            try
            {
                isDirectory = (File.GetAttributes(directoryPath) & FileAttributes.Directory) == FileAttributes.Directory;

            }
            catch (Exception exception)
            {

                throw new IOException("Could not create Bruker dataset. Path " + directoryPath + " does not exist. Ensure you are pointing to a directory that contains the raw MS files (e.g. analysis.baf)", exception);

            }

            _msAnalysis = new MSAnalysis();
            _msAnalysis.Open(directoryPath);

            DatasetFileOrDirectoryPath = directoryPath;
            _spectrumCollection = _msAnalysis.MSSpectrumCollection;
            DatasetName = GetDatasetName(DatasetFileOrDirectoryPath);
            DatasetDirectoryPath = GetDatasetDirectoryPath(DatasetFileOrDirectoryPath);

            MinLCScan = GetMinPossibleLCScanNum();
            MaxLCScan = GetMaxPossibleLCScanNum();

        }




        public override XYData XYData { get; set; }
        public override int GetNumMSScans()
        {
            return _spectrumCollection.Count;
        }

        public override bool IsDataCentroided(int scanNum)
        {
            return false;
        }

        public override double GetTime(int scanNum)
        {

            var spectrum = _spectrumCollection[scanNum];


            //NOTE: retention time is reported in seconds. DeconTools normally reports in Minutes. So need to change this.
            return spectrum.RetentionTime/60;



        }

        public override int GetMSLevelFromRawData(int scanNum)
        {

            var spectrum = _spectrumCollection[scanNum];

            return spectrum.MSMSStage;

        }

        public override XYData GetMassSpectrum(ScanSet scanSet, double minMZ, double maxMZ)
        {
            return GetMassSpectrum(scanSet);
        }

        public override XYData GetMassSpectrum(ScanSet scanSet)
        {

            var spectrum = _spectrumCollection[scanSet.PrimaryScanNumber];


            spectrum.GetMassIntensityValues(SpectrumTypes.SpectrumType_Profile, out var mzVals, out var intensityVals);
            var xyData = new XYData
            {
                Xvalues = (double[])mzVals,
                Yvalues = (double[])intensityVals
            };

            return xyData;

        }


        public override int GetMinPossibleLCScanNum()
        {
            return 1;     //one-based
        }

        public override int GetMaxPossibleLCScanNum()
        {
            return GetNumMSScans();
        }

        private string GetDatasetName(string fullFolderPath)
        {

            var dirInfo = new DirectoryInfo(fullFolderPath);

            if (dirInfo.Name.EndsWith(".d", StringComparison.OrdinalIgnoreCase))
            {
                return dirInfo.Name.Substring(0, dirInfo.Name.Length - ".d".Length);
            }

            return dirInfo.Name;
        }

        private string GetDatasetDirectoryPath(string fullFolderPath)
        {
            var dirInfo = new DirectoryInfo(fullFolderPath);
            return dirInfo.FullName;
        }

    }
}
