using System;
using System.Collections.Generic;
using System.Text;

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
            return mostIntensePeak.XValue;
        }

        public double GetFWHM()
        {
            MSPeak mostIntensePeak = getMostIntensePeak();
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
            return mostIntensePeak.SN;
        }

        public double GetMonoAbundance()
        {
            if (this.peaklist == null) return -1;
            return this.peaklist[0].Height;
        }

        public float GetMonoPlusTwoAbundance()
        {
            if (this.peaklist == null || this.peaklist.Count < 3) return 0;
            return this.peaklist[2].Height;
        }

        public double GetMZ()
        {
            if (this.peaklist == null) return -1;
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
            if (peaklist != null && peaklist[0] != null)
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


    }
}
