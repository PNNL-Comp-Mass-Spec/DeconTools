using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Core
{
    public class OldDecon2LSParameters
    {
        public OldDecon2LSParameters()
        {
            this.hornTransformParameters = new DeconToolsV2.HornTransform.clsHornTransformParameters();
            this.peakProcessorParameters = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            this.dTAGenerationParameters = new DeconToolsV2.DTAGeneration.clsDTAGenerationParameters();
            this.fTICRPreProcessParameters = new DeconToolsV2.Readers.clsRawDataPreprocessOptions();

        }

        private DeconToolsV2.HornTransform.clsHornTransformParameters hornTransformParameters;

        public DeconToolsV2.HornTransform.clsHornTransformParameters HornTransformParameters
        {
            get { return hornTransformParameters; }
            set { hornTransformParameters = value; }
        }
        private DeconToolsV2.Peaks.clsPeakProcessorParameters peakProcessorParameters;

        public DeconToolsV2.Peaks.clsPeakProcessorParameters PeakProcessorParameters
        {
            get { return peakProcessorParameters; }
            set { peakProcessorParameters = value; }
        }
        private DeconToolsV2.DTAGeneration.clsDTAGenerationParameters dTAGenerationParameters;

        public DeconToolsV2.DTAGeneration.clsDTAGenerationParameters DTAGenerationParameters
        {
            get { return dTAGenerationParameters; }
            set { dTAGenerationParameters = value; }
        }
        private DeconToolsV2.Readers.clsRawDataPreprocessOptions fTICRPreProcessParameters;

        public DeconToolsV2.Readers.clsRawDataPreprocessOptions FTICRPreProcessParameters
        {
            get { return fTICRPreProcessParameters; }
            set { fTICRPreProcessParameters = value; }
        }





    }
}
