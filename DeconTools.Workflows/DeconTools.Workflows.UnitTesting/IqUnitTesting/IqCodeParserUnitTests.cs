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
		public void PTMMassFromCodeMSGFTest ()
		{
			MSGFCodeParser parser = new MSGFCodeParser();
			string msgfexample = "+144.102PRYRK+144.102RTPVSLY+79.966QK+144.102T+79.966PNGEK+144.102PYEC+57.021GEC+57.021GK+144.102-202";

			double Mass = parser.PTMMassFromCode(msgfexample);
			Assert.AreEqual(792.484, Mass, 0.0005);
		}

		//Test string is a real example of MSAlign a sequence with both positive and negative PTMs
		//Checks the programs calculated value against a hand calculated value.
		[Test]
		public void PTMMassFromCodeMSAlignTest()
		{
			MSAlignCodeParser parser = new MSAlignCodeParser();
			string msalignexample = "A.AENVVHHKLDGMPISEAVEINAGNNLVF(LSGKVPTKKSADAPEGELASYGNTE)[-713.72]EQTINVLEQIKTNLNNLGLDMKDVVKMQVFLVGGEENNGTMDFKGFMNGYSKFYDASKTNQLPARSAFQVA(K)[1.02]LANPAWRVEIEVIAVRPAK.";

			double Mass = parser.PTMMassFromCode(msalignexample);
			Assert.AreEqual(-712.7, Mass, 0.0005);
		}

		//Test string with PTMs, PTM mass, and base string are all calculated individually. The PTM mass formula is subtracted from the test string with PTMs and checked against the base formula.
		[Test]
		public void MSGFCodeParserTest()
		{
			MSGFCodeParser parser = new MSGFCodeParser();
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
		public void MSAlignCodeParserTest()
		{
			MSAlignCodeParser parser = new MSAlignCodeParser();
			string examplemod =
				"A.AENVVHHKLDGMPISEAVEINAGNNLVF(LSGKVPTKKSADAPEGELASYGNTE)[713.72]EQTINVLEQIKTNLNNLGLDMKDVVKMQVFLVGGEENNGTMDFKGFMNGYSKFYDASKTNQLPARSAFQVA(K)[1.02]LANPAWRVEIEVIAVRPAK.";
			string checkmass = "[714.74]";
			string examplenomod = "A.AENVVHHKLDGMPISEAVEINAGNNLVFEQTINVLEQIKTNLNNLGLDMKDVVKMQVFLVGGEENNGTMDFKGFMNGYSKFYDASKTNQLPARSAFQVALANPAWRVEIEVIAVRPAK.";

			string examplemodresult = parser.GetEmpiricalFormulaFromSequence(examplemod);
			string checkmassresult = parser.GetEmpiricalFormulaFromSequence(checkmass);
			string examplenomodresult = parser.GetEmpiricalFormulaFromSequence(examplenomod);

			Console.WriteLine(examplemodresult);
			Console.WriteLine(checkmassresult);
			Console.WriteLine(examplenomodresult);

			string difference = EmpiricalFormulaUtilities.SubtractFormula(examplemodresult, checkmassresult);

			Assert.AreEqual(examplenomodresult, difference);
		}

	}
}
