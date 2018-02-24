using System;
using System.Collections.Generic;
using System.IO;
using DeconTools.Backend.Utilities.IqLogger;
using DeconTools.Workflows.Backend.Core;

namespace DeconTools.Workflows.Backend.FileIO
{
    public abstract class IqResultExporter
    {
        protected bool HeaderWasWritten;
        protected string Delimiter = "\t";

        public virtual void WriteOutResults(string fileName, IEnumerable<IqResult> results)
        {
            if (!HeaderWasWritten)
            {
                try
                {
                    using (var writer = new StreamWriter(fileName))
                    {
                        var header = GetHeader();
                        writer.WriteLine(header);
                        HeaderWasWritten = true;
                    }
                }
                catch (Exception ex)
                {
                    IqLogger.LogError("Unable to open file for writing: " + fileName, ex);
                    throw new IOException("Unable to open file for writing!", ex);
                }
            }
            foreach (var result in results)
            {
                try
                {
                    using (var writer = File.AppendText(fileName))
                    {
                        var resultAsString = GetResultAsString(result);
                        writer.WriteLine(resultAsString);
                    }
                }
                catch (Exception ex)
                {
                    IqLogger.LogError("Unable to open file for writing: " + fileName, ex);
                    throw new IOException("Unable to open file for writing!", ex);
                }
            }
        }





        public virtual string GetHeader()
        {
            var data = new List<string>
            {
                "TargetID",
                "Code",
                "EmpiricalFormula",
                "ChargeState",
                "MonomassTheor",
                "MZTheor",
                "ElutionTimeTheor",
                "TargetScan",
                "MonoMassObs",
                "MZObs",
                "MonoMassObsCalibrated",
                "MZObsCalibrated",
                "ElutionTimeObs",
                "ChromPeaksWithinTolerance",
                "Scan",
                "Abundance",
                "IsoFitScore",
                "InterferenceScore"
            };

            return string.Join(Delimiter, data);

        }


        public virtual string GetHeader(IqResult result)
        {
            return GetHeader();
        }

        public virtual string GetResultAsString(IqResult result, bool includeHeader = false)
        {
            var data = new List<string>
            {
                result.Target.ID.ToString(),
                result.Target.Code,
                result.Target.EmpiricalFormula,
                result.Target.ChargeState.ToString(),
                result.Target.MonoMassTheor.ToString("0.00000"),
                result.Target.MZTheor.ToString("0.00000"),
                result.Target.ElutionTimeTheor.ToString("0.00000"),
                result.Target.ScanLC.ToString(),
                result.MonoMassObs.ToString("0.00000"),
                result.MZObs.ToString("0.00000"),
                result.MonoMassObsCalibrated.ToString("0.00000"),
                result.MZObsCalibrated.ToString("0.00000"),
                result.ElutionTimeObs.ToString("0.00000"),
                result.NumChromPeaksWithinTolerance.ToString(),
                result.LcScanObs.ToString(),
                result.Abundance.ToString("0.#"),
                result.FitScore.ToString("0.000"),
                result.InterferenceScore.ToString("0.000")
            };

            if (includeHeader)
            {
                return GetHeader() + Environment.NewLine + string.Join(Delimiter, data);
            }

            return string.Join(Delimiter, data);
        }


    }
}