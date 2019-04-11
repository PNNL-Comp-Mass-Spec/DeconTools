using System;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.PeakProcessing
{
    public class ThrashV1Peak : MSPeak, IComparable, IComparable<ThrashV1Peak>
    {
        private double _fwhm;
        private double _intensity;
        private double _signalToNoise;

        // <summary>
        // index in mzs, intensity vectors that were used to create the peaks in
        // <see cref="PeakProcessor.DiscoverPeaks" />.
        // </summary>
        //public int DataIndex;

        /// <summary>
        ///     Full width at half maximum for peak.
        /// </summary>
        public double FWHM
        {
            get => _fwhm;
            set
            {
                _fwhm = value;
                Width = (float)value;
            }
        }

        /// <summary>
        ///     intensity of peak.
        /// </summary>
        public double Intensity
        {
            get => _intensity;
            set
            {
                _intensity = value;
                Height = (float)value;
            }
        }

        /// <summary>
        ///     mz of the peak.
        /// </summary>
        public double Mz
        {
            get => XValue;
            set => XValue = value;
        }

        /// <summary>
        ///     index in <see cref="PeakData.PeakTops" /> List.
        /// </summary>
        public int PeakIndex;

        /// <summary>
        ///     Signal to noise ratio
        /// </summary>
        public double SignalToNoiseDbl
        {
            get => _signalToNoise;
            set
            {
                _signalToNoise = value;
                SignalToNoise = (float)value;
            }
        }

        public ThrashV1Peak(double mz, float intensity = 0) : base(mz, intensity)
        {
            Mz = mz;
            Intensity = intensity;
            SignalToNoiseDbl = 0;
            PeakIndex = -1;
            DataIndex = -1;
            FWHM = 0;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="pk"></param>
        public ThrashV1Peak(ThrashV1Peak pk) : base(pk.Mz, (float)pk.Intensity)
        {
            FWHM = pk.FWHM;
            Intensity = pk.Intensity;
            SignalToNoiseDbl = pk.SignalToNoiseDbl;
            DataIndex = pk.DataIndex;
            PeakIndex = pk.PeakIndex;
        }

        /// <summary>
        ///     Copy constructor
        /// </summary>
        /// <param name="pk"></param>
        public ThrashV1Peak(MSPeak pk) : base(pk.XValue, pk.Height)
        {
            FWHM = pk.Width;
            Intensity = pk.Height;
            SignalToNoiseDbl = pk.SignalToNoise;
            DataIndex = pk.DataIndex;
            PeakIndex = -1;
        }

        /// <summary>
        ///     Sets the members of the ThrashV1Peak.
        /// </summary>
        /// <param name="mz">m/z of the peak.</param>
        /// <param name="intensity">intensity of the peak</param>
        /// <param name="signalToNoise">signal2noise of the peak look at PeakProcessor.PeakStatistician.FindSignalToNoise</param>
        /// <param name="peakIndex">
        ///     index of the peak in PeakData.mvect_peak_tops List of the PeakData instance that was used to
        ///     generate these peaks.
        /// </param>
        /// <param name="dataIndex">
        ///     index of the peak top in the mz, intensity vectors that are the raw data input into
        ///     PeakData.DiscoverPeaks
        /// </param>
        /// <param name="fwhm">
        ///     full width half max of the peak. For details about how this is calculated look at
        ///     PeakProcessor.PeakStatistician.FindFWHM.
        /// </param>
        public ThrashV1Peak(double mz, double intensity, double signalToNoise, int peakIndex, int dataIndex, double fwhm) : base(mz, (float)intensity)
        {
            Intensity = intensity;
            SignalToNoiseDbl = signalToNoise;
            PeakIndex = peakIndex;
            DataIndex = dataIndex;
            FWHM = fwhm;
        }

        /// <summary>
        ///     Compare 2 peaks, for sorting by intensity. Follow Sort() call with a Reverse() call to get sorted by descending
        ///     intensity
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <remarks>
        ///     Used by the sort algorithms to sort List of peaks in descending order of mdbl_intensity.
        ///     Function used to sort peaks in a descending order.
        /// </remarks>
        public int CompareTo(object obj)
        {
            if (!(obj is ThrashV1Peak other))
            {
                throw new NotImplementedException();
            }
            return CompareTo(other);
        }

        /// <summary>
        ///     Compare 2 peaks, for sorting by intensity. Follow Sort() call with a Reverse() call to get sorted by descending
        ///     intensity
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <remarks>
        ///     Used by the sort algorithms to sort List of peaks in descending order of mdbl_intensity.
        ///     Function used to sort peaks in a descending order.
        /// </remarks>
        public int CompareTo(ThrashV1Peak obj)
        {
            var result = Intensity.CompareTo(obj.Intensity);
            if (result == 0)
            {
                result = Mz.CompareTo(obj.Mz);
            }
            return result;
        }

        public override string ToString()
        {
            return Mz + " " + Intensity + " " + FWHM + " " + SignalToNoiseDbl + " " + DataIndex + " " +
                   PeakIndex + "\n";
        }

#if !Disable_Obsolete
        [Obsolete("Unused - use the CompareTo functions.", true)]
        public static bool PeakIntensityComparison(ThrashV1Peak pk1, ThrashV1Peak pk2)
        {
            if (pk1.Intensity > pk2.Intensity)
                return true;
            if (pk1.Intensity < pk2.Intensity)
                return false;
            return pk1.Mz > pk2.Mz;
        }
#endif
    }
}