using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using DeconTools.Backend.DTO;

namespace DeconTools.Backend.ProcessingTasks
{
    public class PeakChromatogramGenerator : Task
    {
        double ppmTol;
        int maxZerosToAdd = 2;
        List<int> msScanList = new List<int>();


        #region Constructors
        public PeakChromatogramGenerator()
            : this(20)
        {

        }

        public PeakChromatogramGenerator(double ppmTol)
        {
            this.ppmTol = ppmTol;

        }
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
        public override void Execute(ResultCollection resultColl)
        {
            Check.Require(resultColl.MSPeakResultList != null, "PeakChromatogramGenerator failed. No peaks.");
            Check.Require(resultColl.Run.CurrentMassTag != null, "PeakChromatogramGenerator failed. This requires a MassTag to be specified.");
            Check.Require(resultColl.Run.CurrentMassTag.MZ != 0, "PeakChromatorgramGenerator failed. MassTag's MZ hasn't been specified.");

            Check.Require(resultColl.Run.MaxScan > 0, "PeakChromatogramGenerator failed.  Problem with 'MaxScan'");

            double mz = resultColl.Run.CurrentMassTag.MZ;



            double minNetVal = resultColl.Run.CurrentMassTag.NETVal - resultColl.Run.CurrentMassTag.NETVal * 0.1;  // set lower bound (10% lower than net val)
            double maxNetVal = resultColl.Run.CurrentMassTag.NETVal + resultColl.Run.CurrentMassTag.NETVal * 0.1;  // set upper bound (10% higher than net val)

            if (minNetVal < 0) minNetVal = 0;
            if (maxNetVal > 1) maxNetVal = 1;

            int lowerScan = resultColl.Run.GetNearestScanValueForNET(minNetVal);
            int upperScan = resultColl.Run.GetNearestScanValueForNET(maxNetVal);




            double lowerMZ = -1 * (ppmTol * mz / 1e6 - mz);
            double upperMZ = ppmTol * mz / 1e6 + mz;

            int scanTolerance = 5;

            //binary search for scan value...
            int indexOfLowerScan = getIndexOfClosestScanValue(resultColl.MSPeakResultList, lowerScan, 0, resultColl.MSPeakResultList.Count, scanTolerance);
            int indexOfUpperScan = getIndexOfClosestScanValue(resultColl.MSPeakResultList, upperScan, 0, resultColl.MSPeakResultList.Count, scanTolerance);

            int currentIndex = indexOfLowerScan;

            List<MSPeakResult> filteredPeakList = new List<MSPeakResult>();

            while (currentIndex<= indexOfUpperScan)
            {
                filteredPeakList.Add(resultColl.MSPeakResultList[currentIndex]);
                currentIndex++;
            }

            filteredPeakList = filteredPeakList.Where(p => p.MSPeak.XValue >= lowerMZ && p.MSPeak.XValue <= upperMZ).ToList();

            //List<MSPeakResult> filteredPeakList = resultColl.MSPeakResultList.Where(p => p.Scan_num >= lowerScan && p.Scan_num <= upperScan)
            //    .Where(p => p.MSPeak.XValue >= lowerMZ && p.MSPeak.XValue <= upperMZ).ToList();
                
            
            //List<MSPeakResult> filteredPeakList = new List<MSPeakResult>();

            ////have to search linearly - the m/z values are not in order
            //for (int i = 0; i < resultColl.MSPeakResultList.Count; i++)
            //{
            //    if (resultColl.MSPeakResultList[i].MSPeak.XValue >= lowerMZ && resultColl.MSPeakResultList[i].MSPeak.XValue <= upperMZ)
            //    {
            //        filteredPeakList.Add(resultColl.MSPeakResultList[i]);
            //    }

            //}



            //int indexOfLowerMZValue = getIndexOfClosestMZValue(resultColl.MSPeakResultList, lowerMZ,0,resultColl.MSPeakResultList.Count,1.0d);


            bool containsPeaks = (filteredPeakList != null && filteredPeakList.Count() != 0);
            Check.Ensure(containsPeaks, "No chromatographic peaks were found using the provided m/z range.");



            this.msScanList = resultColl.Run.GetMSLevelScanValues();

            Check.Require(this.msScanList != null && this.msScanList.Count > 0, "PeakChromatogramGenerator failed. Had problems defining the Scan array");


            MassTagResultBase result = resultColl.GetMassTagResult(resultColl.Run.CurrentMassTag);


            //store XYData in the Run's data to be used by other tasks...
            resultColl.Run.XYData = getChromValues2(filteredPeakList, resultColl.Run);

            //store XYData in the MassTag result object
            //result.ChromValues = resultColl.Run.XYData;

        }

