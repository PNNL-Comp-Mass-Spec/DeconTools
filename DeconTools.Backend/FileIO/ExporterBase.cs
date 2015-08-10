using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

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
            // Note that we replace commas with periods in case the user's language settings use a comma for a decimal point
            // Output files in DeconTools are CSV files, so we cannot have commas as decimal points
            return PNNLOmics.Utilities.StringUtilities.DblToString(value, digitsAfterDecimal, limitDecimalsForLargeValues).Replace(',', '.');
        }

        #endregion
    }
}
