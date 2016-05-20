using System;
using System.Collections.Generic;
using System.Linq;

namespace DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.PeakProcessing
{
    /// <summary>
    ///     Used to store and retreive information about peaks that were found in the raw data.
    /// </summary>
    /// <remarks>
    ///     This class stores all the information about peaks in the data. Additionally it is used in processing extensively.
    ///     Because this information
    ///     needs to be accessed with high degree of efficiency we keep different maps.
    ///     The order of processing is as follows.
    ///     -# PeakProcessor discovers all the peaks in the data using <see cref="PeakProcessor.DiscoverPeaks" /> and sets its
    ///     member variable <see cref="PeakProcessor.PeakData" />.
    ///     -# PeakData keeps a List of these peaks in <see cref="PeakTops" />.
    ///     -# PeakData keeps a map of unprocessed peaks in <see cref="_peakMzToIndexDict" /> sorted by intensity. This allows
    ///     us to pick up the index of the next most intense unprocessed
    ///     peak in <see cref="PeakTops" />.
    ///     -# PeakData keeps a map of all peaks in <see cref="_allPeakMzToIndexDict" />. This map keeps a sorted tree in terms
    ///     of intensity of peaks for all the peaks in the List
    ///     <see cref="PeakTops" />.
    /// </remarks>
    public class PeakData
    {
        /// <summary>
        ///     multimap of indices of all peaks in <see cref="PeakTops" /> sorted in ascending m/z. This helps fast retrieval when
        ///     looking for a peak (not only unprocessed ones) around an approximate m/z.
        /// </summary>
        private readonly SortedDictionary<double, int> _allPeakMzToIndexDict = new SortedDictionary<double, int>();

        /// <summary>
        ///     multimap of indices of unprocessed peaks in <see cref="PeakTops" /> sorted in descending intensity. This helps fast
        ///     retrieval of the next most intense unprocessed peak.
        /// </summary>
        /// <remarks>
        ///     While the intensity of the peaks might actually be double, for the map, we only used the integral values.
        /// </remarks>
        private readonly SortedDictionary<int, List<int>> _peakIntensityToIndexDict =
            new SortedDictionary<int, List<int>>(new ReverseSorter<int>());

        /// <summary>
        ///     multimap of indices of unprocessed peaks in <see cref="PeakTops" /> sorted in ascending m/z. This helps fast
        ///     retrieval when looking for an unprocessed peak around an approximate m/z.
        /// </summary>
        private readonly SortedDictionary<double, int> _peakMzToIndexDict = new SortedDictionary<double, int>();

        /// <summary>
        ///     List of intensities in the raw data.
        /// </summary>
        public List<double> IntensityList = new List<double>();

        /// <summary>
        ///     List of mz's in the raw data
        /// </summary>
        public List<double> MzList = new List<double>();

        /// <summary>
        ///     List of peaks found in the data. It is recommended that this object not be touched by calling functions.
        /// </summary>
        public List<ThrashV1Peak> PeakTops = new List<ThrashV1Peak>();

        // Default constructor
        public PeakData()
        {
        }

        // copy constructor.
        public PeakData(PeakData a)
        {
            IntensityList = new List<double>(a.IntensityList);
            MzList = new List<double>(a.MzList);
            //PeakTops = new List<Peak>(a.PeakTops);
            //force copy by value....
            PeakTops = new List<ThrashV1Peak>(a.PeakTops.Capacity);
            PeakTops.AddRange(a.PeakTops.Select(x => new ThrashV1Peak(x)));
        }

        /// <summary>
        ///     Gets the index^th peak in <see cref="PeakTops" />.
        /// </summary>
        /// <param name="index">is the index of the peak in <see cref="PeakTops" />.</param>
        /// <returns>the peak which is at the index^th position in <see cref="PeakTops" />.</returns>
        public ThrashV1Peak GetPeak(int index)
        {
            return new ThrashV1Peak(PeakTops[index]);
        }

