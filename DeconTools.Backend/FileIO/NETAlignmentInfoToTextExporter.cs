using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MultiAlignEngine.Alignment;
using DeconTools.Backend.Core;
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
            Check.Assert(this.FileName != null && this.FileName.Length > 0, this.Name + " failed. Illegal filename.");
            using (StreamWriter writer = File.AppendText(this.FileName))
            {



                //StringBuilder sb = new StringBuilder();

                for (int i = 0; i < alignmentInfo.marrNETFncTimeInput.Length; i++)
                {

                    writer.WriteLine(alignmentInfo.marrNETFncTimeInput[i] + "\t" + alignmentInfo.marrNETFncNETOutput[i]);

                    //    sb = new StringBuilder();
                    //    sb.Append(alignmentInfo.marrNETFncTimeInput[i]);
                    //    sb.Append(Delimiter);
                    //    sb.Append(alignmentInfo.marrNETFncNETOutput[i]);

                    //    writer.WriteLine(sb.ToString());


                }

                writer.Flush();
                writer.Close();
            }
        }


        protected override string buildResultOutput(KeyValuePair<int, float> scanNetPair)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(scanNetPair.Key);
            sb.Append(Delimiter);
            sb.Append(scanNetPair.Value);

            return sb.ToString();
        }

        protected override string buildHeaderLine()
        {
            return "scan" + Delimiter + "NET";
        }
    }
}
