using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.ElementalFormulas;

// Compare MercuryIsotopeDistribution to DeconTools.Backend.Utilities.IsotopeDistributionCalculation.MercuryIsotopicDistribution.MercuryIsoDistCreator2
namespace DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.Mercury
{
    /// <summary>
    /// ApodizationType
    /// </summary>
    /// <seealso cref="DeconTools.Backend.Utilities.IsotopeDistributionCalculation.MercuryIsotopicDistribution.MercuryApodizationType"/>
    public enum ApodizationType
    {
        Unknown,
        Gaussian,
        Lorentzian
    }
    /*internal enum enmApodizationType
    {
        GAUSSIAN = 1,
        LORENTZIAN,
        UNGAUSSIAN
    }*/

    /// <summary>
    ///     Implementation of the MERCURY Isotope Distribution Generator
    /// </summary>
    /// <remarks>
    ///     MERCURY is an isotope distribution generator based on Fourier transform methods. The calculation is performed by
    ///     generating the transformed mass (or mu-domain) for the input molecule which is then inverse Fourier transformed
    ///     into the mass spectrum. The FFT routine is taken from Numerical Recipes in C, 2nd ed. by Press, Teukolsky,
    ///     Vetterling, and Flannery, Cambridge Univ. Press. This program can be used to swiftly calculate high resolution and
    ///     ultrahigh resolution (a mass defect spectrum of a single isotopic peak) distributions.   The program outputs an
    ///     ASCII file of mass intensity pairs. The output file does not account for the electron mass, but the interactive
    ///     display does. (We'll fix that in a later version.) When running an ultrahigh resolution calculation, do not use
    ///     zero charge or you will get bogus output. Also, when it asks for the mass shift in ultrahigh resolution mode, it
    ///     expects you to feed it a negative number. The high resolution calculations are very fast (typically a second on a
    ///     66 MHz '486), but ultrahigh resolution calculations are rather slow (several  minutes for a single isotope peak.)
    ///     Overall, the program is good, but could be simplified and also optimized to run several times faster.
    ///     Algorithm by: Alan L. Rockwood
    ///     Original Program by: Steven L. Van Orden
    /// </remarks>
    /// <seealso cref="DeconTools.Backend.Utilities.IsotopeDistributionCalculation.MercuryIsotopicDistribution.MercuryIsoDistCreator2"/>
    public class MercuryIsotopeDistribution
    {
        public const double Pi = 3.14159265358979323846;
        public const double TwoPi = 2 * 3.14159265358979323846;
        private readonly List<double> _intensityList = new List<double>();
        private readonly List<double> _mzList = new List<double>();
        private List<double> _frequencyData = new List<double>();
        private int _massRange;
        private double _maxMz;
        private double _minMz;
        public ApodizationType ApType;

        /// <summary>
        ///     average mass calculated for current peptide. It includes the charge specified for the species.
        /// </summary>
        public double AverageMw;

        public double ChargeCarrierMass;

        public ElementIsotopes ElementalIsotopeComposition = new ElementIsotopes();
        public double MaxPeakMz;
        public int MercurySize;

        /// <summary>
        ///     monoisotopic mass calculated for current peptide. It includes the charge specified for the species.
        /// </summary>
        public double MonoMw;

        public double MostIntenseMw;
        public int PointsPerAmu;

        public MercuryIsotopeDistribution()
        {
            ChargeCarrierMass = 1.00727638;
            ApType = ApodizationType.Gaussian;
            MercurySize = 8192;
        }

        public MercuryIsotopeDistribution(MercuryIsotopeDistribution mercDistribution)
        {
            ElementalIsotopeComposition = mercDistribution.ElementalIsotopeComposition;
            ChargeCarrierMass = mercDistribution.ChargeCarrierMass;
            ApType = mercDistribution.ApType;
            MercurySize = mercDistribution.MercurySize;
            PointsPerAmu = mercDistribution.PointsPerAmu;
        }

        public double MassVariance { get; private set; }

