using System.Collections.Generic;
using System.Linq;

namespace DeconTools.Workflows.Backend.Core
{
	public class TopDownIqTarget:IqTarget
	{
		public TopDownIqTarget ()
		{
			
		}

		public TopDownIqTarget(IqWorkflow workflow) : base(workflow)
		{

		}

		public List<double> PTMList { get; set; } 

		/// <summary>
		/// Removes redundant charge state targets and adds in missing targets within range. 
		/// </summary>
		public override void RefineChildTargets()
		{
			List<int> chargeStates= new List<int>();
			var children = ChildTargets();
			var removeList = new List<IqTarget>();

			foreach (IqChargeStateTarget child in children)
			{
				if (!chargeStates.Contains(child.ChargeState))
				{
					chargeStates.Add(child.ChargeState);
				}
				else
				{
					removeList.Add(child);
				}
			}
			foreach (IqTarget removal in removeList)
			{
				RemoveTarget(removal);
			}

			chargeStates.Sort();
			int minCharge = chargeStates.Min() - 3;
			int maxCharge = chargeStates.Max() + 3;

			for (int charge = minCharge; charge <= maxCharge; charge++)
			{
				if (!chargeStates.Contains(charge))
				{
					IqTarget newCharge = new IqChargeStateTarget();
					newCharge.ChargeState = charge;
					AddTarget(newCharge);
				}
			}
			_childTargets.Sort((target1, target2) => target1.ChargeState.CompareTo(target2.ChargeState));
		}

		/// <summary>
		/// Sets the childrens code, NET, and other values based on its parent.
		/// Removes children targets if out of MZ range.
		/// </summary>
		public void SetChildrenFromParent(double minMZ = 400, double maxMZ = 1500)
		{
			var children = ChildTargets();
			var removalList = new List<IqTarget>();
			foreach (IqChargeStateTarget child in children)
			{
				child.ID = ID;
				child.MonoMassTheor = MonoMassTheor;
				child.EmpiricalFormula = EmpiricalFormula;
				child.ElutionTimeTheor = ElutionTimeTheor;
				child.Code = Code;
				child.MZTheor = child.MonoMassTheor/child.ChargeState + DeconTools.Backend.Globals.PROTON_MASS;

				if (child.MZTheor < 400 || child.MZTheor > 1500)
				{
					removalList.Add(child);
				}
			}

			foreach (IqTarget remove in removalList)
			{
				RemoveTarget(remove);
			}
		}


		public void SortChildTargetsByCharge()
		{
			_childTargets.Sort((target1, target2) => target1.ChargeState.CompareTo(target2.ChargeState));
		}
	}
}
