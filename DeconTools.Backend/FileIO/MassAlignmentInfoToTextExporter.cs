using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MultiAlignEngine.Alignment;
using DeconTools.Utilities;
using System.IO;

namespace DeconTools.Backend.FileIO
{
    public class MassAlignmentInfoToTextExporter : TextFileExporter<KeyValuePair<int, float>>
    {

        #region Constructors
        public MassAlignmentInfoToTextExporter(string exportFilename) : base(exportFilename) { }
        #endregion

      
        public void ExportAlignmentInfo(clsAlignmentFunction alignmentInfo)
        {
            Check.Assert(!string.IsNullOrEmpty(this.FileName), this.Name + " failed. Illegal filename.");

            Check.Require(alignmentInfo != null, "Alignment object is empty");
            Check.Require(alignmentInfo.marrMassFncMZInput != null && alignmentInfo.marrMassFncMZInput.Length > 0, "Mass alignment data is empty.");



            using (StreamWriter writer = File.AppendText(this.FileName))
            {



                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < alignmentInfo.marrMassFncMZInput.Length; i++)
                {



                    sb = new StringBuilder();
                    sb.Append(alignmentInfo.marrMassFncMZInput[i]);
                    sb.Append(Delimiter);
                    sb.Append(alignmentInfo.marrMassFncMZPPMOutput[i]);
                    sb.Append(Delimiter);
                    sb.Append(alignmentInfo.marrMassFncTimeInput[i]);
                    sb.Append(Delimiter);
                    sb.Append(alignmentInfo.marrMassFncTimePPMOutput[i]);

                    writer.WriteLine(sb.ToString());


                }

                writer.Flush();
                writer.Close();
            }
        }


        protected override string buildResultOutput(KeyValuePair<int, float> scanNetPair)
        {
            throw new NotImplementedException();
        }

        protected override string buildHeaderLine()
        {
            return "mz" + Delimiter + "mzPPMCorrection" + Delimiter + "scan" + Delimiter+ "scanPPMCorrection";
        }
        #region Private Methods

        #endregion

    }
}
