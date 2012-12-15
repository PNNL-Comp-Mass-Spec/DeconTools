using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
	public class ChromPeakQualityData
	{
		public ChromPeak Peak { get; private set; }
		public bool IsotopicProfileFound { get; set; }
		public bool IsIsotopicProfileFlagged { get; set; }
		public double FitScore { get; set; }
		public double InterferenceScore { get; set; }
		public double Abundance { get; set; }
		public IsotopicProfile IsotopicProfile { get; set; }

		public ChromPeakQualityData(ChromPeak peak)
		{
			this.InterferenceScore = 1;     // worst possible
			this.FitScore = 1;   // worst possible
			this.Abundance = 0;
			this.Peak = peak;
			this.IsotopicProfileFound = false;
			this.IsIsotopicProfileFlagged = false;
		}

		public void Display()
		{
			Console.WriteLine(Peak.XValue.ToString("0.00") + "\t" + Peak.NETValue.ToString("0.0000") + "\t" + Abundance + "\t" + FitScore.ToString("0.0000") + "\t" + InterferenceScore.ToString("0.000") + "\t" + IsIsotopicProfileFlagged);
		}

	}
}
