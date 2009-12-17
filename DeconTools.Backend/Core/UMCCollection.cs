using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Utilities;

namespace DeconTools.Backend.Core
{
    public class UMCCollection
    {
        #region Constructors
        public UMCCollection()
        {
            UMCList = new List<UMC>();
        }
        #endregion

        #region Properties
        private List<UMC> uMCList;

        public List<UMC> UMCList
        {
            get { return uMCList; }
            set { uMCList = value; }
        }
        #endregion

        #region Public Methods

        public Dictionary<int, double> GetScanNETLookupTable()
        {
            Dictionary<int, double> lookupTable = new Dictionary<int, double>();
            var distinctItems = UMCList.GroupBy(p => p.ScanClassRep).Select(p => p.First());    //this creates a list of UMCs distinct with reference to the ScanClassRep field
            List<UMC> tempUMCs = distinctItems.ToList();

            foreach (UMC umc in tempUMCs)
            {
                lookupTable.Add(umc.ScanClassRep, umc.NETClassRep);
            }
            return lookupTable;

        }

        
        public double GetNETValueForScan(int scanNum)
        {
            double netVal = -1;
            Check.Require(UMCList != null && UMCList.Count > 0, "Cannot retrieve NET value info. UMCs not loaded yet.");



            var distinctItems = UMCList.GroupBy(p => p.ScanClassRep).Select(p => p.First());
            List<UMC>tempUMCs= distinctItems.ToList();
               //create a temp copy of list


            UMC umcWithSameScanNum = tempUMCs.Find(p => p.ScanClassRep == scanNum);
            if (umcWithSameScanNum != null)
            {
                return umcWithSameScanNum.NETClassRep;
            }
            else
            {
                tempUMCs.Sort(delegate(UMC umc1, UMC umc2)
                {
                    return umc1.ScanClassRep.CompareTo(umc2.ScanClassRep);
                });

            }


            return netVal;

        }


        #endregion

        #region Private Methods
        #endregion
    }
}
