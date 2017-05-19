using System.Collections.Generic;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution
{

    /// <summary>
    /// Written by Tom Tavener
    /// </summary>
    public class TomIsotopicPattern
    {
        public TomIsotopicPattern()
        {
            _peptideUtils = new PeptideUtils();

            initializeVariables();
        }

        private void initializeVariables()
        {
            fNPerThLow = 0.0090f;
            fNPerThHigh = 0.0180f;
            fNn15a = 0.98f;  //0.996337f;//  // this is our N15 abundance
            fNn14a = 1.0f - fNn15a;//0.003663f;//
            afHa = new double[] { fH1a, fH2a };
            afCa = new double[] { fC12a, fC13a };
            afNa = new double[] { fN14a, fN15a };
            afNna = new double[] { fNn14a, fNn15a };
            afOa = new double[] { fO16a, fO18a };  // ignore below 0.1%
            afSa = new double[] { fS32a, fS34a };  // ignore below 1%
            aafIsos = new double[][] { afCa, afHa, afNa, afOa, afSa };       //[Gord]  - I adjusted the order
            aafN15Isos = new double[][] { afCa, afHa, afNna, afOa, afSa };
            afHm = new double[] { fH1m, fH2m };
            afCm = new double[] { fC12m, fC13m };
            afNm = new double[] { fN14m, fN15m };
            afOm = new double[] { fO16m, fO18m };  // ignore below 0.1%
            afSm = new double[] { fS32m, fS34m };  // ignore below 1%
            aafIsosm = new double[][] { afHm, afCm, afNm, afOm, afSm };

            fAveMassDefect = fC13m - fC12m; //is this the best way to do it?
            fAvnC = 4.9384f;
            fAvnH = 7.7583f;
            fAvnN = 1.3577f;
            fAvnO = 1.4773f;
            fAvnS = 0.0417f;
            afAvn =new double[] { fAvnH, fAvnC, fAvnN, fAvnO, fAvnS };

        }

        public double fNPerThLow; // lower and upper limits of how many 
        public double fNPerThHigh;  // nitrogen atoms are in most peptides

        public double fNn15a = 0.98f;//0.996337f;//  // this is our N15 abundance
        public double fNn14a;


        public double fH1m = 1.007825f;
        double fH1a = 0.99985f;
        double fH2m = 2.014102f;
        double fH2a = 0.00015f;
        double fC12m = 12.0f;
        double fC12a = 0.98893f;
        double fC13m = 13.00336f;
        double fC13a = 0.01107f;
        public double fN14m = 14.00307f;
        double fN14a = 0.996337f;
        public double fN15m = 15.00011f;
        double fN15a = 0.003663f;
        double fO16m = 15.99491f;
        double fO16a = 0.99759f;
        //double fO17m = 16.99913f;
        //double fO17a = 0.000374f;
        double fO18m = 17.99916f;
        double fO18a = 0.002036f;
        double fS32m = 31.97207f;
        double fS32a = 0.9502f;
        //double fS33m = 32.97145f;
        //double fS33a = 0.0075f;
        double fS34m = 33.96786f;
        double fS34a = 0.0421f;
        //double fS35m = 35.96754f;
       // double fS35a = 0.0002f;
        double[] afHa;
        double[] afCa;
        double[] afNa;
        double[] afNna;
        double[] afOa;
        double[] afSa;
        //public double[][] aafIsos = { afHa, afCa, afNa, afOa, afSa };
        //public double[][] aafN15Isos = { afHa, afCa, afNna, afOa, afSa };
        public double[][] aafIsos;
        public double[][] aafN15Isos;
        double[] afHm ;
        double[] afCm ;
        double[] afNm;
        double[] afOm;
        double[] afSm;
        double[][] aafIsosm;

        public double fAveMassDefect;

        double fAvnC = 4.9384f;
        double fAvnH = 7.7583f;
        double fAvnN = 1.3577f;
        double fAvnO = 1.4773f;
        double fAvnS = 0.0417f;
        double[] afAvn;
        private PeptideUtils _peptideUtils;

        public double GetBasePeakMass(int[] afFormula)
        {
            double fBase = 0f;
            for (var i = 0; i < 5; i++)
                fBase += aafIsosm[i][0] * afFormula[i];
            return fBase;
        }

        public double GetBasePeakMass(double[] afFormula, bool bLabel)
        {
            double fBase = 0f;
            if (bLabel == false)
                for (var i = 0; i < 5; i++)
                    fBase += aafIsosm[i][0] * afFormula[i];
            else
                for (var i = 0; i < 5; i++)
                    if (i == 2)
                        fBase += fN15m * afFormula[i];
                    else
                        fBase += aafIsosm[i][0] * afFormula[i];
            return fBase;

        }
        public string GetClosestAvnFormula(double inputMass, bool hasLabel)
        {
            var fAvnMass = GetBasePeakMass(afAvn, hasLabel);
            // get closest pattern to mass 
            var fNumAvn = inputMass / fAvnMass;
            var averagineIntArray = new int[5];
            for (var i = 0; i < 5; i++)
                averagineIntArray[i] = (int)System.Math.Round(afAvn[i] * fNumAvn);


            //I will reverse these to report  C H N O S
            var numHydrogens = averagineIntArray[0];
            var numCarbons = averagineIntArray[1];

            averagineIntArray[0] = numCarbons;
            averagineIntArray[1] = numHydrogens;

            return ("C" + averagineIntArray[0] + "H" + averagineIntArray[1] + "N" + averagineIntArray[2] + "O" + averagineIntArray[3] + "S" + averagineIntArray[4]);

            
        }

        public IsotopicProfile GetAvnPattern(double fInputMass, bool bLabel)
        {
            var averagineFormula = GetClosestAvnFormula(fInputMass, bLabel);


            if (bLabel == false)
                return GetIsotopePattern(averagineFormula, aafIsos);
            else
                return GetIsotopePattern(averagineFormula, aafN15Isos);
        }



        public IsotopicProfile GetIsotopePattern(string empiricalFormula)
        {
            return GetIsotopePattern(empiricalFormula, this.aafIsos);
        }

        public IsotopicProfile GetIsotopePattern(string empiricalFormula, double[][] aafIsoLocal)
        {

            var elementTable = _peptideUtils.ParseEmpiricalFormulaString(empiricalFormula);

            var formulaArray = new int[5];
            formulaArray[0] = getAtomCount(elementTable, "C");
            formulaArray[1] = getAtomCount(elementTable, "H");
            formulaArray[2] = getAtomCount(elementTable, "N");
            formulaArray[3] = getAtomCount(elementTable, "O");
            formulaArray[4] = getAtomCount(elementTable, "S");
            


            double EPS = 0.001f;

            var afIonIntensity = new double[100]; // store structure here
            var aafIons = new double[5, 100];

            double fLogTotal = 0;
            double fLogTotalLast = 0;
            var maxIndices = new int[5];
            for (var i0 = 0; i0 < 5; i0++)
            { // iterate thru elements
                var fLogAmtLight = (double)System.Math.Log10((double)aafIsoLocal[i0][0]);
                var fLogAmtHeavy = (double)System.Math.Log10((double)aafIsoLocal[i0][1]);
                for (var i1 = 0; i1 <= formulaArray[i0]; i1++)
                {
                    fLogTotal = fLogAmtLight * (formulaArray[i0] - i1) + fLogAmtHeavy * i1;
                    fLogTotal += (double)LogAChooseB(formulaArray[i0], i1);
                    aafIons[i0, i1] = (double)(System.Math.Pow(10, fLogTotal));
                    maxIndices[i0] = i1;
                    if (i1 > 1 && fLogTotalLast > fLogTotal && aafIons[i0, i1] < EPS)
                        break;
                    fLogTotalLast = fLogTotal;
                }
            }

            double fIntTmp;
            var maxOffset = 1;
            for (var i = 0; i <= maxIndices[0]; i++)
                for (var j = 0; j <= maxIndices[1]; j++)
                    for (var k = 0; k <= maxIndices[2]; k++)
                        for (var l = 0; l <= maxIndices[3]; l++)
                            for (var m = 0; m <= maxIndices[4]; m++)
                            {
                                var offset = i + j + k + 2 * l + 2 * m; // O and S are +2
                                fIntTmp = aafIons[0, i] * aafIons[1, j] *
                                    aafIons[2, k] * aafIons[3, l] * aafIons[4, m];
                                if (offset > maxOffset) maxOffset = offset;
                                afIonIntensity[offset] += fIntTmp;
                            }
            // normalize
            double max = 0;
            var j1 = 0;
            for (j1 = 0; j1 <= maxOffset; j1++)
                if (afIonIntensity[j1] > max) max = afIonIntensity[j1];
            for (j1 = 0; j1 <= maxOffset; j1++) afIonIntensity[j1] /= max;
            var isoCluster = new IsotopicProfile();

            for (j1 = 0; j1 < afIonIntensity.Length && j1 <= maxOffset; j1++)
                isoCluster.Peaklist.Add(new MSPeak(0.0f, (float)afIonIntensity[j1], 0.0f, 0.0f));
            return isoCluster;
        }

        private int getAtomCount(Dictionary<string, int> elementTable, string elementSymbol)
        {
            if (elementTable.ContainsKey(elementSymbol))
            {
                return elementTable[elementSymbol];
            }
            else
            {
                return 0;
            }

        }

        public double LogAChooseB(int a, int b) // 10 choose 2 returns 10.9/1.2
        {
            if (a == 0) return 0.0f;
            if (b == 0) return 0.0f;
            if (a == b) return 0.0f;
            var total = 0.0;
            total += LogFactorial(a);
            total -= LogFactorial(b);
            total -= LogFactorial(a - b);
            return (double)total;
        }

        public double LogFactorial(int n)
        {
            //log n! = 0.5log(2.pi) + 0.5logn + nlog(n/e) + log(1 + 1/(12n))
            return (double)0.5 * (
                System.Math.Log10(2 * System.Math.PI * n))
                + n * (System.Math.Log10(n / System.Math.E))
                + (System.Math.Log10(1.0 + 1.0 / (12 * n)));
        }
    }
}
