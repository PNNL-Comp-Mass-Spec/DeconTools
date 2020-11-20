using System;
using System.Collections.Generic;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.ElementalFormulas;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.Mercury;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.PeakProcessing;

namespace DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.FitScoring
{
    /// <summary>
    ///     Base class for calculating isotope fit values between theoretical distribution and observed one.
    /// </summary>
    /// <remarks>
    ///     This class is the base class for the isotope fitting. There are three types of fits:
    ///     Area Fit, Peak Fit, and Chi Sq. Fit.
    /// </remarks>
    /// <seealso cref="DeconTools.Backend.Utilities.IsotopeDistributionCalculation.IsotopicDistributionCalculator"/>
    public abstract class IsotopicProfileFitScorer
    {
        private readonly MercuryCache _mercuryCache = new MercuryCache();

        private bool _lastValueWasCached;

        protected Averagine AveragineObj = new Averagine();
        // mass of the charge carrier.
        protected double ChargeCarrierMass;
        // flag to make the fit function look at all possible isotopes to thrash to. If this is set to false, thrashing stops as soon as we reach a missing isotopic peak.
        protected bool CompleteFitThrash;

        /// <summary>
        ///     this variable helps keep track of the last intensity value at the lower m/z which we looked at with the function
        ///     IsotopeFit.GetPointIntensity
        /// </summary>
        /// <seealso cref="IsotopicProfileFitScorer.GetPointIntensity" />
        protected double Intensity1;

        /// <summary>
        ///     this variable helps keep track of the last intensity value at the higher m/z which we looked at with the function
        ///     IsotopeFit.GetPointIntensity
        /// </summary>
        /// <seealso cref="IsotopicProfileFitScorer.GetPointIntensity" />
        protected double Intensity2;

        // variable to do the calculation of isotopic distribution from mass, using averagine and mercury.
        public MercuryIsotopeDistribution IsotopeDistribution = new MercuryIsotopeDistribution();
        protected List<double> IsotopeIntensities = new List<double>();
        protected List<double> IsotopeMzs = new List<double>();

        /// <summary>
        ///     keeps track of the last index looked at with IsotopeFit.GetPointIntensity
        /// </summary>
        /// <seealso cref="IsotopicProfileFitScorer.GetPointIntensity" />
        protected int LastPointIndex;

        /// <summary>
        ///     this variable helps keep track of the last min m/z value we looked at with the function
        ///     IsotopeFit.GetPointIntensity
        /// </summary>
        /// <seealso cref="IsotopicProfileFitScorer.GetPointIntensity" />
        protected double Mz1;

        /// <summary>
        ///     this variable helps keep track of the last max m/z value we looked at with the function
        ///     IsotopeFit.GetPointIntensity
        /// </summary>
        /// <seealso cref="IsotopicProfileFitScorer.GetPointIntensity" />
        protected double Mz2;

        // List to store intensities from the theoretical distribution. Corresponding mzs are stored in IsotopeFit.mvect_distribution_intensities
        protected List<double> TheoreticalDistIntensities = new List<double>();
        // List to store m/z values from the theoretical distribution. Corresponding intensities are stored in IsotopeFit.mvect_distribution_intensities
        protected List<double> TheoreticalDistMzs = new List<double>();
        // flag to control thrashing about the most intense peak. See details of THRASH algorithm by Horn et. al.
        protected bool UseThrash;

        // default constructor.
        protected IsotopicProfileFitScorer()
        {
            ChargeCarrierMass = 1.00727638;
            UseThrash = false;
            CompleteFitThrash = false;
            UseIsotopeDistributionCaching = false;
            Init();
            AveragineObj.SetElementalIsotopeComposition(IsotopeDistribution.ElementalIsotopeComposition);
        }

        protected IsotopicProfileFitScorer(IsotopicProfileFitScorer fit)
        {
            // only copies settings not variables.
            CompleteFitThrash = fit.CompleteFitThrash;
            UseThrash = fit.UseThrash;
            ChargeCarrierMass = fit.ChargeCarrierMass;
            AveragineObj = new Averagine(fit.AveragineObj);
            IsotopeDistribution = new MercuryIsotopeDistribution(fit.IsotopeDistribution);
            Init();
        }

        public bool UseIsotopeDistributionCaching { get; set; }

        /// <summary>
        ///     Gets or Sets the theoretical isotope composition (AtomicInformation) for all the elements.
        /// </summary>
        public ElementIsotopes ElementalIsotopeComposition
        {
            get => IsotopeDistribution.ElementalIsotopeComposition;
            set
            {
                IsotopeDistribution.SetElementalIsotopeComposition(value);
                AveragineObj.SetElementalIsotopeComposition(value);
            }
        }

        /// <summary>
        ///     Create a new scorer based on the specified fit type, copying scorer settings from another scorer
        /// </summary>
        /// <param name="fitType"></param>
        /// <param name="oldFit"></param>
        /// <returns></returns>
        public static IsotopicProfileFitScorer ScorerFactory(Globals.IsotopicProfileFitType fitType,
            IsotopicProfileFitScorer oldFit)
        {
            var scorer = ScorerFactory(fitType);
            scorer.CloneSettings(oldFit);
            return scorer;
        }

