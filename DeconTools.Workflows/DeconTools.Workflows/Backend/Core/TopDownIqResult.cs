using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Workflows.Backend.Core.ChromPeakSelection;

namespace DeconTools.Workflows.Backend.Core
{
	public class TopDownIqResult : IqResult
	{
		public TopDownIqResult(IqTarget target)
			: base(target)
		{
			ChargeCorrelationData = new ChargeCorrelationData();
		}

		public ChargeCorrelationData ChargeCorrelationData { get; set; }
	}
}
