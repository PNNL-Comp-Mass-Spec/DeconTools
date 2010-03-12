using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class ScanSetCollection
    {
        public ScanSetCollection()
        {
            this.scanSetList = new List<ScanSet>();
        }

        private List<ScanSet> scanSetList;
        public List<ScanSet> ScanSetList
        {
            get { return scanSetList; }
            set { scanSetList = value; }
        }

        public ScanSet GetScanSet(int primaryNum)
        {
            if (this.scanSetList == null || this.scanSetList.Count == 0) return null;

            return (this.scanSetList.Find(p => p.PrimaryScanNumber == primaryNum));

        }

        public ScanSet GetNextMSScanSet(Run run, int primaryNum, bool ascending)
        {
            if (this.scanSetList == null || this.scanSetList.Count == 0) return null;

            ScanSet scan = this.scanSetList.Find(p => p.PrimaryScanNumber == primaryNum);

            if (run.GetMSLevel(scan.PrimaryScanNumber) == 1)
            {
                return scan;
            }
            else
            {
                if (ascending)
                {
                    scan = GetNextMSScanSet(run, ++primaryNum, ascending);
                }
                else
                {
                    scan = GetNextMSScanSet(run, --primaryNum, ascending);
                }
            }

            return scan;
        }




        public int GetLastScanSet()
        {
            if (this.ScanSetList == null || this.ScanSetList.Count == 0) return -1;
            return (this.ScanSetList[this.ScanSetList.Count - 1].PrimaryScanNumber);
        }
    }
}
