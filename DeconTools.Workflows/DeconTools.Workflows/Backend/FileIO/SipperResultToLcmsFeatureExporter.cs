using System.Text;
using DeconTools.Workflows.Backend.Results;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class SipperResultToLcmsFeatureExporter : TargetedResultToTextExporter
    {

        #region Constructors
        public SipperResultToLcmsFeatureExporter(string filename)
            : base(filename)
        {

        }

        #endregion

        protected override string addAdditionalInfo(TargetedResultDTO result)
        {
            var sipperResult = (SipperLcmsFeatureTargetedResultDTO)result;

            StringBuilder sb = new StringBuilder();
            sb.Append(Delimiter);
            sb.Append(sipperResult.MatchedMassTagID);
            sb.Append(Delimiter);
            sb.Append(sipperResult.AreaUnderDifferenceCurve.ToString("0.000"));
            sb.Append(Delimiter);
            sb.Append(sipperResult.AreaUnderRatioCurve.ToString("0.000"));
            sb.Append(Delimiter);
            sb.Append(sipperResult.AreaUnderRatioCurveRevised.ToString("0.000"));
            sb.Append(Delimiter);
            sb.Append(sipperResult.RSquaredValForRatioCurve.ToString("0.00000"));
            sb.Append(Delimiter);
            sb.Append(sipperResult.ChromCorrelationMin.ToString("0.00000"));
            sb.Append(Delimiter);
            sb.Append(sipperResult.ChromCorrelationMax.ToString("0.00000"));
            sb.Append(Delimiter);
            sb.Append(sipperResult.ChromCorrelationAverage.ToString("0.00000"));
            sb.Append(Delimiter);
            sb.Append(sipperResult.ChromCorrelationMedian.ToString("0.00000"));

            return sb.ToString();

        }


        protected override string buildHeaderLine()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(base.buildHeaderLine());
            sb.Append(Delimiter);
            sb.Append("MatchedMassTagID");
            sb.Append(Delimiter);
            sb.Append("AreaDifferenceCurve");
            sb.Append(Delimiter);
            sb.Append("AreaRatioCurve");
            sb.Append(Delimiter);
            sb.Append("AreaRatioCurveRevised");
            sb.Append(Delimiter);
            
            sb.Append("RSquared");
            sb.Append(Delimiter);
            sb.Append("ChromCorrMin");
            sb.Append(Delimiter);
            sb.Append("ChromCorrMax");
            sb.Append(Delimiter);
            sb.Append("ChromCorrAverage");
            sb.Append(Delimiter);
            sb.Append("ChromCorrMedian");
            


            return sb.ToString();

        }


    }
}
