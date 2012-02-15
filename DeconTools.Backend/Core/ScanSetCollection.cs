using System;
using System.Collections.Generic;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class ScanSetCollection
    {
        public ScanSetCollection()
        {
            this.ScanSetList = new List<ScanSet>();
        }

        public static ScanSetCollection Create(Run run, int numScansSummed, int scanIncrement, bool processMSMS = true)
        {
            Check.Require(run != null, "Cannot create target scans. Run is null");
            return Create(run, run.GetMinPossibleScanNum(), run.GetMaxPossibleScanNum(), numScansSummed, scanIncrement,
                          processMSMS);

        }


        public static ScanSetCollection Create(Run run, int scanStart, int scanStop, int numScansSummed, int scanIncrement, bool processMSMS = true)
        {
            ScanSetCollection scanSetCollection;

            bool isNumScansOdd = (numScansSummed % 2 == 1 && numScansSummed > 0);

            if (scanStop < scanStart)
            {
                scanStop = scanStart;
            }

            Check.Require(run != null, "Cannot create target scans. Run is null");
            Check.Require(isNumScansOdd, "Number of scans summed must be an odd number");
            Check.Require(scanIncrement > 0, "Scan Increment must be greater than 0");



            //For IMS data, 'scan' refers to IMS_Scan'. IMS_Scans are always of the same type. No intermixing of MS1 and MS2 scans. So no need to check MSLevel
            if (run is UIMFRun || run is IMFRun)
            {
                scanSetCollection = CreateScanSetCollectionMS1OnlyData(run, scanStart, scanStop, numScansSummed,
                                                                scanIncrement);
            }
            else
            {
                scanSetCollection = CreateStandardScanSetCollection(run, scanStart, scanStop, numScansSummed,
                                                                    scanIncrement, processMSMS);
            }

            return scanSetCollection;
        }

        private static ScanSetCollection CreateScanSetCollectionMS1OnlyData(Run run, int scanStart, int scanStop, int numScansSummed, int scanIncrement)
        {
            int minPossibleScanIndex = run.GetMinPossibleScanNum();
            int maxPossibleScanIndex = run.GetMaxPossibleScanNum();

            ScanSetCollection scanSetCollection = new ScanSetCollection();


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


            int numLowerScansToGet = (numScansSummed - 1) / 2;
            int numUpperScansToGet = (numScansSummed - 1) / 2;

            for (int currentScan = scanStart; currentScan <= scanStop; currentScan++)
            {
                //add lower scans
                int lowerScan = currentScan - 1;
                List<int> scansToSum = new List<int>();
                int scansCounter = 0;
                while (lowerScan >= minPossibleScanIndex && numLowerScansToGet > scansCounter)
                {
                    scansToSum.Insert(0, lowerScan);
                    scansCounter++;

                    lowerScan--;
                }

                //add middle scan
                scansToSum.Add(currentScan);


                //add upper scans
                scansCounter = 0;
                int upperScan = currentScan + 1;
                while (upperScan <= maxPossibleScanIndex && numUpperScansToGet > scansCounter)
                {
                    scansToSum.Add(upperScan);
                    scansCounter++;
                    upperScan++;
                }

                var scanSet = new ScanSet(currentScan, scansToSum.ToArray());
                scanSetCollection.ScanSetList.Add(scanSet);
            }

            return scanSetCollection;



        }

        private static ScanSetCollection CreateStandardScanSetCollection(Run run, int scanStart, int scanStop, int numScansSummed, int scanIncrement, bool processMSMS = false)
        {
            int minPossibleScanIndex = run.GetMinPossibleScanNum();
            int maxPossibleScanIndex = run.GetMaxPossibleScanNum();

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


            ScanSetCollection scanSetCollection = new ScanSetCollection();
            for (int i = scanStart; i <= scanStop; i++)
            {
                int currentMSLevel = run.GetMSLevel(i);

                if (!processMSMS && currentMSLevel > 1) continue;     // if we process only MS-level and scan i is an MSMS scan, then loop

                ScanSet scanSet;
                if (currentMSLevel==1)
                {
                    List<int> lowerScansToSum = GetLowerScans(run, i, currentMSLevel, (numScansSummed - 1) / 2);
                    List<int> upperScansToSum = GetUpperScans(run, i, currentMSLevel, (numScansSummed - 1) / 2);

                    List<int> scansToSum = lowerScansToSum;
                    scansToSum.Add(i);
                    scansToSum.AddRange(upperScansToSum);

                    scanSet = new ScanSet(i, scansToSum.ToArray());
                }
                else
                {
                    scanSet = new ScanSet(i);

                }

                
                scanSetCollection.ScanSetList.Add(scanSet);

                i = i + scanIncrement - 1;   //  '-1' because we advance by +1 when the loop iterates. 

            }

            return scanSetCollection;
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
                    lowerScans.Insert(0,currentScan);
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
            int scanUppperLimit = run.GetMaxPossibleScanNum();

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

        public List<ScanSet> ScanSetList { get; set; }

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
