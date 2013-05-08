
namespace DeconTools.Workflows.Backend.Core
{
    public class IqChargeStateTarget:IqTarget
    {
        public IqChargeStateTarget()
        {
	        ObservedScan = -1;
	        AlternateID = -1;
        }

        public IqChargeStateTarget(IqWorkflow workflow) : base(workflow)
        {
	        ObservedScan = -1;
	        AlternateID = -1;
        }

		/// <summary>
		/// Strictly used during the data importing process. Parent uses this value to calculate overall NET for the sequence. Once a NET value is calculated, this value is not needed anymore.
		/// </summary>
	    public int ObservedScan;

	    public int AlternateID;
    }
}
