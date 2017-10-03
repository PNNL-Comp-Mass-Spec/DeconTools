
using System.Collections.Generic;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class N14N15TargetedResultToTextExporter : TargetedResultToTextExporter
    {

        #region Constructors
        public N14N15TargetedResultToTextExporter(string fileName) : base(fileName)
        {
        }


        #endregion

        #region Private Methods
        protected override string addAdditionalInfo(TargetedResultDTO result)
        {
            var n14result = (N14N15TargetedResultDTO)result;

            var data = new List<string>
            {
                n14result.ScanN15.ToString(),
                n14result.ScanN15Start.ToString(),
                n14result.ScanN15End.ToString(),
                n14result.NETN15.ToString("0.0000"),
                n14result.NumChromPeaksWithinTolN15.ToString(),
                n14result.NumQualityChromPeaksWithinTolN15.ToString(),
                n14result.MonoMassN15.ToString("0.00000"),
                n14result.MonoMassCalibratedN15.ToString("0.00000"),
                n14result.MonoMZN15.ToString("0.0000"),
                n14result.IntensityN15.ToString("0.0000"),
                n14result.FitScoreN15.ToString("0.0000"),
                n14result.IScoreN15.ToString("0.0000"),
                n14result.RatioContributionN14.ToString("0.0000"),
                n14result.RatioContributionN15.ToString("0.0000"),
                n14result.Ratio.ToString("0.0000")
            };

            return string.Join(Delimiter.ToString(), data);

        }

        protected override string buildHeaderLine()
        {
            var data = new List<string>
            {
                base.buildHeaderLine(),
                "ScanN15",
                "ScanN15Start",
                "ScanN15End",
                "NETN15",
                "ChromPeaksWithinTolN15",
                "NumQualityChromPeaksWithinTolN15",
                "MonoisotopicMassN15",
                "MonoisotopicMassCalibratedN15",
                "MonoMZN15",
                "IntensityN15",
                "FitScoreN15",
                "IScoreN15",
                "RatioContribN14",
                "RatioContribN15",
                "Ratio"
            };

            return string.Join(Delimiter.ToString(), data);

        }


        #endregion

    }
}