        /// <summary>
        ///     Create a new scorer based on the specified fit type
        /// </summary>
        /// <param name="fitType"></param>
        /// <returns></returns>
        public static IsotopicProfileFitScorer ScorerFactory(Globals.IsotopicProfileFitType fitType)
        {
            IsotopicProfileFitScorer scorer = new AreaFitScorer();
            switch (fitType)
            {
                case Globals.IsotopicProfileFitType.Undefined:
                    throw new Exception(
                        "Error.  IsotopicProfile fit type has not been defined. Cannot be used in HornTransform");
                case Globals.IsotopicProfileFitType.PEAK:
                    scorer = new PeakFitScorer();
                    break;
                case Globals.IsotopicProfileFitType.AREA:
                    scorer = new AreaFitScorer();
                    break;
                case Globals.IsotopicProfileFitType.CHISQ:
                    scorer = new ChiSqFitScorer();
                    break;
            }
            return scorer;
        }

        /// <summary>
        ///     calculates the fit score between the theoretical distribution stored and the observed data. Normalizes the observed
        ///     intensity by specified intensity.
        /// </summary>
        /// <param name="peakData"> variable which stores the data itself</param>
        /// <param name="chargeState"> charge state at which we want to compute the peak.</param>
        /// <param name="peak"> peak for which we want to compute the fit function.</param>
        /// <param name="mzDelta">specifies the mass delta between theoretical and observed m/z with the best fit so far.</param>
        /// <param name="minIntensityForScore">minimum intensity for score</param>
        /// <param name="pointsUsed">number of points used</param>
        /// <param name="debug">debug output flag</param>
        public abstract double FitScore(PeakData peakData, int chargeState, ThrashV1Peak peak, double mzDelta,
            double minIntensityForScore, out int pointsUsed, bool debug = false);

        /// <summary>
        ///     calculates the fit score between the theoretical distribution stored and the observed data. Normalizes the observed
        ///     intensity by specified intensity.
        /// </summary>
        /// <param name="peakData"> variable which stores the data itself</param>
        /// <param name="chargeState"> charge state at which we want to compute the peak.</param>
        /// <param name="normalizer">
        ///     intensity to normalize the peaks to. assumes that if peak with intensity = normalizer was
        ///     present, it would be normalized to 100
        /// </param>
        /// <param name="mzDelta">
        ///     specifies the mass delta between theoretical and observed m/z. The we are looking to score
        ///     against the feature in the observed data at theoretical m/z + mz_delta
        /// </param>
        /// <param name="minIntensityForScore">minimum intensity for score</param>
        /// <param name="debug">prints debugging information if this is set to true.</param>
        public abstract double FitScore(PeakData peakData, int chargeState, double normalizer, double mzDelta,
            double minIntensityForScore, bool debug = false);

        /// <summary>
        ///     gets the intensity for a given mz.
        /// </summary>
        /// <remarks>
        ///     We look for the intensity at a given m/z value in the raw data List mzs
        ///     (the intensities are stored in the corresponding raw data intensity List intensities).
        ///     If the value does not exist, we interpolate the intensities of points before and after this m/z value.
        /// </remarks>
        /// <param name="mz">the m/z value for which we want to find the intensity.</param>
        /// <param name="mzs">pointer to List with observed m/z values.</param>
        /// <param name="intensities">pointer to List with observed intensity values.</param>
        /// <returns>
        ///     returns the intensity of the peak which has the given m/z. If the exact peak is not present,
        ///     then we interpolate the intensity. If the m/z value is greater than the maximum mz or less,
        ///     then the minimum m/z, 0 is returned.
        /// </returns>
        public double GetPointIntensity(double mz, List<double> mzs, List<double> intensities)
        {
            if ((Mz1 < mz && Mz2 < mz) || (Mz1 > mz && Mz2 > mz))
            {
                var numpts = mzs.Count;
                if (LastPointIndex >= numpts)
                    LastPointIndex = -1;

                //since points are more likely to be searched in order.
                if (LastPointIndex != -1 && Mz2 < mz)
                {
                    if (LastPointIndex < numpts - 1 && mzs[LastPointIndex + 1] > mz)
                        LastPointIndex++;
                    else
                        LastPointIndex = PeakIndex.GetNearestBinary(mzs, mz, LastPointIndex,
                            numpts - 1);
                }
                else
                    LastPointIndex = PeakIndex.GetNearestBinary(mzs, mz, 0, numpts - 1);
                if (LastPointIndex >= numpts)
                    return 0;

                if (LastPointIndex < 0)
                    LastPointIndex = 0;

                if (mzs[LastPointIndex] > mz)
                {
                    while (LastPointIndex > 0 && mzs[LastPointIndex] > mz)
                        LastPointIndex--;
                }
                else
                {
                    while (LastPointIndex < numpts && mzs[LastPointIndex] < mz)
                        LastPointIndex++;
                    LastPointIndex--;
                }

                if (LastPointIndex == numpts - 1)
                    LastPointIndex = numpts - 2;

                Mz1 = mzs[LastPointIndex];
                Mz2 = mzs[LastPointIndex + 1];

                Intensity1 = intensities[LastPointIndex];
                Intensity2 = intensities[LastPointIndex + 1];
            }
            if (Mz1.Equals(Mz2))
                return Intensity1;

            return (mz - Mz1) / (Mz2 - Mz1) * (Intensity2 - Intensity1) + Intensity1;
        }

        public void Reset()
        {
            LastPointIndex = -1;
            Mz1 = Mz2 = 0;
            Intensity1 = Intensity2 = 0;
        }

        private void Init()
        {
            Reset();
        }

