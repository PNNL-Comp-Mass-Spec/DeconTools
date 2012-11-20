using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;

namespace DeconTools.Backend.DTO
{
    public class IsosResultUtilities
    {
        public enum enumLinqOperator         //i'm sure there's a better way!
        {
            greaterThan,
            lessThan,
            EqualTo
        }



        List<IsosResult> results;

        public List<IsosResult> Results
        {
            get { return results; }
            set { results = value; }
        }

        public IsosResultUtilities(List<IsosResult> results)
        {

            this.results = results;
        }


        public IsosResultUtilities()
        {

        }

        public void LoadResults(string isosTextFile, Globals.MSFileType fileType)
        {
            bool fileExists = false;
            try
            {
                fileExists = System.IO.File.Exists(isosTextFile);

            }
            catch (Exception)
            {

            }
            Check.Require(fileExists, "IsosResultUtilities cannot open input file");

            List<IsosResult> isosResults = new List<IsosResult>();

            IsosImporter importer = new IsosImporter(isosTextFile, fileType);
            isosResults= importer.Import();

            this.results = isosResults;
        }

        public void LoadResults(string isosTextFile, Globals.MSFileType fileType, int minVal, int maxVal)
        {
            bool fileExists = false;
            try
            {
                fileExists = System.IO.File.Exists(isosTextFile);

            }
            catch (Exception)
            {

            }
            Check.Require(fileExists, "IsosResultUtilities cannot open input file");

            List<IsosResult> isosResults = new List<IsosResult>();

            IsosImporter importer = new IsosImporter(isosTextFile, fileType, minVal, maxVal);
            isosResults = importer.Import();

            this.results = isosResults;
        }

        public void LoadResults(string uimfIsos1, Globals.MSFileType mSFileType, int frameNum)
        {
            Check.Require(mSFileType == Globals.MSFileType.PNNL_UIMF, "This option only works for UIMF files");

        }


        public List<IsosResult> getUIMFResults(string uimfInputFile, int minFrame, int maxFrame)
        {
            IsosResultUtilities uimfisoUtil = new IsosResultUtilities();
            uimfisoUtil.LoadResults(uimfInputFile, DeconTools.Backend.Globals.MSFileType.PNNL_UIMF,minFrame,maxFrame);

           // List<IsosResult> filteredIsos = new List<IsosResult>();



            //foreach (IsosResult result in uimfisoUtil.Results)
            //{
            //    UIMFIsosResult uimfResult = (UIMFIsosResult)result;
            //    if (uimfResult.FrameSet.PrimaryFrame >= minFrame && uimfResult.FrameSet.PrimaryFrame <= maxFrame)
            //    {
            //        filteredIsos.Add(result);
            //    }
            //}
            return uimfisoUtil.Results;
        }

        public List<UIMFIsosResult> convertToUIMFIsosResults(List<IsosResult> inputList, int minFrame, int maxFrame)
        {
            List<UIMFIsosResult> returnedResults = new List<UIMFIsosResult>();

            foreach (IsosResult result in inputList)
            {
                UIMFIsosResult uimfResult = (UIMFIsosResult)result;
                if (uimfResult.ScanSet.PrimaryScanNumber >= minFrame && uimfResult.ScanSet.PrimaryScanNumber <= maxFrame)
                {
                    returnedResults.Add(uimfResult);
                }
            }
            return returnedResults;
        }

        public List<IsosResult> getIMFResults(string imfInputFolder, int minFrame, int maxFrame)
        {
            List<IsosResult> returnedResults = new List<IsosResult>();

            System.IO.FileInfo[] files = null;

            System.IO.DirectoryInfo imfFolder = new DirectoryInfo(imfInputFolder);
            files = imfFolder.GetFiles("*_isos.csv");

            foreach (FileInfo file in files)
            {
                int currentFrame = getFrameFromFilename(file.Name);
                if (currentFrame >= minFrame && currentFrame <= maxFrame)
                {
                    IsosResultUtilities isoutil = new IsosResultUtilities();

                    string isosFileName = file.FullName;
                    //string isosFileName = Path.GetDirectoryName(file.FullName) + Path.DirectorySeparatorChar+ Path.GetFileNameWithoutExtension(file.FullName) + "_isos.csv";
                    isoutil.LoadResults(isosFileName, DeconTools.Backend.Globals.MSFileType.PNNL_IMS);

                    List<IsosResult> isosResultsForFrame = convertIMFResultsToUIMFResults(currentFrame, isoutil.Results);

                    returnedResults.AddRange(isosResultsForFrame);

                }


            }




            return returnedResults;
        }

