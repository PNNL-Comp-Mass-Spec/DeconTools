using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using BrukerDataReader;

namespace DeconTools.Backend.FileIO
{
    public class MassTagTextFileExporter : TextFileExporter<TargetBase>
    {

        #region Constructors
        public MassTagTextFileExporter(string fileName):base(fileName,'\t'){}

        public MassTagTextFileExporter(string fileName, char delimiter):base(fileName,delimiter){}

        #endregion

        protected override string buildResultOutput(TargetBase target)
        {
            StringBuilder sb = new StringBuilder();
            Check.Require(target is MassTag, "Exported result is of the wrong type.");

            MassTag result = (MassTag)target;

            sb.Append(target.ID);
            sb.Append(this.Delimiter);
            sb.Append(target.MonoIsotopicMass);
            sb.Append(this.Delimiter); 
            sb.Append(target.Code);
            sb.Append(this.Delimiter); 
            sb.Append(target.ChargeState);
            sb.Append(this.Delimiter); 
            sb.Append(target.ObsCount);
            sb.Append(this.Delimiter); 
            sb.Append(target.MZ);
            sb.Append(this.Delimiter); 
            sb.Append(target.NormalizedElutionTime);
            sb.Append(this.Delimiter);
            sb.Append(result.RefID);
            sb.Append(this.Delimiter);
            sb.Append(result.ProteinDescription);

            return sb.ToString();
        }

        protected override string buildHeaderLine()
        {
            return "ID\tMonoisotopic_Mass\tSequence\tZ\tObsCount\tMZ\tNET\tRef_ID\tDescription";
        }
    }
}