        private int getIndexOfClosestScanValue(List<MSPeakResult> peakList, int targetScan, int leftIndex, int rightIndex, int scanTolerance)
        {
            if (leftIndex <= rightIndex)
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
            return -1;
        }

        /// <summary>
        /// Recursive binary search algorithm for searching through MSPeakResults
        /// </summary>
        private int getIndexOfClosestMZValue(List<MSPeakResult> peakList, double targetMZ, int leftIndex, int rightIndex, double toleranceInDaltons)
        {
            if (leftIndex <= rightIndex)
            {
                int middle = (leftIndex + rightIndex) / 2;
                if (Math.Abs(targetMZ - peakList[middle].MSPeak.XValue) <= toleranceInDaltons)
                {
                    return middle;
                }
                else if (targetMZ<peakList[middle].MSPeak.XValue)
                {
                    return getIndexOfClosestMZValue(peakList, targetMZ, leftIndex, middle - 1, toleranceInDaltons);
                }
                else
                {
                    return getIndexOfClosestMZValue(peakList, targetMZ, middle + 1, rightIndex, toleranceInDaltons);
                }
            }
            return -1;


        }


        private XYData getChromValues(List<MSPeakResult> filteredPeakList, Run run)
        {
            XYData data = new XYData();

            SortedDictionary<int, double> xyValues = new SortedDictionary<int, double>();
            foreach (MSPeakResult peak in filteredPeakList)
            {
                //find reference points within the MS-Level scan list.  These are the scans for which we will make sure there is some value declared
                int centerScanPtr = msScanList.IndexOf(peak.Scan_num);

                int leftScanPtr = centerScanPtr - maxZerosToAdd;
                int rightScanptr = centerScanPtr + maxZerosToAdd;

                //handle the rare case of having a point at the end of the scan list
                if (rightScanptr >= msScanList.Count)
                {
                    rightScanptr = msScanList.Count - 1;
                }

                //case of being at the beginning of the scan list
                if (leftScanPtr < 0) leftScanPtr = 0;


                //this loop adds zeros to the left of a chrom value
                for (int i = leftScanPtr; i < centerScanPtr; i++)
                {
                    int targetScan = msScanList[i];
                    if (filteredPeakList.Exists(p => p.Scan_num == targetScan))
                    {
                        // do nothing
                    }
                    else
                    {
                        //add 0 points, if they haven't been added already  (which can occur in the forward direction)

                        if (!xyValues.ContainsKey(targetScan))
                        {
                            xyValues.Add(targetScan, 0);
                        }

                    }


                }

                //here, the primary MS Peak's data is used to establish a chromatogram point
                if (filteredPeakList.Exists(p => p.Scan_num == msScanList[centerScanPtr]))
                {
                    //a given scan can have more than one peak that falls within the m/z tolerance
                    if (xyValues.ContainsKey(peak.Scan_num))
                    {
                        //for now, will just take the peak that is the most intense.
                        if (peak.MSPeak.Height > xyValues[peak.Scan_num])
                        {
                            xyValues[peak.Scan_num] = peak.MSPeak.Height;
                        }
                    }
                    else
                    {
                        xyValues.Add(peak.Scan_num, peak.MSPeak.Height);
                    }
                }



                //add zeros to the right of a peak
                for (int i = centerScanPtr; i <= rightScanptr; i++)
                {
                    if (i >= msScanList.Count)
                    {
                        Console.WriteLine("something wrong here");
                    }


                    int targetScan = msScanList[i];
                    if (filteredPeakList.Exists(p => p.Scan_num == targetScan))
                    {
                        // do nothing
                    }
                    else
                    {
                        if (!xyValues.ContainsKey(targetScan))
                        {
                            xyValues.Add(targetScan, 0);
                        }

                    }

                }


            }




            data.Xvalues = XYData.ConvertIntsToDouble(xyValues.Keys.ToArray());
            data.Yvalues = xyValues.Values.ToArray();

            return data;




        }


