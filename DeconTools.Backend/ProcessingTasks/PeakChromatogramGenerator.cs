using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{


    public enum ChromatogramGeneratorMode
    {
        MONOISOTOPIC_PEAK,
        MOST_ABUNDANT_PEAK,
        TOP_N_PEAKS
    }

    public enum IsotopicProfileType
    {
        UNLABELLED,
        LABELLED
    }



    public class PeakChromatogramGenerator : Task
    {
        int maxZerosToAdd = 2;
        List<int> msScanList = new List<int>();

        #region Constructors
        public PeakChromatogramGenerator()
            : this(20)
        {

        }

        public PeakChromatogramGenerator(double ppmTol)
            : this(ppmTol, ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK)
        {

        }

        public PeakChromatogramGenerator(double ppmTolerance, ChromatogramGeneratorMode chromMode)
            : this(ppmTolerance, chromMode, IsotopicProfileType.UNLABELLED)
        {

        }

        public PeakChromatogramGenerator(double ppmTolerance, ChromatogramGeneratorMode chromMode, IsotopicProfileType isotopicProfileTarget)
        {
            this.PPMTolerance = ppmTolerance;
            this.ChromatogramGeneratorMode = chromMode;
            this.IsotopicProfileTarget = isotopicProfileTarget;

            this.TopNPeaksLowerCutOff = 0.3;
            this.NETWindowWidth = 0.3f;

        }

        #endregion

        #region Properties
        public IsotopicProfileType IsotopicProfileTarget { get; set; }

        public ChromatogramGeneratorMode ChromatogramGeneratorMode { get; set; }

        public double PPMTolerance { get; set; }

        /// <summary>
        /// The width or range of the NET / scan window. A larger value will result in a chromatogram covering more of the dataset scan range. 
        /// </summary>
        public float NETWindowWidth { get; set; }


        /// <summary>
        /// Peaks of the theoretical isotopic profile that fall below this cutoff will not be used in generating the chromatogram. 
        /// </summary>
        public double TopNPeaksLowerCutOff { get; set; }

        #endregion

        #region Public Methods
        

        
        
        public override void Execute(ResultCollection resultColl)
        {
            Check.Require(resultColl.MSPeakResultList != null && resultColl.MSPeakResultList.Count>0, "PeakChromatogramGenerator failed. No peaks.");
            Check.Require(resultColl.Run.CurrentMassTag != null, "PeakChromatogramGenerator failed. This requires a MassTag to be specified.");
            Check.Require(resultColl.Run.CurrentMassTag.MZ != 0, "PeakChromatorgramGenerator failed. MassTag's MZ hasn't been specified.");

            Check.Require(resultColl.Run.MaxScan > 0, "PeakChromatogramGenerator failed.  Problem with 'MaxScan'");

            float minNetVal = resultColl.Run.CurrentMassTag.NETVal - resultColl.Run.CurrentMassTag.NETVal * NETWindowWidth;
            float maxNetVal = resultColl.Run.CurrentMassTag.NETVal + resultColl.Run.CurrentMassTag.NETVal * NETWindowWidth;  

            if (minNetVal < 0) minNetVal = 0;
            if (maxNetVal > 1) maxNetVal = 1;

            

            //[gord] restricting the scan range from which the chromatogram is generated greatly improves speed. e.g) on an Orbitrap file
            //if I get the chrom from the entire scan range (18500 scans) the average time is 120ms. If I restrict to a width of 3000 scans
            //the average time is 20ms. But if we are too restrictive, I have seen cases where the real chrom peak is never generated because
            //it fell outside the chrom generator window. 


            int lowerScan = resultColl.Run.GetScanValueForNET(minNetVal);
            if (lowerScan == -1) lowerScan = resultColl.Run.MinScan;

            int upperScan = resultColl.Run.GetScanValueForNET(maxNetVal);
            if (upperScan == -1) upperScan = resultColl.Run.MaxScan;


            XYData chromValues;

            if (ChromatogramGeneratorMode == ChromatogramGeneratorMode.TOP_N_PEAKS)
            {
                List<double> targetMZList = getTargetMZListForTopNPeaks(resultColl.Run.CurrentMassTag, this.IsotopicProfileTarget);

                if (resultColl.Run.MassIsAligned)
                {
                    //if we have alignment information, we can adjust the targetMZ...
                    for (int i = 0; i < targetMZList.Count; i++)
                    {
                        targetMZList[i] = getAlignedMZValue(targetMZList[i], resultColl.Run);
                    }
                   
                }


                ChromatogramGenerator chromGen = new ChromatogramGenerator();
                chromValues = chromGen.GenerateChromatogram(resultColl.MSPeakResultList, lowerScan, upperScan, targetMZList, this.PPMTolerance);

            }
            else
            {
                double targetMZ = getTargetMZBasedOnChromGeneratorMode(resultColl.Run.CurrentMassTag, this.ChromatogramGeneratorMode, this.IsotopicProfileTarget);

                //if we have alignment information, we can adjust the targetMZ...
                if (resultColl.Run.MassIsAligned)
                {
                    targetMZ = getAlignedMZValue(targetMZ, resultColl.Run);
                }

                ChromatogramGenerator chromGen = new ChromatogramGenerator();
                chromValues = chromGen.GenerateChromatogram(resultColl.MSPeakResultList, lowerScan, upperScan, targetMZ, this.PPMTolerance);
            }



            MassTagResultBase result = resultColl.GetMassTagResult(resultColl.Run.CurrentMassTag);
            //result.WasPreviouslyProcessed = true;     // set an indicator that the mass tag has been processed at least once. This indicator is used when the mass tag is processed again (i.e. for labelled data)


            resultColl.Run.XYData = chromValues;

            if (chromValues == null)
            {
                if (result != null)
                {
                    result.Flags.Add(new ChromPeakNotFoundResultFlag());
                }

                return;


            }



            // zeros were inserted wherever discontiguous scans were found.   For some files, MS/MS scans having a 0 should be removed so that we can have a continuous elution peak
            if (resultColl.Run.ContainsMSMSData)
            {
                this.msScanList = resultColl.Run.GetMSLevelScanValues();

                Dictionary<int, double> filteredChromVals = new Dictionary<int, double>();

                for (int i = 0; i < chromValues.Xvalues.Length; i++)
                {
                    int currentScanVal = (int)chromValues.Xvalues[i];

                    if (msScanList.Contains(currentScanVal))
                    {
                        filteredChromVals.Add(currentScanVal, chromValues.Yvalues[i]);
                    }
                }

                chromValues.Xvalues = XYData.ConvertIntsToDouble(filteredChromVals.Keys.ToArray());
                chromValues.Yvalues = filteredChromVals.Values.ToArray();

            }



        }

        private double getAlignedMZValue(double targetMZ, Run run)
        {
            if (run == null) return targetMZ;

            if (run.MassIsAligned)
            {
                return run.GetTargetMZAligned(targetMZ);

            }
            else
            {
                return targetMZ;
            }
        }

      

        private List<double> getTargetMZListForTopNPeaks(MassTag massTag, IsotopicProfileType isotopicProfileTarget)
        {
            List<double> targetMZList = new List<double>();

            IsotopicProfile iso = new IsotopicProfile();
            switch (isotopicProfileTarget)
            {
                case IsotopicProfileType.UNLABELLED:
                    iso = massTag.IsotopicProfile;
                    Check.Require(iso != null && iso.Peaklist != null && iso.Peaklist.Count > 0, "PeakChromatogramGenerator failed. Attempted to generate chromatogram on unlabelled isotopic profile, but profile was never defined.");
                    break;
                case IsotopicProfileType.LABELLED:
                    iso = massTag.IsotopicProfileLabelled;
                    Check.Require(iso != null && iso.Peaklist != null && iso.Peaklist.Count > 0, "PeakChromatogramGenerator failed. Attempted to generate chromatogram on labelled isotopic profile, but profile was never defined.");
                    break;
                default:
                    iso = massTag.IsotopicProfile;
                    Check.Require(iso != null && iso.Peaklist != null && iso.Peaklist.Count > 0, "PeakChromatogramGenerator failed. Attempted to generate chromatogram on unlabelled isotopic profile, but profile was never defined.");
                    break;
            }

            List<MSPeak> msPeakListAboveThreshold = IsotopicProfileUtilities.GetTopMSPeaks(iso.Peaklist, this.TopNPeaksLowerCutOff);

            Check.Require(msPeakListAboveThreshold != null && msPeakListAboveThreshold.Count > 0, "PeakChromatogramGenerator failed. Attempted to generate chromatogram on unlabelled isotopic profile, but profile was never defined.");

            targetMZList = (from n in msPeakListAboveThreshold select n.XValue).ToList();

            return targetMZList;



        }

        private double getTargetMZBasedOnChromGeneratorMode(MassTag massTag, ChromatogramGeneratorMode chromatogramGeneratorMode, IsotopicProfileType isotopicProfileTarget)
        {
            IsotopicProfile iso = new IsotopicProfile();
            switch (isotopicProfileTarget)
            {
                case IsotopicProfileType.UNLABELLED:
                    iso = massTag.IsotopicProfile;
                    Check.Require(iso != null && iso.Peaklist != null && iso.Peaklist.Count > 0, "PeakChromatogramGenerator failed. Attempted to generate chromatogram on unlabelled isotopic profile, but profile was never defined.");
                    break;
                case IsotopicProfileType.LABELLED:
                    iso = massTag.IsotopicProfileLabelled;
                    Check.Require(iso != null && iso.Peaklist != null && iso.Peaklist.Count > 0, "PeakChromatogramGenerator failed. Attempted to generate chromatogram on labelled isotopic profile, but profile was never defined.");
                    break;
                default:
                    iso = massTag.IsotopicProfile;
                    Check.Require(iso != null && iso.Peaklist != null && iso.Peaklist.Count > 0, "PeakChromatogramGenerator failed. Attempted to generate chromatogram on unlabelled isotopic profile, but profile was never defined.");
                    break;
            }

            MSPeak msPeak;
            switch (chromatogramGeneratorMode)
            {

                case ChromatogramGeneratorMode.MONOISOTOPIC_PEAK:
                    msPeak = iso.getMonoPeak();
                    break;
                case ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK:
                    msPeak = iso.getMostIntensePeak();
                    break;
                case ChromatogramGeneratorMode.TOP_N_PEAKS:
                    throw new NotSupportedException();
                default:
                    msPeak = iso.getMostIntensePeak();
                    break;
            }

            return msPeak.XValue;




        }


        #endregion

        #region Private Methods
        private int getIndexOfClosestMZValue(List<MSPeakResult> peakList, double targetMZ, int leftIndex, int rightIndex, double toleranceInMZ)
        {
            if (leftIndex <= rightIndex)
            {
                int middle = (leftIndex + rightIndex) / 2;
                if (Math.Abs(targetMZ - peakList[middle].MSPeak.XValue) <= toleranceInMZ)
                {
                    return middle;
                }
                else if (targetMZ < peakList[middle].MSPeak.XValue)
                {
                    return getIndexOfClosestMZValue(peakList, targetMZ, leftIndex, middle - 1, toleranceInMZ);
                }
                else
                {
                    return getIndexOfClosestMZValue(peakList, targetMZ, middle + 1, rightIndex, toleranceInMZ);
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


        public XYData getChromValues2(List<MSPeakResult> filteredPeakList, Run run)
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
        #endregion

        /// <summary>
        /// Recursive binary search algorithm for searching through MSPeakResults
        /// </summary>


        
    }
}
