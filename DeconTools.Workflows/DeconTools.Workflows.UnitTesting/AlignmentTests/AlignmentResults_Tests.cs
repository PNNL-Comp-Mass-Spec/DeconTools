using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Runs;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;
using System.Text;
using System;

namespace DeconTools.Workflows.UnitTesting
{
    [TestFixture]
    public class AlignmentResults_Tests
    {
        private Run run;
        private NETAndMassAligner aligner;
        [TestFixtureSetUp]
        public void setupTests()
        {
            RunFactory rf = new RunFactory();
            run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            string deconToolsResultFile = FileRefs.ImportedData + "\\" + "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_targetedFeatures.txt";

            UnlabelledTargetedResultFromTextImporter importer = new UnlabelledTargetedResultFromTextImporter(deconToolsResultFile);
            TargetedResultRepository repo = importer.Import();

            string massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\qcshew_standard_file_allMassTags.txt";
            MassTagCollection mtc = new MassTagCollection();
            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            aligner = new NETAndMassAligner();
            aligner.SetFeaturesToBeAligned(repo.Results);
            aligner.SetReferenceMassTags(mtc.MassTagList);

            aligner.Execute(run);
        }

        [Test]
        public void outputAlignmentHeatmapData_Test1()
        {
           
            StringBuilder sb=new StringBuilder();

            foreach (var scan in aligner.Result.ScanLCValues)
            {
                sb.Append(scan);
                sb.Append("\t");
            }

            sb.Append(Environment.NewLine);
         
            foreach (var netval in aligner.Result.NETValues)
            {
                sb.Append(netval);
                sb.Append(Environment.NewLine);
                
            }
            sb.Append(Environment.NewLine);
            Console.WriteLine(sb.ToString());

            Console.WriteLine(displayHeatMapData(aligner.Result));

        }


        [Test]
        public void outputMassErrorData()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("scan\tbefore\tafter");
            for (int i = 0; i < aligner.Result.massErrorResidualsBeforeAlignment.Length; i++)
            {
                sb.Append(aligner.Result.ScanValuesForMassErrorResiduals[i]);
                sb.Append("\t");
                sb.Append(aligner.Result.massErrorResidualsBeforeAlignment[i]);
                sb.Append("\t");
                sb.Append(aligner.Result.massErrorResidualsAfterAlignement[i]);
                sb.Append(Environment.NewLine);
            }

            Console.WriteLine(sb.ToString());

        }

        [Test]
        public void outputMass_vs_MZResidualsData()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("scan\tbefore\tafter");
            sb.Append(Environment.NewLine);
            for (int i = 0; i < aligner.Result.mass_vs_mz_residualsMZValues.Length; i++)
            {
                sb.Append(aligner.Result.mass_vs_mz_residualsMZValues[i]);
                sb.Append("\t");
                sb.Append(aligner.Result.mass_vs_mz_residualsBeforeAlignment[i]);
                sb.Append("\t");
                sb.Append(aligner.Result.mass_vs_mz_residualsAfterAlignment[i]);
                sb.Append(Environment.NewLine);
            }

            Console.WriteLine(sb.ToString());

        }

        [Test]
        public void getStatsOnVariablity()
        {

            Console.WriteLine("MassMean = " + aligner.Result.MassAverage + " +/- " + aligner.Result.MassStDev);
            Console.WriteLine("NETMean = " + aligner.Result.NETAverage + " +/- " + aligner.Result.NETStDev);


        }


        [Test]
        public void getMassErrorHistogramTest1()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < aligner.Result.massHistogramData.GetLength(0); i++)
            {
                for (int k = 0; k < aligner.Result.massHistogramData.GetLength(1); k++)
                {
                    sb.Append(aligner.Result.massHistogramData[i,k]);
                    sb.Append("\t");
                    
                }
                sb.Append(Environment.NewLine);
                
            }

            Console.WriteLine("MassErrorHistogramData:");
            Console.WriteLine(sb.ToString());
        }



        [Test]
        public void getNETErrorHistogramTest1()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < aligner.Result.NETHistogramData.GetLength(0); i++)
            {
                for (int k = 0; k < aligner.Result.NETHistogramData.GetLength(1); k++)
                {
                    sb.Append(aligner.Result.NETHistogramData[i, k]);
                    sb.Append("\t");

                }
                sb.Append(Environment.NewLine);

            }

            Console.WriteLine("NET_ErrorHistogramData:");
            Console.WriteLine(sb.ToString());
        }

       

        private string displayHeatMapData(AlignmentResult alignmentResult)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = alignmentResult.AlignmentHeatmapScores.GetLength(1)-1; i >=0 ; i--)
            {
                for (int k = 0; k < alignmentResult.AlignmentHeatmapScores.GetLength(0); k++)
                {
                    sb.Append(alignmentResult.AlignmentHeatmapScores[k, i]);
                    sb.Append("\t");
                }
                sb.Append(Environment.NewLine);

                



            }
            return sb.ToString();
        }

    }
}
