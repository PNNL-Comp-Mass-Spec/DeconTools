using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.Algorithms
{
    public class ChromatogramGenerator
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods

        /// <summary>
        /// Generates chromatogram based on a single m/z value and a given tolerance for a range of scans.
        /// </summary>
        /// <param name="msPeakList"></param>
        /// <param name="minScan"></param>
        /// <param name="maxScan"></param>
        /// <param name="targetMZ"></param>
        /// <param name="tolerance"></param>
        /// <param name="toleranceUnit"></param>
        /// <returns></returns>
        public XYData GenerateChromatogram(List<MSPeakResult> msPeakList, int minScan, int maxScan, double targetMZ, double tolerance, Globals.ToleranceUnit toleranceUnit = Globals.ToleranceUnit.PPM)
        {
            var targetMZList = new List<double>
            {
                targetMZ
            };

            return GenerateChromatogram(msPeakList, minScan, maxScan, targetMZList, tolerance, toleranceUnit);
        }

        public XYData GenerateChromatogram(List<MSPeakResult> msPeakList, int minScan, int maxScan, double targetMZ, double tolerance, int chromIDToAssign, Globals.ToleranceUnit toleranceUnit = Globals.ToleranceUnit.PPM)
        {
            var targetMZList = new List<double>
            {
                targetMZ
            };

            return GenerateChromatogram(msPeakList, minScan, maxScan, targetMZList, tolerance, chromIDToAssign, toleranceUnit);
        }

        /// <summary>
        /// Will generate a chromatogram that is in fact a combination of chromatograms based on user-supplied target m/z values.
        /// This is geared for producing a chromatogram for an isotopic profile, but only using narrow mass ranges
        /// that encompass individual peaks of an isotopic profile.
        /// </summary>
        /// <param name="msPeakList"></param>
        /// <param name="minScan"></param>
        /// <param name="maxScan"></param>
        /// <param name="targetMZList"></param>
        /// <param name="tolerance"></param>
        /// <param name="toleranceUnit"></param>
        /// <returns></returns>
        public XYData GenerateChromatogram(List<MSPeakResult> msPeakList, int minScan, int maxScan, List<double> targetMZList, double tolerance, Globals.ToleranceUnit toleranceUnit = Globals.ToleranceUnit.PPM)
        {
            var defaultChromID = 0;

            return GenerateChromatogram(msPeakList, minScan, maxScan, targetMZList, tolerance, defaultChromID, toleranceUnit);
        }

        /// <summary>
        /// Will generate a chromatogram that is in fact a combination of chromatograms based on user-supplied target m/z values.
        /// This is geared for producing a chromatogram for an isotopic profile, but only using narrow mass ranges
        /// that encompass individual peaks of an isotopic profile.
        /// </summary>
        /// <param name="groupedMsPeakList"></param>
        /// <param name="minScan"></param>
        /// <param name="maxScan"></param>
        /// <param name="targetMZList"></param>
        /// <param name="tolerance"></param>
        /// <param name="toleranceUnit"></param>
        /// <returns></returns>
        public XYData GenerateChromatogram(Dictionary<int, List<MSPeakResult>> groupedMsPeakList, int minScan, int maxScan, List<double> targetMZList, double tolerance, Globals.ToleranceUnit toleranceUnit = Globals.ToleranceUnit.PPM)
        {
            var defaultChromID = 0;

            return GenerateChromatogram(groupedMsPeakList, minScan, maxScan, targetMZList, tolerance, defaultChromID, toleranceUnit);
        }

        //TODO:  make a ChromatogramObject that will help handle my MSPeakResults, etc.
        public XYData GenerateChromatogram(Dictionary<int, List<MSPeakResult>> groupedMsPeakList, int minScan, int maxScan, List<double> targetMZList, double tolerance, int chromIDToAssign, Globals.ToleranceUnit toleranceUnit = Globals.ToleranceUnit.PPM)
        {
            Check.Require(groupedMsPeakList != null && groupedMsPeakList.Count > 0, "Cannot generate chromatogram. Source msPeakList is empty or hasn't been defined.");

            var scanTolerance = 5;     // TODO:   keep an eye on this

            // PNNLOmics.Generic.AnonymousComparer<MSPeakResult> comparer = new PNNLOmics.Generic.AnonymousComparer<MSPeakResult>((x, y) => x.MSPeak.XValue.CompareTo(y.MSPeak.XValue));
            var comparer = new MSPeakResultComparer();

            var tempPeakList = new List<MSPeakResult>();

            for (var i = minScan - scanTolerance; i < maxScan + scanTolerance; i++)
            {
                if (groupedMsPeakList == null || !groupedMsPeakList.TryGetValue(i, out var msPeakResultList))
                {
                    continue;
                }

                foreach (var targetMZ in targetMZList)
                {
                    double lowerMZ;
                    double upperMZ;

                    if (toleranceUnit == Globals.ToleranceUnit.PPM)
                    {
                        lowerMZ = targetMZ - tolerance*targetMZ/1e6;
                        upperMZ = targetMZ + tolerance*targetMZ/1e6;
                    }
                    else if (toleranceUnit == Globals.ToleranceUnit.MZ)
                    {
                        lowerMZ = targetMZ - tolerance;
                        upperMZ = targetMZ + tolerance;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("Trying to create chromatogram, but the " + toleranceUnit +
                                                              " unit isn't supported");
                    }

                    var lowMsPeak = new MSPeak(lowerMZ);
                    var lowMsPeakResult = new MSPeakResult {MSPeak = lowMsPeak};

                    var binarySearchResult = msPeakResultList.BinarySearch(lowMsPeakResult, comparer);
                    var nearestSearchResult = binarySearchResult >= 0 ? binarySearchResult : ~binarySearchResult;

                    for (var j = nearestSearchResult; j < msPeakResultList.Count; j++)
                    {
                        var msPeakResult = msPeakResultList[j];
                        if(msPeakResult.MSPeak.XValue <= upperMZ)
                        {
                            tempPeakList.Add(msPeakResult);
                        }
                    }
                }
            }

            XYData chromData = null;

            if (!tempPeakList.Any())
            {
                //TODO: we want to return 0 intensity values. But need to make sure there are no downstream problems with this change.
            }
            else
            {
                chromData = GetChromDataAndFillInZerosAndAssignChromID(tempPeakList, chromIDToAssign);
            }

            return chromData;
        }

        public XYData GenerateChromatogram(List<MSPeakResult> msPeakList, int minScan, int maxScan, List<double> targetMZList, double tolerance, int chromIDToAssign, Globals.ToleranceUnit toleranceUnit = Globals.ToleranceUnit.PPM)
        {
            Check.Require(msPeakList != null && msPeakList.Count > 0, "Cannot generate chromatogram. Source msPeakList is empty or hasn't been defined.");

            var scanTolerance = 5;     // TODO:   keep an eye on this

            var indexOfLowerScan = GetIndexOfClosestScanValue(msPeakList, minScan, 0, msPeakList.Count - 1, scanTolerance);
            var indexOfUpperScan = GetIndexOfClosestScanValue(msPeakList, maxScan, 0, msPeakList.Count - 1, scanTolerance);

            XYData chromData = null;

            foreach (var targetMZ in targetMZList)
            {
                double lowerMZ;
                double upperMZ;

                if (toleranceUnit==Globals.ToleranceUnit.PPM)
                {
                    lowerMZ = targetMZ - tolerance * targetMZ / 1e6;
                    upperMZ = targetMZ + tolerance * targetMZ / 1e6;
                }
                else if (toleranceUnit == Globals.ToleranceUnit.MZ)
                {
                    lowerMZ = targetMZ - tolerance;
                    upperMZ = targetMZ + tolerance;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Trying to create chromatogram, but the "  + toleranceUnit +" unit isn't supported");
                }

                var tempPeakList = new List<MSPeakResult>();

                for (var i = indexOfLowerScan; i <= indexOfUpperScan; i++)
                {
                    var msPeakResult = msPeakList[i];
                    var xValue = msPeakResult.MSPeak.XValue;

                    if (xValue >= lowerMZ && xValue <= upperMZ)
                    {
                        tempPeakList.Add(msPeakResult);
                    }
                }

                if (!tempPeakList.Any())
                {
                    //TODO: we want to return 0 intensity values. But need to make sure there are no downstream problems with this change.
                }
                else
                {
                    var currentChromdata = GetChromDataAndFillInZerosAndAssignChromID(tempPeakList, chromIDToAssign);
                    chromData = AddCurrentXYDataToBaseXYData(chromData, currentChromdata);
                }
            }

            return chromData;
        }


        public XYData GenerateChromatogramFromRawData(Run run, int minScan, int maxScan, double targetMZ, double toleranceInPPM)
        {
            var xydata = new XYData();

            var xvals = new List<double>();
            var yvals = new List<double>();

            for (var scan = minScan; scan <= maxScan; scan++)
            {
                var scanIsGoodToGet = true;

                var currentScanContainsMSMS = (run.ContainsMSMSData && run.GetMSLevel(scan) > 1);
                if (currentScanContainsMSMS)
                {
                    scanIsGoodToGet = false;
                }

                if (!scanIsGoodToGet)
                {
                    continue;
                }

                var scanset = new ScanSet(scan);
                run.GetMassSpectrum(scanset);

                var chromDataPointIntensity = GetChromDataPoint(run.XYData, targetMZ, toleranceInPPM);

                xvals.Add(scan);
                yvals.Add(chromDataPointIntensity);
            }

            xydata.Xvalues = xvals.ToArray();
            xydata.Yvalues = yvals.ToArray();

            return xydata;
        }

        private double GetChromDataPoint(XYData xydata, double targetMZ, double toleranceInPPM)
        {
            var dataIsEmpty = (xydata == null || xydata.Xvalues.Length == 0);
            if (dataIsEmpty)
            {
                return 0;
            }

            var toleranceInMZ = toleranceInPPM * targetMZ / 1e6;

            var lowerMZ = targetMZ - toleranceInMZ;
            var upperMZ = targetMZ + toleranceInMZ;

            var startingPointMZ = lowerMZ - 2;

            var indexOfGoodStartingPoint = MathUtils.BinarySearchWithTolerance(xydata.Xvalues, startingPointMZ, 0, xydata.Xvalues.Length - 1, 1.9);

            if (indexOfGoodStartingPoint == -1)
            {
                indexOfGoodStartingPoint = 0;
            }

            double intensitySum = 0;

            for (var i = indexOfGoodStartingPoint; i < xydata.Xvalues.Length; i++)
            {
                if (xydata.Xvalues[i] >= lowerMZ)
                {

                    if (xydata.Xvalues[i] > upperMZ)
                    {
                        break;
                    }
                    intensitySum = +xydata.Yvalues[i];
                }
            }

            return intensitySum;
        }



        [Obsolete("Use the other GeneratePeakChromatogram!")]
        public List<MSPeakResult> GeneratePeakChromatogram(List<MSPeakResult> msPeakList, int minScan, int maxScan, List<double> targetMZList, double toleranceInPPM)
        {
            var scanTolerance = 5;     // TODO:   keep an eye on this

            var indexOfLowerScan = GetIndexOfClosestScanValue(msPeakList, minScan, 0, msPeakList.Count - 1, scanTolerance);
            var indexOfUpperScan = GetIndexOfClosestScanValue(msPeakList, maxScan, 0, msPeakList.Count - 1, scanTolerance);

            var currentIndex = indexOfLowerScan;
            var filteredPeakList = new List<MSPeakResult>();
            while (currentIndex <= indexOfUpperScan)
            {
                filteredPeakList.Add(msPeakList[currentIndex]);
                currentIndex++;
            }

            var compiledChromPeakList = new List<MSPeakResult>();

            var counter = 0;
            foreach (var targetMZ in targetMZList)
            {
                counter++;
                var lowerMZ = targetMZ - toleranceInPPM * targetMZ / 1e6;
                var upperMZ = targetMZ + toleranceInPPM * targetMZ / 1e6;

                var tempPeakList = filteredPeakList.Where(p => p.MSPeak.XValue >= lowerMZ && p.MSPeak.XValue <= upperMZ).ToList();

                compiledChromPeakList.AddRange(tempPeakList);
            }

            if (counter > 1) // if the list contains multiple peak chromatograms, then need to sort.  Otherwise, don't need to sort.
            {
                compiledChromPeakList.Sort(delegate(MSPeakResult peak1, MSPeakResult peak2)
                {
                    return peak2.Scan_num.CompareTo(peak1.Scan_num);
                });
            }

            return compiledChromPeakList;
        }

        [Obsolete("Use the other GeneratePeakChromatogram!")]
        public List<MSPeakResult> GeneratePeakChromatogram(List<MSPeakResult> msPeakList, int minScan, int maxScan, double targetMZ, double toleranceInPPM)
        {
            var targetMZList = new List<double>
            {
                targetMZ
            };

            return GeneratePeakChromatogram(msPeakList, minScan, maxScan, targetMZList, toleranceInPPM);
        }

        private XYData GetChromDataAndFillInZerosAndAssignChromID(IReadOnlyList<MSPeakResult> filteredPeakList, int chromID)
        {
            var filteredPeakListCount = filteredPeakList.Count;

            var leftZeroPadding = 200;   //number of scans to the left of the minscan for which zeros will be added
            var rightZeroPadding = 200;   //number of scans to the left of the minscan for which zeros will be added

            var peakListMinScan = filteredPeakList[0].Scan_num;
            var peakListMaxScan = filteredPeakList[filteredPeakList.Count - 1].Scan_num;

            //will pad min and max scans with zeros, and add zeros in between. This allows smoothing to execute properly

            peakListMinScan = peakListMinScan - leftZeroPadding;
            peakListMaxScan = peakListMaxScan + rightZeroPadding;

            if (peakListMinScan < 0) peakListMinScan = 0;

            //populate dictionary with zero intensities.
            var xyValues = new Dictionary<int, double>(peakListMaxScan - peakListMinScan + 1);
            for (var i = peakListMinScan; i <= peakListMaxScan; i++)
            {
                xyValues.Add(i, 0);
            }

            //iterate over the peaklist, assign chromID,  and extract intensity values
            for (var i = 0; i < filteredPeakListCount; i++)
            {
                var peakResult = filteredPeakList[i];

                //NOTE:   we assign the chromID here.
                peakResult.ChromID = chromID;

                double intensity = peakResult.MSPeak.Height;
                var scanNumber = peakResult.Scan_num;

                //because we have tolerances to filter the peaks, more than one m/z peak may occur for a given scan. So will take the most abundant...

                if (!xyValues.ContainsKey(scanNumber))
                {
                    var errorString = "Unexpected error in chromatogram generator!! Scan= " + scanNumber +
                                        "; num filtered peaks = " + filteredPeakListCount;

                    Console.WriteLine(errorString);

                    throw new InvalidProgramException(errorString);
                }

                if (intensity > xyValues[scanNumber])
                {
                    xyValues[scanNumber] = intensity;
                }
            }

            var outputXYData = new XYData
            {
                Xvalues = XYData.ConvertIntsToDouble(xyValues.Keys.ToArray()),
                Yvalues = xyValues.Values.ToArray()
            };

            return outputXYData;
        }

        private XYData AddCurrentXYDataToBaseXYData(XYData baseData, XYData newdata)
        {
            var returnedData = new XYData();

            if (baseData == null)
            {
                returnedData = newdata;
            }
            else
            {
                //this might need to be cleaned up   :)

                //first add the base data
                var baseValues = new SortedDictionary<int, double>();
                for (var i = 0; i < baseData.Xvalues.Length; i++)
                {
                    baseValues.Add((int)baseData.Xvalues[i], baseData.Yvalues[i]);
                }

                //now combine base data with the new
                for (var i = 0; i < newdata.Xvalues.Length; i++)
                {
                    var scanToBeInserted = (int)newdata.Xvalues[i];
                    var intensityToBeInserted = newdata.Yvalues[i];

                    if (baseValues.ContainsKey(scanToBeInserted))
                    {
                        baseValues[scanToBeInserted] += intensityToBeInserted;
                    }
                    else
                    {
                        baseValues.Add(scanToBeInserted, intensityToBeInserted);
                    }

                }

                returnedData.Xvalues = XYData.ConvertIntsToDouble(baseValues.Keys.ToArray());
                returnedData.Yvalues = baseValues.Values.ToArray();

            }

            return returnedData;
        }

        #endregion

        #region Private Methods
        private int GetIndexOfClosestScanValue(IReadOnlyList<MSPeakResult> peakList, int targetScan, int leftIndex, int rightIndex, int scanTolerance)
        {
            if (leftIndex < rightIndex)
            {
                var middle = (leftIndex + rightIndex) / 2;
                var scanNumber = peakList[middle].Scan_num;

                if (Math.Abs(targetScan - scanNumber) <= scanTolerance)
                {
                    return middle;
                }

                if (targetScan < scanNumber)
                {
                    return GetIndexOfClosestScanValue(peakList, targetScan, leftIndex, middle - 1, scanTolerance);
                }

                return GetIndexOfClosestScanValue(peakList, targetScan, middle + 1, rightIndex, scanTolerance);
            }

            if (leftIndex == rightIndex)
            {
                {
                    return leftIndex;
                }
            }
            return leftIndex;    // if fails to find...  will return the inputted left-most scan
        }

        #endregion
    }

    class MSPeakResultComparer : Comparer<MSPeakResult>
    {
        // Compares by Length, Height, and Width.
        public override int Compare(MSPeakResult x, MSPeakResult y)
        {
            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            return x.MSPeak.XValue.CompareTo(y.MSPeak.XValue);
        }
    }
}
