using System;
using System.Xml;
using DeconToolsV2.DTAGeneration;
using DeconToolsV2.HornTransform;
using DeconToolsV2.Peaks;
using DeconToolsV2.Readers;

namespace DeconTools.Backend.Core
{
    public class OldDecon2LSParameters
    {



        public OldDecon2LSParameters()
        {
            this.HornTransformParameters = new DeconToolsV2.HornTransform.clsHornTransformParameters();
            this.PeakProcessorParameters = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            this.DTAGenerationParameters = new DeconToolsV2.DTAGeneration.clsDTAGenerationParameters();
            this.FTICRPreProcessParameters = new DeconToolsV2.Readers.clsRawDataPreprocessOptions();

            HornTransformParameters.NumScansToAdvance = 1;    //this needs to be changed in DeconEngine

        }

        public clsHornTransformParameters HornTransformParameters { get; set; }

        public clsPeakProcessorParameters PeakProcessorParameters { get; set; }

        public clsDTAGenerationParameters DTAGenerationParameters { get; set; }

        public clsRawDataPreprocessOptions FTICRPreProcessParameters { get; set; }

        public string ParameterFilename { get; set; }


        public void Load(string xmlFileName)
        {
            ParameterFilename = xmlFileName;

            var rdr = new XmlTextReader(xmlFileName);

            //Read each node in the tree.
            while (rdr.Read())
            {
                switch (rdr.NodeType)
                {
                    case XmlNodeType.Element:
                        if (rdr.Name == "PeakParameters")
                        {
                            PeakProcessorParameters.LoadV1PeakParameters(rdr);
                        }
                        else if (rdr.Name == "HornTransformParameters")
                        {
                            HornTransformParameters.LoadV1HornTransformParameters(rdr);
                        }
                        else if (rdr.Name == "Miscellaneous")
                        {
                            HornTransformParameters.LoadV1MiscellaneousParameters(rdr);
                        }
                        else if (rdr.Name == "FTICRRawPreProcessingOptions")
                        {
                            FTICRPreProcessParameters.LoadV1FTICRPreProcessOptions(rdr);
                        }
                        else if (rdr.Name == "ElementIsotopes")
                        {
                            HornTransformParameters.ElementIsotopeComposition.LoadV1ElementIsotopes(rdr);
                        }
                        else if (rdr.Name == "DTAGenerationParameters")
                        {
                            DTAGenerationParameters.LoadV1DTAGenerationParameters(rdr);
                        }
                        break;
                    default:
                        break;
                }
            }
            rdr.Close();
        }

        public void Save(string fileName)
        {
            try
            {
                //Create a new XmlTextWriter.
                //				XmlTextWriter xwriter = new XmlTextWriter(Console.Out);
                XmlTextWriter xwriter = new XmlTextWriter(fileName, System.Text.Encoding.UTF8);
                xwriter.Formatting = Formatting.None;
                xwriter.IndentChar = '\t';
                xwriter.Indentation = 1;

                //Write the beginning of the document including the 
                //document declaration. Standalone is true. 

                xwriter.WriteStartDocument(true);

                //Write the beginning of the "data" element. This is 
                //the opening tag to our data 

                xwriter.WriteWhitespace("\n");
                xwriter.WriteStartElement("parameters");
                xwriter.WriteWhitespace("\n\t");
                xwriter.WriteElementString("version", "1.0");
                xwriter.WriteWhitespace("\n\t");

                PeakProcessorParameters.SaveV1PeakParameters(xwriter);
                DTAGenerationParameters.SaveV1DTAGenerationParameters(xwriter);
                HornTransformParameters.SaveV1HornTransformParameters(xwriter);
                HornTransformParameters.SaveV1MiscellaneousParameters(xwriter);
                if (FTICRPreProcessParameters.IsToBePreprocessed)
                    FTICRPreProcessParameters.SaveV1FTICRPreProcessOptions(xwriter);

                // And the isotope abundances.
                HornTransformParameters.ElementIsotopeComposition.SaveV1ElementIsotopes(xwriter);

                xwriter.WriteEndElement();

                //End the document
                xwriter.WriteEndDocument();
                xwriter.WriteWhitespace("\n");

                //Flush the xml document to the underlying stream and
                //close the underlying stream. The data will not be
                //written out to the stream until either the Flush()
                //method is called or the Close() method is called.
                xwriter.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }





    }
}
