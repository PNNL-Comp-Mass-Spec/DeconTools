using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;

namespace DeconTools.Workflows.Backend.Core.ChromPeakSelection
{
	public class IqChargeCorrelator : IqChromCorrelatorBase
	{
		#region Constructors

		public IqChargeCorrelator(int numPointsInSmoother, double minRelativeIntensityForChromCorr = 0.01, double chromToleranceInPPM = 20, DeconTools.Backend.Globals.ToleranceUnit toleranceUnit = DeconTools.Backend.Globals.ToleranceUnit.PPM) 
			: base(numPointsInSmoother, minRelativeIntensityForChromCorr, chromToleranceInPPM, toleranceUnit)
		{
			_chromGen = new PeakChromatogramGenerator();
		}

		#endregion

		#region Properties

		private readonly PeakChromatogramGenerator _chromGen;

		#endregion

		public List<ChargeCorrelationItem> CorrelateData(List<ChromPeakIqTarget> targetList, Run run, double correlationThreshold = 0.8, int peaksToCorrelate = 3)
		{
			List<ChargeCorrelationItem> correlationList = new List<ChargeCorrelationItem>();
			List<ChromPeakIqTarget> availableTargets = new List<ChromPeakIqTarget>(targetList);
			//Sorts target list by fit score
			//Lowest to Highest
			var sortedTargetList = targetList.OrderBy(n => n.GetResult().FitScore);

			foreach (ChromPeakIqTarget referenceTarget in sortedTargetList)
			{
				//Checks to see if reference peak is in availableTargets list
				if (!availableTargets.Contains(referenceTarget)) continue;

				//New ChargeCorrelationItem for the reference peak
				ChargeCorrelationItem referenceCorrelationData = new ChargeCorrelationItem(referenceTarget);

				//Gets the ChromPeaks base width in scans for correlation
				int startScan, stopScan;
				GetBaseScanRange(referenceTarget.ChromPeak, out startScan, out stopScan);

				//Generates an array of XICs for the reference peak
				var referenceMZList = DeconTools.Backend.Utilities.IsotopicProfileUtilities.GetTopNMZValues(referenceTarget.TheorIsotopicProfile.Peaklist, peaksToCorrelate);
				XYData[] referenceXIC = GetCorrelationXICs(peaksToCorrelate, referenceMZList, run, startScan, stopScan);

				//Iterates through the rest of the available targets
				foreach (ChromPeakIqTarget correlatingTarget in targetList)
				{
					//Checks if target is available
					if (!availableTargets.Contains(correlatingTarget)) continue;

					//Checks if the peak to be correlated has an XValue that falls within the base peak range of the reference peak
					if (correlatingTarget.ChromPeak.XValue > startScan && correlatingTarget.ChromPeak.XValue < stopScan)
					{
						//Generates an XIC array for the peak being correlated. 
						var correlationMZList =
							DeconTools.Backend.Utilities.IsotopicProfileUtilities.GetTopNMZValues(
								correlatingTarget.TheorIsotopicProfile.Peaklist, peaksToCorrelate);
						XYData[] correlationXIC = GetCorrelationXICs(peaksToCorrelate, correlationMZList, run, startScan, stopScan);

						//Generates new correlation data item for current correlation
						ChromCorrelationData correlationData = new ChromCorrelationData();
						for (int i = 0; i < peaksToCorrelate; i++)
						{
							//Checks if either of the XICs are null
							if (referenceXIC[i] != null && correlationXIC[i] != null)
							{
								double slope, intercept, rsquaredval;
								GetElutionCorrelationData(referenceXIC[i],
								                          FillInAnyMissingValuesInChromatogram(referenceXIC[i], correlationXIC[i]), out slope,
								                          out intercept, out rsquaredval);
								correlationData.AddCorrelationData(slope, intercept, rsquaredval);
							}
							else
							{
								//A placeholder to show that the data was poor
								correlationData.AddCorrelationData(-9999, -9999, 0);
							}
						}

						//Checks if the overall correlation median is higher than the correlation threshold
						if (correlationData.RSquaredValsMedian >= correlationThreshold)
						{
							referenceCorrelationData.PeakCorrelationData.Add(correlatingTarget, correlationData);

							//Removes correlating target from the availableTargets list because it was successfully correlated.
							availableTargets.Remove(correlatingTarget);
						}
					}
				}
				//Removes reference target from availableTargets list
				availableTargets.Remove(referenceTarget);
				correlationList.Add(referenceCorrelationData);
			}
			return correlationList;
		}

		private void GetBaseScanRange(ChromPeak referencePeak, out int startScan, out int stopScan)
		{
			double baseWidth = (4 * (referencePeak.Width / 2.35));
			startScan = Convert.ToInt32(Math.Floor(referencePeak.XValue - (0.5 * baseWidth)));
			stopScan = Convert.ToInt32(Math.Ceiling(referencePeak.XValue + (0.5 * baseWidth)));
		}

		private XYData[] GetCorrelationXICs(int peaksToCorrelate, IEnumerable<double> MZList, Run run, int startScan, int stopScan)
		{
			XYData[] XICArray = new XYData[peaksToCorrelate];
			int index = 0;
			foreach (double mz in MZList)
			{
				XICArray[index] = Smoother.Smooth(_chromGen.GenerateChromatogram(run, startScan, stopScan, mz, 20));
				index++;
			}
			return XICArray;
		}
	
	}
}
