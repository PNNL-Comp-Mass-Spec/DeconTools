using System.Collections.Generic;
using BrukerDataReader;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.FileIO
{
    public class MassTagTextFileExporter : TextFileExporter<TargetBase>
    {

        #region Constructors
        public MassTagTextFileExporter(string fileName) : base(fileName, '\t') { }

        public MassTagTextFileExporter(string fileName, char delimiter) : base(fileName, delimiter) { }

        #endregion

        protected override string buildResultOutput(TargetBase target)
        {
            Check.Require(target is PeptideTarget, "Exported result is of the wrong type.");

            var result = (PeptideTarget)target;

            var data = new List<string>
            {
                target.ID.ToString(),
                DblToString(target.MonoIsotopicMass, 6),
                target.Code,
                target.ChargeState.ToString(),
                target.EmpiricalFormula,
                target.ModCount.ToString(),
                target.ModDescription,
                target.ObsCount.ToString(),
                DblToString(target.MZ, 6),
                DblToString(target.NormalizedElutionTime, 6),
                result.RefID.ToString(),
                result.GeneReference,
                result.ProteinDescription
            };

            return string.Join(Delimiter.ToString(), data);
        }

        protected override string buildHeaderLine()
        {
            var data = new List<string>
            {
                "ID",
                "Monoisotopic_Mass",
                "Sequence",
                "Z",
                "Formula",
                "ModCount",
                "Mod",
                "ObsCount",
                "MZ",
                "NET",
                "Ref_ID",
                "Reference",
                "Description"
            };

            return string.Join(Delimiter.ToString(), data);
        }
    }
}
