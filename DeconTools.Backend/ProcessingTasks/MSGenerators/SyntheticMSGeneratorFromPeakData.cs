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


    public class SyntheticMSGeneratorFromPeakData : I_MSGenerator
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
        public override void GenerateMS(DeconTools.Backend.Core.Run run)
        {
            Check.Require(run != null, String.Format("{0} failed. Run has not been defined.", this.Name));
            Check.Require(run.PeakList != null && run.PeakList.Count > 0, String.Format("{0} failed. Run has not been defined.", this.Name));

            XYData syntheticMSData = new XYData();
            List<double> xvals = new List<double>();
            List<double> yvals = new List<double>();

            foreach (MSPeak peak in run.PeakList)
            {
                XYData generatedXYData;

                switch (ModeOfPeakWidthCalculation)
                {
                    case SyntheticMSGeneratorFromPeakDataMode.WidthsCalculatedFromSingleValue:
                        generatedXYData = TheorXYDataCalculationUtilities.GetTheorPeakData(peak, this.PeakWidthForAllPeaks, 11);
                        break;
                    case SyntheticMSGeneratorFromPeakDataMode.WidthsCalculatedOnAPerPeakBasis:
                        generatedXYData = TheorXYDataCalculationUtilities.GetTheorPeakData(peak, peak.Width, 11);
                        break;
                    default:
                        generatedXYData = TheorXYDataCalculationUtilities.GetTheorPeakData(peak, this.PeakWidthForAllPeaks, 11);
                        break;
                }

                xvals.AddRange(generatedXYData.Xvalues);
                yvals.AddRange(generatedXYData.Yvalues);
            }

            run.XYData.Xvalues = xvals.ToArray();
            run.XYData.Yvalues = yvals.ToArray();
        }

        protected override void createNewScanResult(DeconTools.Backend.Core.ResultCollection resultList, DeconTools.Backend.Core.ScanSet scanSet)
        {
            throw new NotImplementedException();
        }
    }
}
