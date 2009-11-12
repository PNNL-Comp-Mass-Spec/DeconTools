using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.N14N15Analyzers.TomN14N15Analyzer
{
    class TemplateMatch : System.IComparable
    {
        public enum SortBy { SCORE, MZ, MONOMASS };
        public double fMZ; // this is the maximum intensity mz in the spectrum
        public double fMonoMass; // this is the maximum intensity mz in the spectrum
        public int iScan;
        public int iMonoPosition; // this is where the monoisotopic mass lives
        //public int iPeakPosition; // this is where the max-intensity mass lives
        public double fScore;
        public double fTotalPeakRatio;
        public bool bIs14N15N = false;
        public double fHeavyMonoMass;
        public double fHeavyMZ;
        public double fHeavyIntensity;
        public double fLightIntensity;
        public double fBasePeakInten;
        public double fFit;
        public int iCharge;
        public int iDeltaN;
        public int iPos; // this is the position of the monoisotope mass in the template
        public bool bIsoPatternSet;
        public IsotopicProfile isoPattern; // the isotope pattern
        public static SortBy m_SortBy = SortBy.SCORE;
        public int[] aiCorrespondingPeaks; // stores positions of corr peaks

       
        public void SetCorrespondingPeaks(int[] aiPeaks)
        {
            aiCorrespondingPeaks = new int[aiPeaks.Length];
            for (int ii = 0; ii < aiCorrespondingPeaks.Length; ii++)
                aiCorrespondingPeaks[ii] = aiPeaks[ii];
        }

        public void Print()
        {
            Console.WriteLine("{0} {1} {2} {3}", fMZ, fHeavyMZ, iCharge, iDeltaN);
            //foreach (MZPeak mz in amzIsoPattern)
            //	mz.Print();
        }


        public TemplateMatch(TemplateMatch TM)//, int[] aiMZW1)
        {
            fScore = TM.fScore;
            fMZ = TM.fMZ;
            iCharge = TM.iCharge;
            iPos = TM.iPos;
            iDeltaN = TM.iDeltaN;
            isoPattern = TM.isoPattern;
            fMonoMass = TM.fMonoMass;
            fTotalPeakRatio = TM.fTotalPeakRatio;
            bIs14N15N = TM.bIs14N15N;
            fHeavyMonoMass = TM.fHeavyMonoMass;
            fHeavyMZ = TM.fHeavyMZ;
            fHeavyIntensity = TM.fHeavyIntensity;
            fLightIntensity = TM.fLightIntensity;
            fBasePeakInten = TM.fBasePeakInten;
            fFit = TM.fFit;
            iScan = TM.iScan;

        }

        public TemplateMatch(double fS, double fMZ1, int iC, int iP)//, int[] aiMZW1)
        {
            fScore = fS;
            fMZ = fMZ1;
            iCharge = iC;
            iPos = iP;
            iDeltaN = 0;
            isoPattern = new IsotopicProfile();
            aiCorrespondingPeaks = new int[isoPattern.Peaklist.Count];
            //for (int ii = 0; ii < aiCorrespondingPeaks.Count; ii++) aiCorrespondingPeaks[ii] = -1; // not there

        }

        public void SetTemplateIsoPattern(IsotopicProfile mzIsoPattern)
        {
            isoPattern = mzIsoPattern;
            bIsoPatternSet = true;
        }

        public static SortBy SetSortBy(SortBy val)
        {
            m_SortBy = (SortBy)val;
            return m_SortBy;
        }

        public int CompareTo(object obj)
        {
            TemplateMatch TM = (TemplateMatch)obj;
            switch (m_SortBy)
            {
                case TemplateMatch.SortBy.SCORE:
                    return this.fScore.CompareTo(TM.fScore);
                case TemplateMatch.SortBy.MZ:
                    return this.fMZ.CompareTo(TM.fMZ);
                case TemplateMatch.SortBy.MONOMASS:
                    return this.fMonoMass.CompareTo(TM.fMonoMass);
                default:
                    return this.fScore.CompareTo(TM.fScore);
            }
        }
    }

}
