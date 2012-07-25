using System.Text;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.FileIO
{
	public class TopDownTargetedResultToTextExporter : TargetedResultToTextExporter
	{
		public TopDownTargetedResultToTextExporter(string filename)
            : base(filename)
        {

        }

		#region Private Methods

		protected override string addBasicTargetedResult(TargetedResultDTO result)
		{
			var topDownResult = (TopDownTargetedResultDTO)result;

			var sb = new StringBuilder();

			var prsmList = new StringBuilder();
			foreach (var prsm in topDownResult.PrsmList)
			{
				if (prsmList.Length > 0) prsmList.Append(", ");
				prsmList.Append(prsm);
			}
			var chargeList = new StringBuilder();
			foreach (var charge in topDownResult.ChargeList)
			{
				if (chargeList.Length > 0) chargeList.Append(", ");
				chargeList.Append(charge);
			}

			sb.Append(topDownResult.MatchedMassTagID);
			sb.Append(Delimiter);
			sb.Append(prsmList.ToString());
			sb.Append(Delimiter);
			sb.Append(chargeList.ToString());
			sb.Append(Delimiter);
			sb.Append(topDownResult.ProteinName);
			sb.Append(Delimiter);
			sb.Append(topDownResult.ProteinMass);
			sb.Append(Delimiter);
			sb.Append(topDownResult.PeptideSequence);
			sb.Append(Delimiter);
			sb.Append(topDownResult.EmpiricalFormula);
			sb.Append(Delimiter);
			sb.Append(topDownResult.Quantitation);
			sb.Append(Delimiter);
			sb.Append(topDownResult.ScanLC);
			sb.Append(Delimiter);
			sb.Append(topDownResult.FailureType);
			sb.Append(Delimiter);
			sb.Append(topDownResult.ErrorDescription);
			
			return sb.ToString();
		}


		protected override string buildHeaderLine()
		{
			var sb = new StringBuilder();

			sb.Append("Prsm_ID");
			sb.Append(Delimiter);
			sb.Append("Prsm_list");
			sb.Append(Delimiter);
			sb.Append("Charge_list");
			sb.Append(Delimiter);
			sb.Append("Protein_name");
			sb.Append(Delimiter);
			sb.Append("Protein_mass");
			sb.Append(Delimiter);
			sb.Append("Peptide");
			sb.Append(Delimiter);
			sb.Append("Empirical_formula");
			sb.Append(Delimiter);
			sb.Append("Quantitation");
			sb.Append(Delimiter);
			sb.Append("Most_abundant_charge_apex_scan");
			sb.Append(Delimiter);
			sb.Append("Failure_type");
			sb.Append(Delimiter);
			sb.Append("Error_description");

			return sb.ToString();

		}

		#endregion

	}
}
