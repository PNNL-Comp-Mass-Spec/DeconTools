using System.Collections.Generic;

namespace DeconTools.Backend.FileIO
{
    public abstract class ExporterBase<T>
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        public abstract void ExportResults(IEnumerable<T> results);
        #endregion

        #region Private Methods

        protected string DblToString(double value, byte digitsAfterDecimal, bool limitDecimalsForLargeValues = false)
        {
            return PNNLOmics.Utilities.StringUtilities.DblToString(value, digitsAfterDecimal, limitDecimalsForLargeValues);
        }

        #endregion
    }
}
