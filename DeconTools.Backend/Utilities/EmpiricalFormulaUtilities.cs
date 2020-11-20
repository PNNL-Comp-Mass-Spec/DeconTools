using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DeconTools.Utilities;
using PNNLOmics.Data.Constants;

namespace DeconTools.Backend.Utilities
{
    public class EmpiricalFormulaUtilities
    {
        public static double GetMonoisotopicMassFromEmpiricalFormula(string empiricalFormula)
        {
            var formulaTable = ParseDoubleEmpiricalFormulaString(empiricalFormula);
            double monoisotopicMass = 0;
            foreach (var valuePair in formulaTable)
            {
                var elementMass = Constants.Elements[valuePair.Key].MassMonoIsotopic;
                var elementCount = valuePair.Value;

                monoisotopicMass += elementMass * elementCount;
            }

            return monoisotopicMass;
        }

        public static string AddFormula(string baseFormula, string formulaToBeAdded)
        {
            var baseElementTable = ParseDoubleEmpiricalFormulaString(baseFormula);
            var addedTable = ParseDoubleEmpiricalFormulaString(formulaToBeAdded);

            foreach (var item in addedTable)
            {
                if (baseElementTable.ContainsKey(item.Key))
                {
                    baseElementTable[item.Key] = baseElementTable[item.Key] + item.Value;
                }
                else
                {
                    baseElementTable.Add(item.Key, item.Value);
                }
            }

            return GetEmpiricalFormulaFromElementTable(baseElementTable);
        }
        public static string SubtractFormula(string baseFormula, string formulaToBeSubtracted)
        {
            var baseElementTable = ParseDoubleEmpiricalFormulaString(baseFormula);
            var subtractedTable = ParseDoubleEmpiricalFormulaString(formulaToBeSubtracted);

            foreach (var item in subtractedTable)
            {
                if (baseElementTable.ContainsKey(item.Key))
                {
                    baseElementTable[item.Key] = baseElementTable[item.Key] - item.Value;
                }
                else
                {
                    baseElementTable.Add(item.Key, item.Value);
                }
            }

            return GetEmpiricalFormulaFromElementTable(baseElementTable);
        }

        public static Dictionary<string, int> ParseEmpiricalFormulaString(string empiricalFormula)
        {
            var doubleDictionary = ParseDoubleEmpiricalFormulaString(empiricalFormula);
            var parsedFormula = new Dictionary<string, int>();

            foreach (var item in doubleDictionary)
            {
                parsedFormula.Add(item.Key, (int)Math.Round(item.Value));
            }

            return parsedFormula;
        }
        public static Dictionary<string, double> ParseDoubleEmpiricalFormulaString(string empiricalFormula)
        {
            var parsedFormula = new Dictionary<string, double>();
            if (string.IsNullOrEmpty(empiricalFormula)) return parsedFormula;

            var formulaIsUniModFormat = empiricalFormula.Contains("(");

            string regexString;
            if (formulaIsUniModFormat)
            {
                var elementArray = empiricalFormula.Split(' ');

                foreach (var element in elementArray)
                {
                    if (element.Contains("("))
                    {
                        regexString = GetRegexStringForUniModFormat();
                        var match = Regex.Match(element, regexString);
                        var elementCountString = match.Groups["num"].Value;
                        var elementSymbol = match.Groups["ele"].Value;
                        Double.TryParse(elementCountString, out var numAtoms);

                        var formulaContainsDuplicateElements = (parsedFormula.ContainsKey(elementSymbol));
                        Check.Require(!formulaContainsDuplicateElements, "Cannot parse formula string. It contains multiple identical elements. Formula= " + empiricalFormula);

                        parsedFormula.Add(elementSymbol, numAtoms);
                    }
                    else
                    {
                        regexString = GetRegexStringForUniModFormatNoParentheses();
                        var regex = new Regex(regexString);    //got this from StackOverflow
                        var matches = regex.Matches(element);

                        foreach (Match item in matches)
                        {
                            var elementSymbol = item.Groups[1].Value + item.Groups[2].Value;

                            var elementCountString = item.Groups[3].Value;

                            if (string.IsNullOrEmpty(elementSymbol)) continue;

                            double numAtoms;
                            if (elementCountString.Length > 0)
                            {
                                Double.TryParse(elementCountString, out numAtoms);
                            }
                            else
                            {
                                numAtoms = 1;
                            }

                            var formulaContainsDuplicateElements = (parsedFormula.ContainsKey(elementSymbol));
                            Check.Require(!formulaContainsDuplicateElements, "Cannot parse formula string. It contains multiple identical elements. Formula= " + empiricalFormula);

                            parsedFormula.Add(elementSymbol, numAtoms);
                        }
                    }
                }
                return parsedFormula;
            }

            regexString = GetRegexStringForNonUniModFormat();
            var re = new Regex(regexString);
            var mc = re.Matches(empiricalFormula);

            foreach (Match item in mc)
            {
                double numAtoms;
                var elementSymbol = item.Groups[1].Value;

                var elementCountString = item.Groups[2].Value;

                if (elementCountString.Length > 0)
                {
                    numAtoms = double.Parse(elementCountString);
                }
                else
                {
                    numAtoms = 1;
                }

                var formulaContainsDuplicateElements = (parsedFormula.ContainsKey(elementSymbol));
                Check.Require(!formulaContainsDuplicateElements,
                              "Cannot parse formula string. It contains multiple identical elements. Formula= " + empiricalFormula);

                parsedFormula.Add(elementSymbol, numAtoms);
            }
            return parsedFormula;
        }

