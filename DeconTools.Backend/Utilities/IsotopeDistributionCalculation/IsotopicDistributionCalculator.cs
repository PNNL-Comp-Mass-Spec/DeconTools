﻿using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using PNNLOmics.Data.Constants;

namespace DeconTools.Backend.Utilities.IsotopeDistributionCalculation
{
    /// <summary>
    /// Isotopic distribution calculator
    /// </summary>
    /// <remarks>
    /// Written by Josh Aldrich, Tom Taverner, and Gordon Slysz at PNNL
    /// </remarks>
    public sealed class IsotopicDistributionCalculator
    {
        #region Member Variables
        static IsotopicDistributionCalculator instance;
        static readonly object padlock = new object();
        double naturalAbundanceLight = -1;
        double naturalAbundanceHeavy = -1;
        string elementalSymbol = string.Empty;
        int lightIsotopeNum = -1;
        int heavyIsotopeNum = -1;
        private readonly PeptideUtils _peptideUtils = new PeptideUtils();

        #endregion

        #region Properties

        public double[] IonProbabilities { get; private set; }

        public bool IsSetToLabeled { get; private set; }

        #endregion

        #region Constructor
        IsotopicDistributionCalculator()
        {
        }
        #endregion

        #region Public Methods

        public static IsotopicDistributionCalculator Instance
        {
            get
            {
                lock (padlock)
                {
                    return instance ?? (instance = new IsotopicDistributionCalculator());
                }
            }
        }

        /// <summary>
        /// Assign the labeling and the efficiency
        /// </summary>
        /// <param name="labeledElementSymbol">element symbol for the label</param>
        /// <param name="labeledLightIsoNum">the light isotope number</param>
        /// <param name="labeledLightIsoAbundance">the light isotope new abundance</param>
        /// <param name="labeledHeavyIsoNum">isotope number of the heavy version</param>
        /// <param name="labeledHeavyIsoAbundance">the heavy label abundance</param>
        public void SetLabeling(string labeledElementSymbol,
            int labeledLightIsoNum, double labeledLightIsoAbundance,
            int labeledHeavyIsoNum, double labeledHeavyIsoAbundance)
        {
            IsSetToLabeled = true;
            elementalSymbol = labeledElementSymbol;

            naturalAbundanceHeavy = Constants.Elements[labeledElementSymbol].IsotopeDictionary[
                labeledElementSymbol + labeledHeavyIsoNum].NaturalAbundance;
            naturalAbundanceLight = Constants.Elements[labeledElementSymbol].IsotopeDictionary[
                labeledElementSymbol + labeledLightIsoNum].NaturalAbundance;

            //TODO: we should never be messing with the Constants values. We should be only altering our own copy!!

            Constants.Elements[labeledElementSymbol].IsotopeDictionary[labeledElementSymbol + labeledHeavyIsoNum].NaturalAbundance =
                labeledHeavyIsoAbundance;
            Constants.Elements[labeledElementSymbol].IsotopeDictionary[labeledElementSymbol + labeledLightIsoNum].NaturalAbundance =
                labeledLightIsoAbundance;
            lightIsotopeNum = labeledLightIsoNum;
            heavyIsotopeNum = labeledHeavyIsoNum;
        }

        /// <summary>
        /// Clears the labeling, resetting to the natural abundances
        /// </summary>
        public void ResetToUnlabeled()
        {
            if (elementalSymbol == "")
            {
                return;
            }

            Constants.Elements[elementalSymbol].IsotopeDictionary[elementalSymbol + lightIsotopeNum].NaturalAbundance =
                naturalAbundanceLight;
            Constants.Elements[elementalSymbol].IsotopeDictionary[elementalSymbol + heavyIsotopeNum].NaturalAbundance =
                naturalAbundanceHeavy;
            IsSetToLabeled = false;
            naturalAbundanceLight = -1;
            naturalAbundanceHeavy = -1;
            elementalSymbol = string.Empty;
            lightIsotopeNum = -1;
            heavyIsotopeNum = -1;
        }

        public IsotopicProfile GetIsotopePattern(string empiricalFormula)
        {
            var elementLookupTable = _peptideUtils.ParseEmpiricalFormulaString(empiricalFormula);
            return GetIsotopePattern(elementLookupTable);
        }

