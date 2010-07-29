using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.DTO;

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
        /// Will generate a chromatogram that is in fact a combination of chromatograms based on user-supplied target m/z values. 
        /// This is geared for producing a chromatogram for an isotopic profile, but only using narrow mass ranges
        /// that encompass individual peaks of an isotopic profile. 
        /// </summary>
        /// <param name="msPeakList"></param>
        /// <param name="minScan"></param>
        /// <param name="maxScan"></param>
        /// <param name="targetMZList"></param>
        /// <param name="toleranceInPPM"></param>
        /// <returns></returns>
        public XYData GenerateChromatogram(List<MSPeakResult> msPeakList, int minScan, int maxScan, List<double> targetMZList, double toleranceInPPM)
        {
            int scanTolerance = 5;     // TODO:   keep an eye on this

            int indexOfLowerScan = getIndexOfClosestScanValue(msPeakList, minScan, 0, msPeakList.Count - 1, scanTolerance);
            int indexOfUpperScan = getIndexOfClosestScanValue(msPeakList, maxScan, 0, msPeakList.Count - 1, scanTolerance);

            int currentIndex = indexOfLowerScan;
            List<MSPeakResult> filteredPeakList = new List<MSPeakResult>();
            while (currentIndex <= indexOfUpperScan)
            {
                filteredPeakList.Add(msPeakList[currentIndex]);
                currentIndex++;
            }

            XYData chromData = null;

            foreach (var targetMZ in targetMZList)
            {
                double lowerMZ = targetMZ - toleranceInPPM * targetMZ / 1e6;
                double upperMZ = targetMZ + toleranceInPPM * targetMZ / 1e6;

                

                List<MSPeakResult> tempPeakList = filteredPeakList.Where(p => p.MSPeak.XValue >= lowerMZ && p.MSPeak.XValue <= upperMZ).ToList();

                if (tempPeakList == null || tempPeakList.Count == 0)
                {

                }
                else
                {
                    XYData currentChromdata = getChromDataAndFillInZeros(tempPeakList);
                    chromData = AddCurrentXYDataToBaseXYData(chromData, currentChromdata);
                }

            }

            return chromData;
        }


        /// <summary>
        /// Generates chromatogram based on a single m/z value and a given tolerance for a range of scans. 
        /// </summary>
        /// <param name="msPeakList"></param>
        /// <param name="minScan"></param>
        /// <param name="maxScan"></param>
        /// <param name="targetMZ"></param>
        /// <param name="toleranceInPPM"></param>
        /// <returns></returns>
        public XYData GenerateChromatogram(List<MSPeakResult> msPeakList, int minScan, int maxScan, double targetMZ, double toleranceInPPM)
        {
            List<double> targetMZList = new List<double>();
            targetMZList.Add(targetMZ);

            return GenerateChromatogram(msPeakList, minScan, maxScan, targetMZList, toleranceInPPM);
        }

        private XYData getChromDataAndFillInZeros(List<MSPeakResult> filteredPeakList)
        {

            int leftZeroPadding = 200;   //number of scans to the left of the minscan for which zeros will be added
            int rightZeroPadding = 200;   //number of scans to the left of the minscan for which zeros will be added

            int peakListMinScan = filteredPeakList[0].Scan_num;
            int peakListMaxScan = filteredPeakList[filteredPeakList.Count - 1].Scan_num;

            //will pad min and max scans with zeros, and add zeros in between. This allows smoothing to execute properly

            peakListMinScan = peakListMinScan - leftZeroPadding;
            peakListMaxScan = peakListMaxScan + rightZeroPadding;

            if (peakListMinScan < 0) peakListMinScan = 0;

            //populate array with zero intensities.
            SortedDictionary<int, double> xyValues = new SortedDictionary<int, double>();
            for (int i = peakListMinScan; i <= peakListMaxScan; i++)
            {
                xyValues.Add(i, 0);
            }

            //iterate over the peaklist and extract intensity values
            for (int i = 0; i < filteredPeakList.Count; i++)
            {
                double intensity = filteredPeakList[i].MSPeak.Height;

                //because we have tolerances to filter the peaks, more than one m/z peak may occur for a given scan. So will take the most abundant...
                if (intensity > xyValues[filteredPeakList[i].Scan_num])
                {
                    xyValues[filteredPeakList[i].Scan_num] = intensity;
                }

            }

            XYData outputXYData = new XYData();

            outputXYData.Xvalues = XYData.ConvertIntsToDouble(xyValues.Keys.ToArray());
            outputXYData.Yvalues = xyValues.Values.ToArray();

            return outputXYData;


        }

        private XYData AddCurrentXYDataToBaseXYData(XYData baseData, XYData newdata)
        {
            XYData returnedData = new XYData();

            if (baseData == null)
            {
                returnedData = newdata;
            }
            else
            {
                //this might need to be cleaned up   :)

                //first add the base data
                SortedDictionary<int, double> baseValues = new SortedDictionary<int, double>();
                for (int i = 0; i < baseData.Xvalues.Length; i++)
                {
                    baseValues.Add((int)baseData.Xvalues[i], baseData.Yvalues[i]);
                }


                //now combine base data with the new
                for (int i = 0; i < newdata.Xvalues.Length; i++)
                {
                    int scanToBeInserted = (int)newdata.Xvalues[i];
                    double intensityToBeInserted = newdata.Yvalues[i];

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
        private int getIndexOfClosestScanValue(List<MSPeakResult> peakList, int targetScan, int leftIndex, int rightIndex, int scanTolerance)
        {
            if (leftIndex < rightIndex)
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
            else if (leftIndex == rightIndex)
            {

                {
                    return leftIndex;

                }
            }
            return leftIndex;    // if fails to find...  will return the inputted left-most scan
        }

        #endregion


    }
}
