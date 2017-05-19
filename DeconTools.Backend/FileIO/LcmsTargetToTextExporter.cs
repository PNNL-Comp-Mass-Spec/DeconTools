using System.Text;
using BrukerDataReader;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.FileIO
{
    public class LcmsTargetToTextExporter : TextFileExporter<TargetBase>
    {

        #region Constructors

        public LcmsTargetToTextExporter(string fileName):base(fileName,'\t'){}

        public LcmsTargetToTextExporter(string fileName, char delimiter) : base(fileName, delimiter) { }


        #endregion


        protected override string buildResultOutput(TargetBase target)
        {
            var sb = new StringBuilder();
            Check.Require(target is LcmsFeatureTarget, "Exported result is of the wrong type.");

            var lcmsFeature = (LcmsFeatureTarget)target;

            sb.Append(lcmsFeature.ID);
            sb.Append(Delimiter);
            sb.Append(lcmsFeature.FeatureToMassTagID);
            sb.Append(Delimiter);
            sb.Append(lcmsFeature.EmpiricalFormula);
            sb.Append(Delimiter);
            sb.Append(target.Code);
            sb.Append(Delimiter);
            sb.Append(lcmsFeature.MonoIsotopicMass);
            sb.Append(Delimiter);
            sb.Append(lcmsFeature.ChargeState);
            sb.Append(Delimiter);
            sb.Append(lcmsFeature.MZ);
            sb.Append(Delimiter);
            sb.Append(lcmsFeature.ModCount);
            sb.Append(Delimiter);
            sb.Append(lcmsFeature.ModDescription);
            sb.Append(Delimiter);
            sb.Append(lcmsFeature.ScanLCTarget);
            sb.Append(Delimiter);
            sb.Append(lcmsFeature.NormalizedElutionTime);
            

            return sb.ToString();
        }

        protected override string buildHeaderLine()
        {
            return "TargetID\tReferenceID\tEmpiricalFormula\tSequence\tMonoisotopicMass\tZ\tMZ\tModCount\tMod\tScan\tNET";
        }
    }
}
