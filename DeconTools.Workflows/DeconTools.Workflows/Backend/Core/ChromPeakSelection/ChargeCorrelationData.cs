using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
