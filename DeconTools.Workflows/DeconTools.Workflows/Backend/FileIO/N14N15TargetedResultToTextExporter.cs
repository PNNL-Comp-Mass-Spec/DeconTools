
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


            //TODO: finish this!!


            StringBuilder sb = new StringBuilder();
            sb.Append(Delimiter);
            sb.Append(n14result.ScanN15);
            sb.Append(Delimiter);
            sb.Append(n14result.ScanN15Start);
            sb.Append(Delimiter);
            sb.Append(n14result.ScanN15End);
            sb.Append(Delimiter);
            sb.Append(n14result.NETN15);
            sb.Append(Delimiter);
            sb.Append(n14result.MonoMassN15);
            sb.Append(Delimiter);
            sb.Append(n14result.MonoMassCalibratedN15);
            sb.Append(Delimiter);
            sb.Append(n14result.IntensityN15);
            sb.Append(Delimiter);
            sb.Append(n14result.FitScoreN15);
            sb.Append(Delimiter);
            sb.Append(n14result.IScoreN15);
            sb.Append(Delimiter);
            sb.Append(n14result.RatioContributionN15);
            sb.Append(Delimiter);
            
            sb.Append(n14result.Ratio);



            return sb.ToString();

        }


        protected override string buildHeaderLine()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(base.buildHeaderLine());

            //sb.Append(Delimiter);
            //sb.Append("IntensityTheorI0");
            //sb.Append(Delimiter);
            //sb.Append("IntensityTheorI2");
            //sb.Append(Delimiter);
            //sb.Append("IntensityTheorI4");
            //sb.Append(Delimiter);

            //sb.Append("IntensityI0");
            //sb.Append(Delimiter);
            //sb.Append("IntensityI2");
            //sb.Append(Delimiter);
            //sb.Append("IntensityI4");
            //sb.Append(Delimiter);
            //sb.Append("IntensityI4Adjusted");
            //sb.Append(Delimiter);
            //sb.Append("Ratio");

            return sb.ToString();

        }





        #endregion

    }
}
