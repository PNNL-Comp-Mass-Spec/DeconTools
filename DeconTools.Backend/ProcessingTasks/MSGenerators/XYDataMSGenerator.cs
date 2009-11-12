using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks
{
    public class XYDataMSGenerator : I_MSGenerator
    {

        public XYDataMSGenerator()
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
