using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks
{
    public class DeconvolutorFactory
    {

        public IDeconvolutor CreateDeconvolutor(OldDecon2LSParameters parameters)
        {
            IDeconvolutor decon;
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
