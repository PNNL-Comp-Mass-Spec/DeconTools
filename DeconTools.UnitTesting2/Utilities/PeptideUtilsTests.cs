﻿using DeconTools.Backend.Utilities;
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
            PeptideUtils utils = new PeptideUtils();

            char testAminoAcid = 'A';
            Assert.AreEqual("C3H5NO", utils.GetEmpiricalFormulaForAminoAcidResidue(testAminoAcid));
        }

        [Test]
        public void GetEmpiricalFormulaForPeptideSequenceTest1()
        {
            PeptideUtils utils = new PeptideUtils();

            string testSequence1 = "SAMPLER";
            string testSequence2 = "GRDIESLYSR";

            Assert.AreEqual("C33H58N10O11S", utils.GetEmpiricalFormulaForPeptideSequence(testSequence1));
            Assert.AreEqual(802.40071, Math.Round(utils.GetMonoIsotopicMassForPeptideSequence(testSequence1), 5));
            Assert.AreEqual(1194.59928, Math.Round(utils.GetMonoIsotopicMassForPeptideSequence(testSequence2), 5));

            //note: masses were also confirmed at MacCoss's site: http://proteome.gs.washington.edu/cgi-bin/aa_calc.pl
        }






    }
}
