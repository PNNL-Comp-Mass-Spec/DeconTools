using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IqLogger;
using DeconTools.Workflows.Backend.Core;

namespace DeconTools.Workflows.Backend.FileIO
{
	public class TopDownIqResultExporter : IqLabelFreeResultExporter
	{

		#region Properties

		protected bool TargetOutputHeaderWritten { get; set; }
		protected bool ChargeOutputHeaderWritten { get; set; }

		#endregion


		#region Public Methods


		/// <summary>
		/// Writes out both charge and target level output for Top Down Iq analysis
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="results"></param>
		public override void WriteOutResults(string fileName, IEnumerable<IqResult> results)
		{
			WriteOutTargetResults(fileName, results);
			WriteOutChargeResults(fileName, results);
		}


		/// <summary>
		/// Writes out the target level result output
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="results"></param>
		public void WriteOutTargetResults(string fileName, IEnumerable<IqResult> results)
		{
			const string targetFileExt = @"_IqResults.txt";
			if (!TargetOutputHeaderWritten)
			{
				try
				{
					using (var writer = new StreamWriter(fileName + targetFileExt))
					{
						string header = GetTargetHeader();
						writer.WriteLine(header);
						TargetOutputHeaderWritten = true;
					}
				}
				catch (Exception ex)
				{
					IqLogger.Log.Fatal("Unable to open file for writing!" + Environment.NewLine);
					throw new IOException("Unable to open file for writing!", ex);
				}
			}
			try
			{
				using (StreamWriter writer = File.AppendText(fileName + targetFileExt))
				{
					var resultAsString = GetTargetResultAsString(results);
					writer.WriteLine(resultAsString);
				}
			}
			catch (Exception ex)
			{
				IqLogger.Log.Fatal("Unable to open file for writing!" + Environment.NewLine);
				throw new IOException("Unable to open file for writing!", ex);
			}
		}


		/// <summary>
		/// Writes out the charge state level result output
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="results"></param>
		public void WriteOutChargeResults(string fileName, IEnumerable<IqResult> results)
		{
			const string chargeFileExt = @"_ChargeStateVerboseOutput.txt";
			if (!ChargeOutputHeaderWritten)
			{
				try
				{
					using (var writer = new StreamWriter(fileName + chargeFileExt))
					{
						string header = GetChargeHeader();
						writer.WriteLine(header);
						ChargeOutputHeaderWritten = true;
					}
				}
				catch (Exception ex)
				{
					IqLogger.Log.Fatal("Unable to open file for writing!" + Environment.NewLine);
					throw new IOException("Unable to open file for writing!", ex);
				}
			}
			foreach (var result in results)
			{
				try
				{
					using (StreamWriter writer = File.AppendText(fileName + chargeFileExt))
					{
						var resultAsString = GetChargeResultAsString(result);
						if (resultAsString != null)
						{
							writer.WriteLine(resultAsString);
						}
					}
				}
				catch (Exception ex)
				{
					IqLogger.Log.Fatal("Unable to open file for writing!" + Environment.NewLine);
					throw new IOException("Unable to open file for writing!", ex);
				}
			}
		}


		/// <summary>
		/// Gets target output header
		/// </summary>
		/// <returns></returns>
		public string GetTargetHeader()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("TargetID");
			sb.Append(Delimiter);
			sb.Append("Proteoform");
			sb.Append(Delimiter);
			sb.Append("EmpiricalFormula");
			sb.Append(Delimiter);
			sb.Append("PrecursorMass");
			sb.Append(Delimiter);
			sb.Append("ChargeStateList");
			sb.Append(Delimiter);
			sb.Append("Abundance");
			sb.Append(Delimiter);
			sb.Append("MedianFitScore");
			sb.Append(Delimiter);
			sb.Append("MedianChargeCorrelation");

			string outString = sb.ToString();
			return outString;

		}


		/// <summary>
		/// Gets charge output header
		/// </summary>
		/// <returns></returns>
		public string GetChargeHeader()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("TargetID");
			sb.Append(Delimiter);
			sb.Append("Proteoform");
			sb.Append(Delimiter);
			sb.Append("EmpiricalFormula");
			sb.Append(Delimiter);
			sb.Append("ChargeState");
			sb.Append(Delimiter);
			sb.Append("TheoreticalMonoisotopicMass");
			sb.Append(Delimiter);
			sb.Append("TheoreticalMZ");
			sb.Append(Delimiter);
			sb.Append("TargetNormalizedElutionTime");
			sb.Append(Delimiter);
			sb.Append("MonoisotopicMassObs");
			sb.Append(Delimiter);
			sb.Append("MZObs");
			sb.Append(Delimiter);
			sb.Append("NormalizedElutionTimeObs");
			sb.Append(Delimiter);
			sb.Append("ChromPeaksWithinTolerance");
			sb.Append(Delimiter);
			sb.Append("ScanObserved");
			sb.Append(Delimiter);
			sb.Append("Abundance");
			sb.Append(Delimiter);
			sb.Append("IsoFitScore");
			sb.Append(Delimiter);
			sb.Append("IsotopeCorrelation");

