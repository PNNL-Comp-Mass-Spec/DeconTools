using System.Collections.Generic;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class DeuteratedTargetedResultToTextExporter : TargetedResultToTextExporter
    {
        public DeuteratedTargetedResultToTextExporter(string filename)
            : base(filename)
        {
        }

        #region Private Methods

        protected override string addAdditionalInfo(TargetedResultDTO result)
        {
            var deuteratedResult = (DeuteratedTargetedResultDTO)result;

            var data = new List<string>
            {
                deuteratedResult.HydrogenI0.ToString("0.000"),
                deuteratedResult.HydrogenI1.ToString("0.000"),
                deuteratedResult.HydrogenI2.ToString("0.000"),
                deuteratedResult.HydrogenI3.ToString("0.000"),
                deuteratedResult.HydrogenI4.ToString("0.000"),
                "0",            // DeuteriumI
                deuteratedResult.DeuteriumI0.ToString("0.000"),
                deuteratedResult.DeuteriumI1.ToString("0.000"),
                deuteratedResult.DeuteriumI2.ToString("0.000"),
                deuteratedResult.DeuteriumI3.ToString("0.000"),
                deuteratedResult.DeuteriumI4.ToString("0.000"),
                deuteratedResult.TheoryI0.ToString("0.000"),
                deuteratedResult.TheoryI1.ToString("0.000"),
                deuteratedResult.TheoryI2.ToString("0.000"),
                deuteratedResult.TheoryI3.ToString("0.000"),
                deuteratedResult.TheoryI4.ToString("0.000"),
                deuteratedResult.RawI0.ToString("0.000"),
                deuteratedResult.RawI1.ToString("0.000"),
                deuteratedResult.RawI2.ToString("0.000"),
                deuteratedResult.RawI3.ToString("0.000"),
                deuteratedResult.RawI4.ToString("0.000"),
                deuteratedResult.LabelingEfficiency.ToString("0.000"),
                deuteratedResult.RatioDH.ToString("0.0000"),
            };

            return string.Join(Delimiter.ToString(), data);
        }

        protected override string buildHeaderLine()
        {
            var data = new List<string>
            {
                base.buildHeaderLine(),
                "HydrogenI0",
                "HydrogenI1",
                "HydrogenI2",
                "HydrogenI3",
                "HydrogenI4",
                "DeuteriumI",
                "DeuteriumI0",
                "DeuteriumI1",
                "DeuteriumI2",
                "DeuteriumI3",
                "DeuteriumI4",
                "TheoryI0",
                "TheoryI1",
                "TheoryI2",
                "TheoryI3",
                "TheoryI4",
                "RawI0",
                "RawI1",
                "RawI2",
                "RawI3",
                "RawI4",
                "LabelingEfficiency",
                "RatioDH",
                "IntegratedLcAbundance"
            };

            return string.Join(Delimiter.ToString(), data);
        }

        #endregion

    }
}
