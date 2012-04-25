//Written by Josh Aldrich, Tom Taverner, Gordon Slysz at PNNL

using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using PNNLOmics.Data.Constants;

namespace DeconTools.Backend.Utilities.IsotopeDistributionCalculation
{
    public sealed class IsotopicDistributionCalculator
    {
        #region Member Variables
        static IsotopicDistributionCalculator instance = null;
        static readonly object padlock = new object();
        bool isLabeledFlag = false;
        double naturalAbundanceLight = -1;
        double naturalAbundanceHeavy = -1;
        string elementalSymbol = string.Empty;
        int lightIsotopeNum = -1;
        int heavyIsotopeNum = -1;
        double[] probabilities;
        private PeptideUtils _peptideUtils = new PeptideUtils();


        #endregion

        #region Properties

        public double[] IonProbailities
        {
            get { return probabilities; }
        }

        public bool IsSetToLabeled
        {
            get { return isLabeledFlag; }
        }

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
                    if (instance == null)
                    {
                        instance = new IsotopicDistributionCalculator();
                    }
                    return instance;
                }
            }
        }



        /// <summary>
        /// Assign the labeling and the efficiency
        /// </summary>
        /// <param name="labeledElmentSymbol">element symbol for the label</param>
        /// <param name="labeledLightIsoNum">the light isotope number</param>
        /// <param name="labeledLightIsoAbundance">the light isotope new abundance</param>
        /// <param name="labeledHeavyIsoNum">isotope number of the heavy version</param>
        /// <param name="labeledHeavyIsoAbundance">the heavy label abundance</param>
        public void SetLabeling(string labeledElmentSymbol,
            int labeledLightIsoNum, double labeledLightIsoAbundance,
            int labeledHeavyIsoNum, double labeledHeavyIsoAbundance)
        {
            isLabeledFlag = true;
            elementalSymbol = labeledElmentSymbol;

            naturalAbundanceHeavy = Constants.Elements[labeledElmentSymbol].IsotopeDictionary[
                labeledElmentSymbol + labeledHeavyIsoNum].NaturalAbundance;
            naturalAbundanceLight = Constants.Elements[labeledElmentSymbol].IsotopeDictionary[
                labeledElmentSymbol + labeledLightIsoNum].NaturalAbundance;
            Constants.Elements[labeledElmentSymbol].IsotopeDictionary[labeledElmentSymbol + labeledHeavyIsoNum].NaturalAbundance =
                labeledHeavyIsoAbundance;
            Constants.Elements[labeledElmentSymbol].IsotopeDictionary[labeledElmentSymbol + labeledLightIsoNum].NaturalAbundance =
                labeledLightIsoAbundance;
            lightIsotopeNum = labeledLightIsoNum;
            heavyIsotopeNum = labeledHeavyIsoNum;
        }

        /// <summary>
        /// Clears the labeling, resetting to the natural abundances
        /// </summary>
        public void ResetToUnlabeled()
        {
            Constants.Elements[elementalSymbol].IsotopeDictionary[elementalSymbol + lightIsotopeNum].NaturalAbundance =
                naturalAbundanceLight;
            Constants.Elements[elementalSymbol].IsotopeDictionary[elementalSymbol + heavyIsotopeNum].NaturalAbundance =
                naturalAbundanceHeavy;
            isLabeledFlag = false;
            naturalAbundanceLight = -1;
            naturalAbundanceHeavy = -1;
            elementalSymbol = string.Empty;
            lightIsotopeNum = -1;
            heavyIsotopeNum = -1;
        }



        public IsotopicProfile GetIsotopePattern(string empiricalFormula)
        {
            Dictionary<string, int> elementLookupTable = _peptideUtils.ParseEmpiricalFormulaString(empiricalFormula);
            return GetIsotopePattern(elementLookupTable);
        }


        /// <summary>
        /// Isotope pattern calculator
        /// </summary>
        /// <param name="formula">Dictionary of elements and count, must conform to Constants library</param>
        public IsotopicProfile GetIsotopePattern(Dictionary<string, int> formula)
        {
            double smallestProbability = 0.001f; //Smallest probability we care about.
            double logTotal = 0;
            double logTotalLast = 0;
            double[][] jaggedProbability = new double[formula.Count][];//Store Isotope probability for each element
            int elementCount = 0;
            foreach (KeyValuePair<string, int> iso in formula)
            {
                List<double> ionProbability = new List<double>();
                List<Isotope> currentElement = GetMostAbundant(iso.Key);
                if (currentElement.Count > 1)
                {
                    int isotopeDifference = currentElement[1].IsotopeNumber - currentElement[0].IsotopeNumber;
                    double logAmtLight = (double)System.Math.Log10((double)currentElement[0].NaturalAbundance);
                    double logAmtHeavy = (double)System.Math.Log10((double)currentElement[1].NaturalAbundance);

                    for (int i = 0; i <= iso.Value; i++)
                    {

                        logTotal = logAmtLight * (iso.Value - i) + logAmtHeavy * i;
                        logTotal += (double)LogAChooseB(iso.Value, i);
                        ionProbability.Add((double)(System.Math.Pow(10, logTotal)));
                        for (int k = 1; k < isotopeDifference; k++)
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
            double max = 0.0;
            foreach (double f in probabilities)
            {
                if (f > max)
                {
                    max = f;
                }
            }
            for (int i = 0; i < probabilities.Length; i++)
            {
                
                probabilities[i] = probabilities[i] / max;
            }

            
            IsotopicProfile isoCluster = new IsotopicProfile();
            for (int l = 0; l < probabilities.Length; l++)
            {
                if (probabilities[l] == 0)    // avoid adding all the zero probability peaks
                {
                    break;
                }
                isoCluster.Peaklist.Add(new MSPeak(0.0f, (float)probabilities[l], 0.0f, 0.0f));
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
            Dictionary<string, int> formula = GetAveragineFormulaAsTableRoundedToInteger(inputMass);
            IsotopicProfile profile = GetIsotopePattern(formula);
            return profile;

        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Provides an averaginedictionary
        /// </summary>
        /// <returns>Averagine formula</returns>
        private Dictionary<string, double> GetAveragineDictionary()
        {
            //NOTE: these averagine values are the standard ones used
            //in the DMS standard workflow for Orbi data
            //see "\\gigasax\DMS_Parameter_Files\Decon2LS\LTQ_Orb_O18_SN2_PeakBR2_PeptideBR1_Thrash_Sum3.xml"

            Dictionary<string, double> averagineDict = new Dictionary<string, double>();
            averagineDict.Add("C", 4.9384);
            averagineDict.Add("H", 7.7583);
            averagineDict.Add("N", 1.3577);
            averagineDict.Add("O", 1.4773);
            averagineDict.Add("S", 0.0417);

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
            foreach (string element in averagineDictionary.Keys)
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
            Dictionary<string, double> averagineDict = GetAveragineDictionary();
            double averagineUnitMass = GetBasePeakMass(averagineDict);
            // get closest pattern to mass 
            double numberOfAveraginesInInput = inputMass / averagineUnitMass;
            Dictionary<string, int> formula = new Dictionary<string, int>();
            foreach (string element in averagineDict.Keys)
            {
                formula.Add(element, (int)System.Math.Round(averagineDict[element] * numberOfAveraginesInInput));
            }

            return formula;
        }

        public Dictionary<string, double> GetAveragineFormulaAsTable(double inputMass)
        {
            Dictionary<string, double> averagineDict = GetAveragineDictionary();
            double averagineUnitMass = GetBasePeakMass(averagineDict);
            // get closest pattern to mass 
            double numberOfAveraginesInInput = inputMass / averagineUnitMass;
            Dictionary<string, double> formula = new Dictionary<string, double>();
            foreach (string element in averagineDict.Keys)
            {
                formula.Add(element, averagineDict[element] * numberOfAveraginesInInput);
            }

            return formula;
        }




        public string GetAveragineFormulaAsString(double inputMass, bool roundToIntegers=true)
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
                else
                {
                    return -1;
                }
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                else
                {
                    int retval = y.NaturalAbundance.CompareTo(x.NaturalAbundance);

                    if (retval != 0)
                    {
                        return retval;
                    }
                    else
                    {
                        return y.Mass.CompareTo(x.Mass);
                    }
                }
            }
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
                else
                {
                    return -1;
                }
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                else
                {
                    return x.IsotopeNumber.CompareTo(y.IsotopeNumber);
                }
            }
        }

        /// <summary>
        /// Outputs a list of isotopes sorted descending by natural abundance
        /// </summary>
        /// <param name="element">isotope</param>
        /// <returns></returns>
        private List<Isotope> GetMostAbundant(string element)
        {
            List<Isotope> s = Constants.Elements[element].IsotopeDictionary.Values.ToList();
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
        private double LogAChooseB(int a, int b) // 10 choose 2 returns 10.9/1.2
        {
            if (a == 0) return 0.0f;
            if (b == 0) return 0.0f;
            if (a == b) return 0.0f;
            double total = 0.0;
            total += LogFactorial(a);
            total -= LogFactorial(b);
            total -= LogFactorial(a - b);
            return (double)total;
        }

        /// <summary>
        /// Performs the log 10 factorial
        /// </summary>
        /// <param name="n">number of terms</param>
        /// <returns></returns>
        private double LogFactorial(int n)
        {
            //log n! = 0.5log(2.pi) + 0.5logn + nlog(n/e) + log(1 + 1/(12n))
            return (double)0.5 * (
                System.Math.Log10(2 * System.Math.PI * n))
                + n * (System.Math.Log10(n / System.Math.E))
                + (System.Math.Log10(1.0 + 1.0 / (12 * n)));
        }

        #endregion

        #region Isopopic Profile Calculation

        /// <summary>
        /// Resets the member variable probabilities to 0.  Begins the isotopic profile recursion
        /// 
        /// <param name="jaggedProbabilities">probabilities provided by getisotopepattern</param>
        private void RecursiveCalculator(double[][] jaggedProbabilities)
        {
            probabilities = new double[300];
            ProbabilityCalculator(jaggedProbabilities, 0, 0, new List<double>());
        }

        /// <summary>
        /// Calculate the probability of each isotope and stores these values in the probabilities member variable
        /// </summary>
        /// <param name="jaggedProbabilities">probabilities provided by getisotopepattern</param>
        /// <param name="level">depth of recursion</param>
        /// <param name="isotopeNum">the isotope indice to add the current probability</param>
        /// <param name="probabilitySet">list of current values accumulated in the recursion step</param>
        /// <returns></returns>
        private bool ProbabilityCalculator(double[][] jaggedProbabilities, int level, int isotopeNum, List<double> probabilitySet)
        {
            int arrayRowCount = jaggedProbabilities.GetLength(0);
            if (level == arrayRowCount)
            {
                double product = 1;
                foreach (double prob in probabilitySet)
                {
                    product *= prob;
                }
                probabilities[isotopeNum] += product;
                return false;
            }
            else
            {
                for (int j = 0; j < jaggedProbabilities[level].Length; j++)
                {
                    probabilitySet.Add(jaggedProbabilities[level][j]);
                    ProbabilityCalculator(jaggedProbabilities, level + 1, isotopeNum + j, probabilitySet);
                    probabilitySet.Remove(jaggedProbabilities[level][j]);
                }
            }
            return false;
        }


        #endregion





    }
}
