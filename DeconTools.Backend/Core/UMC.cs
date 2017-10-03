
namespace DeconTools.Backend.Core
{
    public class UMC
    {

        public int UMCIndex { get; set; }

        public int ScanStart { get; set; }

        public int ScanEnd { get; set; }

        public int ScanClassRep { get; set; }

        public double NETClassRep { get; set; }

        public double UMCMonoMW { get; set; }

        public double UMCMWStDev { get; set; }

        public double UMCMWMin { get; set; }

        public double UMCMWMax { get; set; }

        public double UMCAbundance { get; set; }

        public short ClassStatsChargeBasis { get; set; }

        public short ChargeStateMin { get; set; }

        public short ChargeStateMax { get; set; }

        public double UMCMZForChargeBasis { get; set; }

        public int UMCMemberCount { get; set; }

        public int UMCMemberCountUsedForAbu { get; set; }

        public double UMCAverageFit { get; set; }

        public int PairIndex { get; set; }

        public int PairMemberType { get; set; }

        public double ExpressionRatio { get; set; }

        public double ExpressionRatioStDev { get; set; }

        public int ExpressionRatioChargeStateBasisCount { get; set; }

        public int ExpressionRatioMemberBasisCount { get; set; }

        public short MultiMassTagHitCount { get; set; }

        public int MassTagID { get; set; }

        public double MassTagMonoMW { get; set; }

        public double MassTagNET { get; set; }

        public double MassTagNETStDev { get; set; }

        public double SLiCScore { get; set; }

        public double DelSLiC { get; set; }

        public int MemberCountMatchingMassTag { get; set; }

        public bool IsInternalStdMatch { get; set; }

        public double PeptideProphetProbability { get; set; }

        public string Peptide { get; set; }

        public double StacScore { get; set; }

        public double StacUniquenessProbability { get; set; }

    }
}
