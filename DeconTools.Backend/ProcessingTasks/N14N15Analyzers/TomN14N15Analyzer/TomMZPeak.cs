using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.ProcessingTasks.N14N15Analyzers.TomN14N15Analyzer
{
    public class TomMZPeak: System.IComparable
    {


        public enum SortBy {MZ, INTENSITY};
		public double fMZ;
		public double fInt;
		public double fInUse;
		public static SortBy m_SortBy = SortBy.MZ;
		//public ArrayList aiMatchesPeaks;
        public TomMZPeak(double fM, double fI, double fU)
		{
			fMZ = fM;
			fInt = fI;
			fInUse = fU;
			//aiMatchesPeaks = new ArrayList();
		}

		public static SortBy SetSortBy(SortBy val)
		{
			m_SortBy = (SortBy)val;
			return m_SortBy;
		}
		

		public double[] GetMzIntPair()
		{
			return new double[] {fMZ, fInt};
		}

		public void Print()
		{
			System.Console.WriteLine("{0} {1}", fMZ, fInt);
		}



        public int CompareTo(object obj)
        {
            TomMZPeak TM = (TomMZPeak)obj;
            switch (m_SortBy)
            {
                case TomMZPeak.SortBy.INTENSITY:
                    return this.fInt.CompareTo(TM.fInt);
                case TomMZPeak.SortBy.MZ:
                    return this.fMZ.CompareTo(TM.fMZ);
                default:
                    return this.fMZ.CompareTo(TM.fMZ);
            }
        }

    }
}
