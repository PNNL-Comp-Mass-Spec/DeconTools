using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution
{
    public class TomIsotopicPattern
    {


        public static double fNPerThLow = 0.0090f; // lower and upper limits of how many 
        public static double fNPerThHigh = 0.0180f; // nitrogen atoms are in most peptides

        public static double fNn15a = 0.98f;//0.996337f;//  // this is our N15 abundance
        public static double fNn14a = 1.0f - fNn15a;//0.003663f;//


        public static double fH1m = 1.007825f;
        static double fH1a = 0.99985f;
        static double fH2m = 2.014102f;
        static double fH2a = 0.00015f;
        static double fC12m = 12.0f;
        static double fC12a = 0.98893f;
        static double fC13m = 13.00336f;
        static double fC13a = 0.01107f;
        public static double fN14m = 14.00307f;
        static double fN14a = 0.996337f;
        public static double fN15m = 15.00011f;
        static double fN15a = 0.003663f;
        static double fO16m = 15.99491f;
        static double fO16a = 0.99759f;
        static double fO17m = 16.99913f;
        static double fO17a = 0.000374f;
        static double fO18m = 17.99916f;
        static double fO18a = 0.002036f;
        static double fS32m = 31.97207f;
        static double fS32a = 0.9502f;
        static double fS33m = 32.97145f;
        static double fS33a = 0.0075f;
        static double fS34m = 33.96786f;
        static double fS34a = 0.0421f;
        static double fS35m = 35.96754f;
        static double fS35a = 0.0002f;
        static double[] afHa = { fH1a, fH2a };
        static double[] afCa = { fC12a, fC13a };
        static double[] afNa = { fN14a, fN15a };
        static double[] afNna = { fNn14a, fNn15a };
        static double[] afOa = { fO16a, fO18a };  // ignore below 0.1%
        static double[] afSa = { fS32a, fS34a };  // ignore below 1%
        //public static double[][] aafIsos = { afHa, afCa, afNa, afOa, afSa };
        //public static double[][] aafN15Isos = { afHa, afCa, afNna, afOa, afSa };
        public static double[][] aafIsos = { afCa, afHa, afNa, afOa, afSa };       //[Gord]  - I adjusted the order
        public static double[][] aafN15Isos = { afCa, afHa, afNna, afOa, afSa };
        static double[] afHm = { fH1m, fH2m };
        static double[] afCm = { fC12m, fC13m };
        static double[] afNm = { fN14m, fN15m };
        static double[] afOm = { fO16m, fO18m };  // ignore below 0.1%
        static double[] afSm = { fS32m, fS34m };  // ignore below 1%
        static double[][] aafIsosm = { afHm, afCm, afNm, afOm, afSm };

        public static double fAveMassDefect = fC13m - fC12m; //is this the best way to do it?

        static double fAvnC = 4.9384f;
        static double fAvnH = 7.7583f;
        static double fAvnN = 1.3577f;
        static double fAvnO = 1.4773f;
        static double fAvnS = 0.0417f;
        static double[] afAvn = { fAvnH, fAvnC, fAvnN, fAvnO, fAvnS };

        public static double GetBasePeakMass(int[] afFormula)
        {
            double fBase = 0f;
            for (int i = 0; i < 5; i++)
                fBase += aafIsosm[i][0] * afFormula[i];
            return fBase;
        }

        public static double GetBasePeakMass(double[] afFormula, bool bLabel)
        {
            double fBase = 0f;
            if (bLabel == false)
                for (int i = 0; i < 5; i++)
                    fBase += aafIsosm[i][0] * afFormula[i];
            else
                for (int i = 0; i < 5; i++)
                    if (i == 2)
                        fBase += fN15m * afFormula[i];
                    else
                        fBase += aafIsosm[i][0] * afFormula[i];
            return fBase;

        }
        public static int[] GetClosestAvnFormula(double fInputMass, bool bLabel)
        {
            double fAvnMass = GetBasePeakMass(afAvn, bLabel);
            // get closest pattern to mass 
            double fNumAvn = fInputMass / fAvnMass;
            int[] afAvnPep = new int[5];
            for (int i = 0; i < 5; i++)
                afAvnPep[i] = (int)System.Math.Round(afAvn[i] * fNumAvn);
            return afAvnPep;
        }

        public static IsotopicProfile GetAvnPattern(double fInputMass, bool bLabel)
        {
            int[] afAvnPep = GetClosestAvnFormula(fInputMass, bLabel);
            if (bLabel == false)
                return GetIsotopePattern(afAvnPep, aafIsos);
            else
                return GetIsotopePattern(afAvnPep, aafN15Isos);
        }

        public static IsotopicProfile GetIsotopePattern(int[] afFormula, double[][] aafIsoLocal)
        {
            double EPS = 0.001f;

            double[] afIonIntensity = new double[100]; // store structure here
            double[,] aafIons = new double[5, 100];

            double fLogTotal = 0;
            double fLogTotalLast = 0;
            int[] maxIndices = new int[5];
            for (int i0 = 0; i0 < 5; i0++)
            { // iterate thru elements
                double fLogAmtLight = (double)System.Math.Log10((double)aafIsoLocal[i0][0]);
                double fLogAmtHeavy = (double)System.Math.Log10((double)aafIsoLocal[i0][1]);
                for (int i1 = 0; i1 <= afFormula[i0]; i1++)
                {
                    fLogTotal = fLogAmtLight * (afFormula[i0] - i1) + fLogAmtHeavy * i1;
                    fLogTotal += (double)LogAChooseB(afFormula[i0], i1);
                    aafIons[i0, i1] = (double)(System.Math.Pow(10, fLogTotal));
                    maxIndices[i0] = i1;
                    if (i1 > 1 && fLogTotalLast > fLogTotal && aafIons[i0, i1] < EPS)
                        break;
                    fLogTotalLast = fLogTotal;
                }
            }

            double fIntTmp;
            int maxOffset = 1;
            for (int i = 0; i <= maxIndices[0]; i++)
                for (int j = 0; j <= maxIndices[1]; j++)
                    for (int k = 0; k <= maxIndices[2]; k++)
                        for (int l = 0; l <= maxIndices[3]; l++)
                            for (int m = 0; m <= maxIndices[4]; m++)
                            {
                                int offset = i + j + k + 2 * l + 2 * m; // O and S are +2
                                fIntTmp = aafIons[0, i] * aafIons[1, j] *
                                    aafIons[2, k] * aafIons[3, l] * aafIons[4, m];
                                if (offset > maxOffset) maxOffset = offset;
                                afIonIntensity[offset] += fIntTmp;
                            }
            // normalize
            double max = 0;
            int j1 = 0;
            for (j1 = 0; j1 <= maxOffset; j1++)
                if (afIonIntensity[j1] > max) max = afIonIntensity[j1];
            for (j1 = 0; j1 <= maxOffset; j1++) afIonIntensity[j1] /= max;
            IsotopicProfile isoCluster = new IsotopicProfile();

            for (j1 = 0; j1 < afIonIntensity.Length && j1 <= maxOffset; j1++)
                isoCluster.Peaklist.Add(new MSPeak(0.0f, (float)afIonIntensity[j1], 0.0f, 0.0f));
            return isoCluster;
        }

        public static double LogAChooseB(int a, int b) // 10 choose 2 returns 10.9/1.2
        {
            if (a == 0) return 0.0f;
            if (b == 0) return 0.0f;
            if (a == b) return 0.0f;
            double total = 0.0;
            total += LogFactorial(a);
            total -= LogFactorial(b);
            total -= LogFactorial(a - b);
            return (double)total;
        }

        public static double LogFactorial(int n)
        {
            //log n! = 0.5log(2.pi) + 0.5logn + nlog(n/e) + log(1 + 1/(12n))
            return (double)0.5 * (
                System.Math.Log10(2 * System.Math.PI * n))
                + n * (System.Math.Log10(n / System.Math.E))
                + (System.Math.Log10(1.0 + 1.0 / (12 * n)));
        }
    }
}
