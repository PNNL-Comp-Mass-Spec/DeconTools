using System;
using System.Collections.Generic;

namespace DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.Mercury
{
    internal class MercuryCache
    {
        public const double ElectronMass = 0.00054858;

        private const double GaussianProfileFactorValue = 1.86;

        private readonly SortedDictionary<int, List<int>> _cachedIsotopeDistValues =
            new SortedDictionary<int, List<int>>();

        private readonly List<IsotopicDistribution> _isotopicDistributions = new List<IsotopicDistribution>();
        private readonly bool _useChargeStateInCaching;

        public double AverageMw;
        public double MaxPeakMz;
        public double MonoMw;
        public double MostIntenseMw;

        public MercuryCache()
        {
            MaxPeakMz = 0;
            AverageMw = 0;
            MercurySize = 8192;
            _useChargeStateInCaching = true;
        }

        public void GetPeakAndIntensity(double mz1, double mz2, double i1, double i2, double halfFwhm, out double mz,
            out double i)
        {
            var logInv = 1 / Math.Log(GaussianProfileFactorValue);
            mz = 0.5 * (mz1 * mz1 - mz2 * mz2 - halfFwhm * halfFwhm * Math.Log(i2 / i1) * logInv) / (mz1 - mz2);
            i = i1 * Math.Pow(GaussianProfileFactorValue, (mz1 - mz) * (mz1 - mz) / (halfFwhm * halfFwhm));
            //  i = i1 * Math.Pow(2, (mz1-mz) * (mz1-mz)/ (halfFwhm*halfFwhm));
        }

        public void GetPeakTops(List<double> mzs, List<double> intensities, out List<double> peakTopMzs,
            out List<double> peakTopIntensities, List<int> isotopeIndices, double halfFwhm, double mzSpacing)
        {
            peakTopMzs = new List<double>();
            peakTopIntensities = new List<double>();
            // Now go through each isotope, figure out its peak top and the intensity at the peak top.
            // remember that this is nothing but a sampling issue. The peak tops might not be sampled.
            foreach (var index in isotopeIndices)
            {
                // for really high masses, some of the first few isotopes will not be present at all.
                if (index == -1)
                    continue;

                var mz1 = mzs[index];
                var int1 = intensities[index];
                double mz2, int2;

                // it might be the case that our peak has only one point and there is nothing before and after it,
                // so we cannot get a good peak top estimate for it. If this is the case use, its mz and intensity,
                // without trying to adjust.
                if (index < isotopeIndices.Count - 1)
                {
                    mz2 = mzs[index + 1];
                    int2 = intensities[index + 1];
                }
                else
                {
                    mz2 = mzs[index - 1];
                    int2 = intensities[index - 1];
                }

                double mz;
                double intensity;
                if (int2 > 0 && (mz1 - mz2 < 2 * mzSpacing || (mz2 - mz1 < 2 * mzSpacing)))
                {
                    GetPeakAndIntensity(mz1, mz2, int1, int2, halfFwhm, out mz, out intensity);
                    // also check for overflows etc by making sure the value didn't explode by a factor greater than 1.5.
                    if (intensity > int1 * 1.5)
                    {
                        mz = mz1;
                        intensity = int1;
                    }
                }
                else
                {
                    mz = mz1;
                    intensity = int1;
                }

                peakTopMzs.Add(mz);
                peakTopIntensities.Add(intensity);
            }
        }

        public void ExtractIsotopeIndices(List<double> mzs, List<double> intensities, out List<int> peakIndex,
            double mzSpacing, double threshold)
        {
            peakIndex = new List<int>();
            if (mzs.Count == 0)
            {
                return;
            }
            if (mzs.Count == 1)
            {
                peakIndex.Add(0);
                return;
            }

            // Find peaks and return their indices. In doing so, however, recognize that the input might be a bunch of
            // separated peaks. So if there is a peak with just point point in it, there will be a discontinuous stretch on the x axis.
            // estimate that by using two times the expected mz spacing.
            if (mzs[1] - mzs[0] > 2 * mzSpacing && intensities[0] > threshold)
                peakIndex.Add(0);

            var currentMz = mzs[0];
            var currentIntensity = intensities[0];
            var nextMz = mzs[1];
            var nextIntensity = intensities[1];
            var nextMzDiff = nextMz - currentMz;

            for (var index = 1; index < mzs.Count - 1; index++)
            {
                var previousIntensity = currentIntensity;
                currentIntensity = nextIntensity;
                nextIntensity = intensities[index + 1];

                //var previousMz = currentMz;
                currentMz = nextMz;
                nextMz = mzs[index + 1];

                var previousMzDiff = nextMzDiff;
                nextMzDiff = nextMz - currentMz;
                if (currentIntensity >= previousIntensity && currentIntensity > nextIntensity)
                {
                    if (currentIntensity > threshold)
                        peakIndex.Add(index);
                }
                else if (previousMzDiff > 2 * mzSpacing && nextMzDiff > 2 * mzSpacing)
                {
                    if (currentIntensity > threshold)
                        peakIndex.Add(index);
                }
            }

            // now check if the last guy is a peak.
            if (nextMzDiff > 2 * mzSpacing && nextIntensity > threshold)
            {
                peakIndex.Add(mzs.Count - 1);
            }
        }

