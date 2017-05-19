// Written by Kyle Littlefield for the Department of Energy (PNNL, Richland, WA)
// Copyright 2006, Battelle Memorial Institute
// E-mail: navdeep.jaitly@pnl.gov
// Website: http://omics.pnl.gov/software or http://panomics.pnnl.gov
// -------------------------------------------------------------------------------
// 
// Licensed under the Apache License, Version 2.0; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace Decon2LS
{
    public class MolecularFormulaTranslator
    {
        private Hashtable partFormulas = new Hashtable();

        public MolecularFormulaTranslator() 
        {
        }
        
        public MolecularFormula Translate(String input, String partRegex) 
        {
            try 
            {
                var regex = new Regex(partRegex);
                var formula = new MolecularFormula();
                var matches = regex.Matches(input) ;
                foreach (Match match in matches) 
                {
                    var partFormula = (MolecularFormula) this.partFormulas[match.Value];
                    if (partFormula == null) 
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

        public void Add(String part, MolecularFormula mf) 
        {
            if (part == null) 
            {
                throw new ArgumentNullException("part");
            }
            if (mf == null) 
            {
                throw new ArgumentNullException("mf");
            }
            partFormulas.Add(part, mf);
        }

        public void Set(String part, MolecularFormula mf) 
        {
            if (part == null) 
            {
                throw new ArgumentNullException("part");
            }
            if (mf == null) 
            {
                throw new ArgumentNullException("mf");
            }
            this.partFormulas.Remove(part);
            this.Add(part, mf);
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
        private static ArrayList KNOWN_ELEMENTS = 
            new ArrayList(new String[] {"D","H","Hi","He","Li","Be","B","C","Ci","Cn","N","Ni","Nn","Nx","O",
                                           "Oi","On","F","Ne","Na","Mg","Al","Si","P","S","Cl","Ar","K","Ca","Sc","Ti",
                                           "V","Cr","Mn","Fe","Co","Ni","Cu","Zn","Ga","Ge","As","Se","Br","Kr","Rb",
                                           "Sr","Y","Zr","Nb","Mo","Tc","Ru","Rh","Pd","Ag","Cd","In","Sn","Sb","Te",
                                           "I","Xe","Cs","Ba","La","Ce","Pr","Nd","Pm","Sm","Eu","Gd","Tb","Dy","Ho",
                                           "Er","Tm","Yb","Lu","Hf","Ta","W","Re","Os","Ir","Pt","Au","Hg","Tl","Pb",
                                           "Bi","Po","At","Rn","Fr","Ra","Ac","Th","Pa","U","Np","Pu","Am","Cm","Bk",
                                           "Cf","Es","Fm","Md","No","Lr"});

        /// <summary>
        /// The elements that appear at the start of the result of the ToSimpleOrganicElement string 
        /// method.
        /// </summary>
        private static ArrayList ORGANIC_PREFERENCE_ELEMENTS = 
            new ArrayList(new String[] {"C", "Ci", "Cn", "H", "D", "Hi", "N", "Ni", "Nn", "Nx", "O", "Oi", "On", "S"});

        /// <summary>
        /// Parses a formula into &lt;Symbol, Count&gt; pairs.  The count matches at most 
        /// 8 digits.  Any more and the formula will fail to be parsed.  This guarantees that 
        /// all input the passING the regular expression can be parsed.
        /// </summary>
        private readonly static Regex parsingRegex = 
            new Regex(@"^\s*((?<element>[A-Z][a-z]*)(?<count>-?\d{0,8})\s*)*$");

        public static MolecularFormula Hydrogen = MolecularFormula.Parse("H", "Hydrogen");
        public static MolecularFormula Phosphorus = MolecularFormula.Parse("P", "Phosphorus");
        public static MolecularFormula Oxygen = MolecularFormula.Parse("O", "Oxygen");
        public static MolecularFormula Carbon = MolecularFormula.Parse("C", "Carbon");
        public static MolecularFormula Sulphur = MolecularFormula.Parse("S", "Sulphur");

        /// <summary>
        /// The name of this molecule.  Must be specified at construction and is 
        /// returned by ToString if non-null.
        /// </summary>
        private String name = null;
        /// <summary>
        /// The initial formula passes in.  Is returned by ToString if name is not given.
        /// Is returned by ToFormulaString if non-null.
        /// </summary>
        private String formulaString = null;
        /// <summary>
        /// The count of elements in the table, in &lt;symbol (String), count (int)&gt; pairs.
        /// </summary>
        private Hashtable elementCounts;

        /// <summary>
        /// Creates a new formula, initially with no element counts
        /// </summary>
        public MolecularFormula()
        {
            this.name = "";
            this.formulaString = "";
            this.elementCounts = new Hashtable();
        }

        /// <summary>
        /// Clones the formula.
        /// </summary>
        /// <param name="formula"></param>
        private MolecularFormula(MolecularFormula formula) 
        {
            this.name = formula.name;
            this.formulaString = formula.formulaString;
            this.elementCounts = new Hashtable(formula.elementCounts);
        }

        /// <summary>
        /// Parse the given formula.  Must be in a flat-form (no parantheses).  Only 
        /// elements in the KNOWN_ELEMENTS collection are accepted.
        /// </summary>
        /// <param name="formula"></param>
        /// <returns></returns>
        public static MolecularFormula Parse(String formula) 
        {
            return Parse(formula, null);
        }

        public static MolecularFormula Parse(String formula, String name) 
        {
            return Parse(formula, name, false);
        }

        public static MolecularFormula Parse(String formula, String name, bool allowNegatives) 
        {
            if (formula == null) 
            {
                throw new ArgumentNullException("formula");
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
        private void Add(String element, int count) 
        {
            if (!KNOWN_ELEMENTS.Contains(element))
            {
                throw new ApplicationException("The element (symbol " + element + ") is not known.");
            }
            if (!this.elementCounts.Contains(element)) 
            {
                this.elementCounts.Add(element, count);
            }
            else 
            {
                var currentCount = (int) this.elementCounts[element];
                this.elementCounts[element] = currentCount + count;
            }
        }

        #region "String forms"
        public String ToFormulaString() 
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
        public String ToSimpleElementalString() 
        {
            var formula = "";
            foreach (String	element in KNOWN_ELEMENTS) 
            {
                if (this.elementCounts.Contains(element)) 
                {
                    formula += element + this.elementCounts[element].ToString() + " ";
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
        /// Same as to SimpleElementalString, but favors carbon, hydrogen, nitrogen, oxygen, and sulphur
        /// variants first.
        /// </summary>
        /// <returns></returns>
        public String ToSimpleOrganicElementalString() 
        {
            var formula = "";
            foreach (String s in ORGANIC_PREFERENCE_ELEMENTS) 
            {
                if (this.elementCounts.Contains(s)) 
                {
                    formula += s + this.elementCounts[s] + " ";
                }
            }
            foreach (String	element in KNOWN_ELEMENTS) 
            {
                if (this.elementCounts.Contains(element) && !ORGANIC_PREFERENCE_ELEMENTS.Contains(element)) 
                {
                    formula += element + this.elementCounts[element].ToString() + " ";
                }
            }
            return formula;
        }
        #endregion

        /// <summary>
        /// Returns the formula represented as a hashtable of &lt;element symbols, atomic count&gt;.
        /// The returned hashtable has &lt;String, int&gt; contents.
        /// </summary>
        /// <returns></returns>
        public Hashtable ToElementTable() 
        {
            return new Hashtable(this.elementCounts);
        }

        /// <summary>
        /// Add the given multiple of mf into to.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="mf"></param>
        /// <param name="multiple"></param>
        private void Add(MolecularFormula to, MolecularFormula mf, int multiple) 
        {
            foreach (String s in mf.elementCounts.Keys) 
            {
                to.Add(s, ((int) mf.elementCounts[s]) * multiple);
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
                throw new ArgumentOutOfRangeException("multiple", multiple, "Multiple must be >= 0");
            }
            var newFormula = new MolecularFormula(this);
            newFormula.name = null;
            newFormula.formulaString = null;
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
            if (!(obj is MolecularFormula) || obj == null) 
            {
                return false;
            }

            var mf = (MolecularFormula) obj;
            if (mf.elementCounts.Count != this.elementCounts.Count) 
            {
                return false;
            }
            foreach (String element in this.elementCounts.Keys) 
            {
                if (!mf.elementCounts.Contains(element)) 
                {
                    return false;
                }
                if  (((int) this.elementCounts[element]) != ((int) mf.elementCounts[element])) 
                {
                    return false;
                }
            }
            return true;
        }

    }
}
