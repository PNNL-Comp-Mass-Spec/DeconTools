using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Workflows.Backend.Core
{
	/// <summary>
	/// Intended to be used with ChromPeakAnalyzerIqWorkflow.
	/// Inherits from IqTarget as its base class.
	/// The only addition is a ChromPeak object
	/// </summary>
	public class ChromPeakIqTarget : IqTarget
	{

		#region Constructors

		public ChromPeakIqTarget ()
		{
			
		}

		public ChromPeakIqTarget(IqWorkflow workflow) : base(workflow)
		{
			
		}

		#endregion

		#region Properties

		public ChromPeak ChromPeak { get; set; }

		#endregion



	}
}
