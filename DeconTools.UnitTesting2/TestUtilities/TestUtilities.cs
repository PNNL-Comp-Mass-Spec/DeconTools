using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Runs;
using DeconTools.Workflows.Backend.Core;

namespace DeconTools.UnitTesting2
{
    public class TestUtilities
    {
        private static string xcaliburTestfile = FileRefs.TestFileBasePath + "\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        private static string uimfTestFile = FileRefs.TestFileBasePath + "\\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf";

        public static void GetXYValuesToStringBuilder(StringBuilder sb, double[] xvals, double[] yvals)
        {
            if (sb == null) return;
            for (var i = 0; i < xvals.Length; i++)
            {
                sb.Append(xvals[i]);
                sb.Append("\t");
                sb.Append(yvals[i]);
                sb.Append("\n");
            }
        }




        public static bool AreIsosResultsTheSame(List<IsosResult> resultList1, List<IsosResult> resultList2)
        {
            if (resultList1.Count != resultList2.Count) return false;     //counts are different

            for (var i = 0; i < resultList1.Count; i++)
            {
                var resultIsTheSame = (resultList1[i].IsotopicProfile.ChargeState == resultList2[i].IsotopicProfile.ChargeState &&
                    resultList1[i].IntensityAggregate == resultList2[i].IntensityAggregate &&
                    //resultList1[i].IsotopicProfile.MonoIsotopicMass == resultList2[i].IsotopicProfile.MonoIsotopicMass &&
                    resultList1[i].IsotopicProfile.MonoPeakMZ == resultList2[i].IsotopicProfile.MonoPeakMZ &&
                    resultList1[i].IsotopicProfile.MonoPlusTwoAbundance == resultList2[i].IsotopicProfile.MonoPlusTwoAbundance &&
                    //resultList1[i].IsotopicProfile.MostAbundantIsotopeMass == resultList2[i].IsotopicProfile.MostAbundantIsotopeMass &&
                    resultList1[i].IsotopicProfile.IsSaturated == resultList2[i].IsotopicProfile.IsSaturated &&
                    resultList1[i].IsotopicProfile.OriginalIntensity == resultList2[i].IsotopicProfile.OriginalIntensity &&
                    //resultList1[i].IsotopicProfile.Score == resultList2[i].IsotopicProfile.Score &&
                    resultList1[i].IsotopicProfile.GetNumOfIsotopesInProfile() == resultList2[i].IsotopicProfile.GetNumOfIsotopesInProfile() &&
                    //resultList1[i].IsotopicProfile.GetSignalToNoise() == resultList2[i].IsotopicProfile.GetSignalToNoise() &&
                    //resultList1[i].IsotopicProfile.GetFWHM() == resultList2[i].IsotopicProfile.GetFWHM() &&
                    resultList1[i].IsotopicProfile.getMonoPeak().XValue == resultList2[i].IsotopicProfile.getMonoPeak().XValue &&
                    resultList1[i].IsotopicProfile.getMonoPeak().Height == resultList2[i].IsotopicProfile.getMonoPeak().Height);

                if (!resultIsTheSame)   //result is different
                {
                    Console.WriteLine("The method 'AreIsosResultsTheSame' found at least one differing IsosResult at index = " + i);
                    return false;
                }

            }
            return true;


        }

        public static void IsotopicProfileDataToStringBuilder(StringBuilder sb, IsotopicProfile profile)
        {
            var counter = 0;

            foreach (var peak in profile.Peaklist)
            {
                sb.Append(counter);
                sb.Append("\t");
                sb.Append(peak.XValue);
                sb.Append("\t");
                sb.Append(peak.Height);
                sb.Append("\t");
                sb.Append(peak.Width);
                sb.Append("\t");
                sb.Append(peak.SignalToNoise);
                sb.Append("\n");

                counter++;
            }
        }

