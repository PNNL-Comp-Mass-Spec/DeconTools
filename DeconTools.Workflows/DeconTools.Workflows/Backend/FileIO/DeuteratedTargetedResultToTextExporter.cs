using System.Text;
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

            var sb = new StringBuilder();
            sb.Append(Delimiter);
            sb.Append(deuteratedResult.HydrogenI0);
            sb.Append(Delimiter);
            sb.Append(deuteratedResult.HydrogenI1);
            sb.Append(Delimiter);
            sb.Append(deuteratedResult.HydrogenI2);
            sb.Append(Delimiter);
            sb.Append(deuteratedResult.HydrogenI3);
            sb.Append(Delimiter);
            sb.Append(deuteratedResult.HydrogenI4);

            sb.Append(Delimiter);
            sb.Append(0);
            sb.Append(Delimiter);
            sb.Append(deuteratedResult.DeuteriumI0);
            sb.Append(Delimiter);
            sb.Append(deuteratedResult.DeuteriumI1);
            sb.Append(Delimiter);
            sb.Append(deuteratedResult.DeuteriumI2);
            sb.Append(Delimiter);
            sb.Append(deuteratedResult.DeuteriumI3);
            sb.Append(Delimiter);
            sb.Append(deuteratedResult.DeuteriumI4);

            sb.Append(Delimiter);
            sb.Append(deuteratedResult.TheoryI0);
            sb.Append(Delimiter);
            sb.Append(deuteratedResult.TheoryI1);
            sb.Append(Delimiter);
            sb.Append(deuteratedResult.TheoryI2);
            sb.Append(Delimiter);
            sb.Append(deuteratedResult.TheoryI3);
            sb.Append(Delimiter);
            sb.Append(deuteratedResult.TheoryI4);

            sb.Append(Delimiter);
            sb.Append(deuteratedResult.RawI0);
            sb.Append(Delimiter);
            sb.Append(deuteratedResult.RawI1);
            sb.Append(Delimiter);
            sb.Append(deuteratedResult.RawI2);
            sb.Append(Delimiter);
            sb.Append(deuteratedResult.RawI3);
            sb.Append(Delimiter);
            sb.Append(deuteratedResult.RawI4);
            sb.Append(Delimiter);
            sb.Append(deuteratedResult.LabelingEfficiency);
            sb.Append(Delimiter);
            sb.Append(deuteratedResult.RatioDH.ToString("0.0000"));

            return sb.ToString();

        }


        protected override string buildHeaderLine()
        {
            var sb = new StringBuilder();

            sb.Append(base.buildHeaderLine());

            sb.Append(Delimiter);
            sb.Append("HydrogenI0");
            sb.Append(Delimiter);
            sb.Append("HydrogenI1");
            sb.Append(Delimiter);
            sb.Append("HydrogenI2");
            sb.Append(Delimiter);
            sb.Append("HydrogenI3");
            sb.Append(Delimiter);
            sb.Append("HydrogenI4");

            sb.Append(Delimiter);
            sb.Append("DeuteriumI");
            sb.Append(Delimiter);
            sb.Append("DeuteriumI0");
            sb.Append(Delimiter);
            sb.Append("DeuteriumI1");
            sb.Append(Delimiter);
            sb.Append("DeuteriumI2");
            sb.Append(Delimiter);
            sb.Append("DeuteriumI3");
            sb.Append(Delimiter);
            sb.Append("DeuteriumI4");

            sb.Append(Delimiter);
            sb.Append("TheoryI0");
            sb.Append(Delimiter);
            sb.Append("TheoryI1");
            sb.Append(Delimiter);
            sb.Append("TheoryI2");
            sb.Append(Delimiter);
            sb.Append("TheoryI3");
            sb.Append(Delimiter);
            sb.Append("TheoryI4");


            sb.Append(Delimiter);
            sb.Append("RawI0");
            sb.Append(Delimiter);
            sb.Append("RawI1");
            sb.Append(Delimiter);
            sb.Append("RawI2");
            sb.Append(Delimiter);
            sb.Append("RawI3");
            sb.Append(Delimiter);
            sb.Append("RawI4");
            sb.Append(Delimiter);

            sb.Append("LabelingEfficiency");
            sb.Append(Delimiter);
            sb.Append("RatioDH");
            sb.Append(Delimiter);
            sb.Append("IntegratedLcAbundance");

            return sb.ToString();

        }

        #endregion

    }
}
