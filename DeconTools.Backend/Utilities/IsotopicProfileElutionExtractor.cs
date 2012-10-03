using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using DeconTools.Utilities;

namespace DeconTools.Backend.Utilities
{
    public class IsotopicProfileElutionExtractor
    {

        #region Constructors
        #endregion

        #region Properties


        public int[] Scans { get; set; }

        public double[] MzBins { get; set; }

        public int[] Intensities { get; set; }


        private int[,] _intensities2D;
        public int[,] Intensities2D
        {
            get
            {
                if (_intensities2D==null)
                {
                    _intensities2D = GetIntensitiesAs2DArray();
                }
                return _intensities2D;
            }
            set { _intensities2D = value; }
        }

        #endregion

        #region Public Methods



        public void Get3DElutionProfile(Run run, int minScan, int maxScan, double minMZ, double maxMZ, out int[] scans, out double[] mzBins, out int[] intensities, double binWidth = 0.01)
        {
            List<MSPeakResult> msPeakList = run.ResultCollection.MSPeakResultList;
            Check.Require(msPeakList != null && msPeakList.Count > 0, "Run is missing peaks. Elution profile is based on peak-level info");

            Check.Require(binWidth > 0, "Binwidth must be greater than 0");


            ScanSetCollection scanSetCollection = ScanSetCollection.Create(run, minScan, maxScan, 1, 1, false);

            


           


            //create bins
            int numBins = (int)Math.Round((maxMZ - minMZ) / binWidth);
            mzBins = new double[numBins];
            double firstBinMZ = minMZ;

            for (int i = 0; i < numBins; i++)
            {
                mzBins[i] = firstBinMZ + i * binWidth;
            }


            //iterate over scans and collected MzIntensityArray for each scan

            List<int> intensityList = new List<int>();



            scans = scanSetCollection.ScanSetList.Select(p => p.PrimaryScanNumber).ToArray();
            
            //gather relevant peaks. 
            int scanTolerance = 5;     // TODO:   keep an eye on this

            minScan = scans.First();
            maxScan = scans.Last();

            int indexOfLowerScan = getIndexOfClosestScanValue(msPeakList, minScan, 0, msPeakList.Count - 1, scanTolerance);
            int indexOfUpperScan = getIndexOfClosestScanValue(msPeakList, maxScan, 0, msPeakList.Count - 1, scanTolerance);

            int currentIndex = indexOfLowerScan;

            var filteredPeakList = new List<MSPeakResult>();
            while (currentIndex <= indexOfUpperScan)
            {
                var currentPeak = msPeakList[currentIndex];

                if (currentPeak.XValue >= minMZ && currentPeak.XValue <= maxMZ)
                {
                    filteredPeakList.Add(currentPeak);
                }
                currentIndex++;
            }



            foreach (var scan in scans)
            {
                var peaksForScan = filteredPeakList.Where(n => n.Scan_num == scan).ToList();

                int[] intensitiesForScan = new int[numBins];

                foreach (var msPeakResult in peaksForScan)
                {
                    int targetBin = (int)Math.Round((msPeakResult.XValue - firstBinMZ) / binWidth);
                    
                    if (targetBin < intensitiesForScan.Length)
                    {
                        intensitiesForScan[targetBin] += (int)Math.Round(msPeakResult.Height); 
                    }
                    


                }

                intensityList.AddRange(intensitiesForScan);
            }

            intensities = intensityList.ToArray();


            Scans = scans;
            Intensities = intensities;
            MzBins = mzBins;
        }

        public int[,] GetIntensitiesAs2DArray()
        {
            if (Intensities == null || Intensities.Length == 0 || Scans == null || Scans.Length == 0 || MzBins == null || MzBins.Length == 0)
            {
                return new int[0, 0];
            }
            
            int[,] twoDimensionalIntensityArray = new int[Scans.Length, MzBins.Length];

            int counter = 0;
            for (int scan = 0; scan < Scans.Length; scan++)
            {
                for (int mzBin = 0; mzBin < MzBins.Length; mzBin++)
                {
                    
                    twoDimensionalIntensityArray[scan, mzBin] = Intensities[ counter];
                    counter++;
                }
            }

            return twoDimensionalIntensityArray;
        }


        public string OutputElutionProfileAsString(char delimiter = '\t', bool outputZeroValues=true)
        {
            if (Intensities == null || Intensities.Length == 0 || Scans == null || Scans.Length == 0 || MzBins == null || MzBins.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();



            int scanArrayLength = Intensities2D.GetLength(0);

            int mzBinLength = Intensities2D.GetLength(1);

            for (int i = 0; i < mzBinLength; i++)
            {
                for (int j = 0; j < scanArrayLength; j++)
                {

                    int currentVal = Intensities2D[j, i];

                    
                    if (outputZeroValues || currentVal >0)
                    {
                        sb.Append(currentVal);
                    }
                   


                    if (j != mzBinLength - 1)    // if not the last value, add delimiter
                    {
                        sb.Append(delimiter);
                    }

                }

                sb.Append(Environment.NewLine);


            }

            return sb.ToString();


        }

        #endregion


        private int getIndexOfClosestScanValue(List<MSPeakResult> peakList, int targetScan, int leftIndex, int rightIndex, int scanTolerance)
        {
            if (leftIndex < rightIndex)
            {
                int middle = (leftIndex + rightIndex) / 2;

                if (Math.Abs(targetScan - peakList[middle].Scan_num) <= scanTolerance)
                {
                    return middle;
                }
                else if (targetScan < peakList[middle].Scan_num)
                {
                    return getIndexOfClosestScanValue(peakList, targetScan, leftIndex, middle - 1, scanTolerance);
                }
                else
                {
                    return getIndexOfClosestScanValue(peakList, targetScan, middle + 1, rightIndex, scanTolerance);
                }
            }
            else if (leftIndex == rightIndex)
            {

                {
                    return leftIndex;

                }
            }
            return leftIndex;    // if fails to find...  will return the inputted left-most scan
        }



        #region Private Methods

        #endregion

    }
}
