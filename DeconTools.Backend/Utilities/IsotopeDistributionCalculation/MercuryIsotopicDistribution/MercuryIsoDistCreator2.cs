using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using PNNLOmics.Data.Constants;
using Math = System.Math;

namespace DeconTools.Backend.Utilities.IsotopeDistributionCalculation.MercuryIsotopicDistribution
{
    public enum MercuryApodizationType
    {
        Gaussian,
        Lorentzian
    }

    public class MercuryIsoDistCreator2
    {
        private Dictionary<string, int> _elementTable;
        private int _mercuryArraySize = 8192;
        private double _massRange = 16;    //not sure what this is!
        private double _pointsPerAtomicMassUnit;

        private Dictionary<string, Element> _elementList = new Dictionary<string, Element>();

        #region Constructors

        public MercuryIsoDistCreator2()
        {
            ApodizationType = MercuryApodizationType.Gaussian;
            Resolution = 100000;
            _pointsPerAtomicMassUnit = _mercuryArraySize / _massRange;
        }

        #endregion

        #region Properties

        public MercuryApodizationType ApodizationType { get; set; }

        public double Resolution { get; set; }

        #endregion

        #region Public Methods

        public IsotopicProfile GetIsotopePattern(string empiricalFormula, int chargeState)
        {
            _elementTable = EmpiricalFormulaUtilities.ParseEmpiricalFormulaString(empiricalFormula);

            AddElementsToReferenceTable();

            var frequencyData = new double[2 * _mercuryArraySize + 1];

            GetMonoAndAverageMass(_elementTable, out var monoIsotopicMass, out var averageMass);

            CalculateFrequencies(_mercuryArraySize, chargeState, ref frequencyData);

            Apodize(ApodizationType, _mercuryArraySize, averageMass, chargeState, ref frequencyData);

            Realft(ref frequencyData);

            var intensityVals = new List<double>();
            var mzVals = new List<double>();

            for (var i = _mercuryArraySize / 2 + 1; i <= _mercuryArraySize; i++)
            {
                var mz = (i - _mercuryArraySize - 1) / _pointsPerAtomicMassUnit + monoIsotopicMass / chargeState + Globals.PROTON_MASS;
                var intensity = frequencyData[2 * i - 1];

                mzVals.Add(mz);
                intensityVals.Add(intensity);
            }

            for (var i = 1; i <= _mercuryArraySize / 2; i++)
            {
                var mz = (i - 1) / _pointsPerAtomicMassUnit + monoIsotopicMass / chargeState + Globals.PROTON_MASS;
                var intensity = frequencyData[2 * i - 1];

                mzVals.Add(mz);
                intensityVals.Add(intensity);
            }

            //StringBuilder sb = new StringBuilder();
            //for (int i = 0; i < intensityVals.Count; i++)
            //{
            //    sb.Append(mzVals[i] + "\t" + intensityVals[i] + "\n");
            //}

            //Console.WriteLine(sb.ToString());

            //Console.WriteLine();
            //Console.WriteLine("Monomass= " + monoIsotopicMass.ToString("0.000000"));
            //Console.WriteLine("monoMZ= " + (monoIsotopicMass/chargeState + Globals.PROTON_MASS));
            //Console.WriteLine("Average mass= " + averageMass.ToString("0.000000"));

            return null;
        }

        private void AddElementsToReferenceTable()
        {
            foreach (var i in _elementTable)
            {
                if (!_elementList.ContainsKey(i.Key))
                {
                    _elementList.Add(i.Key, Constants.Elements[i.Key]);
                }
            }
        }

        private void DisplayFrequencyDataToConsole(double[] frequencyData)
        {
            var sb = new StringBuilder();

            for (var j = 1; j <= _mercuryArraySize; j++)
            {
                var index1 = 2 * j - 1;
                var index2 = 2 * j;
                sb.Append(frequencyData[index1]);
                sb.Append("\t");
                sb.Append(frequencyData[index2]);
                sb.Append(Environment.NewLine);
            }

            Console.WriteLine(sb.ToString());
        }

