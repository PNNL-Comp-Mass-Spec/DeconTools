using System;
using System.Collections.Generic;

namespace DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.PeakProcessing
{
    /// <summary>
    /// enumeration for type of fit.
    /// </summary>
    internal enum PeakFitType
    {
        /// <summary>
        /// The peak is the m/z value higher than the points before and after it
        /// </summary>
        Apex = 0,

        /// <summary>
        /// The peak is the m/z value which is a quadratic fit of the three points around the apex
        /// </summary>
        Quadratic,

        /// <summary>
        /// The peak is the m/z value which is a lorentzian fit of the three points around the apex
        /// </summary>
        Lorentzian
    }

    /// <summary>
    /// Used for detecting peaks in the data.
    /// </summary>
    /// <seealso cref="DeconTools.Backend.ProcessingTasks.FitScoreCalculators.PeakLeastSquaresFitter"/> TODO: Verify this
    internal class PeakFitter
    {
        // member variable to find out information about peaks such as signal to noise and full width at half maximum.
        private readonly PeakStatistician _peakStatistician = new PeakStatistician();
        // Type of fit function used to find the peaks
        private PeakFitType _peakFitType;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>By default uses Quadratic fit.</remarks>
        public PeakFitter()
        {
            _peakFitType = PeakFitType.Quadratic;
        }

        /// <summary>
        /// Sets the type of fit.
        /// </summary>
        /// <param name="type">sets the type of fit function that this instance uses.</param>
        public void SetOptions(PeakFitType type)
        {
            _peakFitType = type;
        }

        /// <summary>
        /// Gets the peak that fits the point at a given index by the specified peak fit function.
        /// </summary>
        /// <param name="index">index of the point in the m/z vectors which is the apex of the peak.</param>
        /// <param name="mzs">List of raw data of m\zs.</param>
        /// <param name="intensities">List of raw data of intensities.</param>
        /// <returns>returns the m/z of the peak.</returns>
        public double Fit(int index, List<double> mzs, List<double> intensities)
        {
            if (_peakFitType == PeakFitType.Apex)
            {
                return mzs[index];
            }

            if (_peakFitType == PeakFitType.Quadratic)
            {
                return QuadraticFit(mzs, intensities, index);
            }

            if (_peakFitType == PeakFitType.Lorentzian)
            {
                var fwhm = _peakStatistician.FindFwhm(mzs, intensities, index);
                if (!fwhm.Equals(0))
                {
                    return LorentzianFit(mzs, intensities, index, fwhm);
                }

                return mzs[index];
            }
            return 0.0;
        }

        /// <summary>
        /// Gets the peak that fits the point at a given index with a quadratic fit.
        /// </summary>
        /// <param name="index">index of the point in the m/z vectors which is the apex of the peak.</param>
        /// <param name="mzs">List of raw data of m\zs.</param>
        /// <param name="intensities">List of raw data of intensities.</param>
        /// <returns>returns the m/z of the peak.</returns>
        private double QuadraticFit(IReadOnlyList<double> mzs, IReadOnlyList<double> intensities, int index)
        {
            if (index < 1)
            {
                return mzs[0];
            }

            if (index >= mzs.Count - 1)
            {
                return mzs[mzs.Count - 1];
            }

            var x1 = mzs[index - 1];
            var x2 = mzs[index];
            var x3 = mzs[index + 1];
            var y1 = intensities[index - 1];
            var y2 = intensities[index];
            var y3 = intensities[index + 1];

            var d = (y2 - y1) * (x3 - x2) - (y3 - y2) * (x2 - x1);
            if (d.Equals(0))
            {
                return x2; // no good.  Just return the known peak
            }

            d = (x1 + x2 - (y2 - y1) * (x3 - x2) * (x1 - x3) / d) / 2.0;
            return d; // Calculated new peak.  Return it.
        }

        /// <summary>
        /// Gets the peak that fits the point at a given index with a Lorentzian fit.
        /// </summary>
        /// <param name="index">index of the point in the m/z vectors which is the apex of the peak.</param>
        /// <param name="mzs">List of raw data of m\zs.</param>
        /// <param name="intensities">List of raw data of intensities.</param>
        /// <param name="fwhm"></param>
        /// <returns>returns the m/z of the peak.</returns>
        private double LorentzianFit(List<double> mzs, IReadOnlyList<double> intensities, int index, double fwhm)
        {
            var a = intensities[index];
            var vo = mzs[index];
            var e = Math.Abs((vo - mzs[index + 1]) / 100);

            if (index < 1)
            {
                return mzs[index];
            }

            if (index == mzs.Count)
            {
                return mzs[index];
            }

            var leftStart = PeakIndex.GetNearest(mzs, vo + fwhm, index) + 1;
            var leftStop = PeakIndex.GetNearest(mzs, vo - fwhm, index) - 1;

            var currentE = LorentzianLS(mzs, intensities, a, fwhm, vo, leftStart, leftStop);
            for (var i = 0; i < 50; i++)
            {
                var lastE = currentE;
                vo += e;
                currentE = LorentzianLS(mzs, intensities, a, fwhm, vo, leftStart, leftStop);
                if (currentE > lastE)
                {
                    break;
                }
            }

            vo -= e;
            currentE = LorentzianLS(mzs, intensities, a, fwhm, vo, leftStart, leftStop);
            for (var i = 0; i < 50; i++)
            {
                var lastE = currentE;
                vo -= e;
                currentE = LorentzianLS(mzs, intensities, a, fwhm, vo, leftStart, leftStop);
                if (currentE > lastE)
                {
                    break;
                }
            }
            vo += e;
            return vo;
        }

        /// <summary>
        /// Gets the peak that fits the point at a given index with a Lorentzian least square fit.
        /// </summary>
        /// <param name="mzs">List of raw data of m\zs.</param>
        /// <param name="intensities">List of raw data of intensities.</param>
        /// <param name="a"></param>
        /// <param name="fwhm"></param>
        /// <param name="vo"></param>
        /// <param name="leftStart"></param>
        /// <param name="leftStop"></param>
        /// <returns>returns the m/z of the fit peak.</returns>
        private double LorentzianLS(IReadOnlyList<double> mzs, IReadOnlyList<double> intensities, double a, double fwhm, double vo,
            int leftStart, int leftStop)
        {
            double rmsError = 0;
            for (var index = leftStart; index <= leftStop; index++)
            {
                var u = 2 / fwhm * (mzs[index] - vo);
                double y1 = (int)(a / (1 + u * u));
                var y2 = intensities[index];
                rmsError += (y1 - y2) * (y1 - y2);
            }
            return rmsError;
        }
    }
}