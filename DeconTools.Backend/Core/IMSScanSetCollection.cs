using System.Collections.Generic;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.Core
{
    public class IMSScanSetCollection : ScanSetCollection
    {
        

        protected override int GetMinScan(Run run)
        {
            Check.Require(run is UIMFRun, "Trying to make the IMSScanset collection, but Run is not a UIMF run");

            return ((UIMFRun)run).GetMinPossibleIMSScanNum();


        }

        protected override int GetMaxScan(Run run)
        {
            Check.Require(run is UIMFRun, "Trying to make the IMSScanset collection, but Run is not a UIMF run");

            return ((UIMFRun)run).GetMaxPossibleIMSScanNum();
        }

        protected override void CreateScansets(Run run, int scanStart, int scanStop, int numScansSummed, int scanIncrement, bool processMSMS = false, bool sumConsecutiveMsMs = true)
        {
               ScanSetList = new List<ScanSet>();

               int minPossibleScanIndex = GetMinScan(run);
               int maxPossibleScanIndex = GetMaxScan(run);


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

                   var scanSet =new IMSScanSet(currentScan, scansToSum.ToArray());
                   ScanSetList.Add(scanSet);
               }

               

        }


    }
}
