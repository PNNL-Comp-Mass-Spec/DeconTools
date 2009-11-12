using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public abstract class IsosResult
    {

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
