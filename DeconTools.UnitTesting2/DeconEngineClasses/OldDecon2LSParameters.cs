#if !Disable_DeconToolsV2
using System;
using System.Xml;
using DeconTools.Backend;
using DeconTools.Backend.Parameters;
using DeconToolsV2.HornTransform;
using DeconToolsV2.Peaks;
using DeconToolsV2.Readers;
using DeconToolsV2.DTAGeneration;

namespace DeconTools.UnitTesting2.DeconEngineClasses
{
    public class OldDecon2LSParameters
    {
        public OldDecon2LSParameters()
        {
            this.HornTransformParameters = new DeconToolsV2.HornTransform.clsHornTransformParameters();
            this.PeakProcessorParameters = new DeconToolsV2.Peaks.clsPeakProcessorParameters();
            this.DTAGenerationParameters = new DeconToolsV2.DTAGeneration.clsDTAGenerationParameters();
            this.FTICRPreProcessParameters = new DeconToolsV2.Readers.clsRawDataPreprocessOptions();

            this.HornTransformParameters.NumScansToAdvance = 1;    //this needs to be changed in DeconEngine
        }

        public clsHornTransformParameters HornTransformParameters { get; set; }

        public clsPeakProcessorParameters PeakProcessorParameters { get; set; }

        public clsDTAGenerationParameters DTAGenerationParameters { get; set; }

        public clsRawDataPreprocessOptions FTICRPreProcessParameters { get; set; }

        public string ParameterFilename { get; set; }

        public void Load(string xmlFileName)
        {
            this.ParameterFilename = xmlFileName;

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

                            if (HornTransformParameters.NumScansToAdvance==0)
                            {
                                HornTransformParameters.NumScansToAdvance = 1;
                            }

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
                var xwriter = new XmlTextWriter(fileName, System.Text.Encoding.UTF8);
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

                this.PeakProcessorParameters.SaveV1PeakParameters(xwriter);
                this.DTAGenerationParameters.SaveV1DTAGenerationParameters(xwriter);
                this.HornTransformParameters.SaveV1HornTransformParameters(xwriter);
                this.HornTransformParameters.SaveV1MiscellaneousParameters(xwriter);
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
                Console.WriteLine(ex.Message);
                Console.WriteLine(PRISM.clsStackTraceFormatter.GetExceptionStackTraceMultiLine(ex));
            }
        }

        public DeconToolsParameters GetDeconToolsParameters()
        {
            var deconToolsParameters = new DeconToolsParameters();

            deconToolsParameters.ThrashParameters.AbsolutePeptideIntensity = HornTransformParameters.AbsolutePeptideIntensity;
            deconToolsParameters.ThrashParameters.AveragineFormula = HornTransformParameters.AveragineFormula;
            deconToolsParameters.ThrashParameters.ChargeCarrierMass = HornTransformParameters.CCMass;
            deconToolsParameters.ThrashParameters.CheckAllPatternsAgainstChargeState1 = HornTransformParameters.CheckAllPatternsAgainstCharge1;
            deconToolsParameters.ThrashParameters.MinIntensityForDeletion = HornTransformParameters.DeleteIntensityThreshold;
            deconToolsParameters.ThrashParameters.UseAbsoluteIntensity = HornTransformParameters.UseAbsolutePeptideIntensity;
            deconToolsParameters.ThrashParameters.CompleteFit = HornTransformParameters.CompleteFit;
            deconToolsParameters.ScanBasedWorkflowParameters.ProcessMS2 = HornTransformParameters.ProcessMSMS;
            deconToolsParameters.MSGeneratorParameters.UseMZRange = HornTransformParameters.UseMZRange;
            deconToolsParameters.ThrashParameters.IsO16O18Data = HornTransformParameters.O16O18Media;
            switch (HornTransformParameters.IsotopeFitType)
            {
                case DeconToolsV2.enmIsotopeFitType.AREA:
                    deconToolsParameters.ThrashParameters.IsotopicProfileFitType = Globals.IsotopicProfileFitType.AREA;
                    break;
                case DeconToolsV2.enmIsotopeFitType.CHISQ:
                    deconToolsParameters.ThrashParameters.IsotopicProfileFitType = Globals.IsotopicProfileFitType.CHISQ;
                    break;
                case DeconToolsV2.enmIsotopeFitType.PEAK:
                    deconToolsParameters.ThrashParameters.IsotopicProfileFitType = Globals.IsotopicProfileFitType.PEAK;
                    break;
                default:
                    deconToolsParameters.ThrashParameters.IsotopicProfileFitType = Globals.IsotopicProfileFitType.Undefined;
                    break;
            }
            deconToolsParameters.ThrashParameters.IsThrashUsed = HornTransformParameters.ThrashOrNot;
            deconToolsParameters.ThrashParameters.LeftFitStringencyFactor = HornTransformParameters.LeftFitStringencyFactor;
            deconToolsParameters.ThrashParameters.MaxCharge = HornTransformParameters.MaxCharge;
            deconToolsParameters.ThrashParameters.MaxFit = HornTransformParameters.MaxFit;
            deconToolsParameters.ThrashParameters.MaxMass = HornTransformParameters.MaxMW;
            deconToolsParameters.MSGeneratorParameters.MaxMZ = HornTransformParameters.MaxMZ; //TODO: review this later
            deconToolsParameters.MSGeneratorParameters.MinMZ = HornTransformParameters.MinMZ; //TODO: review this later
            deconToolsParameters.ThrashParameters.MinIntensityForScore = HornTransformParameters.MinIntensityForScore;
            deconToolsParameters.ThrashParameters.MinMSFeatureToBackgroundRatio = HornTransformParameters.PeptideMinBackgroundRatio;
            deconToolsParameters.ThrashParameters.NumPeaksForShoulder = HornTransformParameters.NumPeaksForShoulder;
            deconToolsParameters.ThrashParameters.RightFitStringencyFactor = HornTransformParameters.RightFitStringencyFactor;
            deconToolsParameters.ThrashParameters.TagFormula = HornTransformParameters.TagFormula;
            deconToolsParameters.ThrashParameters.NumPeaksUsedInAbundance = HornTransformParameters.NumPeaksUsedInAbundance;

            return deconToolsParameters;
        }
    }
}
#endif