        /// <summary>
        /// Isotope pattern calculator
        /// </summary>
        /// <param name="formula">Dictionary of elements and count, must conform to Constants library</param>
        public IsotopicProfile GetIsotopePattern(Dictionary<string, int> formula)
        {
            double smallestProbability = 0.001f; //Smallest probability we care about.
            double logTotalLast = 0;
            var jaggedProbability = new double[formula.Count][];//Store Isotope probability for each element
            var elementCount = 0;
            foreach (var iso in formula)
            {
                var ionProbability = new List<double>();
                var currentElement = GetMostAbundant(iso.Key);
                if (currentElement.Count > 1)
                {
                    var isotopeDifference = currentElement[1].IsotopeNumber - currentElement[0].IsotopeNumber;
                    var logAmtLight = Math.Log10(currentElement[0].NaturalAbundance);
                    var logAmtHeavy = Math.Log10(currentElement[1].NaturalAbundance);

                    for (var i = 0; i <= iso.Value; i++)
                    {
                        var logTotal = logAmtLight * (iso.Value - i) + logAmtHeavy * i;
                        logTotal += LogAChooseB(iso.Value, i);
                        ionProbability.Add(Math.Pow(10, logTotal));
                        for (var k = 1; k < isotopeDifference; k++)
                        {
                            ionProbability.Add(0.0);
                        }
                        if (i > 1 && logTotalLast > logTotal && (ionProbability[i + i * (isotopeDifference - 1)] < smallestProbability))
                        {
                            break;
                        }
                        logTotalLast = logTotal;
                    }
                }
                else
                {
                    ionProbability.Add(1.0);
                }
                jaggedProbability[elementCount] = ionProbability.ToArray();
                elementCount++;
            }
            RecursiveCalculator(jaggedProbability);

            //normalize the probabilities
            var max = 0.0;
            foreach (var f in IonProbabilities)
            {
                if (f > max)
                {
                    max = f;
                }
            }
            for (var i = 0; i < IonProbabilities.Length; i++)
            {
                IonProbabilities[i] /= max;
            }

            var isoCluster = new IsotopicProfile();
            foreach (var probability in IonProbabilities)
            {
                if (Math.Abs(probability) < double.Epsilon)    // avoid adding all the zero probability peaks
                {
                    break;
                }
                isoCluster.Peaklist.Add(new MSPeak(0.0f, (float)probability));
            }

            return isoCluster;
        }