        /// <summary>
        ///     Adds a peak to <see cref="PeakTops" />
        /// </summary>
        /// <param name="peak">is the peak that we want to add to <see cref="PeakTops" />.</param>
        public void AddPeak(ThrashV1Peak peak)
        {
            PeakTops.Add(new ThrashV1Peak(peak));
        }

        public void SetPeaks(ThrashV1Peak[] peaks)
        {
            foreach (var peak in peaks)
            {
                AddPeak(peak);
            }
            InitializeUnprocessedPeakData();
        }

        /// <summary>
        ///     Adds a peak to the processing list.
        /// </summary>
        /// <param name="pk">is the peak that we want to add to our processing list.</param>
        /// <remarks>
        ///     The processing list is really the set of
        ///     peaks that are unprocessed and the way these are tracked, are by puttting these indices in the processing maps
        ///     <see cref="_peakIntensityToIndexDict" /> and <see cref="_peakMzToIndexDict" />
        /// </remarks>
        public void AddPeakToProcessingList(ThrashV1Peak pk)
        {
            // The assumption is that this peak already exists in the List.
            // The peak was removed from the processing list, so we're going to add it in.
            // The map for all peaks is unaffected so we won't add to it.
            // Also the intensity is set to 0 when deletion happens. So lets copy
            // the peak back into our peak vector.
            var peakIndex = pk.PeakIndex;
            var mz = pk.Mz;
            var intensity = (int) pk.Intensity;
            PeakTops[peakIndex] = new ThrashV1Peak(pk);
            _peakMzToIndexDict.Add(mz, peakIndex);
            //mmap_pk_intensity_index.insert(std.pair<int,int> (intensity, peak_index));
            if (_peakIntensityToIndexDict.ContainsKey(intensity))
            {
                _peakIntensityToIndexDict[intensity].Add(peakIndex);
            }
            else
            {
                _peakIntensityToIndexDict.Add(intensity, new List<int> {peakIndex});
            }
        }

        /// <summary>
        ///     Clears auxillary data structures and initializes them at the start of processing.
        /// </summary>
        /// <remarks>
        ///     We track which peaks are unprocessed through the following variables:
        ///     - <see cref="_peakIntensityToIndexDict" /> is the map of unprocessed peaks sorted in decreasing intensity, allowing
        ///     for a quick binary search for the next most intense peak.
        ///     - <see cref="_peakMzToIndexDict" /> is the map of the same unprocessed peaks, but sorted by m/z for m/z searches.
        ///     At the start of the deconvolution, these variable are filled in with pairs of (peak intensity, peak index) and
        ///     (peak mz, peak index) for peaks which will be processed.
        ///     As deconvolution processes, these elements are taken off the unprocessed maps.
        ///     The current function is called to set these variables.
        /// </remarks>
        public void InitializeUnprocessedPeakData()
        {
            _peakMzToIndexDict.Clear();
            _allPeakMzToIndexDict.Clear();
            _peakIntensityToIndexDict.Clear();

            for (var i = 0; i < PeakTops.Count; i++)
            {
                var mz = PeakTops[i].Mz;
                var intensity = (int) PeakTops[i].Intensity;
                PeakTops[i].PeakIndex = i;
                // some peaks might not need to be added to the list because they were already removed.
                // check the intensity to see if this might be the case
                if (intensity != -1)
                {
                    _peakMzToIndexDict.Add(mz, i);
                    //mmap_pk_intensity_index.insert(std.pair<int, int>(intensity, i));
                    if (_peakIntensityToIndexDict.ContainsKey(intensity))
                    {
                        _peakIntensityToIndexDict[intensity].Add(i);
                    }
                    else
                    {
                        _peakIntensityToIndexDict.Add(intensity, new List<int> {i});
                    }
                }
                _allPeakMzToIndexDict.Add(mz, i);
            }
        }

