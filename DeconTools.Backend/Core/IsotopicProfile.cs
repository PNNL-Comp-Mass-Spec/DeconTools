using System;
using System.Collections.Generic;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class IsotopicProfile
    {

        public IsotopicProfile()
        {
            this.peaklist = new List<MSPeak>();
        }
        
        private List<MSPeak> peaklist;

        public List<MSPeak> Peaklist
        {
            get { return peaklist; }
            set { peaklist = value; }
        }

        /// <summary>
        /// Zero-based index value that points to which peak of the PeakList is the monoisotopic peak.  (it isn't always the first one)
        /// </summary>
        public int MonoIsotopicPeakIndex { get; set; }


        private int chargeState;

        public int ChargeState
        {
            get { return chargeState; }
            set { chargeState = value; }
        }


        private double intensityAggregate;   //    a way of storing an overall intensity for the whole profile

        public double IntensityAggregate
        {
            get { return intensityAggregate; }
            set { intensityAggregate = value; }
        }


        private double originalIntensity;   // the unsummed intensity;  
        public double OriginalIntensity
        {
            get { return originalIntensity; }
            set { originalIntensity = value; }
        }

        private double original_Total_isotopic_abundance;  // aka original_TIA;   used in IMS-TOF analysis.

        public double Original_Total_isotopic_abundance
        {
            get { return original_Total_isotopic_abundance; }
            set { original_Total_isotopic_abundance = value; }
        }



        private double score;

        public double Score
        {
            get { return score; }
            set { score = value; }
        }


        private double monoIsotopicMass;

        public double MonoIsotopicMass
        {
            get { return monoIsotopicMass; }
            set { monoIsotopicMass = value; }
        }

        private double mostAbundantIsotopeMass;

        public double MostAbundantIsotopeMass
        {
            get { return mostAbundantIsotopeMass; }
            set { mostAbundantIsotopeMass = value; }
        }

        private float monoPlusTwoAbundance;

        public float MonoPlusTwoAbundance
        {
            get { return monoPlusTwoAbundance; }
            set { monoPlusTwoAbundance = value; }
        }

        private double averageMass; 

        public double AverageMass
        {
            get { return averageMass; }
            set { averageMass = value; }
        }



        public int GetNumOfIsotopesInProfile()
        {
            return peaklist.Count;
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


        public int getIndexOfMostIntensePeak()
        {
            if (this.peaklist == null || this.peaklist.Count == 0) return -1;

            int indexOfMaxPeak = -1;
            float maxIntensity = 0;
            
            for (int i = 0; i < this.peaklist.Count; i++)
            {
                if (this.peaklist[i].Height > maxIntensity)
                {
                    maxIntensity = this.peaklist[i].Height;
                    indexOfMaxPeak = i;
                }
            }
            return indexOfMaxPeak;

        }

        public MSPeak getMostIntensePeak()
        {
            if (this.peaklist == null || this.peaklist.Count == 0) return null;

            MSPeak maxPeak = new MSPeak();
            foreach (MSPeak peak in this.peaklist)
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
            if (this.peaklist == null||this.Peaklist.Count==0) return 0;
            return this.peaklist[0].Height;
        }

        public float GetMonoPlusTwoAbundance()
        {
            if (this.peaklist == null || this.peaklist.Count < 3) return 0;
            return this.peaklist[2].Height;
        }

        public double GetMZ()
        {
            if (this.peaklist == null || this.Peaklist.Count==0) return -1;
            return this.peaklist[0].XValue;
        }

        public double MonoPeakMZ { get; set; }

        public double GetAbundance()
        {
            //MSPeak mostIntensePeak = getMostIntensePeak(this.peaklist);
            //return mostIntensePeak.Intensity;

            return this.IntensityAggregate;
        }

        public double GetScore()
        {
            return score;
        }

        public MSPeak getMonoPeak()
        {
            if (peaklist != null && peaklist.Count>0 )
            {
                return peaklist[0];
            }
            else
            {
                return null;
            }
        }

        public double GetSummedIntensity()
        {
            if (peaklist == null) return -1;
            double summedIntensity = 0;
            foreach (MSPeak peak in this.peaklist)
            {
                summedIntensity += (double)peak.Height;

            }
            return summedIntensity;
        }


        public IsotopicProfile CloneIsotopicProfile()
        {
            IsotopicProfile iso = new IsotopicProfile();
            iso.AverageMass = this.AverageMass;
            iso.ChargeState = this.ChargeState;
            iso.IntensityAggregate = this.IntensityAggregate;
            iso.MonoIsotopicMass = this.MonoIsotopicMass;
            iso.MonoIsotopicPeakIndex = this.MonoIsotopicPeakIndex;
            iso.MonoPeakMZ = this.MonoPeakMZ;
            iso.MonoPlusTwoAbundance = this.MonoPlusTwoAbundance;
            iso.MostAbundantIsotopeMass = this.MostAbundantIsotopeMass;
            iso.Original_Total_isotopic_abundance = this.Original_Total_isotopic_abundance;
            iso.OriginalIntensity = this.OriginalIntensity;
            iso.Peaklist = new List<MSPeak>();

            foreach (var mspeak in this.Peaklist)
            {
                MSPeak peak = new MSPeak(mspeak.XValue, mspeak.Height, mspeak.Width, mspeak.SN);
                iso.Peaklist.Add(peak);
            }

            iso.Score = this.Score;

            return iso;

        }


    }
}