        public IsotopicProfileFitScorer CloneSettings(IsotopicProfileFitScorer fit)
        {
            // only copies settings not variables.
            CompleteFitThrash = fit.CompleteFitThrash;
            UseThrash = fit.UseThrash;
            ChargeCarrierMass = fit.ChargeCarrierMass;
            AveragineObj = new Averagine(fit.AveragineObj);
            IsotopeDistribution = new MercuryIsotopeDistribution(fit.IsotopeDistribution);
            Init();
            return this;
        }

        public bool FindPeak(double minMz, double maxMz, out double mzValue, out double intensity, bool debug)
        {
            if (!_lastValueWasCached)
            {
                var found = IsotopeDistribution.FindPeak(minMz, maxMz, out mzValue, out intensity);
                return found;
            }

            mzValue = 0;
            intensity = 0;
            var numPts = TheoreticalDistMzs.Count;

            var index = PeakIndex.GetNearestBinary(TheoreticalDistMzs, minMz, 0, numPts - 1);
            if (index >= numPts)
                return false;

            if (TheoreticalDistMzs[index] > minMz)
            {
                while (index > 0 && TheoreticalDistMzs[index] > minMz)
                    index--;
            }
            else
            {
                while (index < numPts && TheoreticalDistMzs[index] < minMz)
                    index++;
                index--;
            }

            //find index of peak with maximum intensity
            var maxIndex = -1;

            for (; index < numPts; index++)
            {
                var mz = TheoreticalDistMzs[index];
                if (mz > maxMz)
                    break;
                if (mz > minMz)
                {
                    if (TheoreticalDistIntensities[index] > intensity)
                    {
                        maxIndex = index;
                        intensity = TheoreticalDistIntensities[index];
                    }
                }
            }
            if (maxIndex == -1)
            {
                return false;
            }

            var x2 = TheoreticalDistMzs[maxIndex];
            var x1 = x2 - 1.0 / IsotopeDistribution.PointsPerAmu;
            var x3 = x2 + 1.0 / IsotopeDistribution.PointsPerAmu;

            if (maxIndex > 0 && maxIndex < numPts - 1)
            {
                var y1 = TheoreticalDistIntensities[maxIndex - 1];
                var y2 = TheoreticalDistIntensities[maxIndex];
                var y3 = TheoreticalDistIntensities[maxIndex + 1];

                // if the points are cached, these could be single sticks with surrounding
                // points below background. To avoid that case, lets just check
                // and see if the differences in the theoretical mz values is as
                // expected or not.
                if (TheoreticalDistMzs[maxIndex - 1] > x2 - 2.0 / IsotopeDistribution.PointsPerAmu
                    && TheoreticalDistMzs[maxIndex + 1] < x2 + 2.0 / IsotopeDistribution.PointsPerAmu)
                {
                    var d = (y2 - y1) * (x3 - x2); //[gord] slope?  what is this?...  Denominator... see below
                    d = d - (y3 - y2) * (x2 - x1); //

                    if (d.Equals(0))
                        mzValue = x2;
                    else
                        mzValue = (x1 + x2 - (y2 - y1) * (x3 - x2) * (x1 - x3) / d) / 2.0;
                    //[gord] what's this doing?? Looks like a mid-point calculation
                }
                else
                {
                    mzValue = x2;
                    intensity = TheoreticalDistIntensities[maxIndex];
                }
                return true;
            }

            mzValue = x2;
            intensity = TheoreticalDistIntensities[maxIndex];
            return true;
        }

        /// <summary>
        ///     checks if any of the isotopes in the distribution is possibly part of a different distribution
        /// </summary>
        /// <param name="minThreshold">- threshold for that spectrum</param>
        public bool IsIsotopeLinkedDistribution(double minThreshold)
        {
            for (var isotopeNum = 3; isotopeNum < IsotopeMzs.Count; isotopeNum++)
            {
                //double mz = IsotopeMzs[isotope_num];
                var intensity = IsotopeIntensities[isotopeNum];
                if (intensity - minThreshold > 50)
                    return true;
            }

            return false;
        }

