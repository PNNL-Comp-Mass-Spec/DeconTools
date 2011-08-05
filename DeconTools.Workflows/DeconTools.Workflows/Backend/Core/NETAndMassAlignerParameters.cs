
namespace DeconTools.Workflows.Backend.Core
{
    public class NETAndMassAlignerParameters
    {

     
        public NETAndMassAlignerParameters()
        {
            //note: defaults are based on VIPER defaults

            ContractionFactor = 3;
            MassBinSize = 0.02;
            MassCalibrationLSQNumKnots = 12;
            MassCalibrationLSQZScore = 3;
            MassCalibrationMaxJump = 50;
            MassCalibrationMaxZScore = 3;
            MassCalibrationNumMassDeltaBins = 100;
            MassCalibrationNumXSlices = 12;                 // seems to be a sensitive parameter
            MassCalibrationUseLSQ = false;
            MassCalibrationWindow = 50;
            MassToleranceForNETAlignment = 6;
            MaxPromiscuity = 2;
            MaxTimeJump = 10;
            this.NETBinSize = 0.001;
            this.NETTolerance = 0.02;
            this.NumTimeSections = 100;
            this.SplitAlignmentInMZ = false;
            this.UsePromiscuousPoints = false;
        
                
        }

        public bool ApplyMassRecalibration { get; set; }
        public short ContractionFactor { get; set; }
        public bool IsAlignmentBaselineAMassTagDB { get; set; }
        public double MassBinSize { get; set; }
        public short MassCalibrationLSQNumKnots { get; set; }
        public double MassCalibrationLSQZScore { get; set; }
        public short MassCalibrationMaxJump { get; set; }
        public double MassCalibrationMaxZScore { get; set; }
        public short MassCalibrationNumMassDeltaBins { get; set; }
        public short MassCalibrationNumXSlices { get; set; }
        public bool MassCalibrationUseLSQ { get; set; }
        public double MassCalibrationWindow { get; set; }
        public double MassToleranceForNETAlignment { get; set; }
        public short MaxPromiscuity { get; set; }
        public short MaxTimeJump { get; set; }
        public double NETBinSize { get; set; }
        public double NETTolerance { get; set; }
        public int NumTimeSections { get; set; }
        public bool SplitAlignmentInMZ { get; set; }
        public bool UsePromiscuousPoints { get; set; }


            //mintNumTimeSections				= 100; 
            //mshortContractionFactor			= 3; 
            //mshortMaxTimeDistortion			= 10; 
            //mshortMaxPromiscuity			= 3; 
            //mblnUsePromiscuousPoints		= false; 
            //mblnMassCalibUseLSQ				= false; 
            //mdblMassCalibrationWindow		= 6.0; 
            //mshortMassCalibNumXSlices		= 12; 
            //mshortMassCalibNumYSlices		= 50; 
            //mshortMassCalibMaxJump			= 20; 
            //mdblMassCalibMaxZScore			= 3; 
            //mdblMassCalibLSQMaxZScore		= 2.5; 
            //mshortMassCalLSQNumKnots		= 12; 
            //mdblMassTolerance				= 6.0; 
            //mdblNETTolerance				= 0.03; 
            //menmAlignmentType				= NET_MASS_WARP; 
            //menmCalibrationType				= HYBRID_CALIB;
            //mblnAlignToMassTagDatabase		= false; 
            //mstrAlignmentBaselineName		= 0; 
            //mdouble_massBinSize				= .2;
            //mdouble_massBinSize				= .001;

            //mbool_alignSplitMZs				= false;
            //mlist_mzBoundaries				= new List<classAlignmentMZBoundary*>();

            ///// Construct the m/z boundary object.			
            ///*mlist_mzBoundaries->Add(new classAlignmentMZBoundary(0.0, 5000.1));
            //mlist_mzBoundaries->Add(new classAlignmentMZBoundary(5000.1,999999999.0));*/
            //mlist_mzBoundaries->Add(new classAlignmentMZBoundary(0.0, 505.7));
            //mlist_mzBoundaries->Add(new classAlignmentMZBoundary(505.7, 999999999.0));
    }
}
