using System.Collections.Generic;
using System.Linq;

namespace DeconTools.Backend.Data
{
    public class ViperMassCalibrationData
    {
        private readonly List<ViperMassCalibrationDataItem> _calibrationDataItems;

        #region Constructors

        public ViperMassCalibrationData()
        {
            _calibrationDataItems = new List<ViperMassCalibrationDataItem>();
        }


        #endregion

        #region Properties
        public double MassError { get; set; }
        public double LeftBaseOfMassErrorCurve { get; set; }
        public double RightBaseOfMassErrorCurve { get; set; }
        protected double MassErrorCurveWidthAtBase { get; set; }
        


        public void SetCalibrationDataItems(List<ViperMassCalibrationDataItem> dataItems )
        {
            _calibrationDataItems.Clear();
            _calibrationDataItems.AddRange(dataItems);
        }


        public void ExtractKeyCalibrationValues()
        {
            if (_calibrationDataItems.Count == 0) return;

            MassError = (from n in _calibrationDataItems where n.Comment.ToLower() == "maximum" select n.MassErrorPpm).FirstOrDefault();

            LeftBaseOfMassErrorCurve = (from n in _calibrationDataItems where n.Comment.ToLower() == "left base" select n.MassErrorPpm).FirstOrDefault();

            RightBaseOfMassErrorCurve = (from n in _calibrationDataItems where n.Comment.ToLower() == "right base" select n.MassErrorPpm).FirstOrDefault();

            MassErrorCurveWidthAtBase = RightBaseOfMassErrorCurve - LeftBaseOfMassErrorCurve;
        }

       

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

    }
}
