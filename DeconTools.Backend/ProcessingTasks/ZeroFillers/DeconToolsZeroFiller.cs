using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ZeroFillers
{
	public class DeconToolsZeroFiller : ZeroFiller
	{

		public DeconToolsZeroFiller()
			: this(DEFAULT_POINTS_TO_ADD, DEFAULT_MAX_ZERO_FILL_DISTANCE)
		{

		}

		public DeconToolsZeroFiller(int maxNumPointsToAdd)
			: this(maxNumPointsToAdd, DEFAULT_MAX_ZERO_FILL_DISTANCE)
		{
		}

		public DeconToolsZeroFiller(int maxNumPointsToAdd, double maxZeroFillDistance)
		{
			MaxNumPointsToAdd = maxNumPointsToAdd;
			MaxZeroFillDistance = maxZeroFillDistance;
		}


		public override XYData ZeroFill(double[] x, double[] y, int maxPointsToAdd)
		{
			return ZeroFill(x, y, maxPointsToAdd, MaxZeroFillDistance);
		}

		/// <summary>
		/// Add zeroes between adjacent data points if there is a wide space between them (on the x-axis)
		/// </summary>
		/// <param name="x">X data values</param>
		/// <param name="y">Y data values</param>
		/// <param name="maxPointsToAdd">Number of points to add</param>
		/// <param name="maxZeroFillDistance">Maximum distance (in Thompsons) between the new zero'd points and the real data points; default is 0.05</param>
		/// <returns></returns>
		public override XYData ZeroFill(double[] x, double[] y, int maxPointsToAdd, double maxZeroFillDistance)
		{
			Check.Require(maxPointsToAdd >= 0, "Zerofiller's maxNumPointsToAdd cannot be a negative number");

			Check.Require(maxZeroFillDistance > 0, "Zerofiller's maxZeroFillDistance cannot be zero or negative");

			{
				if (x.Length != y.Length)
				{
					throw new ArgumentException("Zerofiller failed. Xvalues and Yvalues had different lengths.");
				}

				int numPoints = x.Length;
				if (numPoints <= 1)
				{
					XYData xyData = new XYData();
					xyData.Xvalues = x;
					xyData.Yvalues = y;
					return xyData;
				}


				var zeroFilledXVals = new List<double>();
				var zeroFilledYVals = new List<double>();

				// Determine the initial value for the zero-fill distance based on the minimum width between any two adjacent points
				double minDistance = x[1] - x[0];
				for (int index = 2; index < numPoints; index++)
				{
					var currentDiff = (x[index] - x[index - 1]);
					if (minDistance > currentDiff && currentDiff > 0)
					{
						minDistance = currentDiff;
					}
				}

				if (minDistance > maxZeroFillDistance)
					minDistance = maxZeroFillDistance;

				zeroFilledXVals.Add(x[0]);
				zeroFilledYVals.Add(y[0]);

                double lastDiff = minDistance;
				int lastDiffIndex = 0;

				double lastX = x[0];

				for (int index = 1; index < numPoints; index++)
				{
					double currentDiff = x[index] - lastX;
					double diffBetweenCurrentAndBase = x[index] - x[lastDiffIndex];
					double differenceFactor = 1.5;

					if (Math.Sqrt(diffBetweenCurrentAndBase) > differenceFactor)
					{
						differenceFactor = Math.Sqrt(diffBetweenCurrentAndBase);
					}

					double diffThreshold = lastDiff;
					if (diffThreshold > maxZeroFillDistance)
						diffThreshold = maxZeroFillDistance;

					if (currentDiff > differenceFactor * diffThreshold)
					{
						// insert points. 
						int numPointsToAdd = ((int)(currentDiff / diffThreshold + 0.5)) - 1;
						if (numPointsToAdd > 2 * maxPointsToAdd)
						{
							for (int pointNum = 0; pointNum < maxPointsToAdd; pointNum++)
							{
								if (lastX >= x[index])
									break;
								lastX += diffThreshold;
								zeroFilledXVals.Add(lastX);
								zeroFilledYVals.Add(0);
							}
							double nextLastX = x[index] - maxPointsToAdd * diffThreshold;
							if (nextLastX > lastX + diffThreshold)
							{
								lastX = nextLastX;
							}

							for (int pointNum = 0; pointNum < maxPointsToAdd; pointNum++)
							{
								if (lastX >= x[index])
									break;
								zeroFilledXVals.Add(lastX);
								zeroFilledYVals.Add(0);
								lastX += diffThreshold;
							}
						}
						else
						{
							for (int pointNum = 0; pointNum < numPointsToAdd; pointNum++)
							{
								lastX += diffThreshold;
								if (lastX >= x[index])
									break;
								zeroFilledXVals.Add(lastX);
								zeroFilledYVals.Add(0);
							}
						}
						zeroFilledXVals.Add(x[index]);
						zeroFilledYVals.Add(y[index]);
						lastX = x[index];
					}
					else
					{
						zeroFilledXVals.Add(x[index]);
						zeroFilledYVals.Add(y[index]);
						lastDiff = currentDiff;
						lastDiffIndex = index;
						lastX = x[index];
					}
				}

				var zerofilledData = new XYData { Xvalues = zeroFilledXVals.ToArray(), Yvalues = zeroFilledYVals.ToArray() };
				return zerofilledData;
			}
		}

		[Obsolete("Don't use this one. The other works fine")]
		public XYData ZeroFillOld(double[] x, double[] y, int maxPointsToAdd)
		{

			var floatXVals = x.Select(p => (float)p).ToArray();
			var floatYVals = y.Select(p => (float)p).ToArray();

			DeconEngine.Utils.ZeroFillUnevenData(ref floatXVals, ref floatYVals, maxPointsToAdd);

			var zeroFilledData = new XYData();

			zeroFilledData.Xvalues = floatXVals.Select(p => (double)p).ToArray();
			zeroFilledData.Yvalues = floatYVals.Select(p => (double)p).ToArray();

			return zeroFilledData;

		}

	}
}

