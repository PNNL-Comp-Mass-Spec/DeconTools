using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace DeconTools.Backend.Utilities
{
    public class SerializerGeneric
    {
        private FileMode filemode;

        public SerializerGeneric(string outputFilename, FileMode mode, bool deletePrevious)
        {

            this.outputFilename = outputFilename;
            this.filemode = mode;

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
                throw new System.IO.IOException("Could not create temporary binary file for storing results. Details: " + ex.Message);
            }
        }

        private Stream stream;
        public Stream Stream
        {
            get { return stream; }
            set { stream = value; }
        }

        private string outputFilename;
        public string OutputFilename
        {
            get { return outputFilename; }
            set { outputFilename = value; }
        }

        public void Serialize(Object o)
        {
            using (Stream stream = File.Open(outputFilename, filemode))
            {
                var b = new BinaryFormatter();
                b.Serialize(stream, o);
            }
        }

        public void Close()
        {
            stream.Close();
        }


    }
}
