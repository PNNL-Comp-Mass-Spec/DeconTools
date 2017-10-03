using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core.Results;

namespace DeconTools.Backend.FileIO.TargetedResultFileIO
{
    public class O16O18TargetedResultToTextExporter : TargetedResultToTextExporter
    {

        public O16O18TargetedResultToTextExporter(string filename)
            : base(filename)
        {

        }


        #region Private Methods

        protected override string addAdditionalInfo(Core.Results.TargetedResult result)
        {

            O16O18TargetedResult o16o18result = (O16O18TargetedResult)result;

            var data = new List<string>
            {
                o16o18result.IntensityI0,
                o16o18result.IntensityI2,
                o16o18result.IntensityI4,
                o16o18result.IntensityI4Adjusted,
                o16o18result.Ratio.ToString("0.0000")
            };
            
            return string.Join(Delimiter.ToString(), data);

        }


        protected override string buildHeaderLine()
        {
            var data = new List<string>
            {
                base.buildHeaderLine(),
                "IntensityTheorI0",
                "IntensityTheorI2",
                "IntensityTheorI4",
                "IntensityI0",
                "IntensityI2",
                "IntensityI4",
                "IntensityI4Adjusted",
                "Ratio"
            };

            return string.Join(Delimiter.ToString(), data);

        }

        #endregion

    }
}
