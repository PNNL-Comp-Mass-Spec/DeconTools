using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.DTO;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.Utilities
{
    public class RunUtilities
    {

        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        public static string GetDatasetName(string datasetPath)
        {
            string datasetName;

            var attr = File.GetAttributes(datasetPath);

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var sourceDirInfo = new DirectoryInfo(datasetPath);
                datasetName = sourceDirInfo.Name;
            }
            else
            {
                datasetName = Path.GetFileNameWithoutExtension(datasetPath);
            }

            return datasetName;

        }

        public static string GetDatasetNameWithFileExtension(string datasetPath)
        {
            string datasetName;

            var attr = File.GetAttributes(datasetPath);

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var sourceDirInfo = new DirectoryInfo(datasetPath);
                datasetName = sourceDirInfo.Name;
            }
            else
            {
                datasetName = Path.GetFileName(datasetPath);
            }

            return datasetName;

        }


        /// <summary>
        /// Returns 'true' if Dataset is stored as a windows Folder/Directory  (e.g. Agilent.D mass spec data)
        /// Returns 'false' if Dataset is started as a standard Windows file  (e.g. Thermo .raw file)
        /// </summary>
        /// <param name="datasetPath"></param>
        /// <returns></returns>
        public static bool DatasetIsFolderStyle(string datasetPath)
        {
            var attr = File.GetAttributes(datasetPath);

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return true;
            }

            return false;
        }


        public static bool DatasetAlreadyExists(string datasetPath)
        {
            FileAttributes attr;

            try
            {
                attr = File.GetAttributes(datasetPath);
            }
            catch
            {
                return false;
            }

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var sourceDirInfo = new DirectoryInfo(datasetPath);
                return sourceDirInfo.Exists;
            }

            return File.Exists(datasetPath);
        }

        public static string GetDatasetParentFolder(string datasetPath)
        {
            string datasetFolderPath;

            var attr = File.GetAttributes(datasetPath);

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var sourceDirInfo = new DirectoryInfo(datasetPath);
                datasetFolderPath = sourceDirInfo.FullName;
            }
            else
            {
                datasetFolderPath = Path.GetDirectoryName(datasetPath);
            }

            return datasetFolderPath;

        }



        public static Run CreateAndAlignRun(string filename)
        {
            return CreateAndAlignRun(filename, null);
        }

        public static void GetPeaks(Run run, string expectedPeaksFile, BackgroundWorker bw = null)
        {
            if (bw == null)
            {
                bw = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };
            }

            var peakImporter = new PeakImporterFromText(expectedPeaksFile, bw);
            peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

            if (!run.PrimaryLcScanNumbers.Any())
            {
                run.PrimaryLcScanNumbers = FindPrimaryLcScanNumbers(run.ResultCollection.MSPeakResultList);
            }
        }

        #endregion

        #region Private Methods

        #endregion

        public static bool AlignRunUsingAlignmentInfoInFiles(Run run, string alignmentDataFolder="")
        {
            bool alignmentSuccessful;


            string basePath;
            if (string.IsNullOrEmpty(alignmentDataFolder))
            {
                basePath= run.DataSetPath;
            }
            else
            {
                if (Directory.Exists(alignmentDataFolder))
                {
                    basePath = alignmentDataFolder;
                }
                else
                {
                    throw new DirectoryNotFoundException(
                        "Cannot align dataset. Source alignment folder does not exist. Alignment folder = " +
                        alignmentDataFolder);
                }
            }

            

            var expectedMZAlignmentFile = Path.Combine(basePath,  run.DatasetName + "_MZAlignment.txt");
            var expectedNETAlignmentFile = Path.Combine(basePath, run.DatasetName + "_NETAlignment.txt");

            //first will try to load the multiAlign alignment info
            if (File.Exists(expectedMZAlignmentFile))
            {
                var importer = new MassAlignmentInfoFromTextImporter(expectedMZAlignmentFile);

                var massAlignmentData = importer.Import();

                var massAlignmentInfo = new MassAlignmentInfoLcmsWarp();
                massAlignmentInfo.SetMassAlignmentData(massAlignmentData);
                run.MassAlignmentInfo = massAlignmentInfo;
            }

            if (File.Exists(expectedNETAlignmentFile))
            {
                var netAlignmentInfoImporter = new NETAlignmentFromTextImporter(expectedNETAlignmentFile);
                var scanNETList = netAlignmentInfoImporter.Import();

                NetAlignmentInfo netAlignmentInfo = new NetAlignmentInfoBasic(run.MinLCScan, run.MaxLCScan);
                netAlignmentInfo.SetScanToNETAlignmentData(scanNETList);

                run.NetAlignmentInfo = netAlignmentInfo;
                }

            //if still not aligned, try to get the NET alignment from UMCs file. (this is the older way to do it)
            if (run.NETIsAligned)
            {

                alignmentSuccessful = true;
            }
            else
            {
                var alignmentDirInfo = new DirectoryInfo(basePath);
                var umcFileInfo = alignmentDirInfo.GetFiles("*_umcs.txt");

                var umcFileCount = umcFileInfo.Count();

                if (umcFileCount == 1)
                {
                    var targetUmcFileName = umcFileInfo.First().FullName;

                    var importer = new UMCFileImporter(targetUmcFileName, '\t');
                    var umcs = importer.Import();

                    var scannetPairs = umcs.GetScanNETLookupTable();

                    NetAlignmentInfo netAlignmentInfo = new NetAlignmentInfoBasic(run.MinLCScan, run.MaxLCScan);
                    netAlignmentInfo.SetScanToNETAlignmentData(scannetPairs);

                    run.NetAlignmentInfo = netAlignmentInfo;
                    
                   
                    Console.WriteLine(run.DatasetName + " aligned.");
                    alignmentSuccessful = true;
                }
                else if (umcFileCount>1)
                {
                    var expectedUMCName = Path.Combine(basePath, run.DatasetName + "_UMCs.txt");

                    if (File.Exists(expectedUMCName))
                    {
                        var importer = new UMCFileImporter(expectedUMCName, '\t');
                        var umcs = importer.Import();

                        var scannetPairs = umcs.GetScanNETLookupTable();

                        NetAlignmentInfo netAlignmentInfo = new NetAlignmentInfoBasic(run.MinLCScan, run.MaxLCScan);
                        netAlignmentInfo.SetScanToNETAlignmentData(scannetPairs);

                        run.NetAlignmentInfo = netAlignmentInfo;
                       
                        Console.WriteLine(run.DatasetName + " NET aligned using UMC file: " + expectedUMCName);

                        alignmentSuccessful = true;
                    }
                    else
                    {
                        throw new FileNotFoundException("Trying to align dataset: " + run.DatasetName +
                                                        " but UMC file not found.");
                    }


                }
                else
                {
                    Console.WriteLine(run.DatasetName + " is NOT NET aligned.");
                    alignmentSuccessful = false;
                }
            }

            //mass is still not aligned... use data in viper output file: _MassAndGANETErrors_BeforeRefinement.txt
            if (run.MassIsAligned==false)
            {

                var expectedViperMassAlignmentFile = Path.Combine(basePath, run.DatasetName + "_MassAndGANETErrors_BeforeRefinement.txt");

                if (File.Exists(expectedViperMassAlignmentFile))
                {
                    
                    var importer = new ViperMassCalibrationLoader(expectedViperMassAlignmentFile);
                    var viperCalibrationData = importer.ImportMassCalibrationData();

                    var massAlignmentInfo = new MassAlignmentInfoLcmsWarp();
                    massAlignmentInfo.SetMassAlignmentData(viperCalibrationData);
                    run.MassAlignmentInfo = massAlignmentInfo;


                    Console.WriteLine(run.DatasetName + "- mass aligned using file: " + expectedViperMassAlignmentFile);

                    alignmentSuccessful = true;
                }
                else
                {
                    Console.WriteLine(run.DatasetName + " is NOT mass aligned");
                }

            }


            return alignmentSuccessful;
        }



        public static Run CreateAndAlignRun(string filename, string peaksFile)
        {

            var folderExists = Directory.Exists(filename);
            var fileExists = File.Exists(filename);
            
            
            Check.Require(folderExists||fileExists, "Dataset file not found error when RunUtilites tried to create Run.");
     
            var rf = new RunFactory();
            var run = rf.CreateRun(filename);

            Check.Ensure(run != null, "RunUtilites could not create run. Run is null.");

            //Console.WriteLine(run.DatasetName + " loaded.");


            AlignRunUsingAlignmentInfoInFiles(run);


            string sourcePeaksFile;
            if (string.IsNullOrEmpty(peaksFile))
            {
                // ReSharper disable once PossibleNullReferenceException (already checked for null above)
                sourcePeaksFile = Path.Combine(run.DataSetPath, run.DatasetName + "_peaks.txt");
            }
            else
            {
                sourcePeaksFile = peaksFile;
            }

            GetPeaks(run, sourcePeaksFile);

            // Console.WriteLine("Peaks loaded = " + run.ResultCollection.MSPeakResultList.Count);
            return run;
        }

        public static Run CreateAndLoadPeaks(string rawdataFilename)
        {
            return CreateAndLoadPeaks(rawdataFilename, string.Empty);
        }


        public static Run CreateAndLoadPeaks(string rawdataFilename, string peaksTestFile, BackgroundWorker bw = null)
        {
            var rf = new RunFactory();
            var run = rf.CreateRun(rawdataFilename);

            //Console.WriteLine(run.DatasetName + " loaded.");


            string sourcePeaksFile;
            if (string.IsNullOrEmpty(peaksTestFile))
            {
                sourcePeaksFile = Path.Combine(run.DataSetPath, run.DatasetName + "_peaks.txt");
            }
            else
            {
                sourcePeaksFile = peaksTestFile;
            }

            GetPeaks(run, sourcePeaksFile, bw);

            // Console.WriteLine("Peaks loaded = " + run.ResultCollection.MSPeakResultList.Count);
            return run;
        }

        public static List<int> FindPrimaryLcScanNumbers(IEnumerable<MSPeakResult> msPeaks)
        {
            var primaryLcScanNumbers = new HashSet<int>();
            
            foreach (var msPeakResult in msPeaks)
            {
                var scan = msPeakResult.FrameNum > 0 ? msPeakResult.FrameNum : msPeakResult.Scan_num;
                primaryLcScanNumbers.Add(scan);
            }

            return primaryLcScanNumbers.ToList();
        }
    }
}
