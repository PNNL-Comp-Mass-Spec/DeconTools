using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data.SQLite;
using DeconTools.Utilities;

namespace DeconTools.Backend.Utilities.SqliteNETUtils
{
    public class SqliteNETUtils
    {
        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods

        public static List<string> GetColumnNames(DbConnection cnn, string tableName)
        {

            List<string>columnNames=new List<string>();

            Check.Assert(cnn is SQLiteConnection, "Method is for SQLite databases only.");
            
            SQLiteConnection myconnection = (SQLiteConnection)cnn;

            using (SQLiteTransaction mytransaction = myconnection.BeginTransaction())
            {
                using (SQLiteCommand mycommand = new SQLiteCommand(myconnection))
                {
                    mycommand.CommandText = "PRAGMA table_info("+tableName+");";

                    SQLiteDataReader reader;
                    try
                    {
                        reader = mycommand.ExecuteReader();

                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Couldn't read table information from SQLite database.\n\nDetails:"+ex.Message);

                    }
                    
                    
                    while (reader.Read())
                    {
                        columnNames.Add(reader.GetString(1));
                    }

                }
            }
            return columnNames;
        }



        #endregion

        #region Private Methods
        #endregion
    }
}