        public int MercurySize { get; set; }

        /// <summary>
        ///     the observed most abundance mass is what the map will be created from. This comes from the spectrum. The most
        ///     abundant mass, etc comes from the values generated by mercury.
        /// </summary>
        /// <param name="observedMostAbundantMass"></param>
        /// <param name="mostAbundantMass"></param>
        /// <param name="monoMass"></param>
        /// <param name="averageMass"></param>
        /// <param name="mostAbundantMz"></param>
        /// <param name="charge"></param>
        /// <param name="fwhm"></param>
        /// <param name="massVariance"></param>
        /// <param name="numPtsPerAmu"></param>
        /// <param name="minIntensity"></param>
        /// <param name="mzs">Isotope m/zs</param>
        /// <param name="intensities">Isotope intensities</param>
        public void CacheIsotopeDistribution(double observedMostAbundantMass, double mostAbundantMass, double monoMass,
            double averageMass, double mostAbundantMz, int charge, double fwhm, double massVariance, int numPtsPerAmu,
            double minIntensity, List<double> mzs, List<double> intensities)
        {
            // we're going to extract the peak tops for this distribution and add them to the array of distributions.
            var position = _isotopicDistributions.Count;
            var key = (int) (observedMostAbundantMass + 0.5);
            if (_cachedIsotopeDistValues.ContainsKey(key))
            {
                _cachedIsotopeDistValues[key].Add(position);
            }
            else
            {
                _cachedIsotopeDistValues.Add(key, new List<int> {position});
            }
            //var amuPerValue = 1.0 / num_pts_per_amu;
            var dist = new IsotopicDistribution
            {
                Charge = charge,
                MostAbundantMass = mostAbundantMass,
                MonoMass = monoMass,
                AverageMass = averageMass,
                MassVariance = massVariance,
                MostAbundantMz = mostAbundantMz,
                NumIsotopes = mzs.Count
            };
            dist.Mzs.AddRange(mzs);
            dist.Intensities.AddRange(intensities);
            _isotopicDistributions.Add(dist);
        }

