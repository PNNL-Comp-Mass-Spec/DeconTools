using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data.Importers;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.ProcessingTasks.NETAlignment;
using DeconTools.Backend.Data;
using DeconTools.Backend;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.ResultValidators;

namespace DeconTools.UnitTesting.ProcessingTasksTests.TargetedAnalysisTests
{
    [TestFixture]
    public class O16O18TargetedTests
    {
        string bsaO16O18file1 = @"F:\Gord\Data\O16O18\Weijun\TechTest_O18_01\TechTest_O18_01_RunA_10Dec09_Doc_09-11-08.RAW";
        string bsaPeaksFile1 = @"F:\Gord\Data\O16O18\Weijun\TechTest_O18_01\TechTest_O18_01_RunA_10Dec09_Doc_09-11-08_peaks.txt";

        string bsaO16O18file2 = @"F:\Gord\Data\O16O18\Weijun\TechTest_O18_01\TechTest_O18_01_RunB_10Dec09_Doc_09-11-08.RAW";
        string bsaPeaksFile2 = @"F:\Gord\Data\O16O18\Weijun\TechTest_O18_01\TechTest_O18_01_RunB_10Dec09_Doc_09-11-08_peaks.txt";


        string bsaO16O18file3 = @"F:\Gord\Data\O16O18\Weijun\TechTest_O18_01\TechTest_O18_01_RunC_10Dec09_Doc_09-11-08.RAW";
        string bsaPeaksFile3 = @"F:\Gord\Data\O16O18\Weijun\TechTest_O18_01\TechTest_O18_01_RunC_10Dec09_Doc_09-11-08_peaks.txt";

        string bsaO16O18file4 = @"F:\Gord\Data\O16O18\Weijun\TechTest_O18_01\TechTest_O18_01_RunD_10Dec09_Doc_09-11-08.RAW";
        string bsaPeaksFile4 = @"F:\Gord\Data\O16O18\Weijun\TechTest_O18_01\TechTest_O18_01_RunD_10Dec09_Doc_09-11-08_peaks.txt";

        string bsaO16O18file5 = @"F:\Gord\Data\O16O18\Weijun\TechTest_O18_01\TechTest_O18_01_RunE_10Dec09_Doc_09-11-08.RAW";
        string bsaPeaksFile5 = @"F:\Gord\Data\O16O18\Weijun\TechTest_O18_01\TechTest_O18_01_RunE_10Dec09_Doc_09-11-08_peaks.txt";


        string bsaDBName = "MT_BSA_P171";
        string bsaDBServer = "Albert";


        string mousePlasmaFile1 = @"F:\Gord\Data\O16O18\Weijun\TechTest_O18_02\TechTest_O18_02_RunA_10Dec09_Doc_09-11-08.RAW";
        string mousePlasmaFile2 = @"F:\Gord\Data\O16O18\Weijun\TechTest_O18_02\TechTest_O18_02_RunB_10Dec09_Doc_09-11-08.RAW";
        string mousePlasmaFile3 = @"F:\Gord\Data\O16O18\Weijun\TechTest_O18_02\TechTest_O18_02_RunC_10Dec09_Doc_09-11-08.RAW";
        string mousePlasmaFile4 = @"F:\Gord\Data\O16O18\Weijun\TechTest_O18_02\TechTest_O18_02_RunD_10Dec09_Doc_09-11-08.RAW";
        string mousePlasmaFile5 = @"F:\Gord\Data\O16O18\Weijun\TechTest_O18_02\TechTest_O18_02_RunE_17Dec09_Doc_09-11-08.RAW";






        [Test]
        public void getBSA_targetedTests1()
        {

            int numMSScansSummed = 5;

            List<Run> runList = new List<Run>();
            runList.Add(new XCaliburRun(bsaO16O18file1));
            runList.Add(new XCaliburRun(bsaO16O18file2));
            runList.Add(new XCaliburRun(bsaO16O18file3));
            runList.Add(new XCaliburRun(bsaO16O18file4));
            runList.Add(new XCaliburRun(bsaO16O18file5));




            MassTagCollection mtColl = GetBSAMassTags();

            foreach (var run in runList)
            {
                string peakFileName = run.DataSetPath + "\\"+run.DatasetName + "_peaks.txt";

                PeakImporterFromText peakImporter = new PeakImporterFromText(peakFileName);
                peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);

                TomTheorFeatureGenerator theorFeatureGenerator = new TomTheorFeatureGenerator();
                ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
                chromAligner.Execute(run);

                Task peakChromGen = new PeakChromatogramGenerator(10);

                Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);
                Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(0.5, 0.5);
                Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(numMSScansSummed, 0.1, Globals.PeakSelectorMode.CLOSEST_TO_TARGET);

                MSGeneratorFactory msgenFact = new MSGeneratorFactory();
                Task msgen = msgenFact.CreateMSGenerator(run.MSFileType);

                Task msPeakdetector = new DeconToolsPeakDetector(1.3, 2, Globals.PeakFitType.QUADRATIC, true);

                Task finder = new O16O18FeatureFinderTask(10);

                run.ResultCollection.MassTagResultType = Globals.MassTagResultType.O16O18_MASSTAG_RESULT;

                MassTagResultBase massTagResult;

                foreach (var mt in mtColl.MassTagList)
                {
                    run.CurrentMassTag = mt;

                    try
                    {
                        theorFeatureGenerator.Execute(run.ResultCollection);
                        peakChromGen.Execute(run.ResultCollection);

                        massTagResult = run.ResultCollection.GetMassTagResult(mt);
                        if (massTagResult.Flags.Count > 0)
                        {
                            continue;

                        }


                        smoother.Execute(run.ResultCollection);
                        peakDet.Execute(run.ResultCollection);
                        chromPeakSel.Execute(run.ResultCollection);
                        msgen.Execute(run.ResultCollection);
                        msPeakdetector.Execute(run.ResultCollection);

                        finder.Execute(run.ResultCollection);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message + "trace = " + ex.StackTrace);

                        //throw;
                    }


                }


