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

        public float[] Intensities { get; set; }


        private float[,] _intensities2D;
        public float[,] Intensities2D
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


       


        public void Get3DElutionProfileFromPeakLevelData(Run run, int minScan, int maxScan, double minMZ, double maxMZ, out int[] scans, out double[] mzBins, out float[] intensities, double binWidth = 0.01, bool applyLogTransform = false)
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

            List<float> intensityList = new List<float>();



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

                float[] intensitiesForScan = new float[numBins];

                foreach (var msPeakResult in peaksForScan)
                {
                    int targetBin = (int)Math.Round((msPeakResult.XValue - firstBinMZ) / binWidth);
                    
                    if (targetBin < intensitiesForScan.Length)
                    {
                        intensitiesForScan[targetBin] += (float)Math.Round(msPeakResult.Height); 
                    }
                    


                }

                intensityList.AddRange(intensitiesForScan);
            }

            if (applyLogTransform)
            {
                intensities = intensityList.Select(p => (float)Math.Log(p)).ToArray();  
            }
            else
            {
                intensities = intensityList.ToArray();
            }
            
            Scans = scans;
            Intensities = intensities;
            MzBins = mzBins;
        }

        public float[,] GetIntensitiesAs2DArray()
        {
            if (Intensities == null || Intensities.Length == 0 || Scans == null || Scans.Length == 0 || MzBins == null || MzBins.Length == 0)
            {
                return new float[0, 0];
            }
            
            float[,] twoDimensionalIntensityArray = new float[Scans.Length, MzBins.Length];

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


        public string OutputElutionProfileAsString(char delimiter = '\t', bool outputZeroValues=true, int numDecimals=0)
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

                    float currentVal = Intensities2D[j, i];

                    
                    if (outputZeroValues || currentVal >0 )
                    {
                        if (!float.IsInfinity(currentVal))
                        {
                            string formatString = "0";
                            if (numDecimals > 0)
                            {
                                formatString = "0.".PadRight(numDecimals + 3, '0');     // making a format string based on number of decimals wanted. Clunky but works!
                            }

                            sb.Append(currentVal.ToString(formatString));
                            
                        }
                        else
                        {
                            sb.Append(0);    //value is infinity. This happens with a log of '0'.  So will output 0, the lowest log value. 
                        }

                       
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
