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


        /// <summary>
        /// This generates the MS and also gets the TIC value for this scan (by simply summing the intensities)
        /// </summary>
        /// <param name="resultList"></param>
        public override void GenerateMS(Run run)
        {
            Check.Require(run != null, "MS_Generator failed;  'Run' has not yet been defined");
            Check.Require(!(run is UIMFRun), "MS Generator failed; You tried to use the 'Generic_MS_Generator.' Try using the 'UIMF_MSGenerator' instead");
            Check.Assert(run.CurrentScanSet != null, "MS Generator failed; Reason: run.CurrentScanSet is null");

            if (run.XYData == null)
            {
                run.XYData = new XYData();
            }

          
            run.GetMassSpectrum(run.CurrentScanSet, MinMZ, MaxMZ);

            if (run.XYData.Xvalues == null || run.XYData.Xvalues.Length == 0)
            {
                run.XYData.Xvalues = new double[1];
                run.XYData.Yvalues = new double[1];
            }

            if (IsTICRequested)
            {
                run.CurrentScanSet.TICValue = run.GetTIC(this.MinMZ, this.MaxMZ);
            }
        }

    

    }
}
