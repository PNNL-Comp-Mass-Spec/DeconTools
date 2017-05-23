using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.ElementalFormulas;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.Utilities
{
    public class MercuryDistributionCreator
    {
        private Averagine averagineFormulaCreator;
        private ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.Mercury.MercuryIsotopeDistribution decon2LSMercuryDistribution;
        private DeconToolsPeakDetector peakDetector;
        private System.Collections.Hashtable elementTable;

        public MolecularFormula MolecularFormula { get; set; }

        public int ChargeState { get; set; }

        public double Resolution { get; set; }

        public XYData Data { get; set; }

        public IsotopicProfile IsotopicProfile { get; set; }

        public MercuryDistributionCreator()
        {
            averagineFormulaCreator = new Averagine();
            decon2LSMercuryDistribution = new ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.Mercury.MercuryIsotopeDistribution();
            peakDetector = new DeconToolsPeakDetector();
        }

        public void getIsotopicProfile()
        {
            Check.Require(this.Data != null, "Data has not been loaded. 'CalculateDistribution' should first be executed");

            Run run = new ConcreteXYDataRun(this.Data.Xvalues, this.Data.Yvalues);
            run.CurrentScanSet = new ScanSet(0);

            var tempResults = new ResultCollection(run);
            peakDetector.PeakToBackgroundRatio = 0.5;
            peakDetector.SignalToNoiseThreshold = 1;
            peakDetector.PeakFitType = Globals.PeakFitType.QUADRATIC;
            peakDetector.IsDataThresholded = true;
            peakDetector.Execute(run.ResultCollection);

            this.IsotopicProfile = new IsotopicProfile();
            this.IsotopicProfile.ChargeState = this.ChargeState;
            this.IsotopicProfile.Peaklist = Utilities.Converters.PeakTypeConverter.ConvertToMSPeaks(run.PeakList);
        }

        private MSPeak findClosestPeak(List<MSPeak> list, double targetMZ)
        {
            if (list == null) return null;
            if (list.Count == 0) return null;

            var diff = double.MaxValue;
            var closestPeak = new MSPeak();

            foreach (var peak in list)
            {
                var currentDiff = Math.Abs(peak.XValue - targetMZ);
                if (currentDiff < diff)
                {
                    closestPeak = peak;
                    diff = currentDiff;
                }

            }
            return closestPeak;
        }

        public MolecularFormula GetAveragineFormula(double mz, int chargeState)
        {
            var monoIsotopicMass = mz * chargeState - chargeState * decon2LSMercuryDistribution.ChargeCarrierMass;
            var empiricalFormula = averagineFormulaCreator.GenerateAveragineFormula(monoIsotopicMass);
            return MolecularFormula.Parse(empiricalFormula);
        }

        public void CreateDistribution(MolecularFormula molecularFormula)
        {
            Check.Require(molecularFormula != null, "Molecular formula has not been defined.");

            this.MolecularFormula=molecularFormula;
            this.Data = new XYData();    //clears any old data

            var deconToolsMolFormula = ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.ElementalFormulas.MolecularFormula.ConvertFromString(molecularFormula.ToFormulaString());
            this.decon2LSMercuryDistribution = new ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.Mercury.MercuryIsotopeDistribution();

            List<double> x, y, xIsotope, yIsotope;
            this.decon2LSMercuryDistribution.CalculateDistribution(this.ChargeState, this.Resolution, deconToolsMolFormula, out x, out y, 0, out xIsotope, out yIsotope, false);

            this.Data = new XYData() { Xvalues = x.ToArray(), Yvalues = y.ToArray() };
        }

        public void CreateDistribution(double mass, int chargeState, double resolution)
        {
            var empiricalFormula = averagineFormulaCreator.GenerateAveragineFormula(mass);
            this.MolecularFormula = MolecularFormula.Parse(empiricalFormula);
            this.Resolution = resolution;
            this.ChargeState = chargeState;
            CreateDistribution(this.MolecularFormula);
        }

        //public void CreateDistribution(double mz, int chargeState, double fwhm)
        //{
        //    double monoIsotopicMass = mz * chargeState - chargeState * decon2LSMercuryDistribution.ChargeCarrierMass;
        //    string empiricalFormula = avergineFormulaCreator.GenerateAveragineFormula(monoIsotopicMass);
        //    this.MolecularFormula = MolecularFormula.Parse(empiricalFormula);
        //    this.Resolution = mz / fwhm;
        //    this.ChargeState = chargeState;

        //    CreateDistribution(this.MolecularFormula);
        //}

        private double calculateOffset(double targetMZ)
        {
            var closestPeak = findClosestPeak(this.IsotopicProfile.Peaklist, targetMZ);
            return (targetMZ - closestPeak.XValue);

        }

        /// <summary>
        /// This applies an offset to the XY data of the theoretical distribution. This is typically done
        /// to align the theoretical distribution to the observed dist.  The algorithm finds the most
        /// intense peak of the theor dist, and then checks to see if the same relative peak is available in
        /// the observed dist.  The offset is calculated from the difference in the mz of these two peaks.
        /// If the relative peak can't be found in the observed dist, then the theor dist is offset based on
        /// the first peak of each dist.
        /// </summary>
        /// <param name="targetIsotopicProfile"></param>
        public void OffsetDistribution(IsotopicProfile targetIsotopicProfile)
        {
            double offset = 0;

            getIsotopicProfile();    //this generates the peakList from the theor dist
            if (this.IsotopicProfile == null || this.IsotopicProfile.Peaklist == null || this.IsotopicProfile.Peaklist.Count == 0) return;

            var mostIntensePeak = this.IsotopicProfile.getMostIntensePeak();
            var indexOfMostIntensePeak = this.IsotopicProfile.Peaklist.IndexOf(mostIntensePeak);

            if (targetIsotopicProfile.Peaklist == null || targetIsotopicProfile.Peaklist.Count == 0) return;

            var enoughPeaksInTarget = (indexOfMostIntensePeak <= targetIsotopicProfile.Peaklist.Count - 1);

            if (enoughPeaksInTarget)
            {
                var targetPeak = targetIsotopicProfile.Peaklist[indexOfMostIntensePeak];
                //offset = targetPeak.MZ - mostIntensePeak.MZ;
                offset = targetIsotopicProfile.Peaklist[0].XValue - this.IsotopicProfile.Peaklist[0].XValue;   //want to test to see if Thrash is same as rapid
            }
            else
            {
                offset = targetIsotopicProfile.Peaklist[0].XValue - this.IsotopicProfile.Peaklist[0].XValue;
            }

            for (var i = 0; i < this.Data.Xvalues.Length; i++)
            {
                this.Data.Xvalues[i] = this.Data.Xvalues[i] + offset;
            }
        }
    }
}
