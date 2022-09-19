
using System;

namespace DeconTools.Backend.Core
{
    public abstract class Task
    {
        public abstract void Execute(ResultCollection resultList);

        public virtual string Name { get; set; }

        public bool ShowTraceMessages { get; set; }

        public virtual void Cleanup()
        {
            return;
        }

        #region Protected Methods

        protected string DblToString(double value, byte digitsAfterDecimal, bool limitDecimalsForLargeValues = false)
        {
            return PNNLOmics.Utilities.StringUtilities.DblToString(value, digitsAfterDecimal, limitDecimalsForLargeValues);
        }

        protected void ShowTraceMessageIfEnabled(string currentTask)
        {
            if (ShowTraceMessages)
            {
                Console.WriteLine(currentTask);
            }
        }

        #endregion

    }
}
