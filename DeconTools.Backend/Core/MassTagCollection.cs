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
        }
        #endregion

        #region Properties
        private List<MassTag> massTagList;
        public List<MassTag> MassTagList
        {
            get { return massTagList; }
            set { massTagList = value; }
        }

        public List<long> MassTagIDList;

        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

        public void Display()
        {
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

        internal void ApplyChargeStateFilter()
        {
            List<MassTag> filteredMassTagList = new List<MassTag>();

            HashSet<int> massTagIDs = new HashSet<int>();

            //first collect all massTagIDs   (there are more than one massTag having the same ID - because there are multiple charge states for each ID
            for (int i = 0; i < this.MassTagList.Count; i++)
            {
                massTagIDs.Add(this.MassTagList[i].ID);
            }


            foreach (var mtID in massTagIDs)
            {
                
                //get MTs with same ID (but different charge state) 
                List<MassTag> mt_withSameID = this.MassTagList.Where(p => p.ID == mtID).OrderByDescending(n => n.ObsCount).ToList();
                
                //get sum of all observed MS/MS for the MT
                int totObs = mt_withSameID.Sum(p => p.ObsCount);

                foreach (MassTag mt in mt_withSameID)
                {
                    //if the obsCount for a charge state is greater than 10% of the total, add it. 
                    if ((double)mt.ObsCount / (double)totObs > 0.1)
                    {
                        filteredMassTagList.Add(mt);
                    }

                    
                }

            }

            this.MassTagList = filteredMassTagList;




        }
    }
}