        private int getFrameFromFilename(string imfFileName)
        {
            //35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.Accum_500.imf
            Match match = Regex.Match(imfFileName, @".*Accum_(?<frameNum>\d+)");

            if (match.Success)
            {
                return Convert.ToInt32(match.Groups["frameNum"].Value);
            }
            else
            {
                throw new Exception("Couldn't find frame number in IMF filename");
            }

        }


        public List<IsosResult> convertIMFResultsToUIMFResults(int currentFrame, List<IsosResult> list)
        {
            List<IsosResult> returnedResults = new List<IsosResult>();

            foreach (IsosResult result in list)
            {
                UIMFIsosResult uimfResult = new UIMFIsosResult();
                uimfResult.ScanSet = new LCScanSetIMS(currentFrame);
                uimfResult.IsotopicProfile = result.IsotopicProfile;
                uimfResult.Run = result.Run;
                uimfResult.ScanSet = result.ScanSet;
                returnedResults.Add(uimfResult);
            }

            return returnedResults;
        }


        public List<IsosResult> getIsosResults(string isosInputFile, DeconTools.Backend.Globals.MSFileType filetype, int minScan, int maxScan)
        {
            IsosResultUtilities isoutil = new IsosResultUtilities();
            isoutil.LoadResults(isosInputFile, filetype);
            return isoutil.Results.Where(p => p.ScanSet.PrimaryScanNumber >= minScan && p.ScanSet.PrimaryScanNumber <= maxScan).ToList();

        }


        public List<IsosResult> getIsosResults(string isosInputFile, DeconTools.Backend.Globals.MSFileType filetype)
        {
            IsosResultUtilities isoutil = new IsosResultUtilities();
            isoutil.LoadResults(isosInputFile, filetype);
            return isoutil.Results;

        }

        public void replaceAbundanceWithMonoAbundance(List<IsosResult> isosResults)
        {
            foreach (IsosResult result in isosResults)
            {
                result.IsotopicProfile.IntensityAggregate = result.IsotopicProfile.GetMonoAbundance();
            }
        }


        //public void exportIsosResults

        public static List<IsosResult>GetIsosResultsByScan(List<IsosResult>inputList, int scanNum)
        {
            List<IsosResult> results = new List<IsosResult>();

            var query = from n in inputList select n.ScanSet.PrimaryScanNumber;

            int[] scanNums = query.ToArray();


            int indexOfScanNum = MathUtils.BinarySearch(scanNums, scanNum, 0, scanNums.Length - 1);
            if (indexOfScanNum == -1) return results;

            //the found index might point to a isos result line that is mid way through the scan list.  So need to find the starting index
            int currentIdx = indexOfScanNum;
            if (indexOfScanNum > 0)
            {

                while (inputList[currentIdx].ScanSet.PrimaryScanNumber == scanNum)
                {
                    currentIdx--;
                }
                currentIdx++;

            }


            while (inputList[currentIdx].ScanSet.PrimaryScanNumber == scanNum)
            {
                results.Add(inputList[currentIdx]);
                currentIdx++;
            }



            return results;

        }


        public static List<IsosResult> GetIntersection(List<IsosResult> list1, List<IsosResult> list2)
        {
            List<IsosResult> intersectedResults = new List<IsosResult>();

            double tolerance = 0.005;

            for (int i = 0; i < list1.Count; i++)
            {
                for (int i2 = 0; i2 < list2.Count ; i2++)
                {
                    if (list1[i].ScanSet == list2[i2].ScanSet &&
                        list1[i].IsotopicProfile.ChargeState == list2[i2].IsotopicProfile.ChargeState &&
                        Math.Abs(list1[i].IsotopicProfile.MonoIsotopicMass - list2[i2].IsotopicProfile.MonoIsotopicMass) < tolerance)
                    {
                        intersectedResults.Add(list1[i]);
                        break;
                    }

                }
                
            }




            return intersectedResults;

        }



