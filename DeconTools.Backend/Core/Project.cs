using System;
using System.Collections.Generic;
using System.Text;


namespace DeconTools.Backend.Core
{
    public class Project
    {
        private static Project instance;

        private Project()
        {
            this.RunCollection = new List<Run>();
            this.taskCollection = new TaskCollection();
            this.Parameters = new ProjectParameters();


        }

        public static Project getInstance()
        {
            if (instance == null)
            {
                instance = new Project();
            }
            return instance;

        }


  
        
        private TaskCollection taskCollection;

        public TaskCollection TaskCollection
        {
            get { return taskCollection; }
            set { taskCollection = value; }
        }

        private List<Run> runCollection;

        public List<Run> RunCollection
        {
            get { return runCollection; }
            set { runCollection = value; }
        }

        private ProjectParameters parameters;

        public ProjectParameters Parameters
        {
            get { return parameters; }
            set { parameters = value; }
        }

        public void LoadOldDecon2LSParameters(string parameterFilename)
        {
            ParameterLoader loader = new ParameterLoader();
            loader.LoadParametersFromFile(parameterFilename);

            this.Parameters.OldDecon2LSParameters.HornTransformParameters = loader.TransformParameters;
            this.Parameters.OldDecon2LSParameters.PeakProcessorParameters = loader.PeakParameters;
            this.Parameters.OldDecon2LSParameters.FTICRPreProcessParameters = loader.FTICRPreprocessOptions;
            this.Parameters.OldDecon2LSParameters.DTAGenerationParameters = loader.DTAParameters;

            if (this.Parameters.OldDecon2LSParameters.HornTransformParameters.NumScansToAdvance < 1)
            {
                this.Parameters.OldDecon2LSParameters.HornTransformParameters.NumScansToAdvance = 1;     // ensure that minimum is '1'
            }

            
            if (this.Parameters.OldDecon2LSParameters.HornTransformParameters.SumSpectraAcrossScanRange)
            {
                this.Parameters.NumScansSummed = this.Parameters.OldDecon2LSParameters.HornTransformParameters.NumScansToSumOver * 2 + 1;   //I'm changing the format!
            }
            else
            {
                this.Parameters.NumScansSummed = 1;
            }
            
            this.Parameters.NumFramesSummed = this.Parameters.OldDecon2LSParameters.HornTransformParameters.NumFramesToSumOver;


        }







        public static void Reset()
        {
            instance = null;
        }
    }
}
