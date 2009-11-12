using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace DeconTools.Backend
{
    public class ParameterLoader
    {

        DeconToolsV2.Peaks.clsPeakProcessorParameters peakParameters;
		DeconToolsV2.HornTransform.clsHornTransformParameters transformParameters;
		DeconToolsV2.Readers.clsRawDataPreprocessOptions fTICRPreProcessOptions;
		DeconToolsV2.DTAGeneration.clsDTAGenerationParameters dTAParameters;

        public ParameterLoader()
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
		public void LoadParametersFromFile(string fileName)
		{

			XmlTextReader rdr = new XmlTextReader(fileName);

			//Read each node in the tree.
			while (rdr.Read())
			{
				switch (rdr.NodeType)
				{
					case XmlNodeType.Element:
						if (rdr.Name == "PeakParameters")
						{
							peakParameters.LoadV1PeakParameters(rdr) ; 
						}
						else if (rdr.Name == "HornTransformParameters")
						{
							transformParameters.LoadV1HornTransformParameters(rdr) ;
						}
						else if (rdr.Name == "Miscellaneous")
						{
							transformParameters.LoadV1MiscellaneousParameters(rdr) ; 
						}
						else if (rdr.Name == "FTICRRawPreProcessingOptions")
						{
							fTICRPreProcessOptions.LoadV1FTICRPreProcessOptions(rdr) ; 
						}
						else if (rdr.Name == "ElementIsotopes")
						{
							transformParameters.ElementIsotopeComposition.LoadV1ElementIsotopes(rdr) ; 
						}
						else if (rdr.Name == "DTAGenerationParameters") 
						{
							dTAParameters.LoadV1DTAGenerationParameters(rdr) ; 
						}
						break;
					default:
						break ; 
				}
			}
			rdr.Close() ; 
		}

		public void SaveParametersToFile(string fileName)
		{
			try
			{
				//Create a new XmlTextWriter.
				//				XmlTextWriter xwriter = new XmlTextWriter(Console.Out);
				XmlTextWriter xwriter = new XmlTextWriter(fileName,System.Text.Encoding.UTF8);
				xwriter.Formatting = Formatting.None;
				xwriter.IndentChar = '\t' ;
				xwriter.Indentation = 1 ; 

				//Write the beginning of the document including the 
				//document declaration. Standalone is true. 

				xwriter.WriteStartDocument(true);

				//Write the beginning of the "data" element. This is 
				//the opening tag to our data 

				xwriter.WriteWhitespace("\n") ; 
				xwriter.WriteStartElement("parameters");
				xwriter.WriteWhitespace("\n\t") ; 
				xwriter.WriteElementString("version", "1.0") ; 
				xwriter.WriteWhitespace("\n\t") ; 

				peakParameters.SaveV1PeakParameters(xwriter) ; 
				dTAParameters.SaveV1DTAGenerationParameters(xwriter) ; 
				transformParameters.SaveV1HornTransformParameters(xwriter) ; 
				transformParameters.SaveV1MiscellaneousParameters(xwriter) ; 
				if (fTICRPreProcessOptions.IsToBePreprocessed)
					fTICRPreProcessOptions.SaveV1FTICRPreProcessOptions(xwriter) ; 

				// And the isotope abundances.
				transformParameters.ElementIsotopeComposition.SaveV1ElementIsotopes(xwriter) ; 

				xwriter.WriteEndElement();
				
				//End the document
				xwriter.WriteEndDocument();
				xwriter.WriteWhitespace("\n") ; 

				//Flush the xml document to the underlying stream and
				//close the underlying stream. The data will not be
				//written out to the stream until either the Flush()
				//method is called or the Close() method is called.
				xwriter.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message + ex.StackTrace) ;
			}
		}



    }
}
