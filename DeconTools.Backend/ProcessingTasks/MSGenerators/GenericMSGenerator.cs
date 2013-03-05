using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.MSGenerators
{
    public class GenericMSGenerator : MSGenerator
    {



        public GenericMSGenerator()
            : this(0, 5000)
        {

        }




        public GenericMSGenerator(double minMZ, double maxMZ)
        {
            this.MinMZ = minMZ;
            this.MaxMZ = maxMZ;

            this.IsTICRequested = true;

        }


       
        public override XYData GenerateMS(Run run, ScanSet lcScanset, ScanSet imsscanset=null)
        {
            Check.Require(run != null, "MS_Generator failed;  'Run' has not yet been defined");
            Check.Require(!(run is UIMFRun), "MS Generator failed; You tried to use the 'Generic_MS_Generator.' Try using the 'UIMF_MSGenerator' instead");

            if (lcScanset == null) return null;

            XYData xydata = run.GetMassSpectrum(lcScanset, MinMZ, MaxMZ);

            //TODO: this doesn't really belong here!
            if (IsTICRequested)
            {
                lcScanset.TICValue = GetTIC(xydata,  this.MinMZ, this.MaxMZ);
            }

            return xydata;
        }



    }
}
