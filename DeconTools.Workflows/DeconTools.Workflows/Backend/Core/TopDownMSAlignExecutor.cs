using System.Collections.Generic;
using System.ComponentModel;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Workflows.Backend.FileIO;

namespace DeconTools.Workflows.Backend.Core
{
	public class TopDownMSAlignExecutor:IqExecutor
	{
		private BackgroundWorker _backgroundWorker;

		public MSAlignIqTargetImporter TargetImporter { get; set; }

		private RunFactory _runFactory = new RunFactory();

		private IqTargetUtilities _targetUtilities = new IqTargetUtilities();

		#region constructors

		public TopDownMSAlignExecutor(WorkflowExecutorBaseParameters parameters, Run run) : base(parameters, run)
		{
           
        }

		#endregion



		#region public methods

		public override void LoadAndInitializeTargets(string targetsFilePath)
		{
			if (TargetImporter == null)
            {
                TargetImporter = new MSAlignIqTargetImporter(targetsFilePath);
            }

            Targets = TargetImporter.Import();
	
			foreach (TopDownIqTarget target in Targets)
			{
				setParentNetFromChildren(target);
				_targetUtilities.UpdateTargetMissingInfo(target);
				target.RefineChildTargets();
				target.setChildrenFromParent();
			}
		}

		/// <summary>
		/// Sets the parents NET value based on the scan numbers observed in the children.
		/// </summary>
		public void setParentNetFromChildren(IqTarget target)
		{
			var children = target.ChildTargets();
			List<int> scanList = new List<int>();
			foreach (IqChargeStateTarget chargeStateTarget in children)
			{
				scanList.Add(chargeStateTarget.ObservedScan);
			}
			scanList.Sort();
			target.ElutionTimeTheor = Run.GetNETValueForScan(scanList[scanList.Count / 2]);
		}

		#endregion

		#region private methods
		#endregion

	}
}