        /// <summary>
        ///     Calculate the isotope distribution
        /// </summary>
        /// <param name="charge"></param>
        /// <param name="resolution"></param>
        /// <param name="formula"></param>
        /// <param name="x">mz values of isotopic profile that are above the threshold</param>
        /// <param name="y">intensity values of isotopic profile that are above the threshold</param>
        /// <param name="threshold"></param>
        /// <param name="isotopeMzs">peak top mz's for the peaks of the isotopic profile</param>
        /// <param name="isotopeIntensities">peak top intensities's for the peaks of the isotopic profile</param>
        /// <param name="debug"></param>
        public void CalculateDistribution(short charge, double resolution, MolecularFormula formula,
            out List<double> x,
            out List<double> y, double threshold, out List<double> isotopeMzs, out List<double> isotopeIntensities,
            bool debug = false)
        {
            /* Calculate mono and average mass */
            CalculateMasses(formula);
            if (debug)
            {
                Console.Error.WriteLine("MonoMW =" + MonoMw + " AverageMW =" + AverageMw);
            }

            /* Calculate mass range to use based on molecular variance */
            CalcVariancesAndMassRange(charge, formula);
            if (debug)
            {
                Console.Error.WriteLine("Variance =" + MassVariance + " Mass Range =" + _massRange);
            }

            _minMz = AverageMw / charge + (ChargeCarrierMass - MercuryCache.ElectronMass) - _massRange * 1.0 / 2;
            _maxMz = _minMz + _massRange;
            PointsPerAmu = MercurySize / _massRange; /* Use maximum of 2048 real, 2048 imag points */

            // calculate Ap_subscript to from requested Res
            double aPSubscript;
            if (charge == 0)
                aPSubscript = AverageMw / resolution * MercurySize * 2.0 / _massRange;
            else
                aPSubscript = AverageMw / (resolution * Math.Abs(charge)) * MercurySize * 2.0 / _massRange;

            /* Allocate memory for Axis arrays */
            var numPoints = _massRange * PointsPerAmu;
            _frequencyData.Clear();
            _frequencyData.Capacity = 2 * numPoints + 1;
            _frequencyData.AddRange(Enumerable.Repeat(0d, 2 * numPoints + 1));

            if (charge == 0)
                charge = 1;

            if (debug)
            {
                Console.Error.WriteLine("MINMZ = " + _minMz + " MAXMZ = " + _maxMz);
                Console.Error.WriteLine("Num Points per AMU = " + PointsPerAmu);
            }

            CalcFrequencies(charge, numPoints, formula);

            /* Apodize data */
            double apResolution = 1; /* Resolution used in apodization. Not used yet */
            Apodize(numPoints, apResolution, aPSubscript);

            FFT.Four1(numPoints, ref _frequencyData, -1);
            // myers changes this line to Realft(FreqData,NumPoints,-1);

            /*
                [gord] 'OutputData' fills 'x', 'y'
                [gord] x = mz values of isotopic profile that are above the threshold
                [gord] y = intensity values of isotopic profile that are above the threshold
                [gord] isotopeMzs = peak top mz's for the peaks of the isotopic profile
                [gord] isotopeIntensities = peak top intensities's for the peaks of the isotopic profile
                */
            OutputData(numPoints, charge, out x, out y, threshold, out isotopeMzs, out isotopeIntensities);

            //NormalizeToPercentIons(vect_y);
        }

        public double NormalizeToPercentIons(List<double> y)
        {
            var numPoints = y.Count;

            var sum = 0.0;
            var lastDelta = 0;
            for (var i = 1; i < numPoints; i++)
            {
                if (Math.Abs(y[i]) >= Math.Abs(y[i - 1]))
                {
                    if (Math.Abs(y[i]) > Math.Abs(y[i - 1])) lastDelta = 1;
                }
                else
                {
                    if (lastDelta == 1)
                    {
                        sum += y[i - 1];
                    }
                    lastDelta = -1;
                }
            }
            for (var i = 0; i < numPoints; i++)
            {
                y[i] /= sum / 100.0;
            }
            return sum;
        }

        public void CalculateMasses(MolecularFormula formula)
        {
            MonoMw = 0;
            AverageMw = 0;
            var numElementsFound = formula.NumElements;
            for (var j = 0; j < numElementsFound; j++)
            {
                var elementIndex = formula.ElementalComposition[j].Index;
                var atomicity = (int) formula.ElementalComposition[j].NumCopies;

                if (atomicity == 0)
                    continue;
                //int numIsotopes = ElementalIsotopeComposition.ElementalIsotopesList[elementIndex].NumberOfIsotopes;
                var monoMw = ElementalIsotopeComposition.ElementalIsotopesList[elementIndex].Isotopes[0].Mass;
                var avgMw = ElementalIsotopeComposition.ElementalIsotopesList[elementIndex].AverageMass;
                MonoMw += atomicity * monoMw;
                AverageMw += atomicity * avgMw;
            }
        }

