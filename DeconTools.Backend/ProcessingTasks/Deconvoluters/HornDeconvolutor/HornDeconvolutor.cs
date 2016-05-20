using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.ChargeDetermination;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.ElementalFormulas;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.FitScoring;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.PeakProcessing;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.ProcessingTasks
{
    public class HornDeconvolutor : Deconvolutor
    {
        #region Member Variables

        private const int MaxIsotopes = 16;
        private ThrashV1Peak[] mSpeakList;
        private HornTransformResults[] mTransformResults;

        //gord added
        private IsotopicProfileFitScorer _isotopeFitScorer = new AreaFitScorer();

        private Globals.IsotopicProfileFitType _isotopicProfileFitType = Globals.IsotopicProfileFitType.AREA;

        private bool _isMercuryCachingUsed;
        private string _averagineFormula;
        private string _tagFormula;
        private double _chargeCarrierMass;
        private bool _isThrashed;
        private bool _isCompleteFit;
        private bool _needToUpdateIsotopeFitScorerOptions;

        #endregion

        #region Properties

        public int PercentDone { get; private set; }
        public string StatusMessage { get; private set; }

        public ElementIsotopes ElementalIsotopeComposition
        {
            get { return _isotopeFitScorer.ElementalIsotopeComposition; }
            set { _isotopeFitScorer.ElementalIsotopeComposition = value; }
        }

        /// <summary>
        ///     minimum signal to noise for a peak to consider it for deisotoping.
        /// </summary>
        private double MinSignalToNoise { get; set; }

        public bool IsAbsolutePepIntensityUsed { get; set; }

        public bool IsMercuryCachingUsed
        {
            get { return _isMercuryCachingUsed; }
            set
            {
                if (value != _isMercuryCachingUsed)
                {
                    _isMercuryCachingUsed = value;
                    _needToUpdateIsotopeFitScorerOptions = true;
                }
            }
        }

        public bool IsThrashed
        {
            get { return _isThrashed; }
            set
            {
                if (value != _isThrashed)
                {
                    _isThrashed = value;
                    _needToUpdateIsotopeFitScorerOptions = true;
                }
            }
        }

        /// <summary>
        ///     Is the medium a mixture of O16 and O18 labelled peptides.
        /// </summary>
        public bool IsO16O18Data { get; set; }

        public bool IsActualMonoMZUsed { get; set; }

        /// <summary>
        ///     Check feature against charge 1.
        /// </summary>
        public bool CheckPatternsAgainstChargeOne { get; set; }

        public bool IsMZRangeUsed { get; set; }

        public bool IsCompleteFit
        {
            get { return _isCompleteFit; }
            set
            {
                if (value != _isCompleteFit)
                {
                    _isCompleteFit = value;
                    _needToUpdateIsotopeFitScorerOptions = true;
                }
            }
        }

        public bool IsMSMSProcessed { get; set; }

        public double MinMZ { get; set; }

        public double MaxMZ { get; set; }

        public double LeftFitStringencyFactor { get; set; }

        public double RightFitStringencyFactor { get; set; }

        /// <summary>
        ///     mass of charge carrier
        /// </summary>
        public double ChargeCarrierMass
        {
            get { return _chargeCarrierMass; }
            set
            {
                if (!value.Equals(_chargeCarrierMass))
                {
                    _chargeCarrierMass = value;
                    _needToUpdateIsotopeFitScorerOptions = true;
                }
            }
        }

        /// <summary>
        ///     After deisotoping is done, we delete the isotopic profile.  This threshold sets the value of the minimum
        ///     intensity of a peak to delete. Note that ths intensity is in the theoretical profile which is scaled to
        ///     where the maximum peak has an intensity of 100.
        /// </summary>
        /// <seealso cref="IsotopicProfileFitScorer.GetZeroingMassRange" />
        public double DeleteIntensityThreshold { get; set; }

        /// <summary>
        ///     minimum intensity of a point in the theoretical profile of a peptide for it to be considered in scoring.
        /// </summary>
        /// <seealso cref="IsotopicProfileFitScorer.GetIsotopeDistribution" />
        public double MinIntensityForScore { get; set; }

        public double MinPeptideBackgroundRatio { get; set; }

        public double AbsoluteThresholdPeptideIntensity { get; set; }

        /// <summary>
        ///     maximium MW for deisotoping
        /// </summary>
        public double MaxMWAllowed { get; set; }

        /// <summary>
        ///     maximum charge to check while deisotoping
        /// </summary>
        public int MaxChargeAllowed { get; set; }

        /// <summary>
        ///     maximum fit value to report a deisotoped peak
        /// </summary>
        public double MaxFitAllowed { get; set; }

        public string AveragineFormula
        {
            get { return _averagineFormula; }
            set
            {
                if (value != _averagineFormula)
                {
                    _averagineFormula = value;
                    _needToUpdateIsotopeFitScorerOptions = true;
                }
            }
        }

        public string TagFormula
        {
            get { return _tagFormula; }
            set
            {
                if (value != _tagFormula)
                {
                    _tagFormula = value;
                    _needToUpdateIsotopeFitScorerOptions = true;
                }
            }
        }

        public Globals.IsotopicProfileFitType IsotopicProfileFitType
        {
            get { return _isotopicProfileFitType; }
            set
            {
                if (value != _isotopicProfileFitType)
                {
                    _isotopicProfileFitType = value;
                    _isotopeFitScorer = IsotopicProfileFitScorer.ScorerFactory(_isotopicProfileFitType,
                        _isotopeFitScorer);
                }
            }
        }

        /// <summary>
        ///     Number of peaks from the monoisotope before the shoulder
        /// </summary>
        /// <remarks>
        ///     After deisotoping is performed, we delete points corresponding to the isotopic profile, To do so, we move
        ///     to the left and right of each isotope peak and delete points till the shoulder of the peak is reached. To
        ///     decide if the current point is a shoulder, we check if the next (_numPeaksForShoulder) # of
        ///     points are of continuously increasing intensity.
        /// </remarks>
        /// <seealso cref="SetPeakToZero" />
        public int NumAllowedShoulderPeaks { get; set; }

        public int NumPeaksUsedInAbundance { get; set; }

        #endregion

        #region Constructors

        public HornDeconvolutor()
        {
            SetDefaults();
        }

        private void SetDefaults()
        {
            // ---- Matching defaults from HornTransform and HornTransformParameters ----------
            IsotopicProfileFitType = Globals.IsotopicProfileFitType.AREA;
            MaxChargeAllowed = 10;
            MaxMWAllowed = 10000;
            IsO16O18Data = false;
            //Charge carrier mass = [atomic mass of hydrogen (1.007825) - atomic mass of an electron (0.00054858)]
            ChargeCarrierMass = 1.00727638; // DeconTools default - Globals.PROTON_MASS (1.00727649)
            NumAllowedShoulderPeaks = 1;
            CheckPatternsAgainstChargeOne = false;
            IsActualMonoMZUsed = false;
            LeftFitStringencyFactor = 1;
            RightFitStringencyFactor = 1;
            // ------------------ HornTransform defaults ---------------------------------
            //MaxFitAllowed = 0.15;
            //MinSignalToNoise = 5;
            //DeleteIntensityThreshold = 1;
            //MinIntensityForScore = 1;
            // ------------------ end HornTransform defaults ---------------------------------
            // ------------------ HornTransformParameters defaults ---------------------------------
            MaxFitAllowed = 0.25;
            MinSignalToNoise = 3;
            DeleteIntensityThreshold = 10;
            MinIntensityForScore = 10;

            IsAbsolutePepIntensityUsed = false;
            AbsoluteThresholdPeptideIntensity = 0;
            AveragineFormula = "C4.9384 H7.7583 N1.3577 O1.4773 S0.0417";
            TagFormula = "";
            IsThrashed = true;
            IsCompleteFit = false;
            IsMercuryCachingUsed = true;
            IsMSMSProcessed = false;
            IsMZRangeUsed = true;
            MaxMZ = 400;
            MinMZ = 2000;
            MinPeptideBackgroundRatio = 5;
            NumPeaksUsedInAbundance = 1;

            _needToUpdateIsotopeFitScorerOptions = true;
            SetIsotopeFitScorerOptions();

            ElementalIsotopeComposition = new ElementIsotopes();
            // ------------------ end HornTransformParameters defaults ---------------------------------
        }
        
#if !Disable_DeconToolsV2
        [Obsolete(
            "Not needed - this workflow no longer uses DeconEngineV2; use DeconToolsParameters or properties instead",
            false)]
        public HornDeconvolutor(DeconToolsV2.HornTransform.clsHornTransformParameters hornParameters)
        {
            SetDefaults();
            AbsoluteThresholdPeptideIntensity = hornParameters.AbsolutePeptideIntensity;
            AveragineFormula = hornParameters.AveragineFormula;
            ChargeCarrierMass = hornParameters.CCMass;
            CheckPatternsAgainstChargeOne = hornParameters.CheckAllPatternsAgainstCharge1;
            DeleteIntensityThreshold = hornParameters.DeleteIntensityThreshold;
            IsAbsolutePepIntensityUsed = hornParameters.UseAbsolutePeptideIntensity;
            IsActualMonoMZUsed = hornParameters.IsActualMonoMZUsed;
            IsCompleteFit = hornParameters.CompleteFit;
            IsMercuryCachingUsed = hornParameters.UseMercuryCaching;
            IsMSMSProcessed = hornParameters.ProcessMSMS;
            IsMZRangeUsed = hornParameters.UseMZRange;
            IsO16O18Data = hornParameters.O16O18Media;
            switch (hornParameters.IsotopeFitType)
            {
                    case DeconToolsV2.enmIsotopeFitType.AREA:
                    IsotopicProfileFitType = Globals.IsotopicProfileFitType.AREA;
                    break;
                    case DeconToolsV2.enmIsotopeFitType.CHISQ:
                    IsotopicProfileFitType = Globals.IsotopicProfileFitType.CHISQ;
                    break;
                    case DeconToolsV2.enmIsotopeFitType.PEAK:
                    IsotopicProfileFitType = Globals.IsotopicProfileFitType.PEAK;
                    break;
                default:
                    IsotopicProfileFitType = Globals.IsotopicProfileFitType.Undefined;
                    break;
            }
            IsThrashed = hornParameters.ThrashOrNot;
            LeftFitStringencyFactor = hornParameters.LeftFitStringencyFactor;
            MaxChargeAllowed = hornParameters.MaxCharge;
            MaxFitAllowed = hornParameters.MaxFit;
            MaxMWAllowed = hornParameters.MaxMW;
            MaxMZ = hornParameters.MaxMZ; //TODO: review this later
            MinMZ = hornParameters.MinMZ; //TODO: review this later
            MinIntensityForScore = hornParameters.MinIntensityForScore;
            MinPeptideBackgroundRatio = hornParameters.PeptideMinBackgroundRatio;
            NumAllowedShoulderPeaks = hornParameters.NumPeaksForShoulder;
            RightFitStringencyFactor = hornParameters.RightFitStringencyFactor;
            TagFormula = hornParameters.TagFormula;
            NumPeaksUsedInAbundance = hornParameters.NumPeaksUsedInAbundance;
            SetIsotopeFitScorerOptions();
        }
#endif

        public HornDeconvolutor(DeconToolsParameters deconParameters)
        {
            SetDefaults();
            AbsoluteThresholdPeptideIntensity = deconParameters.ThrashParameters.AbsolutePeptideIntensity;
            AveragineFormula = deconParameters.ThrashParameters.AveragineFormula;
            ChargeCarrierMass = deconParameters.ThrashParameters.ChargeCarrierMass;
            CheckPatternsAgainstChargeOne = deconParameters.ThrashParameters.CheckAllPatternsAgainstChargeState1;
            DeleteIntensityThreshold = deconParameters.ThrashParameters.MinIntensityForDeletion;
            IsAbsolutePepIntensityUsed = deconParameters.ThrashParameters.UseAbsoluteIntensity;
            IsActualMonoMZUsed = false;
            IsCompleteFit = deconParameters.ThrashParameters.CompleteFit;
            IsMercuryCachingUsed = true;
            IsMSMSProcessed = deconParameters.ScanBasedWorkflowParameters.ProcessMS2;
            IsMZRangeUsed = deconParameters.MSGeneratorParameters.UseMZRange;
            IsO16O18Data = deconParameters.ThrashParameters.IsO16O18Data;
            IsotopicProfileFitType = deconParameters.ThrashParameters.IsotopicProfileFitType;
            IsThrashed = deconParameters.ThrashParameters.IsThrashUsed;
            LeftFitStringencyFactor = deconParameters.ThrashParameters.LeftFitStringencyFactor;
            MaxChargeAllowed = deconParameters.ThrashParameters.MaxCharge;
            MaxFitAllowed = deconParameters.ThrashParameters.MaxFit;
            MaxMWAllowed = deconParameters.ThrashParameters.MaxMass;
            MaxMZ = deconParameters.MSGeneratorParameters.MaxMZ; //TODO: review this later
            MinMZ = deconParameters.MSGeneratorParameters.MinMZ; //TODO: review this later
            MinIntensityForScore = deconParameters.ThrashParameters.MinIntensityForScore;
            MinPeptideBackgroundRatio = deconParameters.ThrashParameters.MinMSFeatureToBackgroundRatio;
            NumAllowedShoulderPeaks = deconParameters.ThrashParameters.NumPeaksForShoulder;
            RightFitStringencyFactor = deconParameters.ThrashParameters.RightFitStringencyFactor;
            TagFormula = deconParameters.ThrashParameters.TagFormula;
            NumPeaksUsedInAbundance = deconParameters.ThrashParameters.NumPeaksUsedInAbundance;

            SetIsotopeFitScorerOptions();
        }

        #endregion

        #region Public Methods

        public override void Deconvolute(ResultCollection resultList)
        {
            var backgroundIntensity = (float) resultList.Run.CurrentBackgroundIntensity;
            var minPeptideIntensity = (float) (resultList.Run.CurrentBackgroundIntensity * MinPeptideBackgroundRatio);

            if (resultList.Run.XYData == null) return;

            var xvals = new float[1];
            var yvals = new float[1];
            resultList.Run.XYData.GetXYValuesAsSingles(ref xvals, ref yvals);

            mSpeakList = resultList.Run.DeconToolsPeakList;
            mTransformResults = new HornTransformResults[0];

            if (resultList.Run.PeakList == null || resultList.Run.PeakList.Count == 0) return;

            mSpeakList = resultList.Run.PeakList.Select(x => new ThrashV1Peak(x as MSPeak)).ToArray();
            //mSpeakList = new ThrashV1Peak[resultList.Run.PeakList.Count];
            //
            //for (var index = 0; index < resultList.Run.PeakList.Count; index++)
            //{
            //    if (ShowTraceMessages)
            //        Console.Write(index + " ");
            //
            //    var peak = (MSPeak) resultList.Run.PeakList[index];
            //    var oldPeak = new ThrashV1Peak
            //    {
            //        FWHM = peak.Width,
            //        SignalToNoiseDbl = peak.SignalToNoise,
            //        Intensity = peak.Height,
            //        DataIndex = peak.DataIndex,
            //        Mz = peak.XValue
            //    };
            //
            //    mSpeakList[index] = oldPeak;
            //}

            if (ShowTraceMessages)
                Console.WriteLine();

            PerformTransform(backgroundIntensity, minPeptideIntensity, ref xvals, ref yvals, ref mSpeakList,
                ref mTransformResults);
            GenerateResults(mTransformResults, mSpeakList, resultList);
            //addDataToScanResults(transformResults.Length, resultList.GetCurrentScanResult());
        }

        public void PerformTransform(float backgroundIntensity, float minPeptideIntensity, ref float[] mzs,
            ref float[] intensities, ref ThrashV1Peak[] peaks, ref HornTransformResults[] transformResults)
        {
            PercentDone = 0;
            var numPoints = mzs.Length;

            if (mzs.Length == 0)
                return;

            // mzs should be in sorted order
            double minMz = mzs[0];
            double maxMz = mzs[numPoints - 1];
            var mzList = new List<double>(mzs.Select(x => (double) x));
            var intensityList = new List<double>(intensities.Select(x => (double) x));

            var peakData = new PeakData();
            peakData.SetPeaks(peaks);
            peakData.MzList = mzList;
            peakData.IntensityList = intensityList;

            if (IsMZRangeUsed)
            {
                minMz = MinMZ;
                maxMz = MaxMZ;
            }

            //loads 'currentPeak' with the most intense peak within minMZ and maxMZ
            ThrashV1Peak currentPeak;
            var found = peakData.GetNextPeak(minMz, maxMz, out currentPeak);
            //var fwhm_SN = currentPeak.FWHM;

            var transformRecords = new List<HornTransformResults>();
            var numTotalPeaks = peakData.GetNumPeaks();
            StatusMessage = "Performing Horn Transform on peaks";
            while (found)
            {
                var numPeaksLeft = peakData.GetNumUnprocessedPeaks();
                PercentDone = 100 * (numTotalPeaks - numPeaksLeft) / numTotalPeaks;
                if (PercentDone % 5 == 0)
                {
                    StatusMessage = string.Concat("Done with ", Convert.ToString(numTotalPeaks - numPeaksLeft), " of ",
                        Convert.ToString(numTotalPeaks), " peaks.");
                }
                if (currentPeak.Intensity < minPeptideIntensity)
                    break;

                //--------------------- Transform performed ------------------------------
                HornTransformResults transformRecord;
                var foundTransform = FindTransform(peakData, ref currentPeak, out transformRecord, backgroundIntensity);
                if (foundTransform && transformRecord.ChargeState <= MaxChargeAllowed)
                {
                    if (IsActualMonoMZUsed)
                    {
                        //retrieve experimental monoisotopic peak
                        var monoPeakIndex = transformRecord.IsotopePeakIndices[0];
                        ThrashV1Peak monoPeak;
                        peakData.GetPeak(monoPeakIndex, out monoPeak);

                        //set threshold at 20% less than the expected 'distance' to the next peak
                        var errorThreshold = 1.003 / transformRecord.ChargeState;
                        errorThreshold = errorThreshold - errorThreshold * 0.2;

                        var calcMonoMz = transformRecord.MonoMw / transformRecord.ChargeState + 1.00727638;

                        if (Math.Abs(calcMonoMz - monoPeak.Mz) < errorThreshold)
                        {
                            transformRecord.MonoMw = monoPeak.Mz * transformRecord.ChargeState -
                                                     1.00727638 * transformRecord.ChargeState;
                        }
                    }
                    transformRecords.Add(transformRecord);
                }
                found = peakData.GetNextPeak(minMz, maxMz, out currentPeak);
            }
            PercentDone = 100;

            // Done with the transform. Lets copy them all to the given memory structure.
            //Console.WriteLine("Done with Mass Transform. Found " + transformRecords.Count + " features");

            transformResults = transformRecords.ToArray();
            PercentDone = 100;
        }

        #endregion

        #region Protected Methods

        protected virtual bool FindTransform(PeakData peakData, ref ThrashV1Peak peak, out HornTransformResults record,
            double backgroundIntensity = 0)
        {
            SetIsotopeFitScorerOptions();
            record = new HornTransformResults();
            if (peak.SignalToNoiseDbl < MinSignalToNoise || peak.FWHM.Equals(0))
            {
                return false;
            }

            //var resolution = peak.Mz / peak.FWHM;
            /**/
            var chargeState = AutoCorrelationChargeDetermination.GetChargeState(peak, peakData, ShowTraceMessages);
            /*/
            // This chunk of code tries to use the DeconTools Patterson Algorithm Charge State Calculator, but it gets vastly different results.
            var xyData = new XYData();
            var mzArray = peakData.MzList.ToArray();
            var intensityArray = peakData.IntensityList.ToArray();
            xyData.SetXYValues(ref mzArray, ref intensityArray);
            var peakList = new List<Peak>();
            for (int i = 0; i < peakData.MzList.Count; i++)
            {
                peakList.Add(new Peak(peakData.MzList[i], (float)peakData.IntensityList[i], 1));
            }
            var chargeState = DeconTools.Backend.Algorithms.ChargeStateDetermination.PattersonAlgorithm.PattersonChargeStateCalculator.GetChargeState(xyData, peakList, convertDeconPeakToMSPeak(peak));
            /**/

            if (chargeState == -1 && CheckPatternsAgainstChargeOne)
            {
                chargeState = 1;
            }

            if (ShowTraceMessages)
            {
                Console.Error.WriteLine("Deisotoping :" + peak.Mz);
                Console.Error.WriteLine("Charge = " + chargeState);
            }

            if (chargeState == -1)
            {
                return false;
            }

            if ((peak.Mz + ChargeCarrierMass) * chargeState > MaxMWAllowed)
            {
                return false;
            }

            if (IsO16O18Data)
            {
                if (peak.FWHM < 1.0 / chargeState)
                {
                    // move back by 4 Da and see if there is a peak.
                    var minMz = peak.Mz - 4.0 / chargeState - peak.FWHM;
                    var maxMz = peak.Mz - 4.0 / chargeState + peak.FWHM;
                    ThrashV1Peak o16Peak;
                    var found = peakData.GetPeak(minMz, maxMz, out o16Peak);
                    if (found && !o16Peak.Mz.Equals(peak.Mz))
                    {
                        // put back the current into the to be processed list of peaks.
                        peakData.AddPeakToProcessingList(peak);
                        // reset peak to the right peak so that the calling function may
                        // know that the peak might have changed in the O16/O18 business
                        peak = o16Peak;
                        peakData.RemovePeak(peak);
                        return FindTransform(peakData, ref peak, out record, backgroundIntensity);
                    }
                }
            }

            var peakCharge1 = new ThrashV1Peak(peak);

            // Until now, we have been using constant theoretical delete intensity threshold..
            // instead, from now, we should use one that is proportional to intensity, for more intense peaks.
            // However this will not solve all problems. If thrashing occurs, then the peak intensity will
            // change when the function returns and we may not delete far enough.
            //double deleteThreshold = backgroundIntensity / peak.Intensity * 100;
            //if (backgroundIntensity ==0 || deleteThreshold > _deleteIntensityThreshold)
            //  deleteThreshold = _deleteIntensityThreshold;
            var deleteThreshold = DeleteIntensityThreshold;
            int fitCountBasis;
            var bestFit = _isotopeFitScorer.GetFitScore(peakData, chargeState, ref peak, out record, deleteThreshold,
                MinIntensityForScore, LeftFitStringencyFactor, RightFitStringencyFactor, out fitCountBasis,
                ShowTraceMessages);

            // When deleting an isotopic profile, this value is set to the first m/z to perform deletion at.
            double zeroingStartMz;
            // When deleting an isotopic profile, this value is set to the last m/z to perform deletion at.
            double zeroingStopMz;
            _isotopeFitScorer.GetZeroingMassRange(out zeroingStartMz, out zeroingStopMz, record.DeltaMz, deleteThreshold,
                ShowTraceMessages);
            //bestFit = _isotopeFitter.GetFitScore(peakData, chargeState, peak, record, _deleteIntensityThreshold, _minTheoreticalIntensityForScore, DebugFlag);
            //_isotopeFitter.GetZeroingMassRange(_zeroingStartMz, _zeroingStopMz, record.DeltaMz, _deleteIntensityThreshold, DebugFlag);

            if (CheckPatternsAgainstChargeOne && chargeState != 1)
            {
                HornTransformResults recordCharge1;
                int fitCountBasisCharge1;
                var bestFitCharge1 = _isotopeFitScorer.GetFitScore(peakData, 1, ref peakCharge1, out recordCharge1,
                    deleteThreshold, MinIntensityForScore, LeftFitStringencyFactor,
                    RightFitStringencyFactor, out fitCountBasisCharge1, ShowTraceMessages);

                //double bestFitCharge1 = _isotopeFitter.GetFitScore(peakData, 1, peakCharge1, recordCharge1, _deleteIntensityThreshold, _minTheoreticalIntensityForScore, DebugFlag);
                //_isotopeFitter.GetZeroingMassRange(_zeroingStartMz, _zeroingStopMz, record.DeltaMz, _deleteIntensityThreshold, DebugFlag);
                double startMz1 = 0;
                double stopMz1 = 0;
                _isotopeFitScorer.GetZeroingMassRange(out startMz1, out stopMz1, record.DeltaMz, deleteThreshold,
                    ShowTraceMessages);
                if (bestFit > MaxFitAllowed && bestFitCharge1 < MaxFitAllowed)
                {
                    bestFit = bestFitCharge1;
                    fitCountBasis = fitCountBasisCharge1;
                    peak = peakCharge1;
                    record = new HornTransformResults(recordCharge1);
                    zeroingStartMz = startMz1;
                    zeroingStopMz = stopMz1;
                    chargeState = 1;
                }
            }

            if (bestFit > MaxFitAllowed) // check if fit is good enough
                return false;

            if (ShowTraceMessages)
                Console.Error.WriteLine("\tBack with fit = " + record.Fit);

            // Applications using this DLL should use Abundance instead of AbundanceInt
            record.Abundance = peak.Intensity;
            record.ChargeState = chargeState;

            ThrashV1Peak monoPeak;
            var monoMz = record.MonoMw / record.ChargeState + ChargeCarrierMass;

            // used when _reportO18Plus2Da is true.
            ThrashV1Peak m3Peak;
            var monoPlus2Mz = record.MonoMw / record.ChargeState + 2.0 / record.ChargeState + ChargeCarrierMass;

            peakData.FindPeak(monoMz - peak.FWHM, monoMz + peak.FWHM, out monoPeak);
            peakData.FindPeak(monoPlus2Mz - peak.FWHM, monoPlus2Mz + peak.FWHM, out m3Peak);

            record.MonoIntensity = (int) monoPeak.Intensity;
            record.MonoPlus2Intensity = (int) m3Peak.Intensity;
            record.SignalToNoise = peak.SignalToNoiseDbl;
            record.FWHM = peak.FWHM;
            record.PeakIndex = peak.PeakIndex;

            SetIsotopeDistributionToZero(peakData, peak, zeroingStartMz, zeroingStopMz, record.MonoMw, chargeState, true,
                record, ShowTraceMessages);
            if (ShowTraceMessages)
            {
                Console.Error.WriteLine("Performed deisotoping of " + peak.Mz);
            }
            return true;
        }

        #endregion

        #region Private Methods

        private void SetIsotopeFitScorerOptions()
        {
            if (!_needToUpdateIsotopeFitScorerOptions)
            {
                return;
            }
            _isotopeFitScorer.UseIsotopeDistributionCaching = IsMercuryCachingUsed;
            _isotopeFitScorer.SetOptions(AveragineFormula, TagFormula, ChargeCarrierMass, IsThrashed, IsCompleteFit);
            _needToUpdateIsotopeFitScorerOptions = false;
        }

        private void SetIsotopeDistributionToZero(PeakData peakData, ThrashV1Peak peak, double zeroingStartMz,
            double zeroingStopMz, double monoMw, int chargeState, bool clearSpectrum, HornTransformResults record,
            bool debug = false)
        {
            var peakIndices = new List<int>();
            peakIndices.Add(peak.PeakIndex);
            var mzDelta = record.DeltaMz;

            if (debug)
            {
                Console.Error.WriteLine("Clearing peak data for " + peak.Mz + " Delta = " + mzDelta);
                Console.Error.WriteLine("Zeroing range = " + zeroingStartMz + " to " + zeroingStopMz);
            }

            double maxMz = 0;
            if (IsO16O18Data)
                maxMz = (monoMw + 3.5) / chargeState + ChargeCarrierMass;

            var numUnprocessedPeaks = peakData.GetNumUnprocessedPeaks();
            if (numUnprocessedPeaks == 0)
            {
                record.IsotopePeakIndices.Add(peak.PeakIndex);
                return;
            }

            if (clearSpectrum)
            {
                if (debug)
                    Console.Error.WriteLine("Deleting main peak :" + peak.Mz);
                SetPeakToZero(peak.DataIndex, ref peakData.IntensityList, ref peakData.MzList, debug);
            }

            peakData.RemovePeaks(peak.Mz - peak.FWHM, peak.Mz + peak.FWHM, debug);

            if (1 / (peak.FWHM * chargeState) < 3) // gord:  ??
            {
                record.IsotopePeakIndices.Add(peak.PeakIndex);
                peakData.RemovePeaks(zeroingStartMz, zeroingStopMz, debug);
                return;
            }

            // Delete isotopes of mzs higher than mz of starting isotope
            for (var peakMz = peak.Mz + 1.003 / chargeState;
                (!IsO16O18Data || peakMz <= maxMz) && peakMz <= zeroingStopMz + 2 * peak.FWHM;
                peakMz += 1.003 / chargeState)
            {
                if (debug)
                {
                    Console.Error.WriteLine("\tFinding next peak top from " + (peakMz - 2 * peak.FWHM) + " to " +
                                            (peakMz + 2 * peak.FWHM) + " pk = " + peakMz + " FWHM = " + peak.FWHM);
                }
                ThrashV1Peak nextPeak;
                peakData.GetPeakFromAll(peakMz - 2 * peak.FWHM, peakMz + 2 * peak.FWHM, out nextPeak);

                if (nextPeak.Mz.Equals(0))
                {
                    if (debug)
                        Console.Error.WriteLine("\t\tNo peak found.");
                    break;
                }
                if (debug)
                {
                    Console.Error.WriteLine("\t\tFound peak to delete =" + nextPeak.Mz);
                }

                // Before assuming that the next peak is indeed an isotope, we must check for the height of this
                // isotope. If the height is greater than expected by a factor of 3, lets not delete it.
                peakIndices.Add(nextPeak.PeakIndex);
                SetPeakToZero(nextPeak.DataIndex, ref peakData.IntensityList, ref peakData.MzList, debug);

                peakData.RemovePeaks(nextPeak.Mz - peak.FWHM, nextPeak.Mz + peak.FWHM, debug);
                peakMz = nextPeak.Mz;
            }

            // Delete isotopes of mzs lower than mz of starting isotope
            // TODO: Use the delta m/z to make sure to remove 1- peaks from the unprocessed list, but not from the list of peaks?
            for (var peakMz = peak.Mz - 1.003 / chargeState;
                peakMz > zeroingStartMz - 2 * peak.FWHM;
                peakMz -= 1.003 / chargeState)
            {
                if (debug)
                {
                    Console.Error.WriteLine("\tFinding previous peak top from " + (peakMz - 2 * peak.FWHM) + " to " +
                                            (peakMz + 2 * peak.FWHM) + " pk = " + peakMz + " FWHM = " + peak.FWHM);
                }
                ThrashV1Peak nextPeak;
                peakData.GetPeakFromAll(peakMz - 2 * peak.FWHM, peakMz + 2 * peak.FWHM, out nextPeak);
                if (nextPeak.Mz.Equals(0))
                {
                    if (debug)
                        Console.Error.WriteLine("\t\tNo peak found.");
                    break;
                }
                if (debug)
                {
                    Console.Error.WriteLine("\t\tFound peak to delete =" + nextPeak.Mz);
                }
                peakIndices.Add(nextPeak.PeakIndex);
                SetPeakToZero(nextPeak.DataIndex, ref peakData.IntensityList, ref peakData.MzList, debug);
                peakData.RemovePeaks(nextPeak.Mz - peak.FWHM, nextPeak.Mz + peak.FWHM, debug);
                peakMz = nextPeak.Mz;
            }

            if (debug)
            {
                Console.Error.WriteLine("Done Clearing peak data for " + peak.Mz);
            }

            peakIndices.Sort();
            // now insert into array.
            var numPeaksObserved = peakIndices.Count;
            var numIsotopesObserved = 0;
            var lastIsotopeNumObserved = int.MinValue;

            for (var i = 0; i < numPeaksObserved; i++)
            {
                var currentIndex = peakIndices[i];
                var currentPeak = new ThrashV1Peak(peakData.PeakTops[currentIndex]);
                var isotopeNum = (int) (Math.Abs((currentPeak.Mz - peak.Mz) * chargeState / 1.003) + 0.5);
                if (currentPeak.Mz < peak.Mz)
                    isotopeNum = -1 * isotopeNum;
                if (isotopeNum > lastIsotopeNumObserved)
                {
                    lastIsotopeNumObserved = isotopeNum;
                    numIsotopesObserved++;
                    if (numIsotopesObserved > MaxIsotopes)
                        break;
                    record.IsotopePeakIndices.Add(peakIndices[i]);
                }
                else
                {
                    record.IsotopePeakIndices[numIsotopesObserved - 1] = peakIndices[i];
                }
            }
            if (debug)
            {
                Console.Error.WriteLine("Copied " + record.NumIsotopesObserved + " isotope peak indices into record ");
            }
        }

        private void SetPeakToZero(int index, ref List<double> intensities, ref List<double> mzs, bool debug = false)
        {
            var lastIntensity = intensities[index];
            var count = -1;
            //double mz1, mz2;

            if (debug)
                Console.Error.WriteLine("\t\tNum Peaks for Shoulder =" + NumAllowedShoulderPeaks);

            for (var i = index - 1; i >= 0; i--)
            {
                var thisIntensity = intensities[i];
                if (thisIntensity <= lastIntensity)
                    count = 0;
                else
                {
                    count++;
                    //mz1 = mzs[i];
                    if (count >= NumAllowedShoulderPeaks)
                        break;
                }
                intensities[i] = 0;
                lastIntensity = thisIntensity;
            }
            count = 0;

            lastIntensity = intensities[index];
            for (var i = index; i < intensities.Count; i++)
            {
                var thisIntensity = intensities[i];
                if (thisIntensity <= lastIntensity)
                    count = 0;
                else
                {
                    count++;
                    //mz2 = mzs[i];
                    if (count >= NumAllowedShoulderPeaks)
                        break;
                }
                intensities[i] = 0;
                lastIntensity = thisIntensity;
            }
        }

        private void GenerateResults(IEnumerable<HornTransformResults> transformResults, ThrashV1Peak[] mspeakList,
            ResultCollection resultList)
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

            currentScanset.NumIsotopicProfiles = 0; //reset to 0;

            foreach (var hornResult in transformResults)
            {
                var result = resultList.CreateIsosResult();
                var profile = new IsotopicProfile
                {
                    AverageMass = hornResult.AverageMw,
                    ChargeState = hornResult.ChargeState,
                    MonoIsotopicMass = hornResult.MonoMw,
                    Score = hornResult.Fit,
                    ScoreCountBasis = hornResult.FitCountBasis,
                    MostAbundantIsotopeMass = hornResult.MostIntenseMw
                };

                GetIsotopicProfile(hornResult.IsotopePeakIndices, mspeakList, ref profile);

                profile.IntensityMostAbundant = (float) hornResult.Abundance;
                profile.IntensityMostAbundantTheor = (float) hornResult.Abundance;

                if (NumPeaksUsedInAbundance == 1) // fyi... this is typical
                {
                    result.IntensityAggregate = profile.IntensityMostAbundant;
                }
                else
                {
                    result.IntensityAggregate = SumPeaks(profile, hornResult.Abundance);
                }

                profile.MonoPlusTwoAbundance = profile.GetMonoPlusTwoAbundance();
                profile.MonoPeakMZ = profile.GetMZ();

                result.IsotopicProfile = profile;

                AddDeconResult(resultList, result, DeconResultComboMode.simplyAddIt);
                //resultList.ResultList.Add(result);
                currentScanset.NumIsotopicProfiles++;
            }
        }

        private double SumPeaks(IsotopicProfile profile, double defaultVal)
        {
            if (profile.Peaklist == null || profile.Peaklist.Count == 0)
                return defaultVal;

            var peakListIntensities = new List<float>();
            foreach (var peak in profile.Peaklist)
            {
                peakListIntensities.Add(peak.Height);
            }
            // Provide a custom sort function to sort it in reverse order
            peakListIntensities.Sort((x, y) => y.CompareTo(x));
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

        private void GetIsotopicProfile(List<int> peakIndexList, ThrashV1Peak[] peakdata, ref IsotopicProfile profile)
        {
            if (peakIndexList == null || peakIndexList.Count == 0) return;
            var deconMonopeak = peakdata[peakIndexList[0]];

            profile.Peaklist.Add(deconMonopeak);

            if (peakIndexList.Count == 1) return; //only one peak in the DeconEngine's profile    

            for (var i = 1; i < peakIndexList.Count; i++) //start with second peak and add each peak to profile
            {
                var deconPeak = peakdata[peakIndexList[i]];
                profile.Peaklist.Add(deconPeak);
            }
        }

        #endregion
    }
}
