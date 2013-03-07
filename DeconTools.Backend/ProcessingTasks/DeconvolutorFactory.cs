using DeconTools.Backend.Core;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.ProcessingTasks.Deconvoluters;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor;

namespace DeconTools.Backend.ProcessingTasks
{
    public class DeconvolutorFactory
    {

        public static Deconvolutor CreateDeconvolutor(OldDecon2LSParameters parameters)
        {
            Deconvolutor decon;

            if (parameters.HornTransformParameters.DetectPeaksOnlyWithNoDeconvolution)
            {
                return new NullDeconvolutor();
            }



            if (parameters.HornTransformParameters.UseRAPIDDeconvolution)
            {
                decon = new RapidDeconvolutor();
            }
            else
            {
                decon = new HornDeconvolutor(parameters.HornTransformParameters);
            }
            return decon;
        }

        public static Deconvolutor CreateDeconvolutor(DeconToolsParameters parameters)
        {
            Deconvolutor decon;

            if (parameters == null)
            {
                return new NullDeconvolutor();
            }


            
            if (parameters.ThrashParameters.UseThrashV1)
            {
                decon = new HornDeconvolutor(parameters); 
            }
            else
            {
                decon = new ThrashDeconvolutorV2(parameters.ThrashParameters);
            }
            
            return decon;
        }


    }
}
