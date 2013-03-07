using DeconTools.Backend.Utilities;

namespace DeconTools.Workflows.Backend.Utilities.IqCodeParser
{
	public class MSGFCodeParser : IqCodeParser
	{
		public MSGFCodeParser()
		{
			PTMExpression = @"([+-]([0-9]+)\.?([0-9]+))";
			SequenceExpression = @"[+-][0-9]+\.?[0-9]+";
			PeptideUtils = new PeptideUtils();
		}
	}
}
