using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.DTO
{
    public class IsosResultStats
    {

        private List<IsosResult> results;

        public List<IsosResult> Results
        {
            get { return results; }
            set { results = value; }
        }

        public IsosResultStats(List<IsosResult>isosResults)
        {
            results = isosResults;

        }

        public double FitAverage { get; set; }
        public double FitStdDev { get; set; }
        public int Count { get; set; }

        public string Description { get; set; }

    }
}
