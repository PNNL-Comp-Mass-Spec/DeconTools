using System;
using System.IO;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Runs;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace DeconTools.Workflows.UnitTesting
{
    [TestFixture]
    public class AlignmentResults_Tests
    {
        private Run run;
        private NETAndMassAligner aligner;

        [OneTimeSetUp]
        public void setupTests()
        {
            var rf = new RunFactory();
            run = rf.CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var deconToolsResultFile = Path.Combine(FileRefs.ImportedData, "QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_targetedFeatures.txt");

            var importer = new UnlabelledTargetedResultFromTextImporter(deconToolsResultFile);
            var repo = importer.Import();

            var massTagFile = @"\\protoapps\UserData\Slysz\Data\MassTags\qcshew_standard_file_allMassTags.txt";
            var mtc = new TargetCollection();
            var mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            aligner = new NETAndMassAligner();
            aligner.SetFeaturesToBeAligned(repo.Results);
            aligner.SetReferenceMassTags(mtc.TargetList);

            aligner.Execute(run);
        }

        [Test]
        public void outputAlignmentHeatmapData_Test1()
        {
           
            var sb=new StringBuilder();

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
            var sb = new StringBuilder();

            sb.Append("scan\tbefore\tafter");
            for (var i = 0; i < aligner.Result.Mass_vs_scan_ResidualsBeforeAlignment.Length; i++)
            {
                sb.Append(aligner.Result.Mass_vs_scan_ResidualsScanValues[i]);
                sb.Append("\t");
                sb.Append(aligner.Result.Mass_vs_scan_ResidualsBeforeAlignment[i]);
                sb.Append("\t");
                sb.Append(aligner.Result.Mass_vs_scan_ResidualsAfterAlignment[i]);
                sb.Append(Environment.NewLine);
            }

            Console.WriteLine(sb.ToString());

        }

        [Test]
        public void outputMass_vs_MZResidualsData()
        {
            var sb = new StringBuilder();

            sb.Append("scan\tbefore\tafter");
            sb.Append(Environment.NewLine);
            for (var i = 0; i < aligner.Result.Mass_vs_mz_ResidualsMZValues.Length; i++)
            {
                sb.Append(aligner.Result.Mass_vs_mz_ResidualsMZValues[i]);
                sb.Append("\t");
                sb.Append(aligner.Result.Mass_vs_mz_ResidualsBeforeAlignment[i]);
                sb.Append("\t");
                sb.Append(aligner.Result.Mass_vs_mz_ResidualsAfterAlignment[i]);
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
            var sb = new StringBuilder();

            for (var i = 0; i < aligner.Result.massHistogramData.GetLength(0); i++)
            {
                for (var k = 0; k < aligner.Result.massHistogramData.GetLength(1); k++)
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
            var sb = new StringBuilder();

            for (var i = 0; i < aligner.Result.NETHistogramData.GetLength(0); i++)
            {
                for (var k = 0; k < aligner.Result.NETHistogramData.GetLength(1); k++)
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
            var sb = new StringBuilder();

            for (var i = alignmentResult.AlignmentHeatmapScores.GetLength(1)-1; i >=0 ; i--)
            {
                for (var k = 0; k < alignmentResult.AlignmentHeatmapScores.GetLength(0); k++)
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
