
namespace DeconTools.Backend.Runs
{
    public sealed class ConcreteXYDataRun:XYDataRun
    {

        public ConcreteXYDataRun(double[]xVals, double[] yVals)
        {
            XYData.Xvalues = xVals;
            XYData.Yvalues = yVals;
        }

        public override double GetTime(int scanNum)
        {
            return -1;
        }

        public override XYData GetMassSpectrum(Core.ScanSet scanSet)
        {
            return XYData;
        }

    }
}
