using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.ProcessingTasks.Deconvoluters
{
    //this functions to do nothing   :)    The purpose of this was to
    //avoid doing any deconvolution and just allow regular processing
    public class NullDeconvolutor:Deconvolutor
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
        public override void Deconvolute(DeconTools.Backend.Core.ResultCollection resultList)
        {
            //do nothing
           
        }
    }
}
