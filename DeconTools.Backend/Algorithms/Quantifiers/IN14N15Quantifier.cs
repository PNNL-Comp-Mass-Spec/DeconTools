
namespace DeconTools.Backend.Algorithms.Quantifiers
{
    public abstract class N14N15Quantifier
    {
        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods
        public abstract double GetRatio(double[] xVals, double[] yVals,
            Core.IsotopicProfile iso1, Core.IsotopicProfile iso2,
            double backgroundIntensity);

        #endregion

        #region Private Methods
        #endregion
    }
}
