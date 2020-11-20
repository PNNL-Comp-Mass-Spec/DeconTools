using System.Collections.Generic;

namespace DeconTools.Workflows.Backend.Results
{
    public class DeuteratedTargetedResultDTO : UnlabeledTargetedResultDTO
    {
        #region Properties
        //public float IntensityI2 { get; set; }
        //public float IntensityI4 { get; set; }
        //public double IntensityI4Adjusted { get; set; }
        //public double Ratio { get; set; }

        //public float IntensityTheorI0 { get; set; }
        //public float IntensityTheorI2 { get; set; }
        //public float IntensityTheorI4 { get; set; }

        public double HydrogenI0 { get; set; }
        public double HydrogenI1 { get; set; }
        public double HydrogenI2 { get; set; }
        public double HydrogenI3 { get; set; }
        public double HydrogenI4 { get; set; }
        public double DeuteriumI0 { get; set; }
        public double DeuteriumI1 { get; set; }
        public double DeuteriumI2 { get; set; }
        public double DeuteriumI3 { get; set; }
        public double DeuteriumI4 { get; set; }
        public double TheoryI0 { get; set; }
        public double TheoryI1 { get; set; }
        public double TheoryI2 { get; set; }
        public double TheoryI3 { get; set; }
        public double TheoryI4 { get; set; }
        public double RawI0 { get; set; }
        public double RawI1 { get; set; }
        public double RawI2 { get; set; }
        public double RawI3 { get; set; }
        public double RawI4 { get; set; }
        public double LabelingEfficiency {get;set;}
        public double RatioDH { get; set; }
        public double IntensityI0HydrogenMono { get; set; }

        /// <summary>
        /// area under the parabolic LC fit
        /// </summary>
        public double IntegratedLcAbundance { get; set; }

        #endregion

        public override string ToStringWithDetailsAsRow()
        {
            var data = new List<string>
            {
                TargetID.ToString(),
                ChargeState.ToString(),
                ScanLC.ToString(),
                ScanLCStart.ToString(),
                ScanLCEnd.ToString(),
                NET.ToString("0.0000"),
                NumChromPeaksWithinTol.ToString(),
                MonoMass.ToString("0.00000"),
                MonoMZ.ToString("0.00000"),
                FitScore.ToString("0.0000"),
                IScore.ToString("0.0000"),
                TheoryI0.ToString("0.000"),
                TheoryI1.ToString("0.000"),
                TheoryI2.ToString("0.000"),
                TheoryI3.ToString("0.000"),
                TheoryI4.ToString("0.000"),
                HydrogenI0.ToString("0.000"),
                HydrogenI1.ToString("0.000"),
                HydrogenI2.ToString("0.000"),
                HydrogenI3.ToString("0.000"),
                HydrogenI4.ToString("0.000"),
                DeuteriumI0.ToString("0.000"),
                DeuteriumI1.ToString("0.000"),
                DeuteriumI2.ToString("0.000"),
                DeuteriumI3.ToString("0.000"),
                DeuteriumI4.ToString("0.000"),
                RawI0.ToString("0.000"),
                RawI1.ToString("0.000"),
                RawI2.ToString("0.000"),
                RawI3.ToString("0.000"),
                RawI4.ToString("0.000"),
                LabelingEfficiency.ToString("0.000"),
                RatioDH.ToString("0.0000"),
                IntegratedLcAbundance.ToString("0.0")
            };

            return string.Join("\t", data);
        }
    }
}
