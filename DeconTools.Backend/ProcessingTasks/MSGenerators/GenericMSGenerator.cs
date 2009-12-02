using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Parameters;
using DeconTools.Utilities;
using DeconTools.Backend.Runs;

namespace DeconTools.Backend.ProcessingTasks.MSGenerators
{
    public class GenericMSGenerator : I_MSGenerator
    {

  

        public GenericMSGenerator()
            : this(0, 5000)
        {

        }

        public GenericMSGenerator(double minMZ, double maxMZ)
        {
            this.MinMZ = minMZ;
            this.MaxMZ = maxMZ;
        }

        
        /// <summary>
        /// This generates the MS and also gets the TIC value for this scan (by simply summing the intensities)
        /// </summary>
        /// <param name="resultList"></param>
        public override void GenerateMS(DeconTools.Backend.Core.ResultCollection resultList)
        {
            Check.Require(resultList.Run != null, "MS_Generator failed;  'Run' has not yet been defined");
            Check.Require(!(resultList.Run is UIMFRun), "MS Generator failed; You tried to use the 'Generic_MS_Generator.' Try using the 'UIMF_MSGenerator' instead");
            Check.Assert(resultList.Run.CurrentScanSet!=null,"MS Generator failed; Reason: run.CurrentScanSet is null");
            
            resultList.Run.MSParameters.MinMZ = this.MinMZ;    
            resultList.Run.MSParameters.MaxMZ = this.MaxMZ;
            
            

            bool isScan_MS_only = (resultList.Run.GetMSLevel(resultList.Run.CurrentScanSet.PrimaryScanNumber) == 1);
            if (true)
            {
                resultList.Run.GetMassSpectrum(resultList.Run.CurrentScanSet, this.MinMZ, this.MaxMZ);
 
            }
            else
            {
                
            }

            if (resultList.Run.XYData.Xvalues == null || resultList.Run.XYData.Xvalues.Length == 0)
            {
                resultList.Run.XYData.Xvalues = new double[1];
                resultList.Run.XYData.Yvalues = new double[1];
            }

            resultList.Run.CurrentScanSet.TICValue = resultList.Run.GetTIC(this.MinMZ, this.MaxMZ);

        }
  
        protected override void createNewScanResult(ResultCollection resultList, ScanSet scanSet)
        {
            //resultList.ScanResultList.Add(new StandardScanResult(scanSet));
            //resultList.GetCurrentScanResult().ScanTime = resultList.Run.GetTime(scanSet.PrimaryScanNumber);
            //resultList.GetCurrentScanResult().SpectrumType = resultList.Run.GetMSLevel(scanSet.PrimaryScanNumber);
        }
    }
}
