using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Workflows.Backend.Core.ChromPeakSelection
{
	public class IqSmartChromPeakSelector : SmartChromPeakSelector
	{
		public IqSmartChromPeakSelector(SmartChromPeakSelectorParameters parameters) : base(parameters)
		{
		}

		public ChromPeakIqTarget SelectBestPeak(List<ChromPeakIqTarget> peakQualityList, bool filterOutFlaggedIsotopicProfiles)
		{

			//flagging algorithm checks for peak-to-the-left. This is ok for peptides whose first isotope
			//is most abundant, but not good for large peptides in which the mono peak is of lower intensity. 

			var filteredList1 = (from n in peakQualityList
								 where n.GetResult().IsotopicProfileFound &&
								 n.GetResult().FitScore < 1
								 select n).ToList();


			if (filterOutFlaggedIsotopicProfiles)
			{
				filteredList1 = filteredList1.Where(p => p.GetResult().IsIsotopicProfileFlagged == false).ToList();
			}

			ChromPeakIqTarget bestpeak;

			//target.NumQualityChromPeaks = filteredList1.Count;

			if (filteredList1.Count == 0)
			{
				bestpeak = null;
				//currentResult.FailedResult = true;
				//currentResult.FailureType = Globals.TargetedResultFailureType.ChrompeakNotFoundWithinTolerances;
			}
			else if (filteredList1.Count == 1)
			{
				bestpeak = filteredList1[0];
			}
			else
			{
				filteredList1 = filteredList1.OrderBy(p => p.GetResult().FitScore).ToList();

				double diffFirstAndSecondFitScores = Math.Abs(filteredList1[0].GetResult().FitScore - filteredList1[1].GetResult().FitScore);

				bool differenceIsSmall = (diffFirstAndSecondFitScores < 0.05);
				if (differenceIsSmall)
				{
					if (_parameters.MultipleHighQualityMatchesAreAllowed)
					{

						if (filteredList1[0].GetResult().Abundance >= filteredList1[1].GetResult().Abundance)
						{
							bestpeak = filteredList1[0];
						}
						else
						{
							bestpeak = filteredList1[1];
						}
					}
					else
					{
						bestpeak = null;
					}
				}
				else
				{
					bestpeak = filteredList1[0];
				}
			}

			return bestpeak;
		}
	}
}
