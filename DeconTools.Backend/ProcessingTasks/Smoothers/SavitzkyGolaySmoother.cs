﻿// Written by Kevin Crowell, Spencer Prost and Gordon Slysz for the Department of Energy (PNNL, Richland, WA)
// Copyright 2012, Battelle Memorial Institute
// E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
// Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://www.pnnl.gov/integrative-omics
// -------------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DeconTools.Backend.ProcessingTasks.Smoothers
{
    public class SavitzkyGolaySmoother : Smoother
    {
        DenseMatrix _smoothingFilters;

        public SavitzkyGolaySmoother(int pointsForSmoothing, int polynomialOrder)
        {
            PointsForSmoothing = pointsForSmoothing;
            PolynomialOrder = polynomialOrder;
        }

        public int PointsForSmoothing { get; set; }
        public int PolynomialOrder { get; set; }

        /// <summary>
        /// When smoothing, smoothed values that once were positive, may become negative. This will zero-out any negative values
        /// </summary>
        public bool AllowNegativeValues { get; set; }

        /// <summary>
        /// Performs SavitzkyGolay smoothing
        /// </summary>
        /// <param name="inputValues">Input values</param>
        /// <returns></returns>
        public double[] Smooth(double[] inputValues)
        {
            if (PointsForSmoothing < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(PointsForSmoothing), "PointsForSmoothing must be an odd number 3 or higher");
            }

            if (PointsForSmoothing % 2 == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(PointsForSmoothing), "PointsForSmoothing must be an odd number 3 or higher");
            }

            var colCount = inputValues.Length;
            var returnYValues = new double[colCount];

            var newPointsForSmoothing = PointsForSmoothing > colCount ? colCount : PointsForSmoothing;
            var m = (newPointsForSmoothing - 1) / 2;

            if (_smoothingFilters == null || _smoothingFilters.ColumnCount != newPointsForSmoothing)
            {
                _smoothingFilters = CalculateSmoothingFilters(PolynomialOrder, newPointsForSmoothing);
            }

            var conjTransposeMatrix = _smoothingFilters.ConjugateTranspose();

            for (var i = 0; i <= m; i++)
            {
                var conjTransposeColumn = conjTransposeMatrix.Column(i);

                double multiplicationResult = 0;
                for (var z = 0; z < newPointsForSmoothing; z++)
                {
                    multiplicationResult += (conjTransposeColumn[z] * inputValues[z]);
                }

                returnYValues[i] = multiplicationResult;
            }

            var conjTransposeColumnResult = conjTransposeMatrix.Column(m);

            for (var i = m + 1; i < colCount - m - 1; i++)
            {
                double multiplicationResult = 0;
                for (var z = 0; z < newPointsForSmoothing; z++)
                {
                    multiplicationResult += (conjTransposeColumnResult[z] * inputValues[i - m + z]);
                }
                returnYValues[i] = multiplicationResult;
            }

            for (var i = 0; i <= m; i++)
            {
                var conjTransposeColumn = conjTransposeMatrix.Column(m + i);

                double multiplicationResult = 0;
                for (var z = 0; z < newPointsForSmoothing; z++)
                {
                    multiplicationResult += (conjTransposeColumn[z] * inputValues[colCount - newPointsForSmoothing + z]);
                }
                returnYValues[colCount - m - 1 + i] = multiplicationResult;
            }

            if (!AllowNegativeValues)
            {
                for (var i = 0; i < returnYValues.Length; i++)
                {
                    if (returnYValues[i] < 0)
                    {
                        returnYValues[i] = 0;
                    }
                }
            }

            return returnYValues;
        }

        public override XYData Smooth(XYData xyData)
        {
            if (xyData == null || xyData.Xvalues.Length == 0)
            {
                return xyData;
            }

            var smoothedData = Smooth(xyData.Yvalues);

            var returnVals = new XYData
            {
                Xvalues = xyData.Xvalues,
                Yvalues = smoothedData
            };

            return returnVals;
        }

        private DenseMatrix CalculateSmoothingFilters(int polynomialOrder, int filterLength)
        {
            var m = (filterLength - 1) / 2;
            var denseMatrix = new DenseMatrix(filterLength, polynomialOrder + 1);

            for (var i = -m; i <= m; i++)
            {
                for (var j = 0; j <= polynomialOrder; j++)
                {
                    denseMatrix[i + m, j] = Math.Pow(i, j);
                }
            }

            var sTranspose = (DenseMatrix)denseMatrix.ConjugateTranspose();
            var f = sTranspose * denseMatrix;
            var fInverse = (DenseMatrix)f.LU().Solve(DenseMatrix.CreateIdentity(f.ColumnCount));
            var smoothingFilters = denseMatrix * fInverse * sTranspose;

            return smoothingFilters;
        }
    }
}