        public List<IsosResult> CreateThrashIsosResults1()
        {
            Run run = new XCaliburRun2(xcaliburTestfile);

            run.ScanSetCollection.Create(run, 6000, 6020, 1, 1);

            Task generator = new GenericMSGenerator();
            Task peakDet = new DeconToolsPeakDetectorV2();
            Task decon = new HornDeconvolutor();



            foreach (var scanset in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scanset;
                generator.Execute(run.ResultCollection);
                peakDet.Execute(run.ResultCollection);
                decon.Execute(run.ResultCollection);

            }

            return run.ResultCollection.ResultList;

        }

        public static void DisplayPeaks(List<Peak> peakList)
        {
            var sb = new StringBuilder();
            sb.Append("--------- Peaks found -----------------\n");
            sb.Append("x_value\ty_value\twidth\n");


            foreach (var peak in peakList)
            {
                sb.Append(peak.XValue);
                sb.Append("\t");
                sb.Append(peak.Height);
                sb.Append("\t");
                sb.Append(peak.Width);
                sb.Append("\t");
                sb.Append(Environment.NewLine);
            }
            sb.Append("--------------------------- end ---------------------------------------\n");

            Console.Write(sb.ToString());
        }

        public static void DisplayXYValues(ResultCollection resultCollection)
        {
            DisplayXYValues(resultCollection, double.MinValue, double.MaxValue);
        }

        public static void DisplayXYValues(ResultCollection resultCollection, double lowerX, double upperX)
        {
            var xvals = resultCollection.Run.XYData.Xvalues;
            var yvals = resultCollection.Run.XYData.Yvalues;

            var sb = new StringBuilder();
            sb.Append("--------- XYData found in Run " + resultCollection.Run.DatasetFileOrDirectoryPath + "-----------------\n");
            for (var i = 0; i < xvals.Length; i++)
            {
                if (xvals[i] >= lowerX && xvals[i] <= upperX)
                {
                    sb.Append(xvals[i]);
                    sb.Append("\t");
                    sb.Append(yvals[i]);
                    sb.Append("\n");
                }
            }
            sb.Append("--------------------------- end ---------------------------------------\n");

            Console.Write(sb.ToString());
        }

        public static void DisplayXYValues(XYData xyData)
        {
            DisplayXYValues(xyData, double.MinValue, double.MaxValue);
        }

        public static void DisplayXYValues(XYData xyData, double lowerX, double upperX)
        {
            var sb = new StringBuilder();
            sb.Append("--------- XYData -----------------\n");
            for (var i = 0; i < xyData.Xvalues.Length; i++)
            {
                if (xyData.Xvalues[i] >= lowerX && xyData.Xvalues[i] <= upperX)
                {
                    sb.Append(xyData.Xvalues[i]);
                    sb.Append("\t");
                    sb.Append(xyData.Yvalues[i]);
                    sb.Append("\n");
                }
            }
            sb.Append("--------------------------- end ---------------------------------------\n");

            Console.Write(sb.ToString());
        }


        public static void DisplayIsotopicProfileData(IsotopicProfile profile)
        {
            var counter = 0;

            Console.WriteLine("{0,-8} {1,-9} {2,-10} {3,-10} {4,-10}", "Index", "XValue", "Height", "Width", "S/N");

            foreach (var peak in profile.Peaklist)
            {
                Console.WriteLine("{0,-8} {1,-9} {2,-10:G3} {3,-10} {4,-10}", counter, peak.XValue, peak.Height, peak.Width, peak.SignalToNoise);
                counter++;
            }
        }

