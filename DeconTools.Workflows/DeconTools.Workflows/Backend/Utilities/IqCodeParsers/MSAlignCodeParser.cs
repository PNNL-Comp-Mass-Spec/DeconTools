using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Utilities;

namespace DeconTools.Workflows.Backend.Utilities.IqCodeParsers
{
    public class MSAlignCodeParser : IqCodeParser
    {
        public MSAlignCodeParser()
        {
            PTMExpression = @"([-]?[0-9]+\.[0-9]+)";
            SequenceExpression = @"[.\(\[\]]";
            PeptideUtils = new PeptideUtils();
        }
    }
}
