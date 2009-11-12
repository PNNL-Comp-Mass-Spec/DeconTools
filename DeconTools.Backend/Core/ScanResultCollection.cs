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
        private List<ScanResult> scanResultList;

        public List<ScanResult> ScanResultList
        {
            get { return scanResultList; }
            set { scanResultList = value; }
        }
    }
}
