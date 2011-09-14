using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core
{
    public class MassTagCollection
    {
        #region Constructors
        public MassTagCollection()
        {
            MassTagList = new List<TargetBase>();
            this.MassTagIDList = new List<long>();
        }
        #endregion

        #region Properties
        private List<TargetBase> massTagList;
        public List<TargetBase> MassTagList
        {
            get { return massTagList; }
            set { massTagList = value; }
        }

        public List<long> MassTagIDList;

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

            for (int i = 0; i < this.MassTagList.Count; i++)
            {
                var mtCurrent = this.MassTagList[i];
                if (massTagsNonRedundant.Where(p => p.ID == mtCurrent.ID && p.ChargeState == mtCurrent.ChargeState).Count() == 0)
                {
                    massTagsNonRedundant.Add(mtCurrent);
                }
            }


            List<int> uniqueMTIDs = (from n in massTagsNonRedundant select n.ID).Distinct().ToList();


            foreach (int mtID in uniqueMTIDs)
            {
                List<TargetBase> mt_withSameID = massTagsNonRedundant.Where(p => p.ID == mtID).OrderByDescending(n => n.ObsCount).ToList();
                int totObs = mt_withSameID.Sum(p => p.ObsCount);

                foreach (var uniquelyChargedMT in mt_withSameID)
                {
                    
                    if (totObs < 20)    // if total obs is low, add all observed charge states
                    {
                        filteredMassTagList.Add(uniquelyChargedMT);
                    }

                    else
                    {
                        bool hasEnoughCounts = ((double)uniquelyChargedMT.ObsCount / (double)totObs > threshold);  //if the obsCount for a charge state is greater than threshold of the total, add it. 
                        if (hasEnoughCounts)
                        {
                            filteredMassTagList.Add(uniquelyChargedMT);
                        }
                    }


                }


            }



            this.MassTagList = filteredMassTagList;




        }

        public void FilterOutDuplicates()
        {
            if (this.MassTagList == null || this.MassTagList.Count == 0) return;

            var filteredList = new List<TargetBase>();

            foreach (var mt in this.MassTagList)
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

            this.MassTagList = filteredList;



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
