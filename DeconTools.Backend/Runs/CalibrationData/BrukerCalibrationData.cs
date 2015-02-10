using System;
using System.Text;

namespace DeconTools.Backend.Runs.CalibrationData
{
    public class BrukerCalibrationData : BrukerDataReader.GlobalParameters
    {
   
        #region Properties

        /// <summary>
        /// Only used by BrukerV2Run
        /// </summary>
        public int ByteOrder { get; set; }

        /// <summary>
        /// Only used by BrukerV2Run
        /// </summary>
        public double FR_Low { get; set; }

        /// <summary>
        /// Appears unused
        /// </summary>
        public int NF { get; set; }

        #endregion

        public new void Display()
        {
            base.Display();

            var sb = new StringBuilder();
            sb.Append("FR_Low = " + FR_Low                 + Environment.NewLine);
            sb.Append("NF =     " + NF                     + Environment.NewLine);
            sb.Append("ByteOrder = " + ByteOrder           + Environment.NewLine);

            Console.WriteLine(sb.ToString());
        }
    }
}
