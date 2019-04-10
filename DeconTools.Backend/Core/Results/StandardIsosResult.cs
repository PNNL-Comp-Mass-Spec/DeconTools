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
        /// Constructor that takes a run and scanSet
        /// </summary>
        /// <param name="run"></param>
        /// <param name="scanSet"></param>
        public StandardIsosResult(Run run, ScanSet scanSet)
        {
            Run = run;
            ScanSet = scanSet;
        }

    }
}