        private void Apodize(MercuryApodizationType apodizationType, int numPoints, double averageMass, int chargeState, ref double[] frequencyData)
        {
            double apodizationVal;
            double expDenominator;

            var apodizationSubscript = averageMass / (Resolution * Math.Abs(chargeState)) * _mercuryArraySize * 2 / _massRange;
            expDenominator = numPoints / apodizationSubscript * numPoints / apodizationSubscript;

            switch (apodizationType)
            {
                case MercuryApodizationType.Gaussian:

                    //TODO: make this zero-based (I copied what was done in DeconEngine - which was sloppy)
                    for (var j = 1; j <= numPoints; j++)
                    {
                        if (j <= numPoints / 2)
                        {
                            apodizationVal = Math.Exp(-(j - 1) * (j - 1) / expDenominator);
                        }
                        else
                        {
                            apodizationVal = Math.Exp(-(numPoints - j - 1) * (numPoints - j - 1) / expDenominator);
                        }

                        var index1 = 2 * j - 1;
                        var index2 = 2 * j;

                        frequencyData[index1] = frequencyData[index1] * apodizationVal;
                        frequencyData[index2] = frequencyData[index2] * apodizationVal;
                        // Console.WriteLine(frequencyData[index1] + "\t"+frequencyData[index2]);
                    }

                    break;
                case MercuryApodizationType.Lorentzian:

                    //TODO: make the loop zero-based
                    for (var j = 1; j <= numPoints; j++)
                    {
                        if (j <= numPoints / 2)
                        {
                            apodizationVal = Math.Exp(-(double)(j - 1) * apodizationSubscript / 5000d);
                        }
                        else
                        {
                            apodizationVal = Math.Exp(-(double)(numPoints - j) * (apodizationSubscript / 5000d));
                        }

                        var index1 = 2 * j - 1;
                        var index2 = 2 * j;

                        frequencyData[index1] = frequencyData[index1] * apodizationVal;
                        frequencyData[index2] = frequencyData[index2] * apodizationVal;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(apodizationType));
            }
        }

        private int Realft(ref double[] data)
        {
            var isign = 1;
            var n = data.Length;

            int i, i1, i2, i3, i4, n2p3;
            double c1 = 0.5, c2, hir, h1i, h2r, h2i;
            double wpr, wpi, wi, wr, theta, wtemp;

            n = n / 2;
            theta = 3.141592653589793 / (double)n;
            if (isign == 1)
            {
                c2 = -0.5f;
                FourierTransform(n, ref data, 1);
            }
            else
            {
                c2 = 0.5f;
                theta = -theta;
            }
            wtemp = Math.Sin(0.5 * theta);
            wpr = -2.0 * wtemp * wtemp;
            wpi = Math.Sin(theta);
            wr = 1.0 + wpr;
            wi = wpi;
            n2p3 = 2 * n + 3;
            for (i = 2; i <= n / 2; i++)
            {
                i4 = 1 + (i3 = n2p3 - (i2 = 1 + (i1 = i + i - 1)));
                hir = c1 * (data[i1 - 1] + data[i3 - 1]);
                h1i = c1 * (data[i2 - 1] - data[i4 - 1]);
                h2r = -c2 * (data[i2 - 1] + data[i4 - 1]);
                h2i = c2 * (data[i1 - 1] - data[i3 - 1]);
                data[i1 - 1] = (hir + wr * h2r - wi * h2i);
                data[i2 - 1] = (h1i + wr * h2i + wi * h2r);
                data[i3 - 1] = (hir - wr * h2r + wi * h2i);
                data[i4 - 1] = (-h1i + wr * h2i + wi * h2r);
                wr = (wtemp = wr) * wpr - wi * wpi + wr;
                wi = wi * wpr + wtemp * wpi + wi;
            }
            if (isign == 1)
            {
                data[0] = (hir = data[0]) + data[1];
                data[1] = hir - data[1];
                //		for(i=0;i<(n*2);i++) data[i] /= (n);  // GAA 50-30-00
            }
            else
            {
            }
            return 0;
        }

