﻿using System;

namespace DeconTools.Backend.Core
{
    [Serializable]
    public class StandardScanResult : ScanResult
    {
        public StandardScanResult(ScanSet scanSet) : base(scanSet)
        {
        }
    }
}
