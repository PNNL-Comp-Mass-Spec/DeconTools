using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Data
{
    public abstract class IsosExporter : Exporter<ResultCollection>
    {

        public abstract void Export(string binaryResultCollectionFilename, bool deleteBinaryFileAfterUse); 
        
        protected virtual int getScanNumber(int scan_num)
        {
            return scan_num;      // default will return scan_num as is; other dependent classes may add +1 to scan_num (for 1-based purposes)

        }


    }
}
