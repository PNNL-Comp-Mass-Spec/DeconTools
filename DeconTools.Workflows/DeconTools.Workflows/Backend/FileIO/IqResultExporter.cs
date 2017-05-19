using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
                    IqLogger.Log.Fatal("Unable to open file for writing!" + Environment.NewLine);
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
                    IqLogger.Log.Fatal("Unable to open file for writing!" + Environment.NewLine);
                    throw new IOException("Unable to open file for writing!", ex);
                }
            }
        }





        public virtual string GetHeader()
        {
            var sb = new StringBuilder();

            

            sb.Append("TargetID");
            sb.Append(Delimiter);
            sb.Append("Code");
            sb.Append(Delimiter);
            sb.Append("EmpiricalFormula");
            sb.Append(Delimiter);
            sb.Append("ChargeState");
            sb.Append(Delimiter);
            sb.Append("MonomassTheor");
            sb.Append(Delimiter);
            sb.Append("MZTheor");
            sb.Append(Delimiter);
            sb.Append("ElutionTimeTheor");
            sb.Append(Delimiter);
            sb.Append("TargetScan");
            sb.Append(Delimiter);
            sb.Append("MonoMassObs");
            sb.Append(Delimiter);
            sb.Append("MZObs");
            sb.Append(Delimiter);
            sb.Append("MonoMassObsCalibrated");
            sb.Append(Delimiter);
            sb.Append("MZObsCalibrated");
            sb.Append(Delimiter);
            sb.Append("ElutionTimeObs");
            sb.Append(Delimiter);
            sb.Append("ChromPeaksWithinTolerance");
            sb.Append(Delimiter);
            sb.Append("Scan");
            sb.Append(Delimiter);
            sb.Append("Abundance");
            sb.Append(Delimiter);
            sb.Append("IsoFitScore");
            sb.Append(Delimiter);
            sb.Append("InterferenceScore");

            var outString = sb.ToString();
            return outString;

        }

        
        public virtual string GetHeader(IqResult result)
        {
            return GetHeader();
        }




        public virtual string GetResultAsString(IqResult result, bool includeHeader = false)
        {
            var sb = new StringBuilder();
            var delim = "\t";

            if (includeHeader)
            {
                var header = GetHeader();
                sb.Append(header);
                sb.Append(Environment.NewLine);
            }
            sb.Append(result.Target.ID);
            sb.Append(delim);
            sb.Append(result.Target.Code);
            sb.Append(delim);
            sb.Append(result.Target.EmpiricalFormula);
            sb.Append(delim);
            sb.Append(result.Target.ChargeState);
            sb.Append(delim);
            sb.Append(result.Target.MonoMassTheor.ToString("0.00000"));
            sb.Append(delim);
            sb.Append(result.Target.MZTheor.ToString("0.00000"));
            sb.Append(delim);
            sb.Append(result.Target.ElutionTimeTheor.ToString("0.00000"));
            sb.Append(delim);
            sb.Append(result.Target.ScanLC);
            sb.Append(delim);
            sb.Append(result.MonoMassObs.ToString("0.00000"));
            sb.Append(delim);
            sb.Append(result.MZObs.ToString("0.00000"));
            sb.Append(delim);
            sb.Append(result.MonoMassObsCalibrated.ToString("0.00000"));
            sb.Append(delim);
            sb.Append(result.MZObsCalibrated.ToString("0.00000"));
            sb.Append(delim);
            sb.Append(result.ElutionTimeObs.ToString("0.00000"));
            sb.Append(delim);
            sb.Append(result.NumChromPeaksWithinTolerance);
            sb.Append(delim);
            sb.Append(result.LcScanObs);
            sb.Append(delim);
            sb.Append(result.Abundance.ToString("0.#"));
            sb.Append(delim);
            sb.Append(result.FitScore.ToString("0.000"));
            sb.Append(delim);
            sb.Append(result.InterferenceScore.ToString("0.000"));

            var outString = sb.ToString();
            return outString;
        }


    }
}