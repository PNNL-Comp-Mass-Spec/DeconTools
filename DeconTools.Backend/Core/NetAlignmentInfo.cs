using System;
using System.Collections.Generic;
using System.Linq;

namespace DeconTools.Backend.Core
{
    public abstract class NetAlignmentInfo
    {

        #region Constructors

        protected NetAlignmentInfo(int minLcScan, int maxLcScan)
        {
            MinLcScan = minLcScan;
            MaxLcScan = maxLcScan;

            ScanToNETAlignmentData = new SortedDictionary<int, float>();

        }


        #endregion

        #region Properties

        public SortedDictionary<int, float> ScanToNETAlignmentData { get; set; }

        //Maximum possible LcScan. We need this for calculating NET. The min LcScan = NET 0.0
        public int MinLcScan { get; set; }

        //Maximum possible LcScan. We need this for calculating NET. The max LcScan = NET 1.0
        public int MaxLcScan { get; set; }



        #endregion

        #region Public Methods



        public virtual double GetNETValueForScan(int scanNum)
        {
            if (ScanToNETAlignmentData == null || ScanToNETAlignmentData.Count == 0)
            {
                CreateDefaultScanToNETAlignmentData();

                var scanToNETTableIsStillEmpty = ScanToNETAlignmentData == null || ScanToNETAlignmentData.Count == 0;
                if (scanToNETTableIsStillEmpty)
                {
                    throw new ArgumentException("Scan-to-NET table is empty. Tried to create it from Dataset but failed.");
                }
            }
            
            if (ScanToNETAlignmentData.ContainsKey(scanNum))
            {
                return ScanToNETAlignmentData[scanNum];
            }
            
            return GetInterpolatedNet(scanNum);
        }


        public virtual double GetScanForNet(double net)
        {
            //need to find the two (scan,net) pairs that are the lower and upper boundaries of the input NET
            //then do an intersect

            var closestNETPair = new KeyValuePair<int, float>();


            var lowerScan = MinLcScan;
            var upperScan = MaxLcScan;

            float lowerNET = 0;
            float upperNET = 1;


            //first find the closest ScanNET pair
            var diff = double.MaxValue;
            foreach (var item in ScanToNETAlignmentData)
            {
                var currentDiff = Math.Abs(item.Value - net);
                if (currentDiff < diff)
                {
                    closestNETPair = item;
                    diff = currentDiff;
                }
            }

            //we found either the point above the inputted NET or below. Need to fill the appropriate lower and upper scan/NET
            var isLowerThanInputNET = closestNETPair.Value <= net;

            if (isLowerThanInputNET)
            {
                lowerScan = closestNETPair.Key;
                lowerNET = closestNETPair.Value;

                var found = false;
                var currentScan = lowerScan + 1; //add one and then start looking for next higher scan
                while (!found && currentScan <= MaxLcScan)
                {
                    currentScan++;
                    if (ScanToNETAlignmentData.ContainsKey(currentScan))
                    {
                        upperScan = currentScan;
                        upperNET = ScanToNETAlignmentData[upperScan];
                        found = true;
                    }
                }
            }
            else
            {
                upperScan = closestNETPair.Key;
                upperNET = closestNETPair.Value;

                var found = false;
                var currentScan = upperScan - 1;

                while (!found && currentScan >= MinLcScan)
                {
                    currentScan--;
                    if (ScanToNETAlignmentData.ContainsKey(currentScan))
                    {
                        lowerScan = currentScan;
                        lowerNET = ScanToNETAlignmentData[lowerScan];
                        found = true;
                    }
                }
            }


            if (upperScan <= lowerScan)    //this happens at the MinScan
            {

                return lowerScan;
            }

            var slope = (upperNET - lowerNET) / (upperScan - lowerScan);
            var yintercept = (upperNET - slope * upperScan);

            var xvalue = (net - yintercept) / slope;

            if (xvalue < MinLcScan)
            {
                xvalue = MinLcScan;
            }

            if (xvalue > MaxLcScan)
            {
                xvalue = MaxLcScan;
            }

            return xvalue;




        }



        public virtual void CreateDefaultScanToNETAlignmentData()
        {
            ScanToNETAlignmentData = new SortedDictionary<int, float>();

            var scanNETList = new List<ScanNETPair>();

            for (var i = MinLcScan; i <= MaxLcScan; i++)
            {
                var snp = new ScanNETPair(i, i/(float) MaxLcScan);
                scanNETList.Add(snp);
            }

            SetScanToNETAlignmentData(scanNETList);
           

        }



        public void SetScanToNETAlignmentData(List<ScanNETPair> scanNETList)
        {
            var scanVals = scanNETList.Select(p => p.Scan).ToArray();
            var netVals = scanNETList.Select(p => p.NET).ToArray();

            SetScanToNETAlignmentData(scanVals, netVals);

        }

        public virtual void SetScanToNETAlignmentData(double[] scanVals, double[] netVals)
        {
            ScanToNETAlignmentData.Clear();


            for (var i = 0; i < scanVals.Length; i++)
            {
                var scanToAdd = (int) (Math.Round(scanVals[i]));

                if (!ScanToNETAlignmentData.ContainsKey(scanToAdd))
                {
                    ScanToNETAlignmentData.Add(scanToAdd, (float) netVals[i]);
                }
            }

        }


      

        private double GetInterpolatedNet(int scanNum)
        {
            if (scanNum < MinLcScan) return MinLcScan;
            var maxScan = MaxLcScan;


            double lowerNET = 0;
            double upperNET = 1;
            var lowerScan = MinLcScan;
            var upperScan = maxScan;


            var found = false;
            var currentScan = scanNum;

            while (!found && currentScan >= MinLcScan)
            {
                currentScan--;
                if (ScanToNETAlignmentData.ContainsKey(currentScan))
                {
                    lowerScan = currentScan;
                    lowerNET = ScanToNETAlignmentData[lowerScan];
                    found = true;
                }
            }

            found = false;
            currentScan = scanNum;
            while (!found && currentScan <= maxScan)
            {
                currentScan++;
                if (ScanToNETAlignmentData.ContainsKey(currentScan))
                {
                    upperScan = currentScan;
                    upperNET = ScanToNETAlignmentData[upperScan];
                    found = true;
                }

            }

            var slope = (upperNET - lowerNET) / (upperScan - lowerScan);
            var yintercept = (upperNET - slope * upperScan);

            return (scanNum * slope + yintercept);

        }






        #endregion

        #region Private Methods

        #endregion

    }
}
