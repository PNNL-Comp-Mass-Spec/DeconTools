using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;

namespace DeconTools.Backend.Utilities
{
    public class PeakUtilities
    {
        //TODO: I'm duplicating code here to make room for List<MSPeak>;  not good... there has to be a better way... 
        public static List<MSPeak> GetMSPeaksWithinTolerance(List<MSPeak> inputList, double targetVal, double toleranceInMZ)
        {
            var outputList = new List<MSPeak>();

            var targetIndex = getIndexOfClosestValue(inputList, targetVal, 0, inputList.Count - 1, toleranceInMZ);
            // look to the left for other peaks within the tolerance

            if (targetIndex == -1) return outputList;

            if (targetIndex > 0)
            {

                for (var i = (targetIndex - 1); i >= 0; i--)
                {
                    if (Math.Abs(targetVal - inputList[i].XValue) <= toleranceInMZ)
                    {
                        outputList.Insert(0, inputList[i]);
                    }
                }
            }
            else
            {

            }

            // add the center peak
            outputList.Add(inputList[targetIndex]);

            // look to the right for other peaks within the tolerance. 
            if (targetIndex < inputList.Count - 1)   // ensure we aren't at the end of the peak list
            {
                for (var i = (targetIndex + 1); i < inputList.Count; i++)
                {
                    if (Math.Abs(targetVal - inputList[i].XValue) <= toleranceInMZ)
                    {
                        outputList.Add(inputList[i]);
                    }

                }
            }

            return outputList;
        }

        public static List<Peak> CreatePeakDataFromXYData(XYData xydata, double peakWidth)
        {
            var mspeakList = new List<Peak>();

            for (var i = 0; i < xydata.Xvalues.Length; i++)
            {
                var peak = new MSPeak(xydata.Xvalues[i], (float)xydata.Yvalues[i], (float)peakWidth, 0);
                mspeakList.Add(peak);
            }

            return mspeakList;

        }



        public static List<Peak> GetPeaksWithinTolerance(List<Peak> inputList, double targetVal, double toleranceInMZ)
        {
            // assuming peaklist is in order 

            var outputList = new List<Peak>();

            // find a peak within the tolerance using a binary search method
            var targetIndex = getIndexOfClosestValue(inputList, targetVal, 0, inputList.Count - 1, toleranceInMZ);
            if (targetIndex == -1) return outputList;

            // look to the left for other peaks within the tolerance
            if (targetIndex > 0)
            {
                for (var i = (targetIndex - 1); i >= 0; i--)
                {
                    var peak = inputList[i];

                    // Once we we reach a certain m/z, we can stop looking
                    if (Math.Abs(targetVal - peak.XValue) > toleranceInMZ) break;

                    outputList.Insert(0, peak);
                }
            }

            // add the center peak
            outputList.Add(inputList[targetIndex]);

            // look to the right for other peaks within the tolerance. 
            if (targetIndex < inputList.Count - 1)   // ensure we aren't at the end of the peak list
            {
                for (var i = (targetIndex + 1); i < inputList.Count; i++)
                {
                    var peak = inputList[i];

                    // Once we we reach a certain m/z, we can stop looking
                    if (Math.Abs(targetVal - peak.XValue) > toleranceInMZ) break;

                    outputList.Add(peak);
                }
            }

            return outputList;
        }


        public Peak GetBasePeak(List<Peak> peakList)
        {
            if (peakList == null || peakList.Count == 0) return new Peak();

            Peak maxPeak;
            if (!(peakList[0] is MSPeak)) return null;
            maxPeak = peakList[0];


            foreach (var peak in peakList)
            {
                if (peak.Height >= maxPeak.Height)
                {
                    maxPeak = peak;
                }

            }
            return maxPeak;

        }

        public static int getIndexOfClosestValue(List<Peak> inputList, double targetVal, int leftIndex, int rightIndex, double toleranceInMZ)
        {
            if (leftIndex <= rightIndex)
            {
                var middle = (leftIndex + rightIndex) / 2;

                var xValue = inputList[middle].XValue;

                if (Math.Abs(targetVal - xValue) <= toleranceInMZ)
                {
                    return middle;
                }
                else if (targetVal < xValue)
                {
                    return getIndexOfClosestValue(inputList, targetVal, leftIndex, middle - 1, toleranceInMZ);
                }
                else
                {
                    return getIndexOfClosestValue(inputList, targetVal, middle + 1, rightIndex, toleranceInMZ);
                }
            }
            return -1;
        }

