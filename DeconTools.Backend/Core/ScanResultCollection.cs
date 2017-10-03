using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Core
{
    public class ScanResultCollection
    {
        public ScanResultCollection()
        {
            this.scanResultList = new List<ScanResult>();
        }

        public List<ScanResult> ScanResultList { get; set; }
    }
}
