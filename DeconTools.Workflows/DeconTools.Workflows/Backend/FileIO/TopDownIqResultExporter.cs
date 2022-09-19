using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var iqResults = results.ToList();

            WriteOutTargetResults(fileName, iqResults);
            WriteOutChargeResults(fileName, iqResults);
        }

        /// <summary>
        /// Writes out the target level result output
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="results"></param>
        public void WriteOutTargetResults(string fileName, IEnumerable<IqResult> results)
        {
            const string targetFileExt = "_IqResults.txt";

            var filePath = fileName + targetFileExt;

            if (!TargetOutputHeaderWritten)
            {
                try
                {
                    using (var writer = new StreamWriter(filePath))
                    {
                        var header = GetTargetHeader();
                        writer.WriteLine(header);
                        TargetOutputHeaderWritten = true;
                    }
                }
                catch (Exception ex)
                {
                    IqLogger.LogError("Unable to open file for writing: " + filePath, ex);
                    throw new IOException("Unable to open file for writing!", ex);
                }
            }

            try
            {
                using (var writer = File.AppendText(filePath))
                {
                    var resultAsString = GetTargetResultAsString(results);
                    writer.WriteLine(resultAsString);
                }
            }
            catch (Exception ex)
            {
                IqLogger.LogError("Unable to open file for writing: " + filePath, ex);
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
            const string chargeFileExt = "_ChargeStateVerboseOutput.txt";

            var filePath = fileName + chargeFileExt;

            if (!ChargeOutputHeaderWritten)
            {
                try
                {
                    using (var writer = new StreamWriter(filePath))
                    {
                        var header = GetChargeHeader();
                        writer.WriteLine(header);
                        ChargeOutputHeaderWritten = true;
                    }
                }
                catch (Exception ex)
                {
                    IqLogger.LogError("Unable to open file for writing: " + filePath, ex);
                    throw new IOException("Unable to open file for writing!", ex);
                }
            }
            foreach (var result in results)
            {
                try
                {
                    using (var writer = File.AppendText(filePath))
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
                    IqLogger.LogError("Unable to open file for writing: " + filePath, ex);
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
            var data = new List<string>
            {
                "TargetID",
                "Proteoform",
                "EmpiricalFormula",
                "PrecursorMass",
                "ChargeStateList",
                "Abundance",
                "MedianFitScore",
                "MedianChargeCorrelation"
            };

            return string.Join(Delimiter, data);
        }

        /// <summary>
        /// Gets charge output header
        /// </summary>
        /// <returns></returns>
        public string GetChargeHeader()
        {
            var data = new List<string>
            {
                "TargetID",
                "Proteoform",
                "EmpiricalFormula",
                "ChargeState",
                "TheoreticalMonoisotopicMass",
                "TheoreticalMZ",
                "TargetNormalizedElutionTime",
                "MonoisotopicMassObs",
                "MZObs",
                "NormalizedElutionTimeObs",
                "ChromPeaksWithinTolerance",
                "ScanObserved",
                "Abundance",
                "IsoFitScore",
                "IsotopeCorrelation"
            };

            return string.Join(Delimiter, data);
        }

        /// <summary>
        /// Combines charge state results into one line for target level output
        /// </summary>
        /// <param name="results"></param>
        /// <param name="includeHeader"></param>
        /// <returns></returns>
        public string GetTargetResultAsString(IEnumerable<IqResult> results, bool includeHeader = false)
        {
            double abundance = 0, correlationMedian = 0;
            var chargeStateList = "";
            var fitScoreList = new List<double>();
            var isFirst = true;
            IqTarget target = new TopDownIqTarget();

            foreach (var result in results)
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
                if (result is TopDownIqResult parentResult)
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

            var data = new List<string>
            {
                target.ID.ToString(),
                target.Code,
                target.EmpiricalFormula,
                target.MonoMassTheor.ToString("0.0000"),
                chargeStateList,
                abundance.ToString("0.000"),
                medianFit.ToString("0.0000"),
                correlationMedian.ToString("0.000")
            };

            if (includeHeader)
            {
                return GetHeader() + Environment.NewLine + string.Join(Delimiter, data);
            }

            return string.Join(Delimiter, data);
        }

        /// <summary>
        /// Gets charge results as strings, 1 line per result
        /// </summary>
        /// <param name="result"></param>
        /// <param name="includeHeader"></param>
        /// <returns></returns>
        public string GetChargeResultAsString (IqResult result, bool includeHeader = false)
        {
            if (result.Target.ChargeState == 0 || result.Abundance <= 0)
            {
                return null;
            }

            var data = new List<string>
            {
                result.Target.ID.ToString(),
                result.Target.Code,
                result.Target.EmpiricalFormula,
                result.Target.ChargeState.ToString(),
                result.Target.MonoMassTheor.ToString("0.000"),
                result.Target.MZTheor.ToString("0.000"),
                result.Target.ElutionTimeTheor.ToString("0.000"),
                result.MonoMassObs.ToString("0.000"),
                result.MZObs.ToString("0.000"),
                result.ElutionTimeObs.ToString("0.0000"),
                result.NumChromPeaksWithinTolerance.ToString(),
                result.LcScanObs.ToString(),
                result.Abundance.ToString("0.#"),
                result.FitScore.ToString("0.000")
            };

            var medianRSquared = result.CorrelationData.RSquaredValsMedian;
            if (medianRSquared.HasValue)
            {
                data.Add(medianRSquared.Value.ToString("0.000"));
            }
            else
            {
                data.Add(string.Empty);
            }

            if (includeHeader)
            {
                return GetHeader() + Environment.NewLine + string.Join(Delimiter, data);
            }

            return string.Join(Delimiter, data);
        }

        #endregion
    }
}
