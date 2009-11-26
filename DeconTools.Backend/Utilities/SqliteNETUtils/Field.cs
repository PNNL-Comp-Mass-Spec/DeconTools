using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Utilities.SqliteUtils
{
    public class Field
    {



        #region Constructors
        public Field(string name, string type)
        {
            this.Name = name;
            this.Type = type;
        }
        #endregion

        #region Properties
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string type;
        public string Type
        {
            get { return type.ToUpper(); }
            set { type = value; }
        }

        #endregion

        #region Public Methods
        public override string ToString()
        {
            return (this.Name + " " + this.Type);
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