        public string buildStatsForSingleIsosResultSet(List<IsosResult> resultList)
        {
            StringBuilder sb = new StringBuilder();

            IsosResultStats allResults = new IsosResultStats(resultList);

            IsosResultStats cs1results = new IsosResultStats(IsosResultUtilities.getIsosResultsByChargeState(resultList, 1, IsosResultUtilities.enumLinqOperator.EqualTo));
            IsosResultStats cs2results = new IsosResultStats(IsosResultUtilities.getIsosResultsByChargeState(resultList, 2, IsosResultUtilities.enumLinqOperator.EqualTo));
            IsosResultStats cs3results = new IsosResultStats(IsosResultUtilities.getIsosResultsByChargeState(resultList, 3, IsosResultUtilities.enumLinqOperator.EqualTo));
            IsosResultStats cs4results = new IsosResultStats(IsosResultUtilities.getIsosResultsByChargeState(resultList, 4, IsosResultUtilities.enumLinqOperator.EqualTo));
            IsosResultStats greaterThanCS4results = new IsosResultStats(IsosResultUtilities.getIsosResultsByChargeState(resultList, 4, IsosResultUtilities.enumLinqOperator.greaterThan));

            List<IsosResultStats> stats = new List<IsosResultStats>();
            stats.Add(allResults);
            stats.Add(cs1results);
            stats.Add(cs2results);
            stats.Add(cs3results);
            stats.Add(cs4results);
            stats.Add(greaterThanCS4results);


            allResults.Description = "all";
            cs1results.Description = "1";
            cs2results.Description = "2";
            cs3results.Description = "3";
            cs4results.Description = "4";
            greaterThanCS4results.Description = ">4";

            //build header
            sb.Append("Z");
            sb.Append("\t");
            sb.Append("NumIsos");
            sb.Append("\t");
            sb.Append("AvgFit");
            sb.Append("\t");
            sb.Append("StdDevFit");
            sb.Append(Environment.NewLine);

            foreach (IsosResultStats statItem in stats)
            {

                statItem.FitAverage = IsosResultUtilities.getAverageScore(statItem.Results);
                statItem.FitStdDev = IsosResultUtilities.getStdDevScore(statItem.Results);
                statItem.Count = IsosResultUtilities.getCount(statItem.Results);

                sb.Append(statItem.Description);
                sb.Append("\t");
                sb.Append(statItem.Count);
                sb.Append("\t");
                sb.Append(statItem.FitAverage.ToString("0.000"));
                sb.Append("\t");
                sb.Append(statItem.FitStdDev.ToString("0.000"));
                sb.Append(Environment.NewLine);

            }

            return sb.ToString();






        }





        public static List<IsosResult> getIsosResultsByChargeState(List<IsosResult> inputList, int chargeState, enumLinqOperator chargeOperator)
        {
            List<IsosResult> results = new List<IsosResult>();

            switch (chargeOperator)
            {
                case enumLinqOperator.greaterThan:
                    var query = from p in inputList
                                where p.IsotopicProfile.ChargeState > chargeState
                                select p;
                    results = query.ToList();
                    break;
                case enumLinqOperator.lessThan:
                    query = from p in inputList
                            where p.IsotopicProfile.ChargeState < chargeState
                            select p;
                    results = query.ToList();
                    break;
                case enumLinqOperator.EqualTo:
                    query = from p in inputList
                            where p.IsotopicProfile.ChargeState == chargeState
                            select p;
                    results = query.ToList();
                    break;
                default:
                    break;
            }



            return results;
        }

        public static double getAverageScore(List<IsosResult> inputList)
        {
            Check.Require(inputList != null, "IsosResult list is null");
            if (inputList.Count == 0) return double.NaN;

            return inputList.Average(p => p.IsotopicProfile.Score);
            //return inputList.Where(p => p.IsotopicProfile.Score >= 0 && p.IsotopicProfile.Score <= 0.3).Average(p => p.IsotopicProfile.Score);




        }

        public static int getCount(List<IsosResult> inputList)
        {
            Check.Require(inputList != null, "IsosResult list is null");
            if (inputList.Count == 0) return -1;

            return inputList.Count;
            //return inputList.Where(p => p.IsotopicProfile.Score >= 0 && p.IsotopicProfile.Score <= 0.3).Count();

        }


        public static double getStdDevScore(List<IsosResult> inputList)
        {
            Check.Require(inputList != null, "IsosResult list is null");

            if (inputList.Count == 0) return double.NaN;

            var query = from p in inputList
                        //where p.IsotopicProfile.Score >= 0 && p.IsotopicProfile.Score <= 0.3
                        select p.IsotopicProfile.Score;




            double[] scoreArray = query.ToArray();


            //List<double> vals = new List<double>();
            //foreach (IsosResult result in inputList)
            //{
            //    vals.Add(result.IsotopicProfile.Score);

            //}


            return MathUtils.GetStDev(scoreArray);

        }


