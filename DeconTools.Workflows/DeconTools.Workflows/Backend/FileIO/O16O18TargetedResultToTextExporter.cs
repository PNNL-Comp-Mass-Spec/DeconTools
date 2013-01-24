using System.Text;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class O16O18TargetedResultToTextExporter : TargetedResultToTextExporter
    {

        public O16O18TargetedResultToTextExporter(string filename)
            : base(filename)
        {

        }


        #region Private Methods

        protected override string addAdditionalInfo(TargetedResultDTO result)
        {

            var o16o18result = (O16O18TargetedResultDTO)result;

            StringBuilder sb = new StringBuilder();
            sb.Append(Delimiter);
            sb.Append(o16o18result.IntensityTheorI0);
            sb.Append(Delimiter);
            sb.Append(o16o18result.IntensityTheorI2);
            sb.Append(Delimiter);
            sb.Append(o16o18result.IntensityTheorI4);
            sb.Append(Delimiter);
            sb.Append(o16o18result.IntensityI0);
            sb.Append(Delimiter);
            sb.Append(o16o18result.IntensityI2);
            sb.Append(Delimiter);
            sb.Append(o16o18result.IntensityI4);
            sb.Append(Delimiter);
            sb.Append(o16o18result.IntensityI4Adjusted);
            sb.Append(Delimiter);
            sb.Append(o16o18result.ChromCorrO16O18SingleLabel.ToString("0.000"));
            sb.Append(Delimiter);
            sb.Append(o16o18result.ChromCorrO16O18DoubleLabel.ToString("0.000"));
            sb.Append(Delimiter);
            sb.Append(o16o18result.Ratio.ToString("0.0000"));
            sb.Append(Delimiter);
            sb.Append(o16o18result.RatioFromChromCorr.ToString("0.0000"));
            return sb.ToString();

        }


        protected override string buildHeaderLine()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(base.buildHeaderLine());

            sb.Append(Delimiter);
            sb.Append("IntensityTheorI0");
            sb.Append(Delimiter);
            sb.Append("IntensityTheorI2");
            sb.Append(Delimiter);
            sb.Append("IntensityTheorI4");
            sb.Append(Delimiter);

            sb.Append("IntensityI0");
            sb.Append(Delimiter);
            sb.Append("IntensityI2");
            sb.Append(Delimiter);
            sb.Append("IntensityI4");
            sb.Append(Delimiter);
            sb.Append("IntensityI4Adjusted");
            sb.Append(Delimiter);
            sb.Append("ChromCorrO16O18SingleLabel");
            sb.Append(Delimiter);
            sb.Append("ChromCorrO16O18DoubleLabel");
            sb.Append(Delimiter);
            sb.Append("Ratio");
            sb.Append(Delimiter);
            sb.Append("RatioFromChromCorr");
            return sb.ToString();

        }

        #endregion

    }
}
