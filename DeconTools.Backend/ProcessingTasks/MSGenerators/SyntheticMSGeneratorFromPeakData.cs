using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Utilities;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;

namespace DeconTools.Backend.ProcessingTasks.MSGenerators
{
    public enum SyntheticMSGeneratorFromPeakDataMode
    {
        WidthsCalculatedFromSingleValue,
        WidthsCalculatedOnAPerPeakBasis
    }


    public class SyntheticMSGeneratorFromPeakData : MSGenerator
    {
        #region Constructors
        public SyntheticMSGeneratorFromPeakData()
        {
            this.ModeOfPeakWidthCalculation = SyntheticMSGeneratorFromPeakDataMode.WidthsCalculatedOnAPerPeakBasis;
        }

        public SyntheticMSGeneratorFromPeakData(double peakWidthForAllPeaks)
            : this()
        {
            this.PeakWidthForAllPeaks = peakWidthForAllPeaks;
            this.ModeOfPeakWidthCalculation = SyntheticMSGeneratorFromPeakDataMode.WidthsCalculatedFromSingleValue;

        }
        #endregion

        #region Properties
        public double PeakWidthForAllPeaks { get; set; }

        public SyntheticMSGeneratorFromPeakDataMode ModeOfPeakWidthCalculation { get; set; }

        #endregion

        #region Public Methods



        #endregion

        #region Private Methods
        #endregion
        public override XYData GenerateMS(DeconTools.Backend.Core.Run run, ScanSet lcScanset, ScanSet imsScanset=null)
        {
            Check.Require(run != null, String.Format("{0} failed. Run has not been defined.", this.Name));
            Check.Require(run.PeakList != null && run.PeakList.Count > 0, String.Format("{0} failed. Run has not been defined.", this.Name));

            var syntheticMSData = new XYData();
            var xvals = new List<double>();
            var yvals = new List<double>();

            foreach (MSPeak peak in run.PeakList)
            {
                XYData generatedXYData;

                switch (ModeOfPeakWidthCalculation)
                {
                    case SyntheticMSGeneratorFromPeakDataMode.WidthsCalculatedFromSingleValue:
                        generatedXYData = peak.GetTheorPeakData(this.PeakWidthForAllPeaks, 11);
                        break;
                    case SyntheticMSGeneratorFromPeakDataMode.WidthsCalculatedOnAPerPeakBasis:
                        generatedXYData = peak.GetTheorPeakData(peak.Width, 11);
                        break;
                    default:
                        generatedXYData = peak.GetTheorPeakData(this.PeakWidthForAllPeaks, 11);
                        break;
                }

                xvals.AddRange(generatedXYData.Xvalues);
                yvals.AddRange(generatedXYData.Yvalues);
            }

            var xydata=new XYData();
            xydata.Xvalues = xvals.ToArray();
            xydata.Yvalues = yvals.ToArray();
            return xydata;
        }

        
    }
}
