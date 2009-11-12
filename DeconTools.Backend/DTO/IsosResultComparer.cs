using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Utilities;

namespace DeconTools.Backend.DTO
{
    public class IsosResultComparer : IEqualityComparer<IsosResult>
    {

        private double massTolerance;

        public IsosResultComparer()
        {

        }

        /// <summary>
        /// this is slow but sure!
        /// </summary>
        /// <param name="resultList1"></param>
        /// <param name="resultList2"></param>
        /// <returns></returns>
        public List<IsosResult> GetIntersectionTheManualWay(List<IsosResult> resultList1, List<IsosResult> resultList2)
        {
            Check.Require(resultList1 != null, "resultList1 is null");
            Check.Require(resultList2 != null, "resultList2 is null");
            Check.Require(resultList1.GetType() == resultList2.GetType(), "resultLists are not the same Type");

            List<IsosResult> intersectedResults = new List<IsosResult>();
            foreach (IsosResult result in resultList1)
            {
                double lowerMassLimit = result.IsotopicProfile.MonoIsotopicMass - result.IsotopicProfile.GetFWHM() * result.IsotopicProfile.ChargeState;
                double upperMassLimit = result.IsotopicProfile.MonoIsotopicMass + result.IsotopicProfile.GetFWHM() * result.IsotopicProfile.ChargeState;

                //IsosResult targetResult = findTargetResult(result, resultList2);
                //if (targetResult != null) intersectedResults.Add(targetResult);

                var query = from p in resultList2
                            where p.ScanSet.PrimaryScanNumber == result.ScanSet.PrimaryScanNumber
                            where p.IsotopicProfile.ChargeState == result.IsotopicProfile.ChargeState
                            where p.IsotopicProfile.MonoPeakMZ == result.IsotopicProfile.MonoPeakMZ
                            select p;

                List<IsosResult> resultsWithinMassTol = query.ToList();

                if (resultsWithinMassTol.Count > 1) Console.WriteLine("Scan = " + result.ScanSet.PrimaryScanNumber + "; m/z = " + result.IsotopicProfile.MonoPeakMZ);
                if (resultsWithinMassTol.Count > 0) intersectedResults.Add(resultsWithinMassTol[0]);



            }

            return intersectedResults;
        }




        /// <summary>
        /// This is faster, but the IComparable parts of this ('Equals' and 'GetHashCode') must be tested if there are any changes
        /// </summary>
        /// <param name="resultList1"></param>
        /// <param name="resultList2"></param>
        /// <returns></returns>
        public List<IsosResult> GetIntersectionBetweenTwoIsosResultSets(List<IsosResult> resultList1, List<IsosResult> resultList2)
        {
            Check.Require(resultList1 != null, "resultList1 is null");
            Check.Require(resultList2 != null, "resultList2 is null");
            Check.Require(resultList1.GetType() == resultList2.GetType(), "resultLists are not the same Type");
            List<IsosResult> intersectedResults = resultList1.Intersect(resultList2, this).ToList();
            return intersectedResults;
        }

        private IsosResult findTargetResult(IsosResult targetResult, List<IsosResult> resultList)
        {
            foreach (IsosResult item in resultList)
            {
                double massTol = targetResult.IsotopicProfile.GetFWHM() * targetResult.IsotopicProfile.ChargeState * 2;
                bool isWithinMassTol = (Math.Abs(item.IsotopicProfile.MonoIsotopicMass - targetResult.IsotopicProfile.MonoIsotopicMass) <= massTol);

                if (item.IsotopicProfile.ChargeState == targetResult.IsotopicProfile.ChargeState &&
                    item.ScanSet.PrimaryScanNumber == targetResult.ScanSet.PrimaryScanNumber &&
                    isWithinMassTol)
                {
                    return item;
                }

            }
            return null;
        }



        public List<IsosResult> GetUnionBetweenTwoIsosResultSets()
        {
            throw new NotImplementedException();
        }

        public List<IsosResult> GetUniqueBetweenTwoIsosResultSets(List<IsosResult> resultList1, List<IsosResult> resultList2)
        {
            Check.Require(resultList1 != null, "resultList1 is null");
            Check.Require(resultList2 != null, "resultList2 is null");
            Check.Require(resultList1.GetType() == resultList2.GetType(), "resultLists are not the same Type");
            List<IsosResult> uniqueResults = resultList1.Except(resultList2, this).ToList();
            return uniqueResults;

        }



        #region IEqualityComparer<IsosResult> Members



        public bool Equals(IsosResult result1, IsosResult result2)
        {
            //return true;


            return (result1.IsotopicProfile.MonoPeakMZ == result2.IsotopicProfile.MonoPeakMZ &&
                result1.ScanSet.PrimaryScanNumber == result2.ScanSet.PrimaryScanNumber &&
                result1.IsotopicProfile.ChargeState == result2.IsotopicProfile.ChargeState);

        }

        public int GetHashCode(IsosResult obj)
        {
            return obj.IsotopicProfile.MonoPeakMZ.GetHashCode() * obj.ScanSet.PrimaryScanNumber.GetHashCode() * obj.IsotopicProfile.ChargeState.GetHashCode();
        }

        #endregion
    }
}
