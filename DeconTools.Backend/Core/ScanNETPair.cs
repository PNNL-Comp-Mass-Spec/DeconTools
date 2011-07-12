
namespace DeconTools.Backend.Core
{
    public class ScanNETPair
    {
        public ScanNETPair(float scan, float net)
        {
            Scan = scan;
            NET = net;
        }

        public float Scan {get;set;}
        public float NET { get; set; }


        public override string ToString()
        {
            return (Scan.ToString() + "; " + NET.ToString("0.0000"));
        }
    }
}
