using System;
using System.IO;
using DeconTools.Backend.Core;
using System.Runtime.Serialization.Formatters.Binary;

namespace DeconTools.Backend.Data
{
    public class IsosResultSerializer
    {
        public IsosResultSerializer(string outputFilename, FileMode mode, bool deletePrevious)
        {
            OutputFilename = outputFilename;

            try
            {
                if (deletePrevious)
                {
                    if (File.Exists(outputFilename))
                    {
                        File.Delete(outputFilename);
                    }
                }
                Stream = File.Open(outputFilename, mode);
            }
            catch (Exception ex)
            {
                throw new IOException("Could not create temporary binary file for storing results. Details: " + ex.Message);
            }
        }

        public Stream Stream { get; set; }

        public string OutputFilename { get; set; }

        public void Serialize(ResultCollection resultCollection)
        {
            var b = new BinaryFormatter();
            b.Serialize(Stream, resultCollection);
        }

        public void Close()
        {
            Stream.Close();
        }
    }
}
