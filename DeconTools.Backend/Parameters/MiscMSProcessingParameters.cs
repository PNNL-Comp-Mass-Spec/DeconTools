
namespace DeconTools.Backend.Parameters
{
    public class MiscMSProcessingParameters
    {

        #region Constructors
        public MiscMSProcessingParameters()
        {
            SmoothingType = Globals.SmoothingType.SavitzkyGolay;

            UseSmoothing = false;
            UseZeroFilling = false;

            SavitzkyGolayNumSmoothedRight = 2;
            SavitzkyGolayNumSmoothedLeft = 2;
            SavitzkyGolayOrder = 2;

            ZeroFillingNumZerosToFill = 3;



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

        public float SaturationThreshold { get; set; }


        #endregion

    

    }
}
