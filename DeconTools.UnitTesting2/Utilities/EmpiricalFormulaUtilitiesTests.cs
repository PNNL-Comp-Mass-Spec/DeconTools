using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Utilities;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Utilities
{
    [TestFixture]
    public class EmpiricalFormulaUtilitiesTests
    {
        [Test]
        public void parseUnimodStyleFormulaTest1()
        {
            string formula = @"H(-3) N(-1)";

            var formulaDictionary = EmpiricalFormulaUtilities.ParseEmpiricalFormulaString(formula, true);


            Assert.AreEqual(-3,formulaDictionary["H"]);
            Assert.AreEqual(-1, formulaDictionary["N"]);


            var parsedFormula = EmpiricalFormulaUtilities.GetEmpiricalFormulaFromElementTable(formulaDictionary);

            

        }

        [Test]
        public void parseUnimodStyleFormulaTest2()
        {
            string formula = @"H26 2H(8) C20 N4 O5 S";

            var formulaDictionary = EmpiricalFormulaUtilities.ParseEmpiricalFormulaString(formula, true);


            Assert.AreEqual(8, formulaDictionary["2H"]);
            Assert.AreEqual(26, formulaDictionary["H"]);
            Assert.AreEqual(1, formulaDictionary["S"]);


            var parsedFormula = EmpiricalFormulaUtilities.GetEmpiricalFormulaFromElementTable(formulaDictionary);



        }

        [Test]
        public void parseUnimodStyleFormulaTest3()
        {
            string formula = @"H32 C34 N4 O4 Fe";

            var formulaDictionary = EmpiricalFormulaUtilities.ParseEmpiricalFormulaString(formula, true);

            Assert.AreEqual(34, formulaDictionary["C"]);
            Assert.AreEqual(32, formulaDictionary["H"]);
            Assert.AreEqual(1, formulaDictionary["Fe"]);

        }

        [Test]
        public void parseUnimodStyleFormulaTest4()
        {
            string formula = @"H32C34N4O4Fe";

            var formulaDictionary = EmpiricalFormulaUtilities.ParseEmpiricalFormulaString(formula, true);

            Assert.AreEqual(34, formulaDictionary["C"]);
            Assert.AreEqual(32, formulaDictionary["H"]);
            Assert.AreEqual(1, formulaDictionary["Fe"]);

        }


        [Test]
        public void addUnimodFormulaTest1()
        {
            string testPeptide = "SAMPLER";

            PeptideUtils peptideUtils=new PeptideUtils();
            string baseFormula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(testPeptide);

            string mod = "H3 C2 N O";
            var modFormula = EmpiricalFormulaUtilities.AddFormula(baseFormula, mod);

            Console.WriteLine("Unmodified peptide= "+ baseFormula);
            Console.WriteLine("Modified peptide= " + modFormula);

            Assert.AreEqual("C35H61N11O12S",modFormula);
           
        }

        [Test]
        public void addUnimodFormulaTest2()
        {
            string testPeptide = "SAMPLER";

            PeptideUtils peptideUtils = new PeptideUtils();
            string baseFormula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(testPeptide);

            string mod = "H(-3) N(-1)";
            var modFormula = EmpiricalFormulaUtilities.AddFormula(baseFormula, mod);

            Console.WriteLine("Unmodified peptide= " + baseFormula);
            Console.WriteLine("Modified peptide= " + modFormula);

            Assert.AreEqual("C33H55N9O11S", modFormula);

        }


        [Test]
        public void GetMonoisotopicMassFromEmpiricalFormulaTest1()
        {
            string testPeptide = "SAMPLER";
            PeptideUtils peptideUtils = new PeptideUtils();
            string formula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(testPeptide);

            double monomass= EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(formula);

            Assert.IsTrue(monomass > 0);
            Console.WriteLine("SAMPLER monoisotopic mass= \t"+ monomass);

            Assert.AreEqual(802.40072m, (decimal)Math.Round(monomass, 5));    //note that Peptide Util reports 802.40071, as does MacCoss's lab: http://proteome.gs.washington.edu/cgi-bin/aa_calc.pl
        }


        [Test]
        public void AddAcetylationTest1()
        {
            string testPeptide = "SAMPLER";
            PeptideUtils peptideUtils = new PeptideUtils();
            string formula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(testPeptide);

            string acetylationFormula = "C2H2O";

           string empiricalFormula = EmpiricalFormulaUtilities.AddFormula(formula, acetylationFormula);

            double massUnmodified = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(formula);
            double massModified = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(empiricalFormula);

           Console.WriteLine(formula + "\t" + massUnmodified);
           Console.WriteLine(empiricalFormula + "\t" + massModified);

            Console.WriteLine("diff= " + (massModified - massUnmodified));

        }

    }
}