        public void CalcVariancesAndMassRange(short charge, MolecularFormula formula)
        {
            MassVariance = 0;
            var numElementsFound = formula.NumElements;
            for (var elementNum = 0; elementNum < numElementsFound; elementNum++)
            {
                var elementIndex = formula.ElementalComposition[elementNum].Index;
                var atomicity = (int) formula.ElementalComposition[elementNum].NumCopies;
                var elementalVariance = ElementalIsotopeComposition.ElementalIsotopesList[elementIndex].MassVariance;
                MassVariance += elementalVariance * atomicity;
            }

            if (charge == 0)
                _massRange = (int) (Math.Sqrt(1 + MassVariance) * 10);
            else
                _massRange = (int) (Math.Sqrt(1 + MassVariance) * 10.0 / charge);
            /* +/- 5 sd's : Multiply charged */

            /* Set to nearest (upper) power of 2 */
            for (var i = 1024; i > 0; i /= 2)
            {
                if (i < _massRange)
                {
                    _massRange = i * 2;
                    i = 0;
                }
            }
            if (_massRange <= 0)
                _massRange = 1;
        }

        /// <summary>
        ///     Apodize - called by main()
        /// </summary>
        /// <param name="numPoints"></param>
        /// <param name="resolution"></param>
        /// <param name="sub"></param>
        /// <remarks>
        ///     ApType = 1 : Gaussian
        ///     = 2 : Lorentzian
        ///     = 3 : ??
        ///     = -1 : Unapodize Gaussian
        /// </remarks>
        public void Apodize(int numPoints, double resolution, double sub)
        {
            int i;
            double apVal;

            switch (ApType)
            {
                case ApodizationType.Gaussian:
                    for (i = 1; i <= numPoints; i++)
                    {
                        var expDenom = numPoints / sub * (numPoints / sub);
                        if (i <= numPoints / 2)
                            apVal = Math.Exp(-(i - 1) * (i - 1) / expDenom);
                        else
                            apVal = Math.Exp(-(numPoints - i - 1) * (numPoints - i - 1) / expDenom);
                        
                        _frequencyData[2 * i - 1] *= apVal;
                        _frequencyData[2 * i] *= apVal;
                    }
                    break;
                case ApodizationType.Lorentzian: /* Lorentzian */
                    for (i = 1; i <= numPoints; i++)
                    {
                        if (i <= numPoints / 2)
                            apVal = Math.Exp(-(double) (i - 1) * (sub / 5000.0));
                        else
                            apVal = Math.Exp(-(double) (numPoints - i) * (sub / 5000.0));
                        _frequencyData[2 * i - 1] *= apVal;
                        _frequencyData[2 * i] *= apVal;
                    }
                    break;
                /* Never used - this was the only use of the enum value "UNGAUSSIAN"
                case enmApodizationType.UNGAUSSIAN: // Unapodize Gaussian
                    for (i = 1; i <= numPoints; i++)
                    {
                        expdenom = (numPoints / sub) * (numPoints / sub);
                        if (i <= numPoints / 2)
                            apVal = Math.Exp(-(i - 1) * (i - 1) / expdenom);
                        else
                            apVal = Math.Exp(-(numPoints - i - 1) * (numPoints - i - 1) / expdenom);
                        _frequencyData[2 * i - 1] /= apVal;
                        _frequencyData[2 * i] /= apVal;
                    }
                    break;
                */
            } /* End of Apodize() */
        }

