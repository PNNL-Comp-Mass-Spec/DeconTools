
namespace DeconTools.Backend.Core
{
    public class UMC
    {
        #region Constructors
        #endregion

        #region Properties


        private int uMCIndex;

        public int UMCIndex
        {
            get { return uMCIndex; }
            set { uMCIndex = value; }
        }
        private int scanStart;

        public int ScanStart
        {
            get { return scanStart; }
            set { scanStart = value; }
        }
        private int scanEnd;

        public int ScanEnd
        {
            get { return scanEnd; }
            set { scanEnd = value; }
        }
        private int scanClassRep;


        public int ScanClassRep
        {
            get { return scanClassRep; }
            set { scanClassRep = value; }
        }
        private double nETClassRep;

        public double NETClassRep
        {
            get { return nETClassRep; }
            set { nETClassRep = value; }
        }
        private double uMCMonoMW;

        public double UMCMonoMW
        {
            get { return uMCMonoMW; }
            set { uMCMonoMW = value; }
        }
        private double uMCMWStDev;

        public double UMCMWStDev
        {
            get { return uMCMWStDev; }
            set { uMCMWStDev = value; }
        }
        private double uMCMWMin;

        public double UMCMWMin
        {
            get { return uMCMWMin; }
            set { uMCMWMin = value; }
        }
        private double uMCMWMax;

        public double UMCMWMax
        {
            get { return uMCMWMax; }
            set { uMCMWMax = value; }
        }
        private double uMCAbundance;

        public double UMCAbundance
        {
            get { return uMCAbundance; }
            set { uMCAbundance = value; }
        }
        private short classStatsChargeBasis;

        public short ClassStatsChargeBasis
        {
            get { return classStatsChargeBasis; }
            set { classStatsChargeBasis = value; }
        }
        private short chargeStateMin;

        public short ChargeStateMin
        {
            get { return chargeStateMin; }
            set { chargeStateMin = value; }
        }
        private short chargeStateMax;

        public short ChargeStateMax
        {
            get { return chargeStateMax; }
            set { chargeStateMax = value; }
        }
        private double uMCMZForChargeBasis;

        public double UMCMZForChargeBasis
        {
            get { return uMCMZForChargeBasis; }
            set { uMCMZForChargeBasis = value; }
        }
        private int uMCMemberCount;

        public int UMCMemberCount
        {
            get { return uMCMemberCount; }
            set { uMCMemberCount = value; }
        }
        private int uMCMemberCountUsedForAbu;

        public int UMCMemberCountUsedForAbu
        {
            get { return uMCMemberCountUsedForAbu; }
            set { uMCMemberCountUsedForAbu = value; }
        }
        private double uMCAverageFit;

        public double UMCAverageFit
        {
            get { return uMCAverageFit; }
            set { uMCAverageFit = value; }
        }
        private int pairIndex;

        public int PairIndex
        {
            get { return pairIndex; }
            set { pairIndex = value; }
        }

        private int pairMemberType;

        public int PairMemberType
        {
            get { return pairMemberType; }
            set { pairMemberType = value; }
        }


        private double expressionRatio;

        public double ExpressionRatio
        {
            get { return expressionRatio; }
            set { expressionRatio = value; }
        }
        private double expressionRatioStDev;

        public double ExpressionRatioStDev
        {
            get { return expressionRatioStDev; }
            set { expressionRatioStDev = value; }
        }
        private int expressionRatioChargeStateBasisCount;

        public int ExpressionRatioChargeStateBasisCount
        {
            get { return expressionRatioChargeStateBasisCount; }
            set { expressionRatioChargeStateBasisCount = value; }
        }
        private int expressionRatioMemberBasisCount;

        public int ExpressionRatioMemberBasisCount
        {
            get { return expressionRatioMemberBasisCount; }
            set { expressionRatioMemberBasisCount = value; }
        }
        private short multiMassTagHitCount;

        public short MultiMassTagHitCount
        {
            get { return multiMassTagHitCount; }
            set { multiMassTagHitCount = value; }
        }
        private int massTagID;

        public int MassTagID
        {
            get { return massTagID; }
            set { massTagID = value; }
        }
        private double massTagMonoMW;

        public double MassTagMonoMW
        {
            get { return massTagMonoMW; }
            set { massTagMonoMW = value; }
        }
        private double massTagNET;

        public double MassTagNET
        {
            get { return massTagNET; }
            set { massTagNET = value; }
        }
        private double massTagNETStDev;

        public double MassTagNETStDev
        {
            get { return massTagNETStDev; }
            set { massTagNETStDev = value; }
        }
        private double sLiCScore;

        public double SLiCScore
        {
            get { return sLiCScore; }
            set { sLiCScore = value; }
        }
        private double delSLiC;

        public double DelSLiC
        {
            get { return delSLiC; }
            set { delSLiC = value; }
        }
        private int memberCountMatchingMassTag;

        public int MemberCountMatchingMassTag
        {
            get { return memberCountMatchingMassTag; }
            set { memberCountMatchingMassTag = value; }
        }
        private bool isInternalStdMatch;

        public bool IsInternalStdMatch
        {
            get { return isInternalStdMatch; }
            set { isInternalStdMatch = value; }
        }
        private double peptideProphetProbability;

        public double PeptideProphetProbability
        {
            get { return peptideProphetProbability; }
            set { peptideProphetProbability = value; }
        }
        private string peptide;

        public string Peptide
        {
            get { return peptide; }
            set { peptide = value; }
        }

        public double StacScore { get; set; }

        public double StacUniquenessProbability { get; set; }

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
