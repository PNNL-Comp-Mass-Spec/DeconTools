
namespace DeconTools.Backend.Parameters
{
    public class MSGeneratorParameters 
    {

        public MSGeneratorParameters()
            : this(0, 10000)
        {


        }

        public MSGeneratorParameters(double minMZ, double maxMZ)
        {
            MinMZ = minMZ;
            MaxMZ = maxMZ;

        }

        public bool UseMZRange { get; set; }

        public bool GetTicInfo { get; set; }

        public double MinMZ { get; set; }

        public double MaxMZ { get; set; }

        
        public bool SumAllSpectra { get; set; }

        public bool SumSpectraAcrossLC { get; set; }

        public bool SumSpectraAcrossIMS { get; set; }

        public int NumLCScansToSum { get; set; }

        public int NumImsScansToSum { get; set; }

        
        public bool UseLCScanRange { get; set; }

        public int MinLCScan { get; set; }

        public int MaxLCScan { get; set; }


        public int MinIMSScan { get; set; }

        public int MaxIMSScan { get; set; }


    }
}