        /*[gord]  the following is currently unused. The idea was to give weighting to the algorithm so that
          the user could favor certain fitting parameters (i.e. space between isotopomers) over others
        public double FindIsotopicDist(PeakProcessing.PeakData peakData, int cs, PeakProcessing.Peak peak,
            IsotopeFitRecord isoRecord, double deleteIntensityThreshold, double spacingWeight, double spacingVar,
            double signalToNoiseWeight, double signalToNoiseThresh, double ratioWeight, double ratioThreshold,
            double fitWeight, double fitThreshold, bool debug = false)
        {
            if (cs <= 0)
            {
                Environment.Exit(1);
            }

            //Get theoretical distribution using Mercury algorithm
            double peakMass = (peak.mdbl_mz - ChargeCarrierMass) * cs;
            double resolution = peak.mdbl_mz / peak.mdbl_FWHM;
            GetIsotopeDistribution(peakMass, cs, resolution, out TheoreticalDistMzs,
                out TheoreticalDistIntensities,
                deleteIntensityThreshold, debug);

            double theorMostAbundantPeakMz = IsotopeDistribution.mdbl_max_peak_mz;
            double delta = peak.mdbl_mz - theorMostAbundantPeakMz;
            double spacingScore = 0;
            double signalToNoiseScore = 0;
            double ratioScore = 0;
            double totalScore = 0;
            double maximumScore = spacingWeight + signalToNoiseWeight + ratioWeight + fitWeight;

            //this will select peaks to the left until
            for (double dd = 1.003 / cs; dd <= 10.03 / cs; dd += 1.003 / cs)
            {
                double theorLeftPeakMz = 0;
                double theorLeftPeakIntensity = 0;
                PeakProcessing.Peak leftPeak;
                peakData.FindPeak(peak.mdbl_mz - dd - peak.mdbl_FWHM, peak.mdbl_mz - dd + peak.mdbl_FWHM, out leftPeak);
                //PeakProcessing.FindPeak
                IsotopeDistribution.FindPeak(theorMostAbundantPeakMz - dd - 0.2 / cs,
                    theorMostAbundantPeakMz - dd + 0.2 / cs, out theorLeftPeakMz, out theorLeftPeakIntensity);

                if (leftPeak.mdbl_mz > 0) //if there is an experimental peak...
                {
                    //get spacing score
                    spacingScore = spacingWeight * 1;

                    //get S/N score
                    if (leftPeak.mdbl_SN > signalToNoiseThresh)
                    {
                        signalToNoiseScore = signalToNoiseWeight * 1;
                    }

                    //get Ratio score
                    double leftPeakRatio = leftPeak.mdbl_intensity / peak.mdbl_intensity;
                    double theorLeftPeakRatio = theorLeftPeakIntensity / 1;
                    //TODO: need to check if this most abundant theor peak's intensity is 1
                }

                //get Ratio score
            }
            //get S/N score
            //get Fit score
            //calculate maximum score
            //get overall score
            return 0;
        }*/

        public PeakData GetTheoreticalIsotopicDistributionPeakList(List<double> xvals, List<double> yvals)
        {
            var peakList = new PeakData();
            var processor = new PeakProcessor();
            processor.SetOptions(0.5, 1, false, PeakFitType.Apex);
            processor.DiscoverPeaks(xvals, yvals, 0, 10000);

            var numpeaks = processor.PeakData.GetNumPeaks();

            for (var i = 0; i < numpeaks; i++)
            {
                processor.PeakData.GetPeak(i, out var peak);
                peakList.AddPeak(peak);
            }

            return peakList;
        }