			string outString = sb.ToString();
			return outString;

		}


		/// <summary>
		/// Combines charge state results into one line for target level output
		/// </summary>
		/// <param name="results"></param>
		/// <param name="includeHeader"></param>
		/// <returns></returns>
		public string GetTargetResultAsString(IEnumerable<IqResult> results, bool includeHeader = false)
		{
			StringBuilder sb = new StringBuilder();

			if (includeHeader)
			{
				string header = GetHeader();
				sb.Append(header);
				sb.Append(Environment.NewLine);
			}

			double abundance = 0, correlationMedian = 0;
			string chargeStateList = "";
			List<double> fitScoreList = new List<double>();
			bool isFirst = true;
			IqTarget target = new TopDownIqTarget();

			foreach (IqResult result in results)
			{
				//Sums abundance from each result
				abundance += result.Abundance;

				//Eliminates charge 0 or targets that are too poor to quantify
				if (!(result.Target.ChargeState == 0 || result.Abundance <= 0))
				{
					fitScoreList.Add(result.FitScore);
					if (isFirst)
					{
						chargeStateList += (result.Target.ChargeState);
						isFirst = false;
					}
					else
					{
						chargeStateList += (", " + result.Target.ChargeState);
					}
				}

				//Looks for parent level target for information
				TopDownIqResult parentResult = result as TopDownIqResult;
				if (parentResult != null)
				{
					target = parentResult.Target;
					correlationMedian = parentResult.SelectedCorrelationGroup.ChargeCorrelationMedian();
				}
			}

			double medianFit = 1;

			if (fitScoreList.Count != 0)
			{
				medianFit = MathUtils.GetMedian(fitScoreList);
			}
			
			

			sb.Append(target.ID);
			sb.Append(Delimiter);
			sb.Append(target.Code);
			sb.Append(Delimiter);
			sb.Append(target.EmpiricalFormula);
			sb.Append(Delimiter);
			sb.Append(target.MonoMassTheor);
			sb.Append(Delimiter);
			sb.Append(chargeStateList);
			sb.Append(Delimiter);
			sb.Append(abundance);
			sb.Append(Delimiter);
			sb.Append(medianFit);
			sb.Append(Delimiter);
			sb.Append(correlationMedian);

			string outString = sb.ToString();
			return outString;
		}



		/// <summary>
		/// Gets charge results as strings, 1 line per result
		/// </summary>
		/// <param name="result"></param>
		/// <param name="includeHeader"></param>
		/// <returns></returns>
		public string GetChargeResultAsString (IqResult result, bool includeHeader = false)
		{
			if (!(result.Target.ChargeState == 0 || result.Abundance <= 0))
			{
				StringBuilder sb = new StringBuilder();

				if (includeHeader)
				{
					string header = GetHeader();
					sb.Append(header);
					sb.Append(Environment.NewLine);
				}
				sb.Append(result.Target.ID);
				sb.Append(Delimiter);
				sb.Append(result.Target.Code);
				sb.Append(Delimiter);
				sb.Append(result.Target.EmpiricalFormula);
				sb.Append(Delimiter);
				sb.Append(result.Target.ChargeState);
				sb.Append(Delimiter);
				sb.Append(result.Target.MonoMassTheor.ToString("0.000"));
				sb.Append(Delimiter);
				sb.Append(result.Target.MZTheor.ToString("0.000"));
				sb.Append(Delimiter);
				sb.Append(result.Target.ElutionTimeTheor.ToString("0.000"));
				sb.Append(Delimiter);
				sb.Append(result.MonoMassObs.ToString("0.000"));
				sb.Append(Delimiter);
				sb.Append(result.MZObs.ToString("0.000"));
				sb.Append(Delimiter);
				sb.Append(result.ElutionTimeObs.ToString("0.0000"));
				sb.Append(Delimiter);
				sb.Append(result.NumChromPeaksWithinTolerance);
				sb.Append(Delimiter);
				sb.Append(result.LcScanObs);
				sb.Append(Delimiter);
				sb.Append(result.Abundance.ToString("0.#"));
				sb.Append(Delimiter);
				sb.Append(result.FitScore.ToString("0.000"));
				sb.Append(Delimiter);
				sb.Append(result.CorrelationData.RSquaredValsMedian);

				string outString = sb.ToString();
				return outString;
			}
			else
			{
				return null;
			}
		}


		#endregion
	}
}