        /// <summary>
        ///     Sort peak list <see cref="PeakTops" /> in order of decreasing intensity.
        /// </summary>
        public void SortPeaksByIntensity()
        {
            // Sort, then call Reverse() to get sorted by descending
            PeakTops.Sort();
            PeakTops.Reverse();
            for (var i = 0; i < PeakTops.Count; i++)
            {
                PeakTops[i].PeakIndex = i;
            }
        }

        /// <summary>
        ///     Get the most intense unprocessed peak in the given m/z range and remove it from the processing list.
        /// </summary>
        /// <param name="startMz">minimum m/z of the peak.</param>
        /// <param name="stopMz">maximum m/z of the peak.</param>
        /// <param name="peak">is assigned the most intense peak with m/z between the startMz and stopMz.</param>
        /// <returns>returns true if a peak was found and false if none was found.</returns>
        /// <remarks>
        ///     The peak that is returned by this function is removed from the processing list. This is essentially the
        ///     function that is called repeatedly in the deconvolution process which deisotopes peaks in order of decreasing
        ///     intensity.
        /// </remarks>
        public bool GetNextPeak(double startMz, double stopMz, out ThrashV1Peak peak)
        {
            peak = new ThrashV1Peak();
            peak.Mz = -1;
            peak.Intensity = -1;

            var found = false;
            foreach (var indexList in _peakIntensityToIndexDict)
            {
                foreach (var peakIndex in indexList.Value)
                {
                    var mz = PeakTops[peakIndex].Mz;
                    if (mz > startMz && mz <= stopMz)
                    {
                        peak = new ThrashV1Peak(PeakTops[peakIndex]);
                        found = true;
                        break;
                    }
                }
                if (found)
                {
                    break;
                }
            }
            if (found)
            {
                RemovePeak(peak);
            }
            return found;
        }

        /// <summary>
        ///     Removes the peak from the unprocessed list.
        /// </summary>
        /// <param name="peak">is the peak we want to remove from the unprocessed peaks.</param>
        /// <remarks>
        ///     In order to remove the peak from the processing "list", we clear the indices of the peak from the unprocessed
        ///     maps <see cref="_peakMzToIndexDict" /> and <see cref="_peakIntensityToIndexDict" />
        /// </remarks>
        public void RemovePeak(ThrashV1Peak peak)
        {
            if (peak == null)
            {
                return;
            }
            var found = false;
            if (_peakIntensityToIndexDict.ContainsKey((int) peak.Intensity))
            {
                var indexList = _peakIntensityToIndexDict[(int) peak.Intensity];
                found = indexList.Remove(peak.PeakIndex);
                if (indexList.Count == 0)
                {
                    _peakIntensityToIndexDict.Remove((int) peak.Intensity);
                }
            }
            if (!found)
                return;

            found = false;
            if (_peakMzToIndexDict.ContainsKey(peak.Mz))
            {
                if (_peakMzToIndexDict[peak.Mz] == peak.PeakIndex)
                {
                    _peakMzToIndexDict.Remove(peak.Mz);
                    found = true;
                }
            }
            else
            {
                var items = _peakMzToIndexDict.Where(x => x.Value == peak.PeakIndex).ToList();
                foreach (var item in items)
                {
                    if (peak.Mz.Equals(item.Key))
                    {
                        _peakMzToIndexDict.Remove(item.Key);
                        found = true;
                    }
                }
            }
            if (!found)
            {
                // so how did this happen ?
                return;
            }

            PeakTops[peak.PeakIndex].Intensity = -1;
        }

        /// <summary>
        ///     Removes all the peaks in the supplied m/z range from the unprocessed list.
        /// </summary>
        /// <param name="startMz">is the minimum m/z.</param>
        /// <param name="stopMz">is the maximum m/z.</param>
        /// <param name="debug">set to true if you want to print debug information from this function.</param>
        /// <remarks>
        ///     In order to remove the peaks from the processing "list", we clear the indices of the peaks from the
        ///     unprocessed maps <see cref="_peakMzToIndexDict" /> and <see cref="_peakIntensityToIndexDict" />
        /// </remarks>
        public void RemovePeaks(double startMz, double stopMz, bool debug = false)
        {
            // force to list to allow removal of items from the original dictionary
            foreach (var item in _peakMzToIndexDict.Where(x => x.Key >= startMz).ToList())
            {
                var mz = item.Key;

                if (mz > stopMz)
                    return;

                var peakIndex = item.Value;

                if (mz < startMz)
                    continue;
                if (debug)
                    Console.Error.WriteLine("\tRemoving peak" + PeakTops[peakIndex].Mz);
                RemovePeak(PeakTops[peakIndex]);
            }
        }

