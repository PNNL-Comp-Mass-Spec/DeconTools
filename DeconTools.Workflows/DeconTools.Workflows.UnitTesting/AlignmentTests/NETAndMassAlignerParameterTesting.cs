﻿using System;
using System.Collections.Generic;
using System.Linq;
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
    public class NETAndMassAlignerParameterTesting
    {
        [Test]
        public void AlignmentParameterTesting1()
        {
            var run = new RunFactory().CreateRun(DeconTools.UnitTesting2.FileRefs.RawDataMSFiles.OrbitrapStdFile1);

            var alignmentFeaturesFile = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_READONLY_alignedFeatures.txt";

            var importer = new UnlabeledTargetedResultFromTextImporter(alignmentFeaturesFile);
            var repo = importer.Import();

            var massTagFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Targets\QCShew_Formic_MassTags_Bin10_all.txt";

            var mtc = new TargetCollection();
            var mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            var parameters = new NETAndMassAlignerParameters();
            var aligner = new NETAndMassAligner();

            aligner.SetFeaturesToBeAligned(repo.Results);
            aligner.SetReferenceMassTags(mtc.TargetList);

            var sb = new StringBuilder();
            sb.Append("theorMZ\tobsMZ\talignedMZ\tppmErrorBefore\tppmErrorAfter\n");

            parameters.MassCalibrationWindow = 50;
            parameters.MassCalibrationNumMassDeltaBins = 100;

            int[]massCalXSliceValues = {3,6,9,12,15,20,30,50};

            foreach (var xsliceVal in massCalXSliceValues)
            {
                var ppmErrorsBefore = new List<double>();
                var ppmErrorsAfter = new List<double>();

                parameters.MassCalibrationNumXSlices =(short)xsliceVal;
                aligner.AlignerParameters = parameters;
                aligner.Execute(run);

                foreach (var result in repo.Results)
                {
                    var mt = mtc.TargetList.Where(p => p.ID == result.TargetID).First(p => p.ChargeState == result.ChargeState);
                    var theorMZ = mt.MZ;
                    var obsMZ = result.MonoMZ;
                    double scan = result.ScanLC;
                    var alignedMZ = run.GetAlignedMZ(obsMZ, scan);
                    var ppmErrorBefore = (theorMZ - obsMZ) / theorMZ * 1e6;
                    var ppmErrorAfter = (theorMZ - alignedMZ) / theorMZ * 1e6;

                    sb.Append(result.TargetID).Append('\t').Append(result.ScanLC).Append('\t').Append(theorMZ.ToString("0.00000")).Append('\t').Append(obsMZ.ToString("0.00000")).Append('\t').Append(alignedMZ.ToString("0.00000")).Append('\t').Append(ppmErrorBefore.ToString("0.0")).Append('\t').Append(ppmErrorAfter.ToString("0.0"));
                    sb.Append(Environment.NewLine);

                    ppmErrorsAfter.Add(ppmErrorAfter);
                    ppmErrorsBefore.Add(ppmErrorBefore);
                }

                Console.WriteLine(xsliceVal + "\t" + ppmErrorsBefore.Average().ToString("0.00") + "\t"+ ppmErrorsAfter.Average().ToString("0.00"));

                //Console.WriteLine(sb.ToString());
                //Console.WriteLine();
                //Console.WriteLine();
                //Console.WriteLine("Average ppm error before alignment = " + ppmErrorsBefore.Average().ToString("0.00"));
                //Console.WriteLine("Average ppm error after alignment = " + ppmErrorsAfter.Average().ToString("0.00"));

            }
        }

        [Test]
        public void AlignmentParameterTesting2()
        {
            var run = new RunFactory().CreateRun(@"D:\Data\Orbitrap\Subissue01\QC_Shew_10_01-pt5-1_8Feb10_Doc_09-12-24.RAW");

            var alignmentFeaturesFile = @"\\protoapps\UserData\Slysz\Data\QCShew_MassiveTargeted\AlignmentInfo\QC_Shew_10_01-pt5-1_8Feb10_Doc_09-12-24_alignedFeatures.txt";

            var importer = new UnlabeledTargetedResultFromTextImporter(alignmentFeaturesFile);
            var repo = importer.Import();

            var massTagFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\Unlabelled\Targets\QCShew_Formic_MassTags_Bin10_all.txt";

            var mtc = new TargetCollection();
            var mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            var parameters = new NETAndMassAlignerParameters();
            var aligner = new NETAndMassAligner();

            aligner.SetFeaturesToBeAligned(repo.Results);
            aligner.SetReferenceMassTags(mtc.TargetList);

            var sb = new StringBuilder();
            sb.Append("mtid\tscanLC\ttheorMZ\tobsMZ\talignedMZ\tppmErrorBefore\tppmErrorAfter\n");

            parameters.MassCalibrationWindow = 20;
            parameters.MassCalibrationNumMassDeltaBins = 100;

           // int[] massCalXSliceValues = { 3, 6, 9, 12, 15 };

            int[] massCalXSliceValues = { 15 };

            foreach (var xsliceVal in massCalXSliceValues)
            {
                var ppmErrorsBefore = new List<double>();
                var ppmErrorsAfter = new List<double>();

                parameters.MassCalibrationNumXSlices = (short)xsliceVal;
                aligner.AlignerParameters = parameters;
                aligner.Execute(run);

                foreach (var result in repo.Results)
                {
                    var mt = mtc.TargetList.Where(p => p.ID == result.TargetID).First(p => p.ChargeState == result.ChargeState);
                    var theorMZ = mt.MZ;
                    var obsMZ = result.MonoMZ;
                    double scan = result.ScanLC;
                    var alignedMZ = run.GetAlignedMZ(obsMZ, scan);
                    var ppmErrorBefore = (theorMZ - obsMZ) / theorMZ * 1e6;
                    var ppmErrorAfter = (theorMZ - alignedMZ) / theorMZ * 1e6;

                    sb.Append(result.TargetID).Append('\t').Append(result.ScanLC).Append('\t').Append(theorMZ.ToString("0.00000")).Append('\t').Append(obsMZ.ToString("0.00000")).Append('\t').Append(alignedMZ.ToString("0.00000")).Append('\t').Append(ppmErrorBefore.ToString("0.0")).Append('\t').Append(ppmErrorAfter.ToString("0.0"));
                    sb.Append(Environment.NewLine);

                    ppmErrorsAfter.Add(ppmErrorAfter);
                    ppmErrorsBefore.Add(ppmErrorBefore);
                }

                Console.WriteLine(sb.ToString());
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Average ppm error before alignment = " + ppmErrorsBefore.Average().ToString("0.00"));
                Console.WriteLine("Average ppm error after alignment = " + ppmErrorsAfter.Average().ToString("0.00"));
                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine(xsliceVal + "\t" + ppmErrorsBefore.Average().ToString("0.00") + "\t" + ppmErrorsAfter.Average().ToString("0.00"));
            }
        }

        [Test]
        public void AlignmentParameterTesting3()
        {
            var run = new RunFactory().CreateRun(@"D:\Data\Orbitrap\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW");

            var alignmentFeaturesFile = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\AlignmentInfo\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18_READONLY_alignedFeatures.txt";
            var importer = new UnlabeledTargetedResultFromTextImporter(alignmentFeaturesFile);
            var repo = importer.Import();

            var massTagFile = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\QCShew_Formic_MassTags_Bin10_all.txt";

            var mtc = new TargetCollection();
            var mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            var parameters = new NETAndMassAlignerParameters();
            var aligner = new NETAndMassAligner();

            aligner.SetFeaturesToBeAligned(repo.Results);
            aligner.SetReferenceMassTags(mtc.TargetList);

            var sb = new StringBuilder();
            sb.Append("mtid\tscanLC\ttheorMZ\tobsMZ\talignedMZ\tppmErrorBefore\tppmErrorAfter\n");

            parameters.MassCalibrationWindow = 50;
            parameters.MassCalibrationNumMassDeltaBins = 100;

            int[] massCalXSliceValues = { 3, 6, 9, 12, 15, 16, 17, 18, 19, 20 };

            //int[] massCalXSliceValues = { 9 };

            foreach (var xsliceVal in massCalXSliceValues)
            {
                var ppmErrorsBefore = new List<double>();
                var ppmErrorsAfter = new List<double>();

                parameters.MassCalibrationNumXSlices = (short)xsliceVal;
                aligner.AlignerParameters = parameters;
                aligner.Execute(run);

                foreach (var result in repo.Results)
                {
                    var mt = mtc.TargetList.Where(p => p.ID == result.TargetID).First(p => p.ChargeState == result.ChargeState);
                    var theorMZ = mt.MZ;
                    var obsMZ = result.MonoMZ;
                    double scan = result.ScanLC;
                    var alignedMZ = run.GetAlignedMZ(obsMZ, scan);
                    var ppmErrorBefore = (theorMZ - obsMZ) / theorMZ * 1e6;
                    var ppmErrorAfter = (theorMZ - alignedMZ) / theorMZ * 1e6;

                    sb.Append(result.TargetID).Append('\t').Append(result.ScanLC).Append('\t').Append(theorMZ.ToString("0.00000")).Append('\t').Append(obsMZ.ToString("0.00000")).Append('\t').Append(alignedMZ.ToString("0.00000")).Append('\t').Append(ppmErrorBefore.ToString("0.0")).Append('\t').Append(ppmErrorAfter.ToString("0.0"));
                    sb.Append(Environment.NewLine);

                    ppmErrorsAfter.Add(ppmErrorAfter);
                    ppmErrorsBefore.Add(ppmErrorBefore);
                }

                //Console.WriteLine(sb.ToString());
                //Console.WriteLine();
                //Console.WriteLine();
                //Console.WriteLine("Average ppm error before alignment = " + filterWithGrubbsApplied(ppmErrorsBefore).Average().ToString("0.00"));
                //Console.WriteLine("Average ppm error after alignment = " + filterWithGrubbsApplied(ppmErrorsAfter).Average().ToString("0.00"));
                //Console.WriteLine();
                //Console.WriteLine();

                Console.WriteLine(xsliceVal + "\t" + filterWithGrubbsApplied(ppmErrorsBefore).Average().ToString("0.00") + "\t" + filterWithGrubbsApplied(ppmErrorsAfter).Average().ToString("0.00"));
            }
        }

        [Test]
        public void Issue0724_AlignmentProblemsTest1()
        {
            var run = new RunFactory().CreateRun(@"D:\Data\Orbitrap\Subissue01\QC_Shew_10_01-pt5-1_8Feb10_Doc_09-12-24.RAW");

            var alignmentFeaturesFile = @"D:\Data\Orbitrap\Subissue01\QC_Shew_10_01-pt5-1_8Feb10_Doc_09-12-24_alignedFeatures.txt";

            var importer = new UnlabeledTargetedResultFromTextImporter(alignmentFeaturesFile);
            var repo = importer.Import();

            var massTagFile = @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\QCShew_Formic_MassTags_Bin10_all.txt";

            var mtc = new TargetCollection();
            var mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            var parameters = new NETAndMassAlignerParameters();
            var aligner = new NETAndMassAligner();

            aligner.SetFeaturesToBeAligned(repo.Results);
            aligner.SetReferenceMassTags(mtc.TargetList);

            var sb = new StringBuilder();
            sb.Append("mtid\tscanLC\ttheorMZ\tobsMZ\talignedMZ\tppmErrorBefore\tppmErrorAfter\n");

            parameters.MassCalibrationWindow = 50;
            parameters.MassCalibrationNumMassDeltaBins = 100;

             int[] massCalXSliceValues = { 3, 6, 9, 12, 15, 16, 17, 18, 19, 20 };

            //int[] massCalXSliceValues = { 6 };

            foreach (var xsliceVal in massCalXSliceValues)
            {
                var ppmErrorsBefore = new List<double>();
                var ppmErrorsAfter = new List<double>();

                parameters.MassCalibrationNumXSlices = (short)xsliceVal;
                aligner.AlignerParameters = parameters;
                aligner.Execute(run);

                foreach (var result in repo.Results)
                {
                    var mt = mtc.TargetList.Where(p => p.ID == result.TargetID).First(p => p.ChargeState == result.ChargeState);
                    var theorMZ = mt.MZ;
                    var obsMZ = result.MonoMZ;
                    double scan = result.ScanLC;
                    var alignedMZ = run.GetAlignedMZ(obsMZ, scan);
                    var ppmErrorBefore = (theorMZ - obsMZ) / theorMZ * 1e6;
                    var ppmErrorAfter = (theorMZ - alignedMZ) / theorMZ * 1e6;

                    sb.Append(result.TargetID).Append('\t').Append(result.ScanLC).Append('\t').Append(theorMZ.ToString("0.00000")).Append('\t').Append(obsMZ.ToString("0.00000")).Append('\t').Append(alignedMZ.ToString("0.00000")).Append('\t').Append(ppmErrorBefore.ToString("0.0")).Append('\t').Append(ppmErrorAfter.ToString("0.0"));
                    sb.Append(Environment.NewLine);

                    ppmErrorsAfter.Add(ppmErrorAfter);
                    ppmErrorsBefore.Add(ppmErrorBefore);
                }

                //Console.WriteLine(sb.ToString());
                //Console.WriteLine();
                //Console.WriteLine();
                //Console.WriteLine("Average ppm error before alignment = " + filterWithGrubbsApplied(ppmErrorsBefore).Average().ToString("0.00"));
                //Console.WriteLine("Average ppm error after alignment = " + filterWithGrubbsApplied(ppmErrorsAfter).Average().ToString("0.00"));
                //Console.WriteLine();
                //Console.WriteLine();

                Console.WriteLine(xsliceVal + "\t" + filterWithGrubbsApplied(ppmErrorsBefore).Average().ToString("0.00") + "\t" + filterWithGrubbsApplied(ppmErrorsAfter).Average().ToString("0.00"));
            }
        }

        public List<double> filterWithGrubbsApplied(List<double> vals)
        {
            var filteredVals = new List<double>();

            var stdev = getStDev(vals);
            var average = vals.Average();

            var zValue = 2;

            foreach (var item in vals)
            {
                var diff = Math.Abs(item - average);
                if (diff < (stdev * zValue))
                {
                    filteredVals.Add(item);
                }
            }

            return filteredVals;
        }

        public double getStDev(List<double> vals)
        {
            var average = vals.Average();

            double sumSquaredDiffs = 0;
            foreach (var item in vals)
            {
                sumSquaredDiffs += ((item - average) * (item - average));
            }

            var stdev = Math.Sqrt(sumSquaredDiffs / (vals.Count - 1));
            return stdev;
        }

        [Test]
        public void Issue0725_AlignmentProblemsTest1()
        {
            var run = new RunFactory().CreateRun(@"D:\Data\Orbitrap\Issue0725_badAlignment\QC_Shew_10_03-2_100min_06May10_Tiger_10-04-08.RAW");

            var alignmentFeaturesFile = run.DatasetFileOrDirectoryPath.Replace(".RAW", "_alignedFeatures.txt");

            var importer = new UnlabeledTargetedResultFromTextImporter(alignmentFeaturesFile);
            var repo = importer.Import();

            var massTagFile = @"\\protoapps\UserData\Slysz\Data\QCShew_MassiveTargeted\MassTags\QCShew_Formic_MassTags_for_alignment.txt";

            var mtc = new TargetCollection();
            var mtimporter = new MassTagFromTextFileImporter(massTagFile);
            mtc = mtimporter.Import();

            var parameters = new NETAndMassAlignerParameters();
            var aligner = new NETAndMassAligner();

            aligner.SetFeaturesToBeAligned(repo.Results);
            aligner.SetReferenceMassTags(mtc.TargetList);

            var sb = new StringBuilder();
            sb.Append("mtid\tscanLC\ttheorMZ\tobsMZ\talignedMZ\tppmErrorBefore\tppmErrorAfter\n");

            parameters.MassCalibrationWindow = 50;
            parameters.MassCalibrationNumMassDeltaBins = 100;

            int[] massCalXSliceValues = { 3, 6, 9, 10, 12, 15,18,20 };

            //int[] massCalXSliceValues = { 6 };

            foreach (var xsliceVal in massCalXSliceValues)
            {
                var ppmErrorsBefore = new List<double>();
                var ppmErrorsAfter = new List<double>();

                parameters.MassCalibrationNumXSlices = (short)xsliceVal;
                aligner.AlignerParameters = parameters;
                aligner.Execute(run);

                foreach (var result in repo.Results)
                {
                    var mt = mtc.TargetList.Where(p => p.ID == result.TargetID).First(p => p.ChargeState == result.ChargeState);
                    var theorMZ = mt.MZ;
                    var obsMZ = result.MonoMZ;
                    double scan = result.ScanLC;
                    var alignedMZ = run.GetAlignedMZ(obsMZ, scan);
                    var ppmErrorBefore = (theorMZ - obsMZ) / theorMZ * 1e6;
                    var ppmErrorAfter = (theorMZ - alignedMZ) / theorMZ * 1e6;
                    double theorNET = mt.NormalizedElutionTime;
                    double obsNET = result.NET;
                    var alignedNET = run.NetAlignmentInfo.GetNETValueForScan((int) scan);

                    sb.Append(result.TargetID).Append('\t').Append(result.ScanLC).Append('\t').Append(theorMZ.ToString("0.00000")).Append('\t').Append(obsMZ.ToString("0.00000")).Append('\t').Append(alignedMZ.ToString("0.00000")).Append('\t').Append(ppmErrorBefore.ToString("0.0")).Append('\t').Append(ppmErrorAfter.ToString("0.0")).Append('\t').Append(theorNET.ToString("0.0000")).Append('\t').Append(obsNET.ToString("0.0000")).Append('\t').Append(alignedNET.ToString("0.0000"));

                    //sb.Append(result.MassTagID + "\t" + result.ScanLC + "\t" + theo
                    sb.Append(Environment.NewLine);
                    ppmErrorsAfter.Add(ppmErrorAfter);
                    ppmErrorsBefore.Add(ppmErrorBefore);
                }

               // Console.WriteLine(sb.ToString());
                //Console.WriteLine();
                //Console.WriteLine();
                //Console.WriteLine("Average ppm error before alignment = " + filterWithGrubbsApplied(ppmErrorsBefore).Average().ToString("0.00"));
                //Console.WriteLine("Average ppm error after alignment = " + filterWithGrubbsApplied(ppmErrorsAfter).Average().ToString("0.00"));
                //Console.WriteLine();
                //Console.WriteLine();

                Console.WriteLine(xsliceVal + "\t" + filterWithGrubbsApplied(ppmErrorsBefore).Average().ToString("0.00") + "\t" + filterWithGrubbsApplied(ppmErrorsAfter).Average().ToString("0.00"));
            }
        }
    }
}
