using DeconTools.Backend.Utilities;

namespace DeconTools.Workflows.Backend.Utilities.IqCodeParser
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
