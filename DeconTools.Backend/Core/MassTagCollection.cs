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
    }
}
