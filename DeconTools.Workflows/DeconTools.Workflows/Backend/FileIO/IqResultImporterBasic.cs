using System.Collections.Generic;
using DeconTools.Workflows.Backend.Core;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class IqResultImporterBasic : IqResultImporter
    {

        public IqResultImporterBasic(string filename)
            : base(filename)
        {
        }

        protected override void ConvertTextToDataObject(ref IqResult iqResult, List<string> rowData)
        {
            GetBasicIqResultData(ref iqResult, rowData);



        }

        protected override IqResult CreateIqResult()
        {
            IqTarget target = new IqChargeStateTarget();
            var result = new IqResult(target);
            return result;
        }
    }
}