        public void OutputData(int numPoints, int charge, out List<double> x, out List<double> y, double threshold,
            out List<double> isotopeMzs, out List<double> isotopeIntensities)
        {
            int i;
            double maxint = 0;

            /* Normalize intensity to 0%-100% scale */
            for (i = 1; i < 2 * numPoints; i += 2)
            {
                var intensity = _frequencyData[i];
                if (intensity > maxint)
                    maxint = intensity;
            }
            for (i = 1; i < 2 * numPoints; i += 2)
            {
                var intensity = _frequencyData[i];
                _frequencyData[i] = 100 * intensity / maxint;
            }

            _intensityList.Clear();
            _mzList.Clear();

            if (charge == 0)
                charge = 1;

            //[gord] fill mz and intensity arrays, ignoring minimum thresholds
            for (i = numPoints / 2 + 1; i <= numPoints; i++)
            {
                var mz = (double) (i - numPoints - 1) / PointsPerAmu + AverageMw / charge +
                         (ChargeCarrierMass - MercuryCache.ElectronMass);
                var intensity = _frequencyData[2 * i - 1];
                _mzList.Add(mz);
                _intensityList.Add(intensity);
            }
            for (i = 1; i <= numPoints / 2; i++)
            {
                var mz = (double) (i - 1) / PointsPerAmu + AverageMw / charge +
                         (ChargeCarrierMass - MercuryCache.ElectronMass);
                var intensity = _frequencyData[2 * i - 1];
                _mzList.Add(mz);
                _intensityList.Add(intensity);
            }

            var highestIntensity = -1 * double.MaxValue;
            x = new List<double>();
            y = new List<double>();
            isotopeMzs = new List<double>();
            isotopeIntensities = new List<double>();
            //var n1 = _intensityList.Count;

            double x1 = 0;
            double x2 = 0;
            double x3 = 0;
            double y1 = 0;
            double y2 = 0;
            double y3 = 0;

            var lastIntensity = double.MaxValue * -1;
            for (i = 0; i < numPoints; i++)
            {
                var intensity = _intensityList[i];
                var mz = _mzList[i];
                if (intensity > threshold)
                {
                    y.Add(intensity); //fill intensity array if above threshold
                    x.Add(mz); //fill mz array if above threshold
                    if (intensity >= lastIntensity) // intensities are increasing... (on the upslope of the theor peak)
                    {
                        double x1Iso;
                        double x2Iso;
                        double x3Iso;
                        double y1Iso;
                        double y2Iso;
                        double y3Iso;
                        if (i != numPoints - 1)
                        {
                            if (_intensityList[i + 1] < intensity) //  if true, 'intensity' marks the maximum
                            {
                                // now three points are defined (X1,Y1) (X2,Y2) and (X3,Y2), with the maximum being (X2,Y2)
                                if (i > 0)
                                {
                                    x1Iso = _mzList[i - 1];
                                    y1Iso = _intensityList[i - 1];
                                }
                                else
                                {
                                    x1Iso = mz - 1.0 / (charge * PointsPerAmu);
                                    y1Iso = 0;
                                }

                                y2Iso = intensity;
                                x2Iso = mz;

                                x3Iso = _mzList[i + 1];
                                y3Iso = _intensityList[i + 1];

                                //[gord] it seems that the top three points are not necessarily symmetrical.
                                // so must first test the symmetry
                                var symmetryRatioCalc1 = (y2Iso - y1Iso) * (x3Iso - x2Iso) -
                                                         (y3Iso - y2Iso) * (x2Iso - x1Iso);
                                if (symmetryRatioCalc1.Equals(0)) //symmetrical
                                    isotopeMzs.Add(x2Iso);
                                else
                                //not symmetrical...   gord:  I'm not sure how the center point is calculated... perhaps a midpoint calc?
                                    isotopeMzs.Add((x1Iso + x2Iso -
                                                    (y2Iso - y1Iso) * (x3Iso - x2Iso) * (x1Iso - x3Iso) /
                                                    ((y2Iso - y1Iso) * (x3Iso - x2Iso) -
                                                     (y3Iso - y2Iso) * (x2Iso - x1Iso))) / 2.0);
                                isotopeIntensities.Add(intensity);
                            }
                        }
                        else // if intensities are decreasing, fill the remaining points
                        {
                            x1Iso = _mzList[i - 1];
                            y1Iso = _intensityList[i - 1];
                            x2Iso = mz;
                            y2Iso = intensity;
                            x3Iso = mz + 1.0 / (charge * PointsPerAmu);
                            y3Iso = 0;

                            var dIso = (y2Iso - y1Iso) * (x3Iso - x2Iso) - (y3Iso - y2Iso) * (x2Iso - x1Iso);
                            if (dIso.Equals(0))
                                isotopeMzs.Add(x2Iso);
                            else
                                isotopeMzs.Add((x1Iso + x2Iso -
                                                (y2Iso - y1Iso) * (x3Iso - x2Iso) * (x1Iso - x3Iso) /
                                                ((y2Iso - y1Iso) * (x3Iso - x2Iso) - (y3Iso - y2Iso) * (x2Iso - x1Iso))) /
                                               2.0);
                            isotopeIntensities.Add(intensity);
                        }
                    }
                }
                lastIntensity = intensity;
                if (intensity > highestIntensity)
                    //[gord] this is used in determining the max of the entire theor isotopic profile
                {
                    highestIntensity = intensity;

                    if (i > 0)
                    {
                        x1 = _mzList[i - 1];
                        y1 = _intensityList[i - 1];
                    }
                    else
                    {
                        x1 = mz - 1.0 / (charge * PointsPerAmu);
                        y1 = 0;
                    }

                    y2 = intensity;
                    x2 = mz;

                    if (i < numPoints - 1)
                    {
                        x3 = _mzList[i + 1];
                        y3 = _intensityList[i + 1];
                    }
                    else
                    {
                        x3 = mz + 1.0 / (charge * PointsPerAmu);
                        y3 = y2;
                    }
                }
            } //end of loop over all points

            //[gord] determine if three points are symmetrical around the max point (X2,Y2)
            var symmetryRatioCalc = (y2 - y1) * (x3 - x2) - (y3 - y2) * (x2 - x1);
            if (symmetryRatioCalc.Equals(0)) //symmetrical
                MaxPeakMz = x2;
            else //not symmetrical
                MaxPeakMz = (x1 + x2 -
                             (y2 - y1) * (x3 - x2) * (x1 - x3) / ((y2 - y1) * (x3 - x2) - (y3 - y2) * (x2 - x1))) / 2.0;
            // remember that the mono isotopic mass is calculated using theoretical values rather than
            // fit with the fourier transformed point. Hence even if the monoisotopic mass is the most intense
            // its value might not match exactly with mdbl_most_intense_mw, hence check if they are the same
            // and set it.
            MostIntenseMw = MaxPeakMz * charge - ChargeCarrierMass * charge + MercuryCache.ElectronMass * charge;
            if (Math.Abs(MostIntenseMw - MonoMw) < 0.5 * 1.003 / charge)
                MostIntenseMw = MonoMw;
        } /* End of OutputData() */

