using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Core
{
    public abstract class Task
    {

        public abstract void Execute(ResultCollection resultColl);

        public string Name;
        
        public virtual void Cleanup()
        {
            return;
        }


    }
}
