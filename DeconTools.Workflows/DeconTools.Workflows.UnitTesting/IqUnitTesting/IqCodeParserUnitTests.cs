using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Utilities.IqCodeParser;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting.IqUnitTesting
{
    class IqCodeParserUnitTests
    {
        //Reference JIRA: https://jira.pnnl.gov/browse/OMCR-183

        //Test string is a real example of MSGF a sequence with both positive and negative PTMs
        //Checks the programs calculated value against a hand calculated value.
        [Test]
        public void PtmMassFromCodeMsgfTest ()
        {
            var parser = new IqCodeParser();
            var msgfexample = "+144.102PRYRK+144.102RTPVSLY+79.966QK+144.102T+79.966PNGEK+144.102PYEC+57.021GEC+57.021GK+144.102-202";

            var Mass = parser.PtmMassFromCode(msgfexample);
            Assert.AreEqual(792.484, Mass, 0.0005);
        }

        //Test string is a real example of MSAlign a sequence with both positive and negative PTMs
        //Checks the programs calculated value against a hand calculated value.
        [Test]
        public void PtmMassFromCodeMsAlignTest()
        {
            var parser = new IqCodeParser();
            var msalignexample = "A.AENVVHHKLDGMPISEAVEINAGNNLVF(LSGKVPTKKSADAPEGELASYGNTE)[-713.72]EQTINVLEQIKTNLNNLGLDMKDVVKMQVFLVGGEENNGTMDFKGFMNGYSKFYDASKTNQLPARSAFQVA(K)[1.02]LANPAWRVEIEVIAVRPAK.";

            var Mass = parser.PtmMassFromCode(msalignexample);
            Assert.AreEqual(-712.7, Mass, 0.0005);
        }

        //Test string with PTMs, PTM mass, and base string are all calculated individually. The PTM mass formula is subtracted from the test string with PTMs and checked against the base formula.
        [Test]
        public void MsgfCodeParserTest()
        {
            var parser = new IqCodeParser();
            var examplemod =
                "+144.102PRYRK+144.102RTPVSLY+79.966QK+144.102T+79.966PNGEK+144.102PYEC+57.021GEC+57.021GK+144.102-202";
            var checkmass = "+792.484";
            var examplenomod = "PRYRKRTPVSLYQKTPNGEKPYECGECGK";

            var examplemodresult = parser.GetEmpiricalFormulaFromSequence(examplemod);
            var checkmassresult = parser.GetEmpiricalFormulaFromSequence(checkmass);
            var examplenomodresult = parser.GetEmpiricalFormulaFromSequence(examplenomod);

            Console.WriteLine(examplemodresult);
            Console.WriteLine(checkmassresult);
            Console.WriteLine(examplenomodresult);

            var difference = EmpiricalFormulaUtilities.SubtractFormula(examplemodresult, checkmassresult);

            Assert.AreEqual(examplenomodresult, difference);
        }

        //Test string with PTMs, PTM mass, and base string are all calculated individually. The PTM mass formula is subtracted from the test string with PTMs and checked against the base formula.
        [Test]
        public void MsAlignCodeParserTest()
        {
            var parser = new IqCodeParser();
            var examplemod =
                "A.AENVVHHKLDGMPISEAVEINAGNNLVF(LSGKVPTKKSADAPEGELASYGNTE)[713.72]EQTINVLEQIKTNLNNLGLDMKDVVKMQVFLVGGEENNGTMDFKGFMNGYSKFYDASKTNQLPARSAFQVA(K)[1.02]LANPAWRVEIEVIAVRPAK.";
            var checkmass = "[714.74]";
            var examplenomod = "A.AENVVHHKLDGMPISEAVEINAGNNLVLSGKVPTKKSADAPEGELASYGNTEFEQTINVLEQIKTNLNNLGLDMKDVVKMQVFLVGGEENNGTMDFKGFMNGYSKFYDASKTNQLPARSAFQVAKLANPAWRVEIEVIAVRPAK.";

            var examplemodresult = parser.GetEmpiricalFormulaFromSequence(examplemod);
            var checkmassresult = parser.GetEmpiricalFormulaFromSequence(checkmass);
            var examplenomodresult = parser.GetEmpiricalFormulaFromSequence(examplenomod);

            Console.WriteLine(examplemodresult);
            Console.WriteLine(checkmassresult);
            Console.WriteLine(examplenomodresult);

            var difference = EmpiricalFormulaUtilities.SubtractFormula(examplemodresult, checkmassresult);

            Assert.AreEqual(examplenomodresult, difference);
        }

        //Verify the sequences empirical formula with an empirical formula calculated by an external tool
        [Test]
        public void EmpiricalFormulaCalculatorTest()
        {
            var parser = new IqCodeParser();

            var exampleSequence = @"A.ADLEDNMDILNDNLKVVEKTDSAPELKAALTKMRAAALDAQKATPPKLEDKAPDSPEMKDFRHGFDILVGQIDGALKLANEGNVKEAKAAAEALKTTRNTYHKKYR.";
            var empiricalFormula = parser.GetEmpiricalFormulaFromSequence(exampleSequence);
            Console.WriteLine(empiricalFormula);
            Assert.AreEqual("C507H832N144O163S3", empiricalFormula);
        }

        //Test case: Sequence generated a very unusal Empirical Formula with 1E-5 Sulfur. Empirical Formula was tricked into thinking E was an element
        [Test]
        public void EmpiricalFormulaCalculatorRoundingTest()
        {
            var parser = new IqCodeParser();

            var testSequence = @".MITGIQITKA(AN)[1.02]DDLLNSFWLLDSEKGEARCIVAKAGYAEDEVVAVSKLGDIEYREVPVEVKPEVRVEGGQHLNVNVLRRETLEDAVKHPEKYPQLTI(RV)[-.99]S.G";

            var empiricalFormula = parser.GetEmpiricalFormulaFromSequence(testSequence);
            Console.WriteLine(empiricalFormula);
            Assert.IsTrue(!empiricalFormula.Contains("E"));
        }

        [Test]
        public void AveragineRoundTripTest()
        {
            var parser = new IqCodeParser();

            var ptmDouble = -11849.17;
            var ptmMass = "[" + ptmDouble.ToString() + "]"; // this is done just for formatting of the function call below
            var absPtmDouble = Math.Abs(ptmDouble); //this is done because the emperical formula returns a positive amount of atoms (not negative)
                    //so in the assert comparison we have to use the positive mass value

            var empiricalFormula = parser.GetEmpiricalFormulaFromSequence(ptmMass);
        
            Console.WriteLine("This is my emperical formula:" + empiricalFormula + ":");

            var returnedMass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(empiricalFormula);

            Console.WriteLine(empiricalFormula);
            Console.WriteLine(returnedMass);
            Assert.AreEqual(absPtmDouble, returnedMass, .0001);

        }

        //Test case: this is to test the accuracy of the averagine atomic formula conversion with very large PTMs
        [Test]
        public void ProteinSequenceToMassHugePTMTest()
        {
            var parser = new IqCodeParser();

            var proteoform =
                "M.V(HLTPEEKSAVTALWGKVNVDEVGGEALGRLLVVYPWTQRFFESFGDLSTPDAVMGNPKVKAHGKKVLGAFSDGLAHLDNLKGTFATLSELHCDKLHVDPENFRLLGNVLVC)[-11849.17]VLAHHFGKEFTPPVQAAYQKVVAGVANALAHKYH.";
            var trueMass = 4008.08; // only one significant decimal really.  could be 4008.07 or .08 Can't tell yet
            var unmodifiedProteoform =
                "M.VHLTPEEKSAVTALWGKVNVDEVGGEALGRLLVVYPWTQRFFESFGDLSTPDAVMGNPKVKAHGKKVLGAFSDGLAHLDNLKGTFATLSELHCDKLHVDPENFRLLGNVLVCVLAHHFGKEFTPPVQAAYQKVVAGVANALAHKYH.";
            var ptm = "[-11849.17]";

            var proteoformComposition = parser.GetEmpiricalFormulaFromSequence(proteoform);

            var unmodifiedProteoformComposition = parser.GetEmpiricalFormulaFromSequence(unmodifiedProteoform);

            var ptmComposition = parser.GetEmpiricalFormulaFromSequence(ptm);

            var difference = EmpiricalFormulaUtilities.SubtractFormula(unmodifiedProteoformComposition, ptmComposition);

            Console.WriteLine(proteoformComposition);
            Console.WriteLine(difference);

            Assert.AreEqual(proteoformComposition, difference);

            var differenceMass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(difference);
            var proteformMass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(proteoformComposition);

            var unmodifiedProteoformMass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(unmodifiedProteoformComposition);
            Console.WriteLine(unmodifiedProteoformMass);
            var ptmMass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(ptmComposition);
            Console.WriteLine(ptmMass);

            var conversionFirst = unmodifiedProteoformMass - ptmMass;
            Console.WriteLine(conversionFirst);

            Assert.AreEqual(trueMass, conversionFirst, 0.1);
            Assert.AreEqual(trueMass, differenceMass, .1);
            Assert.AreEqual(trueMass, proteformMass, .1);
        }

    }
}
