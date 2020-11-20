using System.Collections.Generic;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.Core
{
    public class IMSScanSetCollection : ScanSetCollection
    {
        protected override int GetMinScan(Run run)
        {
            Check.Require(run is UIMFRun, "Trying to make the IMSScanSet collection, but Run is not a UIMF run");

            return ((UIMFRun)run).GetMinPossibleIMSScanNum();
        }

        protected override int GetMaxScan(Run run)
        {
            Check.Require(run is UIMFRun, "Trying to make the IMSScanSet collection, but Run is not a UIMF run");

            return ((UIMFRun)run).GetMaxPossibleIMSScanNum();
        }

        protected override void CreateScanSets(Run run, int scanStart, int scanStop, int numScansSummed, int scanIncrement, bool processMSMS = false, bool sumConsecutiveMsMs = true)
        {
            ScanSetList = new List<ScanSet>();

            var minPossibleScanIndex = GetMinScan(run);
            var maxPossibleScanIndex = GetMaxScan(run);

            var numLowerScansToGet = (numScansSummed - 1) / 2;
            var numUpperScansToGet = (numScansSummed - 1) / 2;

            for (var currentScan = scanStart; currentScan <= scanStop; currentScan++)
            {
                //add lower scans
                var lowerScan = currentScan - 1;
                var scansToSum = new List<int>();
                var scansCounter = 0;
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
                var upperScan = currentScan + 1;
                while (upperScan <= maxPossibleScanIndex && numUpperScansToGet > scansCounter)
                {
                    scansToSum.Add(upperScan);
                    scansCounter++;
                    upperScan++;
                }

                var scanSet = new IMSScanSet(currentScan, scansToSum.ToArray());
                ScanSetList.Add(scanSet);
            }
        }
    }
}
