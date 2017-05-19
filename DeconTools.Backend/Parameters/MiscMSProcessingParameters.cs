using System;
using System.Xml.Linq;

namespace DeconTools.Backend.Parameters
{
    [Serializable]
    public class MiscMSProcessingParameters : ParametersBase
    {

        #region Constructors
        public MiscMSProcessingParameters()
        {
            UseSmoothing = false;
            SmoothingType = Globals.SmoothingType.SavitzkyGolay;
            SavitzkyGolayNumPointsInSmooth = 5;
            SavitzkyGolayOrder = 2;

            UseZeroFilling = false;
            ZeroFillingNumZerosToFill = 3;

            SaturationThreshold = 50000;

            MaxMinutesPerScan = 4;
            MaxMinutesPerFrame = 20;

            // Default to a max of 14 days processing time
            MaxHoursPerDataset = 336;
        }
        #endregion

        #region Properties

        public bool UseSmoothing { get; set; }

        public Globals.SmoothingType SmoothingType { get; set; }

        public int SavitzkyGolayNumPointsInSmooth { get; set; }

        public int SavitzkyGolayOrder { get; set; }



        public bool UseZeroFilling { get; set; }

        public int ZeroFillingNumZerosToFill { get; set; }

        public double SaturationThreshold { get; set; }

        public int MaxMinutesPerScan { get; set; }
        public int MaxMinutesPerFrame { get; set; }
        public int MaxHoursPerDataset { get; set; }

        #endregion

        public override void LoadParameters(XElement xElement)
        {
            throw new System.NotImplementedException();
        }

        public override void LoadParametersV2(XElement xElement)
        {
            UseSmoothing = GetBoolVal(xElement, "ApplySavitzkyGolay", UseSmoothing);
            SmoothingType = (Globals.SmoothingType) GetEnum(xElement, "SmoothingType", SmoothingType.GetType(), SmoothingType);  //Smoothing type does not yet exist in parameter file

            var smoothLeft = GetIntValue(xElement, "SGNumLeft", (SavitzkyGolayNumPointsInSmooth-1)/2);
            var smoothRight = GetIntValue(xElement, "SGNumRight", (SavitzkyGolayNumPointsInSmooth - 1) / 2);

            SavitzkyGolayNumPointsInSmooth = smoothLeft + smoothRight + 1;
            SavitzkyGolayOrder = GetIntValue(xElement, "SGOrder", SavitzkyGolayOrder);

            UseZeroFilling = GetBoolVal(xElement, "ZeroFillDiscontinousAreas", UseZeroFilling);
            ZeroFillingNumZerosToFill = GetIntValue(xElement, "NumZerosToFill", ZeroFillingNumZerosToFill);

            SaturationThreshold = GetDoubleValue(xElement, "SaturationThreshold", SaturationThreshold);

            MaxMinutesPerScan = GetIntValue(xElement, "MaxMinutesPerScan", MaxMinutesPerScan);
            MaxMinutesPerFrame = GetIntValue(xElement, "MaxMinutesPerFrame", MaxMinutesPerFrame);
            MaxHoursPerDataset = GetIntValue(xElement, "MaxHoursPerDataset", MaxHoursPerDataset);

        }
    }
}
