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
			IqCodeParser parser = new IqCodeParser();
			string msgfexample = "+144.102PRYRK+144.102RTPVSLY+79.966QK+144.102T+79.966PNGEK+144.102PYEC+57.021GEC+57.021GK+144.102-202";

			double Mass = parser.PtmMassFromCode(msgfexample);
			Assert.AreEqual(792.484, Mass, 0.0005);
		}

		//Test string is a real example of MSAlign a sequence with both positive and negative PTMs
		//Checks the programs calculated value against a hand calculated value.
		[Test]
		public void PtmMassFromCodeMsAlignTest()
		{
			IqCodeParser parser = new IqCodeParser();
			string msalignexample = "A.AENVVHHKLDGMPISEAVEINAGNNLVF(LSGKVPTKKSADAPEGELASYGNTE)[-713.72]EQTINVLEQIKTNLNNLGLDMKDVVKMQVFLVGGEENNGTMDFKGFMNGYSKFYDASKTNQLPARSAFQVA(K)[1.02]LANPAWRVEIEVIAVRPAK.";

			double Mass = parser.PtmMassFromCode(msalignexample);
			Assert.AreEqual(-712.7, Mass, 0.0005);
		}

		//Test string with PTMs, PTM mass, and base string are all calculated individually. The PTM mass formula is subtracted from the test string with PTMs and checked against the base formula.
		[Test]
		public void MsgfCodeParserTest()
		{
			IqCodeParser parser = new IqCodeParser();
			string examplemod =
				"+144.102PRYRK+144.102RTPVSLY+79.966QK+144.102T+79.966PNGEK+144.102PYEC+57.021GEC+57.021GK+144.102-202";
			string checkmass = "+792.484";
			string examplenomod = "PRYRKRTPVSLYQKTPNGEKPYECGECGK";

			string examplemodresult = parser.GetEmpiricalFormulaFromSequence(examplemod);
			string checkmassresult = parser.GetEmpiricalFormulaFromSequence(checkmass);
			string examplenomodresult = parser.GetEmpiricalFormulaFromSequence(examplenomod);

			Console.WriteLine(examplemodresult);
			Console.WriteLine(checkmassresult);
			Console.WriteLine(examplenomodresult);

			string difference = EmpiricalFormulaUtilities.SubtractFormula(examplemodresult, checkmassresult);

			Assert.AreEqual(examplenomodresult, difference);
		}

		//Test string with PTMs, PTM mass, and base string are all calculated individually. The PTM mass formula is subtracted from the test string with PTMs and checked against the base formula.
		[Test]
		public void MsAlignCodeParserTest()
		{
			IqCodeParser parser = new IqCodeParser();
			string examplemod =
				"A.AENVVHHKLDGMPISEAVEINAGNNLVF(LSGKVPTKKSADAPEGELASYGNTE)[713.72]EQTINVLEQIKTNLNNLGLDMKDVVKMQVFLVGGEENNGTMDFKGFMNGYSKFYDASKTNQLPARSAFQVA(K)[1.02]LANPAWRVEIEVIAVRPAK.";
			string checkmass = "[714.74]";
			string examplenomod = "A.AENVVHHKLDGMPISEAVEINAGNNLVLSGKVPTKKSADAPEGELASYGNTEFEQTINVLEQIKTNLNNLGLDMKDVVKMQVFLVGGEENNGTMDFKGFMNGYSKFYDASKTNQLPARSAFQVAKLANPAWRVEIEVIAVRPAK.";

			string examplemodresult = parser.GetEmpiricalFormulaFromSequence(examplemod);
			string checkmassresult = parser.GetEmpiricalFormulaFromSequence(checkmass);
			string examplenomodresult = parser.GetEmpiricalFormulaFromSequence(examplenomod);

			Console.WriteLine(examplemodresult);
			Console.WriteLine(checkmassresult);
			Console.WriteLine(examplenomodresult);

			string difference = EmpiricalFormulaUtilities.SubtractFormula(examplemodresult, checkmassresult);

			Assert.AreEqual(examplenomodresult, difference);
		}

		//Verify the sequences empirical formula with an empirical formula calculated by an external tool
        [Test]
        public void EmpiricalFormulaCalculatorTest()
        {
			IqCodeParser parser = new IqCodeParser();

            string exampleSequence = @"A.ADLEDNMDILNDNLKVVEKTDSAPELKAALTKMRAAALDAQKATPPKLEDKAPDSPEMKDFRHGFDILVGQIDGALKLANEGNVKEAKAAAEALKTTRNTYHKKYR.";
            string empiricalFormula = parser.GetEmpiricalFormulaFromSequence(exampleSequence);
            Console.WriteLine(empiricalFormula);
            Assert.AreEqual("C507H832N144O163S3", empiricalFormula);
        }

		//Test case: Sequence generated a very unusal Empirical Formula with 1E-5 Sulfur. Empirical Formula was tricked into thinking E was an element
		[Test]
		public void EmpiricalFormulaCalculatorRoundingTest()
		{
			IqCodeParser parser = new IqCodeParser();

			string testSequence = @".MITGIQITKA(AN)[1.02]DDLLNSFWLLDSEKGEARCIVAKAGYAEDEVVAVSKLGDIEYREVPVEVKPEVRVEGGQHLNVNVLRRETLEDAVKHPEKYPQLTI(RV)[-.99]S.G";

			string empiricalFormula = parser.GetEmpiricalFormulaFromSequence(testSequence);
			Console.WriteLine(empiricalFormula);
			Assert.IsTrue(!empiricalFormula.Contains("E"));
		}

	}
}