        private int GetIsotopeDistributionCachedAtPosition(int position, int charge, double fwhm,
            double minTheoreticalIntensity, out List<double> mzs, out List<double> intensities)
        {
            mzs = new List<double>();
            intensities = new List<double>();
            // in the memory location we will save in the following order:
            // charge, most abundant mw, monoisotopic mw, average mw, num_pts, mz1, int1, mz2, int2 ..
            // use the FWHM that was passed.
            var halfFwhm = fwhm / 2;
            MostIntenseMw = _isotopicDistributions[position].MostAbundantMass;
            MonoMw = _isotopicDistributions[position].MonoMass;
            AverageMw = _isotopicDistributions[position].AverageMass;
            var massVariance = _isotopicDistributions[position].MassVariance;
            MaxPeakMz = _isotopicDistributions[position].MostAbundantMz;
            var numIsotopes = _isotopicDistributions[position].NumIsotopes;

            var massRange = (int) (Math.Sqrt(1 + massVariance) * 10.0 / charge);
            /* +/- 5 standard deviations : Multiply charged */
            /* Set to nearest (upper) power of 2 */
            for (var i = 1024; i > 0; i /= 2)
            {
                if (i < massRange)
                {
                    massRange = i * 2;
                    i = 0; // break out of loop
                }
            }
            if (massRange <= 0)
                massRange = 1;
            var pointsPerAmu = MercurySize / massRange; /* Use maximum of 2048 real, 2048 imaginary points */
            var amuPerPoint = 1.0 / pointsPerAmu;

            var numPts = 0;
            for (var i = 0; i < MercurySize; i++)
            {
                var mz = amuPerPoint * i;
                var ratio = Math.Pow(GaussianProfileFactorValue, -1 * mz * mz / (halfFwhm * halfFwhm));
                if (ratio < minTheoreticalIntensity / 100)
                    break;
                numPts++;
            }

            mzs.Capacity = numIsotopes * 2 * numPts;
            intensities.Capacity = numIsotopes * 2 * numPts;

            var previousIntersected = false;
            var previousIntersectMz = double.MaxValue;

            for (var isotopeNum = 0; isotopeNum < numIsotopes; isotopeNum++)
            {
                var mzIso = _isotopicDistributions[position].Mzs[isotopeNum];
                var intensityIso = _isotopicDistributions[position].Intensities[isotopeNum];

                var intersectMz = double.MaxValue;
                if (isotopeNum < numIsotopes - 1)
                {
                    var nextMz = _isotopicDistributions[position].Mzs[isotopeNum + 1];
                    var nextIntensity = _isotopicDistributions[position].Intensities[isotopeNum + 1];

                    if (nextMz - numPts * amuPerPoint < mzIso + numPts * amuPerPoint)
                    {
                        //intersecting, possibly. See what the m/z is till which current guy should go.
                        intersectMz = (mzIso + nextMz) / 2.0 -
                                      halfFwhm * halfFwhm * Math.Log(nextIntensity / intensityIso) /
                                      (2 * (nextMz - mzIso) * Math.Log(GaussianProfileFactorValue));
                    }
                }
                var i = -numPts;
                if (previousIntersected)
                {
                    var mzPt = mzIso + i * amuPerPoint;
                    while (mzPt < previousIntersectMz && i <= numPts)
                    {
                        i++;
                        mzPt += amuPerPoint;
                    }
                    previousIntersected = false;
                }

                for (; i <= numPts; i++)
                {
                    var mzDiff = i * amuPerPoint;
                    var mzPt = mzIso + mzDiff;

                    if (mzPt > intersectMz)
                    {
                        previousIntersected = true;
                        previousIntersectMz = intersectMz;
                        break;
                    }
                    var ratio = Math.Pow(GaussianProfileFactorValue, -1 * (mzDiff / halfFwhm) * (mzDiff / halfFwhm));
                    var intensityCalc = intensityIso * ratio;
                    if (intensityCalc > minTheoreticalIntensity)
                    {
                        mzs.Add(mzPt);
                        intensities.Add(intensityCalc);
                    }
                }
            }
            return 0;
        }

        public bool GetIsotopeDistributionCached(double observedMostAbundantMass, int charge, double fwhm,
            double minTheoreticalIntensity, out List<double> mzs, out List<double> intensities)
        {
            mzs = new List<double>();
            intensities = new List<double>();

            var found = false;
            var massVal = (int) (observedMostAbundantMass + 0.5);
            //[gord] masses are rounded off to an Int; so '0.5' is added to make sure rounding happens properly.
            var position = -1;

            //[gord]i think (!) the following advances to the mass_val and then if the chargeState
            //is used in caching then will find the correct chargeState and get its cached data
            if (_cachedIsotopeDistValues.ContainsKey(massVal))
            {
                var posList = _cachedIsotopeDistValues[massVal];
                foreach (var pos in posList)
                {
                    position = pos;
                    if (!_useChargeStateInCaching)
                    {
                        found = true;
                        break;
                    }
                    var currentCharge = _isotopicDistributions[position].Charge;
                    if (currentCharge == charge)
                    {
                        found = true;
                        break;
                    }
                }
            }
            if (found) //found the mass value in question; now get the data
            {
                GetIsotopeDistributionCachedAtPosition(position, charge, fwhm, minTheoreticalIntensity, out mzs,
                    out intensities);
                return true;
            }
            return false;
        }

        private class IsotopicDistribution
        {
            public readonly List<double> Intensities = new List<double>();
            public readonly List<double> Mzs = new List<double>();
            public double AverageMass;
            public int Charge;
            public double MassVariance;
            public double MonoMass;
            public double MostAbundantMass;
            public double MostAbundantMz;
            public int NumIsotopes;
        }
    }
}