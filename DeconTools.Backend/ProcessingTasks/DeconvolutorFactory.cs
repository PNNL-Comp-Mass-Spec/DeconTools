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
            Check.Require(parameters != null,"Factory cannot create Deconvolutor class. DeconToolsParameters are null.");

            Deconvolutor decon;
            switch (parameters.ScanBasedWorkflowParameters.DeconvolutionType)
            {
                case Globals.DeconvolutionType.None:
                    return new NullDeconvolutor();

                case Globals.DeconvolutionType.ThrashV1:
                    // 2016 Port of ThrashV1 in DeconEngineV2 to C#, .NET 4
                    // This is the preferred deconvoluter as of Fall 2017
                    decon = new HornDeconvolutor(parameters);
                    return decon;

                case Globals.DeconvolutionType.ThrashV2:
                    // 2012 port of DeconEngine to C#
                    // As of 2016, not used because results do not agree with ThrashV1, C++
#pragma warning disable 618
                    decon = new InformedThrashDeconvolutor(parameters.ThrashParameters);
#pragma warning restore 618
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
                    throw new ArgumentOutOfRangeException(nameof(parameters),
                                                          "Trying to create the deconvolutor, but an incorrect Deconvolutor type was given. Good example: 'ThrashV1'");
            }


        }


    }
}
