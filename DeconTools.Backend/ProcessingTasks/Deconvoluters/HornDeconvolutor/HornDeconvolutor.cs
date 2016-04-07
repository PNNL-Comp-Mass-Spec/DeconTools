using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.Runs;
using DeconToolsV2.HornTransform;
#if !Disable_DeconToolsV2
using DeconToolsV2.Peaks;
#endif

namespace DeconTools.Backend.ProcessingTasks
{
#if !Disable_DeconToolsV2

    public class HornDeconvolutor : Deconvolutor
    {
        #region Member Variables
        
        private DeconToolsV2.Peaks.clsPeak[] mSpeakList;
        DeconToolsV2.HornTransform.clsHornTransformResults[] mTransformResults;

        #endregion

        #region Properties

        public bool IsAbsolutePepIntensityUsed { get; set; }

        public bool IsMercuryCashingUsed { get; set; }

        public bool IsThrashed { get; set; }

        public bool IsO16O18Data { get; set; }

        public bool IsActualMonoMZUsed { get; set; }

        public bool CheckPatternsAgainstChargeOne { get; set; }

        public bool IsMZRangeUsed { get; set; }

        public bool IsCompleteFit { get; set; }

        public bool IsMSMSProcessed { get; set; }

        public double MinMZ { get; set; }

        public double MaxMZ { get; set; }

        public double LeftFitStringencyFactor { get; set; }

        public double RightFitStringencyFactor { get; set; }

        public double ChargeCarrierMass { get; set; }

        public double DeleteIntensityThreshold { get; set; }

        public double MinIntensityForScore { get; set; }

        public double MinPeptideBackgroundRatio { get; set; }

        public double AbsoluteThresholdPeptideIntensity { get; set; }

        public double MaxMWAllowed { get; set; }

        public int MaxChargeAllowed { get; set; }

        public double MaxFitAllowed { get; set; }

        public string AveragineFormula { get; set; }

        public string TagFormula { get; set; }

        public Globals.IsotopicProfileFitType IsotopicProfileFitType { get; set; }

        public int NumAllowedShoulderPeaks { get; set; }

        public DeconToolsV2.HornTransform.clsHornTransformParameters DeconEngineHornParameters { get; set; }

        public bool SaveMemoryByAddingMinimalPeaksToIsotopicProfile { get; set; }

        private DeconToolsV2.HornTransform.clsHornTransform mTransformer;

        internal DeconToolsV2.HornTransform.clsHornTransform Transformer
        {
            get
            {
                if (mTransformer == null)
                {
                    mTransformer = new DeconToolsV2.HornTransform.clsHornTransform
                    {
                        TransformParameters = loadDeconEngineHornParameters()
                    };
                }

                return mTransformer;
            }
            set { mTransformer = value; }
        }

        public int NumPeaksUsedInAbundance { get; set; }

        #endregion

        #region Constructors
        public HornDeconvolutor()
            : this(new DeconToolsV2.HornTransform.clsHornTransformParameters())
        {

        }

        private void setDefaults()
        {
            //TODO:   Finish this
            this.DeleteIntensityThreshold = 10;


        }

        public HornDeconvolutor(DeconToolsV2.HornTransform.clsHornTransformParameters hornParameters)
        {
            convertDeconEngineHornParameters(hornParameters);
            SaveMemoryByAddingMinimalPeaksToIsotopicProfile = false;       // I later found out that this doesn't make much difference
        }

