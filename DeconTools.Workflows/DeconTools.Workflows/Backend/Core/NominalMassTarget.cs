using System;
using DeconTools.Backend.Core;

namespace DeconTools.Workflows.Backend.Core
{
    public class NominalMassTarget : TargetBase
    {
        private const double RescalingFactor = 0.9995;

        public NominalMassTarget(int nominalMass, short chargeState, int msLevel)
        {
            NominalMass = nominalMass;
            MZ = nominalMass / RescalingFactor;
            ChargeState = chargeState;
            MsLevel = msLevel;
        }

        public int NominalMass { get; private set; }

        public override string GetEmpiricalFormulaFromTargetCode()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("ID: {0}, MZ: {1}, ChargeState: {2}", ID, MZ, ChargeState);
        }
    }
}
