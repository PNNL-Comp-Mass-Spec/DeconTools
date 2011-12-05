using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public abstract class IPeak
    {
        #region Constructors
        #endregion
     

        #region Properties
        
        public abstract double XValue { get; set; }
        public abstract float Height { get; set; }
        public abstract float Width { get; set; }
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