        /// <summary>
        ///     calculates the fit score for a peak.
        /// </summary>
        /// <param name="peakData"> variable which stores the data itself</param>
        /// <param name="chargeState"> charge state at which we want to compute the peak.</param>
        /// <param name="peak"> peak for which we want to compute the fit function.</param>
        /// <param name="isoRecord">stores the result of the fit.</param>
        /// <param name="deleteIntensityThreshold">intensity of least isotope to delete.</param>
        /// <param name="minTheoreticalIntensityForScore">minimum intensity of point to consider for scoring purposes.</param>
        /// <param name="leftFitStringencyFactor"></param>
        /// <param name="rightFitStringencyFactor"></param>
        /// <param name="pointsUsed">Number of points used</param>
        /// <param name="debug">enable debugging output</param>
        /// <remarks>
        ///     This fitter is used by MassTransform.cpp.   It does more than just get a fit score. It first
        ///     gets a fit score and then slides to the left until the fit score does not improve and then resets
        ///     to the center point and then slides to the right until the fit score does not improve. Returns the
        ///     best fit score and fills the isotopic profile (isotopeFitRecord)
        /// </remarks>
        public double GetFitScore(PeakData peakData, int chargeState, ref ThrashV1Peak peak,
            out HornTransformResults isoRecord, double deleteIntensityThreshold, double minTheoreticalIntensityForScore,
            double leftFitStringencyFactor, double rightFitStringencyFactor, out int pointsUsed, bool debug = false)
        {
            isoRecord = new HornTransformResults();
            if (chargeState <= 0)
            {
                Console.WriteLine("Negative value for charge state. " + chargeState);
                Environment.Exit(1);
            }
            //initialize
            var peakMass = (peak.Mz - ChargeCarrierMass) * chargeState;
            // by now the cc_mass, tag formula and media options in Mercury(Isotope generation)
            // should be set.
            if (debug)
            {
                Console.WriteLine("\n\n-------------------- BEGIN TRANSFORM ---------------------------" + chargeState);
                Console.WriteLine("Getting isotope distribution for mass = " + peakMass + " mz = " + peak.Mz +
                                  " charge = " + chargeState);
            }
            var resolution = peak.Mz / peak.FWHM;
            // DJ Jan 07 2007: Need to get all peaks down to delete interval so that range of deletion is correct.
            //GetIsotopeDistribution(peakMass, chargeState, resolution, TheoreticalDistMzs, TheoreticalDistIntensities,
            //  deleteIntensityThreshold, debug);
            GetIsotopeDistribution(peakMass, chargeState, resolution, out TheoreticalDistMzs,
                out TheoreticalDistIntensities, deleteIntensityThreshold, debug);
            var theorPeakData = GetTheoreticalIsotopicDistributionPeakList(TheoreticalDistMzs,
                TheoreticalDistIntensities);
            var numpeaks = theorPeakData.GetNumPeaks();

            if (debug)
            {
                Console.WriteLine("---------------------------------------- THEORETICAL PEAKS ------------------");
                Console.WriteLine("Theoretical peak\t" + "Index\t" + "MZ\t" + "Intensity\t" + "FWHM\t" + "SigNoise");
                for (var i = 0; i < numpeaks; i++)
                {
                    theorPeakData.GetPeak(i, out var theorpeak);
                    Console.WriteLine("Theoretical peak\t" + i + "\t" + theorpeak.Mz + "\t" +
                                      theorpeak.Intensity + "\t" + theorpeak.FWHM + "\t" + theorpeak.SignalToNoiseDbl);
                }
                Console.WriteLine("----------------------------------- END THEORETICAL PEAKS ------------------");
            }

            // Anoop April 9 2007: For checking if the distribution does not overlap/link with any other distribution
            // Beginnings of deisotoping correction
            //bool is_linked = false;
            //is_linked =  IsIsotopeLinkedDistribution(deleteIntensityThreshold);
            var delta = peak.Mz - IsotopeDistribution.MaxPeakMz;

            //if(debug)
            //  System.Console.WriteLine("Going for first fit");
            var fit = FitScore(peakData, chargeState, peak, delta, minTheoreticalIntensityForScore, out pointsUsed,
                debug);

            if (debug)
            {
                Console.WriteLine("Peak\tPeakIdx\tmz\tintens\tSN\tFWHM\tfit\tdelta");
                Console.WriteLine("CENTER\t" + peak.PeakIndex + "\t" + peak.Mz + "\t" + peak.Intensity +
                                  "\t" + peak.SignalToNoiseDbl + "\t" + peak.FWHM + "\t" + fit + "\t" + delta + "\t");
            }

            if (!UseThrash)
            {
                isoRecord.Fit = fit;
                isoRecord.FitCountBasis = pointsUsed;
                isoRecord.Mz = peak.Mz;
                isoRecord.AverageMw = IsotopeDistribution.AverageMw + delta * chargeState;
                isoRecord.MonoMw = IsotopeDistribution.MonoMw + delta * chargeState;
                isoRecord.MostIntenseMw = IsotopeDistribution.MostIntenseMw + delta * chargeState;
                isoRecord.DeltaMz = delta;
                return fit;
            }

            double p1Fit = -1, m1Fit = -1; // [gord]: this seems unused
            var mPeak = IsotopeDistribution.MaxPeakMz;
            var nextPeak = new ThrashV1Peak(0);

            var bestFit = fit;
            var bestFitCountBasis = pointsUsed;
            var bestDelta = delta;
            //double maxY = peak.mdbl_intensity;

            var fitCountBasis = 0;

            //------------- Slide to the LEFT --------------------------------------------------
            for (var dd = 1.003 / chargeState; dd <= 10.03 / chargeState; dd += 1.003 / chargeState)
            {
                //check for theoretical peak to the right of TheoreticalMaxPeak; store mz and intensity
                var foundPeak = FindPeak(mPeak + dd - 0.2 / chargeState, mPeak + dd + 0.2 / chargeState, out var mzLeft,
                    out _,
                    debug);

                // if the above theoretical peak was found,  look one peak to the LEFT in the Experimental peaklist
                if (foundPeak)
                {
                    peakData.FindPeak(peak.Mz - dd - peak.FWHM, peak.Mz - dd + peak.FWHM,
                        out nextPeak);
                }

                if (mzLeft > 0 && nextPeak.Mz > 0)
                //if there is a theoreticalPeak to the RIGHT of theoreticalMaxPeak AND there is an experimentalPeak to the LEFT of experimentalMaxPeak...
                {
                    delta = peak.Mz - mzLeft;
                    // essentially, this shifts the theoretical over to the left and gets the delta; then check the fit
                    var currentPeakCopy = new ThrashV1Peak(peak)
                    {
                        Intensity = nextPeak.Intensity      // in c++ this copy is created by value;
                    };

                    fit = FitScore(peakData, chargeState, currentPeakCopy, delta, minTheoreticalIntensityForScore,
                        out fitCountBasis, debug);
                    if (debug)
                    {
                        //System.Console.WriteLine(" isotopes. Fit =" + fit + " Charge = " + cs + " Intensity = " + nxt_peak.mdbl_intensity + " delta = " + delta);
                        Console.WriteLine("LEFT\t" + nextPeak.PeakIndex + "\t" + nextPeak.Mz + "\t" +
                                          nextPeak.Intensity + "\t" + nextPeak.SignalToNoiseDbl + "\t" + nextPeak.FWHM +
                                          "\t" + fit + "\t" + delta);
                    }
                }
                else
                {
                    if (debug)
                        Console.WriteLine("LEFT\t" + -1 + "\t" + -1 + "\t" + -1 + "\t" + -1 + "\t" + -1 + "\t" + -1 +
                                          "\t" + -1);
                    fit = bestFit + 1000; // make the fit terrible
                }
                // TODO: Currently, if fit score is less than best_fit, iteration stops.  Future versions should continue attempted fitting if fit was within a specified range of the best fit
                // 26th February 2007 Deep Jaitly
                /*if (fit <= bestFit)
                {
                    if (nextPeak.mdbl_intensity > peak.mdbl_intensity)
                        peak.mdbl_intensity = nextPeak.mdbl_intensity;
                    maxY = peak.mdbl_intensity;
                    bestFit = fit;
                    bestDelta = delta;
                }*/
                var leftFitFactor = fit / bestFit;
                if (leftFitFactor <= leftFitStringencyFactor)
                {
                    if (nextPeak.Intensity > peak.Intensity)
                        peak.Intensity = nextPeak.Intensity;
                    //maxY = peak.mdbl_intensity;
                    bestFit = fit;
                    bestFitCountBasis = fitCountBasis;
                    bestDelta = delta;
                }
                else
                {
                    if (p1Fit.Equals(-1)) //[gord]   what is this doing?  Peak1 fit??
                        p1Fit = fit;
                    if (!CompleteFitThrash)
                        break;
                }
            }

            //if (debug)
            //      System.Console.WriteLine("\n---------------- Sliding to the RIGHT -------------------------";
            for (var dd = 1.003 / chargeState; dd <= 10.03 / chargeState; dd += 1.003 / chargeState)
            {
                ////check for theoretical peak to the LEFT of TheoreticalMaxPeak; store mz and intensity
                var foundPeak = FindPeak(mPeak - dd - 0.2 / chargeState, mPeak - dd + 0.2 / chargeState, out var mzRight,
                    out _,
                    debug);

                // if the above theoretical peak was found,  look one peak to the RIGHT in the Experimental peaklist
                if (foundPeak)
                {
                    peakData.FindPeak(peak.Mz + dd - peak.FWHM, peak.Mz + dd + peak.FWHM,
                        out nextPeak);
                }

                if (mzRight > 0 && nextPeak.Mz > 0)
                {
                    delta = peak.Mz - mzRight;
                    var currentPeakCopy = new ThrashV1Peak(peak)
                    {
                        Intensity = nextPeak.Intensity
                    };

                    fit = FitScore(peakData, chargeState, currentPeakCopy, delta, minTheoreticalIntensityForScore,
                        out fitCountBasis, debug);
                    //fit = FitScore(pk_data, cs, nxt_peak.mdbl_intensity, delta);
                    if (debug)
                    {
                        //System.Console.WriteLine(" isotopes. Fit =" + fit + " Charge = " + chargeState + " Intensity = " + nextPeak.mdbl_intensity + " delta = " + delta);
                        Console.WriteLine("RIGHT\t" + nextPeak.PeakIndex + "\t" + nextPeak.Mz + "\t" +
                                          nextPeak.Intensity + "\t" + nextPeak.SignalToNoiseDbl + "\t" + nextPeak.FWHM +
                                          "\t" + fit + "\t" + delta);
                    }
                }
                else
                {
                    fit = bestFit + 1000; //force it to be a bad fit
                    if (debug)
                    {
                        //  System.Console.WriteLine("No peak found");
                        Console.WriteLine("RIGHT\t" + -1 + "\t" + -1 + "\t" + -1 + "\t" + -1 + "\t" + -1 + "\t" + -1 +
                                          "\t" + -1);
                    }
                }

                /*if (fit <= bestFit)
                {
                if (nextPeak.mdbl_intensity > peak.mdbl_intensity)
                peak.mdbl_intensity = nextPeak.mdbl_intensity;
                MaxY = peak.mdbl_intensity;
                bestFit = fit;
                bestDelta = delta;
                }*/
                var rightFitFactor = fit / bestFit;
                if (rightFitFactor <= rightFitStringencyFactor)
                {
                    if (nextPeak.Intensity > peak.Intensity)
                        peak.Intensity = nextPeak.Intensity;
                    //maxY = peak.mdbl_intensity;
                    bestFit = fit;
                    bestFitCountBasis = fitCountBasis;
                    bestDelta = delta;
                }
                else
                {
                    if (m1Fit.Equals(-1))
                        m1Fit = fit;
                    if (!CompleteFitThrash)
                        break;
                }
            }

            //double theorIntensityCutoff = 30; //
            //double peakWidth = peak.mdbl_FWHM;
            if (debug)
            {
                Console.WriteLine("Std delta = \t" + bestDelta);
            }
            //best_delta = CalculateDeltaFromSeveralObservedPeaks(best_delta, peakWidth, pk_data, theorPeakData, theorIntensityCutoff);
            if (debug)
            {
                Console.WriteLine("Weighted delta = \t" + bestDelta);
            }

            isoRecord.Fit = bestFit;
            isoRecord.FitCountBasis = bestFitCountBasis;
            isoRecord.ChargeState = chargeState;
            isoRecord.Mz = peak.Mz;
            isoRecord.DeltaMz = bestDelta;
            isoRecord.AverageMw = IsotopeDistribution.AverageMw + bestDelta * chargeState;
            isoRecord.MonoMw = IsotopeDistribution.MonoMw + bestDelta * chargeState;
            isoRecord.MostIntenseMw = IsotopeDistribution.MostIntenseMw + bestDelta * chargeState;

            //iso_record.mbln_flag_isotope_link = is_linked;
            pointsUsed = bestFitCountBasis;
            return bestFit;
        }

