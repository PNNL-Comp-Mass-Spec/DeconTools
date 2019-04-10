using System;
using System.Linq;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.Algorithms.Quantifiers
{
    public class BasicO16O18Quantifier : O16O18QuantifierBase
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods

        public override double GetRatio(IsotopicProfile isotopicProfile)
        {
            return Get_I0_I4_ratio(isotopicProfile);
        }

        public double Get_I0_I4_ratio(IsotopicProfile iso)
        {
            resetIntensityValues();

            intensityI0 = getI0Intensity(iso);
            intensityI2 = getI2Intensity(iso);
            intensityI4 = GetI4Intensity(iso);

            if (Math.Abs(intensityI4) < double.Epsilon) intensityI4 = double.Epsilon;

            return intensityI0 / intensityI4;
        }

        private void resetIntensityValues()
        {
            intensityI0 = 0;
            intensityI2 = 0;
            intensityI4 = 0;
            intensityTheorI0 = 0;
            intensityTheorI2 = 0;
            intensityTheorI4 = 0;
            adjustedI4Intensity = 0;

        }


        public double GetAdjusted_I0_I4_YeoRatio(IsotopicProfile iso, IsotopicProfile theorIso)
        {
            resetIntensityValues();

            intensityI0 = getI0Intensity(iso);
            intensityI2 = getI2Intensity(iso);
            intensityI4 = GetI4Intensity(iso);

            intensityTheorI0 = getI0Intensity(theorIso);
            intensityTheorI2 = getI2Intensity(theorIso);
            intensityTheorI4 = GetI4Intensity(theorIso);


            var ratio = CalculateRatio(intensityTheorI0, intensityTheorI2, intensityTheorI4, intensityI0, intensityI2, intensityI4,
                                  out var adjI4Intensity);

            adjustedI4Intensity = adjI4Intensity;


            return ratio;

            //if (intensityI4 == 0)
            //{
            //    adjustedI4Intensity = intensityI4;
            //}
            //else
            //{
            //    // see Yeo et al (2001), Analytical Chemistry. https://www.ncbi.nlm.nih.gov/pubmed/11467524  (Anal Chem. 2001 Jul 1;73(13):2836-42.)
            //    adjustedI4Intensity = intensityI4 - (intensityTheorI4 / intensityTheorI0 * intensityI0);

            //    if (intensityI2 > 0)
            //    {
            //        adjustedI4Intensity = adjustedI4Intensity - (intensityTheorI2 / intensityTheorI0) * (intensityI2 - intensityTheorI2 / intensityTheorI0 * intensityI0) + 0.5 * (intensityI2 - intensityTheorI2 / intensityTheorI0 * intensityI0);
            //    }
            //    else
            //    {
            //        //TODO:  if there is an intensity at I4, there should be something at I2.
            //    }

            //}

            //if (adjustedI4Intensity <= 0) adjustedI4Intensity = double.Epsilon;


            //ratio = intensityI0 / adjustedI4Intensity;
            //return ratio;


        }


        public double GetAdjustedRatioUsingChromCorrData(O16O18TargetedResultObject o16O18TargetedResultObject)
        {
            double ratio;

            intensityTheorI0 = getI0Intensity(o16O18TargetedResultObject.Target.IsotopicProfile);
            intensityTheorI2 = getI2Intensity(o16O18TargetedResultObject.Target.IsotopicProfile);
            intensityTheorI4 = GetI4Intensity(o16O18TargetedResultObject.Target.IsotopicProfile);

            if (o16O18TargetedResultObject.ChromCorrelationData == null) return -1;

            var noO16PeakPresent = !o16O18TargetedResultObject.ChromCorrelationData.CorrelationDataItems.Any();
            if (noO16PeakPresent)
            {
                ratio = 0;
            }
            else
            {
                var ratioSingleO18ToO16 = o16O18TargetedResultObject.ChromCorrelationData.CorrelationDataItems[0].CorrelationSlope ?? 0d;

                var ratioDoubleO18ToO16 = o16O18TargetedResultObject.ChromCorrelationData.CorrelationDataItems[1].CorrelationSlope ?? 0d;

                var tempIntensity = CalculateRatio(intensityTheorI0, intensityTheorI2, intensityTheorI4, 1.0, ratioSingleO18ToO16,
                                                      ratioDoubleO18ToO16, out var adjRatioI0I4);

                ratio = 1 / adjRatioI0I4;   //report the o16/o18 ratio

            }

            return ratio;

        }



        public double CalculateRatio(double theorI0, double theorI2, double theorI4, double i0, double i2, double i4, out double adjustedI4)
        {
            if (Math.Abs(intensityI4) < double.Epsilon)
            {
                adjustedI4 = i4;
            }
            else
            {
                // see Yeo et al (2001), Analytical Chemistry. https://www.ncbi.nlm.nih.gov/pubmed/11467524  (Anal Chem. 2001 Jul 1;73(13):2836-42.)
                adjustedI4 = i4 - (theorI4 / theorI0 * i0);

                if (i2 > 0)
                {
                    adjustedI4 = adjustedI4 - (theorI2 / theorI0) * (i2 - theorI2 / theorI0 * i0) + 0.5 * (i2 - theorI2 / theorI0 * i0);
                }
                else
                {
                    //TODO:  if there is an intensity at I4, there should be something at I2.
                }

            }

            if (adjustedI4 <= 0) adjustedI4 = double.Epsilon;


            var ratio = intensityI0 / adjustedI4;
            return ratio;
        }




        private double GetI4Intensity(IsotopicProfile iso)
        {
            if (iso?.Peaklist == null || iso.Peaklist.Count < 5)
            {
                return 0;
            }

            double intensity = iso.Peaklist[4].Height;
            return intensity;
        }

        private double getI2Intensity(IsotopicProfile iso)
        {
            if (iso?.Peaklist == null || iso.Peaklist.Count < 3)
            {
                return 0;
            }

            double intensity = iso.Peaklist[2].Height;

            return intensity;
        }

        private double getI0Intensity(IsotopicProfile iso)
        {
            if (iso?.Peaklist == null || iso.Peaklist.Count < 1)
            {
                return 0;
            }

            double intensity = iso.Peaklist[0].Height;
            if (Math.Abs(intensity) < Double.Epsilon) intensity = 0;

            return intensity;
        }



        #endregion

        #region Private Methods
        #endregion



        public double intensityI0 { get; set; }

        public double intensityI2 { get; set; }

        public double intensityI4 { get; set; }

        public double intensityTheorI0 { get; set; }

        public double intensityTheorI2 { get; set; }

        public double intensityTheorI4 { get; set; }

        public double adjustedI4Intensity { get; set; }


    }
}
