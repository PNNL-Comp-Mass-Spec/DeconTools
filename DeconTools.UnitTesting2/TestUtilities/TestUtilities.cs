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
        private static readonly string thermoTestFile = FileRefs.TestFileBasePath + "\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        private static readonly string uimfTestFile = FileRefs.TestFileBasePath + "\\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf";

        public static bool AreIsosResultsTheSame(List<IsosResult> resultList1, List<IsosResult> resultList2)
        {
            if (resultList1.Count != resultList2.Count)
            {
                return false;     //counts are different
            }

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

        public List<IsosResult> CreateThrashIsosResults1()
        {
            Run run = new XCaliburRun2(thermoTestFile);

            run.ScanSetCollection.Create(run, 6000, 6020, 1, 1);

            Task generator = new GenericMSGenerator();
            Task peakDet = new DeconToolsPeakDetectorV2();
            Task decon = new HornDeconvolutor();

            foreach (var scanSet in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scanSet;
                generator.Execute(run.ResultCollection);
                peakDet.Execute(run.ResultCollection);
                decon.Execute(run.ResultCollection);
            }

            return run.ResultCollection.ResultList;
        }

        public static void DisplayPeaks(List<Peak> peakList)
        {
            Console.WriteLine("--------- Peaks found -----------------");

            Console.WriteLine("{0,-10} {1,-10} {2,-10}", "X", "Height", "Width");

            foreach (var peak in peakList)
            {
                Console.WriteLine("{0,-10} {1,-10} {2,-10}", peak.XValue, peak.Height, peak.Width);
            }

            Console.WriteLine("--------------------------- end ---------------------------------------");
        }

        public static void DisplayXYValues(ResultCollection resultCollection)
        {
            DisplayXYValues(resultCollection, double.MinValue, double.MaxValue);
        }

        public static void DisplayXYValues(ResultCollection resultCollection, double lowerX, double upperX)
        {
            var xVals = resultCollection.Run.XYData.Xvalues;
            var yVals = resultCollection.Run.XYData.Yvalues;

            Console.WriteLine("--------- XYData found in Run " + resultCollection.Run.DatasetFileOrDirectoryPath + "-----------------");

            Console.WriteLine("{0,-10} {1,-10}", "X", "Y");

            for (var i = 0; i < xVals.Length; i++)
            {
                if (xVals[i] >= lowerX && xVals[i] <= upperX)
                {
                    Console.WriteLine("{0,-10:G0} {1,-10:G2}", xVals[i], yVals[i]);
                }
            }
            Console.WriteLine("--------------------------- end ---------------------------------------");
        }

        public static void DisplayXYValues(XYData xyData)
        {
            DisplayXYValues(xyData, double.MinValue, double.MaxValue);
        }

        public static void DisplayXYValues(XYData xyData, double lowerX, double upperX)
        {
            Console.WriteLine("--------- XYData -----------------");

            Console.WriteLine("{0,-10} {1,-10}", "X", "Y");

            for (var i = 0; i < xyData.Xvalues.Length; i++)
            {
                if (xyData.Xvalues[i] >= lowerX && xyData.Xvalues[i] <= upperX)
                {
                    Console.WriteLine("{0,-10:F3} {1,-10:G2}", xyData.Xvalues[i], xyData.Yvalues[i]);
                }
            }
            Console.WriteLine("--------------------------- end ---------------------------------------");
        }

        public static void DisplayIsotopicProfileData(IsotopicProfile profile)
        {
            var counter = 0;

            Console.WriteLine("{0,-8} {1,-9} {2,-10} {3,-10} {4,-10}", "Index", "XValue", "Height", "Width", "S/N");

            foreach (var peak in profile.Peaklist)
            {
                Console.WriteLine("{0,-8} {1,-9:G3} {2,-10:G3} {3,-10} {4,-10}", counter, peak.XValue, peak.Height, peak.Width, peak.SignalToNoise);
                counter++;
            }
            Console.WriteLine();
        }

        public static void DisplayIsotopicProfileDataFloatX(IsotopicProfile profile)
        {
            var counter = 0;

            Console.WriteLine("{0,-8} {1,-9} {2,-10} {3,-10} {4,-10}", "Index", "XValue", "Height", "Width", "S/N");

            foreach (var peak in profile.Peaklist)
            {
                Console.WriteLine("{0,-8} {1,-9:F3} {2,-10:G3} {3,-10} {4,-10}", counter, peak.XValue, peak.Height, peak.Width, peak.SignalToNoise);
                counter++;
            }

            Console.WriteLine();
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
            var mt = new PeptideTarget
            {
                ID = 86963986,
                MonoIsotopicMass = 1516.791851,
                Code = "AAKEGISCEIIDLR"
            };

            mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();
            mt.NormalizedElutionTime = 0.2284955f;
            mt.ChargeState = 2;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;

            return mt;
        }

        public static List<PeptideTarget> CreateTestMassTagList()
        {
            var mtList = new List<PeptideTarget>();

            var mt = new PeptideTarget
            {
                ID = 24769,
                MonoIsotopicMass = 2086.0595,
                ChargeState = 2
            };

            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;
            mt.Code = "DFNEALVHQVVVAYAANAR";
            mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();
            mtList.Add(mt);
            return mtList;
        }

        public static List<PeptideTarget> CreateN14N15TestMassTagList()
        {
            var mtList = new List<PeptideTarget>();

            var mt = new PeptideTarget
            {
                ID = 23085473,
                NormalizedElutionTime = 0.3807834F,
                MonoIsotopicMass = 2538.33284203802,
                ChargeState = 3
            };

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
            Run run = new XCaliburRun2(thermoTestFile);

            Task generator = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var peakDet = new DeconToolsPeakDetectorV2
            {
                PeakToBackgroundRatio = 1.3,
                SignalToNoiseThreshold = 2,
                IsDataThresholded = true
            };

            run.CurrentScanSet = scanSet;

            generator.Execute(run.ResultCollection);

            peakDet.Execute(run.ResultCollection);

            return run.PeakList;
        }

        public static Run GetStandardUIMFRun()
        {
            return new UIMFRun(uimfTestFile);
        }

        public static List<PeptideTarget> CreateO16O18TestMassTagList1()
        {
            var mtList = new List<PeptideTarget>();

            var mt = new PeptideTarget
            {
                ID = 1142,
                MonoIsotopicMass = 921.4807035,
                ChargeState = 2
            };

            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;
            mt.Code = "AEFVEVTK";
            mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();
            mtList.Add(mt);
            return mtList;
        }

        public static void WriteToFile(XYData xyData, string outputFile)
        {
            using (var writer = new StreamWriter(new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
            {
                writer.WriteLine("{0}\t{1}", "mz", "intensity");

                for (var i = 0; i < xyData.Xvalues.Length; i++)
                {
                    writer.WriteLine("{0}\t{1}", xyData.Xvalues[i], xyData.Yvalues[i]);
                }
            }
        }

        public static void DisplayMSPeakResults(List<Backend.DTO.MSPeakResult> list)
        {
            Console.WriteLine("{0,-10} {1,-8} {2,-8} {3,-8} {4,-12} {5,-12} {6,-10}",
                "peakID", "frame", "scan", "ChromID", "mz", "intensity", "fwhm");

            foreach (var peak in list)
            {
                Console.WriteLine("{0,-10} {1,-8} {2,-8:G0} {3,-8} {4,-12:F5} {5,-12:N0} {6,-10:F4}",
                    peak.PeakID,
                    peak.FrameNum,
                    peak.Scan_num,
                    peak.ChromID,
                    peak.MSPeak.XValue,
                    peak.MSPeak.Height,
                    peak.MSPeak.Width
                );
            }
        }

        public static XYData LoadXYDataFromFile(string testChromatogramDataFile)
        {
            var tempRun = new MSScanFromTextFileRun(testChromatogramDataFile);
            var xyData = tempRun.GetMassSpectrum(new ScanSet(0));

            return xyData;
        }
    }
}
