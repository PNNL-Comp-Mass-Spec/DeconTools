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

        public List<UMC> UMCList { get; set; }

        #endregion

        #region Public Methods

        public List<ScanNETPair> GetScanNETLookupTable()
        {
            var lookupTable = new List<ScanNETPair>();
            var distinctItems = UMCList.GroupBy(p => p.ScanClassRep).Select(p => p.First());    //this creates a list of UMCs distinct with reference to the ScanClassRep field
            var tempUMCs = distinctItems.ToList();



            foreach (var umc in tempUMCs)
            {
                var scannetpair = new ScanNETPair((float)umc.ScanClassRep, (float)umc.NETClassRep);

                lookupTable.Add(scannetpair);
            }
            return lookupTable;
        }

       

        public double GetNETValueForScan(int scanNum)
        {
            double netVal = -1;
            Check.Require(UMCList != null && UMCList.Count > 0, "Cannot retrieve NET value info. UMCs not loaded yet.");



            var distinctItems = UMCList.GroupBy(p => p.ScanClassRep).Select(p => p.First());
            var tempUMCs = distinctItems.ToList();
            //create a temp copy of list


            var umcWithSameScanNum = tempUMCs.Find(p => p.ScanClassRep == scanNum);
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

            var filteredUMCs = new List<UMC>();

            var alreadyAddedMassTags = new List<int>();

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
            var filteredUMCs = new List<UMC>();

            var alreadyAddedMassTags = new List<int>();
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
            var data = new List<string>
            {
                "umcIndex",
                "net",
                "scan",
                "umc_mw",
                "umc_Z",
                "umc_mz",
                "umc_Abundance",
                "umc_Fit",
                "umc_members",
                "slic",
                "delSlic",
                "mass_tag_id"
            };

            Console.WriteLine(string.Join("\t", data));

            foreach (var umc in UMCList)
            {
                data.Clear();
                data.Add(umc.UMCIndex.ToString());
                data.Add(umc.NETClassRep.ToString("0.000"));
                data.Add(umc.ScanClassRep.ToString());
                data.Add(umc.UMCMonoMW.ToString("0.0000"));
                data.Add(umc.ClassStatsChargeBasis.ToString());
                data.Add((umc.UMCMonoMW / umc.ClassStatsChargeBasis + Globals.PROTON_MASS).ToString("0.0000"));
                data.Add(umc.UMCAbundance.ToString("0"));
                data.Add(umc.UMCAverageFit.ToString("0.000"));
                data.Add(umc.UMCMemberCount.ToString());
                data.Add(umc.SLiCScore.ToString("0.0"));
                data.Add(umc.DelSLiC.ToString("0.0"));
                data.Add(umc.MassTagID.ToString());
            }

            Console.WriteLine(string.Join("\t", data));
        }



        public void DisplayUMCExpressionInfo()
        {
            var data = new List<string>
            {
                "umcIndex",
                "scan",
                "umc_mw",
                "umc_Z",
                "umc_mz",
                "slic",
                "delSlic",
                "mass_tag_id",
                "Ratio"

            };

            Console.WriteLine(string.Join("\t", data));

            foreach (var umc in UMCList)
            {
                data.Clear();
                data.Add(umc.UMCIndex.ToString());
                data.Add(umc.ScanClassRep.ToString());
                data.Add(umc.UMCMonoMW.ToString("0.0000"));
                data.Add(umc.ClassStatsChargeBasis.ToString());
                data.Add((umc.UMCMonoMW / umc.ClassStatsChargeBasis + Globals.PROTON_MASS).ToString("0.0000"));
                data.Add(umc.SLiCScore.ToString("0.0"));
                data.Add(umc.DelSLiC.ToString("0.0"));
                data.Add(umc.MassTagID.ToString());

                if (umc.ExpressionRatio < 0)
                {
                    data.Add("-1");
                }
                else
                {
                    data.Add(umc.ExpressionRatio.ToString("0.000"));
                }

                Console.WriteLine(string.Join("\t", data));
            }

        }




        #endregion

        #region Private Methods
        #endregion
    }
}