        public HornDeconvolutor(DeconToolsParameters deconParameters)
        {
            AbsoluteThresholdPeptideIntensity = deconParameters.ThrashParameters.AbsolutePeptideIntensity;
            AveragineFormula = deconParameters.ThrashParameters.AveragineFormula;
            ChargeCarrierMass = deconParameters.ThrashParameters.ChargeCarrierMass;
            CheckPatternsAgainstChargeOne = deconParameters.ThrashParameters.CheckAllPatternsAgainstChargeState1;
            DeleteIntensityThreshold = deconParameters.ThrashParameters.MinIntensityForDeletion;
            IsAbsolutePepIntensityUsed = deconParameters.ThrashParameters.UseAbsoluteIntensity;
            IsActualMonoMZUsed = false;
            IsCompleteFit = deconParameters.ThrashParameters.CompleteFit;
            IsMercuryCashingUsed = true;
            IsMSMSProcessed = deconParameters.ScanBasedWorkflowParameters.ProcessMS2;
            IsMZRangeUsed = deconParameters.MSGeneratorParameters.UseMZRange;
            IsO16O18Data = deconParameters.ThrashParameters.IsO16O18Data;
            IsotopicProfileFitType = deconParameters.ThrashParameters.IsotopicProfileFitType;
            IsThrashed = deconParameters.ThrashParameters.IsThrashUsed;
            LeftFitStringencyFactor = deconParameters.ThrashParameters.LeftFitStringencyFactor;
            MaxChargeAllowed = deconParameters.ThrashParameters.MaxCharge;
            MaxFitAllowed = deconParameters.ThrashParameters.MaxFit;
            MaxMWAllowed = deconParameters.ThrashParameters.MaxMass;
            MaxMZ = deconParameters.MSGeneratorParameters.MaxMZ;      //TODO: review this later
            MinMZ = deconParameters.MSGeneratorParameters.MinMZ;      //TODO: review this later
            MinIntensityForScore = deconParameters.ThrashParameters.MinIntensityForScore;
            MinPeptideBackgroundRatio = deconParameters.ThrashParameters.MinMSFeatureToBackgroundRatio;
            NumAllowedShoulderPeaks = deconParameters.ThrashParameters.NumPeaksForShoulder;
            RightFitStringencyFactor = deconParameters.ThrashParameters.RightFitStringencyFactor;
            TagFormula = deconParameters.ThrashParameters.TagFormula;
            NumPeaksUsedInAbundance = deconParameters.ThrashParameters.NumPeaksUsedInAbundance;
        }

