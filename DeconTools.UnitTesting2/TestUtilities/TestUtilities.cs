using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Runs;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace DeconTools.UnitTesting2
{
    public class TestUtilities
    {
        private static string xcaliburTestfile = FileRefs.TestFileBasePath + "\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        private static string uimfTestFile = FileRefs.TestFileBasePath + "\\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf";

        public static void GetXYValuesToStringBuilder(StringBuilder sb, double[] xvals, double[] yvals)
        {
            if (sb == null) return;
            for (int i = 0; i < xvals.Length; i++)
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

            for (int i = 0; i < resultList1.Count; i++)
            {
                bool resultIsTheSame = (resultList1[i].IsotopicProfile.ChargeState == resultList2[i].IsotopicProfile.ChargeState &&
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
            int counter = 0;

            foreach (MSPeak peak in profile.Peaklist)
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

            run.ScanSetCollection .Create(run, 6000, 6020, 1, 1);
            
            Task msgen = new GenericMSGenerator();
            Task peakDet = new DeconToolsPeakDetector();
            Task decon = new HornDeconvolutor();



            foreach (ScanSet scanset in run.ScanSetCollection.ScanSetList)
            {
                run.CurrentScanSet = scanset;
                msgen.Execute(run.ResultCollection);
                peakDet.Execute(run.ResultCollection);
                decon.Execute(run.ResultCollection);

            }

            return run.ResultCollection.ResultList;

        }

        public static void DisplayPeaks(List<Peak> peakList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("--------- Peaks found -----------------\n");
            sb.Append("x_value\ty_value\twidth\n");


            foreach (Peak peak in peakList)
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
            double[] xvals = resultCollection.Run.XYData.Xvalues;
            double[] yvals = resultCollection.Run.XYData.Yvalues;

            StringBuilder sb = new StringBuilder();
            sb.Append("--------- XYData found in Run " + resultCollection.Run.Filename + "-----------------\n");
            for (int i = 0; i < xvals.Length; i++)
            {
                sb.Append(xvals[i]);
                sb.Append("\t");
                sb.Append(yvals[i]);
                sb.Append("\n");
            }
            sb.Append("--------------------------- end ---------------------------------------\n");

            Console.Write(sb.ToString());
        }

        public static void DisplayXYValues(XYData xydata)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("--------- XYData -----------------\n");
            for (int i = 0; i < xydata.Xvalues.Length; i++)
            {
                sb.Append(xydata.Xvalues[i]);
                sb.Append("\t");
                sb.Append(xydata.Yvalues[i]);
                sb.Append("\n");
            }
            sb.Append("--------------------------- end ---------------------------------------\n");

            Console.Write(sb.ToString());
        }


        public static void DisplayIsotopicProfileData(IsotopicProfile profile)
        {
            StringBuilder sb = new StringBuilder();
            int counter = 0;

            foreach (MSPeak peak in profile.Peaklist)
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

            Console.Write(sb.ToString());
        }

        public static void DisplayMSFeatures(List<IsosResult> resultList)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("------------MSFeatures ---------------\n");
            sb.Append("id\tscanNum\tmonomass\tmz\tz\tintens\tscore\tinterferenceScore\n");
            foreach (var item in resultList)
            {
                sb.Append(item.MSFeatureID);
                sb.Append("\t");
                sb.Append(item.ScanSet.PrimaryScanNumber);
                sb.Append("\t");
                sb.Append(item.IsotopicProfile.MonoIsotopicMass.ToString("0.0000"));
                sb.Append("\t");
                sb.Append(item.IsotopicProfile.MonoPeakMZ.ToString("0.00000"));
                sb.Append("\t");
                sb.Append(item.IsotopicProfile.ChargeState);
                sb.Append("\t");
                sb.Append(item.IntensityAggregate.ToString("0"));
                sb.Append("\t");
                sb.Append(item.IsotopicProfile.Score.ToString("0.000"));
                sb.Append("\t");
                sb.Append(item.InterferenceScore.ToString("0.000"));
                sb.Append("\t");
                sb.Append(item.Flags.Count == 0 ? "" : "FLAGGED");

                sb.Append("\n");

            }

            Console.Write(sb.ToString());

        }


        public static string DisplayRunInformation(Run run)
        {
            StringBuilder sb = new StringBuilder();
            string delim = Environment.NewLine;

            sb.Append("Dataset name = " + run.DatasetName);
            sb.Append(delim);
            sb.Append("Datatset path = " + run.DataSetPath);
            sb.Append(delim);
            sb.Append("MinScan = " + run.MinLCScan);
            sb.Append(delim);
            sb.Append("MaxScan = " + run.MaxLCScan);
            sb.Append(delim);
            sb.Append("FileType = " + run.MSFileType);
            sb.Append(delim);
            sb.Append("MassIsAligned = " + run.MassIsAligned);
            sb.Append(delim);
            sb.Append("NETIsAligned = " + run.NETIsAligned);

            return sb.ToString();


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
            target.MZTheor = target.MonoMassTheor/target.ChargeState + Globals.PROTON_MASS;


            JoshTheorFeatureGenerator theorFeatureGenerator = new JoshTheorFeatureGenerator();
            target.TheorIsotopicProfile = theorFeatureGenerator.GenerateTheorProfile(target.EmpiricalFormula, target.ChargeState);

            return target;


        }


        public static PeptideTarget GetMassTagStandard(int standardNum)
        {
            PeptideTarget mt = new PeptideTarget();
            mt.ID = 86963986;
            mt.MonoIsotopicMass = 1516.791851;
            mt.Code = "AAKEGISCEIIDLR";

            mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();
            mt.NormalizedElutionTime = 0.2284955f;
            mt.ChargeState = 2;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;

            return mt;


        }

        public static List<PeptideTarget> CreateTestMassTagList()
        {
            List<PeptideTarget> mtList = new List<PeptideTarget>();

            PeptideTarget mt = new PeptideTarget();
            mt.ID = 24769;
            mt.MonoIsotopicMass = 2086.0595;
            mt.ChargeState = 2;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;
            mt.Code = "DFNEALVHQVVVAYAANAR";
            mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();
            mtList.Add(mt);
            return mtList;
        }


        public static List<PeptideTarget> CreateN14N15TestMassTagList()
        {
            List<PeptideTarget> mtList = new List<PeptideTarget>();

            PeptideTarget mt = new PeptideTarget();
            mt.ID = 23085473;
            mt.NormalizedElutionTime = 0.3807834F;
            mt.MonoIsotopicMass = 2538.33284203802;
            mt.ChargeState = 3;
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
            StringBuilder sb = new StringBuilder();
            sb.Append("scan\tMSLevel\n");

            for (int i = run.MinLCScan; i <= run.MaxLCScan; i++)
            {
                sb.Append(i);
                sb.Append("\t");
                sb.Append(run.GetMSLevel(i));
                sb.Append("\n");

            }

            Console.WriteLine(sb.ToString());

        }




        public static void DisplayScanSetData(List<ScanSet> scanSetlist)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("--------------- ScanSetCollection details ---------------------- \n");

            foreach (var scan in scanSetlist)
            {
                sb.Append(scan.PrimaryScanNumber);
                sb.Append("\t{");
                for (int i = 0; i < scan.IndexValues.Count; i++)
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

            Task msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);


            DeconToolsPeakDetector peakDet = new DeconToolsPeakDetector();
            peakDet.PeakToBackgroundRatio = 1.3;
            peakDet.SignalToNoiseThreshold = 2;
            peakDet.IsDataThresholded = true;

            run.CurrentScanSet = scanSet;

            msgen.Execute(run.ResultCollection);

            peakDet.Execute(run.ResultCollection);

            return run.PeakList;
        }

        public static Run GetStandardUIMFRun()
        {
            return new UIMFRun(uimfTestFile);

        }

        public static void DisplayInfoForProcess(System.Diagnostics.Process currentProcess)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Process Name =\t" + currentProcess.ProcessName);
            sb.Append(Environment.NewLine);
            sb.Append("Private bytes =\t" + String.Format("{0:N0}", currentProcess.PrivateMemorySize64));
            sb.Append(Environment.NewLine);

            Console.Write(sb.ToString());


        }

        public static List<PeptideTarget> CreateO16O18TestMassTagList1()
        {
            List<PeptideTarget> mtList = new List<PeptideTarget>();

            PeptideTarget mt = new PeptideTarget();
            mt.ID = 1142;
            mt.MonoIsotopicMass = 921.4807035;
            mt.ChargeState = 2;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;
            mt.Code = "AEFVEVTK";
            mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();
            mtList.Add(mt);
            return mtList;
        }


        public static void WriteToFile(XYData xydata, string outputFile)
        {
            using (StreamWriter sw = new StreamWriter(outputFile))
            {
                sw.WriteLine("mz\tintensity");
                for (int i = 0; i < xydata.Xvalues.Length; i++)
                {
                    sw.Write(xydata.Xvalues[i]);
                    sw.Write("\t");
                    sw.Write(xydata.Yvalues[i]);
                    sw.Write(Environment.NewLine);
                }

            }


        }


        public static void DisplayMSPeakResults(List<DeconTools.Backend.DTO.MSPeakResult> list)
        {
            StringBuilder sb = new StringBuilder();
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
            XYData xydata=   tempRun.GetMassSpectrum(new ScanSet(0));

            return xydata;

        }
    }
}
