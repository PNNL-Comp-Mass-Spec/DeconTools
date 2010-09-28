using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Runs.CalibrationData
{
    public class BrukerCalibrationData : CalibrationData
    {
        #region Constructors
        #endregion

        #region Properties
        public double ML1 { get; set; }
        public double ML2 { get; set; }
        public double SW_h { get; set; }
        public int ByteOrder { get; set; }
        public int TD { get; set; }
        public double FR_Low { get; set; }
        public int NF { get; set; }



        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        public override void Display()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ML1 = \t");
            sb.Append(ML1);
            sb.Append(Environment.NewLine);
            sb.Append("ML2 = \t");
            sb.Append(ML2);
            sb.Append(Environment.NewLine);
            sb.Append("SW_h = \t");
            sb.Append(SW_h);
            sb.Append(Environment.NewLine);
            sb.Append("TD = \t");
            sb.Append(TD);
            sb.Append(Environment.NewLine);
            sb.Append("FR_Low = \t");
            sb.Append(FR_Low);
            sb.Append(Environment.NewLine);
            sb.Append("NF = \t");
            sb.Append(NF);
            sb.Append(Environment.NewLine);
            sb.Append("ByteOrder = \t");
            sb.Append(ByteOrder);
            sb.Append(Environment.NewLine);

            Console.WriteLine(sb.ToString());
        }
    }
}
