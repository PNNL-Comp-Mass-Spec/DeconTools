using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks
{
    public class HornDeconvolutor : Deconvolutor
    {
        #region Member Variables
        private DeconToolsV2.Peaks.clsPeak[] mspeakList;
        DeconToolsV2.HornTransform.clsHornTransformResults[] transformResults;
        #endregion

        #region Properties

        private bool isAbsolutePepIntensityUsed;

        public bool IsAbsolutePepIntensityUsed
        {
            get { return isAbsolutePepIntensityUsed; }
            set { isAbsolutePepIntensityUsed = value; }
        }
        private bool isMercuryCashingUsed;

        public bool IsMercuryCashingUsed
        {
            get { return isMercuryCashingUsed; }
            set { isMercuryCashingUsed = value; }
        }
        private bool isThrashed;

        public bool IsThrashed
        {
            get { return isThrashed; }
            set { isThrashed = value; }
        }
        private bool isO16O18Data;

        public bool IsO16O18Data
        {
            get { return isO16O18Data; }
            set { isO16O18Data = value; }
        }
        private bool isActualMonoMZUsed;

        public bool IsActualMonoMZUsed
        {
            get { return isActualMonoMZUsed; }
            set { isActualMonoMZUsed = value; }
        }

        private bool checkPatternsAgainstChargeOne;
        public bool CheckPatternsAgainstChargeOne
        {
            get { return checkPatternsAgainstChargeOne; }
            set { checkPatternsAgainstChargeOne = value; }
        }

        private bool isMZRangeUsed;

        public bool IsMZRangeUsed
        {
            get { return isMZRangeUsed; }
            set { isMZRangeUsed = value; }
        }

        private bool isCompleteFit;

        public bool IsCompleteFit
        {
            get { return isCompleteFit; }
            set { isCompleteFit = value; }
        }
        private bool isMSMSProcessed;

        public bool IsMSMSProcessed
        {
            get { return isMSMSProcessed; }
            set { isMSMSProcessed = value; }
        }


        private double minMZ;

        public double MinMZ
        {
            get { return minMZ; }
            set { minMZ = value; }
        }
        private double maxMZ;

        public double MaxMZ
        {
            get { return maxMZ; }
            set { maxMZ = value; }
        }

        private double leftFitStringencyFactor;

        public double LeftFitStringencyFactor
        {
            get { return leftFitStringencyFactor; }
            set { leftFitStringencyFactor = value; }
        }
        private double rightFitStringencyFactor;

        public double RightFitStringencyFactor
        {
            get { return rightFitStringencyFactor; }
            set { rightFitStringencyFactor = value; }
        }

        private double chargeCarrierMass;

        public double ChargeCarrierMass
        {
            get { return chargeCarrierMass; }
            set { chargeCarrierMass = value; }
        }
        private double deleteIntensityThreshold;

        public double DeleteIntensityThreshold
        {
            get { return deleteIntensityThreshold; }
            set { deleteIntensityThreshold = value; }
        }
        private double minIntensityForScore;

        public double MinIntensityForScore
        {
            get { return minIntensityForScore; }
            set { minIntensityForScore = value; }
        }
        private double minPeptideBackgroundRatio;

        public double MinPeptideBackgroundRatio
        {
            get { return minPeptideBackgroundRatio; }
            set { minPeptideBackgroundRatio = value; }
        }
        private double absoluteThresholdPeptideIntensity;

        public double AbsoluteThresholdPeptideIntensity
        {
            get { return absoluteThresholdPeptideIntensity; }
            set { absoluteThresholdPeptideIntensity = value; }
        }
        private double maxMWAllowed;

        public double MaxMWAllowed
        {
            get { return maxMWAllowed; }
            set { maxMWAllowed = value; }
        }
        private int maxChargeAllowed;

        public int MaxChargeAllowed
        {
            get { return maxChargeAllowed; }
            set { maxChargeAllowed = value; }
        }

        private double maxFitAllowed;

        public double MaxFitAllowed
        {
            get { return maxFitAllowed; }
            set { maxFitAllowed = value; }
        }

        private string averagineFormula;

        public string AveragineFormula
        {
            get { return averagineFormula; }
            set { averagineFormula = value; }
        }
        private string tagFormula;

        public string TagFormula
        {
            get { return tagFormula; }
            set { tagFormula = value; }
        }

        private Globals.IsotopicProfileFitType isotopicProfileFitType;

        public Globals.IsotopicProfileFitType IsotopicProfileFitType
        {
            get { return isotopicProfileFitType; }
            set { isotopicProfileFitType = value; }
        }
        private int numAllowedShoulderPeaks;

        public int NumAllowedShoulderPeaks
        {
            get { return numAllowedShoulderPeaks; }
            set { numAllowedShoulderPeaks = value; }
        }

        DeconToolsV2.HornTransform.clsHornTransformParameters deconEngineHornParameters;

        public DeconToolsV2.HornTransform.clsHornTransformParameters DeconEngineHornParameters
        {
            get { return deconEngineHornParameters; }
            set { deconEngineHornParameters = value; }
        }

        bool saveMemoryByAddingMinimalPeaksToIsotopicProfile;

        public bool SaveMemoryByAddingMinimalPeaksToIsotopicProfile
        {
            get { return saveMemoryByAddingMinimalPeaksToIsotopicProfile; }
            set { saveMemoryByAddingMinimalPeaksToIsotopicProfile = value; }
        }

        private DeconToolsV2.HornTransform.clsHornTransform transformer;

        internal DeconToolsV2.HornTransform.clsHornTransform Transformer
        {
            get
            {
                if (transformer == null)
                {
                    transformer = new DeconToolsV2.HornTransform.clsHornTransform();
                    transformer.TransformParameters = loadDeconEngineHornParameters();
                }

                return transformer;
            }
            set { transformer = value; }
        }

        private int numPeaksUsedInAbundance;
        public int NumPeaksUsedInAbundance
        {
            get { return numPeaksUsedInAbundance; }
            set { numPeaksUsedInAbundance = value; }
        }



        #endregion

        #region Constructors
        public HornDeconvolutor()
            : this(new DeconToolsV2.HornTransform.clsHornTransformParameters())
        {

        }

        private void setDefaults()
        {
            //TODO:   Finish this
            this.deleteIntensityThreshold = 10;


        }

        public HornDeconvolutor(DeconToolsV2.HornTransform.clsHornTransformParameters hornParameters)
        {
            convertDeconEngineHornParameters(hornParameters);
            saveMemoryByAddingMinimalPeaksToIsotopicProfile = false;       // I later found out that this doesn't make much difference
        }

        private void convertDeconEngineHornParameters(DeconToolsV2.HornTransform.clsHornTransformParameters hornParameters)
        {
            this.absoluteThresholdPeptideIntensity = hornParameters.AbsolutePeptideIntensity;
            this.averagineFormula = hornParameters.AveragineFormula;
            this.chargeCarrierMass = hornParameters.CCMass;
            this.checkPatternsAgainstChargeOne = hornParameters.CheckAllPatternsAgainstCharge1;
            this.deleteIntensityThreshold = hornParameters.DeleteIntensityThreshold;
            this.isAbsolutePepIntensityUsed = hornParameters.UseAbsolutePeptideIntensity;
            this.isActualMonoMZUsed = hornParameters.IsActualMonoMZUsed;
            this.isCompleteFit = hornParameters.CompleteFit;
            this.isMercuryCashingUsed = hornParameters.UseMercuryCaching;
            this.isMSMSProcessed = hornParameters.ProcessMSMS;
            this.isMZRangeUsed = hornParameters.UseMZRange;
            this.isO16O18Data = hornParameters.O16O18Media;
            this.isotopicProfileFitType = convertDeconEngineIsoFitType(hornParameters.IsotopeFitType);
            this.isThrashed = hornParameters.ThrashOrNot;
            this.leftFitStringencyFactor = hornParameters.LeftFitStringencyFactor;
            this.maxChargeAllowed = hornParameters.MaxCharge;
            this.maxFitAllowed = hornParameters.MaxFit;
            this.maxMWAllowed = hornParameters.MaxMW;
            this.maxMZ = hornParameters.MaxMZ;      //TODO: review this later
            this.minMZ = hornParameters.MinMZ;      //TODO: review this later
            this.minIntensityForScore = hornParameters.MinIntensityForScore;
            this.minPeptideBackgroundRatio = hornParameters.PeptideMinBackgroundRatio;
            this.numAllowedShoulderPeaks = hornParameters.NumPeaksForShoulder;
            this.rightFitStringencyFactor = hornParameters.RightFitStringencyFactor;
            this.tagFormula = hornParameters.TagFormula;
            this.NumPeaksUsedInAbundance = hornParameters.NumPeaksUsedInAbundance;

        }

        private Globals.IsotopicProfileFitType convertDeconEngineIsoFitType(DeconToolsV2.enmIsotopeFitType enmIsotopeFitType)
        {
            switch (enmIsotopeFitType)
            {
                case DeconToolsV2.enmIsotopeFitType.AREA:
                    return Globals.IsotopicProfileFitType.AREA;
                case DeconToolsV2.enmIsotopeFitType.CHISQ:
                    return Globals.IsotopicProfileFitType.CHISQ;
                case DeconToolsV2.enmIsotopeFitType.PEAK:
                    return Globals.IsotopicProfileFitType.PEAK;
                default:
                    return Globals.IsotopicProfileFitType.AREA;
            }
        }

        #endregion


        #region Public Methods
        public override void deconvolute(ResultCollection resultList)
        {

            float backgroundIntensity = (float)resultList.Run.CurrentBackgroundIntensity;
            float minPeptideIntensity = (float)(resultList.Run.CurrentBackgroundIntensity * this.MinPeptideBackgroundRatio);

            float[] xvals = new float[1];
            float[] yvals = new float[1];
            resultList.Run.XYData.GetXYValuesAsSingles(ref xvals, ref yvals);

            mspeakList = resultList.Run.DeconToolsPeakList;
            transformResults = new DeconToolsV2.HornTransform.clsHornTransformResults[0];

            this.Transformer.PerformTransform(backgroundIntensity, minPeptideIntensity, ref xvals, ref yvals, ref mspeakList, ref transformResults);
            GenerateResults(transformResults, mspeakList, resultList);




            //addDataToScanResults(transformResults.Length, resultList.GetCurrentScanResult());


        }

        private DeconToolsV2.HornTransform.clsHornTransformParameters loadDeconEngineHornParameters()
        {
            DeconToolsV2.HornTransform.clsHornTransformParameters hornParameters = new DeconToolsV2.HornTransform.clsHornTransformParameters();
            hornParameters.AbsolutePeptideIntensity = this.absoluteThresholdPeptideIntensity;
            hornParameters.AveragineFormula = this.averagineFormula;
            hornParameters.CCMass = this.chargeCarrierMass;
            hornParameters.CheckAllPatternsAgainstCharge1 = this.checkPatternsAgainstChargeOne;
            hornParameters.CompleteFit = this.isCompleteFit;
            hornParameters.DeleteIntensityThreshold = this.deleteIntensityThreshold;
            hornParameters.IsActualMonoMZUsed = this.isActualMonoMZUsed;
            hornParameters.IsotopeFitType = convertFitTypeToDeconEngineType(this.isotopicProfileFitType);
            hornParameters.LeftFitStringencyFactor = this.leftFitStringencyFactor;
            hornParameters.MaxCharge = (short)this.maxChargeAllowed;
            hornParameters.MaxFit = this.maxFitAllowed;
            hornParameters.MaxMW = this.maxMWAllowed;
            hornParameters.MinMZ = this.minMZ;
            hornParameters.MaxMZ = this.maxMZ;
            hornParameters.MinIntensityForScore = this.minIntensityForScore;
            // hornParameters.MinS2N = ??     //TODO:  verify that this is no longer used
            hornParameters.NumPeaksForShoulder = (short)this.numAllowedShoulderPeaks;
            hornParameters.O16O18Media = this.isO16O18Data;
            hornParameters.PeptideMinBackgroundRatio = this.minPeptideBackgroundRatio;
            hornParameters.ProcessMSMS = this.isMSMSProcessed;
            hornParameters.RightFitStringencyFactor = this.rightFitStringencyFactor;
            hornParameters.TagFormula = this.tagFormula;
            hornParameters.ThrashOrNot = this.isThrashed;
            hornParameters.UseAbsolutePeptideIntensity = this.isAbsolutePepIntensityUsed;
            hornParameters.UseMercuryCaching = this.isMercuryCashingUsed;
            hornParameters.UseMZRange = this.isMZRangeUsed;
            hornParameters.NumPeaksUsedInAbundance = (short)this.NumPeaksUsedInAbundance;

            /* The following DeconEngine Horn parameters are used elsewhere in the new framework.  For
             * example, Smoothing is now a Task that is performed after the MS is generated by the MSGenerator Task
             * Smoothing-related parameters will be found within the task. 
             
            hornParameters.UseScanRange;
            hornParameters.MinScan;
            hornParameters.MaxScan;
            hornParameters.SGNumLeft;
            hornParameters.SGNumRight;
            hornParameters.SGOrder;
            hornParameters.SumSpectra;
            hornParameters.SumSpectraAcrossScanRange;
            hornParameters.UseSavitzkyGolaySmooth;
            hornParameters.ZeroFill;
            hornParameters.NumZerosToFill;
            hornParameters.ElementIsotopeComposition;
            */

            return hornParameters;
        }

        private DeconToolsV2.enmIsotopeFitType convertFitTypeToDeconEngineType(Globals.IsotopicProfileFitType isotopicProfileFitType)
        {
            switch (isotopicProfileFitType)
            {
                case Globals.IsotopicProfileFitType.Undefined:
                    throw new Exception("Error.  IsotopicProfile fit type has not been defined. Cannot be used in HornTransform");
                case Globals.IsotopicProfileFitType.AREA:
                    return DeconToolsV2.enmIsotopeFitType.AREA;
                case Globals.IsotopicProfileFitType.CHISQ:
                    return DeconToolsV2.enmIsotopeFitType.CHISQ;
                case Globals.IsotopicProfileFitType.PEAK:
                    return DeconToolsV2.enmIsotopeFitType.PEAK;
                default:
                    return DeconToolsV2.enmIsotopeFitType.AREA;

            }
        }
        #endregion

        #region Private Methods
        private void GenerateResults(DeconToolsV2.HornTransform.clsHornTransformResults[] transformResults,
       DeconToolsV2.Peaks.clsPeak[] mspeakList, ResultCollection resultList)
        {
            resultList.Run.CurrentScanSet.NumIsotopicProfiles = 0;   //reset to 0;


            foreach (DeconToolsV2.HornTransform.clsHornTransformResults hornResult in transformResults)
            {
                IsosResult result = resultList.CreateIsosResult();
                IsotopicProfile profile = new IsotopicProfile();
                profile.AverageMass = hornResult.mdbl_average_mw;
                profile.ChargeState = hornResult.mshort_cs;
                profile.MonoIsotopicMass = hornResult.mdbl_mono_mw;
                profile.Score = hornResult.mdbl_fit;
                profile.MostAbundantIsotopeMass = hornResult.mdbl_most_intense_mw;


                GetIsotopicProfile(hornResult.marr_isotope_peak_indices, mspeakList, ref profile);


                if (NumPeaksUsedInAbundance == 1)  // fyi... this is typical
                {
                    profile.IntensityAggregate = hornResult.mint_abundance;
                }
                else
                {
                    profile.IntensityAggregate = sumPeaks(profile, NumPeaksUsedInAbundance, hornResult.mint_abundance);
                }

                profile.MonoPlusTwoAbundance = profile.GetMonoPlusTwoAbundance();
                profile.MonoPeakMZ = profile.GetMZ();

                result.IsotopicProfile = profile;

                this.CombineDeconResults(resultList, result, DeconResultComboMode.simplyAddIt);
                //resultList.ResultList.Add(result);
                resultList.Run.CurrentScanSet.NumIsotopicProfiles++;
            }
        }

        private double sumPeaks(IsotopicProfile profile, int NumPeaksUsedInAbundance, int defaultVal)
        {
            if (profile.Peaklist == null || profile.Peaklist.Count == 0) return defaultVal;
            List<float> peakListIntensities = new List<float>();
            foreach (MSPeak peak in profile.Peaklist)
            {
                peakListIntensities.Add(peak.Height);

            }
            peakListIntensities.Sort();
            peakListIntensities.Reverse();    // i know... this isn't the best way to do this!
            double summedIntensities = 0;

            for (int i = 0; i < peakListIntensities.Count; i++)
            {
                if (i < numPeaksUsedInAbundance)
                {
                    summedIntensities += peakListIntensities[i];
                }


            }

            return summedIntensities;

        }





        private void GetIsotopicProfile(int[] peakIndexList, DeconToolsV2.Peaks.clsPeak[] peakdata, ref IsotopicProfile profile)
        {
            if (peakIndexList == null || peakIndexList.Length == 0) return;
            DeconToolsV2.Peaks.clsPeak deconMonopeak = lookupPeak(peakIndexList[0], peakdata);

            MSPeak monoPeak = convertDeconPeakToMSPeak(deconMonopeak);
            profile.Peaklist.Add(monoPeak);

            if (peakIndexList.Length == 1) return;           //only one peak in the DeconEngine's profile    

            for (int i = 1; i < peakIndexList.Length; i++)     //start with second peak and add each peak to profile
            {
                DeconToolsV2.Peaks.clsPeak deconPeak = lookupPeak(peakIndexList[i], peakdata);
                MSPeak peakToBeAdded = convertDeconPeakToMSPeak(deconPeak);
                profile.Peaklist.Add(peakToBeAdded);
            }




        }

        private DeconToolsV2.Peaks.clsPeak lookupPeak(int index, DeconToolsV2.Peaks.clsPeak[] peakdata)
        {
            return peakdata[index];
        }

        private MSPeak convertDeconPeakToMSPeak(DeconToolsV2.Peaks.clsPeak deconPeak)
        {

            MSPeak peak = new MSPeak();
            peak.XValue = deconPeak.mdbl_mz;
            peak.Width = (float)deconPeak.mdbl_FWHM;
            peak.SN = (float)deconPeak.mdbl_SN;
            peak.Height = (int)deconPeak.mdbl_intensity;

            return peak;
        }

        #endregion





    }
}
