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

                Task finder = new O16O18TFFTask(10);

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
            //runList.Add(new XCaliburRun(mousePlasmaFile1));
            runList.Add(new XCaliburRun(mousePlasmaFile2));
            //runList.Add(new XCaliburRun(mousePlasmaFile3));
            //runList.Add(new XCaliburRun(mousePlasmaFile4));
            //runList.Add(new XCaliburRun(mousePlasmaFile5));

            //MassTagCollection mtColl = GetMouse_ProblemMassTags1();




            //MassTagCollection mtColl = GetMouse_VIPERMassTags2();

            MassTagCollection mtColl = GetMouse_ProblemMassTags2();


           // MassTagCollection mtColl = GetMouseMassTags();


            foreach (var run in runList)
            {

                Console.WriteLine("------------------------- working on " + run.DatasetName);
                string peakFileName = run.DataSetPath + "\\" + run.DatasetName + "_peaks.txt";

                PeakImporterFromText peakImporter = new PeakImporterFromText(peakFileName);
                peakImporter.ImportPeaks(run.ResultCollection.MSPeakResultList);


                foreach (var numMSScansSummed in new int[] {5})
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

                    Task finder = new O16O18TFFTask(10);

                    LeftOfMonoPeakLooker resultFlagger = new DeconTools.Backend.ProcessingTasks.ResultValidators.LeftOfMonoPeakLooker();


                    run.ResultCollection.MassTagResultType = Globals.MassTagResultType.O16O18_MASSTAG_RESULT;

                    MassTagResultBase massTagResult;

                    int counter = 1;

                    foreach (var mt in mtColl.MassTagList)
                    {
                        run.CurrentMassTag = mt;

                        if (counter % 50 == 0) Console.WriteLine("MT Counter = " + counter);
                        counter++;

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

                            run.XYData.Display();

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
                sb.Append(-1);
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
            Task finder = new O16O18TFFTask(5);

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
            massTagCollection.MassTagIDList = new List<long> { 174139157, 176185405, 21600090, 174243317, 20843264, 20754135, 6750879, 20769714, 7340469, 174292363, 8996991, 106783311, 20750746, 174266016, 174266016, 22620034, 6700555, 83198927, 174278099, 20826585, 21594117, 20750619, 20870474, 174072413, 6748846, 20750431, 20891631, 174295357, 27701722, 27701722, 73241427, 20795981, 20773542, 6695779, 21603042, 20751228, 21600087, 176216012, 26594398, 20758777, 83214995, 8590513, 22617167, 85890845, 85890845, 6672088, 174988038, 21597049, 70862387, 20846477, 174587683, 174295446, 22600964, 174270656, 20870121, 29421283, 174258169, 1115, 20846482, 20876755, 174271084, 22612813, 174269896, 174229339, 83453028, 83520553, 188181847, 10627276, 7117205, 106722091, 20751368, 20843080, 20748196, 29311976, 20750962, 83515667, 174267720, 20753084, 22641064, 20874671, 83425121, 20868912, 20889285, 174272460, 20748135, 20869292, 174983687, 21594066, 26879329, 20746921, 20749178, 8784413, 22614172, 106682878, 20759185, 104992667, 69165282, 20750887, 20868809, 174483200, 174270903, 174270903, 26660942, 6684837, 26376663, 26376663, 174256976, 172383146, 83457115, 26663224, 20860941, 20868752, 22796070, 20750948, 6952818, 174273856, 174269921, 20791763, 21597685, 83165951, 20751017, 20830762, 20804123, 188525272, 20795937, 6892974, 6892974, 20893113, 104928639, 83485008, 21594246, 20769984, 20871084, 71445396, 20751427, 129994477, 21320895, 29362244, 20860948, 22626540, 20822789, 20750707, 176172514, 171520263, 20843016, 20868768, 20868749, 20870905, 10182286, 165846582, 20847067, 20746637, 20771065, 63626229, 104856453, 83274536, 20791514, 20750721, 26383300, 26383300, 12961058, 174254332, 23071928, 174275079, 174617377, 174355999, 174987572, 85276891, 85276891, 174269646, 20858425, 29384623, 20869437, 29386298, 13878, 6853895, 20750874, 20846540, 20843007, 20798359, 26681401, 83224838, 174987325, 174269426, 63623641, 22614129, 20869104, 20843684, 174269633, 8557599, 20768031, 20759232, 29329307, 83483332, 20788553, 21779128, 20873371, 26869679, 21600088, 20843005, 29358802, 153746694, 20749207, 29366448, 20846846, 130047802, 20747417, 21596585, 174273407, 6654352, 176180559, 174269904, 172999561, 20851485, 29362500, 174152050, 20750871, 20930317, 174133451, 6635297, 29409356, 8965817, 20837056, 20746216, 6719681, 21593976, 20951963, 63688457, 22614560, 174274513, 22111620, 107354072, 22090494, 20759723, 20844073, 29289073, 104033695, 63725418, 6702977, 20773238, 20748373, 20887382, 10616562, 22614096, 83480288, 174939713, 20748546, 174235694, 106714721, 29357786, 174335187, 83451085, 9354658, 104983473, 22881717, 173942701, 7337724, 174074589, 20745330, 20751675, 174074413, 20847125, 21594818, 20745222, 21594132, 20847197, 174118193, 83480234, 20844369, 21594227, 174337595, 21594038, 10111328, 174168371, 20788548, 20751328, 104822905, 8776822, 20868751, 20868751, 20776242, 174983301, 104083118, 20777007, 20783385, 20783385, 175002491, 20752252, 6666532, 22781644, 29337432, 20797718, 104822668, 8665045, 26440177, 26440276, 6643889, 174235852, 20864191, 20784026, 83514449, 174972596, 152856646, 20750924, 6880220, 20751002, 20870148, 20907792, 20838153, 165456717, 20744758, 20746104, 20753628, 48364162, 9701077, 7328791, 20758949, 20746072, 11617827, 63731700, 20707543, 83451302, 83514552, 20750967, 20766841, 174291623, 6706842, 7328781, 29341845, 20826686, 83272743, 106725620, 188020594, 172997760, 10287411, 6634809, 20864003, 20868762, 20745961, 83451053, 83479115, 20750848, 29521826, 174220485, 20846795, 20750838, 6702716, 20775573, 20744510, 174356253, 20713296, 174199863, 83457068, 63632355, 20767474, 20838091, 6959230, 20842965, 27026361, 174287377, 83451013, 83276739, 174351323, 20750860, 21467365, 20750875, 7735381, 20759930, 26691466, 174335275, 174335275, 174124411, 20744526, 174270640, 174982862, 104113249, 15355140, 174130687, 29406811, 27807490, 174285076, 104141042, 22614247, 20758941, 174297405, 20876757, 26761954, 20864602, 20745302, 20767550, 20851546, 20752078, 22631487, 20851844, 20864563, 20744484, 83221918, 20751052, 83272634, 10184834, 174063814, 83275360, 20745266, 147574848, 83478979, 102384144, 11444439, 21768023, 20751961, 174116060, 48348683, 90977764, 20758893, 48387863, 26879913, 20750877, 20753957, 148391058, 20851566, 20707540, 174285086, 22631496, 174166295, 21593932, 83514300, 29401698, 21800573, 147665381, 104156764, 174236463, 20759750, 6643962, 6959173, 188549952, 104907588, 20790729, 173938624, 21792001, 174220595, 21594304, 113202318, 109945776, 174595922, 22580887, 174150641, 20832433, 174351462, 11086105, 174983253, 83272667, 8980588, 20751260, 174230653, 174230653, 6959269, 6688878, 20769574, 6709133, 6709133, 107069524, 106722349, 174596893, 20760368, 174116762, 21756606, 21786535, 174285098, 174262600, 20797597, 20750886, 26435152, 21756766, 20750857, 26381565, 83479230, 7919252, 174351526, 83478993, 21759859, 20908613, 21604352, 188051970, 20778983, 76659790, 29357989, 188689072, 174285025, 33694302, 14849817, 106916571, 20793506, 22613846, 10149009, 21507800, 26378236, 6959327, 83274713, 164653316, 159962064, 22870387, 174595847, 188552553, 20864611, 174285048, 22646612, 174252121, 20851547, 22604538, 20746151, 22807265, 21600089, 165899057, 20835099, 7340641, 20746152, 83289607, 20754229, 174166324, 174149678, 174149678, 21278767, 20846800, 20853826, 22891478, 7316050, 106722292, 7633434, 20746907, 174655724, 83515016, 21078635, 22609689, 22609749, 174236218, 21785866, 26678501, 20746144, 8990784, 29299699, 20758743, 83273329, 7329368, 20746153, 104766221, 22614087, 22609710, 104189878, 7328787, 22609713, 22813072, 20703886, 26418578, 20711589, 71613674, 20853245, 21593938, 174116064, 174254529, 20853248, 20853248, 20842968, 22631599, 174285243, 20878714, 174135568, 174132671, 20853411, 174222902, 20855963, 22891465, 20855829, 20855937, 22827215, 20757235, 22627261, 20801212, 130017486, 20746150, 20858510, 21468174, 21609150, 22614082, 174273239, 22609680, 20751030, 20868876, 22810428, 20865400, 83242561, 20746654, 20746172, 174169336, 173082679, 188606592, 22635892, 63682520, 20855835, 83424455, 174073805, 20925129, 174178389, 174166345, 20897109, 20852057, 174252126, 20775579, 20825160, 20824237, 22618787, 21593936, 20746164, 22609681, 20763531, 20791942, 26617872, 22609711, 188198598, 22637616, 153705180, 153705180, 106671523, 104166943, 174285029, 22788051, 20758891, 21750841, 10197234, 20961947, 174149600, 188501715, 20827087, 20746183, 174124103, 21593942, 174121432, 22842275, 106723699, 174270236, 20746493, 20782921, 173938399, 8573670, 21593961, 83450991, 22614088, 21799979, 10176811, 188099376, 83272697, 20761655, 20984714, 22622987, 173052443, 22619464, 7328809, 174235446, 22002087, 22798905, 158761276, 31947878, 22079682, 26879572, 188686509, 22888239, 22609692, 6959132, 22622989, 174256286, 22837419, 176205545, 20843019, 188490213, 188547022, 188547022, 20746205, 21593912, 22618774, 20795949, 20758841, 20844798, 83471092, 20750881, 174078187, 20744455, 20746154, 20746142, 174149645, 20846531, 106722293 };

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Mouse_P303", "Pogo");
            importer.Import(massTagCollection);

            massTagCollection.MassTagList = massTagCollection.MassTagList.Distinct().ToList();

            massTagCollection.FilterOutDuplicates();

            return massTagCollection;
        }


        private MassTagCollection GetMouse_VIPERMassTags2()
        {
            MassTagCollection massTagCollection = new MassTagCollection();
            massTagCollection.MassTagIDList = new List<long> { 1108, 1115, 12896, 13878, 15144, 15609, 6634809, 6634940, 6635297, 6643880, 6643889, 6643962, 6644331, 6654332, 6654352, 6662822, 6666532, 6672049, 6672088, 6679478, 6680037, 6684837, 6688878, 6688906, 6695779, 6700555, 6702292, 6702475, 6702716, 6702977, 6706842, 6707619, 6709133, 6711989, 6719681, 6748262, 6748846, 6750879, 6852620, 6853895, 6880220, 6882545, 6892974, 6904760, 6918442, 6943051, 6948987, 6951213, 6952818, 6954996, 6955290, 6959132, 6959138, 6959173, 6959230, 6959238, 6959269, 6959327, 6959801, 6960369, 6967184, 6967222, 6967423, 6967916, 6976171, 6976239, 6982652, 7023673, 7038425, 7117205, 7194584, 7316050, 7316062, 7328781, 7328787, 7328791, 7328809, 7329368, 7337724, 7340469, 7340641, 7438750, 7501262, 7570148, 7583051, 7633434, 7735381, 7919252, 8093089, 8094248, 8557599, 8573670, 8590513, 8665045, 8776822, 8784413, 8965817, 8980588, 8990784, 8996991, 9279574, 9296866, 9300700, 9324699, 9354658, 9378018, 9542840, 9689876, 9701077, 10111328, 10116037, 10141387, 10149009, 10155130, 10160290, 10171139, 10173366, 10176811, 10180282, 10182286, 10184829, 10184834, 10184857, 10184972, 10195959, 10197234, 10244352, 10287411, 10468678, 10616562, 10627276, 10630381, 11086105, 11444439, 11617827, 12668513, 12935151, 12961058, 13094145, 14607744, 14849817, 15301049, 15355140, 15744469, 20703886, 20707387, 20707535, 20707540, 20707543, 20711589, 20711976, 20713296, 20726781, 20744450, 20744452, 20744455, 20744465, 20744473, 20744484, 20744496, 20744500, 20744510, 20744526, 20744758, 20744905, 20745222, 20745266, 20745302, 20745330, 20745358, 20745843, 20745961, 20746008, 20746072, 20746104, 20746142, 20746143, 20746144, 20746145, 20746148, 20746149, 20746150, 20746151, 20746152, 20746153, 20746154, 20746158, 20746163, 20746164, 20746169, 20746170, 20746172, 20746177, 20746183, 20746184, 20746185, 20746187, 20746189, 20746190, 20746192, 20746197, 20746199, 20746205, 20746216, 20746219, 20746283, 20746284, 20746287, 20746289, 20746300, 20746467, 20746493, 20746637, 20746654, 20746865, 20746907, 20746921, 20747417, 20748135, 20748196, 20748373, 20748546, 20749178, 20749207, 20749480, 20749683, 20750431, 20750619, 20750707, 20750718, 20750721, 20750733, 20750746, 20750835, 20750838, 20750846, 20750848, 20750857, 20750860, 20750863, 20750871, 20750874, 20750875, 20750877, 20750878, 20750881, 20750883, 20750886, 20750887, 20750914, 20750924, 20750931, 20750948, 20750962, 20750967, 20750969, 20750991, 20751002, 20751012, 20751017, 20751030, 20751032, 20751052, 20751228, 20751260, 20751328, 20751368, 20751427, 20751460, 20751675, 20751706, 20751961, 20752078, 20752252, 20752297, 20753059, 20753084, 20753628, 20753957, 20754026, 20754135, 20754229, 20754238, 20754337, 20754702, 20755374, 20757235, 20757600, 20758743, 20758777, 20758829, 20758841, 20758891, 20758893, 20758894, 20758898, 20758941, 20758949, 20759149, 20759185, 20759232, 20759330, 20759342, 20759533, 20759613, 20759723, 20759750, 20759930, 20760368, 20761655, 20761660, 20762771, 20763531, 20766841, 20766970, 20767474, 20767483, 20767487, 20767488, 20767504, 20767520, 20767550, 20768031, 20768215, 20769574, 20769714, 20769908, 20769984, 20770230, 20771065, 20773238, 20773542, 20773993, 20774246, 20775573, 20775578, 20775579, 20775604, 20775710, 20776101, 20776242, 20777007, 20778333, 20778983, 20780627, 20780705, 20782921, 20783385, 20783997, 20784026, 20786649, 20788542, 20788548, 20788553, 20790729, 20791514, 20791763, 20791942, 20791971, 20793506, 20795933, 20795937, 20795949, 20795981, 20797597, 20797718, 20797723, 20798353, 20798359, 20800105, 20800268, 20800270, 20801212, 20801649, 20804123, 20822789, 20824237, 20825160, 20826537, 20826585, 20826686, 20827087, 20827460, 20828807, 20830762, 20832433, 20835099, 20835892, 20837056, 20838091, 20838153, 20842959, 20842961, 20842965, 20842967, 20842968, 20843005, 20843007, 20843016, 20843019, 20843040, 20843080, 20843238, 20843264, 20843279, 20843684, 20844073, 20844078, 20844369, 20844798, 20844883, 20846477, 20846482, 20846531, 20846540, 20846795, 20846796, 20846800, 20846809, 20846842, 20846846, 20846867, 20847038, 20847067, 20847125, 20847197, 20848831, 20851485, 20851546, 20851547, 20851548, 20851566, 20851632, 20851844, 20852057, 20853066, 20853239, 20853240, 20853245, 20853248, 20853411, 20853826, 20855724, 20855727, 20855828, 20855829, 20855835, 20855937, 20855963, 20858425, 20858510, 20858863, 20860919, 20860925, 20860941, 20860943, 20860948, 20860952, 20860966, 20861337, 20862843, 20863431, 20864003, 20864181, 20864191, 20864563, 20864570, 20864602, 20864611, 20864704, 20864744, 20865400, 20868745, 20868746, 20868749, 20868750, 20868751, 20868752, 20868761, 20868762, 20868765, 20868768, 20868784, 20868809, 20868876, 20868912, 20868960, 20869104, 20869292, 20869437, 20870121, 20870148, 20870474, 20870905, 20871084, 20871278, 20873371, 20874671, 20876387, 20876755, 20876757, 20878714, 20887382, 20889285, 20891631, 20893113, 20893266, 20894847, 20897109, 20902542, 20907792, 20907864, 20907879, 20908203, 20908613, 20909435, 20909610, 20909622, 20912000, 20925080, 20925129, 20925143, 20930317, 20933852, 20951963, 20954243, 20961595, 20961947, 20984714, 21027762, 21031320, 21032314, 21052896, 21053333, 21061246, 21066649, 21078635, 21111352, 21113773, 21115948, 21171865, 21278767, 21320895, 21405431, 21447815, 21467365, 21468174, 21501904, 21507800, 21593912, 21593915, 21593919, 21593920, 21593926, 21593927, 21593928, 21593930, 21593932, 21593934, 21593936, 21593938, 21593940, 21593942, 21593949, 21593951, 21593952, 21593961, 21593976, 21594038, 21594052, 21594066, 21594117, 21594132, 21594227, 21594246, 21594304, 21594778, 21594818, 21596237, 21596585, 21596981, 21597049, 21597375, 21597685, 21598232, 21598439, 21600087, 21600088, 21600089, 21600090, 21600491, 21603042, 21604352, 21605838, 21609150, 21613729, 21613783, 21750841, 21751274, 21751447, 21753299, 21753424, 21756606, 21756766, 21759809, 21759859, 21759860, 21761285, 21761565, 21762616, 21765029, 21768023, 21771508, 21779128, 21785866, 21786132, 21786535, 21791234, 21792001, 21792015, 21793584, 21795186, 21799979, 21800573, 21804924, 21814834, 21942782, 21945281, 21956085, 21960960, 22002087, 22058877, 22072456, 22079682, 22084964, 22090494, 22090567, 22094351, 22099346, 22111620, 22112047, 22544693, 22558484, 22578929, 22580887, 22587699, 22598396, 22600964, 22604412, 22604538, 22609663, 22609664, 22609669, 22609670, 22609680, 22609681, 22609682, 22609689, 22609692, 22609699, 22609704, 22609710, 22609711, 22609713, 22609720, 22609723, 22609740, 22609741, 22609749, 22611753, 22611820, 22612813, 22613846, 22614082, 22614085, 22614087, 22614088, 22614096, 22614129, 22614172, 22614247, 22614560, 22617167, 22618774, 22618775, 22618783, 22618787, 22619464, 22620034, 22622879, 22622905, 22622987, 22622989, 22622999, 22623005, 22623476, 22626540, 22627261, 22627307, 22627350, 22631038, 22631424, 22631487, 22631496, 22631599, 22631604, 22635892, 22637616, 22638025, 22639017, 22640462, 22641064, 22642457, 22645853, 22646003, 22646077, 22646612, 22647165, 22650470, 22781644, 22788051, 22789335, 22793570, 22795357, 22796070, 22798658, 22798839, 22798905, 22801681, 22807265, 22809582, 22810428, 22813072, 22822497, 22826593, 22826652, 22827215, 22837419, 22841066, 22842046, 22842275, 22842393, 22848968, 22853665, 22867751, 22870387, 22881717, 22888239, 22888332, 22891233, 22891465, 22891478, 22926484, 23071928, 26375560, 26376663, 26377720, 26378236, 26378263, 26381526, 26381565, 26383300, 26386893, 26389683, 26418578, 26435152, 26440177, 26440276, 26470073, 26495506, 26506471, 26517688, 26571862, 26594398, 26617872, 26627612, 26650633, 26660942, 26663224, 26669875, 26678501, 26681401, 26682218, 26691466, 26725922, 26761954, 26811107, 26869679, 26879260, 26879329, 26879572, 26879913, 27026361, 27061571, 27701722, 27807490, 28237014, 28936239, 29279637, 29289040, 29289073, 29299699, 29305029, 29311976, 29318769, 29329307, 29336337, 29337432, 29341845, 29357714, 29357786, 29357989, 29358802, 29362244, 29362500, 29366448, 29366639, 29369737, 29380959, 29381037, 29384623, 29386298, 29401698, 29406811, 29409336, 29409356, 29421283, 29422592, 29432802, 29521826, 29528532, 29534629, 29540704, 29540897, 31341762, 31947878, 32409218, 33575618, 33694302, 33727276, 44018876, 44063080, 44281706, 45577343, 46506440, 47387109, 47387350, 47423771, 48193970, 48228533, 48257973, 48285047, 48348683, 48364162, 48387863, 48419477, 48472694, 60447808, 60454697, 61241144, 62983300, 63553920, 63623641, 63626229, 63632355, 63682520, 63688457, 63725418, 63731700, 63743864, 64177597, 64311204, 66376316, 66406028, 66460158, 67256508, 67271783, 67359884, 69165282, 69446932, 70235879, 70238093, 70250138, 70272422, 70862387, 70884604, 71402868, 71445396, 71613674, 71867691, 73241427, 76602742, 76659790, 83165951, 83167251, 83185887, 83198887, 83198927, 83214995, 83221918, 83222492, 83224838, 83242561, 83242625, 83242779, 83242784, 83245217, 83248317, 83272632, 83272634, 83272667, 83272678, 83272697, 83272718, 83272743, 83272833, 83272897, 83273329, 83274536, 83274713, 83275360, 83276739, 83289607, 83297177, 83424455, 83425121, 83428224, 83428592, 83433085, 83447446, 83447825, 83450991, 83451013, 83451028, 83451053, 83451085, 83451128, 83451302, 83451306, 83451308, 83452780, 83453028, 83457068, 83457115, 83457958, 83458339, 83458963, 83463345, 83464394, 83471092, 83473161, 83478971, 83478979, 83478993, 83479097, 83479115, 83479230, 83479637, 83479985, 83480234, 83480288, 83480321, 83480498, 83481566, 83483332, 83484228, 83484283, 83485008, 83485062, 83486171, 83488456, 83488982, 83507789, 83514300, 83514319, 83514449, 83514552, 83514847, 83515016, 83515667, 83516030, 83517019, 83520044, 83520381, 83520553, 83533906, 83569134, 83574675, 83631937, 85276891, 85890845, 87062698, 90977764, 101023360, 102353484, 102384144, 104032327, 104033068, 104033695, 104051061, 104069703, 104075733, 104083118, 104113249, 104127862, 104129510, 104141042, 104148414, 104156764, 104166943, 104179342, 104189878, 104757686, 104766221, 104822668, 104822824, 104822905, 104824204, 104824902, 104828624, 104829715, 104856453, 104874034, 104907588, 104913537, 104917741, 104928639, 104937118, 104953031, 104966969, 104967597, 104972200, 104983473, 104992667, 106671523, 106671574, 106671617, 106671702, 106682878, 106693424, 106714721, 106716098, 106722091, 106722287, 106722292, 106722293, 106722349, 106722435, 106722893, 106723368, 106723699, 106725620, 106726884, 106749812, 106773767, 106777127, 106783311, 106790859, 106813587, 106916571, 107069524, 107295258, 107319185, 107354072, 107600491, 107750996, 108555342, 108562061, 109945776, 110683267, 113202318, 117803232, 118097582, 118099679, 119002007, 119393220, 129994477, 130017486, 130028729, 130047802, 130068059, 130073362, 147248533, 147259173, 147261011, 147574848, 147665381, 147999495, 148246450, 148268434, 148391058, 148856185, 149175269, 149186196, 151367177, 152856646, 152873339, 153099350, 153160677, 153184641, 153705180, 153746694, 153751074, 153828377, 156597747, 158761276, 159962064, 163895124, 163933160, 164653316, 164692680, 165450313, 165456717, 165765456, 165846582, 165856379, 165881867, 165899057, 170771611, 171520263, 171524957, 171529042, 171533822, 172380260, 172383146, 172390323, 172392571, 172997760, 172999561, 173012300, 173052443, 173082679, 173088997, 173938399, 173938624, 173939170, 173942295, 173942701, 173944778, 173946173, 174062269, 174063722, 174063814, 174065217, 174065378, 174065702, 174066461, 174070218, 174070296, 174070794, 174072413, 174073799, 174073805, 174073812, 174073813, 174074314, 174074413, 174074589, 174076186, 174078187, 174078388, 174079508, 174081174, 174081351, 174082675, 174116046, 174116060, 174116062, 174116064, 174116067, 174116070, 174116080, 174116083, 174116097, 174116102, 174116118, 174116123, 174116134, 174116135, 174116144, 174116156, 174116158, 174116166, 174116196, 174116197, 174116305, 174116326, 174116370, 174116623, 174116625, 174116677, 174116762, 174116767, 174117675, 174117973, 174118193, 174118842, 174119366, 174119586, 174119595, 174119610, 174120136, 174120146, 174120515, 174120906, 174121432, 174122478, 174123222, 174124103, 174124411, 174125057, 174126783, 174127361, 174130687, 174132092, 174132093, 174132094, 174132105, 174132130, 174132139, 174132145, 174132221, 174132541, 174132666, 174132671, 174133451, 174135568, 174135852, 174136101, 174137476, 174137486, 174137567, 174138070, 174138408, 174138889, 174139157, 174139168, 174139197, 174140128, 174142925, 174149596, 174149600, 174149603, 174149615, 174149637, 174149645, 174149651, 174149654, 174149658, 174149678, 174149727, 174149736, 174149836, 174149890, 174149899, 174149968, 174150017, 174150030, 174150060, 174150151, 174150641, 174150903, 174151166, 174152050, 174152289, 174152303, 174154372, 174155043, 174155568, 174155596, 174155752, 174156175, 174163041, 174166230, 174166231, 174166295, 174166320, 174166324, 174166345, 174166377, 174166546, 174166866, 174167077, 174167111, 174168371, 174169336, 174169545, 174169680, 174171836, 174172187, 174172422, 174172935, 174173204, 174177540, 174178389, 174183470, 174199558, 174199569, 174199863, 174203351, 174220478, 174220485, 174220595, 174220993, 174222902, 174224326, 174225200, 174227505, 174229339, 174230653, 174233448, 174235426, 174235435, 174235439, 174235446, 174235460, 174235474, 174235488, 174235533, 174235545, 174235554, 174235685, 174235694, 174235852, 174235936, 174236218, 174236437, 174236463, 174237586, 174237932, 174238407, 174242277, 174242671, 174243317, 174245022, 174251382, 174252112, 174252113, 174252121, 174252126, 174252137, 174252148, 174252150, 174252188, 174252202, 174252226, 174252237, 174252241, 174252341, 174252360, 174252689, 174252701, 174254332, 174254529, 174254846, 174255462, 174255765, 174256286, 174256976, 174258169, 174258910, 174259070, 174259292, 174260145, 174260551, 174262176, 174262600, 174266016, 174266989, 174267720, 174269316, 174269327, 174269331, 174269336, 174269348, 174269424, 174269426, 174269452, 174269473, 174269477, 174269630, 174269633, 174269646, 174269896, 174269904, 174269921, 174270007, 174270106, 174270236, 174270310, 174270372, 174270397, 174270405, 174270640, 174270656, 174270903, 174270948, 174271084, 174271143, 174271232, 174272460, 174272914, 174273239, 174273337, 174273407, 174273856, 174273865, 174274513, 174274695, 174274925, 174275079, 174275579, 174277205, 174278099, 174278396, 174278591, 174278902, 174281533, 174285020, 174285022, 174285025, 174285028, 174285029, 174285034, 174285035, 174285037, 174285041, 174285045, 174285046, 174285048, 174285058, 174285076, 174285086, 174285097, 174285098, 174285120, 174285129, 174285148, 174285189, 174285223, 174285243, 174285257, 174285303, 174285311, 174285352, 174285452, 174285599, 174285768, 174285897, 174285913, 174285940, 174287377, 174287702, 174290229, 174291419, 174291623, 174292101, 174292363, 174293951, 174294364, 174295357, 174295446, 174297405, 174324417, 174327701, 174335157, 174335169, 174335187, 174335203, 174335245, 174335275, 174335301, 174335333, 174335373, 174335547, 174336336, 174336395, 174337595, 174341209, 174341877, 174342540, 174343061, 174343375, 174351170, 174351176, 174351179, 174351180, 174351187, 174351219, 174351234, 174351276, 174351281, 174351323, 174351370, 174351416, 174351462, 174351509, 174351526, 174351533, 174351656, 174352383, 174352756, 174355119, 174355999, 174356253, 174356651, 174357159, 174357752, 174360471, 174360761, 174365621, 174373543, 174463161, 174479241, 174479242, 174479353, 174479985, 174481827, 174482390, 174483200, 174483819, 174493267, 174497891, 174503409, 174565232, 174565257, 174587169, 174587683, 174587975, 174588341, 174589023, 174595847, 174595876, 174595894, 174595922, 174596893, 174597243, 174606304, 174606398, 174617377, 174617645, 174618909, 174632205, 174643404, 174653940, 174654333, 174654886, 174655542, 174655724, 174658481, 174668465, 174673638, 174923854, 174939615, 174939713, 174941499, 174951093, 174951172, 174954971, 174955644, 174965132, 174972596, 174976009, 174982813, 174982836, 174982852, 174982856, 174982862, 174982993, 174983004, 174983172, 174983253, 174983301, 174983367, 174983687, 174984369, 174987325, 174987572, 174988038, 174988098, 174988489, 174989245, 174989252, 174996142, 175001607, 175002491, 175002529, 175035669, 175035675, 175035739, 175035750, 175036077, 175063915, 176171902, 176171918, 176171988, 176171993, 176171996, 176171998, 176172005, 176172039, 176172109, 176172118, 176172514, 176175448, 176175551, 176176524, 176176654, 176176658, 176177424, 176178431, 176178841, 176179023, 176179465, 176180380, 176180559, 176180963, 176183446, 176184750, 176185405, 176189438, 176190717, 176196797, 176205545, 176205563, 176205586, 176205628, 176206065, 176216012, 176222187, 176223630, 184003831, 184006020, 184049146, 188020594, 188023787, 188046478, 188051970, 188090713, 188094517, 188099376, 188179622, 188181847, 188198538, 188198598, 188202388, 188221354, 188238231, 188243394, 188249484, 188266450, 188322477, 188336812, 188456774, 188478611, 188478620, 188478628, 188482032, 188482068, 188486159, 188490213, 188494380, 188495235, 188501715, 188506568, 188506689, 188506861, 188509673, 188525272, 188535101, 188535134, 188541119, 188542831, 188547022, 188547032, 188549445, 188549952, 188552534, 188552553, 188552577, 188552634, 188559744, 188559745, 188559756, 188559776, 188559865, 188566943, 188567062, 188567169, 188568728, 188569950, 188594670, 188594700, 188594703, 188601652, 188606592, 188606911, 188614119, 188617518, 188618205, 188644589, 188655985, 188656004, 188673964, 188686509, 188686585, 188689072, 188691531 };

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Mouse_P303", "Pogo");
            importer.Import(massTagCollection);

            massTagCollection.MassTagList = massTagCollection.MassTagList.Distinct().ToList();

            massTagCollection.FilterOutDuplicates();

            return massTagCollection;
        }


        private MassTagCollection GetMouse_ProblemMassTags2()
        {
            MassTagCollection massTagCollection = new MassTagCollection();
            massTagCollection.MassTagIDList = new List<long> { 106916571 };

            MassTagFromSqlDBImporter importer = new MassTagFromSqlDBImporter("MT_Mouse_P303", "Pogo");
            importer.Import(massTagCollection);

            massTagCollection.MassTagList = massTagCollection.MassTagList.Distinct().ToList();

            massTagCollection.FilterOutDuplicates();

            return massTagCollection;
        }

    }
}
