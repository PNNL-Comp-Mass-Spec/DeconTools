using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks
{
    public class IMF_MSGenerator : I_MSGenerator
    {

        public IMF_MSGenerator()
        {

        }

        
        public override void GenerateMS(ResultCollection resultList)
        {
            //resultList.Run.GetMassSpectrum();
        }

     

        protected override void createNewScanResult(ResultCollection resultList, ScanSet scanSet)
        {
            throw new NotImplementedException();
        }
    }
}
