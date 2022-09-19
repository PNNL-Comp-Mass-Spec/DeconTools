using System;
using System.Collections.Generic;
using DeconTools.Utilities;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class IsotopicProfile
    {
        public IsotopicProfile()
        {
            Peaklist = new List<MSPeak>();
        }

        // ReSharper disable once IdentifierTypo
        public List<MSPeak> Peaklist { get; set; }

        /// <summary>
        /// Zero-based index value that points to which peak of the PeakList is the monoisotopic peak.  (it isn't always the first one)
        /// </summary>
        public int MonoIsotopicPeakIndex { get; set; }

        public bool IsSaturated { get; set; }

        /// <summary>
        /// Return "1" if IsSaturated is true, otherwise return "0"
        /// </summary>
        public string IsSaturatedAsNumericText => IsSaturated ? "1" : "0";

        public int ChargeState { get; set; }

        /// <summary>
        /// The adjusted intensity of the isotopic profile. Currently used for correcting saturated profiles in IMS workflows
        /// </summary>
        public double IntensityAggregateAdjusted { get; set; }
        public double OriginalIntensity { get; set; }
        public double Score { get; set; }
        public int ScoreCountBasis { get; set; }
        public double MonoIsotopicMass { get; set; }
        public double MostAbundantIsotopeMass { get; set; }
        public float MonoPlusTwoAbundance { get; set; }
        public double AverageMass { get; set; }

        public double MonoPeakMZ { get; set; }

        public bool IsFlagged { get; set; }

        /// <summary>
        /// Intensity of the most abundant peak of the isotopic profile
        /// </summary>
        public float IntensityMostAbundant { get; set; }

        /// <summary>
        /// Intensity of the peak that relates to the most abundant peak of the theoretical profile
        /// </summary>
        public float IntensityMostAbundantTheor { get; set; }

        public int GetNumOfIsotopesInProfile()
        {
            return Peaklist.Count;
        }

        public double GetMZofMostAbundantPeak()
        {
            var mostIntensePeak = getMostIntensePeak();
            if (mostIntensePeak == null) return -1;
            return mostIntensePeak.XValue;
        }

        public double GetFWHM()
        {
            var mostIntensePeak = getMostIntensePeak();
            if (mostIntensePeak == null)
            {
                return -1;
            }
            return mostIntensePeak.Width;
        }

        public int GetIndexOfMostIntensePeak()
        {
            if (Peaklist == null || Peaklist.Count == 0) return -1;

            var indexOfMaxPeak = -1;
            float maxIntensity = 0;

            for (var i = 0; i < Peaklist.Count; i++)
            {
                if (Peaklist[i].Height > maxIntensity)
                {
                    maxIntensity = Peaklist[i].Height;
                    indexOfMaxPeak = i;
                }
            }
            return indexOfMaxPeak;
        }

        public MSPeak getMostIntensePeak()
        {
            if (Peaklist == null || Peaklist.Count == 0) return null;

            var maxPeak = new MSPeak(0);

            foreach (var peak in Peaklist)
            {
                if (peak.Height >= maxPeak.Height)
                {
                    maxPeak = peak;
                }
            }
            return maxPeak;
        }

        public double GetSignalToNoise()
        {
            var mostIntensePeak = getMostIntensePeak();
            if (mostIntensePeak == null)
            {
                return -1;
            }
            return mostIntensePeak.SignalToNoise;
        }

        public double GetMonoAbundance()
        {
            if (Peaklist == null || Peaklist.Count == 0) return 0;
            return Peaklist[0].Height;
        }

        public float GetMonoPlusTwoAbundance()
        {
            if (Peaklist == null || Peaklist.Count < 3) return 0;
            return Peaklist[2].Height;
        }

        public double GetMZ()
        {
            if (Peaklist == null || Peaklist.Count == 0) return -1;
            return Peaklist[0].XValue;
        }

        public double GetAbundance()
        {
            return getMostIntensePeak().Height;
        }

        public double GetScore()
        {
            return Score;
        }

        public MSPeak getMonoPeak()
        {
            if (Peaklist?.Count > 0)
            {
                return Peaklist[0];
            }

            return null;
        }

        public double GetSummedIntensity()
        {
            if (Peaklist == null) return -1;
            double summedIntensity = 0;
            foreach (var peak in Peaklist)
            {
                summedIntensity += peak.Height;
            }
            return summedIntensity;
        }

        public IsotopicProfile CloneIsotopicProfile()
        {
            var iso = new IsotopicProfile
            {
                AverageMass = AverageMass,
                ChargeState = ChargeState,
                IntensityMostAbundant = IntensityMostAbundant,
                IntensityMostAbundantTheor = IntensityMostAbundantTheor,
                MonoIsotopicMass = MonoIsotopicMass,
                MonoIsotopicPeakIndex = MonoIsotopicPeakIndex,
                MonoPeakMZ = MonoPeakMZ,
                MonoPlusTwoAbundance = MonoPlusTwoAbundance,
                MostAbundantIsotopeMass = MostAbundantIsotopeMass,
                IsSaturated = IsSaturated,
                OriginalIntensity = OriginalIntensity,
                Peaklist = new List<MSPeak>()
            };

            foreach (var msPeak in Peaklist)
            {
                var peak = new MSPeak(msPeak.XValue, msPeak.Height, msPeak.Width, msPeak.SignalToNoise);
                iso.Peaklist.Add(peak);
            }

            iso.Score = Score;
            iso.ScoreCountBasis = ScoreCountBasis;

            return iso;
        }

        public XYData GetTheoreticalIsotopicProfileXYData(double fwhm)
        {
            Check.Require(Peaklist?.Count > 0, "Cannot get theor isotopic profile. Input isotopic profile is empty.");

            var xyData = new XYData();
            var xVals = new List<double>();
            var yVals = new List<double>();

            if (Peaklist != null)
            {
                foreach (var item in Peaklist)
                {
                    var tempXYData = item.GetTheorPeakData(fwhm);
                    xVals.AddRange(tempXYData.Xvalues);
                    yVals.AddRange(tempXYData.Yvalues);
                }
            }

            xyData.Xvalues = xVals.ToArray();
            xyData.Yvalues = yVals.ToArray();

            return xyData;
        }

        public XYData GetTheoreticalIsotopicProfileXYData(double fwhm, double mzOffset)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return MonoPeakMZ.ToString("0.0000") + "; " + ChargeState + "; " + MonoIsotopicMass.ToString("0.0000");
        }
    }
}