        /// <summary>
        ///     calculates the fit score for a peak against a molecular formula.
        /// </summary>
        /// <param name="peakData"> variable which stores the data itself</param>
        /// <param name="chargeState"> charge state at which we want to compute the peak.</param>
        /// <param name="peak"> peak for which we want to compute the fit function.</param>
        /// <param name="formula">stores the formula we want to fit.</param>
        /// <param name="deleteIntensityThreshold">intensity of least isotope to delete.</param>
        /// <param name="minTheoreticalIntensityForScore">minimum intensity of point to consider for scoring purposes.</param>
        /// <param name="debug">if debugging output is enabled</param>
        public double GetFitScore(PeakData peakData, int chargeState, ThrashV1Peak peak, MolecularFormula formula,
            double deleteIntensityThreshold, double minTheoreticalIntensityForScore, bool debug = false)
        {
            if (chargeState <= 0)
            {
                Console.WriteLine("Negative value for charge state. " + chargeState);
                Environment.Exit(1);
            }
            if (debug)
                Console.WriteLine("Getting isotope distribution for formula = " + formula + " mz = " + peak.Mz +
                                  " charge = " + chargeState);

            var resolution = peak.Mz / peak.FWHM;

            IsotopeDistribution.ChargeCarrierMass = ChargeCarrierMass;
            IsotopeDistribution.ApType = ApodizationType.Gaussian;

            TheoreticalDistIntensities.Clear();
            TheoreticalDistMzs.Clear();

            IsotopeDistribution.CalculateDistribution(chargeState, resolution, formula, out TheoreticalDistMzs,
                out TheoreticalDistIntensities, deleteIntensityThreshold, out IsotopeMzs, out IsotopeIntensities, debug);

            var delta = peak.Mz - IsotopeDistribution.MaxPeakMz;
            if (debug)
                Console.WriteLine("Going for first fit");

            return FitScore(peakData, chargeState, peak, delta, minTheoreticalIntensityForScore, out _, debug);
        }

