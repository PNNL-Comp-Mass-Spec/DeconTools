using System;
// -------------------------------------------------------------------------------
// Written by Navdeep Jaitly for the Department of Energy (PNNL, Richland, WA)
// Copyright 2006, Battelle Memorial Institute
// E-mail: navdeep.jaitly@pnl.gov
// Website: http://omics.pnl.gov/software or http://panomics.pnnl.gov
// -------------------------------------------------------------------------------
// 
// Licensed under the Apache License, Version 2.0; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0

namespace Decon2LS
{
    /// <summary>
    /// Summary description for clsCfgParms.
    /// </summary>
    public class clsCfgParms
    {
        private string mstr_last_open_dir ; 

        public string OpenDir
        {
            get
            {
                return mstr_last_open_dir ; 
            }
            set
            {
                mstr_last_open_dir = value ; 
            }
        }
        public clsCfgParms()
        {
            //
            // TODO: Add constructor logic here
            //
        }
    }
}
