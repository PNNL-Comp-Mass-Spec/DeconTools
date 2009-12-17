using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;
using DeconTools.Backend.DTO;

namespace DeconTools.Backend.ProcessingTasks
{
    public class PeakChromatogramGenerator : Task
    {
        double ppmTol;

        #region Constructors
        public PeakChromatogramGenerator()
            : this(20)
        {

        }

        public PeakChromatogramGenerator(double ppmTol)
        {
            this.ppmTol = ppmTol;

        }
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
        public override void Execute(ResultCollection resultList)
        {
            Check.Require(resultList.MSPeakResultList != null, "PeakChromatogramGenerator failed. No peaks.");
            Check.Require(resultList.Run.CurrentMassTag != null, "PeakChromatogramGenerator failed. This requires a MassTag to be specified.");

            double mz = resultList.Run.CurrentMassTag.MZ;

            double lowerMZ = -1 * (ppmTol * mz / 1e6 - mz);
            double upperMZ = ppmTol * mz / 1e6 + mz;


            //double mzLowerLimit = (mz-ppmTol*

            List<MSPeakResult> filteredPeakList = resultList.MSPeakResultList.Where(p => p.MSPeak.MZ >= lowerMZ && p.MSPeak.MZ <= upperMZ).ToList();

            N14N15_TResult result = new N14N15_TResult();
            result.MassTag = resultList.Run.CurrentMassTag;
            result.ChromValues = getPeakChromValues(filteredPeakList);
            resultList.Run.XYData.Xvalues = result.ChromValues.Xvalues;   // peak detectors or smoothers might use this
            resultList.Run.XYData.Yvalues = result.ChromValues.Yvalues;

            resultList.ResultList.Add(result);

        }

        private XYData getPeakChromValues(List<MSPeakResult> filteredPeakList)
        {
            XYData data = new XYData();

            data.Xvalues = new double[filteredPeakList.Count];
            data.Yvalues = new double[filteredPeakList.Count];

            for (int i = 0; i < filteredPeakList.Count; i++)
            {
                data.Xvalues[i] = filteredPeakList[i].Scan_num;
                data.Yvalues[i] = filteredPeakList[i].MSPeak.Intensity;
                
            }

            return data;

        }
    }
}
