using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    public class O16O18_TResult : MassTagResultBase
    {
        #region Constructors
        public O16O18_TResult()
            : this(null)
        {

        }

        public O16O18_TResult(MassTag massTag)
        {
            this.MassTag = massTag;
            this.IsotopicProfile = new IsotopicProfile();

        }
        #endregion

        #region Properties

        public double RatioO16O18 { get; set; }

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        public double IntensityI4Adjusted { get; set; }
    }
}
