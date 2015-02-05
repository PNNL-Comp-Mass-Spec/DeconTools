using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.Utilities
{
#if !Disable_DeconToolsV2

    public class MercuryDistributionCreator
    {

        private DeconToolsV2.HornTransform.clsHornTransformParameters hornParameters;
        private DeconToolsV2.HornTransform.clsAveragine avergineFormulaCreator;
        private DeconToolsV2.clsMercuryIsotopeDistribution decon2LSMercuryDistribution;
        private DeconToolsPeakDetector peakDetector;


        private MolecularFormula molecularFormula;

        public MolecularFormula MolecularFormula
        {
            get { return molecularFormula; }
            set { molecularFormula = value; }
        }



        System.Collections.Hashtable elementTable;

        private int chargeState;
        public int ChargeState
        {
            get { return chargeState; }
            set { chargeState = value; }
        }

        private double resolution;
        public double Resolution
        {
            get { return resolution; }
            set { resolution = value; }
        }

        private XYData data;
        public XYData Data
        {
            get { return data; }
            set { data = value; }
        }

        private IsotopicProfile isotopicProfile;

        public IsotopicProfile IsotopicProfile
        {
            get { return isotopicProfile; }
            set { isotopicProfile = value; }
        }



        public DeconToolsV2.HornTransform.clsHornTransformParameters HornParameters
        {
            get { return hornParameters; }
            set { hornParameters = value; }
        }
              
        public MercuryDistributionCreator()
        {
            avergineFormulaCreator = new DeconToolsV2.HornTransform.clsAveragine();
            hornParameters = new DeconToolsV2.HornTransform.clsHornTransformParameters();
            decon2LSMercuryDistribution = new DeconToolsV2.clsMercuryIsotopeDistribution();
            peakDetector = new DeconToolsPeakDetector();
        }


        

        public void getIsotopicProfile()
        {
            Check.Require(this.data != null, "Data has not been loaded. 'CalculateDistribution' should first be executed");

            Run run = new ConcreteXYDataRun(this.data.Xvalues, this.data.Yvalues);
            run.CurrentScanSet = new ScanSet(0);

            ResultCollection tempResults = new ResultCollection(run);
            peakDetector.PeakToBackgroundRatio = 0.5;
            peakDetector.SignalToNoiseThreshold = 1;
            peakDetector.PeakFitType = Globals.PeakFitType.QUADRATIC;
            peakDetector.IsDataThresholded = true;
            peakDetector.Execute(run.ResultCollection);

            this.IsotopicProfile = new IsotopicProfile();
            this.IsotopicProfile.ChargeState = this.chargeState;
            this.IsotopicProfile.Peaklist = Utilities.Converters.PeakTypeConverter.ConvertToMSPeaks(run.PeakList);

            
        }




       

        private MSPeak findClosestPeak(List<MSPeak> list, double targetMZ)
        {
            if (list == null) return null;
            if (list.Count == 0) return null;
            
            double diff = double.MaxValue;
            MSPeak closestPeak = new MSPeak();

            foreach (MSPeak peak in list)
            {
                double currentDiff = Math.Abs(peak.XValue - targetMZ);
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
            double monoIsotopicMass = mz * chargeState - chargeState * hornParameters.CCMass;
            string empiricalFormula = avergineFormulaCreator.GenerateAveragineFormula(monoIsotopicMass, hornParameters.AveragineFormula, hornParameters.TagFormula);
            return MolecularFormula.Parse(empiricalFormula);
        }
        
        
        public void CreateDistribution(MolecularFormula molecularFormula)
        {
            Check.Require(molecularFormula != null, "Molecular formula has not been defined.");

            this.molecularFormula=molecularFormula;
            this.data = new XYData();    //clears any old data

            this.elementTable = molecularFormula.ToElementTable();
            this.decon2LSMercuryDistribution = new DeconToolsV2.clsMercuryIsotopeDistribution();

            
           
            

            this.decon2LSMercuryDistribution.Resolution = this.resolution;
            this.decon2LSMercuryDistribution.ChargeState = (short)this.chargeState;

            

            System.Drawing.PointF[] drawingpoints = this.decon2LSMercuryDistribution.CalculateDistribution(this.elementTable);

            this.data = XYData.ConvertDrawingPoints(drawingpoints);
        }

        public void CreateDistribution(double mass, int chargeState, double resolution)
        {
            string empiricalFormula = avergineFormulaCreator.GenerateAveragineFormula(mass, hornParameters.AveragineFormula, hornParameters.TagFormula);
            this.molecularFormula = MolecularFormula.Parse(empiricalFormula);
            this.resolution = resolution;
            this.chargeState = chargeState;
            CreateDistribution(this.molecularFormula);

        }


        //public void CreateDistribution(double mz, int chargeState, double fwhm)
        //{
        //    double monoIsotopicMass = mz * chargeState - chargeState * hornParameters.CCMass;
        //    string empiricalFormula = avergineFormulaCreator.GenerateAveragineFormula(monoIsotopicMass, hornParameters.AveragineFormula, hornParameters.TagFormula);
        //    this.molecularFormula = MolecularFormula.Parse(empiricalFormula);
        //    this.resolution = mz / fwhm;
        //    this.chargeState = chargeState;

        //    CreateDistribution(this.molecularFormula);
            
            


        //}

        
        private double calculateOffset(double targetMZ)
        {
            MSPeak closestPeak = findClosestPeak(this.IsotopicProfile.Peaklist, targetMZ);
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
            if (this.isotopicProfile == null || this.isotopicProfile.Peaklist == null || this.isotopicProfile.Peaklist.Count == 0) return;

            MSPeak mostIntensePeak = this.isotopicProfile.getMostIntensePeak();
            int indexOfMostIntensePeak = this.isotopicProfile.Peaklist.IndexOf(mostIntensePeak);

            if (targetIsotopicProfile.Peaklist == null || targetIsotopicProfile.Peaklist.Count == 0) return;

            bool enoughPeaksInTarget = (indexOfMostIntensePeak <= targetIsotopicProfile.Peaklist.Count - 1);

            if (enoughPeaksInTarget)
            {
                MSPeak targetPeak = targetIsotopicProfile.Peaklist[indexOfMostIntensePeak];
                //offset = targetPeak.MZ - mostIntensePeak.MZ;
                offset = targetIsotopicProfile.Peaklist[0].XValue - this.isotopicProfile.Peaklist[0].XValue;   //want to test to see if Thrash is same as rapid

            }
            else
            {
                offset = targetIsotopicProfile.Peaklist[0].XValue - this.isotopicProfile.Peaklist[0].XValue;
            }

            for (int i = 0; i < this.data.Xvalues.Length; i++)
            {
                this.data.Xvalues[i] = this.data.Xvalues[i] + offset;

            }

        }
        
    }

#endif

}
