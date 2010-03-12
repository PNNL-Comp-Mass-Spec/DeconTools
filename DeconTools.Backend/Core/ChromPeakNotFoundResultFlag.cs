using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    public class ChromPeakNotFoundResultFlag : ResultFlag
    {
        #region Constructors
        public ChromPeakNotFoundResultFlag()
        {
            this.Description = "No Chrom peak found.";
        }

        public ChromPeakNotFoundResultFlag(string description)
        {
            this.Description = description;
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
