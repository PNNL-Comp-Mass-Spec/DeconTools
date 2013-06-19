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
            double monomass = 0;
            foreach (KeyValuePair<string, double> valuePair in formulaTable)
            {
                double elementMass = Constants.Elements[valuePair.Key].MassMonoIsotopic;
                double elementCount = valuePair.Value;

                monomass += elementMass*elementCount;
            }

            return monomass;

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
                parsedFormula.Add(item.Key,(int)Math.Round(item.Value));
            }

            return parsedFormula;
        }
        public static Dictionary<string, double> ParseDoubleEmpiricalFormulaString (string empiricalFormula)
		{
			var parsedFormula = new Dictionary<string, double>();
            if (string.IsNullOrEmpty(empiricalFormula)) return parsedFormula;

		    bool formulaIsUnimodFormat = empiricalFormula.Contains("(");

		    string regexString;
		    if (formulaIsUnimodFormat)
            {
               
                    string[] elementArray = empiricalFormula.Split(' ');

                    foreach (var element in elementArray)
                    {
                        if (element.Contains("("))
                        {
                            regexString = GetRegexStringForUnimodFormat();
                            var match = Regex.Match(element, regexString);  
                            string elementCountString = match.Groups["num"].Value;
                            string elementSymbol = match.Groups["ele"].Value;
                            double numAtoms;
                            Double.TryParse(elementCountString, out numAtoms);

                            bool formulaContainsDuplicateElements = (parsedFormula.ContainsKey(elementSymbol));
                            Check.Require(!formulaContainsDuplicateElements, "Cannot parse formula string. It contains multiple identical elements. Formula= " + empiricalFormula);

                            parsedFormula.Add(elementSymbol, numAtoms);
                        }
                        else
                        {
                            regexString = GetRegexStringForUnimodFormatNoParentheses();
                            var regex = new Regex(regexString);    //got this from StackOverflow
                            var matches = regex.Matches(element);

                            foreach (Match item in matches)
                            {

                                string elementSymbol =  item.Groups[1].Value + item.Groups[2].Value;

                                string elementCountString = item.Groups[3].Value;

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

                                bool formulaContainsDuplicateElements = (parsedFormula.ContainsKey(elementSymbol));
                                Check.Require(!formulaContainsDuplicateElements, "Cannot parse formula string. It contains multiple identical elements. Formula= " + empiricalFormula);

                                parsedFormula.Add(elementSymbol, numAtoms);


                            }
                        
                    }


                }
                return parsedFormula;
            }
		    regexString = GetRegexStringForNonUnimodFormat();
		    var re = new Regex(regexString);    
		    var mc = re.Matches(empiricalFormula);


		    foreach (Match item in mc)
		    {
		        double numAtoms;
		        string elementSymbol = item.Groups[1].Value;

		        string elementCountString = item.Groups[2].Value;

		        if (elementCountString.Length > 0)
		        {
		            numAtoms = Double.Parse(elementCountString);
		        }
		        else
		        {
		            numAtoms = 1;
		        }

		        bool formulaContainsDuplicateElements = (parsedFormula.ContainsKey(elementSymbol));
		        Check.Require(!formulaContainsDuplicateElements,
		                      "Cannot parse formula string. It contains multiple identical elements. Formula= " + empiricalFormula);

		        parsedFormula.Add(elementSymbol, numAtoms);
		    }
		    return parsedFormula;
		}
        
        public static string GetEmpiricalFormulaFromElementTable(Dictionary<string, int> elementTable)
        {

            StringBuilder sb = new StringBuilder();
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
                }
                else if (Math.Abs(item.Value - 1) < double.Epsilon)
                {
                    sb.Append(item.Key);
                }
                else
                {
                    sb.Append(item.Key);

	                double lowerLimitForRounding = 1e-4;
                    if (item.Value < lowerLimitForRounding)
                    {
	                    string roundedValueString = item.Value.ToString("#.#######");
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


        private static string GetRegexStringForNonUnimodFormat()
        {
            return @"([A-Z][a-z]*)([0-9\.]*)";
        }

        private static string GetRegexStringForUnimodFormat()
        {
            return @"(?<ele>[A-Za-z0-9]+)[\(]*(?<num>[0-9-\.]+)[\)]*";
        }

        private static string GetRegexStringForUnimodFormatNoParentheses()
        {
            //eg  '13C' or 'H26' or 'Na2' or '2H2.5'

            return @"([0-9]*)([A-Za-z]*)([0-9\.]*)";
        }



       
    }
}
