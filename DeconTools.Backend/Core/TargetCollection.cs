using System.Collections.Generic;
using System.Linq;

namespace DeconTools.Backend.Core
{
    public class TargetCollection
    {
        #region Constructors
        public TargetCollection()
        {
            TargetList = new List<TargetBase>();
            TargetIDList = new List<long>();
        }
        #endregion

        #region Properties

        public List<TargetBase> TargetList { get; set; }

        public List<long> TargetIDList;

        #endregion

        #region Public Methods
        public void ApplyChargeStateFilter()
        {
            ApplyChargeStateFilter(0.1);



        }

        /// <summary>
        /// This filters the list of mass tags on the basis of their number of observations
        /// </summary>
        /// <param name="threshold"></param>
        public void ApplyChargeStateFilter(double threshold)
        {
            var filteredMassTagList = new List<TargetBase>();

            var massTagsNonRedundant = new List<TargetBase>();

            foreach (var mtCurrent in TargetList)
            {
                var current = mtCurrent;
                if (!massTagsNonRedundant.Any(p => p.ID == current.ID && p.ChargeState == current.ChargeState))
                {
                    massTagsNonRedundant.Add(mtCurrent);
                }
            }


            var uniqueMTIDs = (from n in massTagsNonRedundant select n.ID).Distinct().ToList();


            foreach (var mtID in uniqueMTIDs)
            {
                var topChargeStatesOfMassTag = massTagsNonRedundant.Where(p => p.ID == mtID).OrderByDescending(n => n.ObsCount).Take(3).ToList();
                filteredMassTagList.AddRange(topChargeStatesOfMassTag);

            }

            TargetList = filteredMassTagList;




        }

        public void FilterOutDuplicates()
        {
            if (TargetList == null || TargetList.Count == 0) return;

            var filteredList = new List<TargetBase>();

            foreach (var mt in TargetList)
            {
                if (massTagListContainsMassTag(filteredList, mt))
                {
                    //do nothing
                }
                else
                {
                    filteredList.Add(mt);
                }

            }

            TargetList = filteredList;



        }



        #endregion

        #region Private Methods
        private bool massTagListContainsMassTag(List<TargetBase> filteredList, TargetBase targetMassTag)
        {
            foreach (var mt in filteredList)
            {
                if (mt.ID == targetMassTag.ID && mt.ChargeState == targetMassTag.ChargeState)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion


        /// <summary>
        /// This updates LCMS targets with information from the source list. I.e. we use
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="sourceList"></param>
        public static void UpdateTargetsWithMassTagInfo(IEnumerable<TargetBase>targets, List<TargetBase>sourceList)
        {

            foreach (var targetBase in targets)
            {
                var lcmsFeatureTarget = (LcmsFeatureTarget)targetBase;
                var mt = sourceList.FirstOrDefault(p => p.ID == lcmsFeatureTarget.FeatureToMassTagID);

                if (mt!=null)
                {
                    lcmsFeatureTarget.Code = mt.Code;
                    lcmsFeatureTarget.EmpiricalFormula = mt.EmpiricalFormula;
                }

            }




        }





    }
}
