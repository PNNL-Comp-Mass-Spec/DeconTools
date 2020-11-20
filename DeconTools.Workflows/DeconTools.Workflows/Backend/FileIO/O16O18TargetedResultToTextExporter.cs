using System.Collections.Generic;
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

            var data = new List<string>
            {
                o16o18result.IntensityTheorI0.ToString("0.000"),
                o16o18result.IntensityTheorI2.ToString("0.000"),
                o16o18result.IntensityTheorI4.ToString("0.000"),
                o16o18result.IntensityI0.ToString("0.000"),
                o16o18result.IntensityI2.ToString("0.000"),
                o16o18result.IntensityI4.ToString("0.000"),
                o16o18result.IntensityI4Adjusted.ToString("0.000"),
                o16o18result.ChromCorrO16O18SingleLabel.ToString("0.000"),
                o16o18result.ChromCorrO16O18DoubleLabel.ToString("0.000"),
                o16o18result.Ratio.ToString("0.0000"),
                o16o18result.RatioFromChromCorr.ToString("0.0000")
            };

            return string.Join(Delimiter.ToString(), data);
        }

        protected override string buildHeaderLine()
        {
            var data = new List<string>
            {
                base.buildHeaderLine(),
                "IntensityTheorI0",
                "IntensityTheorI2",
                "IntensityTheorI4",
                "IntensityI0",
                "IntensityI2",
                "IntensityI4",
                "IntensityI4Adjusted",
                "ChromCorrO16O18SingleLabel",
                "ChromCorrO16O18DoubleLabel",
                "Ratio",
                "RatioFromChromCorr"
            };

            return string.Join(Delimiter.ToString(), data);
        }

        #endregion

    }
}
