using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Data;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.NETAlignment;
using DeconTools.Backend.Runs;
using System.IO;
using System.ComponentModel;

namespace DeconTools.Backend.Utilities
{
    public class RunUtilities
    {

        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods
        public static Run CreateAndAlignRun(string filename)
        {
            return CreateAndAlignRun(filename, null);
        }

        private static void GetPeaks(Run run, string expectedPeaksFile)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;

            PeakImporterFromText peakImporter = new DeconTools.Backend.Data.PeakImporterFromText(expectedPeaksFile, bw);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);
        }

        #endregion

        #region Private Methods

        #endregion


        public static Run CreateAndAlignRun(string filename, string peaksFile)
        {

            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(filename);

            //Console.WriteLine(run.DatasetName + " loaded.");


            string basePath = run.DataSetPath;

            DirectoryInfo datasetDirInfo = new DirectoryInfo(basePath);
            FileInfo[] umcFileInfo = datasetDirInfo.GetFiles("*_umcs.txt");

            if (umcFileInfo.Count() > 0)
            {
                string targetUMCFileName = umcFileInfo.First().FullName;

                UMCCollection umcs = new UMCCollection();
                UMCFileImporter importer = new UMCFileImporter(targetUMCFileName, '\t');
                umcs = importer.Import();

                run.ScanToNETAlignmentData = umcs.GetScanNETLookupTable();
                run.UpdateNETAlignment();
                Console.WriteLine(run.DatasetName + " aligned.");

            }
            else
            {
                Console.WriteLine(run.DatasetName + " NOT aligned.");
            }



            string sourcePeaksFile;
            if (peaksFile == null || peaksFile == String.Empty)
            {
                sourcePeaksFile = run.DataSetPath + "\\" + run.DatasetName + "_peaks.txt";
            }
            else
            {
                sourcePeaksFile = peaksFile;
            }

            GetPeaks(run, sourcePeaksFile);

           // Console.WriteLine("Peaks loaded = " + run.ResultCollection.MSPeakResultList.Count);
            return run;
        }

        public static Run CreateAndLoadPeaks(string rawdataFilename, string peaksTestFile)
        {
            RunFactory rf = new RunFactory();
            Run run = rf.CreateRun(rawdataFilename);

            //Console.WriteLine(run.DatasetName + " loaded.");

            string basePath = run.DataSetPath;


            string sourcePeaksFile;
            if (peaksTestFile == null || peaksTestFile == String.Empty)
            {
                sourcePeaksFile = run.DataSetPath + "\\" + run.DatasetName + "_peaks.txt";
            }
            else
            {
                sourcePeaksFile = peaksTestFile;
            }

            GetPeaks(run, sourcePeaksFile);

            // Console.WriteLine("Peaks loaded = " + run.ResultCollection.MSPeakResultList.Count);
            return run;
        }
    }
}
