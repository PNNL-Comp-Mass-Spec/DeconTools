using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ZeroFillers
{
    public class DeconToolsZeroFiller : IZeroFiller
    {

        private int maxNumPointsToAdd;

        public int MaxNumPointsToAdd
        {
            get { return maxNumPointsToAdd; }
            set { maxNumPointsToAdd = value; }
        }

        public DeconToolsZeroFiller()
            : this(3)
        {

        }
        public DeconToolsZeroFiller(int maxNumPointsToAdd)
        {
            this.MaxNumPointsToAdd = maxNumPointsToAdd;
        }


        public override void ZeroFill(DeconTools.Backend.Core.ResultCollection resultList)
        {
            Check.Require(maxNumPointsToAdd >= 0, "Zerofiller's maxNumPointsToAdd cannot be a negative number");
            Check.Require(resultList.Run != null, "Zerofiller failed. Problem: Run is null");
            Check.Require(resultList.Run.XYData != null && resultList.Run.XYData.Xvalues != null &&
                resultList.Run.XYData.Yvalues != null && resultList.Run.XYData.Xvalues.Length > 0 &&
                resultList.Run.XYData.Yvalues.Length > 0, "Zerofiller failed. Problem: Empty XY values");

            float[] xvals = new float[1];
            float[] yvals = new float[1];

            resultList.Run.XYData.GetXYValuesAsSingles(ref xvals, ref yvals);
            
            DeconEngine.Utils.ZeroFillUnevenData(ref xvals, ref yvals, this.maxNumPointsToAdd);
            
            resultList.Run.XYData.SetXYValues(ref xvals, ref yvals);
        }
    }
}
