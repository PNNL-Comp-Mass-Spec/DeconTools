using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.Algorithms
{
    public class ChromatogramCorrelator
    {



        public void GetElutionCorrelationData(XYData chromData1, XYData chromData2, out double slope, out double intercept, out double rsquaredVal)
        {
            Check.Require(chromData1 != null && chromData1.Xvalues!=null, "Chromatogram1 intensities are null");
            Check.Require(chromData2 != null && chromData2.Xvalues!=null, "Chromatogram2 intensities are null");

            Check.Require(chromData1.Xvalues[0] == chromData2.Xvalues[0]);


            GetElutionCorrelationData(chromData1.Yvalues, chromData2.Yvalues, out slope, out intercept, out rsquaredVal);



        }

        public void GetElutionCorrelationData(double[] chromIntensities1,double[] chromIntensities2, out double slope, out double intercept, out double rsquaredVal )
        {
            Check.Require(chromIntensities1!=null, "Chromatogram1 intensities are null");
            Check.Require(chromIntensities2 != null, "Chromatogram2 intensities are null");

            Check.Require(chromIntensities1.Length==chromIntensities2.Length, "Chromatogram1 and Chromatogram2 must be the same length");


            slope = -9999; 
            intercept = -9999;
            rsquaredVal = -1;
            
            MathUtils.GetLinearRegression(chromIntensities1,chromIntensities2, out slope, out intercept, out rsquaredVal);
        }


    }
}
