using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.Deconvoluters;

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

    }
}
