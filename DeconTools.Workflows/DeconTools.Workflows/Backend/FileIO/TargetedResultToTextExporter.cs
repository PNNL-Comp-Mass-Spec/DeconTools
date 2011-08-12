using System;
using System.Text;
using DeconTools.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.FileIO
{
    public abstract class TargetedResultToTextExporter : TextFileExporter<TargetedResult>
    {

        #region Constructors
        public TargetedResultToTextExporter(string filename) : base(filename, '\t') { }

        #endregion

        #region Properties

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        protected override string buildResultOutput(TargetedResult result)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(addBasicTargetedResult(result));

            sb.Append(addAdditionalInfo(result));

            return sb.ToString();

        }

        protected virtual string addAdditionalInfo(TargetedResult result)
        {
            return String.Empty;
        }

        protected virtual string addBasicTargetedResult(TargetedResult result)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(result.DatasetName);
            sb.Append(Delimiter);
            sb.Append(result.MassTagID);
            sb.Append(Delimiter);
            sb.Append(result.ChargeState);
            sb.Append(Delimiter);
            sb.Append(result.ScanLC);
            sb.Append(Delimiter);
            sb.Append(result.ScanLCStart);
            sb.Append(Delimiter);
            sb.Append(result.ScanLCEnd);
            sb.Append(Delimiter);
            sb.Append(result.NET.ToString("0.00000"));
            sb.Append(Delimiter);
            sb.Append(result.NETError.ToString("0.000000"));
            sb.Append(Delimiter);
            sb.Append(result.NumChromPeaksWithinTol);
            sb.Append(Delimiter);
            sb.Append(result.NumQualityChromPeaksWithinTol);
            sb.Append(Delimiter);
            sb.Append(result.MonoMass.ToString("0.00000"));
            sb.Append(Delimiter);
            sb.Append(result.MonoMassCalibrated.ToString("0.00000"));
            sb.Append(Delimiter);
            sb.Append(result.MassErrorInPPM.ToString("0.00"));
            sb.Append(Delimiter);
            sb.Append(result.MonoMZ.ToString("0.00000"));
            sb.Append(Delimiter);
            sb.Append(result.Intensity);
            sb.Append(Delimiter);
            sb.Append(result.FitScore.ToString("0.0000"));
            sb.Append(Delimiter);
            sb.Append(result.IScore.ToString("0.0000"));
            sb.Append(Delimiter);
            sb.Append(result.FailureType);

            
            return sb.ToString();

        }

        protected override string buildHeaderLine()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Dataset");
            sb.Append(Delimiter);
            sb.Append("MassTagID");
            sb.Append(Delimiter);
            sb.Append("ChargeState");
            sb.Append(Delimiter);
            sb.Append("Scan");
            sb.Append(Delimiter);
            sb.Append("ScanStart");
            sb.Append(Delimiter);
            sb.Append("ScanEnd");
            sb.Append(Delimiter);
            sb.Append("NET");
            sb.Append(Delimiter);
            sb.Append("NETError");
            sb.Append(Delimiter);
            sb.Append("NumChromPeaksWithinTol");
            sb.Append(Delimiter);
            sb.Append("NumQualityChromPeaksWithinTol");
            sb.Append(Delimiter);
            sb.Append("MonoisotopicMass");
            sb.Append(Delimiter);
            sb.Append("MonoisotopicMassCalibrated");
            sb.Append(Delimiter);
            sb.Append("MassErrorInPPM");
            sb.Append(Delimiter);
            sb.Append("MonoMZ");
            sb.Append(Delimiter);
            sb.Append("IntensityRep");
            sb.Append(Delimiter);
            sb.Append("FitScore");
            sb.Append(Delimiter);
            sb.Append("IScore");
            sb.Append(Delimiter);
            sb.Append("FailureType");

            return sb.ToString();
        }
    }
}
