using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.DTO;

namespace DeconTools.Backend.Core
{
    public abstract class PeakChrom
    {

        #region Constructors
        #endregion

        #region Properties

        /// <summary>
        /// Acts as storage for the ChromData
        /// </summary>
        public XYData XYData { get; set; }

        public bool IsNullOrEmpty
        {
            get
            {
                if (ChromSourceData == null || ChromSourceData.Count == 0)
                {
                    return true;
                }

                return false;
            }

        }

        public bool PeakDataIsNullOrEmpty
        {
            get
            {
                if (PeakList == null || PeakList.Count == 0)
                {
                    return true;
                }

                return false;
            }
        }



        public XYData GetXYDataFromChromPeakData(int minScan, int maxScan)
        {
        
            //populate array with zero intensities.
            var xyValues = new SortedDictionary<int, double>();
            for (var i = minScan; i <= maxScan; i++)
            {
                xyValues.Add(i, 0);
            }


            var msPeakDataIsEmpty = (ChromSourceData == null || ChromSourceData.Count == 0);
            if (!msPeakDataIsEmpty)
            {
                //iterate over the peaklist, assign chromID,  and extract intensity values
                for (var i = 0; i < ChromSourceData.Count; i++)
                {
                    var p = ChromSourceData[i];
                    double intensity = p.MSPeak.Height;

                    //because we have tolerances to filter the peaks, more than one m/z peak may occur for a given scan. So will take the most abundant...

                    if (xyValues.ContainsKey(p.Scan_num))
                    {

                        if (intensity > xyValues[p.Scan_num])
                        {
                            xyValues[p.Scan_num] = intensity;
                        }
                    }

                }
            }


            var outputXYData = new XYData();

            outputXYData.Xvalues = XYData.ConvertIntsToDouble(xyValues.Keys.ToArray());
            outputXYData.Yvalues = xyValues.Values.ToArray();

            return outputXYData;

        }



        /// <summary>
        /// If the chromatogram was based on peak-level data we may store the Peak-based chrom data here
        /// </summary>
        public List<MSPeakResult> ChromSourceData { get; set; }

        /// <summary>
        /// Peaks from this chromatogram can be stored here
        /// </summary>
        public List<Peak> PeakList { get; set; }

        #endregion

        #region Public Methods
        public List<MSPeakResult> GetMSPeakMembersForGivenChromPeak(Peak chromPeak, double scanTolerance)
        {
            var msPeakDataIsEmpty = (ChromSourceData == null || ChromSourceData.Count == 0);
            if (msPeakDataIsEmpty) return null;

            var minScan = (int)Math.Floor(chromPeak.XValue - scanTolerance);
            var maxScan = (int)Math.Ceiling(chromPeak.XValue + scanTolerance);

            var filteredMSPeaks = (from n in ChromSourceData where n.Scan_num >= minScan && n.Scan_num <= maxScan select n).ToList();
            return filteredMSPeaks;
        }

        public List<MSPeakResult> GetMSPeakMembersForGivenChromPeakAndAssignChromID(Peak chromPeak, double scanTolerance, int id)
        {
            var peaksToBeAssignedID = GetMSPeakMembersForGivenChromPeak(chromPeak, scanTolerance);
            foreach (var peak in peaksToBeAssignedID)
            {
                peak.ChromID = id;
            }

            return peaksToBeAssignedID;

        }


        #endregion

        #region Private Methods

        #endregion


        public Peak GetChromPeakForGivenSource(MSPeakResult peakResult)
        {
            if (PeakDataIsNullOrEmpty) return null;

            double averagePeakWidth = PeakList.Average(p => p.Width);
            var peakWidthSigma = averagePeakWidth / 2.35;    //   width@half-height =  2.35σ   (Gaussian peak theory)

            var fourSigma = 4 * peakWidthSigma;

            return GetChromPeakForGivenSource(peakResult, fourSigma);

        }


        public Peak GetChromPeakForGivenSource(MSPeakResult peakResult, double scanTolerance)
        {

            if (PeakDataIsNullOrEmpty) return null;

      
            var peakQuery = (from n in PeakList where Math.Abs(n.XValue - peakResult.Scan_num) <= scanTolerance select n);

            var peaksWithinTol = peakQuery.Count();
            if (peaksWithinTol == 0)
            {
                return null;
            }

            peakQuery = peakQuery.OrderByDescending(p => p.Height);
            return peakQuery.First();






        }
    }
}
