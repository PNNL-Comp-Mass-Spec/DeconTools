using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using DeconTools.Backend.Data;


namespace DeconTools.Backend.ProcessingTasks
{
    public class ScanResultUpdater : Task
    {



        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList != null, "ResultCollection is null");
            Check.Require(resultList.Run != null, "Run is null");


            ScanResultFactory srf = new ScanResultFactory();
            ScanResult scanresult =  srf.CreateScanResult(resultList.Run);

            if (scanresult != null)
            {
                resultList.ScanResultList.Add(scanresult);
            }

        }
    }
}
