using System.Text;
using BrukerDataReader;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.FileIO
{
    public class MassTagTextFileExporter : TextFileExporter<TargetBase>
    {

        #region Constructors
        public MassTagTextFileExporter(string fileName):base(fileName,'\t'){}

        public MassTagTextFileExporter(string fileName, char delimiter):base(fileName,delimiter){}

        #endregion

        protected override string buildResultOutput(TargetBase target)
        {
            var sb = new StringBuilder();
            Check.Require(target is PeptideTarget, "Exported result is of the wrong type.");

            var result = (PeptideTarget)target;

            sb.Append(target.ID);
            sb.Append(this.Delimiter);
            sb.Append(target.MonoIsotopicMass);
            sb.Append(this.Delimiter); 
            sb.Append(target.Code);
            sb.Append(this.Delimiter); 
            sb.Append(target.ChargeState);
            sb.Append(Delimiter);
            sb.Append(target.EmpiricalFormula);
            sb.Append(Delimiter);
            sb.Append(target.ModCount);
            sb.Append(Delimiter);
            sb.Append(target.ModDescription);
            sb.Append(this.Delimiter); 
            sb.Append(target.ObsCount);
            sb.Append(this.Delimiter); 
            sb.Append(target.MZ);
            sb.Append(this.Delimiter); 
            sb.Append(target.NormalizedElutionTime);
            sb.Append(this.Delimiter);
            sb.Append(result.RefID);
            sb.Append(this.Delimiter);
            sb.Append(result.GeneReference);
            sb.Append(Delimiter);
            sb.Append(result.ProteinDescription);

            return sb.ToString();
        }

        protected override string buildHeaderLine()
        {
            return "ID\tMonoisotopic_Mass\tSequence\tZ\tFormula\tModCount\tMod\tObsCount\tMZ\tNET\tRef_ID\tReference\tDescription";
        }
    }
}
