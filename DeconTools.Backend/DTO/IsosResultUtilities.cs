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
            var fileExists = false;
            try
            {
                fileExists = File.Exists(isosTextFile);

            }
            catch (Exception)
            {

            }
            Check.Require(fileExists, "IsosResultUtilities cannot open input file");

            var isosResults = new List<IsosResult>();

            var importer = new IsosImporter(isosTextFile, fileType);
            isosResults= importer.Import();

            results = isosResults;
        }

        public void LoadResults(string isosTextFile, Globals.MSFileType fileType, int minVal, int maxVal)
        {
            var fileExists = false;
            try
            {
                fileExists = File.Exists(isosTextFile);

            }
            catch (Exception)
            {

            }
            Check.Require(fileExists, "IsosResultUtilities cannot open input file");

            var isosResults = new List<IsosResult>();

            var importer = new IsosImporter(isosTextFile, fileType, minVal, maxVal);
            isosResults = importer.Import();

            results = isosResults;
        }

        public void LoadResults(string uimfIsos1, Globals.MSFileType mSFileType, int frameNum)
        {
            Check.Require(mSFileType == Globals.MSFileType.PNNL_UIMF, "This option only works for UIMF files");

        }


        public void FilterAndOutputIsos(string inputIsosFileName, int colIndex, double minVal, double maxVal, string outputIsosFilename)
        {
            using (var sr = new StreamReader(inputIsosFileName))
            {

                using (var sw = new StreamWriter(outputIsosFilename))
                {
                    var header = sr.ReadLine();
                    sw.WriteLine(header);


                    var msFeatureIndex = 0;
                    while (!sr.EndOfStream)
                    {
                        var currentLine = sr.ReadLine();

                        var splitLine = currentLine.Split(',');

                        var parsedOK = double.TryParse(splitLine[colIndex], out var parsedVal);

                        var writeOutCurrentLine = true;

                        if (parsedOK)
                        {
                            if (Math.Abs(minVal + 1) < float.Epsilon)
                            {
                                // minVal is -1
                                writeOutCurrentLine = (parsedVal <= maxVal);
                            }
                            else if (Math.Abs(maxVal + 1) < float.Epsilon)
                            {
                                // maxVal is -1
                                writeOutCurrentLine = (parsedVal >= minVal);
                            }
                            else
                            {
                                writeOutCurrentLine = (parsedVal >= minVal && parsedVal <= maxVal);
                            }
                        }

                        if (writeOutCurrentLine)
                        {
                            sw.WriteLine(currentLine);

                        }

                        msFeatureIndex++;

                    }

                    sw.Close();
                    sr.Close();

                }








            }



        }




        public List<IsosResult> getUIMFResults(string uimfInputFile, int minFrame, int maxFrame)
        {
            var uimfisoUtil = new IsosResultUtilities();
            uimfisoUtil.LoadResults(uimfInputFile, Globals.MSFileType.PNNL_UIMF,minFrame,maxFrame);

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
            var returnedResults = new List<UIMFIsosResult>();

            foreach (var result in inputList)
            {
                var uimfResult = (UIMFIsosResult)result;
                if (uimfResult.ScanSet.PrimaryScanNumber >= minFrame && uimfResult.ScanSet.PrimaryScanNumber <= maxFrame)
                {
                    returnedResults.Add(uimfResult);
                }
            }
            return returnedResults;
        }

        public List<IsosResult> getIMFResults(string imfInputFolder, int minFrame, int maxFrame)
        {
            var returnedResults = new List<IsosResult>();

            var imfFolder = new DirectoryInfo(imfInputFolder);
            var files = imfFolder.GetFiles("*_isos.csv");

            foreach (var file in files)
            {
                var currentFrame = getFrameFromFilename(file.Name);
                if (currentFrame >= minFrame && currentFrame <= maxFrame)
                {
                    var isoutil = new IsosResultUtilities();

                    var isosFileName = file.FullName;
                    //string isosFileName = Path.Combine(Path.GetDirectoryName(file.FullName), Path.GetFileNameWithoutExtension(file.FullName) + "_isos.csv");
                    isoutil.LoadResults(isosFileName, Globals.MSFileType.PNNL_IMS);

                    var isosResultsForFrame = convertIMFResultsToUIMFResults(currentFrame, isoutil.Results);

                    returnedResults.AddRange(isosResultsForFrame);

                }


            }




            return returnedResults;
        }

        private int getFrameFromFilename(string imfFileName)
        {
            //35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.Accum_500.imf
            var match = Regex.Match(imfFileName, @".*Accum_(?<frameNum>\d+)");

            if (match.Success)
            {
                return Convert.ToInt32(match.Groups["frameNum"].Value);
            }

            throw new Exception("Couldn't find frame number in IMF filename");
        }


        public List<IsosResult> convertIMFResultsToUIMFResults(int currentFrame, List<IsosResult> list)
        {
            var returnedResults = new List<IsosResult>();

            foreach (var result in list)
            {
                var uimfResult = new UIMFIsosResult();
                uimfResult.ScanSet = new LCScanSetIMS(currentFrame);
                uimfResult.IsotopicProfile = result.IsotopicProfile;
                uimfResult.Run = result.Run;
                uimfResult.ScanSet = result.ScanSet;
                returnedResults.Add(uimfResult);
            }

            return returnedResults;
        }


        public List<IsosResult> getIsosResults(string isosInputFile, Globals.MSFileType filetype, int minScan, int maxScan)
        {
            var isoutil = new IsosResultUtilities();
            isoutil.LoadResults(isosInputFile, filetype);
            return isoutil.Results.Where(p => p.ScanSet.PrimaryScanNumber >= minScan && p.ScanSet.PrimaryScanNumber <= maxScan).ToList();

        }


        public List<IsosResult> getIsosResults(string isosInputFile, Globals.MSFileType filetype)
        {
            var isoutil = new IsosResultUtilities();
            isoutil.LoadResults(isosInputFile, filetype);
            return isoutil.Results;

        }

        public void replaceAbundanceWithMonoAbundance(List<IsosResult> isosResults)
        {
            foreach (var result in isosResults)
            {
                result.IntensityAggregate = result.IsotopicProfile.GetMonoAbundance();
            }
        }


        public static List<IsosResult>GetIsosResultsByScan(List<IsosResult>inputList, int scanNum)
        {
            var results = new List<IsosResult>();

            var query = from n in inputList select n.ScanSet.PrimaryScanNumber;

            var scanNums = query.ToArray();


            var indexOfScanNum = MathUtils.BinarySearch(scanNums, scanNum, 0, scanNums.Length - 1);
            if (indexOfScanNum == -1) return results;

            //the found index might point to a isos result line that is mid way through the scan list.  So need to find the starting index
            var currentIdx = indexOfScanNum;
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
            var intersectedResults = new List<IsosResult>();

            var tolerance = 0.005;

            for (var i = 0; i < list1.Count; i++)
            {
                for (var i2 = 0; i2 < list2.Count ; i2++)
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
            var sb = new StringBuilder();

            var allResults = new IsosResultStats(resultList);

            var cs1results = new IsosResultStats(getIsosResultsByChargeState(resultList, 1, enumLinqOperator.EqualTo));
            var cs2results = new IsosResultStats(getIsosResultsByChargeState(resultList, 2, enumLinqOperator.EqualTo));
            var cs3results = new IsosResultStats(getIsosResultsByChargeState(resultList, 3, enumLinqOperator.EqualTo));
            var cs4results = new IsosResultStats(getIsosResultsByChargeState(resultList, 4, enumLinqOperator.EqualTo));
            var greaterThanCS4results = new IsosResultStats(getIsosResultsByChargeState(resultList, 4, enumLinqOperator.greaterThan));

            var stats = new List<IsosResultStats> {
                allResults,
                cs1results,
                cs2results,
                cs3results,
                cs4results,
                greaterThanCS4results };


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

            foreach (var statItem in stats)
            {

                statItem.FitAverage = getAverageScore(statItem.Results);
                statItem.FitStdDev = getStdDevScore(statItem.Results);
                statItem.Count = getCount(statItem.Results);

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
            var results = new List<IsosResult>();

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




            var scoreArray = query.ToArray();


            //List<double> vals = new List<double>();
            //foreach (IsosResult result in inputList)
            //{
            //    vals.Add(result.IsotopicProfile.Score);

            //}


            return MathUtils.GetStDev(scoreArray);

        }


        public static void getSummaryStats1(List<IsosResult> inputList, StringBuilder sb)
        {
            if (sb == null)
                sb = new StringBuilder();

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


            sb.Append("intensity 0 - 200000 \t" + inputList.Count(p => p.IntensityAggregate < 200000));
            sb.Append("\n");
            sb.Append("intensity 200000 - 500000 \t" + inputList.Count(p => p.IntensityAggregate >= 200000 && p.IntensityAggregate < 5e5));
            sb.Append("\n");
            sb.Append("intensity 500000 - 1000000 \t" + inputList.Count(p => p.IntensityAggregate >= 500000 && p.IntensityAggregate < 1e6));
            sb.Append("\n");
            sb.Append("intensity 1000000 - 2000000 \t" + inputList.Count(p => p.IntensityAggregate >= 1e6 && p.IntensityAggregate < 2e6));
            sb.Append("\n");
            sb.Append("intensity 2000000 - 4000000 \t" + inputList.Count(p => p.IntensityAggregate >= 2e6 && p.IntensityAggregate < 4e6));
            sb.Append("\n");
            sb.Append("intensity 4000000 - 8000000 \t" + inputList.Count(p => p.IntensityAggregate >= 4e6 && p.IntensityAggregate < 8e6));
            sb.Append("\n");
            sb.Append("intensity > 8000000 \t" + inputList.Count(p => p.IntensityAggregate > 8e6));
            sb.Append("\n");

        }



        public static void DisplayResults(StringBuilder sb, List<IsosResult> list)
        {
            foreach (var result in list)
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
                sb.Append(result.IntensityAggregate);
                sb.Append("\t");
                sb.Append(result.IsotopicProfile.OriginalIntensity);
                sb.Append("\t");
                sb.Append(result.IsotopicProfile.IsSaturated);
                sb.Append("\n");


            }
        }
    }
}
