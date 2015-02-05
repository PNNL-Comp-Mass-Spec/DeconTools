using System;
using System.Collections.Generic;
using System.Text;
using DeconTools.Backend.ProcessingTasks.MSGenerators;

namespace DeconTools.Backend.ProcessingTasks
{
    public class MSGeneratorFactory
    {

        public MSGeneratorFactory()
        {

        }
        
        public static MSGenerator CreateMSGenerator(Globals.MSFileType filetype)
        {
            MSGenerator msGenerator;
            
            switch (filetype)
            {
                case Globals.MSFileType.Undefined:
                    msGenerator = new GenericMSGenerator();
                    break;
                case Globals.MSFileType.Agilent_WIFF:
                    msGenerator = new GenericMSGenerator();
                    break;
                case Globals.MSFileType.Agilent_D:
                    msGenerator = new GenericMSGenerator();
                    break;

                case Globals.MSFileType.Ascii:
                    msGenerator = new GenericMSGenerator();
                    break;
                case Globals.MSFileType.Bruker:
                    msGenerator = new GenericMSGenerator();
                    break;
                case Globals.MSFileType.Bruker_Ascii:
                    msGenerator = new GenericMSGenerator();
                    break;
                case Globals.MSFileType.Finnigan:
                    msGenerator = new GenericMSGenerator();
                    break;
                case Globals.MSFileType.ICR2LS_Rawdata:
                    msGenerator = new GenericMSGenerator();
                    break;
                case Globals.MSFileType.Micromass_Rawdata:
                    msGenerator = new GenericMSGenerator();
                    break;
                case Globals.MSFileType.MZXML_Rawdata:
                    msGenerator = new GenericMSGenerator();
                    break;
                case Globals.MSFileType.PNNL_IMS:
                    msGenerator = new GenericMSGenerator();
                    break;
                case Globals.MSFileType.PNNL_UIMF:
                    msGenerator = new UIMF_MSGenerator();
                    break;
                case Globals.MSFileType.SUNEXTREL:
                    msGenerator = new GenericMSGenerator();
                    break;
                default:
                    msGenerator = new GenericMSGenerator();
                    break;
            }
            return msGenerator;

        }

#if !Disable_DeconToolsV2
        internal Core.Task CreateMSGenerator(Globals.MSFileType fileType, DeconTools.Backend.Core.OldDecon2LSParameters oldDecon2LSParameters)
        {
            MSGenerator msgenerator = CreateMSGenerator(fileType);

            if (oldDecon2LSParameters.HornTransformParameters.UseMZRange)
            {
                msgenerator.MinMZ = oldDecon2LSParameters.HornTransformParameters.MinMZ;
                msgenerator.MaxMZ = oldDecon2LSParameters.HornTransformParameters.MaxMZ;
            }
            return msgenerator;
            
        }
#endif

    }
}
