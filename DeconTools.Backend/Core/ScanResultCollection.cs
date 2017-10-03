using System.Collections.Generic;

namespace DeconTools.Backend.Core
{
    public class ScanResultCollection
    {
        public ScanResultCollection()
        {
            ScanResultList = new List<ScanResult>();
        }

        public List<ScanResult> ScanResultList { get; set; }
    }
}