        public static string GetEmpiricalFormulaFromElementTable(Dictionary<string, int> elementTable)
        {
            var sb = new StringBuilder();
            foreach (var item in elementTable)
            {
                if (item.Value == 1)
                {
                    sb.Append(item.Key);
                }
                else
                {
                    sb.Append(item.Key);
                    sb.Append(item.Value);
                }
            }
            return sb.ToString();
        }
        public static string GetEmpiricalFormulaFromElementTable(Dictionary<string, double> elementTable)
        {
            var sb = new StringBuilder();
            foreach (var item in elementTable)
            {
                if (item.Value <= 0)
                {
                    // Do not allow negative element counts
                }
                else if (Math.Abs(item.Value - 1) < float.Epsilon)
                {
                    // Single instance of the element
                    sb.Append(item.Key);
                }
                else
                {
                    sb.Append(item.Key);

                    var lowerLimitForRounding = 1e-4;
                    if (item.Value < lowerLimitForRounding)
                    {
                        var roundedValueString = item.Value.ToString("#.#######");
                        sb.Append(roundedValueString);
                    }
                    else
                    {
                        sb.Append(item.Value);
                    }
                }
            }
            return sb.ToString();
        }

        private static string GetRegexStringForNonUniModFormat()
        {
            // This has a bug - the following would pass: Brrrrrrrrrrrrr0.0.19.23.5
            //return @"([A-Z][a-z]*)([0-9\.]*)";
            // This version will not allow multiple decimal points, and restrict lowercase letters to 2
            // (1 lowercase letter is probably all that's necessary, I don't think we will see synthetically produced elements in Mass Spec)
            return @"([A-Z][a-z]{0,2})(\d*\.?\d*)";
        }

        private static string GetRegexStringForUniModFormat()
        {
            return @"(?<ele>[A-Za-z0-9]+)[\(]*(?<num>[0-9-\.]+)[\)]*";
        }

        private static string GetRegexStringForUniModFormatNoParentheses()
        {
            //eg  '13C' or 'H26' or 'Na2' or '2H2.5'

            return @"([0-9]*)([A-Za-z]*)([0-9\.]*)";
        }
    }
}