        public static void getSummaryStats1(List<IsosResult> inputList, StringBuilder sb)
        {
            if (sb == null) sb = new StringBuilder();

            sb.Append("Total isoResults = \t" + inputList.Count);
            sb.Append("\n");
            sb.Append("Total isoResults; fit > 0.15; \t" + inputList.Count(p => p.IsotopicProfile.Score > 0.15));
            sb.Append("\n");
            sb.Append("Total isoResults; fit 0.10 - 0.15; \t" + inputList.Count(p => p.IsotopicProfile.Score > 0.10 && p.IsotopicProfile.Score <= 0.15));
            sb.Append("\n");
            sb.Append("Total isoResults; fit 0.05 - 0.10; \t" + inputList.Count(p => p.IsotopicProfile.Score > 0.05 && p.IsotopicProfile.Score <= 0.10));
            sb.Append("\n");
            sb.Append("Total isoResults; fit 0.01 - 0.05; \t" + inputList.Count(p => p.IsotopicProfile.Score > 0.01 && p.IsotopicProfile.Score <= 0.05));
            sb.Append("\n");
            sb.Append("Total isoResults; fit < 0.01 ; \t" + inputList.Count(p => p.IsotopicProfile.Score <= 0.01));
            sb.Append("\n");

            sb.Append("\n");
            sb.Append("Average fit = \t" + inputList.Average(p => p.IsotopicProfile.Score));
            sb.Append("\n");
            sb.Append("IsoResults charge state +1 = \t" + inputList.Count(p => p.IsotopicProfile.ChargeState == 1) + "\t" + inputList.Where(p => p.IsotopicProfile.ChargeState == 1).Average(n => n.IsotopicProfile.Score));
            sb.Append("\n");
            sb.Append("IsoResults charge state +2 = \t" + inputList.Count(p => p.IsotopicProfile.ChargeState == 2) + "\t" + inputList.Where(p => p.IsotopicProfile.ChargeState == 2).Average(n => n.IsotopicProfile.Score));
            sb.Append("\n");
            sb.Append("IsoResults charge state +3 = \t" + inputList.Count(p => p.IsotopicProfile.ChargeState == 3) + "\t" + inputList.Where(p => p.IsotopicProfile.ChargeState == 3).Average(n => n.IsotopicProfile.Score));
            sb.Append("\n");
            sb.Append("IsoResults charge state +4 = \t" + inputList.Count(p => p.IsotopicProfile.ChargeState == 4) + "\t" + inputList.Where(p => p.IsotopicProfile.ChargeState == 4).Average(n => n.IsotopicProfile.Score));
            sb.Append("\n");


            sb.Append("intensity 0 - 200000 \t" + inputList.Count(p => p.IsotopicProfile.IntensityAggregate < 200000));
            sb.Append("\n");
            sb.Append("intensity 200000 - 500000 \t" + inputList.Count(p => p.IsotopicProfile.IntensityAggregate >= 200000 && p.IsotopicProfile.IntensityAggregate < 5e5));
            sb.Append("\n");
            sb.Append("intensity 500000 - 1000000 \t" + inputList.Count(p => p.IsotopicProfile.IntensityAggregate >= 500000 && p.IsotopicProfile.IntensityAggregate < 1e6));
            sb.Append("\n");
            sb.Append("intensity 1000000 - 2000000 \t" + inputList.Count(p => p.IsotopicProfile.IntensityAggregate >= 1e6 && p.IsotopicProfile.IntensityAggregate < 2e6));
            sb.Append("\n");
            sb.Append("intensity 2000000 - 4000000 \t" + inputList.Count(p => p.IsotopicProfile.IntensityAggregate >= 2e6 && p.IsotopicProfile.IntensityAggregate < 4e6));
            sb.Append("\n");
            sb.Append("intensity 4000000 - 8000000 \t" + inputList.Count(p => p.IsotopicProfile.IntensityAggregate >= 4e6 && p.IsotopicProfile.IntensityAggregate < 8e6));
            sb.Append("\n");
            sb.Append("intensity > 8000000 \t" + inputList.Count(p => p.IsotopicProfile.IntensityAggregate > 8e6));
            sb.Append("\n");





        }



        public static void DisplayResults(StringBuilder sb, List<IsosResult> list)
        {
            foreach (IsosResult result in list)
            {
                sb.Append(result.ScanSet.PrimaryScanNumber);
                sb.Append("\t");

                if (result is UIMFIsosResult)
                {
                    var uimfResult = result as UIMFIsosResult;

                    sb.Append(  uimfResult.IMSScanSet.PrimaryScanNumber);
                    sb.Append("\t");
                }
               
                sb.Append(result.IsotopicProfile.ChargeState);
                sb.Append("\t");
                sb.Append(result.IsotopicProfile.MonoPeakMZ);
                sb.Append("\t");
                sb.Append(result.IsotopicProfile.IntensityAggregate);
                sb.Append("\t");
                sb.Append(result.IsotopicProfile.OriginalIntensity);
                sb.Append("\t");
                sb.Append(result.IsotopicProfile.IsSaturated);
                sb.Append("\n");


            }
        }
    }
}
