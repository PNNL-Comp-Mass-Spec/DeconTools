using System;
using System.Collections.Generic;
using MultiAlignEngine.Alignment;
using DeconTools.Utilities;
using System.IO;

namespace DeconTools.Backend.FileIO
{
    public class NETAlignmentInfoToTextExporter : TextFileExporter<KeyValuePair<int, float>>
    {


        #region Constructors
        public NETAlignmentInfoToTextExporter(string exportFilename) : base(exportFilename) { }

        #endregion

        public override void ExportResults(IEnumerable<KeyValuePair<int, float>> resultList)
        {
            throw new NotImplementedException("Note to developer. Use the alternate Export.");
        }

        public void ExportAlignmentInfo(clsAlignmentFunction alignmentInfo)
        {
            Check.Assert(!string.IsNullOrEmpty(FileName), Name + " failed. Illegal filename.");
            using (var writer = File.AppendText(FileName))
            {

                for (var i = 0; i < alignmentInfo.marrNETFncTimeInput.Length; i++)
                {

                    writer.WriteLine(alignmentInfo.marrNETFncTimeInput[i] + Delimiter + alignmentInfo.marrNETFncNETOutput[i]);
                }

                writer.Flush();
                writer.Close();
            }
        }

        protected override string buildResultOutput(KeyValuePair<int, float> scanNetPair)
        {
            return scanNetPair.Key.ToString() + Delimiter + DblToString(scanNetPair.Value, 5);
        }

        protected override string buildHeaderLine()
        {
            return "scan" + Delimiter + "NET";
        }
    }
}
