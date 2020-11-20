using System;
using DeconTools.Backend.Utilities;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Utilities
{
    [Category("MustPass")]
    [TestFixture]
    public class EmpiricalFormulaUtilitiesTests
    {
        [Test]
        public void parseUnimodStyleFormulaTest1()
        {
            var formula = @"H(-3) N(-1)";
            var formulaDictionary = EmpiricalFormulaUtilities.ParseEmpiricalFormulaString(formula);

            Assert.AreEqual(-3, formulaDictionary["H"]);
            Assert.AreEqual(-1, formulaDictionary["N"]);

            var parsedFormula = EmpiricalFormulaUtilities.GetEmpiricalFormulaFromElementTable(formulaDictionary);
        }

        [Test]
        public void parseUnimodStyleFormulaTest2()
        {
            var formula = @"H26 2H(8) C20 N4 O5 S";

            var formulaDictionary = EmpiricalFormulaUtilities.ParseEmpiricalFormulaString(formula);

            Assert.AreEqual(8, formulaDictionary["2H"]);
            Assert.AreEqual(26, formulaDictionary["H"]);
            Assert.AreEqual(1, formulaDictionary["S"]);

            var parsedFormula = EmpiricalFormulaUtilities.GetEmpiricalFormulaFromElementTable(formulaDictionary);
        }

        [Test]
        public void parseUnimodStyleFormulaTest2A()
        {
            var formula = @"H26 2H(8) C20 N4 O5 S";

            var formulaDictionary = EmpiricalFormulaUtilities.ParseEmpiricalFormulaString(formula);

            Assert.AreEqual(8, formulaDictionary["2H"]);
            Assert.AreEqual(26, formulaDictionary["H"]);
            Assert.AreEqual(1, formulaDictionary["S"]);

            var parsedFormula = EmpiricalFormulaUtilities.GetEmpiricalFormulaFromElementTable(formulaDictionary);
        }

        [Test]
        public void parseUnimodStyleFormulaTest2B()
        {
            var formula = @"H26 2H(8) C20 N4 O5 S";

            var formulaDictionary = EmpiricalFormulaUtilities.ParseDoubleEmpiricalFormulaString(formula);

            Assert.AreEqual(8, formulaDictionary["2H"]);
            Assert.AreEqual(26, formulaDictionary["H"]);
            Assert.AreEqual(1, formulaDictionary["S"]);

            var parsedFormula = EmpiricalFormulaUtilities.GetEmpiricalFormulaFromElementTable(formulaDictionary);
        }

        [Test]
        public void parseUnimodStyleFormulaTest3()
        {
            var formula = @"H32 C34 N4 O4 Fe";

            var formulaDictionary = EmpiricalFormulaUtilities.ParseEmpiricalFormulaString(formula);

            Assert.AreEqual(34, formulaDictionary["C"]);
            Assert.AreEqual(32, formulaDictionary["H"]);
            Assert.AreEqual(1, formulaDictionary["Fe"]);
        }

        [Test]
        public void parseUnimodStyleFormulaTest4()
        {
            var formula = @"H32C34N4O4Fe";

            var formulaDictionary = EmpiricalFormulaUtilities.ParseEmpiricalFormulaString(formula);

            Assert.AreEqual(34, formulaDictionary["C"]);
            Assert.AreEqual(32, formulaDictionary["H"]);
            Assert.AreEqual(1, formulaDictionary["Fe"]);
        }

        [Test]
        public void ParseUnimodStyleTest5()
        {
            var formula = "H(-3) 2H(3) C(-1) 13C O 15N(10)";

            var formulaDictionary = EmpiricalFormulaUtilities.ParseEmpiricalFormulaString(formula);

            Assert.AreEqual(-1, formulaDictionary["C"]);
            Assert.AreEqual(-3, formulaDictionary["H"]);
            Assert.AreEqual(1, formulaDictionary["13C"]);
            Assert.AreEqual(10, formulaDictionary["15N"]);
        }

        [Test]
        public void ParseUnimodStyleTest5B()
        {
            var formula = "H(-3) 2H(3) C(-1.5) 13C O 15N(10.343)";

            var formulaDictionary = EmpiricalFormulaUtilities.ParseEmpiricalFormulaString(formula);

            Assert.AreEqual(-2, formulaDictionary["C"]);
            Assert.AreEqual(-3, formulaDictionary["H"]);
            Assert.AreEqual(1, formulaDictionary["13C"]);
            Assert.AreEqual(10, formulaDictionary["15N"]);
        }

        [Test]
        public void ParseUnimodStyleTest5C()
        {
            var formula = "H(-3) 2H(3) C(-1.5) 13C O 15N(10.343)";

            var formulaDictionary = EmpiricalFormulaUtilities.ParseDoubleEmpiricalFormulaString(formula);

            Assert.AreEqual(-1.5, formulaDictionary["C"]);
            Assert.AreEqual(-3, formulaDictionary["H"]);
            Assert.AreEqual(1, formulaDictionary["13C"]);
            Assert.AreEqual(10.343, formulaDictionary["15N"]);
        }

        [Test]
        public void addUnimodFormulaTest1()
        {
            var testPeptide = "SAMPLER";

            var peptideUtils = new PeptideUtils();
            var baseFormula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(testPeptide);

            var mod = "H3 C2 N O";
            var modFormula = EmpiricalFormulaUtilities.AddFormula(baseFormula, mod);

            Console.WriteLine("Unmodified peptide= " + baseFormula);
            Console.WriteLine("Modified peptide= " + modFormula);

            Assert.AreEqual("C35H61N11O12S", modFormula);
        }

        [Test]
        public void addUnimodFormulaTest2()
        {
            var testPeptide = "SAMPLER";

            var peptideUtils = new PeptideUtils();
            var baseFormula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(testPeptide);

            var mod = "H(-3) N(-1)";
            var modFormula = EmpiricalFormulaUtilities.AddFormula(baseFormula, mod);

            Console.WriteLine("Unmodified peptide= " + baseFormula);
            Console.WriteLine("Modified peptide= " + modFormula);

            Assert.AreEqual("C33H55N9O11S", modFormula);
        }

        [Test]
        public void GetMonoisotopicMassFromEmpiricalFormulaTest1()
        {
            var testPeptide = "SAMPLER";
            var peptideUtils = new PeptideUtils();
            var formula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(testPeptide);

            var monomass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(formula);

            Assert.IsTrue(monomass > 0);
            Console.WriteLine("SAMPLER monoisotopic mass= \t" + monomass);

            Assert.AreEqual(802.40072m, (decimal)Math.Round(monomass, 5));    //note that Peptide Util reports 802.40071, as does MacCoss's lab: http://proteome.gs.washington.edu/cgi-bin/aa_calc.pl
        }

        [Test]
        public void GetMonoisotopicMassForEmpiricalFormulaWithIronTest1()
        {
            var formula = "C145 H208 N39 O40 S2 Fe1";
            var monomass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(formula);

            Assert.IsTrue(monomass > 0);
            Console.WriteLine("monoisotopic mass= \t" + monomass);
        }

        [Test]
        public void AddAcetylationTest1()
        {
            var testPeptide = "SAMPLER";
            var peptideUtils = new PeptideUtils();
            var formula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(testPeptide);

            var acetylationFormula = "C2H2O";

            var empiricalFormula = EmpiricalFormulaUtilities.AddFormula(formula, acetylationFormula);

            var massUnmodified = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(formula);
            var massModified = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(empiricalFormula);
            var diff = Math.Round(massModified - massUnmodified, 1, MidpointRounding.AwayFromZero);

            Console.WriteLine(formula + "\t" + massUnmodified);
            Console.WriteLine(empiricalFormula + "\t" + massModified);

            Console.WriteLine("diff= " + diff);

            Assert.AreEqual(42.0, diff);
        }

        [Test]
        public void PyroglutamateTest1()
        {
            const string testPeptide = "SAMPLER";
            var peptideUtils = new PeptideUtils();
            var formula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(testPeptide);

            const string pyroglutamateMod = "H3N1";

            var empiricalFormula = EmpiricalFormulaUtilities.SubtractFormula(formula, pyroglutamateMod);

            var massUnmodified = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(formula);
            var massModified = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(empiricalFormula);
            var diff = Math.Round(massModified - massUnmodified, 1, MidpointRounding.AwayFromZero);

            Console.WriteLine(formula + "\t" + massUnmodified);
            Console.WriteLine(empiricalFormula + "\t" + massModified);
            Console.WriteLine("diff= " + diff);

            Assert.AreEqual(-17.0, diff);
        }

        [Test]
        public void AddPhosphorylationTest1()
        {
            const string testPeptide = "SAMPLER";
            var peptideUtils = new PeptideUtils();
            var formula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(testPeptide);

            const string phosphorylationMod = "HPO3";

            var empiricalFormula = EmpiricalFormulaUtilities.AddFormula(formula, phosphorylationMod);

            var massUnmodified = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(formula);
            var massModified = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(empiricalFormula);
            var diff = Math.Round(massModified - massUnmodified, 1, MidpointRounding.AwayFromZero);

            Console.WriteLine(formula + "\t" + massUnmodified);
            Console.WriteLine(empiricalFormula + "\t" + massModified);
            Console.WriteLine("diff= " + diff);

            Assert.AreEqual(80.0, diff);
        }
    }
}
