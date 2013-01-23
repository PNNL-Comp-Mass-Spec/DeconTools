using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using GWSGraphLibrary;
using NUnit.Framework;
using ZedGraph;

namespace DeconTools.Workflows.UnitTesting.WorkflowTests
{
    [TestFixture]
    public class SipperWorkflowTests
    {

        [Test]
        public void exportSipperWorkflowParametersTest1()
        {
            string exportedParametersFile = FileRefs.OutputFolderPath + "\\" + "exportedSipperTargetedWorkflowParameters.xml";

            SipperTargetedWorkflowParameters parameters = new SipperTargetedWorkflowParameters();

            parameters.SaveParametersToXML(exportedParametersFile);


        }


        [Test]
        public void loadLCMSFeatures_and_massTags()
        {

            string lcmsfeaturesFile =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2011_02_20_SIPPER_workflow_standards\Yellow_C13_070_23Mar10_Griffin_10-01-28_LCMSFeatures.txt";

            // load LCMSFeatures as targets
            LcmsTargetFromFeaturesFileImporter importer =
                new LcmsTargetFromFeaturesFileImporter(lcmsfeaturesFile);

            var lcmsTargetCollection = importer.Import();


            // load MassTags
            string massTagFile1 =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2011_02_20_SIPPER_workflow_standards\Yellow_C13_070_23Mar10_Griffin_10-01-28_MassTags.txt";

            MassTagFromTextFileImporter massTagImporter = new MassTagFromTextFileImporter(massTagFile1);
            var massTagCollection = massTagImporter.Import();


            var masstagIDlist = (from n in massTagCollection.TargetList select n.ID).ToList();


            // Update LCMSFeatures using MassTag info




            foreach (LcmsFeatureTarget target in lcmsTargetCollection.TargetList)
            {

                if (masstagIDlist.Contains(target.ID))
                {
                    var mt = massTagCollection.TargetList.Where(p => p.ID == target.FeatureToMassTagID).First();
                    target.Code = mt.Code;
                    target.EmpiricalFormula = mt.EmpiricalFormula;
                }
                else
                {

                }

            }

            int[] testMassTags = { 344540889, 344540889, 344972415, 354881152, 355157363, 355162540, 355315129 };

            var filteredLcmsFeatureTargets = (from n in lcmsTargetCollection.TargetList
                                              where testMassTags.Contains(((LcmsFeatureTarget)n).FeatureToMassTagID)
                                              select n).ToList();


            foreach (LcmsFeatureTarget filteredLcmsFeatureTarget in filteredLcmsFeatureTargets)
            {
                Console.WriteLine(filteredLcmsFeatureTarget);
            }


        }


