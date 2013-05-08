using System;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class Peak : IComparable<Peak>
    {
        #region Constructors

        public Peak()
        {
            DataIndex = -1;
        }

        public Peak(double xvalue, float yvalue, float width)
            : this()
        {
            XValue = xvalue;
            Height = yvalue;
            Width = width;


        }

        #endregion


        #region Properties

        public virtual double XValue { get; set; }
        public virtual float Height { get; set; }
        public virtual float Width { get; set; }

        /// <summary>
        /// The index of the raw xy data from which the peak originates. 
        /// </summary>
        public int DataIndex { get; set; }
        #endregion

        #region Public Methods

        public override string ToString()
        {
            return (this.XValue.ToString("0.00000") + ";" + this.Height);
        }
        #endregion

        public XYData GetTheorPeakData(double fwhm)
        {
            return GetTheorPeakData(fwhm, 101);
        }

        public XYData GetTheorPeakData(double fwhm, int numPointsPerPeak)
        {
            return TheorXYDataCalculationUtilities.GetTheorPeakData(XValue, Height, fwhm, numPointsPerPeak);
        }

        public int CompareTo(Peak other)
        {
            return this.XValue.CompareTo(other.XValue);
        }

        protected bool Equals(Peak other)
        {
            return XValue.Equals(other.XValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Peak) obj);
        }

        public override int GetHashCode()
        {
            return XValue.GetHashCode();
        }
    }
}
