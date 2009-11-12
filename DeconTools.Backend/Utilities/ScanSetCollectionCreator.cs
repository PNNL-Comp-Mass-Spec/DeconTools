using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Utilities
{
    public class ScanSetCollectionCreator
    {

        public ScanSetCollectionCreator(Run run, int start, int stop, int range, int increment)
        {
            this.run = run;
            this.startScan = start;
            this.stopScan = stop;
            this.range = range;
            this.increment = increment;

        }

        public ScanSetCollectionCreator(Run run, int range, int increment)
            : this(run, run.MinScan, run.GetMaxPossibleScanIndex(), range, increment)
        {

        }

        private int startScan;
        private int stopScan;
        private Run run;
        private int range;
        private int increment;

        public void Create()
        {
            bool isRangeOdd = (range % 2 == 1 && range > 0);

            if (this.stopScan < this.startScan)
            {
                this.stopScan = this.startScan;
            }

            Check.Require(run != null, "Run is null");
            Check.Require(isRangeOdd, "Range value must be an odd number");
            Check.Require(increment > 0, "Increment must be greater than 0");

            run.ScanSetCollection = new ScanSetCollection();

            int maxPossibleScanIndex = run.GetMaxPossibleScanIndex();
            if (stopScan > maxPossibleScanIndex)
            {
                stopScan = maxPossibleScanIndex;
            }

            bool startAndStopLessThanMaxPossible = (startScan <= maxPossibleScanIndex && stopScan <= maxPossibleScanIndex);
            Check.Require(startAndStopLessThanMaxPossible, "Either the Start Scan or Stop Scan value exceeds the maximum possible value"); 


           for (int i = startScan; i <= stopScan; i = i + increment)
            {
                int lowerScan = i - ((range - 1) / 2);
                if (lowerScan < 0)
                {
                    lowerScan = Math.Abs(lowerScan) - 1;       //bounce effect...  -1 becomes 0; -2 becomes 1; -3 becomes 2;
                    if (lowerScan > maxPossibleScanIndex) lowerScan = maxPossibleScanIndex;    //this will be a very rare condition
                }


                int upperScan = i + ((range - 1) / 2);
                if (upperScan > maxPossibleScanIndex)
                {
                    upperScan = upperScan - (upperScan - maxPossibleScanIndex);   //bounce effect
                    if (upperScan < 0) upperScan = 0;         // rare condition
                }



                ScanSet scanSet = new ScanSet(i, lowerScan, upperScan);
                run.ScanSetCollection.ScanSetList.Add(scanSet);
            }


        }


    }
}
