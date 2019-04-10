using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.Core
{
    public class ScanSetFactory
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods

        public ScanSet CreateScanSet(Run run, int primaryScan, IEnumerable<int> scansToSum)
        {
            if (run is UIMFRun)
            {
                return new LCScanSetIMS(primaryScan, scansToSum);
            }

            return new ScanSet(primaryScan, scansToSum);
        }


        public ScanSet CreateScanSet(Run run, int primaryScan, int startScan, int stopScan)
        {
            var currentLevel = run.GetMSLevel(primaryScan);

            var scansToSum = new List<int>();

            for (var i = startScan; i <= stopScan; i++)
            {
                if (run.GetMSLevel(i) == currentLevel)
                {
                    scansToSum.Add(i);
                }

            }
            return CreateScanSet(run, primaryScan, scansToSum);

            //return new ScanSet(primaryScan, scansToSum.ToArray());

        }


        public ScanSet CreateScanSet(Run run, int scan, int scansSummed)
        {
            var currentLevel = run.GetMSLevel(scan);

            var lowerScansToSum = GetLowerScans(run, scan, currentLevel, (scansSummed - 1) / 2);
            var upperScansToSum = GetUpperScans(run, scan, currentLevel, (scansSummed - 1) / 2);

            var scansToSum = lowerScansToSum.OrderBy(p => p).ToList();
            scansToSum.Add(scan);
            scansToSum.AddRange(upperScansToSum);
            //scansToSum.Sort();

            return CreateScanSet(run, scan, scansToSum);
            //return new ScanSet(scan, scansToSum.ToArray());

        }


        public void TrimScans(ScanSet scanSet, int maxScansAllowed)
        {
            Check.Require(maxScansAllowed > 0, "Scans cannot be trimmed to fewer than one");

            if (scanSet.IndexValues.Count > maxScansAllowed)
            {
                var numScansToBeRemoved = (scanSet.IndexValues.Count - maxScansAllowed + 1) / 2;

                var newScans = new List<int>();

                for (var i = numScansToBeRemoved; i < (scanSet.IndexValues.Count - numScansToBeRemoved); i++)    //this loop will cleave off the first n scans and the last n scans
                {
                    newScans.Add(scanSet.IndexValues[i]);
                }

                scanSet.IndexValues = newScans;
            }

        }

        #endregion

        private IEnumerable<int> GetLowerScans(Run run, int startingScan, int currentMSLevel, int numLowerScansToGet)
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
        private IEnumerable<int> GetUpperScans(Run run, int startingScan, int currentMSLevel, int numUpperScansToGet)
        {
            var currentScan = startingScan + 1;
            var scans = new List<int>();

            var scansCounter = 0;
            var scanUpperLimit = run.GetNumMSScans();

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


        #region Private Methods
        #endregion
    }
}
