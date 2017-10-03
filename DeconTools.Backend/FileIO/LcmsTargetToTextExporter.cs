using System.Collections.Generic;
using BrukerDataReader;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.FileIO
{
    public class LcmsTargetToTextExporter : TextFileExporter<TargetBase>
    {

        #region Constructors

        public LcmsTargetToTextExporter(string fileName) : base(fileName, '\t') { }

        public LcmsTargetToTextExporter(string fileName, char delimiter) : base(fileName, delimiter) { }


        #endregion


        protected override string buildResultOutput(TargetBase target)
        {
            Check.Require(target is LcmsFeatureTarget, "Exported result is of the wrong type.");

            var lcmsFeature = (LcmsFeatureTarget)target;

            var data = new List<string>
            {
                lcmsFeature.ID.ToString(),
                lcmsFeature.FeatureToMassTagID.ToString(),
                lcmsFeature.EmpiricalFormula,
                target.Code,
                lcmsFeature.MonoIsotopicMass.ToString("0.00000"),
                lcmsFeature.ChargeState.ToString(),
                lcmsFeature.MZ.ToString("0.00000"),
                lcmsFeature.ModCount.ToString(),
                lcmsFeature.ModDescription,
                lcmsFeature.ScanLCTarget.ToString(),
                lcmsFeature.NormalizedElutionTime.ToString("0.000"),
            };

            return string.Join(Delimiter.ToString(), data);
        }

        protected override string buildHeaderLine()
        {
            return "TargetID\tReferenceID\tEmpiricalFormula\tSequence\tMonoisotopicMass\tZ\tMZ\tModCount\tMod\tScan\tNET";
        }
    }
}
