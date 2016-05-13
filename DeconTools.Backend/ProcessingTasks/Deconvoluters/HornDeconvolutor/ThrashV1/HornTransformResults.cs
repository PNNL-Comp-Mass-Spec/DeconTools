using System.Collections.Generic;

namespace DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1
{
    /// <summary>
    ///     class to store results of isotope fitting/horn transform.
    /// </summary>
    public class HornTransformResults
    {
        private const int MaxIsotopes = 16;

        /// <summary>
        ///     intensity of feature (as a double)
        /// </summary>
        public double Abundance;

        /// <summary>
        ///     average mw for the feature.
        /// </summary>
        public double AverageMw;

        /// <summary>
        ///     charge state
        /// </summary>
        public int ChargeState;

        /// <summary>
        ///     difference between observed m/z and m/z from theoretical distribution of composition from Averagine
        /// </summary>
        public double DeltaMz;

        /// <summary>
        ///     fit value.
        /// </summary>
        public double Fit;

        /// <summary>
        ///     Number of data points used to compute the fit value
        /// </summary>
        public int FitCountBasis;

        /// <summary>
        ///     full width at half maximum of the peak.
        /// </summary>
        public double FWHM;

        /// <summary>
        ///     list of indices of peak tops
        /// </summary>
        public List<int> IsotopePeakIndices = new List<int>(MaxIsotopes);

        /// <summary>
        ///     intensity of monoisotopic peak observed.
        /// </summary>
        public int MonoIntensity;

        /// <summary>
        ///     monoisotopic mw of feature.
        /// </summary>
        public double MonoMw;

        /// <summary>
        ///     intensity of the third isotopic peak observed. Used by other software for processing of O16/O18  data.
        /// </summary>
        public int MonoPlus2Intensity;

        /// <summary>
        ///     mw at the most intense isotope.
        /// </summary>
        public double MostIntenseMw;

        /// <summary>
        ///     m/z value of most abundant peak in the feature.
        /// </summary>
        public double Mz;

        /// <summary>
        ///     need multiple isotopes to determine charge
        /// </summary>
        public bool NeedMultipleIsotopes;

        /// <summary>
        ///     peak index of the peak.
        /// </summary>
        public int PeakIndex;

        /// <summary>
        ///     scan number of peak
        /// </summary>
        public int ScanNum;

        /// <summary>
        ///     signal to noise for the most intense isotopic peak.
        /// </summary>
        public double SignalToNoise;

        public HornTransformResults()
        {
            Abundance = 0;
            ChargeState = -1;
            Mz = 0;
            Fit = 1;
            AverageMw = 0;
            MonoMw = 0;
            MostIntenseMw = 0;
            FWHM = 0;
            SignalToNoise = 0;
            MonoIntensity = 0;
            MonoPlus2Intensity = 0;
            PeakIndex = -1;
            NeedMultipleIsotopes = false;
        }

        public HornTransformResults(HornTransformResults a)
        {
            PeakIndex = a.PeakIndex;
            ScanNum = a.ScanNum;
            ChargeState = a.ChargeState;
            //AbundanceInt = a.AbundanceInt;
            Abundance = a.Abundance;
            Mz = a.Mz;
            Fit = a.Fit;
            FitCountBasis = a.FitCountBasis;
            AverageMw = a.AverageMw;
            MonoMw = a.MonoMw;
            MostIntenseMw = a.MostIntenseMw;
            FWHM = a.FWHM;
            SignalToNoise = a.SignalToNoise;
            MonoIntensity = a.MonoIntensity;
            MonoPlus2Intensity = a.MonoPlus2Intensity;
            DeltaMz = a.DeltaMz;
            IsotopePeakIndices = new List<int>(a.IsotopePeakIndices);
        }

        /// <summary>
        ///     number of isotope peaks
        /// </summary>
        public int NumIsotopesObserved
        {
            get { return IsotopePeakIndices.Count; }
        }

        public virtual object Clone()
        {
            var result = new HornTransformResults();
            result.PeakIndex = PeakIndex;
            result.ScanNum = ScanNum;
            result.ChargeState = ChargeState;
            //result.AbundanceInt = this.AbundanceInt;
            result.Abundance = Abundance;
            result.Mz = Mz;
            result.Fit = Fit;
            result.FitCountBasis = FitCountBasis;
            result.AverageMw = AverageMw;
            result.MonoMw = MonoMw;
            result.MostIntenseMw = MostIntenseMw;
            result.FWHM = FWHM;
            result.SignalToNoise = SignalToNoise;
            result.MonoIntensity = MonoIntensity;
            result.MonoPlus2Intensity = MonoPlus2Intensity;
            result.DeltaMz = DeltaMz;
            result.IsotopePeakIndices = new List<int>(IsotopePeakIndices);
            return result;
        }
    }
}