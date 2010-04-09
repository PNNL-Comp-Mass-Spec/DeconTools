using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using DeconTools.Backend.ProcessingTasks.Smoothers;

namespace DeconTools.Backend.Algorithms
{
    public class IsotopicProfileMultiChromatogramExtractor
    {
        int m_numPeaks;
        double m_toleranceInPPM;
        

        #region Constructors



        /// <summary>
        /// 
        /// </summary>
        /// <param name="peakList">List of peaks from which chromatogram is constructed</param>
        /// <param name="theorIso">Theoretical isotopic profile, whose peaks are used as the target m/z values for generating the chromatogram</param>
        /// <param name="numPeaks">The number of peaks from the theoretical isotopic profile.  i.e. three numPeaks means that three chromatograms are generated for the top three most intense peaks of the theor isotopic profile </param>
        public IsotopicProfileMultiChromatogramExtractor(int numPeaks, double toleranceInPPM)
        {
            this.m_numPeaks = numPeaks;
            this.m_toleranceInPPM = toleranceInPPM;
        }

        #endregion

        #region Properties
        #endregion

        #region Public Methods

        public Dictionary<MSPeak, XYData> GetChromatogramsForIsotopicProfilePeaks(List<MSPeakResult>peakList, IsotopicProfile theorIso)
        {
            return GetChromatogramsForIsotopicProfilePeaks(peakList, theorIso,false,null);
        }

        public Dictionary<MSPeak, XYData> GetChromatogramsForIsotopicProfilePeaks(List<MSPeakResult> peakList, IsotopicProfile theorIso, bool filterOutMSMSScans, List<int> ms1LevelScanTable)
        {
            IList<IPeak> topTheorIsoPeaks = getTopPeaks(theorIso, m_numPeaks);
            ChromatogramGenerator chromGen = new ChromatogramGenerator();

            Dictionary<MSPeak, XYData> chromatogramsForIsotopicProfiles = new Dictionary<MSPeak, XYData>();

            foreach (var peak in topTheorIsoPeaks)
            {
                XYData xydata = chromGen.GenerateChromatogram(peakList, peakList.First().Scan_num, peakList.Last().Scan_num, peak.XValue, m_toleranceInPPM);

                if (filterOutMSMSScans && ms1LevelScanTable!=null)
                {
                    Dictionary<int, double> filteredChromVals = new Dictionary<int, double>();

                    for (int i = 0; i < xydata.Xvalues.Length; i++)
                    {
                        int currentScanVal = (int)xydata.Xvalues[i];

                        if (ms1LevelScanTable.Contains(currentScanVal))
                        {
                            filteredChromVals.Add(currentScanVal, xydata.Yvalues[i]);
                        }
                    }

                    xydata.Xvalues = XYData.ConvertIntsToDouble(filteredChromVals.Keys.ToArray());
                    xydata.Yvalues = filteredChromVals.Values.ToArray();
                }

                
                chromatogramsForIsotopicProfiles.Add((MSPeak)peak, xydata);
            }

            return chromatogramsForIsotopicProfiles;


        }


        public void SmoothChromatograms(Dictionary<MSPeak, XYData> chromatograms, ISmoother smoother)
        {
            foreach (MSPeak peak in chromatograms.Keys.ToList())
            {
                chromatograms[peak] = smoother.Smooth(chromatograms[peak]);
            }
        }



        #endregion

        #region Private Methods
        private IList<IPeak> getTopPeaks(IsotopicProfile theorIso1, int numPeaks)
        {

            IPeak peak = theorIso1.Peaklist[0];

            IList<IPeak> sortedList = new List<IPeak>();
            sortedList.Add(theorIso1.Peaklist[0]);

            for (int i = 1; i < theorIso1.Peaklist.Count; i++)
            {
                if (theorIso1.Peaklist[i].Height > sortedList[0].Height)
                {
                    sortedList.Insert(0, theorIso1.Peaklist[i]);
                }
                else
                {
                    sortedList.Add(theorIso1.Peaklist[i]);
                }
            }

            return sortedList.Take(numPeaks).ToList();


        }
        #endregion
    }
}
