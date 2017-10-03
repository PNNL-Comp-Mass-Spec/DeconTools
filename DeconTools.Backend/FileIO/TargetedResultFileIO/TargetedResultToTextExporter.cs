using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core.Results;

namespace DeconTools.Backend.FileIO.TargetedResultFileIO
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
            var data = new List<string>
            {
                result.DatasetName,
                result.MassTagID,
                result.ChargeState,
                result.ScanLC,
                result.ScanLCStart,
                result.ScanLCEnd,
                result.NET.ToString("0.0000"),
                result.NumChromPeaksWithinTol,
                result.MonoMass.ToString("0.00000"),
                result.MonoMZ.ToString("0.00000"),
                result.Intensity,
                result.FitScore.ToString("0.0000"),
                result.IScore.ToString("0.0000")
            };
            
            return string.Join(Delimiter.ToString(), data,

        }

        protected override string buildHeaderLine()
        {
            var data = new List<string>
            {
                "Dataset",
                "MassTagID",
                "ChargeState",
                "Scan",
                "ScanStart",
                "ScanEnd",
                "NET",
                "NumChromPeaksWithinTol",
                "MonoisotopicMass",
                "MonoMZ",
                "IntensityRep",
                "FitScore",
                "IScore"
            };

            return string.Join(Delimiter.ToString(), data);
        }
    }
}
