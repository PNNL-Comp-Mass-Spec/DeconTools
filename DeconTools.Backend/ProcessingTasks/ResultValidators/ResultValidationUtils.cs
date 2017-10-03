using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ResultValidators
{
    public class ResultValidationUtils
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        public static int GetFlagCode(IList<ResultFlag>resultFlags)
        {
            if (resultFlags == null || resultFlags.Count == 0) return -1;
            

            //TODO: add code later, but for now, will return 1;
            return 1;



        }


        #endregion

        #region Private Methods
        #endregion

        public static string GetStringFlagCode(IList<ResultFlag> resultFlags)
        {
            var flagCode =  GetFlagCode(resultFlags);
            if (flagCode == -1) return string.Empty;
            else
            {
                return flagCode.ToString();
            }
        }
    }
}
