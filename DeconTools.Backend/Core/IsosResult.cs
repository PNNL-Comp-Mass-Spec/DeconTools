using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public abstract class IsosResult
    {
        public int MSFeatureID { get; set; }
        public List<ResultFlag> Flags = new List<ResultFlag>();

        public abstract Run Run
        {
            get; 
            set; 
        }

        public abstract ScanSet ScanSet
        {
            get;
            set;
        }


        public abstract IsotopicProfile IsotopicProfile
        {
            get;
            set;
        }

        

    }
}
