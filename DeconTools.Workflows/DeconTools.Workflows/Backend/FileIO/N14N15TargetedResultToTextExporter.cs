
using System.Text;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class N14N15TargetedResultToTextExporter : TargetedResultToTextExporter
    {

        #region Constructors
        public N14N15TargetedResultToTextExporter(string fileName):base(fileName)
        {
        }


        #endregion


        #region Private Methods
        protected override string addAdditionalInfo(TargetedResultDTO result)
        {
            var n14result = (N14N15TargetedResultDTO)result;

            var sb = new StringBuilder();
            sb.Append(Delimiter);
            sb.Append(n14result.ScanN15);
            sb.Append(Delimiter);
            sb.Append(n14result.ScanN15Start);
            sb.Append(Delimiter);
            sb.Append(n14result.ScanN15End);
            sb.Append(Delimiter);
            sb.Append(n14result.NETN15);
            sb.Append(Delimiter);
            sb.Append(n14result.NumChromPeaksWithinTolN15);
            sb.Append(Delimiter);
            sb.Append(n14result.NumQualityChromPeaksWithinTolN15);
            sb.Append(Delimiter);
            sb.Append(n14result.MonoMassN15);
            sb.Append(Delimiter);
            sb.Append(n14result.MonoMassCalibratedN15);
            sb.Append(Delimiter);
            sb.Append(n14result.MonoMZN15);
            sb.Append(Delimiter);
            sb.Append(n14result.IntensityN15);
            sb.Append(Delimiter);
            sb.Append(n14result.FitScoreN15);
            sb.Append(Delimiter);
            sb.Append(n14result.IScoreN15);
            sb.Append(Delimiter);
            sb.Append(n14result.RatioContributionN14);
            sb.Append(Delimiter);
            sb.Append(n14result.RatioContributionN15);
            sb.Append(Delimiter);
            sb.Append(n14result.Ratio);



            return sb.ToString();

        }


        protected override string buildHeaderLine()
        {
            var sb = new StringBuilder();

            sb.Append(base.buildHeaderLine());

            sb.Append(Delimiter);
            sb.Append("ScanN15");
            sb.Append(Delimiter);
            sb.Append("ScanN15Start");
            sb.Append(Delimiter);
            sb.Append("ScanN15End");
            sb.Append(Delimiter);

            sb.Append("NETN15");
            sb.Append(Delimiter);
            sb.Append("ChromPeaksWithinTolN15");
            sb.Append(Delimiter);
            sb.Append("NumQualityChromPeaksWithinTolN15");
            sb.Append(Delimiter);
            sb.Append("MonoisotopicMassN15");
            sb.Append(Delimiter);
            sb.Append("MonoisotopicMassCalibratedN15");
            sb.Append(Delimiter);
            sb.Append("MonoMZN15");
            sb.Append(Delimiter);
            sb.Append("IntensityN15");
            sb.Append(Delimiter);
            sb.Append("FitScoreN15");
            sb.Append(Delimiter);
            sb.Append("IScoreN15");
            sb.Append(Delimiter);
            sb.Append("RatioContribN14");
            sb.Append(Delimiter);
            sb.Append("RatioContribN15");
            sb.Append(Delimiter);
            sb.Append("Ratio");

            return sb.ToString();

        }





        #endregion

    }
}
