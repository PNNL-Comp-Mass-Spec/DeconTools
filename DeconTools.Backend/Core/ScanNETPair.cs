
using System.Globalization;

namespace DeconTools.Backend.Core
{
    public class ScanNETPair
    {
        public ScanNETPair(double scan, double net)
        {
            Scan = scan;
            NET = net;
        }

        public double Scan { get; set; }
        public double NET { get; set; }

        public override string ToString()
        {
            return (Scan.ToString(CultureInfo.InvariantCulture) + "; " + NET.ToString("0.0000"));
        }
    }
}
