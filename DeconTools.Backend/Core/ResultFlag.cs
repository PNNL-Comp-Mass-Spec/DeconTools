using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    public abstract class ResultFlag
    {
        #region Constructors
        public ResultFlag()
        {

        }

        public ResultFlag(string description)
        {
            this.Description = description;

        }

        #endregion

        #region Properties

        public string Description { get; set; }


        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
