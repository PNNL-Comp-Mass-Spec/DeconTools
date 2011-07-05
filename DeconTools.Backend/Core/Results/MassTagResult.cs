using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    public class MassTagResult : MassTagResultBase
    {
        #region Constructors
        public MassTagResult()
            : this(null)
        {
        }

        public MassTagResult(MassTag massTag)
        {
            this.IsotopicProfile = new IsotopicProfile();
            this.MassTag = massTag;
        }

        #endregion

        #region Properties
    

     
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion




    }
}
