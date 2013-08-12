using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.Parameters;
using DeconTools.Backend.ProcessingTasks.Deconvoluters;
using DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks
{
    public class DeconvolutorFactory
    {

        public static Deconvolutor CreateDeconvolutor(DeconToolsParameters parameters)
        {
            Check.Require(parameters!=null,"Factory cannot create Deconvolutor class. DeconToolsParameters are null.");

            Deconvolutor decon;
            switch (parameters.ScanBasedWorkflowParameters.DeconvolutionType)
            {
                case Globals.DeconvolutionType.None:
                    return new NullDeconvolutor();
                case Globals.DeconvolutionType.ThrashV1:
                    decon = new HornDeconvolutor(parameters);
                    return decon;
                case Globals.DeconvolutionType.ThrashV2:
                    decon = new InformedThrashDeconvolutor(parameters.ThrashParameters);
                    return decon;
                case Globals.DeconvolutionType.Rapid:
                    return new RapidDeconvolutor(parameters.ThrashParameters.MinMSFeatureToBackgroundRatio,
                                                 Deconvolutor.DeconResultComboMode.simplyAddIt);
                default:
                    throw new ArgumentOutOfRangeException("parameters",
                                                          "Trying to create the deconvolutor, but an incorrect Deconvolutor type was given. Good example: 'ThrashV1'");
            }


        }


    }
}