        public bool FindPeak(double minMz, double maxMz, out double mzValue, out double intensity)
        {
            var maxIndex = -1;
            mzValue = 0;
            intensity = 0;

            for (var i = 0; i < MercurySize; i++)
            {
                var mzLocal = _minMz + i * 1.0 / PointsPerAmu;
                if (mzLocal > _maxMz || mzLocal > maxMz)
                    break;
                if (mzLocal > _minMz && mzLocal > minMz)
                {
                    if (_intensityList[i] > intensity)
                    {
                        maxIndex = i;
                        intensity = _intensityList[i];
                    }
                }
            }

            if (maxIndex == -1)
                return false;

            var x1 = _minMz + (maxIndex - 1) * 1.0 / PointsPerAmu;
            var x2 = x1 + 1.0 / PointsPerAmu;
            var x3 = x2 + 1.0 / PointsPerAmu;
            var y1 = _intensityList[maxIndex - 1];
            var y2 = _intensityList[maxIndex];
            var y3 = _intensityList[maxIndex + 1];

            var d = (y2 - y1) * (x3 - x2) - (y3 - y2) * (x2 - x1);

            if (d.Equals(0))
                mzValue = x2;
            else
                mzValue = (x1 + x2 - (y2 - y1) * (x3 - x2) * (x1 - x3) / d) / 2.0;
            return true;
        }

