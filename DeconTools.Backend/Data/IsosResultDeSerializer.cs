using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DeconTools.Backend.Data
{
    
    /// <summary>
    /// Retrieves ResultCollection objects that have been serialized one after another. 
    /// </summary>
    public class IsosResultDeSerializer
    {
        private string binaryDataFilename;
        FileStream stream;
        long streamPosition;

        public IsosResultDeSerializer(string binaryDataFilename)
        {
            this.binaryDataFilename = binaryDataFilename;
            try
            {
                stream = new FileStream(binaryDataFilename, FileMode.Open, FileAccess.Read);

            }
            catch (Exception ex)
            {
                
                throw new System.IO.IOException("De-serializer could not find temporary binary file. Details: "+ex.Message);
            }
        }


        public ResultCollection GetNextSetOfResults()
        {
            ResultCollection resultcollection= null;
            BinaryFormatter formatter = new BinaryFormatter();

            if (streamPosition < stream.Length)
            {
                stream.Seek(streamPosition, SeekOrigin.Begin);
                resultcollection = (ResultCollection)formatter.Deserialize(stream);
                streamPosition = stream.Position;
            }

            return resultcollection;

        }


        internal void Close()
        {
            try
            {
                stream.Close();
            }
            catch (Exception ex)
            {
                
                throw new System.IO.IOException("Deserializer couldn't close the binary stream. Details: " + ex.Message);
            } 
        }
    }
}
