using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks
{
    public abstract class I_MSGenerator : Task
    {


        public bool IsTICRequested { get; set; }

        private double minMZ;

        public virtual double MinMZ
        {
            get { return minMZ; }
            set 
            {
                 minMZ = value; 
            }
        }

        private double maxMZ;

        public virtual double MaxMZ
        {
            get { return maxMZ; }
            set { maxMZ = value; }
        }

        public abstract void GenerateMS(Run run);

        protected abstract void createNewScanResult(ResultCollection resultList, ScanSet scanSet);

        public override void Execute(ResultCollection resultList)
        {

             GenerateMS(resultList.Run);

        }
    }
}
