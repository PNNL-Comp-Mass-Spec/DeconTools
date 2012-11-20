using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    public class UIMF_MSGenerator : MSGenerator
    {

        public UIMF_MSGenerator()
            : this(0, 5000)
        {

        }

        public UIMF_MSGenerator(double minMZ, double maxMZ)
        {
            Check.Require(minMZ <= maxMZ, "MS Generator failed. MinMZ must be less than or equal to maxMZ");
            //Check.Require(minMZ >=10,"MS Generator failed. MinMZ should be equal or greater than 10. This is due to a problem in the UIMFLibrary that sometimes returns m/z values of 0.000. This will be fixed later.");
            this.MinMZ = minMZ;
            this.MaxMZ = maxMZ;
        }


        public override double MinMZ
        {
            get
            {
                return base.MinMZ;
            }
            set
            {
                if (value < 10)
                {
                    //throw new Exception("MS Generator failed. MinMZ should be greater than 10.\nIf using the xml parameter file, see the 'UseMZRange' and 'MinMZ' parameters.\nThis deficiency will be fixed later.");
                }
                base.MinMZ = value;
            }
        }


        public override void GenerateMS(Run run)
        {
            Check.Require(run is UIMFRun, "UIMF_MSGenerator can only be used with UIMF files");
            UIMFRun uimfRun = (UIMFRun)(run);
            Check.Require(uimfRun.CurrentFrameSet != null, "Cannot generate MS. Target FrameSet ('CurrentFrameSet') has not been assigned to the Run");
            Check.Require(uimfRun.CurrentIMSScanSet != null, "Cannot generate MS. Target ScanSet ('CurrentScanSet') has not been assigned to the Run");
            
            uimfRun.GetMassSpectrum(uimfRun.CurrentFrameSet, uimfRun.CurrentIMSScanSet, this.MinMZ, this.MaxMZ);

            if (uimfRun.XYData.Xvalues == null || uimfRun.XYData.Xvalues.Length == 0)
            {
                uimfRun.XYData.Xvalues = new double[1];
                uimfRun.XYData.Yvalues = new double[1];
            }

        }

    }
}
