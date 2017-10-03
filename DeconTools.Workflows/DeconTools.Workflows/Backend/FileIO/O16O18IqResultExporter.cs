using System.Collections.Generic;
using DeconTools.Workflows.Backend.Core;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class O16O18IqResultExporter:IqResultExporter
    {

        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        public override string GetHeader()
        {
            var baseHeader = base.GetHeader();

            var data = new List<string>
            {
                baseHeader,
                "FitScoreO18Profile",
                "CorrO16O18Single",
                "CorrO16O18Double",
                "CorrO18SingleToDouble",
                "RatioO16O18Single",
                "RatioO16O18Double"
            };

            return string.Join(Delimiter, data);
        }

        public override string GetResultAsString(IqResult result, bool includeHeader = false)
        {
            var baseResult= base.GetResultAsString(result, includeHeader);
            var o16O18Result = (O16O18IqResult) result;

            var data = new List<string>
            {
                baseResult,
                o16O18Result.FitScoreO18Profile.ToString("0.000"),
                o16O18Result.CorrelationO16O18SingleLabel.ToString("0.000"),
                o16O18Result.CorrelationO16O18DoubleLabel.ToString("0.000"),
                o16O18Result.CorrelationBetweenSingleAndDoubleLabel.ToString("0.000"),
                o16O18Result.RatioO16O18SingleLabel.ToString("0.000"),
                o16O18Result.RatioO16O18DoubleLabel.ToString("0.000")
            };

            return string.Join(Delimiter, data);
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
