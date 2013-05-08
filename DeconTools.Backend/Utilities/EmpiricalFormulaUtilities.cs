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

		//Workaround method to return a dictionary containing a double value
		//Allows IQ to account for small averagine masses that do not have whole units of averagine
		public static Dictionary<string, double> ParseDoubleEmpiricalFormulaString (string empiricalFormula)
		{
			var parsedFormula = new Dictionary<string, double>();

			if (string.IsNullOrEmpty(empiricalFormula)) return parsedFormula;

			var re = new Regex(@"([A-Z][a-z]*)([0-9\.]*)");    //got this from StackOverflow
            var mc = re.Matches(empiricalFormula);


			foreach (Match item in mc)
			{
				double numAtoms = 0;
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

            StringBuilder sb = new StringBuilder();
            foreach (var item in elementTable)
            {
                if (item.Value <= 0)
                {
                    continue;
                }
                else if (item.Value == 1)
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




        public static Dictionary<string, int> ParseEmpiricalFormulaString(string empiricalFormula, bool formulaIsUniModFormat = false)
        {
            var parsedFormula = new Dictionary<string, int>();

            if (string.IsNullOrEmpty(empiricalFormula)) return parsedFormula;


            if (formulaIsUniModFormat)
            {
                string[] elementArray = empiricalFormula.Split(' ');

                foreach (var element in elementArray)
                {
                    if (element.Contains("("))
                    {
                        var match = Regex.Match(element, @"(?<ele>[A-Za-z0-9]+)\((?<num>[0-9-]+)\)");
                        string elementCountString = match.Groups["num"].Value;
                        string elementSymbol = match.Groups["ele"].Value;
                        int numAtoms = Int32.Parse(elementCountString);

                        bool formulaContainsDuplicateElements = (parsedFormula.ContainsKey(elementSymbol));
                        Check.Require(!formulaContainsDuplicateElements, "Cannot parse formula string. It contains multiple identical elements. Formula= " + empiricalFormula);

                        parsedFormula.Add(elementSymbol, numAtoms);
                    }
                    else
                    {
                        var re = new Regex(@"([A-Z][a-z]*)(\d*)");    //got this from StackOverflow
                        var mc = re.Matches(element);

                        foreach (Match item in mc)
                        {

                            int numAtoms = 0;
                            string elementSymbol = item.Groups[1].Value;

                            string elementCountString = item.Groups[2].Value;


                            if (elementCountString.Length > 0)
                            {
                                numAtoms = Int32.Parse(elementCountString);
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


            }
            else
            {
                var re = new Regex(@"([A-Z][a-z]*)(\d*)");    //got this from StackOverflow
                var mc = re.Matches(empiricalFormula);


                foreach (Match item in mc)
                {

                    int numAtoms = 0;
                    string elementSymbol = item.Groups[1].Value;

                    string elementCountString = item.Groups[2].Value;


                    if (elementCountString.Length > 0)
                    {
                        numAtoms = Int32.Parse(elementCountString);
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



            return parsedFormula;

        }

       
    }
}
