// Written by Kyle Littlefield for the Department of Energy (PNNL, Richland, WA)
// Copyright 2006, Battelle Memorial Institute
// E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
// Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics
// -------------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DeconTools.Backend.Utilities
{
    public class MolecularFormulaTranslator
    {
        private readonly Dictionary<string, MolecularFormula> partFormulas = new Dictionary<string, MolecularFormula>();

        public MolecularFormula Translate(string input, string partRegex)
        {
            try
            {
                var regex = new Regex(partRegex);
                var formula = new MolecularFormula();
                var matches = regex.Matches(input);
                foreach (Match match in matches)
                {
                    if (!partFormulas.TryGetValue(match.Value, out var partFormula))
                    {
                        throw new ApplicationException("Formula for part " + match.Value + " is unknown.");
                    }
                    formula = formula.Add(partFormula, 1);
                }
                return formula;
            }
            catch (Exception e)
            {
                throw new ApplicationException("Translation of " + input + " failed.  " + e.Message, e);
            }
        }

        public void Add(string part, MolecularFormula mf)
        {
            if (part == null)
            {
                throw new ArgumentNullException(nameof(part));
            }
            if (mf == null)
            {
                throw new ArgumentNullException(nameof(mf));
            }

            // This will raise an exception if partFormulas already contains the part string
            partFormulas.Add(part, mf);
        }

        public void Set(string part, MolecularFormula mf)
        {
            if (part == null)
            {
                throw new ArgumentNullException(nameof(part));
            }

            if (mf == null)
            {
                throw new ArgumentNullException(nameof(mf));
            }

            if (partFormulas.ContainsKey(part))
            {
                partFormulas.Remove(part);
            }

            Add(part, mf);
        }
    }

    /// <summary>
    /// Summary description for MolecularFormula.
    /// </summary>
    public class MolecularFormula
    {
        /// <summary>
        /// The order of this list is not important, but only elements in this list are parsed
        /// from the formula.  Other symbols will cause an exception.  Putting them in a list allows
        /// the Contains() method to be used easily.
        /// </summary>
        private static readonly SortedSet<string> KNOWN_ELEMENTS =
            new SortedSet<string> {"D","H","Hi","He","Li","Be","B","C","Ci","Cn","N","Ni","Nn","Nx","O",
                                  "Oi","On","F","Ne","Na","Mg","Al","Si","P","S","Cl","Ar","K","Ca","Sc","Ti",
                                  "V","Cr","Mn","Fe","Co","Ni","Cu","Zn","Ga","Ge","As","Se","Br","Kr","Rb",
                                  "Sr","Y","Zr","Nb","Mo","Tc","Ru","Rh","Pd","Ag","Cd","In","Sn","Sb","Te",
                                  "I","Xe","Cs","Ba","La","Ce","Pr","Nd","Pm","Sm","Eu","Gd","Tb","Dy","Ho",
                                  "Er","Tm","Yb","Lu","Hf","Ta","W","Re","Os","Ir","Pt","Au","Hg","Tl","Pb",
                                  "Bi","Po","At","Rn","Fr","Ra","Ac","Th","Pa","U","Np","Pu","Am","Cm","Bk",
                                  "Cf","Es","Fm","Md","No","Lr"};

        /// <summary>
        /// The elements that appear at the start of the result of the ToSimpleOrganicElement string
        /// method.
        /// </summary>
        private static readonly SortedSet<string> ORGANIC_PREFERENCE_ELEMENTS =
            new SortedSet<string> { "C", "Ci", "Cn", "H", "D", "Hi", "N", "Ni", "Nn", "Nx", "O", "Oi", "On", "S" };

        /// <summary>
        /// Parses a formula into &lt;Symbol, Count&gt; pairs.  The count matches at most
        /// 8 digits.  Any more and the formula will fail to be parsed.  This guarantees that
        /// all input the passING the regular expression can be parsed.
        /// </summary>
        private static readonly Regex parsingRegex =
            new Regex(@"^\s*((?<element>[A-Z][a-z]*)(?<count>-?\d{0,8})\s*)*$");

        public static readonly MolecularFormula Hydrogen = Parse("H", "Hydrogen");
        public static readonly MolecularFormula Phosphorus = Parse("P", "Phosphorus");
        public static readonly MolecularFormula Oxygen = Parse("O", "Oxygen");
        public static readonly MolecularFormula Carbon = Parse("C", "Carbon");
        public static readonly MolecularFormula Sulphur = Parse("S", "Sulphur");

        /// <summary>
        /// The name of this molecule.  Must be specified at construction and is
        /// returned by ToString if non-null.
        /// </summary>
        private string name;

        /// <summary>
        /// The initial formula passes in.  Is returned by ToString if name is not given.
        /// Is returned by ToFormulaString if non-null.
        /// </summary>
        private string formulaString;

        /// <summary>
        /// The count of elements in the table, in &lt;symbol (string), count (int)&gt; pairs.
        /// </summary>
        private readonly Dictionary<string, int> elementCounts;

        /// <summary>
        /// Creates a new formula, initially with no element counts
        /// </summary>
        public MolecularFormula()
        {
            name = "";
            formulaString = "";
            elementCounts = new Dictionary<string, int>();
        }

        /// <summary>
        /// Clones the formula.
        /// </summary>
        /// <param name="formula"></param>
        private MolecularFormula(MolecularFormula formula)
        {
            name = formula.name;
            formulaString = formula.formulaString;
            elementCounts = new Dictionary<string, int>(formula.elementCounts);
        }

        /// <summary>
        /// Parse the given formula.  Must be in a flat-form (no parentheses).
        /// Only elements in the KNOWN_ELEMENTS collection are accepted.
        /// </summary>
        /// <param name="formula"></param>
        /// <returns></returns>
        public static MolecularFormula Parse(string formula)
        {
            return Parse(formula, null);
        }

        public static MolecularFormula Parse(string formula, string name)
        {
            return Parse(formula, name, false);
        }

        public static MolecularFormula Parse(string formula, string name, bool allowNegatives)
        {
            if (formula == null)
            {
                throw new ArgumentNullException(nameof(formula));
            }
            var match = parsingRegex.Match(formula);
            if (!match.Success)
            {
                throw new ApplicationException("Formula can not be parsed");
            }

            var mf = new MolecularFormula();
            var matched = match.Groups["element"].Captures.Count;

            //Console.WriteLine("Num Groups {0}", matched);
            // Iterate through the matched groups, updating element counts
            for (var i = 0; i < matched; i++)
            {
                var element = match.Groups["element"].Captures[i].Value;
                var count = match.Groups["count"].Captures[i].Value;

                //Console.WriteLine("Element {0}, Count {1}", element, count);
                // If the symbol is unknown, throw an exception.
                // The multiple defaults to 1 if not found.  So CHHCHH is C2 H4.
                var multiple = 1;
                if (count != "")
                {
                    try
                    {
                        multiple = int.Parse(count);
                        if (multiple < 0 && !allowNegatives)
                        {
                            throw new ApplicationException("Negative multiple " + multiple + " for element " +
                                element + " is not allowed.");
                        }
                    }
                    // This can never actually happen, because the regex makes sure that
                    // only integer values which can be parsed make it to this stage
                    catch (Exception ex)
                    {
                        throw new ApplicationException("The multiple for element " + element + ", " + count + ", was not parseable", ex);
                    }
                }
                mf.Add(element, multiple);
            }
            mf.formulaString = formula;
            mf.name = name;
            return mf;
        }

        /// <summary>
        /// Adds the given number of atoms of the element with the given symbol to this
        /// formula.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="count"></param>
        private void Add(string element, int count)
        {
            if (!KNOWN_ELEMENTS.Contains(element))
            {
                throw new ApplicationException("The element (symbol " + element + ") is not known.");
            }

            if (elementCounts.TryGetValue(element, out var currentCount))
            {
                elementCounts[element] = currentCount + count;
            }
            else
            {
                elementCounts.Add(element, count);
            }
        }

        #region "string forms"

        public string ToFormulaString()
        {
            if (formulaString != null)
            {
                return formulaString;
            }
            return ToSimpleElementalString();
        }

        /// <summary>
        /// Writes this formula as a string of elements and counts.
        /// </summary>
        /// <returns></returns>
        public string ToSimpleElementalString()
        {
            var formula = "";
            foreach (var element in KNOWN_ELEMENTS)
            {
                if (elementCounts.TryGetValue(element, out var elementCount))
                {
                    formula += element + elementCount + " ";
                }
            }
            return formula;
        }

        public override string ToString()
        {
            if (name != null)
            {
                return name;
            }
            return ToFormulaString();
        }

        /// <summary>
        /// Same as to SimpleElementalString, but favors carbon, hydrogen, nitrogen, oxygen, and sulfur
        /// variants first.
        /// </summary>
        /// <returns></returns>
        public string ToSimpleOrganicElementalString()
        {
            var formula = "";
            foreach (var element in ORGANIC_PREFERENCE_ELEMENTS)
            {
                if (elementCounts.TryGetValue(element, out var elementCount))
                {
                    formula += element + elementCount + " ";
                }
            }
            foreach (var element in KNOWN_ELEMENTS)
            {
                if (!ORGANIC_PREFERENCE_ELEMENTS.Contains(element) && elementCounts.TryGetValue(element, out var elementCount))
                {
                    formula += element + elementCount + " ";
                }
            }
            return formula;
        }

        #endregion

        /// <summary>
        /// Returns the formula represented as a hashtable of &lt;element symbols, atomic count&gt;.
        /// The returned hashtable has &lt;string, int&gt; contents.
        /// </summary>
        /// <returns></returns>
        public Hashtable ToElementTable()
        {
            return new Hashtable(elementCounts);
        }

        /// <summary>
        /// Add the given multiple of formulaToAdd into targetFormula
        /// </summary>
        /// <param name="targetFormula"></param>
        /// <param name="formulaToAdd"></param>
        /// <param name="multiple"></param>
        private void Add(MolecularFormula targetFormula, MolecularFormula formulaToAdd, int multiple)
        {
            foreach (var item in formulaToAdd.elementCounts)
            {
                targetFormula.Add(item.Key, item.Value * multiple);
            }
        }

        /// <summary>
        /// Create a new MolecularFormula by adding the given formula (times the given multiple)
        /// to this formula.
        /// </summary>
        /// <param name="mf"></param>
        /// <param name="multiple"></param>
        /// <returns></returns>
        public MolecularFormula Add(MolecularFormula mf, int multiple)
        {
            return Add(mf, multiple, false);
        }

        public MolecularFormula Add(MolecularFormula mf, int multiple, bool permitNegativeMultiple)
        {
            if (multiple < 0 && !permitNegativeMultiple)
            {
                throw new ArgumentOutOfRangeException(nameof(multiple), multiple, "Multiple must be >= 0");
            }

            var newFormula = new MolecularFormula(this)
            {
                name = null,
                formulaString = null
            };
            Add(newFormula, mf, multiple);
            return newFormula;
        }

        /// <summary>
        /// Tells whether this formula is equal to the given formula by element count.  This is true
        /// only if both formulas have the same number of each element.  This method does not in
        /// any way check if the formulas have the same molecular structure.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is MolecularFormula))
            {
                return false;
            }

            var mf = (MolecularFormula)obj;
            if (mf.elementCounts.Count != elementCounts.Count)
            {
                return false;
            }

            foreach (var item in elementCounts)
            {
                if (!mf.elementCounts.TryGetValue(item.Key, out var comparisonCount))
                {
                    return false;
                }
                if (item.Value != comparisonCount)
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
