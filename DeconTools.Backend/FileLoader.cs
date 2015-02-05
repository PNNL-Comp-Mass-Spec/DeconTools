using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend
{

#if !Disable_DeconToolsV2
    public class FileLoader
    {

        public void Execute(ResultCollection resultList)
        {
            DeconToolsV2.Readers.clsRawData reader = new DeconToolsV2.Readers.clsRawData();
            
        }
    }

#endif

}
