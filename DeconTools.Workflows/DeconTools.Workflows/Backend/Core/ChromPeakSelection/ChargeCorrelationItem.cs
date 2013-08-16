using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;

namespace DeconTools.Workflows.Backend.Core.ChromPeakSelection
{
	public class ChargeCorrelationItem
	{
		public ChargeCorrelationItem()
		{
			SelectedTargetGrouping = false;
			ReferenceTarget = new ChromPeakIqTarget();
			PeakCorrelationData = new Dictionary<ChromPeakIqTarget, ChromCorrelationData>();
		}

		public ChargeCorrelationItem(ChromPeakIqTarget referenceTarget)
		{
			SelectedTargetGrouping = false;
			ReferenceTarget = referenceTarget;
			PeakCorrelationData = new Dictionary<ChromPeakIqTarget, ChromCorrelationData>();
		}

		public ChromPeakIqTarget ReferenceTarget { get; set; }

		public Dictionary<ChromPeakIqTarget, ChromCorrelationData> PeakCorrelationData { get; set; }

		public bool SelectedTargetGrouping { get; set; }
	}
}
