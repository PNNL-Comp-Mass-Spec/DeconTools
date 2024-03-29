using System;
using System.Xml.Linq;

namespace DeconTools.Backend.Parameters
{
    [Serializable]
    public class MSGeneratorParameters : ParametersBase
    {
        public MSGeneratorParameters()
        {
            UseMZRange = false;
            GetTicInfo = false;
            MinMZ = 0;
            MaxMZ = 5000;
            SumAllSpectra = false;
            SumSpectraAcrossLC = false;
            SumSpectraAcrossIms = false;

            UseLCScanRange = false;
            MinLCScan = 0;
            MaxLCScan = int.MaxValue;

            NumLCScansToSum = 1;
            NumImsScansToSum = 1;

            UseImsScanRange = false;
            MinImsScan = 0;
            MaxImsScan = int.MaxValue;
        }

        public MSGeneratorParameters(double minMZ, double maxMZ)
            : this()
        {
            MinMZ = minMZ;
            MaxMZ = maxMZ;
        }

        public bool UseMZRange { get; set; }
        public double MinMZ { get; set; }
        public double MaxMZ { get; set; }

        public bool UseLCScanRange { get; set; }
        public int MinLCScan { get; set; }
        public int MaxLCScan { get; set; }

        public bool SumAllSpectra { get; set; }
        public bool SumSpectraAcrossLC { get; set; }
        public bool SumSpectraAcrossIms { get; set; }
        public int NumLCScansToSum { get; set; }
        public int NumImsScansToSum { get; set; }

        public bool UseImsScanRange { get; set; }
        public int MinImsScan { get; set; }
        public int MaxImsScan { get; set; }

        public bool GetTicInfo { get; set; }

        public override void LoadParameters(XElement xElement)
        {
            throw new NotImplementedException();
        }

        public override void LoadParametersV2(XElement xElement)
        {
            UseMZRange = GetBoolVal(xElement, "UseMZRange", UseMZRange);
            MinMZ = GetDoubleValue(xElement, "MinMZ", MinMZ);
            MaxMZ = GetDoubleValue(xElement, "MaxMZ", MaxMZ);

            UseLCScanRange = GetBoolVal(xElement, "UseScanRange", UseLCScanRange);
            MinLCScan = GetIntValue(xElement, "MinScan", MinLCScan);
            MaxLCScan = GetIntValue(xElement, "MaxScan", MaxLCScan);

            //TODO: add these parameters to parameter file
            UseImsScanRange = GetBoolVal(xElement, "UseImsScanRange", UseImsScanRange);
            MinImsScan = GetIntValue(xElement, "MinImsScan", MinImsScan);
            MaxImsScan = GetIntValue(xElement, "MaxImsScan", MaxImsScan);

            //TODO:  change how IMS-parameters are loaded and used in this case
            SumAllSpectra = GetBoolVal(xElement, "SumSpectra", SumAllSpectra);
            SumSpectraAcrossLC = GetBoolVal(xElement, "SumSpectraAcrossScanRange", SumSpectraAcrossLC);
            SumSpectraAcrossIms = GetBoolVal(xElement, "SumSpectraAcrossIms", SumSpectraAcrossIms);
            NumLCScansToSum = GetIntValue(xElement, "NumberOfScansToSumOver", NumLCScansToSum);
            NumImsScansToSum = GetIntValue(xElement, "NumberOfImsScansToSumOver", NumImsScansToSum);

            GetTicInfo = GetBoolVal(xElement, "GetTicInfo", GetTicInfo); //doesn't exist yet

            var oldImsParamCheck = xElement.Element("SumSpectraAcrossFrameRange");
            if (oldImsParamCheck != null)
            {
                SumSpectraAcrossIms = SumSpectraAcrossLC;
                SumSpectraAcrossLC = GetBoolVal(xElement, "SumSpectraAcrossFrameRange", SumSpectraAcrossIms);
                NumImsScansToSum = NumLCScansToSum * 2 + 1; // Old format was +/- x scans, new format is x scans, must be odd number.
                NumLCScansToSum = GetIntValue(xElement, "NumberOfFramesToSumOver", NumLCScansToSum);
            }
        }
    }
}
