using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Workflows.Backend.Results
{
    public class TopDownTargetedResultDTO : UnlabelledTargetedResultDTO
    {
        public HashSet<int> PrsmList { get; set; }
        public List<int> ChargeStateList { get; set; }
        public float Quantitation { get; set; }

        public string ProteinName { get; set; }
        public double ProteinMass { get; set; }
        public string PeptideSequence { get; set; }
        public int MostAbundantChargeState { get; set; }
        public float ChromPeakSelectedHeight { get; set; }
    }
}
