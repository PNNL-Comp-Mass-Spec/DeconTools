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
    public class PeakChromatogramGenerator : Task
    {
        int maxZerosToAdd = 2;

        [Obsolete("Unused")]
        readonly List<int> msScanList = new List<int>();

        private readonly ChromatogramGenerator _chromGen;

        #region Constructors
        public PeakChromatogramGenerator()
            : this(20)
        {
        }

        public PeakChromatogramGenerator(double tolerance)
            : this(tolerance, Globals.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK)
        {
        }

        public PeakChromatogramGenerator(double tolerance, Globals.ChromatogramGeneratorMode chromMode)
            : this(tolerance, chromMode, Globals.IsotopicProfileType.UNLABELED)
        {
        }

        public PeakChromatogramGenerator(double tolerance, Globals.ChromatogramGeneratorMode chromMode,
            Globals.IsotopicProfileType isotopicProfileTarget, Globals.ToleranceUnit toleranceUnit = Globals.ToleranceUnit.PPM)
        {
            Tolerance = tolerance;
            ChromatogramGeneratorMode = chromMode;
            IsotopicProfileTarget = isotopicProfileTarget;

            TopNPeaksLowerCutOff = 0.3;
            ChromWindowWidthForNonAlignedData = 0.4f;
            ChromWindowWidthForAlignedData = 0.1f;

            ToleranceUnit = toleranceUnit;

            _chromGen = new ChromatogramGenerator();
        }

        #endregion

        #region Properties
        public Globals.IsotopicProfileType IsotopicProfileTarget { get; set; }

        public Globals.ChromatogramGeneratorMode ChromatogramGeneratorMode { get; set; }
        public Globals.ToleranceUnit ToleranceUnit { get; set; }
        public double Tolerance { get; set; }

        /// <summary>
        /// The width or range of the NET / scan window. A larger value will result in a chromatogram covering more of the dataset scan range.
        /// </summary>
        public float ChromWindowWidthForNonAlignedData { get; set; }

        /// <summary>
        /// The width or range of the NET / scan window. A larger value will result in a chromatogram covering more of the dataset scan range.
        /// For Aligned data, we should be able to use a smaller range which will lead to faster chromatogram generation
        /// </summary>
        public float ChromWindowWidthForAlignedData { get; set; }

        /// <summary>
        /// Peaks of the theoretical isotopic profile that fall below this cutoff will not be used in generating the chromatogram.
        /// </summary>
        public double TopNPeaksLowerCutOff { get; set; }

        #endregion

        #region Public Methods

        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList.MSPeakResultList?.Count > 0, "PeakChromatogramGenerator failed. No peaks.");
            Check.Require(resultList.Run.CurrentMassTag != null, "PeakChromatogramGenerator failed. This requires a MassTag to be specified.");
            if (resultList.Run.CurrentMassTag == null)
            {
                return;
            }

            Check.Require(Math.Abs(resultList.Run.CurrentMassTag.MZ) > float.Epsilon, "PeakChromatogramGenerator failed. MassTag's MZ hasn't been specified.");

            Check.Require(resultList.Run.MaxLCScan > 0, "PeakChromatogramGenerator failed.  Problem with 'MaxScan'");

            //[gord] restricting the scan range from which the chromatogram is generated greatly improves speed. e.g) on an Orbitrap file
            //if I get the chrom from the entire scan range (18500 scans) the average time is 120ms. If I restrict to a width of 3000 scans
            //the average time is 20ms. But if we are too restrictive, I have seen cases where the real chrom peak is never generated because
            //it fell outside the chrom generator window.

            float netElutionTime;
            if (resultList.Run.CurrentMassTag.ElutionTimeUnit == Globals.ElutionTimeUnit.ScanNum)
            {
                netElutionTime = resultList.Run.CurrentMassTag.ScanLCTarget / (float)resultList.Run.GetNumMSScans();
            }
            else
            {
                netElutionTime = resultList.Run.CurrentMassTag.NormalizedElutionTime;
            }

            float minNetVal;
            float maxNetVal;
            if (resultList.Run.NETIsAligned)
            {
                minNetVal = netElutionTime - ChromWindowWidthForAlignedData;
                maxNetVal = netElutionTime + ChromWindowWidthForAlignedData;
            }
            else
            {
                minNetVal = netElutionTime - ChromWindowWidthForNonAlignedData;
                maxNetVal = netElutionTime + ChromWindowWidthForNonAlignedData;
            }

            if (minNetVal < 0)
            {
                minNetVal = 0;
            }

            if (maxNetVal > 1)
            {
                maxNetVal = 1;
            }

            var lowerScan = (int)Math.Floor(resultList.Run.NetAlignmentInfo.GetScanForNet(minNetVal));
            if (lowerScan == -1)
            {
                lowerScan = resultList.Run.MinLCScan;
            }

            var upperScan = (int)Math.Ceiling(resultList.Run.NetAlignmentInfo.GetScanForNet(maxNetVal));
            if (upperScan == -1)
            {
                upperScan = resultList.Run.MaxLCScan;
            }

            List<double> targetMZList;

            if (ChromatogramGeneratorMode == Globals.ChromatogramGeneratorMode.MZ_BASED)
            {
                var currentTargetMZ = resultList.Run.CurrentMassTag.MZ;
                targetMZList = new List<double> { currentTargetMZ };
            }
            else
            {
                var theorIso = resultList.Run.CurrentMassTag.IsotopicProfile;
                targetMZList = GetTargetMzList(theorIso);
            }

            var midScan = (int)((lowerScan + (double)upperScan) / 2);

            if (resultList.Run.MassIsAligned)
            {
                for (var i = 0; i < targetMZList.Count; i++)
                {
                    targetMZList[i] = getAlignedMZValue(targetMZList[i], resultList.Run, midScan);
                }
            }

            var chromValues = _chromGen.GenerateChromatogram(resultList.MSPeakResultList, lowerScan, upperScan, targetMZList, Tolerance, ToleranceUnit);

            var result = resultList.GetTargetedResult(resultList.Run.CurrentMassTag);
            //result.WasPreviouslyProcessed = true;     // set an indicator that the mass tag has been processed at least once. This indicator is used when the mass tag is processed again (i.e. for labeled data)

            resultList.Run.XYData = chromValues;

            if (chromValues == null)
            {
                if (result != null)
                {
                    result.FailedResult = true;
                    result.FailureType = Globals.TargetedResultFailureType.ChromDataNotFound;

                    result.Flags.Add(new ChromPeakNotFoundResultFlag());
                }

                return;
            }

            resultList.Run.XYData = FilterOutDataBasedOnMsMsLevel(resultList.Run, resultList.Run.XYData, result.Run.CurrentMassTag.MsLevel, false);
        }

        private List<double> GetTargetMzList(IsotopicProfile theorIso)
        {
            List<double> targetMZList;
            switch (ChromatogramGeneratorMode)
            {
                case Globals.ChromatogramGeneratorMode.MZ_BASED:
                    {
                        throw new NotSupportedException("Don't use this method if you already know your MZ target.");
                    }
                case Globals.ChromatogramGeneratorMode.TOP_N_PEAKS:
                    {
                        targetMZList = getTargetMZListForTopNPeaks(theorIso);
                    }
                    break;
                case Globals.ChromatogramGeneratorMode.O16O18_THREE_MONOPEAKS:
                    {
                        targetMZList = getTargetMZListForO16O18ThreeMonoPeaks(theorIso);
                    }
                    break;
                case Globals.ChromatogramGeneratorMode.MONOISOTOPIC_PEAK:
                    {
                        var targetMZ = theorIso.getMonoPeak().XValue;
                        targetMZList = new List<double> { targetMZ };
                        break;
                    }
                case Globals.ChromatogramGeneratorMode.MOST_ABUNDANT_PEAK:
                    {
                        var targetMZ = theorIso.getMostIntensePeak().XValue;
                        targetMZList = new List<double> { targetMZ };
                        break;
                    }
                default:
                    {
                        throw new NotSupportedException(
                            "Chromatogram generation failed. Selected ChromatogramGeneratorMode is not supported");
                    }
            }
            return targetMZList;
        }

        public XYData GenerateChromatogram(Run run, List<double> targetMzList, double elutionTimeCenter = 0.5, Globals.ElutionTimeUnit elutionTimeUnit = Globals.ElutionTimeUnit.NormalizedElutionTime)
        {
            var lowerScan = run.MinLCScan;
            var upperScan = run.MaxLCScan;

            if (elutionTimeUnit == Globals.ElutionTimeUnit.NormalizedElutionTime)
            {
                float minNetVal;
                float maxNetVal;
                if (run.NETIsAligned)
                {
                    minNetVal = (float)(elutionTimeCenter - ChromWindowWidthForAlignedData);
                    maxNetVal = (float)(elutionTimeCenter + ChromWindowWidthForAlignedData);
                }
                else
                {
                    minNetVal = (float)(elutionTimeCenter - ChromWindowWidthForNonAlignedData);
                    maxNetVal = (float)(elutionTimeCenter + ChromWindowWidthForNonAlignedData);
                }

                if (minNetVal < 0)
                {
                    minNetVal = 0;
                }

                if (maxNetVal > 1)
                {
                    maxNetVal = 1;
                }

                lowerScan = (int)Math.Floor(run.NetAlignmentInfo.GetScanForNet(minNetVal));
                if (lowerScan == -1)
                {
                    lowerScan = run.MinLCScan;
                }

                upperScan = (int)Math.Ceiling(run.NetAlignmentInfo.GetScanForNet(maxNetVal));
                if (upperScan == -1)
                {
                    upperScan = run.MaxLCScan;
                }
            }
            else if (elutionTimeUnit == Globals.ElutionTimeUnit.ScanNum)
            {
                if (run.NETIsAligned)
                {
                    lowerScan = (int)(elutionTimeCenter - ChromWindowWidthForAlignedData);
                    upperScan = (int)(elutionTimeCenter + ChromWindowWidthForAlignedData);
                }
                else
                {
                    lowerScan = (int)(elutionTimeCenter - ChromWindowWidthForNonAlignedData);
                    upperScan = (int)(elutionTimeCenter + ChromWindowWidthForNonAlignedData);
                }
            }

            if (lowerScan == -1)
            {
                lowerScan = run.MinLCScan;
            }

            if (upperScan == -1)
            {
                upperScan = run.MaxLCScan;
            }

            var midScan = (int)((lowerScan + (double)upperScan) / 2);

            if (run.MassIsAligned)
            {
                for (var i = 0; i < targetMzList.Count; i++)
                {
                    targetMzList[i] = getAlignedMZValue(targetMzList[i], run, midScan);
                }
            }

            var chromValues = _chromGen.GenerateChromatogram(run.ResultCollection.MSPeakResultList, lowerScan, upperScan, targetMzList, Tolerance, ToleranceUnit);

            chromValues = FilterOutDataBasedOnMsMsLevel(run, chromValues, 1, false);

            return chromValues;
        }

        public XYData GenerateChromatogram(Run run, int scanStart, int scanStop, double targetMz, double tolerance, Globals.ToleranceUnit toleranceUnit = Globals.ToleranceUnit.PPM)
        {
            var targetMzList = new List<double> { targetMz };
            return GenerateChromatogram(run, targetMzList, scanStart, scanStop, tolerance, toleranceUnit);
        }

        public XYData GenerateChromatogram(Run run, List<double> targetMzList, int lowerScan, int upperScan, double tolerance, Globals.ToleranceUnit toleranceUnit = Globals.ToleranceUnit.PPM)
        {
            var midScan = (int)((lowerScan + (double)upperScan) / 2);

            if (run.MassIsAligned)
            {
                for (var i = 0; i < targetMzList.Count; i++)
                {
                    targetMzList[i] = getAlignedMZValue(targetMzList[i], run, midScan);
                }
            }

            var chromValues = _chromGen.GenerateChromatogram(run.ResultCollection.GetMsPeakResultsGroupedAndMzOrdered(), lowerScan, upperScan, targetMzList, tolerance, toleranceUnit);

            chromValues = FilterOutDataBasedOnMsMsLevel(run, chromValues, 1, false);

            return chromValues;
        }

        public XYData GenerateChromatogram(Run run, IsotopicProfile theorProfile, int lowerScan, int upperScan, double tolerance, Globals.ToleranceUnit toleranceUnit = Globals.ToleranceUnit.PPM)
        {
            if (ChromatogramGeneratorMode == Globals.ChromatogramGeneratorMode.MZ_BASED)
            {
                throw new NotSupportedException("Don't use this method for MZ_BASED chromatogram generation. Use a different overload");
            }
            var targetMZList = GetTargetMzList(theorProfile);

            return GenerateChromatogram(run, targetMZList, lowerScan, upperScan, tolerance, toleranceUnit);
        }

        public XYData GenerateChromatogram(Run run, double targetMz, double elutionTimeCenter = 0.5, Globals.ElutionTimeUnit elutionTimeUnit = Globals.ElutionTimeUnit.NormalizedElutionTime)
        {
            var targetMzList = new List<double> { targetMz };
            return GenerateChromatogram(run, targetMzList, elutionTimeCenter, elutionTimeUnit);
        }

        public XYData GenerateChromatogram(Run run, IsotopicProfile theorProfile, double elutionTimeCenter = 0.5, Globals.ElutionTimeUnit elutionTimeUnit = Globals.ElutionTimeUnit.NormalizedElutionTime)
        {
            if (ChromatogramGeneratorMode == Globals.ChromatogramGeneratorMode.MZ_BASED)
            {
                throw new NotSupportedException("Don't use this method for MZ_BASED chromatogram generation. Use a different overload");
            }
            var targetMZList = GetTargetMzList(theorProfile);

            return GenerateChromatogram(run, targetMZList, elutionTimeCenter, elutionTimeUnit);
        }

        #endregion

        #region Private Methods

        private XYData FilterOutDataBasedOnMsMsLevel(Run run, XYData xyData, int msLevelToUse = 1, bool usePrimaryLcScanNumberCache = true)
        {
            if (xyData == null || xyData.Xvalues.Length == 0)
            {
                return xyData;
            }

            var filteredXyData = new XYData
            {
                Xvalues = xyData.Xvalues,
                Yvalues = xyData.Yvalues
            };

            if (run.ContainsMSMSData)
            {
                var filteredChromVals = new Dictionary<int, double>();

                var usePrimaryLcScanNumbers = usePrimaryLcScanNumberCache && run.PrimaryLcScanNumbers?.Count > 0;

                for (var i = 0; i < xyData.Xvalues.Length; i++)
                {
                    var currentScanVal = (int)xyData.Xvalues[i];

                    //TODO: this has a problem of cutting off ChromXYData that falls outside the range defined by PrimaryLcScanNumbers. Not good, since this is expected to filter only on MSMS Level
                    //
                    // If the scan is not a primary scan number, then we do not want to consider it
                    if (usePrimaryLcScanNumbers && run.PrimaryLcScanNumbers.BinarySearch(currentScanVal) < 0)
                    {
                        continue;
                    }

                    var msLevel = run.GetMSLevel(currentScanVal);
                    if (msLevel == msLevelToUse)
                    {
                        filteredChromVals.Add(currentScanVal, xyData.Yvalues[i]);
                    }
                }

                filteredXyData.Xvalues = filteredChromVals.Keys.Select(p => (double)p).ToArray();
                filteredXyData.Yvalues = filteredChromVals.Values.ToArray();
            }
            else
            {
                // If we are trying to find MS2 data from a run that does not contain MS2 data, then just return empty arrays
                if (msLevelToUse > 1)
                {
                    filteredXyData.Xvalues = new double[0];
                    filteredXyData.Yvalues = new double[0];
                }
            }

            return filteredXyData;
        }

        private double getAlignedMZValue(double targetMZ, Run run, int lcScan)
        {
            if (run == null)
            {
                return targetMZ;
            }

            if (run.MassIsAligned)
            {
                return run.GetTargetMZAligned(targetMZ, lcScan);
            }

            return targetMZ;
        }

        private List<double> getTargetMZListForO16O18ThreeMonoPeaks(IsotopicProfile iso)
        {
            var targetMZList = new List<double>();

            if (iso.Peaklist.Count > 0)
            {
                targetMZList.Add(iso.Peaklist[0].XValue);
            }

            if (iso.Peaklist.Count > 2)
            {
                targetMZList.Add(iso.Peaklist[2].XValue);
            }

            if (iso.Peaklist.Count > 4)
            {
                targetMZList.Add(iso.Peaklist[4].XValue);
            }

            return targetMZList;
        }

        private List<double> getTargetMZListForTopNPeaks(IsotopicProfile iso)
        {
            var msPeakListAboveThreshold = IsotopicProfileUtilities.GetTopMSPeaks(iso.Peaklist, TopNPeaksLowerCutOff);

            Check.Require(msPeakListAboveThreshold?.Count > 0, "PeakChromatogramGenerator failed. Attempted to generate chromatogram on unlabeled isotopic profile, but profile was never defined.");

            var targetMZList = (from n in msPeakListAboveThreshold select n.XValue).ToList();
            return targetMZList;
        }

        [Obsolete("Unused")]
        private XYData getChromValues(List<MSPeakResult> filteredPeakList, Run run)
        {
            var data = new XYData();

            var xyValues = new SortedDictionary<int, double>();
            foreach (var peak in filteredPeakList)
            {
                //find reference points within the MS-Level scan list.  These are the scans for which we will make sure there is some value declared
                var centerScanPtr = msScanList.IndexOf(peak.Scan_num);

                var leftScanPtr = centerScanPtr - maxZerosToAdd;
                var rightScanPtr = centerScanPtr + maxZerosToAdd;

                //handle the rare case of having a point at the end of the scan list
                if (rightScanPtr >= msScanList.Count)
                {
                    rightScanPtr = msScanList.Count - 1;
                }

                //case of being at the beginning of the scan list
                if (leftScanPtr < 0)
                {
                    leftScanPtr = 0;
                }

                //this loop adds zeros to the left of a chrom value
                for (var i = leftScanPtr; i < centerScanPtr; i++)
                {
                    var targetScan = msScanList[i];
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
                for (var i = centerScanPtr; i <= rightScanPtr; i++)
                {
                    if (i >= msScanList.Count)
                    {
                        Console.WriteLine("something wrong here");
                    }

                    var targetScan = msScanList[i];
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

        [Obsolete("Unused")]
        private XYData GetChromValues2(IReadOnlyList<MSPeakResult> filteredPeakList, Run run)
        {
            var xyData = new XYData();

            var leftZeroPadding = 200;   //number of scans to the left of the min scan for which zeros will be added
            var rightZeroPadding = 200;   //number of scans to the left of the min scan for which zeros will be added

            var peakListMinScan = filteredPeakList[0].Scan_num;
            var peakListMaxScan = filteredPeakList[filteredPeakList.Count - 1].Scan_num;

            //will pad min and max scans with zeros, and add zeros in between. This allows smoothing to execute properly

            peakListMinScan -= leftZeroPadding;
            peakListMaxScan += rightZeroPadding;

            if (peakListMinScan < run.MinLCScan)
            {
                peakListMinScan = run.MinLCScan;
            }

            if (peakListMaxScan > run.MaxLCScan)
            {
                peakListMaxScan = run.MaxLCScan;
            }

            //populate array with zero intensities.
            var xyValues = new SortedDictionary<int, double>();
            for (var i = peakListMinScan; i <= peakListMaxScan; i++)
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

            foreach (var peak in filteredPeakList)
            {
                double intensity = peak.MSPeak.Height;

                //because we have tolerances to filter the peaks, more than one m/z peak may occur for a given scan. So will take the most abundant...
                if (intensity > xyValues[peak.Scan_num])
                {
                    xyValues[peak.Scan_num] = intensity;
                }
            }

            xyData.Xvalues = XYData.ConvertIntsToDouble(xyValues.Keys.ToArray());
            xyData.Yvalues = xyValues.Values.ToArray();

            return xyData;
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

        #endregion

    }
}
