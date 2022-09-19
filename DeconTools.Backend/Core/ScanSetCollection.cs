using System;
using System.Collections.Generic;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class ScanSetCollection
    {
        protected ScanSetFactory ScanSetFactory = new ScanSetFactory();

        public ScanSetCollection()
        {
            ScanSetList = new List<ScanSet>();
        }

        public void Create(Run run, int numScansSummed, int scanIncrement, bool processMSMS = true)
        {
            Check.Require(run != null, "Cannot create target scans. Run is null");

            Create(run, GetMinScan(run), GetMaxScan(run), numScansSummed, scanIncrement, processMSMS);
        }

        public void Create(Run run, int scanStart, int scanStop, int numScansSummed, int scanIncrement, bool processMSMS = true)
        {
            var isNumScansOdd = (numScansSummed % 2 == 1 && numScansSummed > 0);

            if (scanStop < scanStart)
            {
                scanStop = scanStart;
            }

            Check.Require(run != null, "Cannot create target scans. Run is null");
            Check.Require(isNumScansOdd, "Number of scans summed must be an odd number");
            Check.Require(scanIncrement > 0, "Scan Increment must be greater than 0");

            CreateScanSets(run, scanStart, scanStop, numScansSummed, scanIncrement, processMSMS);
        }

        public void Create(Run run, bool sumAllScans = true, bool processMSMS = true)
        {
            ScanSetList = new List<ScanSet>();

            var minScan = GetMinScan(run);
            var maxScan = GetMaxScan(run);

            if (sumAllScans)
            {
                // Find first MS1 scan

                var firstMS1Scan = -1;
                for (var i = minScan; i <= maxScan; i++)
                {
                    if (run.GetMSLevel(i) == 1)
                    {
                        firstMS1Scan = i;
                        break;
                    }
                }

                if (firstMS1Scan == -1)   // Never found a single MS1 scan in the whole dataset
                {
                }
                else
                {
                    var scanSetFactory = new ScanSetFactory();
                    var scanSet = scanSetFactory.CreateScanSet(run, firstMS1Scan, minScan, maxScan);

                    ScanSetList.Add(scanSet);
                }
            }
            else
            {
                for (var i = minScan; i <= maxScan; i++)
                {
                    var msLevel = run.GetMSLevel(i);

                    if (msLevel == 1 || processMSMS)
                    {
                        ScanSetList.Add(new ScanSet(i));
                    }
                }
            }
        }

        #region Properties

        public List<ScanSet> ScanSetList { get; set; }

        #endregion

        protected virtual int GetMinScan(Run run)
        {
            return run.GetMinPossibleLCScanNum();
        }

        protected virtual int GetMaxScan(Run run)
        {
            return run.GetMaxPossibleLCScanNum();
        }

        //private static ScanSetCollection CreateScanSetCollectionMS1OnlyData(Run run, int scanStart, int scanStop, int numScansSummed, int scanIncrement)
        //{
        //    int minPossibleScanIndex = getMinScan(run);
        //    int maxPossibleScanIndex = getMaxScan(run);

        //    ScanSetCollection scanSetCollection = new ScanSetCollection();

        //    if (scanStart < minPossibleScanIndex)
        //    {
        //        scanStart = minPossibleScanIndex;
        //    }

        //    if (scanStart > maxPossibleScanIndex)
        //    {
        //        scanStart = maxPossibleScanIndex;
        //    }

        //    if (scanStop < minPossibleScanIndex)
        //    {
        //        scanStop = minPossibleScanIndex;
        //    }

        //    if (scanStop > maxPossibleScanIndex)
        //    {
        //        scanStop = maxPossibleScanIndex;
        //    }

        //    int numLowerScansToGet = (numScansSummed - 1) / 2;
        //    int numUpperScansToGet = (numScansSummed - 1) / 2;

        //    for (int currentScan = scanStart; currentScan <= scanStop; currentScan++)
        //    {
        //        //add lower scans
        //        int lowerScan = currentScan - 1;
        //        List<int> scansToSum = new List<int>();
        //        int scansCounter = 0;
        //        while (lowerScan >= minPossibleScanIndex && numLowerScansToGet > scansCounter)
        //        {
        //            scansToSum.Insert(0, lowerScan);
        //            scansCounter++;

        //            lowerScan--;
        //        }

        //        //add middle scan
        //        scansToSum.Add(currentScan);

        //        //add upper scans
        //        scansCounter = 0;
        //        int upperScan = currentScan + 1;
        //        while (upperScan <= maxPossibleScanIndex && numUpperScansToGet > scansCounter)
        //        {
        //            scansToSum.Add(upperScan);
        //            scansCounter++;
        //            upperScan++;
        //        }

        //        var scanSet = new ScanSet(currentScan, scansToSum.ToArray());
        //        scanSetCollection.ScanSetList.Add(scanSet);
        //    }

        //    return scanSetCollection;

        //}

        protected virtual void CreateScanSets(Run run, int scanStart, int scanStop, int numScansSummed, int scanIncrement, bool processMSMS = false, bool sumConsecutiveMsMs = true)
        {
            ScanSetList = new List<ScanSet>();

            var minPossibleScanIndex = GetMinScan(run);
            var maxPossibleScanIndex = GetMaxScan(run);

            if (scanStart < 0 && scanStop < 0)
            {
                scanStart = minPossibleScanIndex;
                scanStop = maxPossibleScanIndex;
            }

            if (scanStart < minPossibleScanIndex)
            {
                scanStart = minPossibleScanIndex;
            }

            if (scanStart > maxPossibleScanIndex)
            {
                scanStart = maxPossibleScanIndex;
            }

            if (scanStop < minPossibleScanIndex)
            {
                scanStop = maxPossibleScanIndex;
            }

            if (scanStop > maxPossibleScanIndex)
            {
                scanStop = maxPossibleScanIndex;
            }

            var haveProcessedCurrentMsMsSet = false;

            for (var i = scanStart; i <= scanStop; i += scanIncrement)
            {
                var currentMSLevel = run.GetMSLevel(i);

                if (run is UIMFRun && currentMSLevel == 0)
                {
                    continue;  // This is a calibration frame; skip it
                }

                if (!processMSMS && currentMSLevel > 1)
                {
                    continue;     // if we process only MS-level and scan i is an MSMS scan, then loop
                }

                ScanSet scanSet;
                if (currentMSLevel == 1)
                {
                    var lowerScansToSum = GetLowerScans(run, i, currentMSLevel, (numScansSummed - 1) / 2);
                    var upperScansToSum = GetUpperScans(run, i, currentMSLevel, (numScansSummed - 1) / 2);

                    var scansToSum = lowerScansToSum;
                    scansToSum.Add(i);
                    scansToSum.AddRange(upperScansToSum);

                    scanSet = ScanSetFactory.CreateScanSet(run, i, scansToSum);

                    haveProcessedCurrentMsMsSet = false;
                }
                else
                {
                    // NOTE: we wish to sum non-adjacent UIMF MS/MS frames. I.e. Frame 2 and 7 were both fully fragmented using the same voltage. We wish to sum these together
                    // NOTE: we may want to abstract this section out somehow.
                    if (run is UIMFRun uimfRun)
                    {
                        // TODO: Use a parameter to see if we want to sum all collision energies together or not. Currently hard-coded as default to true
                        if (sumConsecutiveMsMs)
                        {
                            if (!haveProcessedCurrentMsMsSet)
                            {
                                var framesToSum = new List<int>();

                                var numberOfConsecutiveMs2Frames = uimfRun.GetNumberOfConsecutiveMs2Frames(i);

                                for (var j = 0; j < numberOfConsecutiveMs2Frames; j++)
                                {
                                    framesToSum.Add(i + j);
                                }

                                scanSet = ScanSetFactory.CreateScanSet(uimfRun, i, framesToSum.ToArray());

                                haveProcessedCurrentMsMsSet = true;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            var numberOfFrameIndexesToSkip = uimfRun.GetNumberOfConsecutiveMs2Frames(i);

                            var indexOfCurrentFrame = uimfRun.MS2Frames.IndexOf(i);

                            if (indexOfCurrentFrame < 0)
                            {
                                continue;
                            }

                            var lowerIndex = indexOfCurrentFrame - numberOfFrameIndexesToSkip;
                            var upperIndex = indexOfCurrentFrame + numberOfFrameIndexesToSkip;

                            var framesToSum = new List<int>();
                            var numLowerFramesToGet = (numScansSummed - 1) / 2;
                            var numUpperFramesToGet = (numScansSummed - 1) / 2;

                            // Get lower frames
                            var framesCounter = 0;
                            while (lowerIndex >= 0 && numLowerFramesToGet > framesCounter)
                            {
                                framesToSum.Insert(0, uimfRun.MS2Frames[lowerIndex]);
                                lowerIndex -= numberOfFrameIndexesToSkip;
                                framesCounter++;
                            }

                            // Get middle frame
                            framesToSum.Add(i);

                            // Get upper frames
                            framesCounter = 0;
                            var maxPossibleFrameIndex = uimfRun.MS2Frames.Count - 1;
                            while (upperIndex <= maxPossibleFrameIndex && numUpperFramesToGet > framesCounter)
                            {
                                framesToSum.Add(uimfRun.MS2Frames[upperIndex]);
                                upperIndex += numberOfFrameIndexesToSkip;
                                framesCounter++;
                            }

                            scanSet = ScanSetFactory.CreateScanSet(uimfRun, i, framesToSum.ToArray());
                        }
                    }
                    else
                    {
                        scanSet = ScanSetFactory.CreateScanSet(run, i, 1);
                    }
                }

                ScanSetList.Add(scanSet);
                run.PrimaryLcScanNumbers.Add(scanSet.PrimaryScanNumber);
            }
        }

        private static List<int> GetLowerScans(Run run, int startingScan, int currentMSLevel, int numLowerScansToGet)
        {
            var currentScan = startingScan - 1;
            var lowerScans = new List<int>();

            var scansCounter = 0;
            while (currentScan >= 1 && numLowerScansToGet > scansCounter)
            {
                if (run.GetMSLevel(currentScan) == currentMSLevel)
                {
                    lowerScans.Insert(0, currentScan);
                    scansCounter++;
                }
                currentScan--;
            }
            return lowerScans;
        }
        private static List<int> GetUpperScans(Run run, int startingScan, int currentMSLevel, int numUpperScansToGet)
        {
            var currentScan = startingScan + 1;
            var scans = new List<int>();

            var scansCounter = 0;
            var scanUpperLimit = run.GetMaxPossibleLCScanNum();

            while (currentScan <= scanUpperLimit && numUpperScansToGet > scansCounter)
            {
                if (run.GetMSLevel(currentScan) == currentMSLevel)
                {
                    scans.Add(currentScan);
                    scansCounter++;
                }
                currentScan++;
            }
            return scans;
        }

        public ScanSet GetScanSet(int primaryNum)
        {
            if (ScanSetList == null || ScanSetList.Count == 0)
            {
                return null;
            }

            return (ScanSetList.Find(p => p.PrimaryScanNumber == primaryNum));
        }

        public ScanSet GetNextMSScanSet(Run run, int primaryNum, bool ascending)
        {
            if (ScanSetList == null || ScanSetList.Count == 0)
            {
                return null;
            }

            var scan = ScanSetList.Find(p => p.PrimaryScanNumber == primaryNum);

            if (run.GetMSLevel(scan.PrimaryScanNumber) == 1)
            {
                return scan;
            }

            if (ascending)
            {
                scan = GetNextMSScanSet(run, ++primaryNum, true);
            }
            else
            {
                scan = GetNextMSScanSet(run, --primaryNum, false);
            }

            return scan;
        }

        public int GetLastScanSet()
        {
            if (ScanSetList == null || ScanSetList.Count == 0)
            {
                return -1;
            }

            return ScanSetList[ScanSetList.Count - 1].PrimaryScanNumber;
        }
    }
}
