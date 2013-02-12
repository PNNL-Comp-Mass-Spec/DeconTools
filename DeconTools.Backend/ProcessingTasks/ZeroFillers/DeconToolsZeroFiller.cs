using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.ZeroFillers
{
    public class DeconToolsZeroFiller : ZeroFiller
    {
        
        public DeconToolsZeroFiller()
            : this(3)
        {

        }
        public DeconToolsZeroFiller(int maxNumPointsToAdd)
        {
            MaxNumPointsToAdd = maxNumPointsToAdd;
        }


        public override XYData ZeroFill(double[] x, double[] y, int maxPointsToAdd)
        {
            Check.Require(maxPointsToAdd >= 0, "Zerofiller's maxNumPointsToAdd cannot be a negative number");

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

                double minDistance = x[1] - x[0];
                for (int index = 2; index < numPoints - 1; index++)
                {
                    var currentDiff = (x[index] - x[index - 1]);
                    if (minDistance > currentDiff && currentDiff > 0)
                    {
                        minDistance = currentDiff;
                    }
                }

                zeroFilledXVals.Add(x[0]);
                zeroFilledYVals.Add(y[0]);

                double lastDiff = minDistance;
                int lastDiffIndex = 0;

                double lastX = x[0];

                for (int index = 1; index < numPoints - 1; index++)
                {
                    double currentDiff = x[index] - lastX;
                    double diffBetweenCurrentAndBase = x[index] - x[lastDiffIndex];
                    double differenceFactor = 1.5;


                    if (Math.Sqrt(diffBetweenCurrentAndBase) > differenceFactor)
                    {
                        differenceFactor = Math.Sqrt(diffBetweenCurrentAndBase);
                    }


                    if (currentDiff > differenceFactor * lastDiff)
                    {
                        // insert points. 
                        int numPointsToAdd = ((int)(currentDiff / lastDiff + 0.5)) - 1;
                        if (numPointsToAdd > 2 * maxPointsToAdd)
                        {
                            for (int pointNum = 0; pointNum < maxPointsToAdd; pointNum++)
                            {
                                if (lastX >= x[index])
                                    break;
                                lastX += lastDiff;
                                zeroFilledXVals.Add(lastX);
                                zeroFilledYVals.Add(0);
                            }
                            double nextLastX = x[index] - maxPointsToAdd * lastDiff;
                            if (nextLastX > lastX + lastDiff)
                            {
                                lastX = nextLastX;
                            }

                            for (int pointNum = 0; pointNum < maxPointsToAdd; pointNum++)
                            {
                                if (lastX >= x[index])
                                    break;
                                zeroFilledXVals.Add(lastX);
                                zeroFilledYVals.Add(0);
                                lastX += lastDiff;
                            }
                        }
                        else
                        {
                            for (int pointNum = 0; pointNum < numPointsToAdd; pointNum++)
                            {
                                lastX += lastDiff;
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

                var zerofilledData = new XYData {Xvalues = zeroFilledXVals.ToArray(), Yvalues = zeroFilledYVals.ToArray()};
                return zerofilledData;
            }
        }

        [Obsolete("Don't use this one. The other works fine")]
        public XYData ZeroFillOld(double[] x, double[] y, int maxPointsToAdd)
        {

            var floatXVals = x.Select(p => (float) p).ToArray();
            var floatYVals = y.Select(p => (float) p).ToArray();

            DeconEngine.Utils.ZeroFillUnevenData(ref floatXVals, ref floatYVals, maxPointsToAdd);


            var zeroFilledData = new XYData();

            zeroFilledData.Xvalues = floatXVals.Select(p=>(double)p).ToArray();
            zeroFilledData.Yvalues = floatYVals.Select(p => (double)p).ToArray();

            return zeroFilledData;

        }

    }
}

