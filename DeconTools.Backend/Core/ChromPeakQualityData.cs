﻿
namespace DeconTools.Backend.Core
{
    public class ChromPeakQualityData
    {
        public ChromPeak Peak { get; }
        public int ScanLc { get; set; }
        public bool IsotopicProfileFound { get; set; }
        public bool IsIsotopicProfileFlagged { get; set; }
        public double FitScore { get; set; }
        public double InterferenceScore { get; set; }
        public double Abundance { get; set; }
        public IsotopicProfile IsotopicProfile { get; set; }

        public ChromPeakQualityData(ChromPeak peak)
        {
            InterferenceScore = 1;     // worst possible
            FitScore = 1;   // worst possible
            Abundance = 0;
            Peak = peak;
            IsotopicProfileFound = false;
            IsIsotopicProfileFlagged = false;
        }

        public string Display()
        {
            return (Peak.XValue.ToString("0.00") + "\t" + Peak.NETValue.ToString("0.0000") + "\t" + Abundance + "\t" + FitScore.ToString("0.0000") + "\t" + InterferenceScore.ToString("0.000") + "\t" + IsIsotopicProfileFlagged);
        }
    }
}
