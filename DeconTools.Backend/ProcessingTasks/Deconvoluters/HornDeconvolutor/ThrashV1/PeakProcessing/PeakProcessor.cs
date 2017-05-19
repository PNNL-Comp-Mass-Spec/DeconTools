using System;
using System.Collections.Generic;

namespace DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.PeakProcessing
{
    /// <summary>
    ///     class that does the processing of peaks.
    /// </summary>
    internal class PeakProcessor
    {
        /// <summary>
        ///     This variable helps find the m/z value of a peak using the specified fit function.
        /// </summary>
        private readonly PeakFitter _peakFit = new PeakFitter();

        /// <summary>
        ///     True if peaks are centroided
        /// </summary>
        private bool _arePeaksCentroided;

        /// <summary>
        ///     background intensity. When user sets min signal to noise, and min intensity, this value is set as min intensity /
        ///     min signal to noise.
        /// </summary>
        private double _backgroundIntensity;

        /// <summary>
        ///     if the data is thresholded, the ratio is taken as the ratio to background intensity.
        /// </summary>
        private bool _isDataThresholded;

        /// <summary>
        ///     minimum intensity for a point to be considered a peak.
        /// </summary>
        private double _peakIntensityThreshold;

        /// <summary>
        ///     signal to noise threshold for a peak to be considered as a peak.
        /// </summary>
        private double _signalToNoiseThreshold;

        /// <summary>
        ///     PeakData instance that stores the peaks found by an instance of this PeakProcessor.
        /// </summary>
        public PeakData PeakData;

        /// <summary>
        ///     PeakStatistician instance that is used to calculate signal to noise and full width at half maximum for the peaks in
        ///     the raw data.
        /// </summary>
        public PeakStatistician PeakStatistician = new PeakStatistician();

        /// <summary>
        ///     default constructor.
        /// </summary>
        public PeakProcessor()
        {
            SetPeakFitType(PeakFitType.Quadratic);
            PeakData = new PeakData();
            _arePeaksCentroided = false;
        }

        /// <summary>
        ///     sets the threshold for signal to noise for a peak to be considered as real.
        /// </summary>
        /// <param name="signalToNoise">is the signal to noise threshold value.</param>
        /// <remarks>
        ///     For a peak to be considered real it has to pass two criterias:
        ///     - Its signal to noise must be greater than the threshold <see cref="_signalToNoiseThreshold" />
        ///     - Its intensity needs to be greater than the threshold <see cref="_peakIntensityThreshold" />
        /// </remarks>
        public void SetSignalToNoiseThreshold(double signalToNoise)
        {
            _signalToNoiseThreshold = signalToNoise;
            if (_isDataThresholded)
            {
                if (!_signalToNoiseThreshold.Equals(0))
                    _backgroundIntensity = _peakIntensityThreshold / _signalToNoiseThreshold;
                else
                    _backgroundIntensity = 1;
            }
        }

        /// <summary>
        ///     sets the threshold intensity for a peak to be considered a peak.
        /// </summary>
        /// <param name="threshold">is the threshold peak intensity.</param>
        /// <remarks>
        ///     If threshold is less than zero, then the Math.Abs value of the threshold is used as the cutoff intensity.
        ///     However, if threshold is greater than equal to zero, otherwise it is proportional to threshold * background
        ///     intensity in scan.
        /// </remarks>
        /// <remarks>
        ///     For a peak to be considered real it has to pass two criterias:
        ///     - Its signal to noise must be greater than the threshold (PeakProcessor.mdbl_signal_2_noise_threshold)
        ///     - Its intensity needs to be greater than the threshold (PeakProcessor.mdbl_peak_intensity_threshold)
        /// </remarks>
        public void SetPeakIntensityThreshold(double threshold)
        {
            _peakIntensityThreshold = threshold;
            if (_isDataThresholded)
            {
                if (!_signalToNoiseThreshold.Equals(0))
                    _backgroundIntensity = threshold / _signalToNoiseThreshold;
                else if (!threshold.Equals(0))
                    _backgroundIntensity = threshold;
                else
                    _backgroundIntensity = 1;
            }
        }

