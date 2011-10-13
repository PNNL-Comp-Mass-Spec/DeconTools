﻿using System.Collections.Generic;
using System.Linq;

namespace DeconTools.Backend.Core
{
    public class TargetCollection
    {
        #region Constructors
        public TargetCollection()
        {
            TargetList = new List<TargetBase>();
            this.TargetIDList = new List<long>();
        }
        #endregion

        #region Properties
        private List<TargetBase> massTagList;
        public List<TargetBase> TargetList
        {
            get { return massTagList; }
            set { massTagList = value; }
        }

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

            for (int i = 0; i < this.TargetList.Count; i++)
            {
                var mtCurrent = this.TargetList[i];
                if (massTagsNonRedundant.Where(p => p.ID == mtCurrent.ID && p.ChargeState == mtCurrent.ChargeState).Count() == 0)
                {
                    massTagsNonRedundant.Add(mtCurrent);
                }
            }


            List<int> uniqueMTIDs = (from n in massTagsNonRedundant select n.ID).Distinct().ToList();


            foreach (int mtID in uniqueMTIDs)
            {
                List<TargetBase> topChargeStatesOfMassTag = massTagsNonRedundant.Where(p => p.ID == mtID).OrderByDescending(n => n.ObsCount).Take(3).ToList();
                filteredMassTagList.AddRange(topChargeStatesOfMassTag);

            }

            this.TargetList = filteredMassTagList;




        }

        public void FilterOutDuplicates()
        {
            if (this.TargetList == null || this.TargetList.Count == 0) return;

            var filteredList = new List<TargetBase>();

            foreach (var mt in this.TargetList)
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

            this.TargetList = filteredList;



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






    }
}