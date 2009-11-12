using System;
using System.Xml ; 
using System.IO ;

namespace Decon2LS
{
	/// <summary>
	/// Summary description for clsParameterLoader.
	/// </summary>
	public class clsParameterLoader
	{
		DeconToolsV2.Peaks.clsPeakProcessorParameters mobjPeakParameters = new DeconToolsV2.Peaks.clsPeakProcessorParameters() ; 
		DeconToolsV2.HornTransform.clsHornTransformParameters mobjTransformParameters = new DeconToolsV2.HornTransform.clsHornTransformParameters() ; 
		DeconToolsV2.Readers.clsRawDataPreprocessOptions mobjFTICRPreProcessOptions = new DeconToolsV2.Readers.clsRawDataPreprocessOptions() ; 
		DeconToolsV2.DTAGeneration.clsDTAGenerationParameters mobjDTAParameters = new DeconToolsV2.DTAGeneration.clsDTAGenerationParameters()  ; 

		public clsParameterLoader()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public DeconToolsV2.Peaks.clsPeakProcessorParameters PeakParameters
		{
			get
			{
				return mobjPeakParameters ; 
			}
			set
			{
				mobjPeakParameters = value ; 
			}
		}

		public DeconToolsV2.HornTransform.clsHornTransformParameters TransformParameters
		{
			get
			{
				return mobjTransformParameters ; 
			}
			set
			{
				mobjTransformParameters = value ; 
			}
		}

		public DeconToolsV2.Readers.clsRawDataPreprocessOptions FTICRPreprocessOptions
		{
			get
			{
				return mobjFTICRPreProcessOptions ; 
			}
			set
			{
				mobjFTICRPreProcessOptions = value ; 
			} 
		}

		public DeconToolsV2.DTAGeneration.clsDTAGenerationParameters DTAParameters
		{
			get
			{
				return mobjDTAParameters ; 
			}
			set
			{
				mobjDTAParameters = value ; 
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
							mobjPeakParameters.LoadV1PeakParameters(rdr) ; 
						}
						else if (rdr.Name == "HornTransformParameters")
						{
							mobjTransformParameters.LoadV1HornTransformParameters(rdr) ;
						}
						else if (rdr.Name == "Miscellaneous")
						{
							mobjTransformParameters.LoadV1MiscellaneousParameters(rdr) ; 
						}
						else if (rdr.Name == "FTICRRawPreProcessingOptions")
						{
							mobjFTICRPreProcessOptions.LoadV1FTICRPreProcessOptions(rdr) ; 
						}
						else if (rdr.Name == "ElementIsotopes")
						{
							mobjTransformParameters.ElementIsotopeComposition.LoadV1ElementIsotopes(rdr) ; 
						}
						else if (rdr.Name == "DTAGenerationParameters") 
						{
							mobjDTAParameters.LoadV1DTAGenerationParameters(rdr) ; 
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

				mobjPeakParameters.SaveV1PeakParameters(xwriter) ; 
				mobjDTAParameters.SaveV1DTAGenerationParameters(xwriter) ; 
				mobjTransformParameters.SaveV1HornTransformParameters(xwriter) ; 
				mobjTransformParameters.SaveV1MiscellaneousParameters(xwriter) ; 
				if (mobjFTICRPreProcessOptions.IsToBePreprocessed)
					mobjFTICRPreProcessOptions.SaveV1FTICRPreProcessOptions(xwriter) ; 

				// And the isotope abundances.
				mobjTransformParameters.ElementIsotopeComposition.SaveV1ElementIsotopes(xwriter) ; 

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
