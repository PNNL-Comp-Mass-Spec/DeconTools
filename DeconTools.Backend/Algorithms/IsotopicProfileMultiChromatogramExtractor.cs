using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.DTO;
using DeconTools.Backend.ProcessingTasks.Smoothers;

namespace DeconTools.Backend.Algorithms
{
    public class IsotopicProfileMultiChromatogramExtractor
    {
        readonly int m_numPeaks;
        readonly double m_toleranceInPPM;

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="numPeaks">The number of peaks from the theoretical isotopic profile.  i.e. three numPeaks means that three chromatograms are generated for the top three most intense peaks of the theor isotopic profile </param>
        /// <param name="toleranceInPPM">Tolerance, in ppm</param>
        public IsotopicProfileMultiChromatogramExtractor(int numPeaks, double toleranceInPPM)
        {
            m_numPeaks = numPeaks;
            m_toleranceInPPM = toleranceInPPM;
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
            var topTheorIsoPeaks = getTopPeaks(theorIso, m_numPeaks);
            var chromGen = new ChromatogramGenerator();

            var chromatogramsForIsotopicProfiles = new Dictionary<MSPeak, XYData>();

            foreach (var peak in topTheorIsoPeaks)
            {
                var xydata = chromGen.GenerateChromatogram(peakList, peakList.First().Scan_num, peakList.Last().Scan_num, peak.XValue, m_toleranceInPPM);

                if (filterOutMSMSScans && ms1LevelScanTable!=null)
                {
                    var filteredChromVals = new Dictionary<int, double>();

                    for (var i = 0; i < xydata.Xvalues.Length; i++)
                    {
                        var currentScanVal = (int)xydata.Xvalues[i];

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


        public void SmoothChromatograms(Dictionary<MSPeak, XYData> chromatograms, Smoother smoother)
        {
            foreach (var peak in chromatograms.Keys.ToList())
            {
                chromatograms[peak] = smoother.Smooth(chromatograms[peak]);
            }
        }



        #endregion

        #region Private Methods
        private IList<Peak> getTopPeaks(IsotopicProfile theorIso1, int numPeaks)
        {
            IList<Peak> sortedList = new List<Peak>();
            sortedList.Add(theorIso1.Peaklist[0]);

            for (var i = 1; i < theorIso1.Peaklist.Count; i++)
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
