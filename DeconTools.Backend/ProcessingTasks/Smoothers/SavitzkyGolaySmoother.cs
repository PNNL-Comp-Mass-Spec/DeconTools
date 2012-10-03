using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace DeconTools.Backend.ProcessingTasks.Smoothers
{
    public class SavitzkyGolaySmoother:Smoother
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
        /// Performs SavitzkyGolay smoothing
        /// </summary>
        /// <param name="inputValues">Input values</param>
        /// <returns></returns>
        public double[] Smooth(double[] inputValues)
        {
            if (PointsForSmoothing < 3)
                throw new ArgumentOutOfRangeException("savGolayPoints must be an odd number 3 or higher");

            if (PointsForSmoothing % 2 == 0)
                throw new ArgumentOutOfRangeException("savGolayPoints must be an odd number 3 or higher");

            int m = (PointsForSmoothing - 1) / 2;
            int colCount = inputValues.Length;
            double[] returnYValues = new double[colCount];

            
            
            if (_smoothingFilters==null)
            {
                _smoothingFilters = CalculateSmoothingFilters(PolynomialOrder, PointsForSmoothing);
            }

            var conjTransposeMatrix = _smoothingFilters.ConjugateTranspose();

            for (int i = 0; i <= m; i++)
            {
                var conjTransposeColumn = conjTransposeMatrix.Column(i);

                double multiplicationResult = 0;
                for (int z = 0; z < PointsForSmoothing; z++)
                {
                    multiplicationResult += (conjTransposeColumn[z] * inputValues[z]);
                }
                returnYValues[i] = multiplicationResult;
            }

            var conjTransposeColumnResult = conjTransposeMatrix.Column(m);

            for (int i = m + 1; i < colCount - m - 1; i++)
            {
                double multiplicationResult = 0;
                for (int z = 0; z < PointsForSmoothing; z++)
                {
                    multiplicationResult += (conjTransposeColumnResult[z] * inputValues[i - m + z]);
                }
                returnYValues[i] = multiplicationResult;
            }

            for (int i = 0; i <= m; i++)
            {
                var conjTransposeColumn = conjTransposeMatrix.Column(m + i);

                double multiplicationResult = 0;
                for (int z = 0; z < PointsForSmoothing; z++)
                {
                    multiplicationResult += (conjTransposeColumn[z] * inputValues[colCount - PointsForSmoothing + z]);
                }
                returnYValues[colCount - m - 1 + i] = multiplicationResult;
            }


            return returnYValues;
        }



        public override XYData Smooth(XYData xyData)
        {
            var smoothedData = Smooth(xyData.Yvalues);

            XYData returnVals = new XYData();
            returnVals.Xvalues = xyData.Xvalues;
            returnVals.Yvalues = smoothedData;
           
            return returnVals;
        }



        private DenseMatrix CalculateSmoothingFilters(int polynomialOrder, int filterLength )
        {
            int m = (filterLength - 1) / 2;
            var denseMatrix = new DenseMatrix(filterLength, polynomialOrder + 1);

            for (int i = -m; i <= m; i++)
            {
                for (int j = 0; j <= polynomialOrder; j++)
                {
                    denseMatrix[i + m, j] = Math.Pow(i, j);
                }
            }

            var sTranspose = (DenseMatrix)denseMatrix.ConjugateTranspose();
            var f = sTranspose * denseMatrix;
            var fInverse = (DenseMatrix)f.LU().Solve(DenseMatrix.Identity(f.ColumnCount));
            var smoothingFilters = denseMatrix * fInverse * sTranspose;

            return smoothingFilters;
        }


       
    }



}
