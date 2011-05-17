using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.Smoothers
{
    public class DeconToolsSavitzkyGolaySmoother : ISmoother
    {
        #region Properties
        private int rightParam;

        public int RightParam
        {
            get { return rightParam; }
            set { rightParam = value; }
        }
        private int leftParam;

        public int LeftParam
        {
            get { return leftParam; }
            set { leftParam = value; }
        }
        private int order;

        public int Order
        {
            get { return order; }
            set { order = value; }
        }
        #endregion

        #region Constructors

        public DeconToolsSavitzkyGolaySmoother()
            : this(2, 2, 2)
        {

        }

        public DeconToolsSavitzkyGolaySmoother(int leftParam, int rightParam, int order)
        {
            this.LeftParam = leftParam;
            this.RightParam = rightParam;
            this.Order = order;
        }
        #endregion

        #region Public Methods
        public override XYData Smooth(XYData inputData)
        {
            if (inputData == null)
            {
                return null;
            }

            Check.Require(this.leftParam >= 0, "Savitzky left parameter cannot be negative");
            Check.Require(this.order >= 0, "Savitzky order cannot be negative");
            Check.Require(this.rightParam >= 0, "Savitzky right parameter cannot be negative");

            //Check.Require(resultList.Run != null, "Smoother failed. Problem: Run is null");
            //Check.Require(resultList.Run.XYData != null && resultList.Run.XYData.Xvalues != null &&
            //    resultList.Run.XYData.Yvalues != null && resultList.Run.XYData.Xvalues.Length > 0 &&
            //    resultList.Run.XYData.Yvalues.Length > 0, "Smoother failed. Problem: Empty XY values");

            float[] xvals = new float[1];
            float[] yvals = new float[1];

            inputData.GetXYValuesAsSingles(ref xvals, ref yvals);

            DeconEngine.Utils.SavitzkyGolaySmooth((short)this.leftParam, (short)this.rightParam, (short)this.order, ref xvals, ref yvals);

            XYData outData = new XYData();
            outData.SetXYValues(ref xvals, ref yvals);
            return outData;
        }
        #endregion

    }
}
