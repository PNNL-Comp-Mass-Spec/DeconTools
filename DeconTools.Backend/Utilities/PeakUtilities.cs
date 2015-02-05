using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;

namespace DeconTools.Backend.Utilities
{
    public class PeakUtilities
    {
#if !Disable_DeconToolsV2
        public DeconToolsV2.Peaks.clsPeak lookupPeak(double mz, DeconToolsV2.Peaks.clsPeak[] peaklist, double mzVar)
        {
            for (int i = 0; i < peaklist.Length; i++)
            {
                if (Math.Abs(mz - peaklist[i].mdbl_mz) <= mzVar)
                {
                    return peaklist[i];
                }
            }
            return null;      //couldn't find a peak
        }

        public int lookupPeakIndex(double mz, DeconToolsV2.Peaks.clsPeak[] peaklist, double mzVar)
        {
            for (int i = 0; i < peaklist.Length; i++)
            {
                if (Math.Abs(mz - peaklist[i].mdbl_mz) <= mzVar)
                {
                    return i;
                }
            }
            return -1;      //couldn't find a peak

        }
#endif

        //TODO: I'm duplicating code here to make room for List<MSPeak>;  not good... there has to be a better way... 
        public static List<MSPeak> GetMSPeaksWithinTolerance(List<MSPeak> inputList, double targetVal, double toleranceInMZ)
        {
            List<MSPeak> outputList = new List<MSPeak>();

            int targetIndex = getIndexOfClosestValue(inputList, targetVal, 0, inputList.Count - 1, toleranceInMZ);
            // look to the left for other peaks within the tolerance

            if (targetIndex == -1) return outputList;

            if (targetIndex > 0)
            {

                for (int i = (targetIndex - 1); i >= 0; i--)
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
                for (int i = (targetIndex + 1); i < inputList.Count; i++)
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
            List<Peak> mspeakList = new List<Peak>();

            for (int i = 0; i < xydata.Xvalues.Length; i++)
            {
                MSPeak peak = new MSPeak(xydata.Xvalues[i], (float)xydata.Yvalues[i], (float)peakWidth, 0);
                mspeakList.Add(peak);
            }

            return mspeakList;

        }



        public static List<Peak> GetPeaksWithinTolerance(List<Peak> inputList, double targetVal, double toleranceInMZ)
        {
            // assuming peaklist is in order 

            List<Peak> outputList = new List<Peak>();

            // find a peak within the tolerance using a binary search method
            int targetIndex = getIndexOfClosestValue(inputList, targetVal, 0, inputList.Count - 1, toleranceInMZ);
            if (targetIndex == -1) return outputList;

            // look to the left for other peaks within the tolerance
            if (targetIndex > 0)
            {
                for (int i = (targetIndex - 1); i >= 0; i--)
                {
                    Peak peak = inputList[i];

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
                for (int i = (targetIndex + 1); i < inputList.Count; i++)
                {
                    Peak peak = inputList[i];

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


            foreach (Peak peak in peakList)
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
                int middle = (leftIndex + rightIndex) / 2;

                double xValue = inputList[middle].XValue;

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
                int middle = (leftIndex + rightIndex) / 2;

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

            int indexOfMaxPeak = isotopicProfile.GetIndexOfMostIntensePeak();
            List<MSPeak> trimmedPeakList = new List<MSPeak>();


            //Trim left
            if (indexOfMaxPeak > 0)   // if max peak is not the first peak, then trim
            {
                for (int i = indexOfMaxPeak - 1; i >= 0; i--)
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
                for (int i = indexOfMaxPeak + 1; i < isotopicProfile.Peaklist.Count; i++)
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
            List<MSPeak> outputList = new List<MSPeak>();

            outputList.Add(inputList[0]);
            if (inputList.Count > 1)
            {
                for (int i = 1; i < inputList.Count; i++)
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

            for (int i = 0; i < inputPeaks.Count; i++)
            {
                sum += inputPeaks[i].Height - backgroundIntensity;
            }

            return sum;

        }


        public static XYData GetChromatogram(List<DeconTools.Backend.DTO.MSPeakResult> peakList, double targetMZ, double toleranceInMZ)
        {

            XYData xydata = new XYData();

            double lowerMZ = targetMZ - toleranceInMZ;
            double upperMZ = targetMZ + toleranceInMZ;


            List<MSPeakResult> filteredPeakList = peakList.Where(p => p.MSPeak.XValue >= lowerMZ && p.MSPeak.XValue <= upperMZ).ToList();

            if (filteredPeakList == null || filteredPeakList.Count == 0)
            {
                return null;
            }


            int minScan = filteredPeakList.First().Scan_num;
            int maxScan = filteredPeakList.Last().Scan_num;

            Dictionary<int, double> scanAndIntensityList = new Dictionary<int, double>();

            for (int i = minScan; i <= maxScan; i++)
            {
                scanAndIntensityList.Add(i, 0);

            }

            for (int i = 0; i < filteredPeakList.Count; i++)
            {
                MSPeakResult pr = filteredPeakList[i];

                double storedIntensity = scanAndIntensityList[pr.Scan_num];

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
