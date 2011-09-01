
namespace DeconTools.Backend.Core
{
    public class O16O18IsosResult:IsosResult
    {
      

        public O16O18IsosResult(Run run, ScanSet scanSet)
        {
            this.Run = run;
            this.ScanSet = scanSet;
        }

        public float MonoPlus4Abundance { get; set; }
        public float MonoMinus4Abundance { get; set; }
        public float MonoPlus2Abundance { get; set; }

    }
}
