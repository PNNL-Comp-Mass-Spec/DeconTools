﻿using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.DTO
{
    public sealed class MSResultPeakWithLocation : Peak
    {
        private readonly ushort frameNumber;
        private readonly ushort scanNumber;
        private readonly Dictionary<ushort, List<ushort>> frameAndScansRange;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="peak"></param>
        public MSResultPeakWithLocation(MSPeakResult peak)
        {
            XValue = peak.MSPeak.XValue;
            frameNumber = (ushort)peak.FrameNum;
            scanNumber = (ushort)peak.Scan_num;
            frameAndScansRange = new Dictionary<ushort, List<ushort>>();
            var numbers = new List<ushort> {
                scanNumber
            };

            frameAndScansRange.Add(frameNumber, numbers);
        }

        /*
        public MSResultPeakWithLocation(MSPeakResult peak, int numFrames, int numScans)
        {
            this.XValue = peak.MSPeak.XValue;
            this.frameNumber = peak.Frame_num;
            this.scanNumber = peak.Scan_num;

            frameScansRange = new BitArray2D(numScans, numFrames);
        }*/

        /// <summary>
        /// Constructor
        /// Creates a peak result with a sorted list of frame numbers and scan numbers within those frame numbers
        /// </summary>
        /// <param name="peak"></param>
        /// <param name="frameAndScansRange"></param>
        /// <param name="frameNum"></param>
        /// <param name="scanNum"></param>
        /// <remarks>
        /// To improve efficiency these arrays should be passed in sorted order. We might be able to do that by
        /// using insertion sort during the creation of these arrays.
        ///
        /// An n log(n) sort here might be an overkill.
        /// </remarks>
        public MSResultPeakWithLocation(Peak peak, Dictionary<ushort, List<ushort>> frameAndScansRange, int frameNum, int scanNum)
        {
            XValue = peak.XValue;
            this.frameAndScansRange = frameAndScansRange;
            frameNumber = (ushort)frameNum;
            scanNumber = (ushort)scanNum;
        }

        /**
        public MSResultPeakWithLocation(MSPeak peak, BitArray2D framesAndScans, int frame, int scan)
        {
            this.XValue = peak.XValue;
            this.frameNumber = frame;
            this.scanNumber = scan;
            this.frameScansRange = framesAndScans;
        }*/

        //public override int CompareTo(object obj)
        //{
        //    IPeak secondPeak = obj as IPeak;
        //    if (secondPeak == null)
        //    {
        //        return -1;
        //    }
        //    else
        //    {

        //           //we need a system level global parameter that is the tolerance in PPM
        //           //TODO
        //            double toleranceInPPM = 20;
        //            double differenceInPPM = Math.Abs(1000000 * (secondPeak.XValue - this.XValue) / this.XValue);

        //            if (differenceInPPM <= toleranceInPPM)
        //            {
        //                return 0;
        //            }
        //            else
        //            {
        //                return this.XValue.CompareTo(secondPeak.XValue);
        //            }

        //    }

        //}

        /*
        public void UpdateFrameScansRange(BitArray2D frameScansFound)
        {
            this.frameScansRange.Or(frameScansFound);

        }
        */
        private List<ushort> mergeSortUnique(IReadOnlyList<ushort> list1, IReadOnlyList<ushort> list2)
        {
            var valueMap = new Dictionary<ushort, ushort>();
            var end1 = list1.Count;
            var end2 = list2.Count;

            int index1 = 0, index2 = 0;
            var mergedArray = new List<ushort>();
            while (index1 < end1 && index2 < end2)
            {
                if (list1[index1] < list2[index2])
                {
                    if (!valueMap.ContainsKey(list1[index1]))
                    {
                        valueMap.Add(list1[index1], 1);
                        mergedArray.Add(list1[index1]);
                    }
                    index1++;
                }
                else
                {
                    if (!valueMap.ContainsKey(list2[index2]))
                    {
                        valueMap.Add(list2[index2], 1);
                        mergedArray.Add(list2[index2]);
                    }
                    index2++;
                }
            }

            //find out which array is remaining to be appended and simply append that
            if (index1 == end1)
            {
                //then array 2 is remaining
                for (var i = index2; i < end2; i++)
                {
                    if (!valueMap.ContainsKey(list2[i]))
                    {
                        mergedArray.Add(list2[i]);
                    }
                }
            }
            else
            {
                for (var i = index1; i < end1; i++)
                {
                    if (!valueMap.ContainsKey(list1[i]))
                    {
                        mergedArray.Add(list1[i]);
                    }
                }
            }

            return mergedArray;
        }

        public void UpdateFrameScansRange(Dictionary<ushort, List<ushort>> newRangeValues)
        {
            foreach (var key in newRangeValues.Keys.ToList())
            {
                if (frameAndScansRange.ContainsKey(key))
                {
                    //then we need to merge the two scan lists in sorted order
                    var prevSortedList = frameAndScansRange[key];
                    var newList = mergeSortUnique(prevSortedList, newRangeValues[key]);

                    frameAndScansRange[key] = newList;
                }
                else
                {
                    //we simply add it to the dictionary
                    frameAndScansRange.Add(key, newRangeValues[key]);
                }
            }
        }

        //checks if the given mass is within a tolerance of this feature
        public bool ContainsMass(double massValue, int toleranceInPPM)
        {
            var differenceInPPM = Math.Abs(1000000 * (XValue - massValue) / XValue);

            if (differenceInPPM <= toleranceInPPM)
            {
                return true;
            }

            return false;
        }

        public int ContainsPeak(Peak peak, ushort frameNum, ushort scanNum, ushort toleranceInPPM, ushort netRange, ushort driftRange)
        {
            if (peak == null)
            {
                return -1;
            }

            //TODO:: two peaks are the same if they are within a tolerance of each other in
            //terms of mz, scan and lc frame. in this case we're only implementing mz values
            //
            var differenceInPPM = Math.Abs(1000000 * (peak.XValue - XValue) / XValue);

            if (differenceInPPM <= toleranceInPPM)
            {
                //it's within the mass tolerance of our peak
                //now check for net tolerance
                //check if the frameScansRange bit is set for the frame number and a few other scans within tolerance
                for (var i = (ushort)(frameNum - netRange); i < frameNum + netRange; i++)
                {
                    if (frameAndScansRange.ContainsKey(i))
                    {
                        //then we have to check for the scans in that list and look for our scan values in that list
                        //we have to keep that list sorted for fast searching
                        var scanList = frameAndScansRange[i];
                        for (var j = (ushort)(scanNum - driftRange); j < scanNum + driftRange; j++)
                        {
                            if (scanList.BinarySearch(j) >= 0)
                            {
                                //this means that the scan number is present in this list.
                                //we're done here
                                return 0;
                            }
                        }

                        //else we've searched through the entire scan list and didn't find
                        //the scan we're interested in.

                    }
                }

                var netDiff = frameNumber.CompareTo(frameNum);
                if (netDiff == 0)
                {
                    //now compare on scan range
                    return scanNumber.CompareTo(scanNumber);
                }

                return netDiff;
            }

            return peak.XValue.CompareTo(XValue);
        }
    }
}
