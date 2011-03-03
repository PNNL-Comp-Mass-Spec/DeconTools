using System;
using System.Collections;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using System.Collections.Generic;
using DeconTools.Backend.DTO;

namespace DeconTools.Backend.Data.Structures
{
    public class DynamicSummingPeaksTracker
    {

        private BitArray scanMapBitArray { get; set; }
        private BinaryTree<IPeak> peaksProcessed = new BinaryTree<IPeak>();


        /**
         * This constructor will be used when we're first going to create the object
         * to keep track of all the peaks
         * */
        public DynamicSummingPeaksTracker(int numScans, List<int> scanList, List<IPeak> peakList)
        {
            scanMapBitArray = new BitArray(numScans);

            for (int i = 0; i < scanList.Count; i++)
            {
                int index = scanList[i];
                scanMapBitArray[index] = true;
            }

            foreach (IPeak item in peakList)
            {
                peaksProcessed.Add(item);
            }
        }


        public bool AddPeaksToProcessedTree(List<IPeak> peakList)
        {
            foreach (IPeak item in peakList)
            {
                peaksProcessed.Add(item);
            }

            return true;
        }



        public bool peakPresent(MSPeakResult peak ){

            bool present = false;
            peak.SortOnKey = IPeak.SortKey.MZ;


            //first check if that scan was already processed 

            //we'll have to make sure that we're not throwing an array out of bounds exception here
            if (peak.Scan_num <= 598 && peak.Scan_num >= 2)
            {

                //this is done so that we're assuming that a peak present in the same frame and +- 1 scan in IMS apart doesn't
                //need to be processed again. Based on the real tolerance we could increase this value. For now lets be conservative
                if (scanMapBitArray[peak.Scan_num] || scanMapBitArray[peak.Scan_num - 1] || scanMapBitArray[peak.Scan_num + 1])
                {
                    //then check if the peak is present in the tree based on mass tolerance
                    if (peaksProcessed.Find(peak) != null)
                    {
                        present = true;
                    }
                    else
                    {
                        peaksProcessed.Add(peak);
                    }
                }
                else
                {

                    //set the scan to be processed as true
                    scanMapBitArray[peak.Scan_num] = true;

                    //add the peak to the list of processed peaks, so that we can search for another one that's similar
                    peaksProcessed.Add(peak);
                }
            }
            else
            {
                //we should ignore it as it's too close to the junk part of the run
                //TODO change these values to constants based on the UIMF file
                if (peak.Frame_num <= 1 || peak.Frame_num >= 2399)
                {
                    present = true;
                }
            }

            return present;
        }


    }
}
