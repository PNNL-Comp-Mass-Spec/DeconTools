using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DeconTools.Backend.Utilities.IqLogger;
using DeconTools.Workflows.Backend.Core;

namespace DeconTools.Workflows.Backend.FileIO
{
    public abstract class ResultExporter
    {
        protected bool HeaderWasWritten = false;

        public ResultExporter()
        {

        }

        public virtual void WriteOutResults(string fileName, IEnumerable<IqResult> results)
        {
            if (!HeaderWasWritten)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(fileName))
                    {
                        string header = GetHeader();
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
                    using (StreamWriter writer = File.AppendText(fileName))
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
            StringBuilder sb = new StringBuilder();

            string delim = "\t";

            sb.Append("TargetID");
            sb.Append(delim);
            sb.Append("Code");
            sb.Append(delim);
            sb.Append("EmpiricalFormula");
            sb.Append(delim);
            sb.Append("ChargeState");
            sb.Append(delim);
            sb.Append("MonomassTheor");
            sb.Append(delim);
            sb.Append("MZTheor");
            sb.Append(delim);
            sb.Append("ElutionTimeTheor");
            sb.Append(delim);
            sb.Append("TargetScan");
            sb.Append(delim);
            sb.Append("MonoMassObs");
            sb.Append(delim);
            sb.Append("MZObs");
            sb.Append(delim);
            sb.Append("ElutionTimeObs");
            sb.Append(delim);
            sb.Append("ChromPeaksWithinTolerance");
            sb.Append(delim);
            sb.Append("Scan");
            sb.Append(delim);
            sb.Append("Abundance");
            sb.Append(delim);
            sb.Append("IsoFitScore");
            sb.Append(delim);
            sb.Append("InterferenceScore");

            string outString = sb.ToString();
            return outString;

        }

        
        public virtual string GetHeader(IqResult result)
        {
            return this.GetHeader();
        }




        public virtual string GetResultAsString(IqResult result, bool includeHeader = false)
        {
            StringBuilder sb = new StringBuilder();

            string scanSetString;

            if (result.LCScanSetSelected != null)
            {
                scanSetString = result.LCScanSetSelected.PrimaryScanNumber.ToString("0.#");
            }
            else
            {
                scanSetString = "0";
            }

            string delim = "\t";


            if (includeHeader)
            {
                string header = GetHeader();
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
            sb.Append(result.Target.ElutionTimeTheor.ToString("0.000"));
            sb.Append(delim);
            sb.Append(result.Target.ScanLC);
            sb.Append(delim);
            sb.Append(result.MonoMassObs.ToString("0.00000"));
            sb.Append(delim);
            sb.Append(result.MZObs.ToString("0.00000"));
            sb.Append(delim);
            sb.Append(result.ElutionTimeObs.ToString("0.000"));
            sb.Append(delim);
            sb.Append(result.NumChromPeaksWithinTolerance);
            sb.Append(delim);
            sb.Append(scanSetString);
            sb.Append(delim);
            sb.Append(result.Abundance.ToString("0.#"));
            sb.Append(delim);
            sb.Append(result.FitScore.ToString("0.000"));
            sb.Append(delim);
            sb.Append(result.InterferenceScore.ToString("0.000"));

            string outString = sb.ToString();
            return outString;
        }


    }
}