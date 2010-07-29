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
            MassTagList = new List<MassTag>();
            this.MassTagIDList = new List<long>();
        }
        #endregion

        #region Properties
        private List<MassTag> massTagList;
        public List<MassTag> MassTagList
        {
            get { return massTagList; }
            set { massTagList = value; }
        }

        public List<double> test = new List<double>();



        public List<long> MassTagIDList;

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        public void Display()
        {

            test.Add(400.3);
            test.Add(400.3);
            test.Add(400.3);
            test.Add(400.3);

            Console.Write(test.Count);

            char delim = '\t';

            StringBuilder sb = new StringBuilder();
            sb.Append("---------------------------------------- mass tags -------------------------------------\n");
            sb.Append("Mass_Tag_ID\tmonoMW\tNET\tPeptide\n");
            foreach (MassTag mt in MassTagList)
            {
                sb.Append(mt.ID);
                sb.Append(delim);
                sb.Append(mt.MonoIsotopicMass.ToString("#.####"));
                sb.Append(delim);
                sb.Append(mt.NETVal.ToString("0.00"));
                sb.Append(delim);
                sb.Append(mt.PeptideSequence);
                sb.Append(Environment.NewLine);
            }

            Console.WriteLine(sb.ToString());
        }





        public void ApplyChargeStateFilter()
        {
            ApplyChargeStateFilter(0.1);



        }

        public void ApplyChargeStateFilter(double threshold)
        {
            List<MassTag> filteredMassTagList = new List<MassTag>();

            List<MassTag> massTagsNonRedundant = new List<MassTag>();

            for (int i = 0; i < this.MassTagList.Count; i++)
            {
                MassTag mtCurrent = this.MassTagList[i];
                if (massTagsNonRedundant.Where(p => p.ID == mtCurrent.ID && p.ChargeState == mtCurrent.ChargeState).Count() == 0)
                {
                    massTagsNonRedundant.Add(mtCurrent);
                }
            }


            List<int> uniqueMTIDs = (from n in massTagsNonRedundant select n.ID).Distinct().ToList();


            foreach (int mtID in uniqueMTIDs)
            {
                List<MassTag> mt_withSameID = massTagsNonRedundant.Where(p => p.ID == mtID).OrderByDescending(n => n.ObsCount).ToList();
                int totObs = mt_withSameID.Sum(p => p.ObsCount);

                foreach (MassTag uniquelyChargedMT in mt_withSameID)
                {
                    //if the obsCount for a charge state is greater than 10% of the total, add it. 
                    if (totObs == 0)
                    {
                        filteredMassTagList.Add(uniquelyChargedMT);
                    }
                    else
                    {
                        if ((double)uniquelyChargedMT.ObsCount / (double)totObs > threshold)
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

            List<MassTag> filteredList = new List<MassTag>();

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

        private bool massTagListContainsMassTag(List<MassTag> filteredList, MassTag targetMassTag)
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
    }
}
