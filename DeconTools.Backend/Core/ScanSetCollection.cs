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
            this.ScanSetList = new List<ScanSet>();
        }

        public void Create(Run run, int numScansSummed, int scanIncrement, bool processMSMS = true)
        {
            Check.Require(run != null, "Cannot create target scans. Run is null");

            Create(run, GetMinScan(run), GetMaxScan(run), numScansSummed, scanIncrement, processMSMS);
        }

        public void Create(Run run, int scanStart, int scanStop, int numScansSummed, int scanIncrement, bool processMSMS = true)
        {
            bool isNumScansOdd = (numScansSummed % 2 == 1 && numScansSummed > 0);

            if (scanStop < scanStart)
            {
                scanStop = scanStart;
            }

            Check.Require(run != null, "Cannot create target scans. Run is null");
            Check.Require(isNumScansOdd, "Number of scans summed must be an odd number");
            Check.Require(scanIncrement > 0, "Scan Increment must be greater than 0");

            CreateScansets(run, scanStart, scanStop, numScansSummed,
                                                                                  scanIncrement, processMSMS);

        }

        public void Create(Run run, bool sumAllScans = true, bool processMSMS = true)
        {

            ScanSetList = new List<ScanSet>();


            int minScan = GetMinScan(run);
            int maxScan = GetMaxScan(run);


            if (sumAllScans)
            {
                //find first MS1 scan

                int firstMS1Scan = -1;
                for (int i = minScan; i <= maxScan; i++)
                {
                    if (run.GetMSLevel(i) == 1)
                    {
                        firstMS1Scan = i;
                        break;

                    }
                }

                if (firstMS1Scan == -1)   //never found a single MS1 scan in the whole dataset
                {

                }
                else
                {
                    var scanSetFactory = new ScanSetFactory();
                    var scanset = scanSetFactory.CreateScanSet(run, firstMS1Scan, minScan, maxScan);

                    ScanSetList.Add(scanset);
                }

            }
            else
            {

                for (int i = minScan; i <= maxScan; i++)
                {
                    var mslevel = run.GetMSLevel(i);

                    if (mslevel == 1 || processMSMS)
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

        protected virtual void CreateScansets(Run run, int scanStart, int scanStop, int numScansSummed, int scanIncrement, bool processMSMS = false)
        {

            ScanSetList = new List<ScanSet>();

            int minPossibleScanIndex = GetMinScan(run);
            int maxPossibleScanIndex = GetMaxScan(run);

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
                scanStop = minPossibleScanIndex;
            }

            if (scanStop > maxPossibleScanIndex)
            {
                scanStop = maxPossibleScanIndex;
            }


            for (int i = scanStart; i <= scanStop; i++)
            {
                int currentMSLevel = run.GetMSLevel(i);

                if (!processMSMS && currentMSLevel > 1) continue;     // if we process only MS-level and scan i is an MSMS scan, then loop

                ScanSet scanSet;
                if (currentMSLevel == 1)
                {
                    List<int> lowerScansToSum = GetLowerScans(run, i, currentMSLevel, (numScansSummed - 1) / 2);
                    List<int> upperScansToSum = GetUpperScans(run, i, currentMSLevel, (numScansSummed - 1) / 2);

                    List<int> scansToSum = lowerScansToSum;
                    scansToSum.Add(i);
                    scansToSum.AddRange(upperScansToSum);

                    scanSet = ScanSetFactory.CreateScanSet(run, i, scansToSum);


                }
                else
                {
                    //NOTE: we wish to sum non-adjacent UIMF MS/MS frames. I.e. Frame 2 and 7 were both fully fragmented using the same voltage. We wish to sum these together
                    //NOTE: we may want to abstract this section out somehow. 
                    if (run is UIMFRun)
                    {
                        var uimfrun = (UIMFRun) run;

                        var numberOfFrameIndexesToSkip = uimfrun.GetNumberOfConsecutiveMs2Frames();

                        int indexOfCurrentFrame = uimfrun.MS2Frames.IndexOf(i);

                        if (indexOfCurrentFrame < 0) continue;

                        int lowerIndex = indexOfCurrentFrame - numberOfFrameIndexesToSkip;
                        int upperIndex = indexOfCurrentFrame + numberOfFrameIndexesToSkip;

                        List<int> framesToSum = new List<int>();
                        int numLowerFramesToGet = (numScansSummed - 1) / 2;
                        int numUpperFramesToGet = (numScansSummed - 1) / 2;

                        //get lower frames
                        int framesCounter = 0;
                        while (lowerIndex >= 0 && numLowerFramesToGet > framesCounter)
                        {
                            framesToSum.Insert(0, uimfrun.MS2Frames[lowerIndex]);
                            lowerIndex -= numberOfFrameIndexesToSkip;
                            framesCounter++;
                        }

                        //get middle frame
                        framesToSum.Add(i);

                        //get upper frames
                        framesCounter = 0;
                        int maxPossibleFrameIndex = uimfrun.MS2Frames.Count - 1;
                        while (upperIndex <= maxPossibleFrameIndex && numUpperFramesToGet > framesCounter)
                        {
                            framesToSum.Add(uimfrun.MS2Frames[upperIndex]);
                            upperIndex += numberOfFrameIndexesToSkip;
                            framesCounter++;
                        }

                        scanSet = ScanSetFactory.CreateScanSet(run,i, framesToSum.ToArray());
                        
                    }
                    else
                    {
                        scanSet = ScanSetFactory.CreateScanSet(run, i, 1);    
                    }
                }
                ScanSetList.Add(scanSet);

                i = i + scanIncrement - 1;   //  '-1' because we advance by +1 when the loop iterates. 

            }

        }

        private static List<int> GetLowerScans(Run run, int startingScan, int currentMSLevel, int numLowerScansToGet)
        {
            int currentScan = startingScan - 1;
            List<int> lowerScans = new List<int>();

            int scansCounter = 0;
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
            int currentScan = startingScan + 1;
            var scans = new List<int>();

            int scansCounter = 0;
            int scanUppperLimit = run.GetMaxPossibleLCScanNum();

            while (currentScan <= scanUppperLimit && numUpperScansToGet > scansCounter)
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
            if (this.ScanSetList == null || this.ScanSetList.Count == 0) return null;

            return (this.ScanSetList.Find(p => p.PrimaryScanNumber == primaryNum));

        }

        public ScanSet GetNextMSScanSet(Run run, int primaryNum, bool ascending)
        {
            if (this.ScanSetList == null || this.ScanSetList.Count == 0) return null;

            ScanSet scan = ScanSetList.Find(p => p.PrimaryScanNumber == primaryNum);

            if (run.GetMSLevel(scan.PrimaryScanNumber) == 1)
            {
                return scan;
            }
            else
            {
                if (ascending)
                {
                    scan = GetNextMSScanSet(run, ++primaryNum, ascending);
                }
                else
                {
                    scan = GetNextMSScanSet(run, --primaryNum, ascending);
                }
            }

            return scan;
        }

        public int GetLastScanSet()
        {
            if (ScanSetList == null || ScanSetList.Count == 0) return -1;
            return (ScanSetList[ScanSetList.Count - 1].PrimaryScanNumber);
        }
    }
}
