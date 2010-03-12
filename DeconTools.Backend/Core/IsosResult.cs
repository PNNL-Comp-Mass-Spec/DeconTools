using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public abstract class IsosResult
    {
        public int MSFeatureID { get; set; }
        public IList<ResultFlag> Flags = new List<ResultFlag>();

        public Run Run { get; set; }

        public ScanSet ScanSet { get; set; }

        public IsotopicProfile IsotopicProfile { get; set; }


    }
}
