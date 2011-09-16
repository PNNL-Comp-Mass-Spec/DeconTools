using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Algorithms.Quantifiers;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.Quantifiers
{
    public class O16O18QuantifierTask:Task
    {

        BasicO16O18Quantifier _quantifier;

        #region Constructors
        public O16O18QuantifierTask()
        {
            _quantifier = new BasicO16O18Quantifier();
            


        }

        #endregion

      

        public override void Execute(ResultCollection resultColl)
        {
            TargetedResultBase result = resultColl.GetTargetedResult(resultColl.Run.CurrentMassTag);

            Check.Require(result is O16O18TargetedResultObject, "O16O18 quantifier failed. Result is not of the O16O18 type.");

            O16O18TargetedResultObject o16o18result = (O16O18TargetedResultObject)result;

            o16o18result.RatioO16O18 = _quantifier.GetAdjusted_I0_I4_YeoRatio(result.IsotopicProfile, result.Target.IsotopicProfile);
            o16o18result.IntensityI4Adjusted = _quantifier.adjustedI4Intensity;

        }
    }
}