        /// <summary>
        ///     sets the type of peak fitting used to find m/z values for peaks.
        /// </summary>
        /// <param name="type">specifies the type of peak fitting.</param>
        public void SetPeakFitType(PeakFitType type)
        {
            _peakFit.SetOptions(type);
        }

#if !Disable_Obsolete
        /// <summary>
        ///     sets the type of profile
        /// </summary>
        /// <param name="profile">is a boolean, true if profile data, false if centroided</param>
        [Obsolete("Only used by Decon2LS.UI", false)]
        public void SetPeaksProfileType(bool profile)
        {
            _arePeaksCentroided = !profile;
        }
#endif

        /// <summary>
        ///     sets the options for this instance.
        /// </summary>
        /// <param name="signalToNoise">sets the threshold signal to noise value.</param>
        /// <param name="thresh">sets the peak intensity threshold.</param>
        /// <param name="thresholded">if the data is thresholded.</param>
        /// <param name="type">sets the type of peak fitting algorithm used.</param>
        public void SetOptions(double signalToNoise, double thresh, bool thresholded, PeakFitType type)
        {
            _isDataThresholded = thresholded;
            // signal to noise should ideally be set before PeakIntensityThreshold
            SetSignalToNoiseThreshold(signalToNoise);
            SetPeakIntensityThreshold(thresh);
            SetPeakFitType(type);
        }

