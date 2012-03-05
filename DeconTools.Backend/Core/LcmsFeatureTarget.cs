using System;

namespace DeconTools.Backend.Core
{
    public class LcmsFeatureTarget:TargetBase
    {


        /// <summary>
        /// ID of the massTag to which the Feature was matched.
        /// </summary>
        public int FeatureToMassTagID { get; set; }


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
