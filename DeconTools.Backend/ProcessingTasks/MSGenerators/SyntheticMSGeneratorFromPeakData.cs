using System.Collections.Generic;
using DeconTools.Utilities;
using DeconTools.Backend.Core;

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

        public override XYData GenerateMS(DeconTools.Backend.Core.Run run, ScanSet lcScanSet, ScanSet imsScanSet = null)
        {
            Check.Require(run != null, string.Format("{0} failed. Run has not been defined.", this.Name));
            if (run == null)
                return new XYData();

            Check.Require(run.PeakList != null && run.PeakList.Count > 0, string.Format("{0} failed. Run has not been defined.", this.Name));
            if (run.PeakList == null)
                return new XYData();

            var syntheticMSData = new XYData();
            var xVals = new List<double>();
            var yVals = new List<double>();

            foreach (var peak in run.PeakList)
            {
                var msPeak = (MSPeak)peak;

                XYData generatedXYData;

                switch (ModeOfPeakWidthCalculation)
                {
                    case SyntheticMSGeneratorFromPeakDataMode.WidthsCalculatedFromSingleValue:
                        generatedXYData = msPeak.GetTheorPeakData(this.PeakWidthForAllPeaks, 11);
                        break;
                    case SyntheticMSGeneratorFromPeakDataMode.WidthsCalculatedOnAPerPeakBasis:
                        generatedXYData = msPeak.GetTheorPeakData(msPeak.Width, 11);
                        break;
                    default:
                        generatedXYData = msPeak.GetTheorPeakData(this.PeakWidthForAllPeaks, 11);
                        break;
                }

                xVals.AddRange(generatedXYData.Xvalues);
                yVals.AddRange(generatedXYData.Yvalues);
            }

            var xyData = new XYData {
                Xvalues = xVals.ToArray(),
                Yvalues = yVals.ToArray()
            };

            return xyData;
        }


    }
}
