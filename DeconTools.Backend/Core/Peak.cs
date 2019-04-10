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

        public Peak(double xValue, float yValue, float width)
            : this()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            XValue = xValue;

            // ReSharper disable once VirtualMemberCallInConstructor
            Height = yValue;

            // ReSharper disable once VirtualMemberCallInConstructor
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
            return (XValue.ToString("0.00000") + ";" + Height);
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
            return XValue.CompareTo(other.XValue);
        }

        private bool Equals(Peak other)
        {
            return XValue.Equals(other.XValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Peak) obj);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return XValue.GetHashCode();
        }
    }
}
