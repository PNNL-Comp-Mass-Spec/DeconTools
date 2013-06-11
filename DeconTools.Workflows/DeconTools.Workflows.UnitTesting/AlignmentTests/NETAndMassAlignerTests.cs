using System;
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
    public class NETAndMassAlignerTests
    {
        [Test]
        public void ensureAlignment_was_executed()
        {
            Run run = new RunFactory().CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            string alignmentFeaturesFile = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\AlignmentInfo\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_READONLY_alignedFeatures.txt";

            UnlabelledTargetedResultFromTextImporter importer = new UnlabelledTargetedResultFromTextImporter(alignmentFeaturesFile);
            TargetedResultRepository repo = importer.Import();

            string massTagFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Targets\QCShew_Formic_MassTags_Bin10_all.txt";

            TargetCollection mtc = new TargetCollection();
            MassTagFromTextFileImporter mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();


            NETAndMassAlignerParameters parameters = new NETAndMassAlignerParameters();
            NETAndMassAligner aligner = new NETAndMassAligner();

            aligner.SetFeaturesToBeAligned(repo.Results);
            aligner.SetReferenceMassTags(mtc.TargetList);

            int scan = 7835;
            double theorMZ = 780.08485;    //massTagID = 24701 (+3)

            double obsMZ = 780.0824;

            var netBeforeAlignment = run.NetAlignmentInfo.GetNETValueForScan(scan);
            double mzBeforeAlignment = run.GetAlignedMZ(obsMZ);

            aligner.Execute(run);

            var netAfterAlignment = run.NetAlignmentInfo.GetNETValueForScan(scan);
            double mzAfterAlignment = run.GetAlignedMZ(obsMZ);
            
            Console.WriteLine("NET before alignment = " + netBeforeAlignment);
            Console.WriteLine("NET after alignment = " + netAfterAlignment);

            Console.WriteLine("Theor MZ =  " + theorMZ);
            Console.WriteLine("MZ before alignment = " + mzBeforeAlignment);
            Console.WriteLine("MZ after alignment =  " + mzAfterAlignment);
            Console.WriteLine("PPMDiff before alignment = " + (theorMZ - mzBeforeAlignment)/theorMZ*1e6);
            Console.WriteLine("PPMDiff after alignment =  " + (theorMZ - mzAfterAlignment) / theorMZ * 1e6);

        }

    }
}
