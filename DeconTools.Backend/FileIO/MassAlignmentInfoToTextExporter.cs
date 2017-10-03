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



            using (var writer = File.AppendText(this.FileName))
            {



                var data = new List<string>();

                for (var i = 0; i < alignmentInfo.marrMassFncMZInput.Length; i++)
                {
                    data.Clear();
                    data.Add(DblToString(alignmentInfo.marrMassFncMZInput[i], 5));
                    data.Add(DblToString(alignmentInfo.marrMassFncMZPPMOutput[i], 5));
                    data.Add(DblToString(alignmentInfo.marrMassFncTimeInput[i], 5));
                    data.Add(DblToString(alignmentInfo.marrMassFncTimePPMOutput[i], 5));

                    writer.WriteLine(string.Join(Delimiter.ToString(), data));

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
            var data = new List<string>
            {
                "mz",
                "mzPPMCorrection",
                "scan",
                "scanPPMCorrection"
            };

            return string.Join(Delimiter.ToString(), data);

        }
        #region Private Methods

        #endregion

    }
}