        /// <summary>
        /// Gets the isotopic pattern for a given labeled mass using the averagine method
        /// </summary>
        /// <param name="inputMass">mass</param>
        /// <returns></returns>
        public IsotopicProfile GetAveraginePattern(double inputMass)
        {
            var formula = GetAveragineFormulaAsTableRoundedToInteger(inputMass);
            var profile = GetIsotopePattern(formula);
            return profile;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Provides an averagine dictionary
        /// </summary>
        /// <returns>Averagine formula</returns>
        private Dictionary<string, double> GetAveragineDictionary()
        {
            // NOTE: these averagine values are the standard ones used
            // in the DMS standard workflow for Orbitrap data
            // see "\\gigasax\DMS_Parameter_Files\Decon2LS\LTQ_Orb_O18_SN2_PeakBR2_PeptideBR1_Thrash_Sum3.xml"

            var averagineDict = new Dictionary<string, double>
            {
                {"C", 4.9384},
                {"H", 7.7583},
                {"N", 1.3577},
                {"O", 1.4773},
                {"S", 0.0417}
            };

            return averagineDict;
        }

        /// <summary>
        /// Gets a the mass of a single averagine "molecule"
        /// </summary>
        /// <param name="averagineDictionary">averagine</param>
        /// <returns></returns>
        private double GetBasePeakMass(Dictionary<string, double> averagineDictionary)
        {
            double basePeak = 0;
            foreach (var element in averagineDictionary.Keys)
            {
                basePeak += GetMostAbundant(element)[0].Mass * averagineDictionary[element];
            }
            return basePeak;
        }

        /// <summary>
        /// Outputs the formula for averagine in Dictionary format. Values rounded to nearest integer.
        /// </summary>
        /// <param name="inputMass">mass</param>
        /// <returns>Averagine formula</returns>
        public Dictionary<string, int> GetAveragineFormulaAsTableRoundedToInteger(double inputMass)
        {
            var averagineDict = GetAveragineDictionary();
            var averagineUnitMass = GetBasePeakMass(averagineDict);

            // get closest pattern to mass
            var numberOfAveragineValuesInInput = inputMass / averagineUnitMass;
            var formula = new Dictionary<string, int>();
            foreach (var element in averagineDict.Keys)
            {
                formula.Add(element, (int)Math.Round(averagineDict[element] * numberOfAveragineValuesInInput));
            }

            return formula;
        }

        public Dictionary<string, double> GetAveragineFormulaAsTable(double inputMass)
        {
            var averagineDict = GetAveragineDictionary();
            var averagineUnitMass = GetBasePeakMass(averagineDict);

            // get closest pattern to mass
            var numberOfAveragineValuesInInput = inputMass / averagineUnitMass;
            var formula = new Dictionary<string, double>();
            foreach (var element in averagineDict.Keys)
            {
                formula.Add(element, (averagineDict[element] * numberOfAveragineValuesInInput));
            }

            return formula;
        }

        public string GetAveragineFormulaAsString(double inputMass, bool roundToIntegers = true)
        {
            if (roundToIntegers)
            {
                var formulaTable = GetAveragineFormulaAsTableRoundedToInteger(inputMass);
                return EmpiricalFormulaUtilities.GetEmpiricalFormulaFromElementTable(formulaTable);
            }
            else
            {
                var formulaTable = GetAveragineFormulaAsTable(inputMass);
                return EmpiricalFormulaUtilities.GetEmpiricalFormulaFromElementTable(formulaTable);
            }
        }

        /// <summary>
        /// Comparison method to sort the isotopes by natural abundance
        /// Sorts largest to smallest
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int CompareNaturalAbundance(Isotope x, Isotope y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            var returnValue = y.NaturalAbundance.CompareTo(x.NaturalAbundance);

            if (returnValue != 0)
            {
                return returnValue;
            }

            return y.Mass.CompareTo(x.Mass);
        }

        /// <summary>
        /// Compares the isotopicNumber for sorting purposes.  Sorts smallest to largest
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int CompareNumber(Isotope x, Isotope y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            return x.IsotopeNumber.CompareTo(y.IsotopeNumber);
        }

        /// <summary>
        /// Outputs a list of isotopes sorted descending by natural abundance
        /// </summary>
        /// <param name="element">isotope</param>
        /// <returns></returns>
        private List<Isotope> GetMostAbundant(string element)
        {
            var s = Constants.Elements[element].IsotopeDictionary.Values.ToList();
            s.Sort(CompareNaturalAbundance);
            while (s.Count > 2)
            {
                s.RemoveAt(s.Count - 1);
            }
            s.Sort(CompareNumber);
            return s;
        }

        /// <summary>
        /// Solves for binomial coefficient using log10 factorials
        /// </summary>
        /// <param name="a">Total number of observations</param>
        /// <param name="b">count of "successes" for an isotope showing up </param>
        /// <returns></returns>
        public static double LogAChooseB(int a, int b) // 10 choose 2 returns 10.9/1.2
        {
            if (a == 0)
            {
                return 0.0f;
            }

            if (b == 0)
            {
                return 0.0f;
            }

            if (a == b)
            {
                return 0.0f;
            }

            var total = LogFactorial(a) - LogFactorial(b) - LogFactorial(a - b);
            return total;
        }

        /// <summary>
        /// Performs the log 10 factorial
        /// </summary>
        /// <param name="n">number of terms</param>
        /// <returns></returns>
        private static double LogFactorial(int n)
        {
            // log n! = 0.5*log(2*pi) + 0.5*log(n) + n*log(n/e) + log(1 + 1/(12*n))
            // log n! = 0.5*log(2*pi*n)            + n*log(n/e) + log(1 + 1/(12*n))
            return
                0.5 * Math.Log10(2 * Math.PI * n)
                + n * Math.Log10(n / Math.E)
                + Math.Log10(1.0 + 1.0 / (12 * n));
        }

        #endregion

        #region Isopopic Profile Calculation

        /// <summary>
        /// Resets the member variable probabilities to 0.  Begins the isotopic profile recursion
        /// </summary>
        /// <param name="jaggedProbabilities">probabilities provided by GetIsotopePattern</param>
        private void RecursiveCalculator(double[][] jaggedProbabilities)
        {
            IonProbabilities = new double[300];
            ProbabilityCalculator(jaggedProbabilities, 0, 0, new List<double>());
        }

        /// <summary>
        /// Calculate the probability of each isotope and stores these values in the probabilities member variable
        /// </summary>
        /// <param name="jaggedProbabilities">probabilities provided by GetIsotopePattern</param>
        /// <param name="level">depth of recursion</param>
        /// <param name="isotopeNum">the isotope index to add the current probability</param>
        /// <param name="probabilitySet">list of current values accumulated in the recursion step</param>
        /// <returns></returns>
        private void ProbabilityCalculator(double[][] jaggedProbabilities, int level, int isotopeNum, ICollection<double> probabilitySet)
        {
            var arrayRowCount = jaggedProbabilities.GetLength(0);
            if (level == arrayRowCount)
            {
                double product = 1;
                foreach (var prob in probabilitySet)
                {
                    product *= prob;
                }
                IonProbabilities[isotopeNum] += product;
                return;
            }

            for (var j = 0; j < jaggedProbabilities[level].Length; j++)
            {
                probabilitySet.Add(jaggedProbabilities[level][j]);
                ProbabilityCalculator(jaggedProbabilities, level + 1, isotopeNum + j, probabilitySet);
                probabilitySet.Remove(jaggedProbabilities[level][j]);
            }
        }

        #endregion

    }
}