                StringBuilder sb = new StringBuilder();
                foreach (var result in run.ResultCollection.MassTagResultList.Values.ToList())
                {
                    O16O18_TResult o16Result = (O16O18_TResult)result;

                    sb.Append(o16Result.MassTag.ID);
                    sb.Append("\t");
                    sb.Append(o16Result.MassTag.IsotopicProfile.ChargeState);
                    sb.Append("\t");

                    sb.Append(o16Result.MassTag.MZ.ToString("0.0000"));
                    sb.Append("\t");
                    sb.Append(o16Result.MassTag.NETVal.ToString("0.0000"));

                    if (o16Result.IsotopicProfile != null)
                    {
                        sb.Append("\t");
                        sb.Append(o16Result.IsotopicProfile.GetMZ().ToString("0.0000"));
                        sb.Append("\t");

                        if (o16Result.ScanSet != null)
                        {

                            sb.Append(o16Result.ScanSet.PrimaryScanNumber);

                        }
                        else
                        {
                            sb.Append(-1);
                        }
                        sb.Append("\t");
                        sb.Append(o16Result.GetNET().ToString("0.0000"));
                        sb.Append("\t");
                        sb.Append(o16Result.RatioO16O18.ToString("0.0000"));
                    }
                    else
                    {
                        sb.Append("\t");
                        sb.Append("NoProfileFound");
                    }
                    sb.Append("\n");

                }
                Console.Write(sb.ToString());
                run.ResultCollection.ClearAllResults();
                run.ResultCollection.MSPeakResultList.Clear();

                
            }


   


        }

        [Test]
        public void getMousePlasma_targetedTests1()
        {
            List<Run> runList = new List<Run>();
            runList.Add(new XCaliburRun(mousePlasmaFile1));
            //runList.Add(new XCaliburRun(mousePlasmaFile2));
            //runList.Add(new XCaliburRun(mousePlasmaFile3));
            //runList.Add(new XCaliburRun(mousePlasmaFile4));
            //runList.Add(new XCaliburRun(mousePlasmaFile5));

            //MassTagCollection mtColl = GetMouse_ProblemMassTags1();


            MassTagCollection mtColl = GetMouse_VIPERMassTags1();

            foreach (var run in runList)
            {

                Console.WriteLine("------------------------- working on " + run.DatasetName);
                string peakFileName = run.DataSetPath + "\\" + run.DatasetName + "_peaks.txt";

                PeakImporterFromText peakImporter = new PeakImporterFromText(peakFileName);
                peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);


                foreach (var numMSScansSummed in new int[] {1})
                {
                    Console.WriteLine("~~~~~~~~~~~~~~ NUM_MSSCANS_SUMMED = " + numMSScansSummed);


                    TomTheorFeatureGenerator theorFeatureGenerator = new TomTheorFeatureGenerator();
                    ChromAlignerUsingVIPERInfo chromAligner = new ChromAlignerUsingVIPERInfo();
                    chromAligner.Execute(run);

                    Task peakChromGen = new PeakChromatogramGenerator(5);

                    Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);
                    Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(0.5, 0.5);
                    Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(numMSScansSummed, 0.1, Globals.PeakSelectorMode.MOST_INTENSE);

                    MSGeneratorFactory msgenFact = new MSGeneratorFactory();
                    Task msgen = msgenFact.CreateMSGenerator(run.MSFileType);

                    Task msPeakdetector = new DeconToolsPeakDetector(1.3, 2, Globals.PeakFitType.QUADRATIC, true);

                    Task fitScoreCalc = new DeconTools.Backend.ProcessingTasks.FitScoreCalculators.MassTagFitScoreCalculator();

                    Task finder = new O16O18FeatureFinderTask(10);

                    LeftOfMonoPeakLooker resultFlagger = new DeconTools.Backend.ProcessingTasks.ResultValidators.LeftOfMonoPeakLooker();


                    run.ResultCollection.MassTagResultType = Globals.MassTagResultType.O16O18_MASSTAG_RESULT;

                    MassTagResultBase massTagResult;

                    foreach (var mt in mtColl.MassTagList)
                    {
                        run.CurrentMassTag = mt;

                        try
                        {
                            theorFeatureGenerator.Execute(run.ResultCollection);
                            peakChromGen.Execute(run.ResultCollection);

                            massTagResult = run.ResultCollection.GetMassTagResult(mt);
                            if (massTagResult.Flags.Count > 0)
                            {
                                continue;

                            }


                            smoother.Execute(run.ResultCollection);
                            peakDet.Execute(run.ResultCollection);
                            chromPeakSel.Execute(run.ResultCollection);
                            msgen.Execute(run.ResultCollection);
                            msPeakdetector.Execute(run.ResultCollection);

                            finder.Execute(run.ResultCollection);
                            fitScoreCalc.Execute(run.ResultCollection);

                            resultFlagger.CurrentResult = massTagResult;
                            resultFlagger.Execute(run.ResultCollection);

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + "trace = " + ex.StackTrace);

                            //throw;
                        }


                    }


                    StringBuilder sb = new StringBuilder();

                    List<int> alreadyProcessedMTs = new List<int>();

                    foreach (MassTag mt in mtColl.MassTagList)
                    {
                        if (alreadyProcessedMTs.Contains(mt.ID)) continue;
                        List<MassTagResultBase>resultList= run.ResultCollection.MassTagResultList.Values.ToList();

                        List<MassTagResultBase> filteredResultsList = new List<MassTagResultBase>();
                        


                        for (int i = 0; i < resultList.Count; i++)
                        {
                            if (resultList[i].MassTag.ID == mt.ID)
                            {
                                filteredResultsList.Add(resultList[i]);
                            }
                        }

                        if (filteredResultsList.Count==0)throw new Exception();

                        List<MassTagResultBase> candidateResultsList = new List<MassTagResultBase>();


                        foreach (var item in filteredResultsList)
                        {
                            if (item.IsotopicProfile == null) continue;

                            if (item.Flags.Count > 0) continue;


                            double ratio = ((O16O18_TResult)item).RatioO16O18;
                            if (ratio == 0 || double.IsInfinity(ratio)) continue;

                            candidateResultsList.Add(item);


                        }

                        O16O18_TResult selectedResult;

                        if (candidateResultsList.Count == 0)
                        {
                            selectedResult =(O16O18_TResult)filteredResultsList[0];
                        }
                        else if (candidateResultsList.Count==1)
                        {
                            selectedResult = (O16O18_TResult)candidateResultsList[0];
                        }
                        else
                        {
                            selectedResult = (O16O18_TResult)getMostIntenseResult(candidateResultsList);
                        }


                        sb.Append(getReportOnO16O18Result(selectedResult));
                        alreadyProcessedMTs.Add(mt.ID);
                        
                    }


                    //foreach (var result in run.ResultCollection.MassTagResultList.Values.ToList())
                    //{
                    //    O16O18_TResult o16Result = (O16O18_TResult)result;

                    //    sb.Append(getReportOnO16O18Result(o16Result));
                    //}
                    Console.Write(sb.ToString());
                    run.ResultCollection.MassTagResultList.Clear();
                    run.XYData = null;

                    
    

                }

                run.ResultCollection.MSPeakResultList.Clear();

                Console.WriteLine("-------------------- end of " + run.DatasetName);
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();

                

            }





        }

        private string getReportOnO16O18Result(O16O18_TResult o16Result)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(o16Result.MassTag.ID);
            sb.Append("\t");
            sb.Append(o16Result.MassTag.IsotopicProfile.ChargeState);
            sb.Append("\t");

            sb.Append(o16Result.MassTag.MZ.ToString("0.0000"));
            sb.Append("\t");
            sb.Append(o16Result.MassTag.NETVal.ToString("0.0000"));

            if (o16Result.IsotopicProfile != null)
            {
                sb.Append("\t");
                sb.Append(o16Result.IsotopicProfile.GetMZ().ToString("0.0000"));
                sb.Append("\t");

                if (o16Result.ScanSet != null)
                {

                    sb.Append(o16Result.ScanSet.PrimaryScanNumber);

                }
                else
                {
                    sb.Append(-1);
                }
                sb.Append("\t");
                sb.Append(o16Result.Score.ToString("0.000"));


                sb.Append("\t");
                sb.Append(o16Result.GetNET().ToString("0.0000"));
                sb.Append("\t");
                sb.Append(o16Result.RatioO16O18.ToString("0.0000"));

                if (o16Result.Flags.Count > 0)
                {
                    sb.Append("\t");
                    sb.Append("flag");
                }


            }
            else
            {
                sb.Append("\t");
                sb.Append("NoProfileFound");
            }

            sb.Append("\n");

            return sb.ToString();
        }

        private MassTagResultBase getMostIntenseResult(List<MassTagResultBase> candidateResultsList)
        {

            MassTagResultBase mostIntenseResult = null;
            double maxIntensity = -1;

            for (int i = 0; i < candidateResultsList.Count; i++)
            {
                double currentIntensity = candidateResultsList[i].IsotopicProfile.GetAbundance();
                
                if (currentIntensity > maxIntensity)
                {
                    mostIntenseResult = candidateResultsList[i];
                    maxIntensity = currentIntensity;
                }
                
            }

            return mostIntenseResult;
        }

      
    
        public List<Task> getStandardTaskList(Run run)
        {
            Task peakChromGen = new PeakChromatogramGenerator(5);
            Task smoother = new DeconTools.Backend.ProcessingTasks.Smoothers.DeconToolsSavitzkyGolaySmoother(11, 11, 2);
            Task peakDet = new DeconTools.Backend.ProcessingTasks.PeakDetectors.ChromPeakDetector(0.5, 0.5);
            Task chromPeakSel = new DeconTools.Backend.ProcessingTasks.ChromPeakSelector(1,0.1, Globals.PeakSelectorMode.CLOSEST_TO_TARGET);
            MSGeneratorFactory msgenFactory = new MSGeneratorFactory();
            Task msgen = msgenFactory.CreateMSGenerator(run.MSFileType);
            DeconToolsV2.Peaks.clsPeakProcessorParameters peakParams = new DeconToolsV2.Peaks.clsPeakProcessorParameters(2, 3, false, DeconToolsV2.Peaks.PEAK_FIT_TYPE.QUADRATIC);
            Task mspeakDet = new DeconToolsPeakDetector(peakParams);
            Task theorFeatureGen = new TomTheorFeatureGenerator();
            Task finder = new O16O18FeatureFinderTask(5);

            List<Task> taskList = new List<Task>();

            taskList.Add(peakChromGen);
            taskList.Add(smoother);
            taskList.Add(peakDet);
            taskList.Add(chromPeakSel);
            taskList.Add(msgen);
            taskList.Add(mspeakDet);
            taskList.Add(theorFeatureGen);
            taskList.Add(finder);

            return taskList;
        }


        public MassTagCollection GetBSAMassTags()
        {

            MassTagCollection massTagCollection = new MassTagCollection();

            //BSA masstags:
            //massTagCollection.MassTagIDList = new List<long> { 1022, 1039, 1260, 1760, 1104, 1095, 1409, 1234, 1764, 1192, 1142, 1774, 1763, 1913, 11000, 1237, 15691, 1161, 1205, 11027, 10252, 1768, 1781, 3083, 11413, 1767, 11084, 11775, 11396, 1266, 5533, 11654, 2035, 11666, 2488, 1930, 1761, 1903, 15198, 1769, 19644, 4197724, 5536, 1807, 1778, 11660, 1759, 11772, 1770, 19635, 4200130, 4198200, 2021, 16135, 1556, 19634, 15203, 2529, 11976, 1939, 2100, 15087, 6850, 3226, 28302194, 20059, 1772, 1876, 4960, 16391, 12436, 30062411, 1460, 12091, 15090, 1326, 2244, 15208, 19493, 19721, 11259, 1272, 23069828, 15089, 24325, 15675, 28301515, 2627, 3873, 15210, 5769, 2140, 12116, 5505, 2127, 15085, 2588, 6103, 15291, 4570, 2701, 11397, 1857, 1782, 23143, 21476, 21482, 23065, 2910719, 7826, 2770, 2107 };
            //massTagCollection.MassTagIDList = new List<long> { 1022, 1039, 1260};


            massTagCollection.MassTagIDList = new List<long> { 1022, 1039, 1260, 1760, 1104, 1095, 1409, 1234, 1764, 1192, 1142, 1774, 1763, 1913, 11000, 1237, 15691, 1161, 1205, 11027, 10252, 1768, 1781, 3083, 11413, 1767, 11084, 11775, 11396, 1266, 5533, 11654, 2035, 11666, 2488, 1930, 1761, 1903, 15198, 1769, 19644, 4197724, 5536, 1807, 1778, 11660, 1759, 11772, 1770, 19635, 4200130, 4198200, 2021, 16135, 1556, 19634, 15203, 2529, 11976, 1939, 2100, 15087, 6850, 3226, 28302194, 20059, 1772, 1876, 4960, 16391, 12436, 30062411, 1460, 12091, 15090, 1326, 2244, 15208, 19493, 19721, 11259, 1272, 23069828, 15089, 24325, 15675, 2627, 28301515, 3873, 15210, 5769, 2140, 12116, 5505, 2127, 15085, 2588, 6103, 15291, 4570, 2701, 1857, 11397, 23143, 21476, 1782, 2107, 2770, 7826, 21482, 23065, 2910719, 24514, 15278, 2321, 1780, 13832, 21525, 2962956, 13788, 5099, 3390, 3541, 3025, 1771, 2273, 11867, 4197723, 4199232, 29259270, 4197826, 4198297, 12655, 21464, 1798, 1789, 3059, 4538, 22024, 19807, 21459, 11981, 15213, 15049, 4199123, 4197929, 3153522, 29036930, 28300543, 2910716, 4197804, 4198786, 4200139, 21550, 21484, 21498, 21514, 24308, 24313, 24553, 5559, 5671, 1503, 2155, 2168, 2179, 5296, 4168, 2243, 21651, 17305, 2910715, 2910718, 3092419, 2910720, 7996280, 31320826, 50684265, 29258411, 23069862, 6674845, 26136019, 2910721, 24302, 2910714, 4197761, 4199194, 21444, 15775, 5455, 2187, 2247, 2029, 2036, 1936, 2251, 2536, 1524, 1785, 5812, 5818, 9087, 3070, 3272, 3330, 4169, 4162, 4267, 4750, 4773, 15699, 15708, 16631, 14524, 14687, 12591, 13684, 10183, 11098, 21254, 17599, 21703, 22933, 22263, 24499, 24501, 24301, 4198124, 4199433, 4198790, 4199749, 4197725, 4197726, 4197735, 4197823, 4197840, 2910723, 2910729, 2911332, 17174072, 7970177, 30062777, 28762166, 29036768, 29461213, 31319092, 23019438, 4198278, 26136016, 28301678, 28301635, 28302697, 31417994, 31782095, 43171661, 29259049, 31395190, 31779670, 31396834, 34703940, 34780713, 38624971, 41202366, 31318785, 31318829, 31318839, 42240392, 42630736, 42630764, 43170373, 43170484, 43170728, 43171359, 43171548, 43171602, 38625442, 38626140, 38626376, 38626402, 38702338, 38702389, 38702394, 38703165, 38703212, 38703260, 39896371, 39896396, 39896967, 39897010, 39897051, 39897056, 39897256, 39897559, 39897847, 39897905, 41201605, 41201896, 41201920, 41202048, 41202093, 41202249, 41202278, 41202293, 41202336, 41202363, 34780739, 34781534, 34781634, 34781750, 34782300, 38606666, 38623967, 38624039, 38624143, 38624178, 38624221, 38624351, 38624357, 38624533, 38624867, 34704246, 34704425, 34704587, 34704597, 34780320, 34780549, 31397113, 31417865, 31417950, 31779690, 31779758, 31780640, 31780853, 31781026, 31781173, 31781740, 31781752, 31781851, 31781905, 31781917, 31781936, 31781958, 31782025, 31395223, 31395737, 31396467, 31396510, 31396524, 31396672, 29259057, 29259137, 30062414, 30062730, 31320863, 31320870, 31321022, 31321111, 31321143, 31393988, 31394105, 31394182, 31394187, 31394216, 31394690, 31395186, 43171713, 43172046, 43172118, 43172367, 43172499, 43240864, 43831422, 43832084, 44388452, 44388887, 44474576, 44474668, 44474841, 44474851, 44474854, 44475520, 46384860, 46671995, 46672307, 46672380, 46672831, 46672846, 46672923, 46673217, 46673576, 46673815, 46673838, 46674125, 46674258, 46674400, 47195033, 47195038, 47195066, 47195172, 47195197, 47195268, 47683491, 47692678, 47692832, 47692982, 48568690, 48568756, 48568777, 50111064, 50330539, 50895734, 50895779, 51329041, 51329060, 51428548, 51438650, 51438918, 51582970, 51582998, 52132487, 52390686, 52391145, 52391147, 55774336, 59679827, 59679977, 59680115, 60502585, 60502593, 60781843, 60781977, 60782147, 60782160, 60782529, 61338807, 61338910, 61339401, 61796248, 61796631, 61796776, 61797211, 62407647, 62407793, 62489109, 62489344, 62489899, 63636568, 64996403, 65225489, 65226125, 66584294, 68518746, 68518850, 69996881, 69998658, 72893008, 74369939, 74370383, 74601895, 76105893, 78457620, 31782109, 31782210, 31782252, 32027737, 32027802, 32027907, 32027990, 32028052, 32028204, 32028339, 32028425, 32028468, 32028751, 32029130, 32029230, 32743959, 32744025, 33341066, 33341203, 33341398, 33341454, 33341532, 33341693, 33341903, 33342492, 33342660, 33342712, 33342728, 33342786, 33342876, 33342903, 33343268, 33343480, 33343585, 33343673, 34429609, 34431330, 34703383, 34703482, 34703527, 31418013, 31418255, 31418256, 31418761, 31418762, 31418946, 31418962, 31418977, 31419386, 31419482, 31419531, 31419593, 31419697, 31419843, 31419879, 31420036, 31773811, 31774271, 31774298, 31774487, 31774780, 31774853, 31774979, 31775178, 31776043, 31776205, 31776270, 31776773, 31776841, 31776842, 31777286, 31777327, 31777741, 31777811, 31778171, 31778203, 31778463, 31778534, 31778557, 31778720, 31778884, 31779003, 31779239, 31779346, 31779400, 31779495, 31779521, 28302711, 28302887, 28302990, 28675623, 28675638, 28675690, 28675823, 28675830, 28675932, 28676040, 28301656, 28301516, 28302214, 28302304, 28302433, 28302490, 28302564, 28301719, 28301748, 28301754, 28301856, 28301955, 28301980, 28301991, 23019652, 28300920, 28300977, 28301323, 28301376, 28301378, 31319232, 31319482, 31320092, 31320276, 31320337, 31320402, 31320491, 31320594, 31320723, 31320803, 31320819, 29461463, 29461652, 29461667, 29568319, 29568472, 29568519, 30062365, 29036876, 29036733, 29257855, 29258156, 29258190, 29258354, 29258423, 29258468, 29258586, 29258737, 29258800, 29258967, 29259308, 29259323, 29259569, 29460777, 29460780, 29461154, 30062911, 30063088, 30063303, 30063419, 30063500, 30063663, 30063994, 30064291, 30064390, 30064414, 30064518, 30064601, 30064692, 30064712, 30065137, 30065204, 30065365, 30065713, 30065727, 30065827, 30065840, 30065875, 30065977, 30066234, 30066274, 30066457, 30066505, 30066808, 30067055, 30067762, 30068031, 30068053, 30068140, 30068260, 30068390, 30068533, 30068590, 30068709, 30572076, 30572156, 30572198, 30572357, 30572432, 30572686, 30572703, 30572843, 30572885, 30572942, 30573072, 30573185, 30573392, 30573646, 30573890, 30573969, 30574006, 30574069, 30574085, 30574238, 30574294, 30574308, 30574356, 30574499, 30574919, 31317624, 31317749, 31317820, 31318223, 31318466, 31318494, 31318642, 31318668, 7970469, 7970486, 7970729, 7970781, 7970800, 7970812, 7970819, 7996224, 7996234, 7996240, 7996275, 4199237, 4199250, 4199256, 4200132, 4200135, 4200137, 7996296, 7996301, 7996309, 7996549, 7996551, 7996578, 7996669, 7996759, 7996937, 7996954, 7997030, 7997056, 7997075, 7997471, 7997721, 7997738, 10929505, 10929678, 10929811, 10929849, 12764903, 12764924, 17099759, 17099850, 17099940, 17099960, 17100229, 17131401, 17174076, 17174088, 17174273, 17174310, 17220635, 17220727, 21853458, 7958965, 7969718, 7969726, 23070173, 24971022, 25235680, 25235692, 26037903, 26155946, 26156329, 26156479, 26156483, 26156501, 26156673, 26157255, 26157642, 26157854, 26158342, 26158355, 28300285, 28300288, 28300306, 28300314, 28300344, 28300351, 2911333, 2911475, 2911553, 4167963, 4167975, 4168134, 4168139, 4168165, 4168268, 4168481, 4191225, 4197720, 2910731, 2910773, 2910783, 2910821, 2910833, 2910881, 2910984, 2910998, 2911083, 2911212, 2911260, 2911281, 2910727, 2910728, 24529, 24534, 4197847, 4197893, 4197897, 4197910, 4197926, 4197927, 4197825, 4197935, 4197942, 4197809, 4197737, 4197739, 4197740, 4197748, 4197755, 4197757, 4197759, 4197729, 4197763, 4197765, 4197766, 4197775, 4197803, 4199751, 4199766, 4199784, 4199786, 4199799, 4199805, 4199806, 4199807, 4199810, 4199814, 4199836, 4199857, 4199859, 4199872, 4199887, 4199896, 4199953, 4200028, 4200053, 4200057, 4200106, 4200146, 4200156, 4200173, 4200178, 4200182, 4200198, 4200211, 4200227, 4200233, 4200234, 4200444, 4200620, 4200720, 4200855, 4201225, 4201447, 4201477, 4201549, 4201557, 4201626, 4201674, 4201986, 4202180, 4202284, 4202332, 4204796, 4204889, 4204999, 4198892, 4198948, 4199048, 4199050, 4199052, 4199114, 4199117, 4198334, 4198353, 4198372, 4198426, 4198536, 4198541, 4198738, 4198757, 4199450, 4199456, 4199465, 4199474, 4199649, 4199652, 4199654, 4199746, 4198166, 4198171, 4198221, 4198224, 4199208, 4199128, 4199182, 23097, 23316, 24326, 24377, 24558, 24607, 24681, 2910457, 22418, 22828, 22834, 22952, 23047, 22100, 21595, 17627, 17681, 17780, 17907, 18247, 18312, 18340, 18418, 18465, 18668, 18691, 19179, 19197, 19239, 19378, 19479, 19991, 20031, 20041, 17352, 17376, 17486, 11341, 11886, 11709, 11743, 13702, 13789, 14213, 14375, 12205, 13345, 13349, 13559, 13617, 13660, 13676, 14889, 14953, 14571, 14677, 15238, 15293, 15452, 15460, 16797, 17136, 17148, 17167, 16526, 16205, 16255, 15766, 15973, 4794, 4838, 4944, 4614, 5020, 4336, 4490, 4257, 4025, 4064, 3723, 3775, 3531, 2896, 9603, 9855, 9867, 10081, 10406, 10458, 10719, 10837, 7042, 8229, 8730, 8753, 5828, 6078, 6086, 6095, 6347, 6491, 6690, 6744, 5465, 5254, 5295, 5572, 5647, 5553, 1775, 2331, 2314, 2122, 1935, 2037, 1799, 1910 };
            
            //massTagCollection.MassTagIDList = new List<long> { 1192, 1260};
            //massTagCollection.MassTagIDList = new List<long> { 1104, 1260};
           

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_BSA_P171", "Albert");
            importer.Import(massTagCollection);

            return massTagCollection;


        }

        private MassTagCollection GetMouseMassTags()
        {
            MassTagCollection massTagCollection = new MassTagCollection();

            massTagCollection.MassTagIDList = new List<long> { 22807265, 22580887, 20791942, 20791939, 20750857, 20908613, 20842966, 22598396, 174124103 };

         
            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Mouse_P303", "Pogo");
            importer.Import(massTagCollection);

            massTagCollection.MassTagList = massTagCollection.MassTagList.Distinct().ToList();

            massTagCollection.FilterOutDuplicates();

            return massTagCollection;
        }


        private MassTagCollection GetMouse_ProblemMassTags1()
        {
            MassTagCollection massTagCollection = new MassTagCollection();

            massTagCollection.MassTagIDList = new List<long> { 8573670, 10640074, 11444439, 11646915, 20746154, 20746242, 20746242, 20746287, 20746654, 20767698, 20778355, 20842974, 20852057, 20868746, 21467365, 21593912, 22079682, 26377720, 26571862, 26887929, 27093468, 44136730, 46461431, 47389768, 83453028, 104990879, 107700666, 116592100, 119393220, 119597922, 148450916, 148501772, 151913652, 158761276, 174116064, 174116677, 174117511, 174117829, 174118090, 174124070, 174137631, 174137631, 174140003, 174149645, 174151028, 174157492, 174169336, 174174799, 174175793, 174176261, 174178689, 174204927, 174235584, 174260996, 174261580, 174285048, 174285152, 174285433, 174291236, 174295634, 174307120, 174307429, 174335192, 174335214, 174335217, 174335239, 174343685, 174359991, 174587975, 174589262, 174609580, 174633217, 174645929, 174660971, 174661567, 174671629, 174672565, 174944463, 174956373, 174982894, 174982894, 174989911, 175006197, 175009765, 187444874, 188076176, 188401301, 188509673, 188522717, 188540998, 188547022, 188547074, 188562068, 188566943, 188597552, 188597762, 188602664, 188621120, 188621174, 188689356 };


            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Mouse_P303", "Pogo");
            importer.Import(massTagCollection);

            massTagCollection.MassTagList = massTagCollection.MassTagList.Distinct().ToList();

            massTagCollection.FilterOutDuplicates();

            return massTagCollection;
        }


        private MassTagCollection GetMouse_VIPERMassTags1()
        {
            MassTagCollection massTagCollection = new MassTagCollection();
            massTagCollection.MassTagIDList = new List<long> { 1115, 1210, 1687, 13878, 4883887, 6634809, 6635297, 6637497, 6637987, 6639562, 6643889, 6643962, 6644331, 6654332, 6654352, 6662822, 6666532, 6672088, 6672088, 6684837, 6688878, 6688906, 6694578, 6695779, 6700555, 6702292, 6702293, 6702716, 6702977, 6704405, 6706628, 6706842, 6709133, 6711441, 6719681, 6728010, 6748846, 6750879, 6750931, 6750932, 6751043, 6775194, 6853895, 6855080, 6880220, 6892974, 6892974, 6948890, 6951213, 6951213, 6952818, 6959129, 6959132, 6959138, 6959173, 6959230, 6959238, 6962401, 7034341, 7038425, 7167730, 7316050, 7328781, 7328787, 7328791, 7329368, 7335097, 7337724, 7340469, 7340641, 7438750, 7735381, 7889325, 7917252, 8094248, 8103618, 8557599, 8573670, 8590513, 8665045, 8776822, 8784413, 8965817, 8990784, 8995744, 8995744, 8996991, 9018422, 9309259, 9561169, 9561171, 9622229, 9622231, 9701077, 10111328, 10149009, 10176664, 10182286, 10184834, 10184972, 10197234, 10354722, 10385125, 10385126, 10463756, 10616562, 10627276, 10640074, 10737700, 10743686, 11086105, 11154767, 11444439, 11617827, 11646915, 12319179, 12632587, 12658543, 12961058, 13079052, 14198036, 14388226, 14423670, 14423670, 14849817, 15355140, 15705676, 20703886, 20707540, 20707543, 20708463, 20711589, 20711976, 20713296, 20744484, 20744496, 20744510, 20744526, 20744611, 20744758, 20744898, 20745222, 20745266, 20745302, 20745314, 20745358, 20745961, 20746072, 20746142, 20746144, 20746150, 20746151, 20746152, 20746154, 20746172, 20746173, 20746183, 20746184, 20746190, 20746216, 20746242, 20746242, 20746287, 20746289, 20746467, 20746493, 20746637, 20746654, 20746907, 20746921, 20747417, 20748135, 20748196, 20748196, 20748373, 20748546, 20749178, 20749207, 20749683, 20750431, 20750619, 20750707, 20750721, 20750733, 20750746, 20750759, 20750762, 20750835, 20750838, 20750848, 20750857, 20750860, 20750867, 20750867, 20750871, 20750874, 20750875, 20750877, 20750878, 20750881, 20750883, 20750886, 20750887, 20750924, 20750948, 20750960, 20750962, 20750967, 20750980, 20750991, 20751002, 20751017, 20751030, 20751052, 20751098, 20751152, 20751228, 20751260, 20751328, 20751368, 20751427, 20751460, 20751675, 20751961, 20752078, 20752234, 20752252, 20752732, 20753059, 20753084, 20753301, 20753628, 20753950, 20753952, 20753957, 20754018, 20754026, 20754112, 20754135, 20754229, 20754702, 20757235, 20758743, 20758777, 20758798, 20758829, 20758841, 20758893, 20758894, 20758898, 20758941, 20759185, 20759232, 20759330, 20759723, 20759930, 20760368, 20761534, 20761655, 20764691, 20766841, 20767488, 20767550, 20767698, 20768031, 20768435, 20769574, 20769714, 20769720, 20769819, 20769984, 20770230, 20771065, 20773542, 20775573, 20775579, 20776242, 20777007, 20778355, 20778983, 20780705, 20782768, 20782921, 20783385, 20784026, 20786888, 20786888, 20786889, 20786889, 20786890, 20786890, 20788548, 20788553, 20790729, 20791763, 20791942, 20791971, 20795300, 20795926, 20795937, 20795949, 20795981, 20796773, 20797597, 20797718, 20798359, 20798488, 20800105, 20801212, 20804123, 20822789, 20825160, 20826585, 20826686, 20827087, 20827460, 20830762, 20835099, 20837056, 20838091, 20838153, 20842961, 20842965, 20842968, 20842974, 20843005, 20843007, 20843016, 20843019, 20843080, 20843238, 20843264, 20843684, 20843684, 20843957, 20844073, 20844078, 20844369, 20844798, 20846167, 20846477, 20846482, 20846531, 20846540, 20846789, 20846795, 20846800, 20846846, 20847067, 20847197, 20847306, 20850156, 20851485, 20851513, 20851513, 20851514, 20851514, 20851515, 20851515, 20851546, 20851547, 20851566, 20851654, 20851844, 20852057, 20853245, 20853248, 20853411, 20853826, 20855724, 20855829, 20855963, 20858425, 20860941, 20860948, 20860952, 20863431, 20863802, 20864003, 20864191, 20864563, 20864611, 20865400, 20868746, 20868749, 20868749, 20868751, 20868751, 20868761, 20868762, 20868768, 20868768, 20868809, 20868875, 20868876, 20868882, 20868912, 20869010, 20869104, 20869292, 20869437, 20870121, 20870148, 20870474, 20870905, 20871084, 20871135, 20871278, 20872585, 20873071, 20873250, 20873255, 20873256, 20873371, 20874252, 20874671, 20876387, 20876393, 20876755, 20876757, 20877637, 20878714, 20887382, 20889285, 20891631, 20893113, 20902238, 20902238, 20907792, 20908613, 20930317, 20961947, 20984714, 21027762, 21052896, 21053320, 21065822, 21097659, 21113773, 21266264, 21278767, 21320895, 21447815, 21467365, 21468174, 21501904, 21507800, 21508682, 21518073, 21593912, 21593927, 21593927, 21593932, 21593936, 21593938, 21593942, 21593961, 21593976, 21594066, 21594132, 21594227, 21594227, 21594246, 21594304, 21594818, 21595732, 21596237, 21596585, 21596821, 21596821, 21596981, 21597049, 21597685, 21598232, 21598439, 21599103, 21599154, 21600087, 21600088, 21600090, 21600126, 21600130, 21600165, 21600377, 21603042, 21609150, 21614439, 21616238, 21616239, 21616738, 21756606, 21756766, 21756807, 21759809, 21759859, 21768023, 21779128, 21785866, 21786132, 21791771, 21792001, 21792015, 21799979, 21800573, 21802987, 21944557, 21965693, 22002087, 22017407, 22038413, 22065682, 22079682, 22111620, 22350500, 22578929, 22580887, 22604538, 22604538, 22606733, 22609681, 22609682, 22609689, 22609699, 22609711, 22609713, 22609749, 22610272, 22611753, 22612813, 22613846, 22613897, 22613905, 22613908, 22614088, 22614096, 22614129, 22614172, 22614172, 22614247, 22614560, 22617167, 22618365, 22619464, 22620034, 22622905, 22622985, 22622989, 22623021, 22626540, 22626972, 22627315, 22627315, 22631273, 22631281, 22631440, 22631440, 22631487, 22631496, 22631599, 22634815, 22635892, 22637616, 22638861, 22639017, 22641064, 22647165, 22781474, 22781597, 22788051, 22796070, 22798839, 22807265, 22810428, 22813072, 22826593, 22827215, 22837419, 22842275, 22848968, 22870387, 22881717, 22885121, 22888239, 22891465, 22891478, 22928542, 22928542, 23071928, 25291138, 25407502, 26375560, 26376663, 26376663, 26377720, 26378236, 26383300, 26384724, 26418578, 26435152, 26440177, 26440276, 26495506, 26571862, 26594398, 26594398, 26617872, 26629611, 26650338, 26650633, 26660479, 26660942, 26678501, 26679497, 26681401, 26691466, 26697684, 26761954, 26786581, 26791032, 26811107, 26867870, 26869679, 26879329, 26879572, 26879587, 26879913, 26887929, 26887929, 26965132, 26972286, 27026361, 27041051, 27093468, 27133379, 27633498, 27701722, 27876769, 27888410, 28646987, 28819764, 28936239, 29080485, 29274597, 29311976, 29329307, 29337293, 29337432, 29341845, 29342184, 29354968, 29357786, 29357989, 29358802, 29358852, 29362244, 29362500, 29366448, 29366639, 29376359, 29383491, 29384623, 29386298, 29401698, 29409356, 29421283, 29427859, 29432802, 29521826, 29528532, 30930588, 31930078, 31942500, 32016557, 32317151, 33336515, 33336547, 33432303, 33694302, 33722640, 33759955, 34409935, 34967907, 35359243, 36785449, 37186273, 44063080, 44136730, 44187845, 44274386, 44281706, 44533071, 44533072, 44533076, 46422210, 46461431, 47387109, 47389640, 47389768, 47444151, 47578767, 47611075, 47683076, 48284305, 48284306, 48348683, 48364162, 48376660, 48387863, 48928719, 52151238, 52151238, 55684416, 60377484, 60539099, 63623641, 63626229, 63626229, 63632355, 63688457, 63708505, 63725418, 63729338, 63731462, 63737730, 63743864, 63868095, 65195945, 66376316, 67359884, 67440797, 69165282, 70235879, 70260860, 70837309, 70862387, 70886291, 70966064, 71311956, 71389877, 71445396, 71828998, 73212494, 73241427, 73765879, 74801477, 78159219, 78271740, 83165951, 83174550, 83176096, 83198887, 83198927, 83204577, 83214995, 83219270, 83221918, 83222001, 83222492, 83224838, 83242561, 83242784, 83272634, 83272697, 83272743, 83272833, 83274536, 83275360, 83276283, 83276284, 83276739, 83276739, 83280472, 83288425, 83289607, 83424455, 83425121, 83425241, 83428224, 83433085, 83447825, 83450991, 83451013, 83451053, 83451085, 83451128, 83451302, 83451308, 83453028, 83453844, 83457068, 83457115, 83458339, 83459526, 83460245, 83462151, 83478979, 83478993, 83479115, 83479122, 83479230, 83479985, 83480234, 83480288, 83483332, 83484283, 83485008, 83512138, 83514300, 83514449, 83514552, 83515667, 83517019, 83520381, 83520553, 83537405, 83555485, 83572855, 85276891, 85276891, 85890845, 85890845, 85891301, 85892893, 85909190, 86010806, 86640326, 87777541, 90977764, 91580817, 100778860, 101008873, 101023360, 101743364, 101844138, 101877866, 102197896, 102353345, 102384033, 102384144, 102818430, 104032474, 104033695, 104069703, 104071827, 104083118, 104109869, 104113249, 104116556, 104141042, 104166943, 104189878, 104791600, 104822668, 104822905, 104828624, 104856453, 104907588, 104928639, 104937118, 104954748, 104983473, 104987000, 104990879, 106671523, 106682878, 106695904, 106714721, 106715671, 106722091, 106722349, 106723699, 106725620, 106811226, 106811286, 106842370, 107062678, 107069524, 107354072, 107700666, 107701651, 108062101, 108416640, 108519578, 108519578, 109047767, 109171700, 109945776, 110011102, 110068747, 114639794, 116385248, 116564325, 116592100, 117822861, 118270207, 118309976, 118887788, 119393220, 119500744, 119545900, 119597922, 119598642, 123679993, 130001377, 130017486, 130017486, 130047802, 130068059, 130075523, 132073823, 136630000, 137809324, 139012878, 145936033, 147249030, 147665381, 147882909, 147956437, 148043589, 148114189, 148268434, 148391058, 148403659, 148434125, 148434125, 148450916, 148501772, 148513788, 148947192, 149175269, 149198751, 149218820, 149295743, 149306148, 149398265, 149523110, 150211040, 150677121, 151096950, 151395279, 151812616, 151913652, 151935415, 152166287, 152933149, 153655403, 153746694, 154332192, 154350638, 156597747, 158761276, 159962064, 162717370, 162721739, 163866648, 163933160, 163935994, 164950101, 165456717, 165465931, 165500117, 165776447, 165802090, 165846582, 165846582, 165849904, 165850099, 165850099, 165872618, 165899057, 167778558, 167917287, 168092614, 168326978, 168326978, 170464570, 170616425, 170766131, 171520263, 171540032, 171687517, 172303060, 172318060, 172383146, 172391133, 172997760, 172999561, 173012300, 173020140, 173039059, 173052443, 173082679, 173088997, 173207429, 173938399, 173938624, 173939170, 173942011, 173942701, 174058607, 174063814, 174069144, 174072413, 174073805, 174074413, 174074589, 174075614, 174078187, 174079785, 174084196, 174084437, 174116060, 174116061, 174116064, 174116074, 174116075, 174116106, 174116118, 174116129, 174116156, 174116581, 174116677, 174117511, 174117829, 174118090, 174118193, 174118193, 174118813, 174119586, 174119732, 174119790, 174119886, 174120104, 174120146, 174120160, 174120977, 174121177, 174121432, 174121522, 174121707, 174122587, 174124070, 174124103, 174124383, 174124411, 174124511, 174124723, 174124767, 174124973, 174125749, 174126198, 174127791, 174127885, 174130687, 174132097, 174132541, 174133451, 174137397, 174137631, 174137631, 174137764, 174138017, 174138036, 174138065, 174138686, 174138723, 174139110, 174139157, 174139321, 174139332, 174139378, 174139398, 174139557, 174140003, 174140431, 174140988, 174141041, 174141797, 174142031, 174142050, 174142198, 174142997, 174144038, 174144157, 174144557, 174145241, 174146307, 174146307, 174147369, 174147458, 174149596, 174149645, 174149678, 174149727, 174150025, 174151028, 174152771, 174153869, 174155038, 174156052, 174156269, 174157168, 174157215, 174157492, 174158834, 174158908, 174159440, 174160308, 174161169, 174161836, 174162556, 174162637, 174163740, 174166324, 174166345, 174166427, 174166431, 174167431, 174169336, 174169336, 174169584, 174172935, 174173628, 174174300, 174174799, 174175459, 174175793, 174176261, 174176618, 174176744, 174178389, 174178408, 174178689, 174178689, 174180876, 174180876, 174183470, 174187805, 174188123, 174188175, 174189116, 174189406, 174190865, 174191161, 174191434, 174191434, 174191939, 174192443, 174192611, 174195392, 174199863, 174199863, 174200166, 174200166, 174201339, 174203975, 174204904, 174204927, 174205163, 174205623, 174205808, 174206042, 174206042, 174206148, 174206313, 174206452, 174206549, 174206863, 174206958, 174206987, 174207135, 174207249, 174208370, 174208460, 174208695, 174209625, 174210799, 174220485, 174222902, 174223245, 174225341, 174225939, 174227762, 174227900, 174228284, 174228309, 174229339, 174230653, 174231126, 174233448, 174235435, 174235446, 174235555, 174235584, 174235685, 174235852, 174236218, 174236463, 174240736, 174242140, 174242437, 174242512, 174242804, 174242912, 174242913, 174243317, 174243414, 174243507, 174243794, 174243955, 174243999, 174245096, 174245204, 174245793, 174245894, 174245915, 174247073, 174250805, 174252113, 174252121, 174252126, 174252360, 174252701, 174254332, 174255462, 174256286, 174256286, 174256673, 174257407, 174257930, 174257942, 174258169, 174260003, 174260332, 174260422, 174260518, 174260551, 174260996, 174261227, 174261580, 174261610, 174261960, 174262096, 174262099, 174262176, 174262461, 174262600, 174263676, 174263677, 174263787, 174264627, 174265221, 174266016, 174267720, 174268534, 174269336, 174269633, 174269646, 174269735, 174269904, 174270236, 174270397, 174270640, 174270656, 174270903, 174270903, 174271084, 174271765, 174271796, 174272460, 174272780, 174273211, 174273248, 174273292, 174273407, 174273856, 174274956, 174275395, 174275427, 174275489, 174275489, 174275722, 174276249, 174276447, 174276557, 174277025, 174277035, 174277251, 174277485, 174277817, 174278083, 174278099, 174278154, 174278189, 174278894, 174279802, 174280415, 174280841, 174283151, 174285048, 174285097, 174285098, 174285152, 174285199, 174285243, 174285292, 174285433, 174285940, 174287377, 174287836, 174291161, 174291236, 174291390, 174291508, 174291607, 174291623, 174292363, 174292435, 174292894, 174293045, 174293073, 174293628, 174294102, 174294166, 174294338, 174295112, 174295152, 174295177, 174295274, 174295274, 174295357, 174295446, 174295454, 174295634, 174297194, 174297405, 174298127, 174305349, 174307120, 174307429, 174307537, 174307859, 174307878, 174307981, 174308326, 174309098, 174309243, 174309407, 174309588, 174310129, 174310262, 174310340, 174312603, 174312897, 174312897, 174313205, 174325632, 174327066, 174327077, 174328806, 174328832, 174330866, 174335187, 174335192, 174335192, 174335212, 174335214, 174335214, 174335217, 174335217, 174335239, 174335239, 174335275, 174335275, 174337187, 174337595, 174339791, 174340529, 174340788, 174341184, 174342160, 174342224, 174342337, 174342582, 174342887, 174343623, 174343644, 174343685, 174343791, 174344369, 174346122, 174347423, 174351276, 174351323, 174351345, 174351462, 174351533, 174353517, 174355119, 174355999, 174357068, 174357155, 174357179, 174357544, 174357752, 174358436, 174358623, 174359580, 174359690, 174359690, 174359715, 174359965, 174359991, 174360345, 174360480, 174360544, 174360664, 174360761, 174361826, 174362256, 174362377, 174468869, 174469184, 174479985, 174483200, 174483647, 174493778, 174493987, 174494198, 174565264, 174583123, 174587683, 174587975, 174588299, 174588538, 174588910, 174589023, 174589262, 174596807, 174596893, 174597636, 174597813, 174598127, 174598691, 174599063, 174607071, 174607196, 174607544, 174608057, 174608189, 174608295, 174608711, 174608711, 174609580, 174617377, 174619247, 174620111, 174620189, 174620526, 174621258, 174626418, 174626419, 174630197, 174630455, 174631117, 174632195, 174632195, 174632257, 174632373, 174632617, 174633217, 174633217, 174635356, 174637921, 174642639, 174643404, 174644021, 174644305, 174644474, 174644917, 174645929, 174646422, 174646757, 174646989, 174647186, 174650176, 174653982, 174654175, 174654528, 174654886, 174655724, 174657010, 174658192, 174658401, 174658698, 174659193, 174659972, 174660403, 174660801, 174660971, 174661567, 174664585, 174671629, 174672565, 174673617, 174674101, 174674749, 174674862, 174675786, 174676224, 174676298, 174676337, 174676337, 174676443, 174678205, 174678532, 174944463, 174944628, 174951416, 174954413, 174955142, 174955408, 174955931, 174956197, 174956221, 174956373, 174956709, 174957298, 174959996, 174964657, 174965774, 174967095, 174967524, 174969487, 174972596, 174974832, 174975669, 174982862, 174982894, 174982894, 174983253, 174983301, 174983327, 174983367, 174983687, 174984048, 174985681, 174986048, 174987325, 174987572, 174988038, 174989188, 174989255, 174989911, 174991004, 174991665, 174992287, 174992380, 174992384, 174992658, 174993820, 174993820, 174996279, 174997622, 174999642, 175002245, 175002491, 175003557, 175006197, 175006622, 175007642, 175008845, 175008919, 175009765, 175039622, 175041719, 175041748, 175042388, 175043235, 175043929, 175044059, 175044288, 175045042, 175048176, 175048533, 175071844, 175071883, 175072982, 175073004, 175073212, 175079525, 175079526, 175179340, 176171963, 176172514, 176177226, 176178126, 176178769, 176178852, 176178852, 176179465, 176179514, 176179514, 176179904, 176180380, 176180496, 176180559, 176180797, 176181088, 176181211, 176181607, 176182969, 176184708, 176184708, 176185405, 176190534, 176195013, 176195871, 176197452, 176197530, 176197562, 176197585, 176198161, 176201528, 176210455, 176212837, 176212837, 176212842, 176214340, 176215197, 176216012, 176217704, 176224183, 176226221, 176226221, 176227428, 176229211, 176230295, 176230533, 176230533, 176231153, 176231852, 176232304, 176232423, 176380857, 179148585, 184009528, 186426972, 187444874, 188018695, 188020594, 188036082, 188061543, 188076176, 188116340, 188123696, 188123696, 188123728, 188130205, 188137121, 188160642, 188160642, 188174666, 188181847, 188189800, 188221470, 188222506, 188236902, 188240299, 188263778, 188286261, 188401301, 188401301, 188420108, 188464145, 188478993, 188480175, 188480186, 188482115, 188483661, 188483860, 188483878, 188487829, 188487829, 188487863, 188487863, 188487988, 188494380, 188494846, 188495197, 188495709, 188497347, 188497742, 188498185, 188498376, 188498470, 188503711, 188503725, 188503785, 188503918, 188503967, 188506601, 188508753, 188509275, 188509316, 188509673, 188512529, 188513234, 188517029, 188517127, 188522717, 188524647, 188526518, 188526932, 188529992, 188530016, 188533153, 188533190, 188534955, 188535938, 188536386, 188537508, 188537613, 188537723, 188538377, 188540998, 188542841, 188543261, 188543444, 188543571, 188547022, 188547022, 188547074, 188549952, 188550303, 188550513, 188554909, 188555151, 188555151, 188555624, 188555686, 188560854, 188562068, 188562443, 188562713, 188562746, 188562836, 188563098, 188563614, 188563630, 188566856, 188566943, 188570738, 188596014, 188597552, 188597762, 188598941, 188601839, 188602035, 188602052, 188602664, 188602743, 188603139, 188603192, 188604472, 188604472, 188606592, 188607073, 188613272, 188614801, 188619829, 188620657, 188620990, 188621120, 188621174, 188621507, 188644891, 188644891, 188645401, 188647703, 188648798, 188649572, 188649653, 188649677, 188649677, 188649793, 188652514, 188657135, 188658350, 188658369, 188658370, 188658425, 188659282, 188663343, 188664262, 188664407, 188673361, 188673361, 188673496, 188673645, 188673855, 188674419, 188675805, 188676237, 188676332, 188676502, 188676736, 188677094, 188680575, 188681885, 188682052, 188686836, 188687060, 188688106, 188689072, 188689170, 188689356, 188691314 };

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Mouse_P303", "Pogo");
            importer.Import(massTagCollection);

            massTagCollection.MassTagList = massTagCollection.MassTagList.Distinct().ToList();

            massTagCollection.FilterOutDuplicates();

            return massTagCollection;
        }

    }
}
