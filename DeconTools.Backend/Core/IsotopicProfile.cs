using System;
using System.Collections.Generic;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class IsotopicProfile
    {

        public IsotopicProfile()
        {
            _peaklist = new List<MSPeak>();
        }
        
        private List<MSPeak> _peaklist;
        public List<MSPeak> Peaklist
        {
            get { return _peaklist; }
            set { _peaklist = value; }
        }

        /// <summary>
        /// Zero-based index value that points to which peak of the PeakList is the monoisotopic peak.  (it isn't always the first one)
        /// </summary>
        public int MonoIsotopicPeakIndex { get; set; }


        public bool IsSaturated { get; set; }


        private int _chargeState;
        public int ChargeState
        {
            get { return _chargeState; }
            set { _chargeState = value; }
        }


        private double _intensityAggregate;   //    a way of storing an overall intensity for the whole profile
        public double IntensityAggregate
        {
            get { return _intensityAggregate; }
            set { _intensityAggregate = value; }
        }

        /// <summary>
        /// The adjusted intensity of the isotopic profile. Currently used for correcting saturated profiles in IMS workflows
        /// </summary>
        public double IntensityAggregateAdjusted { get; set; }



        private double _originalIntensity;   // the unsummed intensity;  
        public double OriginalIntensity
        {
            get { return _originalIntensity; }
            set { _originalIntensity = value; }
        }

        //private double _originalTotalIsotopicAbundance;  // aka original_TIA;   used in IMS-TOF analysis.
        //public double OriginalTotalIsotopicAbundance
        //{
        //    get { return _originalTotalIsotopicAbundance; }
        //    set { _originalTotalIsotopicAbundance = value; }
        //}

        private double _score;
        public double Score
        {
            get { return _score; }
            set { _score = value; }
        }


        private double _monoIsotopicMass;
        public double MonoIsotopicMass
        {
            get { return _monoIsotopicMass; }
            set { _monoIsotopicMass = value; }
        }

        private double _mostAbundantIsotopeMass;
        public double MostAbundantIsotopeMass
        {
            get { return _mostAbundantIsotopeMass; }
            set { _mostAbundantIsotopeMass = value; }
        }

        private float _monoPlusTwoAbundance;
        public float MonoPlusTwoAbundance
        {
            get { return _monoPlusTwoAbundance; }
            set { _monoPlusTwoAbundance = value; }
        }

        private double _averageMass; 
        public double AverageMass
        {
            get { return _averageMass; }
            set { _averageMass = value; }
        }

        public int GetNumOfIsotopesInProfile()
        {
            return _peaklist.Count;
        }


        public double GetMZofMostAbundantPeak()
        {
            MSPeak mostIntensePeak = getMostIntensePeak();
            if (mostIntensePeak == null) return -1;
            return mostIntensePeak.XValue;
        }

        public double GetFWHM()
        {
            MSPeak mostIntensePeak = getMostIntensePeak();
            if (mostIntensePeak == null)
            {
                return -1;
            }
            return mostIntensePeak.Width;
        }


        public int GetIndexOfMostIntensePeak()
        {
            if (_peaklist == null || _peaklist.Count == 0) return -1;

            int indexOfMaxPeak = -1;
            float maxIntensity = 0;
            
            for (int i = 0; i < _peaklist.Count; i++)
            {
                if (_peaklist[i].Height > maxIntensity)
                {
                    maxIntensity = _peaklist[i].Height;
                    indexOfMaxPeak = i;
                }
            }
            return indexOfMaxPeak;

        }

        public MSPeak getMostIntensePeak()
        {
            if (_peaklist == null || _peaklist.Count == 0) return null;

            MSPeak maxPeak = new MSPeak();
            foreach (MSPeak peak in _peaklist)
            {
                if (peak.Height >= maxPeak.Height)
                {
                    maxPeak = peak;
                }

            }
            return maxPeak;

        }

        public double GetSignalToNoise()
        {
            MSPeak mostIntensePeak = getMostIntensePeak();
            if (mostIntensePeak == null)
            {
                return -1;
            } 
            return mostIntensePeak.SN;
        }

        public double GetMonoAbundance()
        {
            if (_peaklist == null||Peaklist.Count==0) return 0;
            return _peaklist[0].Height;
        }

        public float GetMonoPlusTwoAbundance()
        {
            if (_peaklist == null || _peaklist.Count < 3) return 0;
            return _peaklist[2].Height;
        }

        public double GetMZ()
        {
            if (_peaklist == null || Peaklist.Count==0) return -1;
            return _peaklist[0].XValue;
        }

        public double MonoPeakMZ { get; set; }

        public double GetAbundance()
        {
            //MSPeak mostIntensePeak = getMostIntensePeak(peaklist);
            //return mostIntensePeak.Intensity;

            return IntensityAggregate;
        }

        public double GetScore()
        {
            return _score;
        }

        public MSPeak getMonoPeak()
        {
            if (_peaklist != null && _peaklist.Count>0 )
            {
                return _peaklist[0];
            }
            else
            {
                return null;
            }
        }

        public double GetSummedIntensity()
        {
            if (_peaklist == null) return -1;
            double summedIntensity = 0;
            foreach (MSPeak peak in _peaklist)
            {
                summedIntensity += (double)peak.Height;

            }
            return summedIntensity;
        }


        public IsotopicProfile CloneIsotopicProfile()
        {
            IsotopicProfile iso = new IsotopicProfile();
            iso.AverageMass = AverageMass;
            iso.ChargeState = ChargeState;
            iso.IntensityAggregate = IntensityAggregate;
            iso.MonoIsotopicMass = MonoIsotopicMass;
            iso.MonoIsotopicPeakIndex = MonoIsotopicPeakIndex;
            iso.MonoPeakMZ = MonoPeakMZ;
            iso.MonoPlusTwoAbundance = MonoPlusTwoAbundance;
            iso.MostAbundantIsotopeMass = MostAbundantIsotopeMass;
            iso.IsSaturated = IsSaturated;
            iso.OriginalIntensity = OriginalIntensity;
            iso.Peaklist = new List<MSPeak>();

            foreach (var mspeak in Peaklist)
            {
                MSPeak peak = new MSPeak(mspeak.XValue, mspeak.Height, mspeak.Width, mspeak.SN);
                iso.Peaklist.Add(peak);
            }

            iso.Score = Score;

            return iso;

        }


    }
}
