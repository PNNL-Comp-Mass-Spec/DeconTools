using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using System.IO;
using Agilent.MassSpectrometry.DataAnalysis;

namespace DeconTools.Backend.Runs
{
    public class AgilentD_Run : Run
    {
        #region Constructors

        IMsdrDataReader m_reader;

        
        public AgilentD_Run(string datasetFullName)
        {
            bool isFile = (File.Exists(datasetFullName));
            bool isFolder = (Directory.Exists(datasetFullName));

            Check.Require(!isFile, "Dataset's inputted name refers to a file, but should refer to a Folder");
            Check.Require(isFolder, "Dataset not found.");

            string nameWithExtension = Path.GetFileName(datasetFullName);

            this.Filename = datasetFullName;
            this.DatasetName = nameWithExtension.Substring(0, nameWithExtension.LastIndexOf(".d"));    //get dataset name without .d extension
            this.DataSetPath = Path.GetDirectoryName(datasetFullName);

            OpenDataset();
        }

        private void OpenDataset()
        {
            m_reader = new MassSpecDataReader();
            m_reader.OpenDataFile(this.Filename);
        }



        #endregion

        #region Properties
        public override XYData XYData { get; set; }

        #endregion

        #region Public Methods
        public override int GetNumMSScans()
        {
            //m_reader=new MassSpecDataReader();


            IBDAMSScanFileInformation msscan = m_reader.FileInformation.MSScanFileInformation;

            return (int)msscan.TotalScansPresent;

        }

        public override double GetTime(int scanNum)
        {
            throw new NotImplementedException();
        }

        public override int GetMSLevelFromRawData(int scanNum)
        {
            throw new NotImplementedException();
        }

        public override void GetMassSpectrum(ScanSet scanset, double minMZ, double maxMZ)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private Methods
        #endregion



    }
}
