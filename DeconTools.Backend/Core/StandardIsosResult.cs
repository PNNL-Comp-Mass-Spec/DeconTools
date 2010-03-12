using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend
{
    [Serializable]
    public class StandardIsosResult : IsosResult
    {

        public StandardIsosResult()
        {

        }
        
        public StandardIsosResult(Run run, ScanSet scanset)
        {
            this.Run = run;
            this.ScanSet = scanset;
        }


    







    }
}
