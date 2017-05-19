using DeconTools.Backend.Utilities;
using NUnit.Framework;
using System;
using ProteinCalc;

namespace DeconTools.UnitTesting2.Utilities
{
    [TestFixture]
    public class PeptideUtilsTests
    {
        [Test]
        public void GetEmpiricalFormulaForAminoAcidTest1()
        {
            var utils = new PeptideUtils();

            var testAminoAcid = 'A';
            Assert.AreEqual("C3H5NO", utils.GetEmpiricalFormulaForAminoAcidResidue(testAminoAcid));
        }

        [Test]
        public void GetEmpiricalFormulaForPeptideSequenceTest1()
        {
            var utils = new PeptideUtils();

            var testSequence1 = "SAMPLER";
            var testSequence2 = "GRDIESLYSR";
            var testSequence3 = "ANKYLSRRH";
            var testSequence4 = "ARNDCEQGHILKMFPSTWYV";

            Assert.AreEqual("C33H58N10O11S", utils.GetEmpiricalFormulaForPeptideSequence(testSequence1));
            Assert.AreEqual(802.40071m, (decimal)Math.Round(utils.GetMonoIsotopicMassForPeptideSequence(testSequence1), 5));
            Assert.AreEqual(1194.59929m, (decimal)Math.Round(utils.GetMonoIsotopicMassForPeptideSequence(testSequence2), 5));

            Assert.AreEqual("C107H159N29O30S2", utils.GetEmpiricalFormulaForPeptideSequence(testSequence4));
           

            Console.WriteLine(utils.GetEmpiricalFormulaForPeptideSequence(testSequence3));

            //note: masses were also confirmed at MacCoss's site: http://proteome.gs.washington.edu/cgi-bin/aa_calc.pl
        }

        [Test]
        public void GetEmpiricalFormulaForPeptideSequenceTest2()
        {
            var utils = new PeptideUtils();

            var testSequence1 = "IPNFWVTTFVNHPQVSALLGEEDEEALHYLTR";

            var testSequence2 = "K.IPNFWVTTFVNHPQVSALLGEEDEEALHYLTR*.V";


            var empiricalFormula1 = utils.GetEmpiricalFormulaForPeptideSequence(testSequence1);

            var empiricalFormula2 = utils.GetEmpiricalFormulaForPeptideSequence(testSequence2);


            Assert.AreEqual(empiricalFormula1, empiricalFormula2);
            Assert.AreEqual((decimal)Math.Round(utils.GetMonoIsotopicMassForPeptideSequence(testSequence1), 5), (decimal)Math.Round(utils.GetMonoIsotopicMassForPeptideSequence(testSequence2), 5));
        
        }




    }
}
