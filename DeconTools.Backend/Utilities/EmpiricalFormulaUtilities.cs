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
            
            var formulaTable = ParseEmpiricalFormulaString(empiricalFormula);
            
            double monomass = 0;
            foreach (KeyValuePair<string, int> valuePair in formulaTable)
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
            var parsedFormula = new Dictionary<string, int>();

            if (string.IsNullOrEmpty(empiricalFormula)) return parsedFormula;

            bool formulaIsUniModFormat = (empiricalFormula.Contains("(") || empiricalFormula.Contains(" "));

            if (formulaIsUniModFormat)
            {
                string[] elementArray = empiricalFormula.Split(' ');

                foreach (var element in elementArray)
                {

                    Regex regex;
                    if (element.Contains("("))
                    {
                        regex = new Regex(@"([0-9]*[A-Z][a-z]*)\(([0-9-\.]+)\)");

                    }
                    else
                    {
                        regex = new Regex(@"([0-9]*[A-Z][a-z]*)([0-9\.]*)");


                    }

                    var match = regex.Match(element);

                    int numAtoms = 0;
                    string elementSymbol = match.Groups[1].Value;
                    string elementCountString = match.Groups[2].Value;


                    if (elementCountString.Length > 0)
                    {
                        numAtoms = (int)Math.Round(double.Parse(elementCountString));
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
            else
            {

                //eg C50H100N20O15S  or C50.5050H100.4003N20.4994O15S0.3434

                var re = new Regex(@"([A-Z][a-z]*)([0-9\.]*)");    //got this from StackOverflow
                var mc = re.Matches(empiricalFormula);


                foreach (Match item in mc)
                {

                    int numAtoms = 0;
                    string elementSymbol = item.Groups[1].Value;

                    string elementCountString = item.Groups[2].Value;




                    if (elementCountString.Length > 0)
                    {
                        numAtoms = (int)Math.Round(double.Parse(elementCountString));
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



		//Workaround method to return a dictionary containing a double value
		//Allows IQ to account for small averagine masses that do not have whole units of averagine
		public static Dictionary<string, double> ParseDoubleEmpiricalFormulaString (string empiricalFormula)
		{

            var parsedFormula = new Dictionary<string, double>();

            if (string.IsNullOrEmpty(empiricalFormula)) return parsedFormula;

            bool formulaIsUniModFormat = (empiricalFormula.Contains("(") || empiricalFormula.Contains(" "));

            if (formulaIsUniModFormat)
            {
                string[] elementArray = empiricalFormula.Split(' ');

                foreach (var element in elementArray)
                {

                    Regex regex;

                    if (element.Contains("("))
                    {
                        regex = new Regex(@"([0-9]*[A-Z][a-z]*)\(([0-9-\.]+)\)");

                    }
                    else
                    {
                        regex = new Regex(@"([0-9]*[A-Z][a-z]*)([0-9\.]*)");


                    }

                    var match = regex.Match(element);

                    double numAtoms = 0;
                    string elementSymbol = match.Groups[1].Value;
                    string elementCountString = match.Groups[2].Value;


                    if (elementCountString.Length > 0)
                    {
                        numAtoms = double.Parse(elementCountString);
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
            else
            {
                var re = new Regex(@"([A-Z][a-z]*)([0-9\.]*)");    //got this from StackOverflow
                var mc = re.Matches(empiricalFormula);


                foreach (Match item in mc)
                {

                    double numAtoms = 0;
                    string elementSymbol = item.Groups[1].Value;

                    string elementCountString = item.Groups[2].Value;




                    if (elementCountString.Length > 0)
                    {
                        numAtoms = double.Parse(elementCountString);
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



            //var parsedFormula = new Dictionary<string, double>();

            

            //if (string.IsNullOrEmpty(empiricalFormula)) return parsedFormula;

            //bool formulaIsUniModFormat = (empiricalFormula.Contains("(") || empiricalFormula.Contains(" "));



            //var re = new Regex(@"([A-Z][a-z]*)([0-9\.]*)");    //got this from StackOverflow
            //var mc = re.Matches(empiricalFormula);


            //foreach (Match item in mc)
            //{
            //    double numAtoms = 0;
            //    string elementSymbol = item.Groups[1].Value;

            //    string elementCountString = item.Groups[2].Value;

            //    if (elementCountString.Length > 0)
            //    {
            //        numAtoms = Double.Parse(elementCountString);
            //    }
            //    else
            //    {
            //        numAtoms = 1;
            //    }

            //    bool formulaContainsDuplicateElements = (parsedFormula.ContainsKey(elementSymbol));
            //    Check.Require(!formulaContainsDuplicateElements,
            //                  "Cannot parse formula string. It contains multiple identical elements. Formula= " + empiricalFormula);

            //    parsedFormula.Add(elementSymbol, numAtoms);
            //}
            //return parsedFormula;
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

	                double lowerLimitForRounding = 1e-5;
                    if (item.Value < lowerLimitForRounding)
                    {
	                    string roundedValueString = item.Value.ToString("#.######");
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




       

       
    }
}
