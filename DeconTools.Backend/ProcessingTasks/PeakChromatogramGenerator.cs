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

            double lowerMZ = -1 * (ppmTol * mz / 1e6 - mz);
            double upperMZ = ppmTol * mz / 1e6 + mz;


            //double mzLowerLimit = (mz-ppmTol*

            List<MSPeakResult> filteredPeakList = resultColl.MSPeakResultList.Where(p => p.MSPeak.XValue >= lowerMZ && p.MSPeak.XValue <= upperMZ).ToList();


            this.msScanList = resultColl.Run.GetMSLevelScanValues();

            Check.Require(this.msScanList != null && this.msScanList.Count > 0, "PeakChromatogramGenerator failed. Had problems defining the Scan array");


            IMassTagResult result = resultColl.GetMassTagResult(resultColl.Run.CurrentMassTag);
       

            //store XYData in the Run's data to be used by other tasks...
            resultColl.Run.XYData = getChromValues(filteredPeakList, resultColl.Run);

            //store XYData in the MassTag result object
            //result.ChromValues = resultColl.Run.XYData;

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
