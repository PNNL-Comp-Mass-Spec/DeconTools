using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    public sealed class UIMF_MSGenerator : MSGenerator
    {
        public UIMF_MSGenerator()
            : this(0, 5000)
        {
        }

        public UIMF_MSGenerator(double minMZ, double maxMZ)
        {
            Check.Require(minMZ <= maxMZ, "MS Generator failed. MinMZ must be less than or equal to maxMZ");
            //Check.Require(minMZ >=10,"MS Generator failed. MinMZ should be equal or greater than 10. This is due to a problem in the UIMFLibrary that sometimes returns m/z values of 0.000. This will be fixed later.");
            MinMZ = minMZ;
            MaxMZ = maxMZ;
        }

        public override double MinMZ
        {
            get => base.MinMZ;
            set
            {
                if (value < 10)
                {
                    //throw new Exception("MS Generator failed. MinMZ should be greater than 10.\nIf using the xml parameter file, see the 'UseMZRange' and 'MinMZ' parameters.\nThis deficiency will be fixed later.");
                }
                base.MinMZ = value;
            }
        }

        public override void Execute(ResultCollection resultList)
        {
            var uimfRun = (UIMFRun)resultList.Run;

            resultList.Run.XYData = GenerateMS(resultList.Run, resultList.Run.CurrentScanSet, uimfRun.CurrentIMSScanSet);
        }

        public override XYData GenerateMS(Run run, ScanSet lcScanSet, ScanSet imsScanSet = null)
        {
            Check.Require(run is UIMFRun, "UIMF_MSGenerator can only be used with UIMF files");
            var uimfRun = (UIMFRun)(run);

            if (lcScanSet == null || imsScanSet == null)
            {
                return null;
            }

            var xyData = uimfRun.GetMassSpectrum(lcScanSet, imsScanSet, MinMZ, MaxMZ);

            if (xyData.Xvalues == null || xyData.Xvalues.Length == 0)
            {
                xyData.Xvalues = new double[1];
                xyData.Yvalues = new double[1];
            }

            return xyData;
        }
    }
}
