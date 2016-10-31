using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;

namespace DeconTools.Workflows.Backend.Core
{
    public class IqResult
    {
        readonly IqLabelFreeResultExporter _labelFreeResultExporter;
        private List<IqResult> _childResults;

        #region Constructors

        public IqResult(IqTarget target)
        {
            _labelFreeResultExporter = new IqLabelFreeResultExporter();
            _childResults = new List<IqResult>();

            Target = target;
            IqResultDetail = new IqResultDetail();
            CorrelationData = new ChromCorrelationData();
            FitScore = 1;
            InterferenceScore = 1;
            IsExported = true;
        }

        #endregion

        #region Properties

        public IqResult ParentResult { get; set; }

        public IqTarget Target { get; set; }

        public double MonoMassObs { get; set; }

        public double MZObs { get; set; }


        public double MonoMassObsCalibrated { get; set; }

        public double MZObsCalibrated { get; set; }

        public IsotopicProfile ObservedIsotopicProfile { get; set; }

        public double ElutionTimeObs { get; set; }

        public List<Peak> ChromPeakList { get; set; }

        public int NumChromPeaksWithinTolerance { get; set; }

        public int NumQualityChromPeaks { get; set; }

        public Peak ChromPeakSelected { get; set; }

        public ScanSet LCScanSetSelected { get; set; }

        public int LcScanObs { get; set; }

        public double FitScore { get; set; }

        public double InterferenceScore { get; set; }

        public float Abundance { get; set; }

        public IqResultDetail IqResultDetail { get; set; }

        public bool IsIsotopicProfileFlagged { get; set; }

        public bool IsotopicProfileFound { get; set; }

        public ChromCorrelationData CorrelationData { get; set; }

        public double MassErrorBefore { get; set; }

        public double MassErrorAfter { get; set; }

        public double NETError { get; set; }

        public bool IsExported { get; set; }

        #endregion

        #region Public Methods


        public string ToStringWithDetailedReport()
        {
            StringBuilder sb = new StringBuilder();
            string delim = "\t";

            sb.Append("---------------------------------------------" + Environment.NewLine);
            sb.Append("TargetID" + delim + Target.ID + delim + Environment.NewLine);
            sb.Append("Code" + delim + Target.Code + delim + Environment.NewLine);
            sb.Append("Z" + delim + Target.ChargeState + delim + Environment.NewLine);
            sb.Append("MZ (theor/Obs)" + delim + Target.MZTheor.ToString("0.0000") + delim + MZObs.ToString("0.0000") + Environment.NewLine);
            sb.Append("MonoMass (theor/Obs)" + delim + Target.MonoMassTheor.ToString("0.0000") + delim + MonoMassObs.ToString("0.0000") + Environment.NewLine);
            sb.Append("ElutionTime (theor/Obs)" + delim + Target.ElutionTimeTheor.ToString("0.000") + delim + ElutionTimeObs.ToString("0.000") + Environment.NewLine);
            sb.Append("ScanSet (Obs)" + delim + delim + LcScanObs + Environment.NewLine);
            sb.Append("FitScore (Obs)" + delim + delim + this.FitScore.ToString("0.000") + Environment.NewLine);

            sb.Append(Environment.NewLine);
            sb.Append("Chromatogram length" + delim + (IqResultDetail.Chromatogram == null ? "[null]" : IqResultDetail.Chromatogram.Xvalues.Length.ToString()) + Environment.NewLine);
            sb.Append("Num chom peaks" + delim + ChromPeakList.Count + Environment.NewLine);

            string chromPeakSelectedString = ChromPeakSelected == null
                                                 ? "[null]"
                                                 : "Scan= " + ChromPeakSelected.XValue.ToString("0.0") + "; Intensity= " +
                                                   ChromPeakSelected.Height + "; Width=" + ChromPeakSelected.Width;

            sb.Append("ChromPeakSelected" + delim + chromPeakSelectedString + Environment.NewLine);
            sb.Append("MassSpectrum length" + delim + (IqResultDetail.MassSpectrum == null ? "[null]" : IqResultDetail.MassSpectrum.Xvalues.Length.ToString()) + Environment.NewLine);

            return sb.ToString();

        }

        public string ToStringAsSingleRow()
        {
            return _labelFreeResultExporter.GetResultAsString(this);
        }

        #endregion

        #region Private Methods

        #endregion

        public void AddResult(IqResult result)
        {
            result.ParentResult = this;
            _childResults.Add(result);

        }

        public void RemoveResult(IqResult result)
        {
            foreach (var childResult in result._childResults)
            {
                childResult.ParentResult = result.ParentResult;
                result.ParentResult._childResults.Add(childResult);
            }
            result.ParentResult._childResults.Remove(result);
        }

        public virtual void Dispose()
        {
            if (HasChildren())
            {
                var childResults = ChildResults();

                foreach (var childResult in childResults)
                {
                    childResult.Dispose();
                }
            }

            IqResultDetail.Chromatogram = null;
            IqResultDetail.MassSpectrum = null;
        }


        public IEnumerable<IqResult>ChildResults()
        {
            return _childResults;
        }

        public bool HasChildren()
        {
            return _childResults.Any();
        }


        public bool HasParent
        {
            get
            {
                return ParentResult != null;
            }
        }

        /// <summary>
        /// This property allows assigning one of the child results as being the best or favored one.
        /// </summary>
        public IqResult FavoriteChild { get; set; }
       

    }
}
