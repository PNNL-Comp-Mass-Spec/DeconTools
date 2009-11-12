using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.ProcessingTasks.MSGenerators
{
    public class XYDataMSGenerator : I_MSGenerator 
    {

           public XYDataMSGenerator()
        {
           

        }
        
        public override void GenerateMS(DeconResult result)
        {
            result.Run.GetMassSpectrum();
            
        }





    }
}
