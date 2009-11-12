using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.Runs;
using DeconTools.Backend.ProcessingTasks;

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
                    resultList1[i].IsotopicProfile.getMonoPeak().MZ == resultList2[i].IsotopicProfile.getMonoPeak().MZ &&
                    resultList1[i].IsotopicProfile.getMonoPeak().Intensity == resultList2[i].IsotopicProfile.getMonoPeak().Intensity);

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
                sb.Append(peak.MZ);
                sb.Append("\t");
                sb.Append(peak.Intensity);
                sb.Append("\t");
                sb.Append(peak.FWHM);
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
    }
}
