using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var baseHeader= base.GetHeader();

            var sb = new StringBuilder();
            sb.Append(baseHeader);
            sb.Append(Delimiter);
            sb.Append("CorrO16O18Single");
            sb.Append(Delimiter);
            sb.Append("CorrO16O18Double");
            sb.Append(Delimiter);
            sb.Append("Ratio");
            
            return sb.ToString();
        }

        public override string GetResultAsString(Core.IqResult result, bool includeHeader = false)
        {
            var baseResult= base.GetResultAsString(result, includeHeader);
            var o16O18Result = (O16O18IqResult) result;
            var sb = new StringBuilder();

            sb.Append(baseResult);
            sb.Append(Delimiter);
            sb.Append(o16O18Result.CorrelationO16O18SingleLabel.ToString("0.000"));
            sb.Append(Delimiter);
            sb.Append(o16O18Result.CorrelationO16O18DoubleLabel.ToString("0.000"));
            sb.Append(Delimiter);
            sb.Append(o16O18Result.RatioO16O18.ToString("0.000"));

            return sb.ToString();
        }



        #endregion

        #region Private Methods

        #endregion

    }
}
