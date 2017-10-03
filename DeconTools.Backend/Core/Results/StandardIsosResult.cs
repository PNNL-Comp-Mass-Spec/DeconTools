using System;
using DeconTools.Backend.Core;

namespace DeconTools.Backend
{
    [Serializable]
    public class StandardIsosResult : IsosResult
    {
        /// <summary>
        /// Empty constructor
        /// </summary>
        public StandardIsosResult()
        {
        }

        /// <summary>
        /// Constructor that takes a run and scanset
        /// </summary>
        /// <param name="run"></param>
        /// <param name="scanset"></param>
        public StandardIsosResult(Run run, ScanSet scanset)
        {
            Run = run;
            ScanSet = scanset;
        }

    }
}
