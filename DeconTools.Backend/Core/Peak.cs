using System;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class Peak
    {
        #region Constructors

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

        #region Private Methods
        #endregion

   
    }
}
