using System;
using System.Collections.Generic;
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

            if (topDownResult.PrsmList==null)
            {
                throw new NullReferenceException(
                    "Top down exporter failed when trying to process a result. The 'PrsmList' object is null.");
            }

            var prsmList = new StringBuilder();
            foreach (var prsm in topDownResult.PrsmList)
            {
                if (prsmList.Length > 0) prsmList.Append(", ");
                prsmList.Append(prsm);
            }

            var chargeList = new StringBuilder();
            foreach (var charge in topDownResult.ChargeStateList)
            {
                if (chargeList.Length > 0) chargeList.Append(", ");
                chargeList.Append(charge);
            }

            var data = new List<string>
            {
            topDownResult.MatchedMassTagID.ToString(),
            prsmList.ToString(),
            chargeList.ToString(),
            topDownResult.ProteinName,
            topDownResult.ProteinMass.ToString("0.0000"),
            topDownResult.PeptideSequence,
            topDownResult.EmpiricalFormula,
            topDownResult.Quantitation.ToString("0.000"),
            topDownResult.MostAbundantChargeState.ToString(),
            topDownResult.ScanLC.ToString(),
            };

            return string.Join(Delimiter.ToString(), data);
        }


        protected override string buildHeaderLine()
        {
            var data = new List<string>
            {
                "Prsm_ID",
                "Prsm_list",
                "Charge_list",
                "Protein_name",
                "Protein_mass",
                "Peptide",
                "Empirical_formula",
                "Abundance",
                "Most_abundant_charge",
                "Most_abundant_charge_apex_scan",
            };

            return string.Join(Delimiter.ToString(), data);

        }

        #endregion

    }
}
