using System.IO;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.NETAlignment
{
    public class ChromAlignerUsingVIPERInfo
    {
        string m_targetUMCFileStoringAlignmentInfo;

        #region Constructors
        public ChromAlignerUsingVIPERInfo()
        {
        }

        public ChromAlignerUsingVIPERInfo(string targetUMCFileStoringAlignmentInfo)
        {
            this.m_targetUMCFileStoringAlignmentInfo = targetUMCFileStoringAlignmentInfo;
        }

        #endregion

        #region Properties
        #endregion

        #region Public Methods

        public void Execute(Run run)
        {
            Check.Require(run != null, "'Run' has not be defined");
            Check.Require(run.DatasetFileOrDirectoryPath?.Length > 0, "MS data file ('run') has not been initialized");
            //Check.Require(run.ScanSetCollection != null && run.ScanSetCollection.ScanSetList.Count > 0, "ChromAligner failed. ScanSets have not been defined.");

            if (m_targetUMCFileStoringAlignmentInfo == null)
            {
                var baseFileName = Path.Combine(run.DatasetDirectoryPath, run.DatasetName);
                m_targetUMCFileStoringAlignmentInfo = baseFileName + "_UMCs.txt";
            }
            Check.Require(File.Exists(m_targetUMCFileStoringAlignmentInfo), "ChromAligner failed. The UMC file from which alignment data is extracted does not exist.");

            var umcs = new UMCCollection();

            var importer = new UMCFileImporter(m_targetUMCFileStoringAlignmentInfo, '\t');
            umcs = importer.Import();

            var scanNetPairs = umcs.GetScanNETLookupTable();

            run.NetAlignmentInfo = new NetAlignmentInfoBasic(run.MinLCScan, run.MaxLCScan);
            run.NetAlignmentInfo.SetScanToNETAlignmentData(scanNetPairs);
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
