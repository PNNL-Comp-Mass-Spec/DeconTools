using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ZeroFillers
{
    public class BasicZeroFiller<TXVal, TYVal>
    {
        TXVal[] xvals;
        TYVal[] yvals;

        public BasicZeroFiller(TXVal[] xvals, TYVal[] yvals)
        {
    
            this.xvals = xvals;
            this.yvals = yvals;

        }

        public int ZeroFill()
        {
            Check.Require(xvals != null && yvals != null, "Input data is null");
            Check.Require(xvals.Length == yvals.Length, "X and Y data arrays must have same length");

            if (xvals.Length == 0) return 0;    
            if (xvals.Length == 1) return 0;    //data contains one point..  so exit

            //double minimumDiff =(double)xvals[1] - (double)xvals[0];

            for (int i = 2; i < xvals.Length; i++)
            {
                //double currentDiff = (xvals[i] - xvals[i - 1]);
               // if (currentDiff < minimumDiff)
              //  {
              //      minimumDiff = currentDiff;
              //  }
            }
            return 0;
            


            
            


        }
    }
}