        public static void DisplayMSFeatures(List<IsosResult> resultList, int maxFeaturesToShow = 100)
        {
            Console.WriteLine("------------MSFeatures ---------------");

            Console.WriteLine("{0,-7} {1,-9} {2,-10} {3,-11} {4,-4} {5,-12} {6,-10} {7,-18} {8}",
                "Index", "ScanNum", "MonoMass", "m/z", "z", "Intensity", "Score", "InterferenceScore", "Status");

            var i = 0;
            foreach (var item in resultList)
            {
                if (i >= maxFeaturesToShow)
                {
                    Console.WriteLine();
                    Console.WriteLine("... {0} additional features not displayed", resultList.Count - i);
                    break;
                }

                Console.WriteLine("{0,-7} {1,-9} {2,-10:F4} {3,-11:F5} {4,-4} {5,-12:E3} {6,-10:G3} {7,-18:F3} {8}",
                    item.MSFeatureID,
                    item.ScanSet.PrimaryScanNumber,
                    item.IsotopicProfile.MonoIsotopicMass,
                    item.IsotopicProfile.MonoPeakMZ,
                    item.IsotopicProfile.ChargeState,
                    item.IntensityAggregate,
                    item.IsotopicProfile.Score,
                    item.InterferenceScore,
                    item.Flags.Count == 0 ? "" : "FLAGGED");

                i++;
            }

        }

        public static void DisplayRunInformation(Run run)
        {

            Console.WriteLine("Dataset name = " + run.DatasetName);
            Console.WriteLine("Dataset path = " + run.DatasetDirectoryPath);
            Console.WriteLine("MinScan = " + run.MinLCScan);
            Console.WriteLine("MaxScan = " + run.MaxLCScan);
            Console.WriteLine("FileType = " + run.MSFileType);
            Console.WriteLine("MassIsAligned = " + run.MassIsAligned);
            Console.WriteLine("NETIsAligned = " + run.NETIsAligned);
        }


        public static IqTarget GetIQTargetStandard(int standardNum)
        {
            //MassTagID	MonoisotopicMass	NET	NETStDev	Obs	minMSGF	mod_count	mod_description	pmt_quality_score	peptide	peptideex	Multiple_Proteins
            //86963986	1516.791851	0.227147	0.007416702	3	1	0		2.00000	AAKEGISCEIIDLR	M.AAKEGISCEIIDLR.T	0

            IqTarget target = new IqChargeStateTarget(null);
            target.ID = 86963986;
            target.MonoMassTheor = 1516.791851;
            target.EmpiricalFormula = "C64H112N18O22S";
            target.Code = "AAKEGISCEIIDLR";
            target.ChargeState = 2;
            target.ElutionTimeTheor = 0.227147;
            target.MZTheor = target.MonoMassTheor / target.ChargeState + Globals.PROTON_MASS;


            var theorFeatureGenerator = new JoshTheorFeatureGenerator();
            target.TheorIsotopicProfile = theorFeatureGenerator.GenerateTheorProfile(target.EmpiricalFormula, target.ChargeState);

            return target;
        }


        public static PeptideTarget GetMassTagStandard(int standardNum)
        {
            var mt = new PeptideTarget {
                ID = 86963986,
                MonoIsotopicMass = 1516.791851,
                Code = "AAKEGISCEIIDLR"};

            mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();
            mt.NormalizedElutionTime = 0.2284955f;
            mt.ChargeState = 2;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;

            return mt;


        }

        public static List<PeptideTarget> CreateTestMassTagList()
        {
            var mtList = new List<PeptideTarget>();

            var mt = new PeptideTarget {
                ID = 24769,
                MonoIsotopicMass = 2086.0595,
                ChargeState = 2};

            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;
            mt.Code = "DFNEALVHQVVVAYAANAR";
            mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();
            mtList.Add(mt);
            return mtList;
        }


        public static List<PeptideTarget> CreateN14N15TestMassTagList()
        {
            var mtList = new List<PeptideTarget>();

            var mt = new PeptideTarget {
                ID = 23085473,
                NormalizedElutionTime = 0.3807834F,
                MonoIsotopicMass = 2538.33284203802,
                ChargeState = 3};

            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;
            mt.Code = "AIHQPAPTFAEQSTTSEILVTGIK";
            mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();
            mtList.Add(mt);
            return mtList;
        }

        public static void DisplayXYValues(Run run)
        {

        }

        public static void DisplayMSLevelData(Run run)
        {
            var sb = new StringBuilder();
            sb.Append("scan\tMSLevel\n");

            for (var i = run.MinLCScan; i <= run.MaxLCScan; i++)
            {
                sb.Append(i);
                sb.Append("\t");
                sb.Append(run.GetMSLevel(i));
                sb.Append("\n");

            }

            Console.WriteLine(sb.ToString());
        }

