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
                if (this.ChromSourceData == null || this.ChromSourceData.Count == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }

        }

        public bool PeakDataIsNullOrEmpty
        {
            get
            {
                if (this.PeakList == null || this.PeakList.Count == 0)
                {
                    return true;
                }
                else
                {
                    return false;

                }
            }
        }



        public XYData GetXYDataFromChromPeakData(int minScan, int maxScan)
        {
        
            //populate array with zero intensities.
            SortedDictionary<int, double> xyValues = new SortedDictionary<int, double>();
            for (int i = minScan; i <= maxScan; i++)
            {
                xyValues.Add(i, 0);
            }


            bool msPeakDataIsEmpty = (this.ChromSourceData == null || this.ChromSourceData.Count == 0);
            if (!msPeakDataIsEmpty)
            {
                //iterate over the peaklist, assign chromID,  and extract intensity values
                for (int i = 0; i < this.ChromSourceData.Count; i++)
                {
                    MSPeakResult p = this.ChromSourceData[i];
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


            XYData outputXYData = new XYData();

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
        public List<IPeak> PeakList { get; set; }

        #endregion

        #region Public Methods
        public List<MSPeakResult> GetMSPeakMembersForGivenChromPeak(IPeak chromPeak, double scanTolerance)
        {
            bool msPeakDataIsEmpty = (this.ChromSourceData == null || this.ChromSourceData.Count == 0);
            if (msPeakDataIsEmpty) return null;

            int minScan = (int)Math.Floor(chromPeak.XValue - scanTolerance);
            int maxScan = (int)Math.Ceiling(chromPeak.XValue + scanTolerance);

            List<MSPeakResult> filteredMSPeaks = (from n in this.ChromSourceData where n.Scan_num >= minScan && n.Scan_num <= maxScan select n).ToList();
            return filteredMSPeaks;
        }

        public List<MSPeakResult> GetMSPeakMembersForGivenChromPeakAndAssignChromID(IPeak chromPeak, double scanTolerance, int id)
        {
            List<MSPeakResult> peaksToBeAssignedID = GetMSPeakMembersForGivenChromPeak(chromPeak, scanTolerance);
            foreach (var peak in peaksToBeAssignedID)
            {
                peak.ChromID = id;
            }

            return peaksToBeAssignedID;

        }


        #endregion

        #region Private Methods

        #endregion


        public IPeak GetChromPeakForGivenSource(MSPeakResult peakResult)
        {
            if (this.PeakDataIsNullOrEmpty) return null;

            double averagePeakWidth = this.PeakList.Average(p => p.Width);
            double peakWidthSigma = averagePeakWidth / 2.35;    //   width@half-height =  2.35σ   (Gaussian peak theory)

            double fourSigma = 4 * peakWidthSigma;

            return GetChromPeakForGivenSource(peakResult, fourSigma);

        }


        public IPeak GetChromPeakForGivenSource(MSPeakResult peakResult, double scanTolerance)
        {

            if (this.PeakDataIsNullOrEmpty) return null;

      
            var peakQuery = (from n in this.PeakList where Math.Abs(n.XValue - peakResult.Scan_num) <= scanTolerance select n);

            int peaksWithinTol = peakQuery.Count();
            if (peaksWithinTol == 0)
            {
                return null;
            }

            peakQuery = peakQuery.OrderByDescending(p => p.Height);
            return peakQuery.First();






        }
    }
}
