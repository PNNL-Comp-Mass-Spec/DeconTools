using System.Collections.Generic;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.DTO
{
    public class IsosResultStats
    {
        public List<IsosResult> Results { get; set; }

        public IsosResultStats(List<IsosResult>isosResults)
        {
            Results = isosResults;

        }

        public double FitAverage { get; set; }
        public double FitStdDev { get; set; }
        public int Count { get; set; }

        public string Description { get; set; }

    }
}
