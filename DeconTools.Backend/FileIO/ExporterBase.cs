using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace DeconTools.Backend.FileIO
{
    public abstract class ExporterBase<T>
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        public abstract void ExportResults(IEnumerable<T> results);
        #endregion

        #region Private Methods
        #endregion
    }
}
