
namespace DeconTools.Backend.Core
{
    public abstract class Task
    {

        public abstract void Execute(ResultCollection resultList);

        public virtual string Name {get;set;}
        
        public virtual void Cleanup()
        {
            return;
        }

        #region Protected Methods

        protected string DblToString(double value, byte digitsAfterDecimal, bool limitDecimalsForLargeValues = false)
        {
            // Note that we replace commas with periods in case the user's language settings use a comma for a decimal point
            // Output files in DeconTools are CSV files, so we cannot have commas as decimal points
            return PNNLOmics.Utilities.StringUtilities.DblToString(value, digitsAfterDecimal, limitDecimalsForLargeValues).Replace(',', '.');
        }

        #endregion

    }
}
