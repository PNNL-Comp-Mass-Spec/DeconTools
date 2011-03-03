using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public abstract class IPeak:IComparable
    {
        #region Constructors
        #endregion
        public enum SortKey {INTENSITY, MZ};

        #region Properties
        
        public abstract double XValue { get; set; }
        public abstract float Height { get; set; }
        public abstract float Width { get; set; }
        #endregion

        #region Public Methods
        public SortKey SortOnKey { get; set; }
        #endregion

        #region Private Methods
        #endregion

        #region IComparable Members

        public virtual int CompareTo(object obj)
        {
            IPeak secondPeak = obj as IPeak;
            if (secondPeak == null)
            {
                return -1;
            }
            else
            {

                if (SortOnKey == SortKey.INTENSITY)
                {
                    return this.Height.CompareTo(secondPeak.Height);
                }
                else
                {

                    //we need a system level global parameter that is the tolerance in PPM
                    //TODO
                    double toleranceInPPM = 20;
                    double differenceInPPM = Math.Abs(1000000 * (secondPeak.XValue - this.XValue) / this.XValue);

                    if (differenceInPPM <= toleranceInPPM)
                    {
                        return 0;
                    }
                }

            }

            return 1;
        }

        #endregion
    }
}