        /// <summary>
        ///     Function discovers peaks in the m/z and intensity vectors supplied within the supplied m/z window.
        /// </summary>
        /// <param name="mzList">is the pointer to List of m/z values</param>
        /// <param name="intensityList">is the pointer to List of intensity values</param>
        /// <param name="startMz">minimum m/z of the peak.</param>
        /// <param name="stopMz">maximum m/z of the peak.</param>
        /// <returns>returns the number of peaks that were found in the vectors.</returns>
        /// <remarks>
        ///     The function uses <see cref="PeakStatistician.FindFwhm" />, and
        ///     <see cref="PeakStatistician.FindSignalToNoise" />
        ///     to discover the full width at half maximum and signal to noise values for a peak. The signal to noise of a
        ///     peak is tested against the threshold value before its accepted as a peak. All peaks are used during the process,
        ///     but once generated only those which are above <see cref="_peakIntensityThreshold" /> are tested for peptidicity by
        ///     Deconvolution.HornMassTransform
        /// </remarks>
        public int DiscoverPeaks(List<double> mzList, List<double> intensityList, double startMz, double stopMz)
        {
            if (intensityList.Count < 1)
                return 0;

            PeakData.Clear();
            var numDataPts = intensityList.Count;

            var startIndex = PeakIndex.GetNearestBinary(mzList, startMz, 0, numDataPts - 1);
            var stopIndex = PeakIndex.GetNearestBinary(mzList, stopMz, startIndex, numDataPts - 1);
            if (startIndex <= 0)
                startIndex = 1;
            if (stopIndex >= mzList.Count - 2)
                stopIndex = mzList.Count - 2;

            for (var index = startIndex; index <= stopIndex; index++)
            {
                double fwhm = -1;
                var currentIntensity = intensityList[index];
                var lastIntensity = intensityList[index - 1];
                var nextIntensity = intensityList[index + 1];
                var currentMz = mzList[index];

                if (_arePeaksCentroided)
                {
                    if (currentIntensity >= _peakIntensityThreshold)
                    {
                        var mz = mzList[index];
                        var signalToNoise = currentIntensity / _peakIntensityThreshold;
                        fwhm = 0.6;
                        PeakData.AddPeak(new ThrashV1Peak(mz, currentIntensity, signalToNoise, PeakData.GetNumPeaks(), index,
                            fwhm));
                    }
                }
                else
                {
                    //three point peak picking. Check if peak is greater than both the previous and next points
                    if (currentIntensity >= lastIntensity && currentIntensity >= nextIntensity &&
                        currentIntensity >= _peakIntensityThreshold)
                    {
                        //See if the peak meets the conditions.
                        //The peak data will be found at _transformData.begin()+i+1.
                        double signalToNoise;

                        if (!_isDataThresholded)
                            signalToNoise = PeakStatistician.FindSignalToNoise(currentIntensity, intensityList, index);
                        else
                            signalToNoise = currentIntensity / _backgroundIntensity;

                        // Run Full-Width Half-Max algorithm to try and squeak out a higher SN
                        if (signalToNoise < _signalToNoiseThreshold)
                        {
                            //double mz = mzList[index];
                            fwhm = PeakStatistician.FindFwhm(mzList, intensityList, index, signalToNoise);
                            if (fwhm > 0 && fwhm < 0.5)
                            {
                                var ilow = PeakIndex.GetNearestBinary(mzList, currentMz - fwhm, 0, index);
                                var ihigh = PeakIndex.GetNearestBinary(mzList, currentMz + fwhm, index, stopIndex);

                                var lowIntensity = intensityList[ilow];
                                var highIntensity = intensityList[ihigh];

                                var sumIntensity = lowIntensity + highIntensity;
                                if (sumIntensity > 0)
                                    signalToNoise = 2.0 * currentIntensity / sumIntensity;
                                else
                                    signalToNoise = 10;
                            }
                        }
                        // Found a peak
                        if (signalToNoise >= _signalToNoiseThreshold)
                        {
                            // Find a more accurate m/z location of the peak.
                            var fittedPeak = _peakFit.Fit(index, mzList, intensityList);
                            if (fwhm.Equals(-1))
                            {
                                fwhm = PeakStatistician.FindFwhm(mzList, intensityList, index, signalToNoise);
                            }

                            if (fwhm > 0)
                            {
                                PeakData.AddPeak(new ThrashV1Peak(fittedPeak, currentIntensity, signalToNoise,
                                    PeakData.GetNumPeaks(),
                                    index, fwhm));
                            }
                            // move beyond peaks have the same intensity.
                            var incremented = false;
                            while (index < numDataPts && intensityList[index].Equals(currentIntensity))
                            {
                                incremented = true;
                                index++;
                            }
                            if (index > 0 && index < numDataPts
                                && incremented)
                                index--;
                        }
                    }
                }
            }
            PeakData.MzList = mzList;
            PeakData.IntensityList = intensityList;

            return PeakData.GetNumPeaks();
        }

        /// <summary>
        ///     Gets the closest to peakMz among the peak list mzList
        /// </summary>
        /// <param name="peakMz"></param>
        /// <param name="peak"></param>
        /// <returns></returns>
        public double GetClosestPeakMz(double peakMz, out ThrashV1Peak peak)
        {
            //looks through the peak list and finds the closest peak to peakMz
            var minScore = 1.00727638; //enough for one charge away
            peak = new ThrashV1Peak {
                Mz = 0.0
            };

            try
            {
                var numberPeaks = PeakData.PeakTops.Count;
                for (var peakCount = 0; peakCount < numberPeaks; peakCount++)
                {
                    var thisPeak = PeakData.PeakTops[peakCount];
                    var score = Math.Pow(peakMz - thisPeak.Mz, 2);
                    if (score < minScore)
                    {
                        minScore = score;
                        peak = new ThrashV1Peak(thisPeak);
                    }
                }
            }
            catch (Exception)
            {
                peak.Mz = 0.0;
                peak.Intensity = 0.0;
#if DEBUG
                throw;
#endif
            }

            return peak.Mz;
        }

