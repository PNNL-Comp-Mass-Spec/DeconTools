using DeconTools.Workflows.Backend.Core.ChromPeakSelection;

namespace DeconTools.Workflows.Backend.Core
{
    public class TopDownIqResult : IqResult
    {
        public TopDownIqResult(IqTarget target)
            : base(target)
        {
            ChargeCorrelationData = new ChargeCorrelationData();
            SelectedCorrelationGroup = new ChargeCorrelationItem();
        }

        public ChargeCorrelationData ChargeCorrelationData { get; set; }

        public ChargeCorrelationItem SelectedCorrelationGroup { get; set; }
    }
}