        public static void DisplayScanSetData(List<ScanSet> scanSetList)
        {
            var sb = new StringBuilder();

            sb.Append("--------------- ScanSetCollection details ---------------------- \n");

            foreach (var scan in scanSetList)
            {
                sb.Append(scan.PrimaryScanNumber);
                sb.Append("\t{");
                for (var i = 0; i < scan.IndexValues.Count; i++)
                {
                    sb.Append(scan.IndexValues[i]);
                    if (i == scan.IndexValues.Count - 1)
                    {
                        sb.Append("}");
                    }
                    else
                    {
                        sb.Append(", ");
                    }

                }
                sb.Append(Environment.NewLine);


            }
            Console.WriteLine(sb.ToString());

        }

        public static List<Peak> GeneratePeakList(ScanSet scanSet)
        {
            Run run = new XCaliburRun2(xcaliburTestfile);

            Task generator = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var peakDet = new DeconToolsPeakDetectorV2 {
                PeakToBackgroundRatio = 1.3,
                SignalToNoiseThreshold = 2,
                IsDataThresholded = true};

            run.CurrentScanSet = scanSet;

            generator.Execute(run.ResultCollection);

            peakDet.Execute(run.ResultCollection);

            return run.PeakList;
        }

        public static Run GetStandardUIMFRun()
        {
            return new UIMFRun(uimfTestFile);

        }

        public static void DisplayInfoForProcess(System.Diagnostics.Process currentProcess)
        {
            var sb = new StringBuilder();
            sb.Append("Process Name =\t" + currentProcess.ProcessName);
            sb.Append(Environment.NewLine);
            sb.Append("Private bytes =\t" + string.Format("{0:N0}", currentProcess.PrivateMemorySize64));
            sb.Append(Environment.NewLine);

            Console.Write(sb.ToString());
        }

        public static List<PeptideTarget> CreateO16O18TestMassTagList1()
        {
            var mtList = new List<PeptideTarget>();

            var mt = new PeptideTarget {
                ID = 1142,
                MonoIsotopicMass = 921.4807035,
                ChargeState = 2};

            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;
            mt.Code = "AEFVEVTK";
            mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();
            mtList.Add(mt);
            return mtList;
        }

        public static void WriteToFile(XYData xyData, string outputFile)
        {
            using (var sw = new StreamWriter(outputFile))
            {
                sw.WriteLine("mz\tintensity");
                for (var i = 0; i < xyData.Xvalues.Length; i++)
                {
                    sw.Write(xyData.Xvalues[i]);
                    sw.Write("\t");
                    sw.Write(xyData.Yvalues[i]);
                    sw.Write(Environment.NewLine);
                }
            }

        }

        public static void DisplayMSPeakResults(List<Backend.DTO.MSPeakResult> list)
        {
            var sb = new StringBuilder();
            sb.Append("peakID\tframe\tscan\tChromID\tmz\tintens\tfwhm\n");
            foreach (var peak in list)
            {
                sb.Append(peak.PeakID);
                sb.Append("\t");
                sb.Append(peak.FrameNum);
                sb.Append("\t");
                sb.Append(peak.Scan_num);
                sb.Append("\t");
                sb.Append(peak.ChromID);
                sb.Append("\t");
                sb.Append(peak.MSPeak.XValue.ToString("0.00000"));
                sb.Append("\t");
                sb.Append(peak.MSPeak.Height.ToString("0"));
                sb.Append("\t");
                sb.Append(peak.MSPeak.Width.ToString("0.0000"));
                sb.Append(Environment.NewLine);


            }

            Console.WriteLine(sb.ToString());

        }

        public static XYData LoadXYDataFromFile(string testChromatogramDataFile)
        {
            var tempRun = new MSScanFromTextFileRun(testChromatogramDataFile);
            var xyData = tempRun.GetMassSpectrum(new ScanSet(0));

            return xyData;

        }
    }
}
