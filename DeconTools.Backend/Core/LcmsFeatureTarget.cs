using System;

namespace DeconTools.Backend.Core
{
    public class LcmsFeatureTarget : TargetBase
    {

        public LcmsFeatureTarget(LcmsFeatureTarget target) : base(target)
        {
            FeatureToMassTagID = target.FeatureToMassTagID;
        }

        public LcmsFeatureTarget()
        {

        }

        /// <summary>
        /// ID of the massTag to which the Feature was matched.
        /// </summary>
        public int FeatureToMassTagID { get; set; }

        /// <summary>
        /// LCMS targets are usually associated with a specific dataset
        /// </summary>
        public string DatabaseName { get; set; }

        public override string ToString()
        {
            return ID + "; " + ScanLCTarget + "; " + MonoIsotopicMass.ToString("0.0000") + "; " + ChargeState;

        }

        public override string GetEmpiricalFormulaFromTargetCode()
        {
            throw new NotImplementedException();
        }
    }
}
