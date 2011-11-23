using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace DeconTools.Backend
{
    public class ParameterLoader_deprecated
    {

        DeconToolsV2.Peaks.clsPeakProcessorParameters peakParameters;
		DeconToolsV2.HornTransform.clsHornTransformParameters transformParameters;
		DeconToolsV2.Readers.clsRawDataPreprocessOptions fTICRPreProcessOptions;
		DeconToolsV2.DTAGeneration.clsDTAGenerationParameters dTAParameters;

        public ParameterLoader_deprecated()
        {
            this.peakParameters = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            this.transformParameters = new DeconToolsV2.HornTransform.clsHornTransformParameters();
            this.fTICRPreProcessOptions = new DeconToolsV2.Readers.clsRawDataPreprocessOptions();
            this.dTAParameters = new DeconToolsV2.DTAGeneration.clsDTAGenerationParameters();
        }
	

		public DeconToolsV2.Peaks.clsPeakProcessorParameters PeakParameters
		{
			get
			{
				return peakParameters ; 
			}
			set
			{
				peakParameters = value ; 
			}
		}
		public DeconToolsV2.HornTransform.clsHornTransformParameters TransformParameters
		{
			get
			{
				return transformParameters ; 
			}
			set
			{
				transformParameters = value ; 
			}
		}
		public DeconToolsV2.Readers.clsRawDataPreprocessOptions FTICRPreprocessOptions
		{
			get
			{
				return fTICRPreProcessOptions ; 
			}
			set
			{
				fTICRPreProcessOptions = value ; 
			} 
		}
		public DeconToolsV2.DTAGeneration.clsDTAGenerationParameters DTAParameters
		{
			get
			{
				return dTAParameters ; 
			}
			set
			{
				dTAParameters = value ; 
			}
		}
	



    }
}