        [Test]
        public void executeWorkflowTest1()
        {
            string testFile = FileRefs.SipperRawDataFile;
            string peaksFile =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2011_02_20_SIPPER_workflow_standards\Yellow_C13_070_23Mar10_Griffin_10-01-28_peaks.txt";

            peaksFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_peaks.txt";

            Run run = RunUtilities.CreateAndLoadPeaks(testFile, peaksFile);


            string lcmsfeaturesFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_LCMSFeatures.txt";

            // load LCMSFeatures as targets
            LcmsTargetFromFeaturesFileImporter importer =
                new LcmsTargetFromFeaturesFileImporter(lcmsfeaturesFile);

            var lcmsTargetCollection = importer.Import();


            // load MassTags
            string massTagFile1 =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_MassTags.txt";

            MassTagFromTextFileImporter massTagImporter = new MassTagFromTextFileImporter(massTagFile1);
            var massTagCollection = massTagImporter.Import();


            var masstagIDlist = (from n in massTagCollection.TargetList select n.ID).ToList();


            // Update LCMSFeatures using MassTag info

            int[] testMassTags = { 344540889, 344972415, 354881152, 355157363, 355162540, 355315129, 355054192, 355160150};

            //testMassTags = new[] {344540889};

            //testMassTags = new int[]
            //                   {
            //                       354942933, 355066611, 354882356, 354928525, 17440471, 355046165, 355008036, 354977066,
            //                       355166304, 354879605, 344965520, 354870998, 355084057, 355034961, 354963652, 344969970,
            //                       355157492, 355176429, 355162540, 355244316, 355139611, 355036935, 355044786, 355139579,
            //                       355163558, 355065622, 355167968, 355033793, 354879421, 355090391, 355034165, 354880925,
            //                       344747857, 354879494, 355315129, 344968376, 355022502, 354879347, 354881152, 354879406,
            //                       344968302, 355037491, 355033717, 355034652, 354879414, 344972415, 355039789, 355025762,
            //                       355129038, 355355053, 354879164, 355037247, 355033862, 355034180, 355033668, 344540889,
            //                       355160123, 345073233, 354880183, 354879165, 354879192, 354879842, 355046211, 355033627,
            //                       355030074, 355033620, 354879142, 354879360, 355034183, 354879174, 354879150
            //                   };

            //no enrichment peptides:
            testMassTags = new int[] { 355057553, 355058671, 355084418 };

            //enriched
            //testMassTags = new int[] { 355116553, 355129038, 355160150, 355162540, 355163371 };


            //testMassTags = new int[] { 355008295 };

            //co-elution peptides
            //testMassTags = new int[] {355034154, 355033668, 355154211, 355035781};

            //testMassTags = new int[] { 355033668 };


            //testMassTags = new int[]{355157492};

            var filteredLcmsFeatureTargets = (from n in lcmsTargetCollection.TargetList
                                              where testMassTags.Contains(((LcmsFeatureTarget)n).FeatureToMassTagID)
                                              select n).ToList();


            foreach (LcmsFeatureTarget target in filteredLcmsFeatureTargets)
            {

                if (masstagIDlist.Contains(target.FeatureToMassTagID))
                {
                    var mt = massTagCollection.TargetList.Where(p => p.ID == target.FeatureToMassTagID).First();
                    target.Code = mt.Code;
                    target.EmpiricalFormula = mt.EmpiricalFormula;
                }
                else
                {
                    //get target's code + empirical formula based on averagine model

                }

            }




            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            parameters.ChromPeakSelectorMode = Globals.PeakSelectorMode.ClosestToTarget;

            SipperTargetedWorkflow workflow = new SipperTargetedWorkflow(run, parameters);


            string outputFolder = @"C:\data\temp\SipperOutput";

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }


            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (var target in filteredLcmsFeatureTargets)
            {
                run.CurrentMassTag = target;

                workflow.Execute();

                OutputMassSpectrum(outputFolder, workflow.MassSpectrumXYData, target, workflow.Result as SipperLcmsTargetedResult);


            }
            stopwatch.Stop();

            foreach (var targetedResultBase in (run.ResultCollection.MassTagResultList))
            {
                var result = (SipperLcmsTargetedResult)targetedResultBase.Value;

                

                Console.WriteLine(result + "\t" +   result.Target.IsotopicProfile.Peaklist.Count);
            }

            Console.WriteLine("Total workflow time (ms) = " + stopwatch.ElapsedMilliseconds);
            Console.WriteLine("Time per target (ms) = " + stopwatch.ElapsedMilliseconds/(double)filteredLcmsFeatureTargets.Count);



        }

        private void OutputMassSpectrum(string outputFolder, XYData massSpectrumXYData, TargetBase target, SipperLcmsTargetedResult sipperLcmsTargetedResult)
        {
            var msGraphGenerator = new MSGraphControl();
            msGraphGenerator.SymbolType = SymbolType.None;

            if (massSpectrumXYData==null)
            {
                massSpectrumXYData = new XYData();
                massSpectrumXYData.Xvalues = new double[] {0, 1, 2, 3, 4, 5};
                massSpectrumXYData.Yvalues = new double[] { 0, 1, 2, 3, 4, 5 };
            }


            msGraphGenerator.GenerateGraph(massSpectrumXYData.Xvalues, massSpectrumXYData.Yvalues, target.MZ - 2,
                                           target.MZ + 6);

            string annotation = "fractionC13= " + sipperLcmsTargetedResult.PercentCarbonsLabelled.ToString("0.000") + "\n" +
                                "populationFraction= " + sipperLcmsTargetedResult.PercentPeptideLabelled.ToString("0.000");


            msGraphGenerator.AddAnnotationRelativeAxis(annotation, 0.45, 0.05);

            string outputFilename = outputFolder + Path.DirectorySeparatorChar + target.ID + "_MS.png";
            msGraphGenerator.SaveGraph(outputFilename);

        }
    }
}
