using System.Collections.Generic;
using DeconTools.Workflows.Backend.Core;

namespace DeconTools.Workflows.Backend.FileIO
{
    public class BasicIqTargetImporter:IqTargetImporter
    {

        public BasicIqTargetImporter(string filename)
        {
            Filename = filename;
        }


        protected override IqTarget ConvertTextToIqTarget(List<string> processedRow)
        {

            IqTarget target = new IqChargeStateTarget();

            target.ID = ParseIntField(processedRow, TargetIDHeaders, 0);
            target.EmpiricalFormula = ParseStringField(processedRow, EmpiricalFormulaHeaders, string.Empty);
            target.Code = ParseStringField(processedRow, CodeHeaders, "");
            target.ElutionTimeTheor = ParseDoubleField(processedRow, NETHeaders, 0);

            return target;
        }

       
    }
}
