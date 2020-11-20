using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.MSGenerators
{
    public sealed class GenericMSGenerator : MSGenerator
    {
        public GenericMSGenerator()
            : this(0, 5000, true)
        {
        }

        public GenericMSGenerator(double minMZ, double maxMZ, bool isTicRequested)
        {
            MinMZ = minMZ;
            MaxMZ = maxMZ;

            IsTICRequested = isTicRequested;
        }

        public override XYData GenerateMS(Run run, ScanSet lcScanSet, ScanSet imsScanSet = null)
        {
            Check.Require(run != null, "MS_Generator failed;  'Run' has not yet been defined");
            if (run == null)
                return null;

            Check.Require(!(run is UIMFRun), "MS Generator failed; You tried to use the 'Generic_MS_Generator.' Try using the 'UIMF_MSGenerator' instead");

            if (lcScanSet == null) return null;

            var xyData = run.GetMassSpectrum(lcScanSet, MinMZ, MaxMZ);

            //TODO: this doesn't really belong here!
            if (IsTICRequested)
            {
                lcScanSet.TICValue = GetTIC(xyData, MinMZ, MaxMZ);
            }

            return xyData;
        }
    }
}


