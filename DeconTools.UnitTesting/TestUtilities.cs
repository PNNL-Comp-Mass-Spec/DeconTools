using System;
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
        private string xcaliburTestfile = "..\\..\\TestFiles\\QC_Shew_08_04-pt5-2_11Jan09_Sphinx_08-11-18.RAW";

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

        public static void DisplayPeaks(ResultCollection resultCollection)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("--------- Peaks found in Run " + resultCollection.Run.Filename + "-----------------\n");
            
            foreach (IPeak peak in resultCollection.Run.PeakList)
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



        public static void DisplayXYValues(Run run)
        {
            
        }

     
    }
}
