using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.Utilities
{
    public class PeakUtilities
    {


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


        public static List<IPeak> GetPeaksWithinTolerance(List<IPeak> inputList, double targetVal, double tolerance)
        {
            // assuming peaklist is in order 

            // find a peak within the tolerance using a binary search method

            List<IPeak> outputList = new List<IPeak>();


            int targetIndex = getIndexOfClosestValue(inputList, targetVal, 0, inputList.Count - 1, tolerance);
            if (targetIndex == -1) return outputList;

            // look to the left for other peaks within the tolerance
            if (targetIndex > 0)
            {

                for (int i = (targetIndex - 1); i >= 0; i--)
                {
                    if (Math.Abs(targetVal - inputList[i].XValue) <= tolerance)
                    {
                        outputList.Add(inputList[i]);
                    }
                }
            }
            else
            {

            }

            // if more than one 'peaks-to-the-left' were added, reverse the order so that in the end it is ordered properly
            if (outputList.Count > 1) outputList.Reverse();

            // add the center peak
            outputList.Add(inputList[targetIndex]);

            // look to the right for other peaks within the tolerance. 
            if (targetIndex < inputList.Count - 1)   // ensure we aren't at the end of the peak list
            {
                for (int i = (targetIndex + 1); i < inputList.Count; i++)
                {
                    if (Math.Abs(targetVal - inputList[i].XValue) <= tolerance)
                    {
                        outputList.Add(inputList[i]);
                    }

                }
            }

            return outputList;
        }

        public static int getIndexOfClosestValue(List<IPeak> inputList, double targetVal, int leftIndex, int rightIndex, double tolerance)
        {
            if (leftIndex <= rightIndex)
            {
                int middle = (leftIndex + rightIndex) / 2;

                if (Math.Abs(targetVal - inputList[middle].XValue) <= tolerance)
                {
                    return middle;
                }
                else if (targetVal < inputList[middle].XValue)
                {
                    return getIndexOfClosestValue(inputList, targetVal, leftIndex, middle - 1, tolerance);
                }
                else
                {
                    return getIndexOfClosestValue(inputList, targetVal, middle + 1, rightIndex, tolerance);
                }
            }
            return -1;





        }



        public static void TrimIsotopicProfile(IsotopicProfile isotopicProfile, double cutOff)
        {
            if (isotopicProfile == null || isotopicProfile.Peaklist == null || isotopicProfile.Peaklist.Count == 0) return;

            int indexOfMaxPeak = isotopicProfile.getIndexOfMostIntensePeak();
            List<MSPeak> trimmedPeakList = new List<MSPeak>();


            //Trim left
            if (indexOfMaxPeak > 0)   // if max peak is not the first peak, then trim
            {
                for (int i = indexOfMaxPeak - 1; i >= 0; i--)
                {
                    if (isotopicProfile.Peaklist[i].Height >= cutOff)
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
                    if (isotopicProfile.Peaklist[i].Height >= cutOff)
                    {
                        trimmedPeakList.Add(isotopicProfile.Peaklist[i]);
                    }

                }
            }

            isotopicProfile.Peaklist = trimmedPeakList;

        }









    }
}
