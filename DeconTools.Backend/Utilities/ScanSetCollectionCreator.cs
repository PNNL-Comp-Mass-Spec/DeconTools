using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Utilities
{
    public class ScanSetCollectionCreator
    {

        public ScanSetCollectionCreator(Run run, int start, int stop, int numScansSummed, int increment, bool processMSMS)
        {
            this.run = run;
            this.startScan = start;
            this.stopScan = stop;
            this.numScansSummed = numScansSummed;
            this.increment = increment;
            this.processMSMS = processMSMS;
        }

        public ScanSetCollectionCreator(Run run, int start, int stop, int numScansSummed, int increment)
            : this(run, start, stop, numScansSummed, increment, false)
        {

        }


        public ScanSetCollectionCreator(Run run, int range, int increment)
            : this(run, run.MinScan, run.GetMaxPossibleScanIndex(), range, increment, false)
        {


        }

        private int startScan;
        private int stopScan;
        private Run run;
        private int numScansSummed;
        private int increment;
        private bool processMSMS;

       
        public void Create()
        {

            //  TODO:   the advancement (increment) is not working right on Xcalibur data containing MS/MS data.  Need to fix this...
            //

            bool isNumScansOdd = (numScansSummed % 2 == 1 && numScansSummed > 0);

            if (this.stopScan < this.startScan)
            {
                this.stopScan = this.startScan;
            }

            Check.Require(run != null, "Run is null");
            Check.Require(isNumScansOdd, "Number of scans summed must be an odd number");
            Check.Require(increment > 0, "Increment must be greater than 0");

            run.ScanSetCollection = new ScanSetCollection();

            int maxPossibleScanIndex = run.GetMaxPossibleScanIndex();
            if (stopScan > maxPossibleScanIndex)
            {
                stopScan = maxPossibleScanIndex;
            }

            bool startAndStopLessThanMaxPossible = (startScan <= maxPossibleScanIndex && stopScan <= maxPossibleScanIndex);
            Check.Require(startAndStopLessThanMaxPossible, "Either the Start Scan or Stop Scan value exceeds the maximum possible value");


            for (int i = startScan; i <= stopScan; i++)
            {
                int currentMSLevel = run.GetMSLevel(i);

                if (!processMSMS && currentMSLevel > 1) continue;     // if we process only MS-level and scan i is an MSMS scan, then loop

                if (numScansSummed > 1)
                {
                }


                List<int> lowerScansToSum = getLowerScans(i, currentMSLevel, (numScansSummed - 1) / 2);
                List<int> upperScansToSum = getUpperScans(i, currentMSLevel, (numScansSummed - 1) / 2);

                List<int> scansToSum = lowerScansToSum;
                scansToSum.Add(i);
                scansToSum.AddRange(upperScansToSum);

                //List<int> lowerScans = getLowerScans(i, currentMSLevel, (numScansSummed - 1) / 2);

                //int lowerScan = i - ((numScansSummed - 1) / 2);
                //if (lowerScan < 0)
                //{
                //    lowerScan = Math.Abs(lowerScan) - 1;       //bounce effect...  -1 becomes 0; -2 becomes 1; -3 becomes 2;
                //    if (lowerScan > maxPossibleScanIndex) lowerScan = maxPossibleScanIndex;    //this will be a very rare condition
                //}


                //int upperScan = i + ((numScansSummed - 1) / 2);
                //if (upperScan > maxPossibleScanIndex)
                //{
                //    upperScan = upperScan - (upperScan - maxPossibleScanIndex);   //bounce effect
                //    if (upperScan < 0) upperScan = 0;         // rare condition
                //}


                scansToSum.Sort();
                ScanSet scanSet = new ScanSet(i, scansToSum.ToArray());
                run.ScanSetCollection.ScanSetList.Add(scanSet);

                i = i + increment - 1;   //  '-1' because we advance by +1 when the loop iterates. 

            }




        }



        private List<int> getLowerScans(int startingScan, int currentMSLevel, int numLowerScansToGet)
        {
            int currentScan = startingScan - 1;
            List<int> lowerScans = new List<int>();

            int scansCounter = 0;
            while (currentScan >= 1 && numLowerScansToGet > scansCounter)
            {
                if (run.GetMSLevel(currentScan) == currentMSLevel)
                {
                    lowerScans.Add(currentScan);
                    scansCounter++;
                }
                currentScan--;


            }
            return lowerScans;

        }
        private List<int> getUpperScans(int startingScan, int currentMSLevel, int numUpperScansToGet)
        {
            int currentScan = startingScan + 1;
            List<int> scans = new List<int>();

            int scansCounter = 0;
            int scanUppperLimit = run.GetNumMSScans();

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

    }
}
