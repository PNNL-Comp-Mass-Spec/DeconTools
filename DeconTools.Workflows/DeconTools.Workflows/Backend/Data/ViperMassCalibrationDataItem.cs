
namespace DeconTools.Workflows.Backend.Data
{
    /// <summary>
    /// Holding place for Viper's 'MassAndGANETErrors_BeforeRefinement'
    /// </summary>
    public class ViperMassCalibrationDataItem
    {

        #region Constructors
        #endregion

        #region Properties

        public double MassErrorPpm { get; set; }

        public int Count { get; set; }

        public double SmoothedCount { get; set; }

        public string Comment { get; set; }

        #endregion

        #region Public Methods

        public string ToStringWithDetails()
        {
            return MassErrorPpm + "\t" + Count + "\t" + SmoothedCount + "\t" + Comment;
        }


        #endregion

        #region Private Methods

        #endregion

    }
}
