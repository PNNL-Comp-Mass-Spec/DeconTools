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

        public SortedDictionary<int, float> GetScanNETLookupTable()
        {
            SortedDictionary<int, float> lookupTable = new SortedDictionary<int, float>();
            var distinctItems = UMCList.GroupBy(p => p.ScanClassRep).Select(p => p.First());    //this creates a list of UMCs distinct with reference to the ScanClassRep field
            List<UMC> tempUMCs = distinctItems.ToList();

            foreach (UMC umc in tempUMCs)
            {
                lookupTable.Add(umc.ScanClassRep,(float)umc.NETClassRep);
            }
            return lookupTable;

        }


        public double GetNETValueForScan(int scanNum)
        {
            double netVal = -1;
            Check.Require(UMCList != null && UMCList.Count > 0, "Cannot retrieve NET value info. UMCs not loaded yet.");



            var distinctItems = UMCList.GroupBy(p => p.ScanClassRep).Select(p => p.First());
            List<UMC> tempUMCs = distinctItems.ToList();
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

        public List<UMC> FilterUMCsByMassTagMatch(List<int> massTagIDList)
        {
            if (this.UMCList == null || this.UMCList.Count == 0) return this.UMCList;

            List<UMC> filteredUMCs = new List<UMC>();

            List<int> alreadyAddedMassTags = new List<int>();

            foreach (var umc in this.UMCList)
            {
                if (massTagIDList.Contains(umc.MassTagID))
                {
                    if (alreadyAddedMassTags.Contains(umc.MassTagID)) continue;
                    filteredUMCs.Add(umc);
                    alreadyAddedMassTags.Add(umc.MassTagID);
                }



            }




            return filteredUMCs;


        }


        public List<UMC> FilterOutPairedData()
        {
            if (this.UMCList == null || this.UMCList.Count == 0) return this.UMCList;
            List<UMC> filteredUMCs = new List<UMC>();

            List<int> alreadyAddedMassTags = new List<int>();
            foreach (var umc in this.UMCList)
            {
                if (umc.PairIndex != -1)
                {
                    if (alreadyAddedMassTags.Contains(umc.MassTagID)) continue;
                    filteredUMCs.Add(umc);
                    alreadyAddedMassTags.Add(umc.MassTagID);
                }

            }

            return filteredUMCs;


        }


        public void DisplayBasicInfo()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("umcIndex\tnet\tscan\tumc_mw\tumc_Z\tumc_mz\tumc_Abundance\tumc_Fit\tumc_members\tslic\tdelSlic\tmass_tag_id\n");
            foreach (var umc in this.UMCList)
            {
                sb.Append(umc.UMCIndex);
                sb.Append("\t");
                sb.Append(umc.NETClassRep.ToString("0.000"));
                sb.Append("\t");
                sb.Append(umc.ScanClassRep);
                sb.Append("\t");
                sb.Append(umc.UMCMonoMW.ToString("0.0000"));
                sb.Append("\t");
                sb.Append(umc.ClassStatsChargeBasis);
                sb.Append("\t");
                sb.Append((umc.UMCMonoMW / umc.ClassStatsChargeBasis + Globals.PROTON_MASS).ToString("0.0000"));
                sb.Append("\t");
                sb.Append(umc.UMCAbundance.ToString("0"));
                sb.Append("\t");
                sb.Append(umc.UMCAverageFit.ToString("0.000"));
                sb.Append("\t");
                sb.Append(umc.UMCMemberCount);
                sb.Append("\t");
                sb.Append(umc.SLiCScore.ToString("0.0"));
                sb.Append("\t");
                sb.Append(umc.DelSLiC.ToString("0.0"));
                sb.Append("\t");
                sb.Append(umc.MassTagID);
                sb.Append("\n"); 
            }

            Console.WriteLine(sb.ToString());
        }



        public void DisplayUMCExpressionInfo()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("umcIndex\tscan\tumc_mw\tumc_Z\tumc_mz\tslic\tdelSlic\tmass_tag_id\tRatio\n");
            foreach (var umc in this.UMCList)
            {
                sb.Append(umc.UMCIndex);
                sb.Append("\t");
                sb.Append(umc.ScanClassRep);
                sb.Append("\t");
                sb.Append(umc.UMCMonoMW.ToString("0.0000"));
                sb.Append("\t");
                sb.Append(umc.ClassStatsChargeBasis);
                sb.Append("\t");
                sb.Append((umc.UMCMonoMW / umc.ClassStatsChargeBasis + Globals.PROTON_MASS).ToString("0.0000"));
                sb.Append("\t");
                sb.Append(umc.SLiCScore.ToString("0.0"));
                sb.Append("\t");
                sb.Append(umc.DelSLiC.ToString("0.0"));
                sb.Append("\t");
                sb.Append(umc.MassTagID);
                sb.Append("\t");

                if (umc.ExpressionRatio < 0)
                {
                    sb.Append(-1);
                }
                else
                {
                    sb.Append(umc.ExpressionRatio.ToString("0.000"));
                }

                sb.Append("\n");

            }

            Console.WriteLine(sb.ToString());



        }




        #endregion

        #region Private Methods
        #endregion
    }
}