        void FourierTransform(int nn, ref double[] data, int isign)
        {
            long m;
            long i;
            double wr, wpr, wpi, wi, theta;

            long n = nn << 1;
            long j = 1;
            for (i = 1; i < n; i += 2)
            {
                if (j > i)
                {
                    SwapValuesInArray(ref data, i - 1, j - 1);
                    SwapValuesInArray(ref data, i, j);
                }
                m = n >> 1;
                while (m >= 2 && j > m)
                {
                    j -= m;
                    m >>= 1;
                }
                j += m;
            }

            long mmax = 2;
            while (n > mmax)
            {
                var istep = 2 * mmax;
                theta = 6.28318530717959 / (isign * mmax);
                var wtemp = Math.Sin(0.5 * theta);
                wpr = -2.0 * wtemp * wtemp;
                wpi = Math.Sin(theta);
                wr = 1.0;
                wi = 0.0;
                for (m = 1; m < mmax; m += 2)
                {
                    for (i = m; i <= n; i += istep)
                    {
                        j = i + mmax;
                        var jm1 = j - 1;
                        var im1 = i - 1;
                        var tempr = (wr * data[jm1] - wi * data[j]);
                        var tempi = (wr * data[j] + wi * data[jm1]);
                        data[jm1] = (data[im1] - tempr);
                        data[j] = (data[i] - tempi);
                        data[im1] += tempr;
                        data[i] += tempi;
                    }
                    wr = (wtemp = wr) * wpr - wi * wpi + wr;
                    wi = wi * wpr + wtemp * wpi + wi;
                }
                mmax = istep;
            }
        }

        private void SwapValuesInArray(ref double[] data, long i, long j)
        {
            var tempVal = data[j];
            data[j] = data[i];
            data[i] = tempVal;
        }

        private void CalculateFrequencies(int numPoints, int chargeState, ref double[] frequencyData)
        {
            for (var i = 1; i < numPoints / 2; i++)
            {
                var freq = (i - 1) / _massRange;
                GetRealAndImagValues(freq, out var realVal, out var imagVal, chargeState);

                frequencyData[2 * i - 1] = realVal;
                frequencyData[2 * i] = imagVal;
            }

            for (var i = numPoints / 2 + 1; i <= numPoints; i++)
            {
                var freq = (i - numPoints - 1) / _massRange;
                GetRealAndImagValues(freq, out var realVal, out var imagVal, chargeState);
                frequencyData[2 * i - 1] = realVal;
                frequencyData[2 * i] = imagVal;
            }
        }

        #endregion

        #region Private Methods
        private void GetMonoAndAverageMass(Dictionary<string, int> elementTable, out double monoIsotopicMass, out double averageMass)
        {
            averageMass = 0;
            monoIsotopicMass = 0;

            foreach (var element in elementTable)
            {
                var elementSymbol = element.Key;
                var elementCount = element.Value;

                var elementObject = _elementList[elementSymbol];

                averageMass += elementObject.MassAverage * elementCount;
                monoIsotopicMass += elementObject.MassMonoIsotopic * elementCount;
            }
        }

        private void GetRealAndImagValues(double freq, out double realVal, out double imagVal, int chargeState)
        {
            double r = 1;
            double theta = 0;

            foreach (var element in _elementTable)
            {
                var numAtoms = element.Value;

                var elementObject = _elementList[element.Key];

                var numIsotopes = elementObject.IsotopeDictionary.Count;

                var averageMass = elementObject.MassAverage;

                double real = 0;
                double imag = 0;

                var isotopes = elementObject.IsotopeDictionary.Values;

                foreach (var isotope in isotopes)
                {
                    double wrapFrequency = 0;
                    if (numIsotopes > 1)
                    {
                        wrapFrequency = isotope.Mass / chargeState - averageMass / chargeState;
                        if (wrapFrequency < 0)
                        {
                            wrapFrequency += _massRange;
                        }
                    }

                    var x = 2 * Math.PI * wrapFrequency * freq;

                    real += isotope.NaturalAbundance * Math.Cos(x);
                    imag += isotope.NaturalAbundance * Math.Sin(x);
                }

                var tempR = Math.Sqrt(real * real + imag * imag);
                r *= Math.Pow(tempR, numAtoms);

                if (real > 0)
                {
                    theta += numAtoms * Math.Atan(imag / real);
                }
                else if (real < 0)
                {
                    theta += numAtoms * (Math.Atan(imag / real) + Math.PI);
                }
                else if (imag > 0)
                {
                    theta += numAtoms * Math.PI / 2;
                }
                else
                {
                    theta += numAtoms * -Math.PI / 2;
                }
            }

            realVal = r * Math.Cos(theta);
            imagVal = r * Math.Sin(theta);
        }

        #endregion

    }
}
