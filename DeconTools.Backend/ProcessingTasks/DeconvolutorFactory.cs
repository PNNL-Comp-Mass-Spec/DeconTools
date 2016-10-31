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
//#if Disable_DeconToolsV2
//                    throw new NotSupportedException(
//                        "Deconvolution method ThrashV1 is not supported since support for C++ based DeconToolsV2 is disabled; update the parameter file to use <DeconvolutionType>ThrashV2</DeconvolutionType>");
//#else
                    decon = new HornDeconvolutor(parameters);
                    return decon;
//#endif
                case Globals.DeconvolutionType.ThrashV2:
                    decon = new InformedThrashDeconvolutor(parameters.ThrashParameters);
                    return decon;
                case Globals.DeconvolutionType.Rapid:
                    // To include support for Rapid, you must add a reference to DeconEngine.dll, which was compiled with Visual Studio 2003 and uses MSVCP71.dll
                    // Note that DeconEngine.dll also depends on xerces-c_2_7.dll while DeconEngineV2.dll depends on xerces-c_2_8.dll
#if INCLUDE_RAPID
                    return new RapidDeconvolutor(parameters.ThrashParameters.MinMSFeatureToBackgroundRatio,
                                                 Deconvolutor.DeconResultComboMode.simplyAddIt);
#else
                    throw new NotSupportedException("Support for Rapid is not included in this version of the DLL");
#endif
                    default:
                    throw new ArgumentOutOfRangeException("parameters",
                                                          "Trying to create the deconvolutor, but an incorrect Deconvolutor type was given. Good example: 'ThrashV1'");
            }


        }


    }
}
