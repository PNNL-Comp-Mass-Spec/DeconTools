using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace DeconTools.Backend.Utilities
{
    public class SerializerGeneric
    {
        private readonly FileMode fileMode;

        public SerializerGeneric(string outputFilename, FileMode mode, bool deletePrevious)
        {

            OutputFilename = outputFilename;
            fileMode = mode;

            try
            {
                if (deletePrevious)
                {
                    if (File.Exists(outputFilename))
                    {
                        File.Delete(outputFilename);
                    }
                }

            }
            catch (Exception ex)
            {
                throw new IOException("Could not create temporary binary file for storing results. Details: " + ex.Message);
            }
        }

        public Stream Stream { get; set; }

        public string OutputFilename { get; set; }

        public void Serialize(Object o)
        {
            using (Stream stream = File.Open(OutputFilename, fileMode))
            {
                var b = new BinaryFormatter();
                b.Serialize(stream, o);
            }
        }

        public void Close()
        {
            Stream.Close();
        }


    }
}