        /// <summary>
        ///     set options for the isotope fit. It also sets the options for theoretical isotope generation.
        /// </summary>
        /// <param name="averagineFormula">is the averagine molecular formula.</param>
        /// <param name="tagFormula">is the molecular formula of the labeling tag used to label peptide ("" if no tag was used).</param>
        /// <param name="chargeCarrierMass">is the charge carrier mass.</param>
        /// <param name="useThrash">specifies whether or not to do thrashing. See details of THRASH by Horn et. al.</param>
        /// <param name="completeFitThrash">
        ///     if thrashing is enable, we may want to thrash not just one or two isotopes (as the score improves)
        ///     but to all possible isotopes. If this value is true, then the thrashing continues to all isotopes looking for
        ///     better scores. If false and thrash_or_not is true, then thrashing only continues as long as the fit score
        ///     keeps increasing. If thrash_or_not is false, none is performed.
        /// </param>
        public void SetOptions(string averagineFormula, string tagFormula, double chargeCarrierMass, bool useThrash,
            bool completeFitThrash)
        {
            AveragineObj.SetElementalIsotopeComposition(IsotopeDistribution.ElementalIsotopeComposition);
            AveragineObj.AveragineFormula = averagineFormula;
            AveragineObj.TagFormula = tagFormula;
            UseThrash = useThrash;
            CompleteFitThrash = completeFitThrash;
            ChargeCarrierMass = chargeCarrierMass;
            _mercuryCache.MercurySize = IsotopeDistribution.MercurySize;
        }

        /// <summary>
        ///     specifies the mass range of the theoretical distribution which covers all points that are of intensity greater than
        ///     specified value.
        /// </summary>
        /// <param name="startMz">variable to store the value of the starting m/z</param>
        /// <param name="stopMz">variable to store the value of the ending m/z</param>
        /// <param name="delta">variable to store the mz delta between theoretical profile and observed.</param>
        /// <param name="thresh">specifies the threshold intensity that the mass range should necessarily cover.</param>
        /// <param name="debug">whether to print debug messages or not. (false by default).</param>
        /// <remarks>
        ///     This function is used to find out the mass range that should be used to zero out peaks after the deisotoping
        ///     of that peak is complete. For this we need to know how much of the mass range the peak would cover.
        /// </remarks>
        public void GetZeroingMassRange(out double startMz, out double stopMz, double delta, double thresh,
            bool debug = false)
        {
            startMz = 0;
            stopMz = 0;
            // this assumes that the last peak was not changed till now.
            for (var i = 0; i < TheoreticalDistIntensities.Count; i++)
            {
                var intensity = TheoreticalDistIntensities[i];
                if (intensity > thresh)
                {
                    startMz = TheoreticalDistMzs[i];
                    stopMz = startMz;
                    break;
                }
            }

            for (var i = TheoreticalDistIntensities.Count - 1; i > 0; i--)
            {
                if (TheoreticalDistIntensities[i] > thresh)
                {
                    stopMz = TheoreticalDistMzs[i];
                    break;
                }
            }
            startMz = startMz + delta;
            // TODO: verify/check; make an O18 mode flag that keeps this as added; regular mode no change/subtract/what?
            stopMz = stopMz + delta; // TODO: Test and check changing this to "stop_mz + delta / 2
            if (debug)
            {
                Console.WriteLine("\t Start MZ for deletion =" + startMz + " Stop MZ for deletion = " + stopMz);
            }
        }