        /// <summary>
        ///     Function discovers peaks in the m/z and intensity vectors supplied.
        /// </summary>
        /// <param name="mzList">is the pointer to List of m/z values</param>
        /// <param name="intensityList">is the pointer to List of intensity values</param>
        /// <returns>returns the number of peaks that were found in the vectors.</returns>
        /// <remarks>
        ///     The function uses <see cref="Engine.PeakProcessing.PeakStatistician.FindFwhm" />, and
        ///     <see cref="Engine.PeakProcessing.PeakStatistician.FindSignalToNoise" /> functions
        ///     to discover the full width at half maximum and signal to noise values for a peak. The signal to noise of a
        ///     peak is tested against the threshold value before its accepted as a peak. All peaks are used during the process,
        ///     but once generated only those which are above <see cref="_peakIntensityThreshold" /> are tested for peptidicity by
        ///     Deconvolution.HornMassTransform
        /// </remarks>
        public int DiscoverPeaks(List<double> mzList, List<double> intensityList)
        {
            if (mzList.Count == 0)
                return 0;
            var minMz = mzList[0];
            var maxMz = mzList[mzList.Count - 1];
            return DiscoverPeaks(mzList, intensityList, minMz, maxMz);
        }

        /// <summary>
        ///     clears the PeakData member variable <see cref="PeakProcessor.PeakData" />
        /// </summary>
        public void Clear()
        {
            PeakData.Clear();
        }

        /// <summary>
        ///     Removes peaks from unprocessed list that do not have any neighbour peaks within the specified tolerance window.
        /// </summary>
        /// <param name="tolerance">the tolerance in looking for neighbouring peaks.</param>
        public void FilterPeakList(double tolerance)
        {
            PeakData.FilterList(tolerance);
        }

        /// <summary>
        ///     Gets the FWHM for a point.
        /// </summary>
        /// <param name="mzList">is List of m/z values.</param>
        /// <param name="intensityList">is List of intensity values.</param>
        /// <param name="dataIndex">is the index of the point at which we want to find FWHM and SN.</param>
        /// <param name="signalToNoiseThreshold">is the threshold signal to noise.</param>
        /// <returns>returns the m/z value of the peak.</returns>
        public double GetFWHM(List<double> mzList, List<double> intensityList, int dataIndex,
            double signalToNoiseThreshold)
        {
            return PeakStatistician.FindFwhm(mzList, intensityList, dataIndex, signalToNoiseThreshold);
        }

        /// <summary>
        ///     Gets the FWHM for a point.
        /// </summary>
        /// <param name="mzList">is List of m/z values.</param>
        /// <param name="intensityList">is List of intensity values.</param>
        /// <param name="peak">is the m/z value at which we want to find FWHM.</param>
        /// <returns>returns the m/z value of the peak.</returns>
        public double GetFWHM(List<double> mzList, List<double> intensityList, double peak)
        {
            var index = PeakIndex.GetNearest(mzList, peak, 0);
            return PeakStatistician.FindFwhm(mzList, intensityList, index);
        }

        /// <summary>
        ///     Gets the signal to noise for a point.
        /// </summary>
        /// <param name="intensityList">is List of intensity values.</param>
        /// <param name="dataIndex">is the index of the point at which we want to find SN.</param>
        /// <returns>returns the signal to noise value of the peak.</returns>
        public double GetSignalToNoise(List<double> intensityList, int dataIndex)
        {
            return PeakStatistician.FindSignalToNoise(intensityList[dataIndex], intensityList, dataIndex);
        }

        /// <summary>
        ///     Gets the signal to noise for a point.
        /// </summary>
        /// <param name="mzList">is List of m/z values.</param>
        /// <param name="intensityList">is List of intensity values.</param>
        /// <param name="peak">is the m/z value at which we want to find the signal to noise ratio.</param>
        /// <returns>returns the signal to noise value of the peak.</returns>
        public double GetSignalToNoise(List<double> mzList, List<double> intensityList, double peak)
        {
            var index = PeakIndex.GetNearest(mzList, peak, 0);
            return PeakStatistician.FindSignalToNoise(intensityList[index], intensityList, index);
        }
    }
}