        private void convertDeconEngineHornParameters(DeconToolsV2.HornTransform.clsHornTransformParameters hornParameters)
        {
            AbsoluteThresholdPeptideIntensity = hornParameters.AbsolutePeptideIntensity;
            AveragineFormula = hornParameters.AveragineFormula;
            ChargeCarrierMass = hornParameters.CCMass;
            CheckPatternsAgainstChargeOne = hornParameters.CheckAllPatternsAgainstCharge1;
            DeleteIntensityThreshold = hornParameters.DeleteIntensityThreshold;
            IsAbsolutePepIntensityUsed = hornParameters.UseAbsolutePeptideIntensity;
            IsActualMonoMZUsed = hornParameters.IsActualMonoMZUsed;
            IsCompleteFit = hornParameters.CompleteFit;
            IsMercuryCashingUsed = hornParameters.UseMercuryCaching;
            IsMSMSProcessed = hornParameters.ProcessMSMS;
            IsMZRangeUsed = hornParameters.UseMZRange;
            IsO16O18Data = hornParameters.O16O18Media;
            IsotopicProfileFitType = convertDeconEngineIsoFitType(hornParameters.IsotopeFitType);
            IsThrashed = hornParameters.ThrashOrNot;
            LeftFitStringencyFactor = hornParameters.LeftFitStringencyFactor;
            MaxChargeAllowed = hornParameters.MaxCharge;
            MaxFitAllowed = hornParameters.MaxFit;
            MaxMWAllowed = hornParameters.MaxMW;
            MaxMZ = hornParameters.MaxMZ;      //TODO: review this later
            MinMZ = hornParameters.MinMZ;      //TODO: review this later
            MinIntensityForScore = hornParameters.MinIntensityForScore;
            MinPeptideBackgroundRatio = hornParameters.PeptideMinBackgroundRatio;
            NumAllowedShoulderPeaks = hornParameters.NumPeaksForShoulder;
            RightFitStringencyFactor = hornParameters.RightFitStringencyFactor;
            TagFormula = hornParameters.TagFormula;
            NumPeaksUsedInAbundance = hornParameters.NumPeaksUsedInAbundance;

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
        public override void Deconvolute(ResultCollection resultList)
        {

            var backgroundIntensity = (float)resultList.Run.CurrentBackgroundIntensity;
            var minPeptideIntensity = (float)(resultList.Run.CurrentBackgroundIntensity * MinPeptideBackgroundRatio);

			if (resultList.Run.XYData == null) return;

            var xvals = new float[1];
            var yvals = new float[1];
            resultList.Run.XYData.GetXYValuesAsSingles(ref xvals, ref yvals);

            mSpeakList = resultList.Run.DeconToolsPeakList;
            mTransformResults = new DeconToolsV2.HornTransform.clsHornTransformResults[0];
            
            if (resultList.Run.PeakList==null||resultList.Run.PeakList.Count==0) return;
            
            mSpeakList=new clsPeak[resultList.Run.PeakList.Count];
            
            for (var index = 0; index < resultList.Run.PeakList.Count; index++)
            {
                if (ShowTraceMessages)
                    Console.Write(index + " ");

                var peak = (MSPeak) resultList.Run.PeakList[index];
                var oldPeak = new clsPeak
                {
                    mdbl_FWHM = peak.Width,
                    mdbl_SN = peak.SignalToNoise,
                    mdbl_intensity = peak.Height,
                    mint_data_index = peak.DataIndex,
                    mdbl_mz = peak.XValue
                };

                mSpeakList[index] = oldPeak;
            }

            if (ShowTraceMessages)
                Console.WriteLine();

            this.Transformer.PerformTransform(backgroundIntensity, minPeptideIntensity, ref xvals, ref yvals, ref mSpeakList, ref mTransformResults);
            GenerateResults(mTransformResults, mSpeakList, resultList);




            //addDataToScanResults(transformResults.Length, resultList.GetCurrentScanResult());


        }

        private DeconToolsV2.HornTransform.clsHornTransformParameters loadDeconEngineHornParameters()
        {
            var hornParameters = new DeconToolsV2.HornTransform.clsHornTransformParameters
            {
                AbsolutePeptideIntensity = AbsoluteThresholdPeptideIntensity,
                AveragineFormula = AveragineFormula,
                CCMass = ChargeCarrierMass,
                CheckAllPatternsAgainstCharge1 = CheckPatternsAgainstChargeOne,
                CompleteFit = IsCompleteFit,
                DeleteIntensityThreshold = DeleteIntensityThreshold,
                IsActualMonoMZUsed = IsActualMonoMZUsed,
                IsotopeFitType = ConvertFitTypeToDeconEngineType(IsotopicProfileFitType),
                LeftFitStringencyFactor = LeftFitStringencyFactor,
                MaxCharge = (short)MaxChargeAllowed,
                MaxFit = MaxFitAllowed,
                MaxMW = MaxMWAllowed,
                MinMZ = MinMZ,
                MaxMZ = MaxMZ,
                MinIntensityForScore = MinIntensityForScore,
                NumPeaksForShoulder = (short)NumAllowedShoulderPeaks,
                O16O18Media = IsO16O18Data,
                PeptideMinBackgroundRatio = MinPeptideBackgroundRatio,
                ProcessMSMS = IsMSMSProcessed,
                RightFitStringencyFactor = RightFitStringencyFactor,
                TagFormula = TagFormula,
                ThrashOrNot = IsThrashed,
                UseAbsolutePeptideIntensity = IsAbsolutePepIntensityUsed,
                UseMercuryCaching = IsMercuryCashingUsed,
                UseMZRange = IsMZRangeUsed,
                NumPeaksUsedInAbundance = (short)NumPeaksUsedInAbundance
            };
            // hornParameters.MinS2N = ??     //TODO:  verify that this is no longer used

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

        private DeconToolsV2.enmIsotopeFitType ConvertFitTypeToDeconEngineType(Globals.IsotopicProfileFitType isoProfileFitType)
        {
            switch (isoProfileFitType)
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

        private void GenerateResults(IEnumerable<clsHornTransformResults> transformResults, clsPeak[] mspeakList, ResultCollection resultList)
        {

            ScanSet currentScanset;
            var currentRun = resultList.Run as UIMFRun;
            if (currentRun != null)
            {
                currentScanset = currentRun.CurrentIMSScanSet;
            }
            else
            {
                currentScanset = resultList.Run.CurrentScanSet;
            }

            currentScanset.NumIsotopicProfiles = 0;   //reset to 0;


            foreach (var hornResult in transformResults)
            {
                var result = resultList.CreateIsosResult();
                var profile = new IsotopicProfile
                {
                    AverageMass = hornResult.mdbl_average_mw,
                    ChargeState = hornResult.mshort_cs,
                    MonoIsotopicMass = hornResult.mdbl_mono_mw,
                    Score = hornResult.mdbl_fit,
                    ScoreCountBasis = hornResult.mint_fit_count_basis,
                    MostAbundantIsotopeMass = hornResult.mdbl_most_intense_mw
                };


                GetIsotopicProfile(hornResult.marr_isotope_peak_indices, mspeakList, ref profile);

                profile.IntensityMostAbundant = (float)hornResult.mdbl_abundance;
                profile.IntensityMostAbundantTheor = (float)hornResult.mdbl_abundance;

                if (NumPeaksUsedInAbundance == 1)  // fyi... this is typical
                {
                    result.IntensityAggregate = profile.IntensityMostAbundant;
                }
                else
                {
                    result.IntensityAggregate = sumPeaks(profile, hornResult.mdbl_abundance);
                }

                profile.MonoPlusTwoAbundance = profile.GetMonoPlusTwoAbundance();
                profile.MonoPeakMZ = profile.GetMZ();

                result.IsotopicProfile = profile;

                this.AddDeconResult(resultList, result, DeconResultComboMode.simplyAddIt);
                //resultList.ResultList.Add(result);
                currentScanset.NumIsotopicProfiles++;
            }
        }

        private double sumPeaks(IsotopicProfile profile, double defaultVal)
        {
            if (profile.Peaklist == null || profile.Peaklist.Count == 0) 
                return defaultVal;

            var peakListIntensities = new List<float>();
            foreach (var peak in profile.Peaklist)
            {
                peakListIntensities.Add(peak.Height);

            }
            peakListIntensities.Sort();
            peakListIntensities.Reverse();    // i know... this isn't the best way to do this!
            double summedIntensities = 0;

            for (var i = 0; i < peakListIntensities.Count; i++)
            {
                if (i < NumPeaksUsedInAbundance)
                {
                    summedIntensities += peakListIntensities[i];
                }
            }

            return summedIntensities;

        }

        private void GetIsotopicProfile(int[] peakIndexList, DeconToolsV2.Peaks.clsPeak[] peakdata, ref IsotopicProfile profile)
        {
            if (peakIndexList == null || peakIndexList.Length == 0) return;
            var deconMonopeak = lookupPeak(peakIndexList[0], peakdata);

            var monoPeak = convertDeconPeakToMSPeak(deconMonopeak);
            profile.Peaklist.Add(monoPeak);

            if (peakIndexList.Length == 1) return;           //only one peak in the DeconEngine's profile    

            for (var i = 1; i < peakIndexList.Length; i++)     //start with second peak and add each peak to profile
            {
                var deconPeak = lookupPeak(peakIndexList[i], peakdata);
                var peakToBeAdded = convertDeconPeakToMSPeak(deconPeak);
                profile.Peaklist.Add(peakToBeAdded);
            }




        }

        private DeconToolsV2.Peaks.clsPeak lookupPeak(int index, DeconToolsV2.Peaks.clsPeak[] peakdata)
        {
            return peakdata[index];
        }

        private MSPeak convertDeconPeakToMSPeak(DeconToolsV2.Peaks.clsPeak deconPeak)
        {

            var peak = new MSPeak
            {
                XValue = deconPeak.mdbl_mz,
                Width = (float)deconPeak.mdbl_FWHM,
                SignalToNoise = (float)deconPeak.mdbl_SN,
                Height = (float)deconPeak.mdbl_intensity
            };

            return peak;
        }

        #endregion
    }

#endif
}
