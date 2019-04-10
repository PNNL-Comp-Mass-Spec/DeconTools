using System;
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
        readonly FileStream stream;
        long streamPosition;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="binaryDataFilename"></param>
        public IsosResultDeSerializer(string binaryDataFilename)
        {
            try
            {
                stream = new FileStream(binaryDataFilename, FileMode.Open, FileAccess.Read);

            }
            catch (Exception ex)
            {

                throw new IOException("De-serializer could not find temporary binary file. Details: "+ex.Message);
            }
        }


        public ResultCollection GetNextSetOfResults()
        {
            ResultCollection results = null;
            var formatter = new BinaryFormatter();

            if (streamPosition < stream.Length)
            {
                stream.Seek(streamPosition, SeekOrigin.Begin);
                results = (ResultCollection)formatter.Deserialize(stream);
                streamPosition = stream.Position;
            }

            return results;

        }

        internal void Close()
        {
            try
            {
                stream.Close();
            }
            catch (Exception ex)
            {

                throw new IOException("Deserializer couldn't close the binary stream. Details: " + ex.Message);
            }
        }
    }
}
