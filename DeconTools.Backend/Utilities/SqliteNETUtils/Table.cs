using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Utilities.SqliteUtils
{
    public abstract class Table
    {
        #region Constructors
        #endregion

        #region Properties
        public abstract string Name { get; set; }

        public abstract List<Field> FieldList { get; set; }
        #endregion

        #region Public Methods
        public virtual string BuildCreateTableString()
        {
            var sb = new StringBuilder();
            sb.Append("CREATE TABLE ");
            sb.Append(this.Name);
            sb.Append(" (");

            foreach (var fieldItem in FieldList)
            {
                sb.Append(fieldItem.ToString());
                if (fieldItem == FieldList[FieldList.Count - 1])  //if last one...
                {
                    sb.Append(");");
                }
                else   //not last
                {
                    sb.Append(", ");
                }
            }

            return sb.ToString();
        }

        public virtual string BuildInsertionHeader()
        {
            var sb = new StringBuilder();
            sb.Append("INSERT INTO ");
            sb.Append(this.Name);

            return sb.ToString();
        }

        #endregion

        #region Private Methods
        #endregion

    }
}
