using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    public class O16O18_TResult : MassTagResultBase
    {
        #region Constructors
        public O16O18_TResult() : base() { }

        public O16O18_TResult(TargetBase target) : base(target) { }
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
