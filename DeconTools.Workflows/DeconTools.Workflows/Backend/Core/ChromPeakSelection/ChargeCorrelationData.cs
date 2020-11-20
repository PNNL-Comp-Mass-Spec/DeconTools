using System.Collections.Generic;

namespace DeconTools.Workflows.Backend.Core.ChromPeakSelection
{
    public class ChargeCorrelationData
    {
        public ChargeCorrelationData()
        {
            CorrelationData = new List<ChargeCorrelationItem>();
        }

        public List<ChargeCorrelationItem> CorrelationData { get; set; }
    }
}
