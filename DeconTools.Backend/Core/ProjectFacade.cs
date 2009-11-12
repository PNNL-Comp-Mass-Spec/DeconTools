using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Utilities;

namespace DeconTools.Backend.Core
{
    public class ProjectFacade
    {
        public ProjectFacade()
        {

        }


        public int GetTotalFeaturesFromScanResultCollection()
        {
            Check.Require(Project.getInstance().RunCollection != null, "Cannot get total features. Project's runCollection is null");

            int totalFeatures=0;
            foreach (Run run in Project.getInstance().RunCollection)
            {
                if (run.ResultCollection == null || run.ResultCollection.ScanResultList == null) continue;

                foreach (ScanResult scanResult in run.ResultCollection.ScanResultList)
                {
                    totalFeatures += scanResult.NumIsotopicProfiles;
                    
                }
                
                
            }
            return totalFeatures;
        }

    }
}
