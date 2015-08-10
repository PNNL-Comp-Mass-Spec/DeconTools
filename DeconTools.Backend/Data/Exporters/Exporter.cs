using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DeconTools.Backend.Data
{
    public abstract class Exporter<T>
    {

        protected abstract string headerLine { get; set; }

        protected abstract char delimiter { get; set; }

        public abstract void Export(T results);

        protected string DblToString(double value, byte digitsAfterDecimal, bool limitDecimalsForLargeValues = false)
        {
            // Note that we replace commas with periods in case the user's language settings use a comma for a decimal point
            // Output files in DeconTools are CSV files, so we cannot have commas as decimal points
            return PNNLOmics.Utilities.StringUtilities.DblToString(value, digitsAfterDecimal, limitDecimalsForLargeValues).Replace(',', '.');
        }
   
    }


}