        /// <summary>
        ///     Gets the isotope distribution for the given most abundance mass and charge with provided resolution.
        /// </summary>
        /// <param name="mostAbundantMass">
        ///     Specifies the mass of the observed distribution which is believed to represent the most
        ///     intense isotope.
        /// </param>
        /// <param name="charge">charge of the species</param>
        /// <param name="resolution">resolution at which the theoretical profile should be generated.</param>
        /// <param name="mzs">vector for output of mz values .</param>
        /// <param name="intensities">vector for output of intensity values .</param>
        /// <param name="minTheoreticalIntensity">intensity of the minimum point to be provided in the vectors as output.</param>
        /// <param name="debug">if debugging info should output</param>
        protected void GetIsotopeDistribution(double mostAbundantMass, int charge, double resolution,
            out List<double> mzs, out List<double> intensities, double minTheoreticalIntensity, bool debug)
        {
            mzs = new List<double>();
            intensities = new List<double>();
            var currentMz = mostAbundantMass / charge + ChargeCarrierMass;
            var fwhm = currentMz / resolution;

            //first check the UseCaching option; then check if it is in the cache or not.
            //if it is in the cache, the data is retrieved and the method returns true
            //so If either is false then need to Calculate theor isotopic dist.
            var needToCalculateIsotopeDistribution = !UseIsotopeDistributionCaching ||
                                                     !_mercuryCache.GetIsotopeDistributionCached(mostAbundantMass,
                                                         charge, fwhm, minTheoreticalIntensity, out mzs, out intensities);

            if (needToCalculateIsotopeDistribution)
            {
                _lastValueWasCached = false;
                var empiricalFormula = AveragineObj.GetAverageFormulaForMass(mostAbundantMass);
                //long lngCharge = (long) charge;

                IsotopeDistribution.ChargeCarrierMass = ChargeCarrierMass;
                IsotopeDistribution.ApType = ApodizationType.Gaussian;

                intensities.Clear();
                mzs.Clear();

                if (debug)
                {
                    Console.WriteLine("Getting distribution for chemical =" + empiricalFormula);
                }
                IsotopeDistribution.CalculateDistribution(charge, resolution, empiricalFormula, out mzs, out intensities,
                    minTheoreticalIntensity, out IsotopeMzs, out IsotopeIntensities, debug);
                if (UseIsotopeDistributionCaching)
                {
                    _mercuryCache.CacheIsotopeDistribution(mostAbundantMass, IsotopeDistribution.MostIntenseMw,
                        IsotopeDistribution.MonoMw, IsotopeDistribution.AverageMw,
                        IsotopeDistribution.MaxPeakMz, charge, fwhm, IsotopeDistribution.MassVariance,
                        IsotopeDistribution.PointsPerAmu, minTheoreticalIntensity, IsotopeMzs, IsotopeIntensities);
                }
            }
            else
            {
                _lastValueWasCached = true;
                //mzs, intensities are fetched, get the average and mono mw stats.
                IsotopeDistribution.AverageMw = _mercuryCache.AverageMw;
                IsotopeDistribution.MonoMw = _mercuryCache.MonoMw;
                IsotopeDistribution.MostIntenseMw = _mercuryCache.MostIntenseMw;
                IsotopeDistribution.MaxPeakMz = _mercuryCache.MostIntenseMw / charge + ChargeCarrierMass -
                                                MercuryCache.ElectronMass;
            }
        }

        /// <summary>
        ///     will calculate the delta mz (referenced to the theor) based on several of the observed peaks
        /// </summary>
        /// <param name="startingDelta"></param>
        /// <param name="peakWidth"></param>
        /// <param name="obsPeakData"></param>
        /// <param name="theorPeakData"></param>
        /// <param name="theorIntensityCutOff"></param>
        /// <returns></returns>
        public double CalculateDeltaFromSeveralObservedPeaks(double startingDelta, double peakWidth,
            PeakData obsPeakData, PeakData theorPeakData, double theorIntensityCutOff)
        {
            //the idea is to use a selected number of theor peaks
            //and for each theor peak,  use the delta (mz offset) info
            //to find the obs peak data and determine the delta value for that peak.
            //accumulate delta values in an array and then calculate a weighted average

            var numTheorPeaks = theorPeakData.GetNumPeaks();
            var filteredTheorPeakData = new PeakData();

            //filter the theor list
            var numFilteredTheorPeaks = 0;
            for (var i = 0; i < numTheorPeaks; i++)
            {
                theorPeakData.GetPeak(i, out var peak);

                if (peak.Intensity >= theorIntensityCutOff)
                {
                    filteredTheorPeakData.AddPeak(peak);
                    numFilteredTheorPeaks++;
                }
            }

            if (numFilteredTheorPeaks == 0)
                return startingDelta;

            var deltaArray = new double[numFilteredTheorPeaks];
            var intensityArray = new double[numFilteredTheorPeaks];
            double intensitySum = 0;

            //double weightedSumOfDeltas = 0;

            for (var i = 0; i < numFilteredTheorPeaks; i++)
            {
                filteredTheorPeakData.GetPeak(i, out var theorPeak);

                var targetMzLower = theorPeak.Mz + startingDelta - peakWidth;
                var targetMzUpper = theorPeak.Mz + startingDelta + peakWidth;

                obsPeakData.FindPeak(targetMzLower, targetMzUpper, out var foundPeak);

                if (foundPeak.Mz > 0)
                {
                    deltaArray[i] = foundPeak.Mz - theorPeak.Mz;
                    intensityArray[i] = foundPeak.Intensity;
                    intensitySum += foundPeak.Intensity;
                }
                else
                {
                    deltaArray[i] = startingDelta;
                    intensityArray[i] = 0;
                    //obs peak was not found; therefore assign 0 intensity (will have no effect on delta calc)
                }
            }

            if (intensitySum.Equals(0))
                return startingDelta; // no obs peaks found at all;  return default

            //now perform a weighted average
            double weightedDelta = 0;
            for (var i = 0; i < numFilteredTheorPeaks; i++)
            {
                weightedDelta += intensityArray[i] / intensitySum * deltaArray[i];
            }

            return weightedDelta;
        }
    }
}