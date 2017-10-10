using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Utilities;
using Mapack;

namespace DeconTools.Backend.Algorithms
{

    /// <summary>
    /// Calculates the distribution of labeled atoms. i.e fraction of species that have 0 label, 1 label, 2 label, etc.
    /// Based on work by Chik et al, 2006.  http://pubs.acs.org/doi/abs/10.1021/ac050988l
    /// </summary>
    public class LabelingDistributionCalculator
    {

        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        /// <summary>
        /// Calculates the distribution of labels on a molecule.
        /// </summary>
        /// <param name="theorIntensities">peak intensities for theoretical isotopic profile. Does not need to be normalized</param>
        /// <param name="obsIntensities">peak intensities for observed isotopic profile. Does not need to be normalized</param>
        /// <param name="theorIntensityThresh">intensity threshold for theor isotopic profile. Peaks below this value are not used in the algorithm</param>
        /// <param name="obsIntensityThresh">intensity threshold for observed isotopic profile. Peaks below this value are not used in the algorithm</param>
        /// <param name="numLabelVals">array of outputted values representing the number of labels incorporated</param>
        /// <param name="labelDistributionVals">array of outputted values representing the fraction of labeling</param>
        /// <param name="truncateTheorBasedOnRelIntensity">determines whether or not the theor isotopic profile is trimmed</param>
        /// <param name="truncateObservedBasedOnRelIntensity">determines whether or not the observed isotopic profile is trimmed</param>
        /// <param name="leftPadding">number of 0 intensity 'pads' added to left side of isotopic profile</param>
        /// <param name="rightPadding">number of 0 intensity 'pads' added to right side of isotopic profile</param>
        /// <param name="numPeaksForAbsoluteTheorList">number of peaks used if an absolute number of peaks is used in the algorithm. Default is '0'. Set to high number to get all peaks</param>
        /// <param name="numPeaksForAbsoluteObsList">number of peaks used if an absolute number of peaks is used in the algorithm. Default is '0', Set to high number to get all peaks</param>
        public void CalculateLabelingDistribution(List<double> theorIntensities, List<double> obsIntensities, double theorIntensityThresh, double obsIntensityThresh,
            out double[] numLabelVals, out double[] labelDistributionVals, bool truncateTheorBasedOnRelIntensity = true, bool truncateObservedBasedOnRelIntensity = true,
            int leftPadding = 0, int rightPadding = 3, int numPeaksForAbsoluteTheorList = 0, int numPeaksForAbsoluteObsList = 0)
        {
            var truncatedTheorIntensities = truncateList(theorIntensities, truncateTheorBasedOnRelIntensity,
                                                                theorIntensityThresh, numPeaksForAbsoluteTheorList);

            var truncatedObsIntensities = truncateList(obsIntensities, truncateObservedBasedOnRelIntensity,
                                                                obsIntensityThresh, numPeaksForAbsoluteObsList);

            Check.Require(truncatedObsIntensities.Count > 0, "No values found after using LabelDistribution threshold");
            var matrixLength = truncatedObsIntensities.Count + leftPadding + rightPadding;

            var degreesFreedom = matrixLength - truncatedTheorIntensities.Count + 1;


            double[] xvals;

            double[] yvals;


            if (degreesFreedom>1)
            {

                var theorMatrix = buildMatrix(truncatedTheorIntensities, matrixLength, degreesFreedom);

                var obsMatrix = buildMatrix(truncatedObsIntensities, matrixLength, 1);

                var solvedMatrix = theorMatrix.Solve(obsMatrix);

                Check.Ensure(solvedMatrix.Rows > 0, "the labeling distribution array is empty");

                xvals = new double[solvedMatrix.Rows];
                yvals = new double[solvedMatrix.Rows];
                for (var i = 0; i < solvedMatrix.Rows; i++)
                {
                    xvals[i] = i;
                    yvals[i] = solvedMatrix[i, 0];
                }


            }
            else
            {
                xvals = new double[1];
                yvals = new double[1];

                xvals[0] = 0;
                yvals[0] = 1;
            }



            numLabelVals = xvals;
            labelDistributionVals = yvals;

        }


        public void OutputLabelingInfo(List<double> distributionData, out double fractionUnlabelled, out double fractionLabelled, out double averageLabelsIncorporated )
        {

            Check.Require(distributionData != null && distributionData.Count > 0, "distributionData is empty in call to OutputLabelingInfo");

            var dotProducts = new List<double>();


            fractionUnlabelled = distributionData[0];
            fractionLabelled = 1 - fractionUnlabelled;

            //for (int numLabels = 0; numLabels < distributionData.Count; numLabels++)
            //{
            //    dotProducts.Add(numLabels*distributionData[numLabels]);
            //}


            //averageLabelsIncorporated = dotProducts.Sum() / 1;

            if (fractionLabelled > 0)
            {
                double sum = 0;
                for (var numLabels = 1; numLabels < distributionData.Count; numLabels++)
                {
                    sum += distributionData[numLabels];
                    dotProducts.Add(numLabels * distributionData[numLabels]);
                }


                averageLabelsIncorporated = dotProducts.Sum() / sum;
            }
            else
            {
                averageLabelsIncorporated = 0;
            }



        }

        private Matrix buildMatrix(List<double> intensityVals, int matrixLength, int degreesFreedom)
        {
            Check.Require(degreesFreedom > 0, "Degrees of freedom too low. Matrix width cannot be less than 1");
            var matrix = new Matrix(matrixLength, degreesFreedom);


            var sumIntensities = intensityVals.Sum();

            var normalizedIntensities = intensityVals.Select(p => p / sumIntensities).ToList();

            for (var i = 0; i < degreesFreedom; i++)
            {
                for (var j = 0; j < normalizedIntensities.Count; j++)
                {
                    matrix[j + i, i] = normalizedIntensities[j];
                }
            }

            return matrix;


        }

        private List<double> truncateList(List<double> intensityVals, bool useRelIntensity, double intensityThresh, int numPeaksForAbsoluteTrucation)
        {
            var truncatedVals = new List<double>();

            if (useRelIntensity)
            {
                var maxIntensity = intensityVals.Max();

                var foundPeakAboveThreshold = false;  // this is a flag that indicates that a peak is found above the threshold intensity

                foreach (var intensityVal in intensityVals)
                {
                    var currentRelIntensity = intensityVal / maxIntensity * 100;

                    var currentValIsAboveTheshold = currentRelIntensity >= intensityThresh;

                    if (currentValIsAboveTheshold)
                    {
                        foundPeakAboveThreshold = true;
                    }

                    //once a value above theshold is found, we will keep adding values until a value is found that is below threshold.
                    //This will truncate the right side of an isotopic profile.
                    if (foundPeakAboveThreshold)
                    {
                        if (currentValIsAboveTheshold)
                        {
                            truncatedVals.Add(intensityVal);
                        }
                        else
                        {
                            break;     //found a value below threshold, after finding a previous value that exceeded the threshold. Truncate here.
                        }
                    }
                    else
                    {
                        truncatedVals.Add(intensityVal);    //all intensity values are added, even if they are below threshold, for the left side of an isotopic profile.
                    }

                }
            }
            else
            {
                var upperLimit = Math.Min(numPeaksForAbsoluteTrucation, intensityVals.Count);

                for (var i = 0; i < upperLimit; i++)
                {
                    truncatedVals.Add(intensityVals[i]);
                }


            }

            return truncatedVals;


        }

        #endregion


    }
}
