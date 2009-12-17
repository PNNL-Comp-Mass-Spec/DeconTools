using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using System.IO;
using DeconTools.Backend.Data;

namespace DeconTools.Backend.ProcessingTasks.NETAlignment
{
    public class ChromAlignerUsingVIPERInfo
    {

        #region Constructors


        #endregion

        #region Properties
        #endregion

        #region Public Methods

        public void Execute(Run run)
        {
            Check.Require(run != null, "'Run' has not be defined");
            Check.Require(run.Filename != null &&
                run.Filename.Length > 0, "MS data file ('run') has not been initialized");
            Check.Require(run.ScanSetCollection != null && run.ScanSetCollection.ScanSetList.Count > 0, "ChromAligner failed. ScanSets have not been defined.");

            string baseFileName = run.Filename.Substring(0, run.Filename.LastIndexOf('.'));

            string targetUMCFileName = baseFileName + "_UMCs.txt";

            Check.Require(File.Exists(targetUMCFileName), "ChromAligner failed. The UMC file from which alignment data is extracted does not exist.");

            UMCCollection umcs = new UMCCollection();
            UMCFileImporter importer = new UMCFileImporter(targetUMCFileName, '\t');
            importer.Import(umcs);


            run.ScanToNETAlignmentData = umcs.GetScanNETLookupTable();
            run.UpdateNETAlignment();


        }



        #endregion

        #region Private Methods
        #endregion
    }
}