        //TODO:  fix this.  I'm duplicating above code.  There should be a better way.... 
        //TODO: there is a bug here; need to unit test this and fix it.
        public static int getIndexOfClosestValue(List<MSPeak> inputList, double targetVal, int leftIndex, int rightIndex, double toleranceInMZ)
        {
            if (leftIndex <= rightIndex)
            {
                var middle = (leftIndex + rightIndex) / 2;

                if (Math.Abs(targetVal - inputList[middle].XValue) <= toleranceInMZ)
                {
                    return middle;
                }
                else if (targetVal < inputList[middle].XValue)
                {
                    return getIndexOfClosestValue(inputList, targetVal, leftIndex, middle - 1, toleranceInMZ);
                }
                else
                {
                    return getIndexOfClosestValue(inputList, targetVal, middle + 1, rightIndex, toleranceInMZ);
                }
            }
            return -1;
        }


        public static void TrimIsotopicProfile(IsotopicProfile isotopicProfile, double cutOff, bool neverTrimLeft = false, bool neverTrimRight = false)
        {
            if (isotopicProfile == null || isotopicProfile.Peaklist == null || isotopicProfile.Peaklist.Count == 0) return;

            var indexOfMaxPeak = isotopicProfile.GetIndexOfMostIntensePeak();
            var trimmedPeakList = new List<MSPeak>();


            //Trim left
            if (indexOfMaxPeak > 0)   // if max peak is not the first peak, then trim
            {
                for (var i = indexOfMaxPeak - 1; i >= 0; i--)
                {
                    if (isotopicProfile.Peaklist[i].Height >= cutOff || neverTrimLeft)
                    {
                        trimmedPeakList.Insert(0, isotopicProfile.Peaklist[i]);
                    }
                }
            }

            //Add max peak
            trimmedPeakList.Add(isotopicProfile.Peaklist[indexOfMaxPeak]);

            //Trim right
            if (indexOfMaxPeak < isotopicProfile.Peaklist.Count - 1)  // if max peak is not the last peak (rare condition)
            {
                for (var i = indexOfMaxPeak + 1; i < isotopicProfile.Peaklist.Count; i++)
                {
                    if (isotopicProfile.Peaklist[i].Height >= cutOff || neverTrimRight)
                    {
                        trimmedPeakList.Add(isotopicProfile.Peaklist[i]);
                    }

                }
            }

            isotopicProfile.Peaklist = trimmedPeakList;

        }

        public static List<MSPeak> OrderPeaksByIntensityDesc(List<MSPeak> inputList)
        {
            if (inputList == null || inputList.Count == 0) return null;
            var outputList = new List<MSPeak>();

            outputList.Add(inputList[0]);
            if (inputList.Count > 1)
            {
                for (var i = 1; i < inputList.Count; i++)
                {
                    if (inputList[i].Height > outputList[0].Height)
                    {
                        outputList.Insert(0, inputList[i]);
                    }
                    else
                    {
                        outputList.Add(inputList[i]);
                    }
                }
            }

            return outputList;




        }



        public static double GetSumOfIntensities(IList<Peak> inputPeaks, double backgroundIntensity)
        {
            double sum = 0;

            for (var i = 0; i < inputPeaks.Count; i++)
            {
                sum += inputPeaks[i].Height - backgroundIntensity;
            }

            return sum;

        }


        public static XYData GetChromatogram(List<DeconTools.Backend.DTO.MSPeakResult> peakList, double targetMZ, double toleranceInMZ)
        {

            var xydata = new XYData();

            var lowerMZ = targetMZ - toleranceInMZ;
            var upperMZ = targetMZ + toleranceInMZ;


            var filteredPeakList = peakList.Where(p => p.MSPeak.XValue >= lowerMZ && p.MSPeak.XValue <= upperMZ).ToList();

            if (filteredPeakList == null || filteredPeakList.Count == 0)
            {
                return null;
            }


            var minScan = filteredPeakList.First().Scan_num;
            var maxScan = filteredPeakList.Last().Scan_num;

            var scanAndIntensityList = new Dictionary<int, double>();

            for (var i = minScan; i <= maxScan; i++)
            {
                scanAndIntensityList.Add(i, 0);

            }

            for (var i = 0; i < filteredPeakList.Count; i++)
            {
                var pr = filteredPeakList[i];

                var storedIntensity = scanAndIntensityList[pr.Scan_num];

                if (pr.MSPeak.Height > storedIntensity)
                {
                    scanAndIntensityList[pr.Scan_num] = pr.MSPeak.Height;
                }
            }

            xydata.Xvalues = XYData.ConvertIntsToDouble(scanAndIntensityList.Keys.ToArray());
            xydata.Yvalues = scanAndIntensityList.Values.ToArray();

            return xydata;
        }
    }
}
