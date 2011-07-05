using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.FileIO
{
    public class MassTagTextFileExporter : TextFileExporter<MassTag>
    {

        #region Constructors
        public MassTagTextFileExporter(string fileName):base(fileName,'\t'){}

        public MassTagTextFileExporter(string fileName, char delimiter):base(fileName,delimiter){}

        #endregion

        protected override string buildResultOutput(MassTag result)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(result.ID);
            sb.Append(this.Delimiter);
            sb.Append(result.MonoIsotopicMass);
            sb.Append(this.Delimiter); 
            sb.Append(result.PeptideSequence);
            sb.Append(this.Delimiter); 
            sb.Append(result.ChargeState);
            sb.Append(this.Delimiter); 
            sb.Append(result.ObsCount);
            sb.Append(this.Delimiter); 
            sb.Append(result.MZ);
            sb.Append(this.Delimiter); 
            sb.Append(result.NETVal);
            sb.Append(this.Delimiter);
            sb.Append(result.RefID);
            sb.Append(this.Delimiter); 
            sb.Append(result.ProteinDescription);

            return sb.ToString();
        }

        protected override string buildHeaderLine()
        {
            return "ID\tMonoisotopic_Mass\tSequence\tZ\tObsCount\tMZ\tNET\tRef_ID\tDescription\n";
        }
    }
}