        /// <summary>
        ///     Removes all the peaks whose m/zs are almost equal to the m/zs supplied in the List peak_mzs with a supplied m/z
        ///     tolerance.
        /// </summary>
        /// <param name="peakMzs">is the List of m/z values we want to remove.</param>
        /// <param name="mzTolerance">used to search for the above peaks in <see cref="MzList" /></param>
        /// <param name="debug">set to true if you want to print debug information from this function.</param>
        /// <remarks>
        ///     In order to remove the peaks from the processing "list", we clear the indices of the peaks from the unprocessed
        ///     maps <see cref="_peakMzToIndexDict" /> and <see cref="_peakIntensityToIndexDict" />
        ///     This function can be used to remove calibration peaks from the list.
        /// </remarks>
        public void RemovePeaks(List<double> peakMzs, double mzTolerance, bool debug = false)
        {
            // in order to remove background peaks and calibration peaks use this function.
            foreach (var peakMz in peakMzs)
            {
                RemovePeaks(peakMz - mzTolerance, peakMz + mzTolerance, debug);
            }
        }

        /// <summary>
        ///     Gets the peak in <see cref="PeakTops" /> whose m/z is exactly equal to mz.
        /// </summary>
        /// <param name="mz">m/z of the peak we are looking for.</param>
        /// <param name="peak">the peak whose m/z equals input parameter.</param>
        /// <returns>true is the peak was found; false otherwise</returns>
        public bool GetPeak(double mz, out ThrashV1Peak peak)
        {
            peak = new ThrashV1Peak();
            if (_peakMzToIndexDict.ContainsKey(mz))
            {
                var peakIndex = _peakMzToIndexDict[mz];
                peak = new ThrashV1Peak(PeakTops[peakIndex]);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Get the most intense unprocessed peak in the given m/z range.
        /// </summary>
        /// <param name="startMz">minimum m/z of the peak.</param>
        /// <param name="stopMz">maximum m/z of the peak.</param>
        /// <param name="peak">is assigned the most intense Peak with m/z between the startMz and stopMz.</param>
        /// <returns>returns true if a peak was found and false if none was found.</returns>
        public bool GetPeak(double startMz, double stopMz, out ThrashV1Peak peak)
        {
            peak = new ThrashV1Peak();
            peak.Intensity = -10;
            var found = false;

            foreach (var item in _peakMzToIndexDict.Where(x => x.Key >= startMz))
            {
                var peakIndex = item.Value;
                var mzVal = item.Key;
                if (mzVal > stopMz)
                    return found;
                if (PeakTops[peakIndex].Intensity > peak.Intensity && mzVal >= startMz)
                {
                    peak = new ThrashV1Peak(PeakTops[peakIndex]);
                    found = true;
                }
            }
            return found;
        }

        /// <summary>
        ///     Gets the most intense peak(whether or not it is processed) in the m/z range (mz - width to mz + width).
        /// </summary>
        /// <param name="startMz">minimum m/z of the Peak.</param>
        /// <param name="stopMz">mimum m/z of the Peak.</param>
        /// <param name="peak">is set to the peak that was found.</param>
        /// <returns>returns true if a peak was found in the window (mz - width to mz + width) and false if not found.</returns>
        /// <remarks>The returned peak can have an intensity of 0 because it was already processed and removed.</remarks>
        public bool GetPeakFromAll(double startMz, double stopMz, out ThrashV1Peak peak)
        {
            peak = new ThrashV1Peak();
            peak.Intensity = -10;
            var found = false;

            foreach (var item in _allPeakMzToIndexDict.Where(x => x.Key >= startMz))
            {
                var peakIndex = item.Value;
                var mzVal = item.Key;
                if (mzVal > stopMz)
                    return found;
                if (PeakTops[peakIndex].Intensity >= peak.Intensity && mzVal >= startMz)
                {
                    //double thisMz = PeakTops[peakIndex].Mz;
                    peak = new ThrashV1Peak(PeakTops[peakIndex]);
                    found = true;
                }
            }
            return found;
        }

        /// <summary>
        ///     Gets the most intense peak(whether or not it is processed) in the m/z range (mz - width to mz + width).
        /// </summary>
        /// <param name="startMz">minimum m/z of the Peak.</param>
        /// <param name="stopMz">mimum m/z of the Peak.</param>
        /// <param name="peak">is set to the peak that was found.</param>
        /// <param name="excludeMass">is the mass we need to exclude in this search.</param>
        /// <returns>returns true if a peak was found in the window (mz - width to mz + width) and false if not found.</returns>
        /// <remarks>The returned peak can have an intensity of 0 because it was already processed and removed.</remarks>
        public bool GetPeakFromAll(double startMz, double stopMz, out ThrashV1Peak peak, double excludeMass)
        {
            peak = new ThrashV1Peak();
            peak.Intensity = -10;
            var found = false;

            foreach (var item in _allPeakMzToIndexDict.Where(x => x.Key >= startMz))
            {
                var peakIndex = item.Value;
                var mzVal = item.Key;
                if (mzVal.Equals(excludeMass))
                    continue;
                if (mzVal > stopMz)
                    return found;
                if (PeakTops[peakIndex].Intensity >= peak.Intensity && mzVal >= startMz)
                {
                    //double thisMz = PeakTops[peakIndex].Mz;
                    peak = new ThrashV1Peak(PeakTops[peakIndex]);
                    found = true;
                }
            }
            return found;
        }

        /// <summary>
        ///     Gets the most intense peak(whether or not it is processed) in the m/z range (mz - width to mz + width). The
        ///     intensity returned is the intensity in the original raw data in <see cref="IntensityList" />
        /// </summary>
        /// <param name="startMz">minimum m/z of the Peak.</param>
        /// <param name="stopMz">mimum m/z of the Peak.</param>
        /// <param name="peak">is set to the peak that was found.</param>
        /// <param name="excludeMass">is the mass we need to exclude in this search.</param>
        /// <returns>returns true if a peak was found in the window (mz - width to mz + width) and false if not found.</returns>
        /// <remarks>The returned peak has the intensity in the original raw data in <see cref="IntensityList" /></remarks>
        public bool GetPeakFromAllOriginalIntensity(double startMz, double stopMz, out ThrashV1Peak peak, double excludeMass)
        {
            peak = new ThrashV1Peak();
            peak.Intensity = -10;
            var found = false;

            foreach (var item in _allPeakMzToIndexDict.Where(x => x.Key >= startMz))
            {
                var peakIndex = item.Value;
                var mzVal = item.Key;
                if (mzVal.Equals(excludeMass))
                    continue;
                if (mzVal > stopMz)
                    return found;
                var dataIndex = PeakTops[peakIndex].DataIndex;
                if (IntensityList[dataIndex] >= peak.Intensity && mzVal >= startMz)
                {
                    //double thisMz = PeakTops[peakIndex].Mz;
                    peak = new ThrashV1Peak(PeakTops[peakIndex]);
                    found = true;
                }
            }
            return found;
        }

        /// <summary>
        ///     Gets the most intense peak(whether or not it is processed) in the m/z range (mz - width to mz + width). The
        ///     intensity returned is the intensity in the original raw data in <see cref="IntensityList" />
        /// </summary>
        /// <param name="startMz">minimum m/z of the Peak.</param>
        /// <param name="stopMz">maximum m/z of the Peak.</param>
        /// <param name="peak">is set to the peak that was found.</param>
        /// <returns>returns true if a peak was found in the window (mz - width to mz + width) and false if not found.</returns>
        /// <remarks>The returned peak has the intensity in the original raw data in <see cref="IntensityList" />.</remarks>
        public bool GetPeakFromAllOriginalIntensity(double startMz, double stopMz, out ThrashV1Peak peak)
        {
            peak = new ThrashV1Peak();
            peak.Intensity = -10;
            var found = false;

            foreach (var item in _allPeakMzToIndexDict.Where(x => x.Key <= stopMz).Reverse())
            {
                var peakIndex = item.Value;
                var mzVal = item.Key;
                if (mzVal < startMz)
                    return found;
                if (PeakTops[peakIndex].Intensity > peak.Intensity && mzVal >= startMz &&
                    mzVal <= stopMz)
                {
                    //double thisMz = PeakTops[peakIndex].Mz;
                    peak = new ThrashV1Peak(PeakTops[peakIndex]);
                    found = true;
                }
            }
            return found;
        }

        /// <summary>
        ///     Gets the unprocessed peak whose m/z is closest to supplied mz in the m/z range (mz - width to mz + width).
        /// </summary>
        /// <param name="mz">the center m\z around which we want to look for a Peak.</param>
        /// <param name="width">the width of the m\z window in which we want to look for the peak.</param>
        /// <param name="peak">is the peak closest to m/z.</param>
        /// <returns>returns true if a peak was found in the window (mz - width to mz + width) and false if not found.</returns>
        public bool GetClosestPeak(double mz, double width, out ThrashV1Peak peak)
        {
            peak = new ThrashV1Peak();
            var found = false;

            var startMz = mz - width;
            var stopMz = mz + width;

            foreach (var item in _peakMzToIndexDict.Where(x => x.Key >= startMz))
            {
                var peakIndex = item.Value;
                var mzVal = item.Key;
                if (mzVal > stopMz)
                    return found;
                if (mzVal >= startMz && Math.Abs(mzVal - mz) < Math.Abs(peak.Mz - mz))
                {
                    peak = new ThrashV1Peak(PeakTops[peakIndex]);
                    found = true;
                }
            }
            return found;
        }

        /// <summary>
        ///     Gets the peak (whether or not it is processed) whose m/z is closest to supplied mz in the m/z range (mz - width to
        ///     mz + width).
        /// </summary>
        /// <param name="mz">the center m\z around which we want to look for a Peak.</param>
        /// <param name="width">the width of the m\z window in which we want to look for the peak.</param>
        /// <param name="peak">is the peak closest to m/z.</param>
        /// <returns>returns true if a peak was found in the window (mz - width to mz + width) and false if not found.</returns>
        /// <remarks>The returned peak can have an intensity of 0 because it was already processed and removed.</remarks>
        public bool GetClosestPeakFromAll(double mz, double width, out ThrashV1Peak peak)
        {
            peak = new ThrashV1Peak();
            var found = false;

            var startMz = mz - width;
            var stopMz = mz + width;

            foreach (var item in _allPeakMzToIndexDict.Where(x => x.Key >= startMz))
            {
                var peakIndex = item.Value;
                var mzVal = item.Key;
                if (mzVal > stopMz)
                    return found;
                if (mzVal >= startMz && Math.Abs(mzVal - mz) < Math.Abs(peak.Mz - mz))
                {
                    peak = new ThrashV1Peak(PeakTops[peakIndex]);
                    found = true;
                }
            }
            return found;
        }

        /// <summary>
        ///     Clears all the data structures in the PeakData instance.
        /// </summary>
        /// <remarks>
        ///     It clears the following member variables of this instance:
        ///     - List of peak tops <see cref="PeakTops" />
        ///     - maps of unprocessed peaks:
        ///     -# map of mz to indices: <see cref="_peakMzToIndexDict" />
        ///     -# map of intensity to indices: <see cref="_peakIntensityToIndexDict" />
        ///     - map of all peaks <see cref="_allPeakMzToIndexDict" />
        ///     - It also sets to null
        ///     -# arg pointer to List of raw intensities <see cref="IntensityList" />
        ///     -# arg pointer to List of raw intensities <see cref="MzList" />
        /// </remarks>
        /// <remarks>This function does not clear or delete the lists of raw mzs and intensities. That is left to the caller.</remarks>
        public void Clear()
        {
            PeakTops.Clear();
            IntensityList = null;
            MzList = null;
            _peakIntensityToIndexDict.Clear();
            _peakMzToIndexDict.Clear();
            _allPeakMzToIndexDict.Clear();
        }

        /// <summary>
        ///     Returns number of peaks.
        /// </summary>
        /// <returns>number of peaks found.</returns>
        public int GetNumPeaks()
        {
            return PeakTops.Count;
        }

        /// <summary>
        ///     Filters the peak list and removes peaks that do not have any neighbouring peaks in the specified window.
        /// </summary>
        /// <param name="tolerance">is the window around a peak in which we look for neighbors.</param>
        public void FilterList(double tolerance)
        {
            // peaks are supposed to be sorted in mz, but
            // we'll just use the maps for this here.

            // list of temporary peaks that are used during the processing.
            var tempPeakTops = new List<ThrashV1Peak>();
            InitializeUnprocessedPeakData();

            foreach (var peakTop in PeakTops)
            {
                ThrashV1Peak nextPeak;
                // look for a peak behind.
                var mz = peakTop.Mz;
                var inback = GetPeak(mz - tolerance, mz - 0.00001, out nextPeak);
                var infront = GetPeak(mz + 0.00001, mz + tolerance, out nextPeak);

                if (inback || infront)
                    tempPeakTops.Add(peakTop);
            }
            PeakTops.Clear();
            PeakTops.AddRange(tempPeakTops);
        }

        /// <summary>
        ///     Gets a copy of the pointers to vectors of mzs, intensities from the raw data.
        /// </summary>
        /// <param name="mzs">pointer to pointer to mzs. *mzs = <see cref="MzList" /></param>
        /// <param name="intensities">pointer to pointer to intensities. *intensities = <see cref="IntensityList" /></param>
        public void GetMzIntVectors(out List<double> mzs, out List<double> intensities)
        {
            mzs = MzList;
            intensities = IntensityList;
        }

        /// <summary>
        ///     Gets number of unprocessed peaks remaining.
        /// </summary>
        /// <returns>returns the number of unprocessed peaks. This is the same as <see cref="_peakMzToIndexDict" />.Count</returns>
        public int GetNumUnprocessedPeaks()
        {
            return _peakMzToIndexDict.Count;
        }

        /// <summary>
        ///     Finds the highest peak from the raw data vectors withing the specified m/z range (reduced from original FindPeak
        /// </summary>
        /// <param name="startMz">minimum m\z at which to look for the peak</param>
        /// <param name="stopMz">maximum m\z at which to look for the peak.</param>
        /// <param name="peak"> instance whose mz and intensity are set to the peak that is found.</param>
        /// <remarks>The function only sets the mz, intensity of the peak, not the other members (SN, FWHM etc).</remarks>
        public void FindPeakAbsolute(double startMz, double stopMz, out ThrashV1Peak peak)
        {
            // Anoop : modified from original FindPEak so as to return peaks only
            // and not shoulders, eliminates all the +ve Da DelM regions
            peak = new ThrashV1Peak();
            peak.Mz = -1;
            peak.Intensity = 0;
            var width = (stopMz - startMz) / 2;
            var foundExistingPeak = GetClosestPeak(startMz + width, width, out peak);
            if (foundExistingPeak)
            {
                // peak already exists. Send it back.
            }
            else
            {
                // peak doesn't exist. Lets find a starting index to start looking at.
                // perhaps there was a peak there.
                var foundPeak = GetClosestPeakFromAll(startMz + width, width, out peak);
                var numPts = MzList.Count;
                if (foundPeak)
                {
                    var index = peak.DataIndex;
                    while (index > 0 && MzList[index] >= startMz)
                    {
                        var intensity = IntensityList[index];
                        var mz = MzList[index];
                        if (intensity > peak.Intensity && mz <= stopMz)
                        {
                            peak.Mz = mz;
                            peak.Intensity = intensity;
                            peak.DataIndex = index;
                        }
                        index--;
                    }
                    index = peak.DataIndex;
                    while (index < numPts && MzList[index] <= stopMz)
                    {
                        var intensity = IntensityList[index];
                        if (intensity > peak.Intensity)
                        {
                            var mz = MzList[index];
                            peak.Mz = mz;
                            peak.Intensity = intensity;
                            peak.DataIndex = index;
                        }
                        index++;
                    }
                    if (peak.Intensity <= 0)
                        peak.Mz = 0;
                }
            }
        }

        /// <summary>
        ///     Finds the highest peak from the raw data lists withing the specified m/z range.
        /// </summary>
        /// <param name="startMz">minimum m\z at which to look for the peak</param>
        /// <param name="stopMz">maximum m\z at which to look for the peak.</param>
        /// <param name="peak"> instance whose mz and intensity are set to the peak that is found.</param>
        /// <remarks>The function only sets the mz, intensity of the peak, not the other members (SN, FWHM etc).</remarks>
        public void FindPeak(double startMz, double stopMz, out ThrashV1Peak peak)
        {
            peak = new ThrashV1Peak();
            peak.Mz = -1;
            var width = (stopMz - startMz) / 2;
            var foundExistingPeak = GetClosestPeak(startMz + width, width, out peak);
            if (foundExistingPeak)
            {
                // peak already exists. Send it back.
            }
            else
            {
                // peak doesn't exist. Lets find a starting index to start looking at.
                // perhaps there was a peak there.
                var foundPeak = GetClosestPeakFromAll(startMz + width, width, out peak);
                var numPts = MzList.Count;
                if (foundPeak)
                {
                    var index = peak.DataIndex;
                    while (index > 0 && MzList[index] >= startMz)
                    {
                        var intensity = IntensityList[index];
                        var mz = MzList[index];
                        if (intensity > peak.Intensity && mz <= stopMz)
                        {
                            peak.Mz = mz;
                            peak.Intensity = intensity;
                            peak.DataIndex = index;
                        }
                        index--;
                    }
                    index = peak.DataIndex;
                    while (index < numPts && MzList[index] <= stopMz)
                    {
                        var intensity = IntensityList[index];
                        if (intensity > peak.Intensity)
                        {
                            var mz = MzList[index];
                            peak.Mz = mz;
                            peak.Intensity = intensity;
                            peak.DataIndex = index;
                        }
                        index++;
                    }
                    if (peak.Intensity <= 0)
                        peak.Mz = 0;
                }
                else
                {
                    var startIndex = PeakIndex.GetNearestBinary(MzList, startMz, 0, numPts - 1);

                    if (startIndex > numPts - 1)
                        startIndex = numPts - 1;
                    if (startIndex < 0)
                        startIndex = 0;

                    if (MzList[startIndex] > startMz)
                    {
                        while (startIndex > 0 && MzList[startIndex] > startMz)
                            startIndex--;
                    }
                    else
                    {
                        while (startIndex < numPts && MzList[startIndex] < startMz)
                            startIndex++;
                        startIndex--;
                    }

                    for (var i = startIndex; i < numPts; i++)
                    {
                        var mz = MzList[i];
                        var intensity = IntensityList[i];
                        if (mz > stopMz)
                            break;
                        if (intensity > peak.Intensity)
                        {
                            peak.Mz = mz;
                            peak.Intensity = intensity;
                            peak.DataIndex = i;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Sorter class for <see cref="_peakIntensityToIndexDict" />
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class ReverseSorter<T> : IComparer<T> where T : IComparable<T>
        {
            public int Compare(T x, T y)
            {
                return y.CompareTo(x);
            }
        }
    }
}