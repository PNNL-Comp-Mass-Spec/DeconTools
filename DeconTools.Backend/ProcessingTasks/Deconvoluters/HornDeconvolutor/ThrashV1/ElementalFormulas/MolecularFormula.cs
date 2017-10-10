using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.ElementalFormulas
{
    public class MolecularFormula
    {
        public double AverageMass;
        public List<AtomicCount> ElementalComposition = new List<AtomicCount>();
        public double MonoisotopicMass;

        public double TotalAtomCount;

        public MolecularFormula()
        {
            TotalAtomCount = 0;
            MonoisotopicMass = 0;
            AverageMass = 0;
            Formula = "";
        }

        public MolecularFormula(string formula, ElementIsotopes atomicInformation)
        {
            SetMolecularFormula(formula, atomicInformation);
        }

        public MolecularFormula(MolecularFormula formula)
        {
            Formula = formula.Formula;
            TotalAtomCount = formula.TotalAtomCount;
            MonoisotopicMass = formula.MonoisotopicMass;
            ElementalComposition.Clear();
            ElementalComposition.AddRange(formula.ElementalComposition);
        }

        public string Formula { get; private set; }

        public int NumElements => ElementalComposition.Count;

        public override string ToString()
        {
            var data = " monomw =" + MonoisotopicMass + " averagemw=" + AverageMass + "\n";
            foreach (var atomicCount in ElementalComposition)
            {
                data += atomicCount.Index + " Num Atoms = " +
                        atomicCount.NumCopies + "\n";
            }
            return data;
        }

        public bool IsAssigned()
        {
            return NumElements != 0;
        }

        public void Clear()
        {
            TotalAtomCount = 0;
            MonoisotopicMass = 0;
            AverageMass = 0;
            Formula = "";
            ElementalComposition.Clear();
        }

        public void SetMolecularFormula(string molFormula, ElementIsotopes atomicInformation)
        {
            Formula = molFormula;
            TotalAtomCount = 0;
            MonoisotopicMass = 0;

            ElementalComposition.Clear();

            var validator = new Regex(@"^(\s*[A-Z][a-z]?\s*\d*\.?\d*)*\s*$", RegexOptions.CultureInvariant);
            if (!validator.IsMatch(molFormula))
            {
                throw new Exception("Molecular formula specified was invalid. Format required is one or more " +
                                    "groups similar to \"[atomic symbol][count]\", where atomic symbol is an " +
                                    "uppercase letter that may be followed by a lowercase letter, and count " +
                                    "is a number that consists of numeric digits and maybe one decimal point.");
            }
            var grouper = new Regex(@"([A-Z][a-z]?)\s*(\d*\.?\d*)", RegexOptions.CultureInvariant);
            var groups = grouper.Matches(molFormula);
            foreach (Match group in groups)
            {
                var match = group.Groups;
                // match[0] returns the entire match, match[1] and match[2] will have the individual groups
                var atomicSymbol = match[1].Value;
                var countStr = match[2].Value;
                double count = 1;
                if (!string.IsNullOrWhiteSpace(countStr))
                {
                    count = Convert.ToDouble(countStr.Trim());
                }
                // now we should have a symbol and a count. Lets get the atomicity of the atom.
                var elementIndex = atomicInformation.GetElementIndex(atomicSymbol);
                if (elementIndex == -1)
                {
                    // theres an error. two decimals.
                    var errorStr =
                        "Molecular Formula specified was incorrect. Symbol in formula was not recognize from elements provided: ";
                    errorStr += atomicSymbol;
                    throw new Exception(errorStr);
                }
                ElementalComposition.Add(new AtomicCount(elementIndex, count));
                TotalAtomCount += count;
                MonoisotopicMass += atomicInformation.ElementalIsotopesList[elementIndex].Isotopes[0].Mass * count;
                AverageMass += atomicInformation.ElementalIsotopesList[elementIndex].AverageMass * count;
            }
            /*
            // Regular expressions simplify this code so much. See above.
            var formulaLength = formula.Length;
            var index = 0;
            //var numAtoms = atomicInformation.GetNumElements();
            while (index < formulaLength)
            {
                while (index < formulaLength && (formula[index] == ' ' || formula[index] == '\t'))
                {
                    index++;
                }

                var startIndex = index;
                var stopIndex = startIndex;
                var symbolChar = formula[stopIndex];
                if (!(symbolChar >= 'A' && symbolChar <= 'Z') && !(symbolChar >= 'a' && symbolChar <= 'z'))
                {
                    var errorStr = "Molecular Formula specified was incorrect at position: " + (stopIndex + 1) +
                                   ". Should have element symbol there";
                    throw new Exception(errorStr);
                }

                while ((symbolChar >= 'A' && symbolChar <= 'Z') || (symbolChar >= 'a' && symbolChar <= 'z'))
                {
                    stopIndex++;
                    if (stopIndex == formulaLength)
                        break;
                    symbolChar = formula[stopIndex];
                }

                var atomicSymbol = formula.Substring(startIndex, stopIndex - startIndex);
                index = stopIndex;
                while (index < formulaLength && (formula[index] == ' ' || formula[index] == '\t'))
                {
                    index++;
                }
                double count = 0;
                if (index == formulaLength)
                {
                    // assume that the last symbol had a 1.
                    count = 1;
                }
                startIndex = index;
                stopIndex = startIndex;
                symbolChar = formula[stopIndex];
                var decimalFound = false;
                while ((symbolChar >= '0' && symbolChar <= '9') || symbolChar == '.')
                {
                    if (symbolChar == '.')
                    {
                        if (decimalFound)
                        {
                            // theres an error. two decimals.
                            var errorStr = "Molecular Formula specified was incorrect at position: " +
                                           (stopIndex + 1) + ". Two decimal points present";
                            throw new Exception(errorStr);
                        }
                        decimalFound = true;
                    }
                    stopIndex++;
                    if (stopIndex == formulaLength)
                        break;
                    symbolChar = formula[stopIndex];
                }
                if (startIndex == stopIndex)
                {
                    count = 1;
                }
                count = Convert.ToDouble(formula.Substring(startIndex, stopIndex - startIndex));
                //count = Helpers.atof(formula.Substring(index));
                // now we should have a symbol and a count. Lets get the atomicity of the atom.
                var elementIndex = atomicInformation.GetElementIndex(atomicSymbol);
                if (elementIndex == -1)
                {
                    // theres an error. two decimals.
                    var errorStr =
                        "Molecular Formula specified was incorrect. Symbol in formula was not recognize from elements provided: ";
                    errorStr += atomicSymbol;
                    throw new Exception(errorStr);
                }
                ElementalComposition.Add(new AtomicCount(elementIndex, count));
                index = stopIndex;
                TotalAtomCount += count;
                MonoisotopicMass += atomicInformation.ElementalIsotopesList[elementIndex].Isotopes[0].Mass * count;
                AverageMass += atomicInformation.ElementalIsotopesList[elementIndex].AverageMass * count;
            }*/
        }

        public void AddAtomicCount(AtomicCount cnt, double monoMass, double avgMass)
        {
            var numElements = ElementalComposition.Count;
            for (var elemNum = 0; elemNum < numElements; elemNum++)
            {
                if (ElementalComposition[elemNum].Index == cnt.Index)
                {
                    ElementalComposition[elemNum].NumCopies += cnt.NumCopies;
                    MonoisotopicMass += monoMass;
                    AverageMass += avgMass;
                    TotalAtomCount += cnt.NumCopies;
                    return;
                }
            }
            ElementalComposition.Add(cnt);
            MonoisotopicMass += monoMass;
            AverageMass += avgMass;
            TotalAtomCount += cnt.NumCopies;
        }

        public static MolecularFormula ConvertFromString(string formulaStr, ElementIsotopes elements = null)
        {
            var formula = new MolecularFormula();
            if (elements == null)
            {
                elements = new ElementIsotopes();
            }

            formula.SetMolecularFormula(formulaStr, elements);

            return formula;
        }
    }
}