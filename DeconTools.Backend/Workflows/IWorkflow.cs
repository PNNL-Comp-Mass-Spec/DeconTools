using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Workflows
{
    public interface IWorkflow
    {

        string Name { get; set; }

        #region Public Methods

        void InitializeWorkflow();

        void ExecuteWorkflow(Run run);


        


        #endregion
    }
}
