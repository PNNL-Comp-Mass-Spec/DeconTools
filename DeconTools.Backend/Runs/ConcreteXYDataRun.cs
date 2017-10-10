
namespace DeconTools.Backend.Runs
{
    public sealed class ConcreteXYDataRun:XYDataRun
    {

        public ConcreteXYDataRun(double[]xvals, double[] yvals)
        {
            XYData.Xvalues = xvals;
            XYData.Yvalues = yvals;
        }

        public override double GetTime(int scanNum)
        {
            return -1;
        }

        public override XYData GetMassSpectrum(Core.ScanSet scanset)
        {
            return XYData;
        }

    }
}
