﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend;

namespace DeconTools.UnitTesting
{
    public class TestUtilities
    {
        private static string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";
        private static string uimfTestFile = "..\\..\\TestFiles\\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf";

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
                    resultList1[i].IsotopicProfile.IntensityAggregate == resultList2[i].IsotopicProfile.IntensityAggregate &&
                    //resultList1[i].IsotopicProfile.MonoIsotopicMass == resultList2[i].IsotopicProfile.MonoIsotopicMass &&
                    resultList1[i].IsotopicProfile.MonoPeakMZ == resultList2[i].IsotopicProfile.MonoPeakMZ &&
                    resultList1[i].IsotopicProfile.MonoPlusTwoAbundance == resultList2[i].IsotopicProfile.MonoPlusTwoAbundance &&
                    //resultList1[i].IsotopicProfile.MostAbundantIsotopeMass == resultList2[i].IsotopicProfile.MostAbundantIsotopeMass &&
                    resultList1[i].IsotopicProfile.Original_Total_isotopic_abundance == resultList2[i].IsotopicProfile.Original_Total_isotopic_abundance &&
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

        public static void ReportIsotopicProfileData(StringBuilder sb, IsotopicProfile profile)
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
                sb.Append(peak.SN);
                sb.Append("\n");

                counter++;
            }
        }

        public  List<IsosResult> CreateThrashIsosResults1()
        {
            Run run = new XCaliburRun(xcaliburTestfile);

            DeconTools.Backend.Utilities.ScanSetCollectionCreator ssc = new DeconTools.Backend.Utilities.ScanSetCollectionCreator(run, 6000, 6020, 1, 1);
            ssc.Create();

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

        public static void DisplayPeaks(List<IPeak> peakList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("--------- Peaks found -----------------\n");
            sb.Append("x_value\ty_value\twidth\n");

            
            foreach (IPeak peak in peakList)
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

        public  static void DisplayXYValues(XYData xydata)
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
                sb.Append(peak.SN);
                sb.Append("\n");

                counter++;
            }

            Console.Write(sb.ToString());
        }

        public static void DisplayMSFeatures(List<IsosResult> resultList)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("------------MSFeatures ---------------\n");
            sb.Append("id\tscanNum\tmz\tz\tintens\tscore\n");
            foreach (var item in resultList)
            {
                sb.Append(item.MSFeatureID);
                sb.Append("\t");
                sb.Append(item.ScanSet.PrimaryScanNumber);
                sb.Append("\t");
               
                sb.Append(item.IsotopicProfile.MonoPeakMZ.ToString("0.00000"));
                sb.Append("\t");
                sb.Append(item.IsotopicProfile.ChargeState);
                sb.Append("\t");
                sb.Append(item.IsotopicProfile.IntensityAggregate);
                sb.Append("\t");
                sb.Append(item.IsotopicProfile.Score.ToString("0.000"));
                sb.Append("\n");
                
            }

            Console.Write(sb.ToString());

        }



        public static MassTag GetMassTagStandard(int standardNum)
        {
            MassTag mt = new MassTag();
            mt.ID = 86963986;
            mt.MonoIsotopicMass = 1516.791851;
            mt.PeptideSequence = "AAKEGISCEIIDLR";
            mt.NETVal = 0.2284955f;
            mt.CreatePeptideObject();
            mt.ChargeState = 2;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;

            return mt;


        }

        public static List<MassTag> CreateTestMassTagList()
        {
            List<MassTag> mtList = new List<MassTag>();

            MassTag mt = new MassTag();
            mt.ID = 24769;
            mt.MonoIsotopicMass = 2086.0595;
            mt.ChargeState = 2;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;
            mt.PeptideSequence = "DFNEALVHQVVVAYAANAR";
            mt.CreatePeptideObject();

            mtList.Add(mt);
            return mtList;
        }


        public static List<MassTag> CreateN14N15TestMassTagList()
        {
            List<MassTag> mtList = new List<MassTag>();

            MassTag mt = new MassTag();
            mt.ID = 23085473;
            mt.NETVal = 0.3807834F;
            mt.MonoIsotopicMass = 2538.33284203802;
            mt.ChargeState = 3;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;
            mt.PeptideSequence = "AIHQPAPTFAEQSTTSEILVTGIK";
            mt.CreatePeptideObject();

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

            for (int i = run.MinScan; i <= run.MaxScan; i++)
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

        public static List<IPeak> GeneratePeakList(ScanSet scanSet)
        {
            Run run = new XCaliburRun(xcaliburTestfile);

            MSGeneratorFactory fact = new MSGeneratorFactory();
            Task msgen = fact.CreateMSGenerator(run.MSFileType);

            DeconToolsPeakDetector peakDet = new DeconToolsPeakDetector();
            peakDet.PeakBackgroundRatio = 1.3;
            peakDet.SigNoiseThreshold = 2;
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

        public static List<MassTag> CreateO16O18TestMassTagList1()
        {
            List<MassTag> mtList = new List<MassTag>();

            MassTag mt = new MassTag();
            mt.ID = 1142;
            mt.MonoIsotopicMass = 921.4807035;
            mt.ChargeState = 2;
            mt.MZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS;
            mt.PeptideSequence = "AEFVEVTK";
            mt.CreatePeptideObject();

            mtList.Add(mt);
            return mtList;
        }

        internal static XYData LoadXYDataFromFile(string testChromatogramDataFile)
        {
            throw new NotImplementedException();
        }
    }
}
