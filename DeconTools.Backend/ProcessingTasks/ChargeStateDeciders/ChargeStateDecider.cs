using System.Collections.Generic;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.ChargeStateDeciders
{
    public abstract class ChargeStateDecider
    {
        public abstract IsotopicProfile DetermineCorrectIsotopicProfile(List<IsotopicProfile> potentialIsotopicProfiles);
    }
}
