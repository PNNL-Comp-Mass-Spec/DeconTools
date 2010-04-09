using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator
{
    public abstract class ITheorFeatureGenerator:Task
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        public abstract void LoadRunRelatedInfo(ResultCollection results);
        public abstract void GenerateTheorFeature(MassTag mt);
        #endregion

        #region Private Methods
        #endregion
        public override void Execute(ResultCollection results)
        {
            Check.Require(results.Run.CurrentMassTag != null, "Theoretical feature generator failed. No target mass tag was provided");

            LoadRunRelatedInfo(results);
            
            GenerateTheorFeature(results.Run.CurrentMassTag);
        }
    }
}
