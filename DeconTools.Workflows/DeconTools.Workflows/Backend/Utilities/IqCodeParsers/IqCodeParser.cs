using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;

namespace DeconTools.Workflows.Backend.Utilities.IqCodeParsers
{
	public abstract class IqCodeParser
	{

		#region Constructors
		
		public IqCodeParser()
		{
			PeptideUtils = new PeptideUtils();
		}

		#endregion

		#region Properties

		//Regular expression that finds PTMs in a specified file format
		protected string PTMExpression { get; set; }

		//Regular expression that finds the unmodified sequence in a specified file format
		protected string SequenceExpression { get; set; }

		protected PeptideUtils PeptideUtils { get; set; }

		#endregion

		#region Public Methods

		//Returns an empirical formula string with or without PTMs.
		//Accounts for the PTMs using the Averagine formula.
		//Parses out the PTMs and calculates the empirical formula for the known unmodified sequence.
		//Adds or subtracts the PTM formula from the sequence formula based on the overall mass of the PTMs
		public string GetEmpiricalFormulaFromSequence(string code)
		{
			string PTM_Formula = "";
			string SequenceFormula = "";
			string EmpiricalFormula = "";

			double ptm_mass = PTMMassFromCode(code);
			PTM_Formula = IsotopicDistributionCalculator.Instance.GetAveragineFormulaAsString(Math.Abs(ptm_mass), false);

			SequenceFormula = SequenceToEmpiricalFormula(code);

			if(ptm_mass < 0)
			{
				EmpiricalFormula = EmpiricalFormulaUtilities.SubtractFormula(SequenceFormula, PTM_Formula);
			}
			else
			{
				EmpiricalFormula = EmpiricalFormulaUtilities.AddFormula(SequenceFormula, PTM_Formula);
			}
			return EmpiricalFormula;
		}

		//Extracts the PTM masses from the input seqence.
		//Uses the regex PTMExpression to parse for the PTM in a give input format.
		public double PTMMassFromCode(string code)
		{
			double ptmMass = 0.0;
			MatchCollection masses = Regex.Matches(code, PTMExpression);
			foreach (Match match in masses)
			{
				ptmMass += Convert.ToDouble(match.Value);
			}
			return ptmMass;
		}

		//Extracts the unmodified sequence from the original input sequence. 
		//Uses the regex SequenceExpression to parse for the sequence in a specified format.
		public string SequenceToEmpiricalFormula(string code)
		{
			string sequence = "";
			string[] test = Regex.Split(code, SequenceExpression);
			foreach (string s in test)
			{
				sequence += Regex.Match(s, "^[A-Z]*[A-Z]$").Value;
			}

			return PeptideUtils.GetEmpiricalFormulaForPeptideSequence(sequence);
		}

		#endregion

		#region Private Methods

		#endregion

	}
}
