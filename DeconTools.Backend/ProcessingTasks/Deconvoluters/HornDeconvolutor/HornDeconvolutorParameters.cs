using System;
using System.Collections.Generic;
using System.Text;
#if !Disable_DeconToolsV2
using DeconToolsV2;
#endif

namespace DeconTools.Backend.ProcessingTasks
{
    
#if !Disable_DeconToolsV2

    //this class will act as an adapter class for dealing with DeconEngine's clsTransformParametersClass
    //note that smoothing-related and other unrelated parameters are not here
    public class HornDeconvolutorParameters
    {
        public bool IsActualMonoMZUsed { get; set; }
        public bool IsCheckedAgainstCharge1 { get; set; }
        public bool IsCompleteFit { get; set; }
        public bool IsO16O18Media { get; set; }
        public bool IsThrashed { get; set; }
        public bool IsMSMSProcessed { get; set; }
        public bool IsAbsolutePeptideIntensityUsed { get; set; }
        public bool IsMercuryCachingUsed { get; set; }
        public bool IsMZRangeUsed { get; set; }
        public bool IsScanRangeUsed { get; set; }
        // public clsElementIsotopes ElementIsotopeComposition { get; set; }  //this should be elsewhere
        public double AbsolutePeptideIntensity { get; set; }
        public double ChargeCarrierMass { get; set; }
        public double DeleteIntensityThreshold { get; set; }
        public double LeftFitStringencyFactor { get; set; }
        public double MaxFit { get; set; }
        public double MaxMW { get; set; }
        public double MaxMZ { get; set; }
        public double MinIntensityForScore { get; set; }
        public double MinMZ { get; set; }
        public double MinSignalToNoise { get; set; }
        public double PeptideMinBackgroundRatio { get; set; }
        public double RightFitStringencyFactor { get; set; }
        public enmIsotopeFitType IsotopeFitType { get; set; }
        public int MaxScan { get; set; }
        public int MinScan { get; set; }
        public int NumScansToSumOver { get; set; }
        public short MaxCharge { get; set; }
        public short NumPeaksForShoulder { get; set; }
        public string AveragineFormula { get; set; }
        public string TagFormula { get; set; }

    }

#endif

}