        public void CalcFrequencies(short charge, int numPoints, MolecularFormula formula)
        {
            int i;
            int j, k;
            double real, imag, freq, x, theta, r, tempr;

            var numElementsInEntity = formula.NumElements;
            /* Calculate first half of Frequency Domain (+)masses */
            for (i = 1; i <= numPoints / 2; i++)
            {
                freq = (double) (i - 1) / _massRange;
                r = 1;
                theta = 0;
                for (j = 0; j < numElementsInEntity; j++)
                {
                    var elementIndex = formula.ElementalComposition[j].Index;
                    var atomicity = (int) formula.ElementalComposition[j].NumCopies;
                    var numIsotopes = ElementalIsotopeComposition.ElementalIsotopesList[elementIndex].NumberOfIsotopes;
                    var averageMass = ElementalIsotopeComposition.ElementalIsotopesList[elementIndex].AverageMass;
                    real = imag = 0.0;
                    for (k = 0; k < numIsotopes; k++)
                    {
                        double wrapFreq = 0;
                        var isotopeAbundance =
                            ElementalIsotopeComposition.ElementalIsotopesList[elementIndex].Isotopes[k].Probability;
                        if (numIsotopes > 1)
                        {
                            wrapFreq =
                                ElementalIsotopeComposition.ElementalIsotopesList[elementIndex].Isotopes[k].Mass /
                                charge - averageMass / charge;
                            if (wrapFreq < 0)
                                wrapFreq += _massRange;
                        }
                        x = 2 * Pi * wrapFreq * freq;
                        real += isotopeAbundance * Math.Cos(x);
                        imag += isotopeAbundance * Math.Sin(x);
                    }
                    /* Convert to polar coordinates, r then theta */
                    tempr = Math.Sqrt(real * real + imag * imag);
                    r *= Math.Pow(tempr, atomicity);
                    if (real > 0)
                        theta += atomicity * Math.Atan(imag / real);
                    else if (real < 0)
                        theta += atomicity * (Math.Atan(imag / real) + Pi);
                    else if (imag > 0)
                        theta += atomicity * Pi / 2;
                    else
                        theta += atomicity * -Pi / 2;
                }
                /* Convert back to real:imag coordinates and store */
                _frequencyData[2 * i - 1] = r * Math.Cos(theta); /* real data in odd index */
                _frequencyData[2 * i] = r * Math.Sin(theta); /* imag data in even index */
            } /* end for(i) */

            /* Calculate second half of Frequency Domain (-)masses */
            for (i = numPoints / 2 + 1; i <= numPoints; i++)
            {
                freq = (double) (i - numPoints - 1) / _massRange;
                r = 1;
                theta = 0;

                for (j = 0; j < numElementsInEntity; j++)
                {
                    var elementIndex = formula.ElementalComposition[j].Index;
                    var atomicity = (int) formula.ElementalComposition[j].NumCopies;
                    var numIsotopes = ElementalIsotopeComposition.ElementalIsotopesList[elementIndex].NumberOfIsotopes;
                    var averageMass = ElementalIsotopeComposition.ElementalIsotopesList[elementIndex].AverageMass;

                    real = imag = 0;
                    for (k = 0; k < numIsotopes; k++)
                    {
                        double wrapFreq = 0;
                        var isotopeAbundance =
                            ElementalIsotopeComposition.ElementalIsotopesList[elementIndex].Isotopes[k].Probability;
                        if (numIsotopes > 1)
                        {
                            wrapFreq =
                                ElementalIsotopeComposition.ElementalIsotopesList[elementIndex].Isotopes[k].Mass /
                                charge - averageMass / charge;
                            if (wrapFreq < 0)
                                wrapFreq += _massRange;
                        }

                        x = 2 * Pi * wrapFreq * freq;
                        real += isotopeAbundance * Math.Cos(x);
                        imag += isotopeAbundance * Math.Sin(x);
                    }

                    /* Convert to polar coordinates, r then theta */
                    tempr = Math.Sqrt(real * real + imag * imag);
                    r *= Math.Pow(tempr, atomicity);
                    if (real > 0)
                    {
                        theta += atomicity * Math.Atan(imag / real);
                    }
                    else if (real < 0)
                    {
                        theta += atomicity * (Math.Atan(imag / real) + Pi);
                    }
                    else if (imag > 0)
                    {
                        theta += atomicity * Pi / 2;
                    }
                    else
                    {
                        theta -= atomicity * Pi / 2;
                    }
                } /* end for(j) */

                /* Convert back to real:imag coordinates and store */
                _frequencyData[2 * i - 1] = r * Math.Cos(theta); /* real data in even index */
                _frequencyData[2 * i] = r * Math.Sin(theta); /* imag data in odd index */
            } /* end of for(i) */
        }

        public void SetElementalIsotopeComposition(ElementIsotopes isoComp)
        {
            ElementalIsotopeComposition = isoComp;
        }
    }
}