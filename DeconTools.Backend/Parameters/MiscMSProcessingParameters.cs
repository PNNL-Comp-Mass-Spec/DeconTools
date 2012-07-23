
using System.Xml.Linq;

namespace DeconTools.Backend.Parameters
{
    public class MiscMSProcessingParameters:ParametersBase
    {

        #region Constructors
        public MiscMSProcessingParameters()
        {
            UseSmoothing = false;
            SmoothingType = Globals.SmoothingType.SavitzkyGolay;
            SavitzkyGolayNumSmoothedRight = 2;
            SavitzkyGolayNumSmoothedLeft = 2;
            SavitzkyGolayOrder = 2;

            UseZeroFilling = false;
            ZeroFillingNumZerosToFill = 3;

            SaturationThreshold = 50000;
        }
        #endregion

        #region Properties

        public bool UseSmoothing { get; set; }

        public Globals.SmoothingType SmoothingType { get; set; }

        public int SavitzkyGolayNumSmoothedLeft { get; set; }

        public int SavitzkyGolayNumSmoothedRight { get; set; }

        public int SavitzkyGolayOrder { get; set; }



        public bool UseZeroFilling { get; set; }

        public int ZeroFillingNumZerosToFill { get; set; }

        public double SaturationThreshold { get; set; }


        #endregion

        public override void LoadParameters(XElement xElement)
        {
            UseSmoothing = GetBoolVal(xElement, "ApplySavitzkyGolay", UseSmoothing);
            SmoothingType = (Globals.SmoothingType) GetEnum(xElement, "SmoothingType", SmoothingType.GetType(), SmoothingType);  //Smoothing type does not yet exist in parameter file

            SavitzkyGolayNumSmoothedLeft = GetIntValue(xElement, "SGNumLeft", SavitzkyGolayNumSmoothedLeft);

            SavitzkyGolayNumSmoothedRight = GetIntValue(xElement, "SGNumRight", SavitzkyGolayNumSmoothedRight);
            SavitzkyGolayOrder = GetIntValue(xElement, "SGOrder", SavitzkyGolayOrder);

            UseZeroFilling = GetBoolVal(xElement, "ZeroFillDiscontinousAreas", UseZeroFilling);
            ZeroFillingNumZerosToFill = GetIntValue(xElement, "NumZerosToFill", ZeroFillingNumZerosToFill);

            SaturationThreshold = GetDoubleValue(xElement, "SaturationThreshold", SaturationThreshold);

        }
    }
}
