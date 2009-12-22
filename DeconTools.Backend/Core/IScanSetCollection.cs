using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    public abstract class IScanSetCollection
    {

        public Run Run { get; set; }

        public abstract List<IScanSet> ScanSetList
        {
            get;
            set;
        }

        public abstract void Create(int minValue, int maxValue);


        public IScanSet GetScanSet(int primaryNum)
        {
            if (this.ScanSetList == null || this.ScanSetList.Count == 0) return null;

            return (this.ScanSetList.Find(p => p.PrimaryScanValue == primaryNum));

        }

        public IScanSet GetNextMSScanSet(Run run, int primaryNum, bool ascending)
        {
            if (this.ScanSetList == null || this.ScanSetList.Count == 0) return null;

            IScanSet scan = this.ScanSetList.Find(p => p.PrimaryScanValue == primaryNum);

            if (run.GetMSLevel(scan.PrimaryScanValue) == 1)
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

    }
}
