using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DeconTools.Utilities;

namespace DeconTools.Backend.Utilities
{
    public class EmpiricalFormulaUtilities
    {

        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        internal static Dictionary<string, int> ParseEmpiricalFormulaString(string empiricalFormula)
        {

            var parsedFormula = new Dictionary<string, int>();

            if (string.IsNullOrEmpty(empiricalFormula)) return parsedFormula;

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
                Check.Require(!formulaContainsDuplicateElements, "Cannot parse formula string. It contains multiple identical elements.");

                parsedFormula.Add(elementSymbol, numAtoms);


            }

            return parsedFormula;
        }
    }
}
