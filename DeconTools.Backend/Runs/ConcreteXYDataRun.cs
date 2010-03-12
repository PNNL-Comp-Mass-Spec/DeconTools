using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Runs
{
    public class ConcreteXYDataRun:XYDataRun
    {

        public ConcreteXYDataRun(double[]xvals, double[] yvals)
        {
            this.XYData.Xvalues = xvals;
            this.XYData.Yvalues = yvals;

        }


        
        public override double GetTime(int scanNum)
        {
            return -1;
        }

 

        public override void GetMassSpectrum(DeconTools.Backend.Core.ScanSet scanset, double minMZ, double maxMZ)
        {
            //do nothing...   constructor requires that XY values are inputted
        }

  
    }
}