        private XYData getChromValues2(List<MSPeakResult> filteredPeakList, Run run)
        {
            XYData xydata = new XYData();

            int leftZeroPadding = 200;   //number of scans to the left of the minscan for which zeros will be added
            int rightZeroPadding = 200;   //number of scans to the left of the minscan for which zeros will be added

            int peakListMinScan = filteredPeakList[0].Scan_num;
            int peakListMaxScan = filteredPeakList[filteredPeakList.Count - 1].Scan_num;

            //will pad min and max scans with zeros, and add zeros in between. This allows smoothing to execute properly

            peakListMinScan = peakListMinScan - leftZeroPadding;
            peakListMaxScan = peakListMaxScan + rightZeroPadding;

            if (peakListMinScan < run.MinScan) peakListMinScan = run.MinScan;
            if (peakListMaxScan > run.MaxScan) peakListMaxScan = run.MaxScan;

            //populate array with zero intensities.
            SortedDictionary<int, double> xyValues = new SortedDictionary<int, double>();
            for (int i = peakListMinScan; i <= peakListMaxScan; i++)
            {
                //add only MS1 level scans
                if (msScanList.Contains(i))
                {
                    xyValues.Add(i, 0);
                }
            }

            //now iterate over peakList and add data to output array


            //foreach (var item in filteredPeakList)
            //{
            //    double intensity = item.MSPeak.Height;
            //    if (intensity > xyValues[item.Scan_num])
            //    {
            //        xyValues[item.Scan_num] = intensity;
            //    }
            //}


            for (int i = 0; i < filteredPeakList.Count; i++)
            {
                double intensity = filteredPeakList[i].MSPeak.Height;

                //because we have tolerances to filter the peaks, more than one m/z peak may occur for a given scan. So will take the most abundant...
                if (intensity > xyValues[filteredPeakList[i].Scan_num])
                {
                    xyValues[filteredPeakList[i].Scan_num] = intensity;
                }

            }


            xydata.Xvalues = XYData.ConvertIntsToDouble(xyValues.Keys.ToArray());
            xydata.Yvalues = xyValues.Values.ToArray();

            return xydata;
        }




        //private XYData getPeakChromValues(List<MSPeakResult> filteredPeakList, Run run)
        //{
        //    XYData data = new XYData();

        //    List<double> tempXVals = new List<double>();
        //    List<double> tempYVals = new List<double>();

        //    HashSet<int> test = new HashSet<int>();


        //    List<int> scansAnalyzed = new List<int>();


        //    foreach (MSPeakResult peak in filteredPeakList)
        //    {








        //        //see if peak is adjacent to the last one analyzed...
        //        int currentScan = peak.Scan_num;
        //        int nextScan = run.GetClosestMSScan(currentScan + 1, Globals.ScanSelectionMode.ASCENDING);


        //        if (test.Contains(peak.Scan_num))
        //        {

        //        }

        //        //if not then need to add some zeros to the left


        //        //add peak's info
        //        tempXVals.Add(peak.Scan_num);
        //        tempYVals.Add(peak.MSPeak.Height);

        //        //check if there is a MS-level data point to the right.  If not need to create one or more.

        //        int currentScan = peak.Scan_num;
        //        for (int i = 1; i <= maxZerosToAdd; i++)
        //        {
        //            int nextScan = run.GetClosestMSScan(currentScan + 1, Globals.ScanSelectionMode.ASCENDING);
        //            if (filteredPeakList.Exists(p => p.Scan_num == nextScan))
        //            {
        //                //found a peak so stop adding zeros
        //                break;
        //            }
        //            else
        //            {
        //                //no peak found.  Add a zero point
        //                tempXVals.Add(nextScan);
        //                tempYVals.Add(0);

        //            }
        //            currentScan = nextScan;


        //        }



        //    }

        //    data.Xvalues = tempXVals.ToArray();
        //    data.Yvalues = tempYVals.ToArray();
        //    return data;

        //}

        private void addZeroIntensityValues(int p)
        {
            throw new NotImplementedException();
        }
    }
}
