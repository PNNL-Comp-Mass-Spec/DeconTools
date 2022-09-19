using System;
using System.Collections.Generic;
using System.IO;
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
            get => _intensities2D ?? (_intensities2D = GetIntensitiesAs2DArray());
            set => _intensities2D = value;
        }

        #endregion

        #region Public Methods

        public void Get3DElutionProfileFromPeakLevelData(Run run, int minScan, int maxScan, double minMZ, double maxMZ, out int[] scans, out double[] mzBins, out float[] intensities, double binWidth = 0.01, bool applyLogTransform = false)
        {
            var msPeakList = run.ResultCollection.MSPeakResultList;
            Check.Require(msPeakList?.Count > 0, "Run is missing peaks. Elution profile is based on peak-level info");

            Check.Require(binWidth > 0, "BinWidth must be greater than 0");

            var scanSetCollection = new ScanSetCollection();
            scanSetCollection.Create(run, minScan, maxScan, 1, 1, false);

            //create bins
            var numBins = (int)Math.Round((maxMZ - minMZ) / binWidth);
            mzBins = new double[numBins];
            var firstBinMZ = minMZ;

            for (var i = 0; i < numBins; i++)
            {
                mzBins[i] = firstBinMZ + i * binWidth;
            }

            //iterate over scans and collected MzIntensityArray for each scan

            var intensityList = new List<float>();

            scans = scanSetCollection.ScanSetList.Select(p => p.PrimaryScanNumber).ToArray();

            //gather relevant peaks.
            var scanTolerance = 5;     // TODO:   keep an eye on this

            minScan = scans[0];
            maxScan = scans.Last();

            var indexOfLowerScan = getIndexOfClosestScanValue(msPeakList, minScan, 0, msPeakList.Count - 1, scanTolerance);
            var indexOfUpperScan = getIndexOfClosestScanValue(msPeakList, maxScan, 0, msPeakList.Count - 1, scanTolerance);

            var currentIndex = indexOfLowerScan;

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

                var intensitiesForScan = new float[numBins];

                foreach (var msPeakResult in peaksForScan)
                {
                    var targetBin = (int)Math.Round((msPeakResult.XValue - firstBinMZ) / binWidth);

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

            var twoDimensionalIntensityArray = new float[Scans.Length, MzBins.Length];

            var counter = 0;
            for (var scan = 0; scan < Scans.Length; scan++)
            {
                for (var mzBin = 0; mzBin < MzBins.Length; mzBin++)
                {
                    twoDimensionalIntensityArray[scan, mzBin] = Intensities[counter];
                    counter++;
                }
            }

            return twoDimensionalIntensityArray;
        }

        public void OutputElutionProfileToFile(string outputFilename, char delimiter = '\t', bool outputZeroValues = true, int numDecimals = 0)
        {
            if (Intensities == null || Intensities.Length == 0 || Scans == null || Scans.Length == 0 || MzBins == null || MzBins.Length == 0)
            {
                return;
            }

            var scanArrayLength = Intensities2D.GetLength(0);
            var mzBinLength = Intensities2D.GetLength(1);

            using (var writer = new StreamWriter(outputFilename))
            {
                for (var i = 0; i < mzBinLength; i++)
                {
                    var sb = new StringBuilder();
                    for (var j = 0; j < scanArrayLength; j++)
                    {
                        var currentVal = Intensities2D[j, i];

                        if (outputZeroValues || currentVal > 0)
                        {
                            if (!float.IsInfinity(currentVal))
                            {
                                var formatString = "0";
                                if (numDecimals > 0)
                                {
                                    formatString = "0.".PadRight(numDecimals + 3, '0');
                                    // making a format string based on number of decimals wanted. Clunky but works!
                                }

                                sb.Append(currentVal.ToString(formatString));
                            }
                            else
                            {
                                sb.Append(0); //value is infinity. This happens with a log of '0'.  So will output 0, the lowest log value.
                            }
                        }

                        if (j != mzBinLength - 1) // if not the last value, add delimiter
                        {
                        }
                    }

                    writer.WriteLine(sb.ToString());
                }
            }
        }

        public string OutputElutionProfileAsString(char delimiter = '\t', bool outputZeroValues = true, int numDecimals = 0)
        {
            if (Intensities == null || Intensities.Length == 0 || Scans == null || Scans.Length == 0 || MzBins == null || MzBins.Length == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            var scanArrayLength = Intensities2D.GetLength(0);

            var mzBinLength = Intensities2D.GetLength(1);

            for (var i = 0; i < mzBinLength; i++)
            {
                for (var j = 0; j < scanArrayLength; j++)
                {
                    var currentVal = Intensities2D[j, i];

                    if (outputZeroValues || currentVal > 0)
                    {
                        if (!float.IsInfinity(currentVal))
                        {
                            var formatString = "0";
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
                    }
                }

                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        #endregion

        private int getIndexOfClosestScanValue(IReadOnlyList<MSPeakResult> peakList, int targetScan, int leftIndex, int rightIndex, int scanTolerance)
        {
            if (leftIndex < rightIndex)
            {
                var middle = (leftIndex + rightIndex) / 2;

                if (Math.Abs(targetScan - peakList[middle].Scan_num) <= scanTolerance)
                {
                    return middle;
                }
                if (targetScan < peakList[middle].Scan_num)
                {
                    return getIndexOfClosestScanValue(peakList, targetScan, leftIndex, middle - 1, scanTolerance);
                }

                return getIndexOfClosestScanValue(peakList, targetScan, middle + 1, rightIndex, scanTolerance);
            }

            if (leftIndex == rightIndex)
            {
                return leftIndex;
            }

            return leftIndex;    // if fails to find...  will return the inputted left-most scan
        }

        #region Private Methods

        #endregion

    }
}
