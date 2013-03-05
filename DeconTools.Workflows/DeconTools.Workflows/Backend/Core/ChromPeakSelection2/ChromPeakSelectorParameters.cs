
namespace DeconTools.Workflows.Backend.Core.ChromPeakSelection
{
    public enum SummingModeEnum
    {
        SUMMINGMODE_STATIC,     // mode in which the number of scans summed is always the same
        SUMMINGMODE_DYNAMIC    // mode in which the number of scans summed is variable; depends on chromatograph peak dimensions

    }


    public class ChromPeakSelectorParameters
    {

        #region Constructors
        public ChromPeakSelectorParameters()
        {
            NumScansToSum = 1;
            NETTolerance = 0.05f;
            PeakSelectorMode = DeconTools.Backend.Globals.PeakSelectorMode.MostIntense;
            SummingMode = SummingModeEnum.SUMMINGMODE_STATIC;
            MaxScansSummedInDynamicSumming = 20;
            AreaOfPeakToSumInDynamicSumming = 2.0;
        }

        public ChromPeakSelectorParameters(ChromPeakSelectorParameters parameters)
            : this()
        {
            NumScansToSum = parameters.NumScansToSum;
            NETTolerance = parameters.NETTolerance;
            PeakSelectorMode = parameters.PeakSelectorMode;
            SummingMode = parameters.SummingMode;
            MaxScansSummedInDynamicSumming = parameters.MaxScansSummedInDynamicSumming;
            AreaOfPeakToSumInDynamicSumming = parameters.AreaOfPeakToSumInDynamicSumming;
        }


        #endregion

        #region Properties
        public int NumScansToSum { get; set; }

        public float NETTolerance { get; set; }

        public DeconTools.Backend.Globals.PeakSelectorMode PeakSelectorMode { get; set; }


        /// <summary>
        /// Summing mode. Static indicates that the number of scans summed is always the same. Dynamic indicates that the number of scans summed is variable
        /// </summary>
        public SummingModeEnum SummingMode { get; set; }


        public int MaxScansSummedInDynamicSumming { get; set; }

        /// <summary>
        /// Area of peak to sum, based on units of sigma. Eg a value of '2' means +/- 2 sigma; thus 4 sigma or ~ 95% of a gaussian peak will be summed
        /// </summary>
        public double AreaOfPeakToSumInDynamicSumming { get; set; }


        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

    }
}
